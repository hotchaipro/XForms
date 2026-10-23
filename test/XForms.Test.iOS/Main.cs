using XForms.Themes.Basic;

namespace XForms.Test.iOS
{
    public static class Program
    {
        // The app's entry point
        static void Main(
            string[] args)
        {
            // XFORMS_TEST_PAGE picks another page, such as "pickers", and
            // XFORMS_THEME_SWITCHES switches theme modes, as on Win32 (the
            // simulator passes them as SIMCTL_CHILD_XFORMS_TEST_PAGE and so on)
            TestApp.StartPage = System.Environment.GetEnvironmentVariable("XFORMS_TEST_PAGE");
            TestApp.ThemeModeSwitches = System.Environment.GetEnvironmentVariable("XFORMS_THEME_SWITCHES");

            XFormsApp.Run(args, new TestApp(), new BasicTheme());
        }
    }
}
