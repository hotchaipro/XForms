using System;
using Android.App;
using Android.OS;
using XForms.Android;

namespace XForms.Test
{
    [Activity(
        Label = "XForms Test",
        // Dark mode and the text size change in place; XForms restyles the views
        ConfigurationChanges = global::Android.Content.PM.ConfigChanges.UiMode | global::Android.Content.PM.ConfigChanges.FontScale,
        MainLauncher = true,
        Icon = "@drawable/icon")]
    public class MainActivity : AndroidApplicationActivity
    {
        private TestApp _app;

        protected override void OnCreate(
            Bundle bundle)
        {
            base.OnCreate(bundle);

            if (null == this._app)
            {
                // An intent extra picks another page: adb shell am start ... --es page benchmark-views
                TestApp.StartPage = this.Intent?.GetStringExtra("page");
                TestApp.ThemeModeSwitches = this.Intent?.GetStringExtra("themes");

                // For comparison: a native object for every view (--es lightweight off)
                if (this.Intent?.GetStringExtra("lightweight") == "off")
                {
                    AppContext.SetSwitch("XForms.DisableLightweightViews", true);
                }

                var platform = new AndroidPlatform(this);
                this._app = new TestApp(platform);
            }

            this._app.Start();
        }
    }
}

