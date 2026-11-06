using XForma.Themes.Basic;

namespace XForma.Test.iOS
{
    public static class Program
    {
        // The app's entry point
        static void Main(
            string[] args)
        {
            // XFORMA_TEST_PAGE picks another page, such as "pickers", and
            // XFORMA_THEME_SWITCHES switches theme modes, as on Win32 (the
            // simulator passes them as SIMCTL_CHILD_XFORMA_TEST_PAGE and so on)
            TestApp.StartPage = System.Environment.GetEnvironmentVariable("XFORMA_TEST_PAGE");
            TestApp.ThemeModeSwitches = System.Environment.GetEnvironmentVariable("XFORMA_THEME_SWITCHES");

            WebViewPage.CreateNativeSample = () => new UIKit.UILabel() { Text = "A native UIKit view" };
            XFormaApp.Run(args, new TestApp(), new BasicTheme());
        }
    }
}
