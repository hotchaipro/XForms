using System;
using Foundation;
using UIKit;

namespace XForms.iOS
{
    /// <summary>
    /// Base application delegate for XForms apps. Uses the UIScene lifecycle, so
    /// the app's Info.plist must declare a UIApplicationSceneManifest; the scene
    /// configuration itself is supplied here.
    /// </summary>
    public class NativeApplicationDelegate : UIApplicationDelegate
    {
        public event EventHandler Suspending;
        public event EventHandler Resuming;

        private UIViewController _rootViewController;
        private UIWindow _window;

        protected NativeApplicationDelegate()
        {
        }

        /// <summary>
        /// The app's window, once its scene has connected.
        /// </summary>
        internal UIWindow Window
        {
            get
            {
                return this._window;
            }
        }

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

            // The app may start before or after its scene connects
            if (null != this._window)
            {
                this._window.RootViewController = viewController;
            }
        }

        internal void OnSceneConnected(
            UIWindowScene windowScene)
        {
            this._window = new UIWindow(windowScene);

            if (null != this._rootViewController)
            {
                this._window.RootViewController = this._rootViewController;
            }

            this._window.MakeKeyAndVisible();
        }

        internal void OnSceneDisconnected()
        {
            this._window = null;
        }

        internal void OnSceneWillResignActive()
        {
            // Invoked when the app is about to move from active to inactive state,
            // such as for an incoming call or when the user leaves the app
            this.Suspending?.Invoke(this, EventArgs.Empty);
        }

        internal void OnSceneDidBecomeActive()
        {
            this.Resuming?.Invoke(this, EventArgs.Empty);
        }

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
                DelegateType = typeof(NativeSceneDelegate),
            };
        }
    }

    [Register("XFormsSceneDelegate")]
    internal sealed class NativeSceneDelegate : UIWindowSceneDelegate
    {
        private static NativeApplicationDelegate ApplicationDelegate
        {
            get
            {
                return UIApplication.SharedApplication.Delegate as NativeApplicationDelegate;
            }
        }

        public override void WillConnect(
            UIScene scene,
            UISceneSession session,
            UISceneConnectionOptions connectionOptions)
        {
            if (scene is UIWindowScene windowScene)
            {
                ApplicationDelegate?.OnSceneConnected(windowScene);
            }
        }

        public override void DidDisconnect(
            UIScene scene)
        {
            ApplicationDelegate?.OnSceneDisconnected();
        }

        public override void WillResignActive(
            UIScene scene)
        {
            ApplicationDelegate?.OnSceneWillResignActive();
        }

        public override void DidBecomeActive(
            UIScene scene)
        {
            ApplicationDelegate?.OnSceneDidBecomeActive();
        }
    }
}
