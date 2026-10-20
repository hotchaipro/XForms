using System;
using Android.App;
using Android.OS;
using XForms.Android;

namespace XForms.Test
{
    [Activity(
        Label = "XForms Test",
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
                var platform = new AndroidPlatform(this);
                this._app = new TestApp(platform);
            }

            this._app.Start();
        }
    }
}

