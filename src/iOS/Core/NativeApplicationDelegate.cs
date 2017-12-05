using System;
using UIKit;

namespace XForms.iOS
{
    public class NativeApplicationDelegate : UIApplicationDelegate
    {
        public event EventHandler Suspending;
        public event EventHandler Resuming;

        private UIWindow _window;

        protected NativeApplicationDelegate()
        {
        }

        internal void SetRootViewController(
            UIViewController viewController)
        {
            if (null == viewController)
            {
                throw new ArgumentNullException(nameof(viewController));
            }

            if (null != this._window)
            {
                throw new NotSupportedException();
            }

            this._window = new UIWindow(UIScreen.MainScreen.Bounds);

            this._window.RootViewController = viewController;
            this._window.MakeKeyAndVisible();
        }

        public override bool FinishedLaunching(
            UIApplication application,
            Foundation.NSDictionary launchOptions)
        {
            return true;
        }

        public override void OnResignActivation(
            UIApplication application)
        {
            // Invoked when the application is about to move from active to inactive state.
            // This can occur for certain types of temporary interruptions (such as an incoming phone call or SMS message) 
            // or when the user quits the application and it begins the transition to the background state.
            // Games should use this method to pause the game.
            this.Suspending?.Invoke(this, EventArgs.Empty);
        }

        public override void DidEnterBackground(
            UIApplication application)
        {
            // Use this method to release shared resources, save user data, invalidate timers and store the application state.
            // If your application supports background exection this method is called instead of WillTerminate when the user quits.
        }

        public override void WillEnterForeground(
            UIApplication application)
        {
            // Called as part of the transiton from background to active state.
            // Here you can undo many of the changes made on entering the background.
        }

        public override void OnActivated(
            UIApplication application)
        {
            // Restart any tasks that were paused (or not yet started) while the application was inactive. 
            // If the application was previously in the background, optionally refresh the user interface.
            this.Resuming?.Invoke(this, EventArgs.Empty);
        }

        public override void WillTerminate(
            UIApplication application)
        {
            // Called when the application is about to terminate. Save data, if needed. See also DidEnterBackground.
        }
    }
}
