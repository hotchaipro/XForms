using Android.App;
using Android.OS;
using XForma;
using XForma.Themes.Basic;

namespace XFormaApp
{
    [Activity(
        Label = "XFormaApp",
        ConfigurationChanges = global::Android.Content.PM.ConfigChanges.UiMode | global::Android.Content.PM.ConfigChanges.FontScale | global::Android.Content.PM.ConfigChanges.Orientation | global::Android.Content.PM.ConfigChanges.ScreenSize,
        MainLauncher = true)]
    public class MainActivity : Activity
    {
        protected override void OnCreate(
            Bundle? bundle)
        {
            base.OnCreate(bundle);
            XFormaApp.Run(this, bundle, new App(), new BasicTheme());
        }
    }
}
