using System;
using Foundation;
using UIKit;
using XForms.Platform;

namespace XForms.iOS
{
    /// <summary>
    /// Runs an XForms app in a window scene: XForms' own app and scene
    /// delegates (<see cref="Run(string[], XForms.Application)"/>), or the
    /// app's own, with whatever base classes they need
    /// (<see cref="Run(UIWindowScene, XForms.Application)"/>). Gives native
    /// code the running app's scene and window.
    /// </summary>
    /// <example>
    /// <code>
    /// static void Main(string[] args)
    /// {
    ///     IosApplication.Run(args, new App());
    /// }
    /// </code>
    /// </example>
    public sealed class IosApplication
    {
        // The app for XForms' own scene delegate to run, when it connects
        private static XForms.Application _pendingApplication;

        private readonly NSObject[] _observers = new NSObject[3];
        private UIViewController _rootViewController;
        private UIWindowScene _windowScene;
        private UIWindow _window;

        private IosApplication()
        {
        }

        /// <summary>
        /// The running app's host, or null before it runs.
        /// </summary>
        public static IosApplication Current
        {
            get;
            private set;
        }

        /// <summary>
        /// The scene the app runs in.
        /// </summary>
        public UIWindowScene WindowScene
        {
            get
            {
                return this._windowScene;
            }
        }

        /// <summary>
        /// The app's window, in its scene.
        /// </summary>
        public UIWindow Window
        {
            get
            {
                return this._window;
            }
        }

        /// <summary>
        /// The window's root view controller, such as to present a native
        /// view controller.
        /// </summary>
        public UIViewController RootViewController
        {
            get
            {
                return this._rootViewController;
            }
        }

        /// <summary>
        /// Runs the app with XForms' own app and scene delegates, and returns
        /// when the app exits. Call it from <c>Main</c>. The app's Info.plist
        /// declares a <c>UIApplicationSceneManifest</c>; XForms supplies the
        /// scene configuration.
        /// </summary>
        public static void Run(
            string[] args,
            XForms.Application application)
        {
            ArgumentNullException.ThrowIfNull(application);

            _pendingApplication = application;
            UIApplication.Main(args, null, typeof(XFormsAppDelegate));
        }

        /// <summary>
        /// Runs the app in the scene, in a window of its own: call it from the
        /// app's own scene delegate's <c>WillConnect</c>.
        /// </summary>
        /// <remarks>
        /// If the app is already running (its scene disconnected and another
        /// connected), the running app moves to the new scene, keeping its
        /// state, and the new app object is left unused.
        /// </remarks>
        public static IosApplication Run(
            UIWindowScene windowScene,
            XForms.Application application)
        {
            ArgumentNullException.ThrowIfNull(windowScene);
            ArgumentNullException.ThrowIfNull(application);

            var current = Current;
            if (null != current)
            {
                current.AttachTo(windowScene);
                return current;
            }

            var host = new IosApplication();
            Current = host;
            host.AttachTo(windowScene);
            ApplicationHost.For(application).Start(new NativePlatform(host));
            return host;
        }

        // Shows the app in a window in the scene, and follows its activation
        private void AttachTo(
            UIWindowScene windowScene)
        {
            if (windowScene == this._windowScene)
            {
                return;
            }

            this.StopObserving();
            this._windowScene = windowScene;
            this._window = new UIWindow(windowScene);

            if (null != this._rootViewController)
            {
                this._window.RootViewController = this._rootViewController;
            }

            this._window.MakeKeyAndVisible();

            var center = NSNotificationCenter.DefaultCenter;
            this._observers[0] = center.AddObserver(UIScene.WillDeactivateNotification, (notification) => this.Suspending?.Invoke(this, EventArgs.Empty), windowScene);
            this._observers[1] = center.AddObserver(UIScene.DidActivateNotification, (notification) => this.Resuming?.Invoke(this, EventArgs.Empty), windowScene);
            this._observers[2] = center.AddObserver(UIScene.DidDisconnectNotification, (notification) => this.OnDisconnected(), windowScene);
        }

        private void OnDisconnected()
        {
            this.StopObserving();
            this._windowScene = null;
            this._window = null;
        }

        private void StopObserving()
        {
            for (int i = 0; i < this._observers.Length; i += 1)
            {
                if (null != this._observers[i])
                {
                    NSNotificationCenter.DefaultCenter.RemoveObserver(this._observers[i]);
                    this._observers[i] = null;
                }
            }
        }

        // ── What the platform uses ─────────────────────────────────────────

        // The scene became inactive (such as for an incoming call, or when the
        // user leaves the app), or active again
        internal event EventHandler Suspending;

        internal event EventHandler Resuming;

        internal void SetRootViewController(
            UIViewController viewController)
        {
            if (null == viewController)
            {
                throw new ArgumentNullException(nameof(viewController));
            }

            if (null != this._rootViewController)
            {
                throw new NotSupportedException();
            }

            this._rootViewController = viewController;
            if (null != this._window)
            {
                this._window.RootViewController = viewController;
            }
        }

        // XForms' own app delegate, for Run(args, app): it supplies the scene
        // configuration, with XForms' scene delegate
        [Register("XFormsAppDelegate")]
        private sealed class XFormsAppDelegate : UIApplicationDelegate
        {
            public override bool FinishedLaunching(
                UIApplication application,
                NSDictionary launchOptions)
            {
                return true;
            }

            public override UISceneConfiguration GetConfiguration(
                UIApplication application,
                UISceneSession connectingSceneSession,
                UISceneConnectionOptions options)
            {
                return new UISceneConfiguration("Default", connectingSceneSession.Role)
                {
                    DelegateType = typeof(XFormsSceneDelegate),
                };
            }
        }

        [Register("XFormsSceneDelegate")]
        private sealed class XFormsSceneDelegate : UIWindowSceneDelegate
        {
            public override void WillConnect(
                UIScene scene,
                UISceneSession session,
                UISceneConnectionOptions connectionOptions)
            {
                if ((scene is UIWindowScene windowScene) && (null != _pendingApplication))
                {
                    Run(windowScene, _pendingApplication);
                }
            }
        }
    }
}
