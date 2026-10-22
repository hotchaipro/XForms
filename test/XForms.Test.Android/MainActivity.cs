using System;
using Android.App;
using Android.OS;
using XForms.Android;
using XForms.Toolkit;

namespace XForms.Test
{
    [Activity(
        Label = "XForms Test",
        // Dark mode and the text size change in place; XForms restyles the views.
        // NOTE: Unlike an app's (see SoonerOrLater), rotating re-creates this
        // activity, to exercise moving the running app to the new one
        ConfigurationChanges = global::Android.Content.PM.ConfigChanges.UiMode | global::Android.Content.PM.ConfigChanges.FontScale,
        MainLauncher = true,
        Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        protected override void OnCreate(
            Bundle bundle)
        {
            base.OnCreate(bundle);
            global::Android.Util.Log.Info("XForms.Test", $"MainActivity {this.GetHashCode():x} created; the app is {((null == AndroidApplication.Current) ? "new" : "running")}");

            if (null == AndroidApplication.Current)
            {
                // An intent extra picks another page: adb shell am start ... --es page benchmark-views
                TestApp.StartPage = this.Intent?.GetStringExtra("page");
                TestApp.ThemeModeSwitches = this.Intent?.GetStringExtra("themes");

                // For comparison: a native object for every view (--es lightweight off)
                if (this.Intent?.GetStringExtra("lightweight") == "off")
                {
                    AppContext.SetSwitch("XForms.DisableLightweightViews", true);
                }

            }

            TestApp.RecreateHost = () => AndroidApplication.Current?.Activity?.Recreate();
            AndroidToolkitServices.Register(() => AndroidApplication.Current?.Activity);
            AndroidApplication.Run(this, new TestApp());
        }
    }
}

