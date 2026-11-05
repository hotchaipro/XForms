using System;
using Android.App;
using Android.OS;
using XForma.Android;
using XForma.Themes.Basic;

namespace XForma.Test
{
    [Activity(
        Label = "XForma Test",
        // Dark mode and the text size change in place; XForma restyles the views.
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
            global::Android.Util.Log.Info("XForma.Test", $"MainActivity {this.GetHashCode():x} created; the app is {((null == AndroidApplication.Current) ? "new" : "running")}");

            if (null == AndroidApplication.Current)
            {
                // An intent extra picks another page: adb shell am start ... --es page benchmark-views
                TestApp.StartPage = this.Intent?.GetStringExtra("page");
                TestApp.ThemeModeSwitches = this.Intent?.GetStringExtra("themes");

                // For comparison: a native object for every view (--es lightweight off)
                if (this.Intent?.GetStringExtra("lightweight") == "off")
                {
                    AppContext.SetSwitch("XForma.DisableLightweightViews", true);
                }

            }

            TestApp.RecreateHost = () => AndroidApplication.Current?.Activity?.Recreate();
            XFormaApp.Run(this, bundle, new TestApp(), new BasicTheme());
        }

        // Does what a hot reload edit does (without the edit), for checking
        // what carries over a rebuild, where dotnet watch can't deliver edits:
        // adb shell am start -f 0x20000000 -n ... --ez simulate true
        protected override void OnNewIntent(
            global::Android.Content.Intent intent)
        {
            base.OnNewIntent(intent);
            if (true == intent?.GetBooleanExtra("simulate", false))
            {
                HotReloadLog.Simulate();
            }
        }
    }
}

