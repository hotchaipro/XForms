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
    /// (<see cref="Run(UIWindowScene, UISceneConnectionOptions, XForms.Application)"/>).
    /// Gives native code the running app's scene and window.
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
        /// app's own scene delegate's <c>WillConnect</c>, with the connection
        /// options it was given.
        /// </summary>
        /// <remarks>
        /// <para>
        /// If the app is already running (its scene disconnected and another
        /// connected), the running app moves to the new scene, keeping its
        /// state, and the new app object is left unused.
        /// </para>
        /// <para>
        /// The app starts on the back stack saved for the scene's session (see
        /// <see cref="CreateStateRestorationActivity"/>), or on the page for a
        /// URL it was opened with (<c>https://example.com/item/42</c> opens
        /// <c>/item/42</c>). XForms' own scene delegate saves the stack and
        /// opens URLs while the app runs; the app's own scene delegate returns
        /// <see cref="CreateStateRestorationActivity"/> from its
        /// <c>GetStateRestorationActivity</c>, and passes URLs to
        /// <see cref="OpenUrl"/> from its <c>OpenUrlContexts</c>.
        /// </para>
        /// </remarks>
        public static IosApplication Run(
            UIWindowScene windowScene,
            UISceneConnectionOptions connectionOptions,
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
            ApplicationHost.For(application).Start(new NativePlatform(host), GetStartStack(windowScene, connectionOptions));
            return host;
        }

        // The activity type of the back stack saved for a scene's session
        private const string NavigationActivityType = "com.hotchaigames.xforms.navigation";

        private const string NavigationStackKey = "stack";

        // The back stack saved for the scene's session, or a launch URL's page
        private static string[] GetStartStack(
            UIWindowScene windowScene,
            UISceneConnectionOptions connectionOptions)
        {
            var activity = windowScene.Session?.StateRestorationActivity;
            if ((null != activity) && (activity.ActivityType == NavigationActivityType) && (activity.UserInfo?[NavigationStackKey] is NSArray saved) && (0 < saved.Count))
            {
                var stack = new string[saved.Count];
                for (nuint i = 0; i < saved.Count; i += 1)
                {
                    stack[i] = saved.GetItem<NSString>(i)?.ToString();
                }

                return stack;
            }

            foreach (var context in connectionOptions?.UrlContexts ?? new NSSet<UIOpenUrlContext>())
            {
                string path = GetPath(context.Url);
                if (null != path)
                {
                    return new[] { path };
                }
            }

            return null;
        }

        // A URL's path and query (/item/42?x=1), still percent-encoded
        private static string GetPath(
            NSUrl url)
        {
            if (null == url)
            {
                return null;
            }

            var components = new NSUrlComponents(url, false);
            string path = components.PercentEncodedPath;
            if (string.IsNullOrEmpty(path))
            {
                path = "/";
            }

            string query = components.PercentEncodedQuery;
            return string.IsNullOrEmpty(query) ? path : path + "?" + query;
        }

        /// <summary>
        /// The app's back stack, for the scene's session to save, so the app
        /// starts on the same pages when iOS ends it in the background: return
        /// it from the scene delegate's <c>GetStateRestorationActivity</c>.
        /// XForms' own scene delegate does.
        /// </summary>
        public NSUserActivity CreateStateRestorationActivity()
        {
            var stack = new NSMutableArray();
            foreach (var path in XForms.Application.Current?.NavigationStack ?? Array.Empty<string>())
            {
                if (null != path)
                {
                    stack.Add(new NSString(path));
                }
            }

            var activity = new NSUserActivity(NavigationActivityType);
            activity.AddUserInfoEntries(NSDictionary.FromObjectAndKey(stack, new NSString(NavigationStackKey)));
            return activity;
        }

        /// <summary>
        /// Opens the page for a URL's path while the app runs (a link): call
        /// it from the scene delegate's <c>OpenUrlContexts</c>. XForms' own
        /// scene delegate does.
        /// </summary>
        public void OpenUrl(
            NSUrl url)
        {
            string path = GetPath(url);
            var application = XForms.Application.Current;
            if ((null != path) && (null != application))
            {
                _ = ApplicationHost.For(application).OpenPathAsync(path);
            }
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
                    Run(windowScene, connectionOptions, _pendingApplication);
                }
            }

            public override NSUserActivity GetStateRestorationActivity(
                UIScene scene)
            {
                return Current?.CreateStateRestorationActivity();
            }

            public override void OpenUrlContexts(
                UIScene scene,
                NSSet<UIOpenUrlContext> urlContexts)
            {
                foreach (var context in urlContexts)
                {
                    Current?.OpenUrl(context.Url);
                }
            }
        }
    }
}
