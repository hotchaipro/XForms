using System;
using Foundation;
using UIKit;
using XForma.Platform;

namespace XForma.iOS
{
    /// <summary>
    /// Runs an XForma app in a window scene: XForma's own app and scene
    /// delegates (<see cref="Run(string[], XForma.Application)"/>), or the
    /// app's own, with whatever base classes they need
    /// (<see cref="Run(UIWindowScene, UISceneConnectionOptions, XForma.Application)"/>).
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
        // The app for XForma's own scene delegate to run, when it connects
        private static XForma.Application _pendingApplication;

        private readonly NSObject[] _observers = new NSObject[3];
        private IUITraitChangeRegistration _styleObserver;
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
        /// Runs the app with XForma's own app and scene delegates, and returns
        /// when the app exits. Call it from <c>Main</c>. The app's Info.plist
        /// declares a <c>UIApplicationSceneManifest</c>; XForma supplies the
        /// scene configuration.
        /// </summary>
        public static void Run(
            string[] args,
            XForma.Application application)
        {
            ArgumentNullException.ThrowIfNull(application);

            _pendingApplication = application;
            UIApplication.Main(args, null, typeof(XFormaAppDelegate));
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
        /// <c>/item/42</c>). XForma's own scene delegate saves the stack and
        /// opens URLs while the app runs; the app's own scene delegate returns
        /// <see cref="CreateStateRestorationActivity"/> from its
        /// <c>GetStateRestorationActivity</c>, and passes URLs to
        /// <see cref="OpenUrl"/> from its <c>OpenUrlContexts</c>.
        /// </para>
        /// </remarks>
        public static IosApplication Run(
            UIWindowScene windowScene,
            UISceneConnectionOptions connectionOptions,
            XForma.Application application)
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
            AppBadge.Watch(application);
            ApplicationHost.For(application).Start(new NativePlatform(host), GetStartStack(windowScene, connectionOptions));
            return host;
        }

        // The activity type of the back stack saved for a scene's session
        // Keeps the name from before XForma: data users already have is stored under it
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
        /// XForma's own scene delegate does.
        /// </summary>
        public NSUserActivity CreateStateRestorationActivity()
        {
            var stack = new NSMutableArray();
            foreach (var path in XForma.Application.Current?.NavigationStack ?? Array.Empty<string>())
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
        /// it from the scene delegate's <c>OpenUrlContexts</c>. XForma's own
        /// scene delegate does.
        /// </summary>
        public void OpenUrl(
            NSUrl url)
        {
            string path = GetPath(url);
            var application = XForma.Application.Current;
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

            // Dark mode, as it changes while the app shows (before iOS 17, it's
            // reread as the scene becomes active). The window's own mode isn't
            // the system's, so the scene is watched.
            if (OperatingSystem.IsIOSVersionAtLeast(17))
            {
                this._styleObserver = windowScene.RegisterForTraitChanges(
                    new[] { typeof(UITraitUserInterfaceStyle) },
                    (IUITraitEnvironment environment, UITraitCollection previous) => this.OnSystemStyleChanged());
            }
        }

        // In the background, iOS changes the mode for its snapshots of the app:
        // the mode is reread as the scene becomes active
        private void OnSystemStyleChanged()
        {
            if (UISceneActivationState.Background != this._windowScene?.ActivationState)
            {
                this.SystemStyleChanged?.Invoke(this, EventArgs.Empty);
            }
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

            if (null != this._styleObserver)
            {
                if (OperatingSystem.IsIOSVersionAtLeast(17))
                {
                    this._windowScene?.UnregisterForTraitChanges(this._styleObserver);
                }

                this._styleObserver = null;
            }
        }

        // ── What the platform uses ─────────────────────────────────────────

        // The scene became inactive (such as for an incoming call, or when the
        // user leaves the app), or active again
        internal event EventHandler Suspending;

        internal event EventHandler Resuming;

        // The system's dark mode changed while the app shows
        internal event EventHandler SystemStyleChanged;

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

        /// <summary>
        /// The app <see cref="Run(string[], XForma.Application)"/> runs, before
        /// a scene starts it (as in a background refresh).
        /// </summary>
        internal static XForma.Application PendingApplication
        {
            get
            {
                return _pendingApplication;
            }
        }

        /// <summary>
        /// Lets the system wake the app in the background (see
        /// <see cref="XForma.Application.BackgroundRefreshInterval"/>). XForma's
        /// own app delegate calls it; an app's own calls it from
        /// <c>FinishedLaunching</c>, since iOS takes the registration only
        /// before the app finishes launching.
        /// </summary>
        public static void RegisterBackgroundRefresh()
        {
            BackgroundRefresh.Register();
        }

        // XForma's own app delegate, for Run(args, app): it supplies the scene
        // configuration, with XForma's scene delegate
        [Register("XFormaAppDelegate")]
        private sealed class XFormaAppDelegate : UIApplicationDelegate
        {
            public override bool FinishedLaunching(
                UIApplication application,
                NSDictionary launchOptions)
            {
                RegisterBackgroundRefresh();

                // The icon shows the app's count once the app has launched
                if (null != _pendingApplication)
                {
                    AppBadge.Watch(_pendingApplication);
                }

                return true;
            }

            public override UISceneConfiguration GetConfiguration(
                UIApplication application,
                UISceneSession connectingSceneSession,
                UISceneConnectionOptions options)
            {
                return new UISceneConfiguration("Default", connectingSceneSession.Role)
                {
                    DelegateType = typeof(XFormaSceneDelegate),
                };
            }
        }

        [Register("XFormaSceneDelegate")]
        private sealed class XFormaSceneDelegate : UIWindowSceneDelegate
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
