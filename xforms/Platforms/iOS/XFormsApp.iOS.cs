using System;
using UIKit;
using XForms.Controls;
using XForms.iOS;
using XForms.Toolkit;

namespace XForms
{
    public static partial class XFormsApp
    {
        /// <summary>
        /// Runs the app, with its theme, with XForms' own app and scene
        /// delegates, and returns when the app exits. Call it from <c>Main</c>.
        /// </summary>
        public static void Run(
            string[] args,
            Application application,
            AppTheme theme)
        {
            Prepare(application, theme);
            RegisterServices();
            IosApplication.Run(args, application);
        }

        /// <summary>
        /// Runs the app, with its theme, in the scene: call it from the app's
        /// own scene delegate's <c>WillConnect</c>, with the connection options
        /// it was given. The delegate also returns
        /// <c>IosApplication.Current.CreateStateRestorationActivity()</c> from
        /// its <c>GetStateRestorationActivity</c>, so the back stack is
        /// restored, and passes URLs to <c>IosApplication.Current.OpenUrl</c>.
        /// </summary>
        public static void Run(
            UIWindowScene windowScene,
            UISceneConnectionOptions connectionOptions,
            Application application,
            AppTheme theme)
        {
            Prepare(application, theme);
            RegisterServices();
            IosApplication.Run(windowScene, connectionOptions, application);
        }

        private static void RegisterServices()
        {
            IosToolkitServices.Register(() => IosApplication.Current?.RootViewController);
        }
    }
}
