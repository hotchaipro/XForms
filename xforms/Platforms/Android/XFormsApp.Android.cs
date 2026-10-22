using System;
using XForms.Android;
using XForms.Controls;
using XForms.Toolkit;

namespace XForms
{
    public static partial class XFormsApp
    {
        /// <summary>
        /// Runs the app, with its theme, in the activity (the app's own, with
        /// any base class): call it from the activity's <c>OnCreate</c>, after
        /// the base class's. When Android re-creates the activity, the running
        /// app moves to the new one, and the new app object is left unused.
        /// </summary>
        public static void Run(
            global::Android.App.Activity activity,
            Application application,
            AppTheme theme)
        {
            Prepare(application, theme);
            AndroidToolkitServices.Register(() => AndroidApplication.Current?.Activity);
            AndroidApplication.Run(activity, application);
        }
    }
}
