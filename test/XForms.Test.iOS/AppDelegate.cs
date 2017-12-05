using Foundation;
using UIKit;
using XForms.iOS;

namespace XForms.Test.iOS
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the
    // User Interface of the application, as well as listening (and optionally responding) to application events from iOS.
    [Register("AppDelegate")]
    public class AppDelegate : NativeApplicationDelegate
    {
        private TestApp _app;

        public override bool FinishedLaunching(
            UIApplication application,
            NSDictionary launchOptions)
        {
            // Override point for customization after application launch.
            // If not required for your application you can safely delete this method

            if (null == this._app)
            {
                var platform = new NativePlatform(this);
                this._app = new TestApp(platform);
            }

            this._app.Start();

            return base.FinishedLaunching(application, launchOptions);
        }
    }
}
