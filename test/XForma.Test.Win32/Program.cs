using System;
using XForma;
using XForma.Themes.Basic;

namespace XForma.Test.Win32
{
    public static class Program
    {
        [STAThread]
        public static void Main()
        {
            // XFORMA_LOG_EXCEPTIONS=1 logs every exception, even caught ones, to exceptions.log
            if (Environment.GetEnvironmentVariable("XFORMA_LOG_EXCEPTIONS") == "1")
            {
                string path = System.IO.Path.Combine(AppContext.BaseDirectory, "exceptions.log");
                AppDomain.CurrentDomain.FirstChanceException += (sender, e) =>
                    System.IO.File.AppendAllText(path, $"{DateTime.Now:O} {e.Exception.GetType().Name}: {e.Exception.Message}{Environment.NewLine}{Environment.StackTrace}{Environment.NewLine}");
            }

            // XFORMA_TEST_PAGE picks another page, such as "pickers"
            TestApp.StartPage = Environment.GetEnvironmentVariable("XFORMA_TEST_PAGE");
            TestApp.ThemeModeSwitches = Environment.GetEnvironmentVariable("XFORMA_THEME_SWITCHES");

            // For comparison: a native object for every view
            if (Environment.GetEnvironmentVariable("XFORMA_LIGHTWEIGHT") == "0")
            {
                AppContext.SetSwitch("XForma.DisableLightweightViews", true);
            }

            XFormaApp.Run("XForma Test (Win32)", new TestApp(), new BasicTheme());
        }
    }
}
