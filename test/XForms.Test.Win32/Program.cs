using System;
using XForms.Win32;

namespace XForms.Test.Win32
{
    public static class Program
    {
        [STAThread]
        public static void Main()
        {
            // XFORMS_LOG_EXCEPTIONS=1 logs every exception, even caught ones, to exceptions.log
            if (Environment.GetEnvironmentVariable("XFORMS_LOG_EXCEPTIONS") == "1")
            {
                string path = System.IO.Path.Combine(AppContext.BaseDirectory, "exceptions.log");
                AppDomain.CurrentDomain.FirstChanceException += (sender, e) =>
                    System.IO.File.AppendAllText(path, $"{DateTime.Now:O} {e.Exception.GetType().Name}: {e.Exception.Message}{Environment.NewLine}{Environment.StackTrace}{Environment.NewLine}");
            }

            // XFORMS_TEST_PAGE picks another page, such as "pickers"
            TestApp.StartPage = Environment.GetEnvironmentVariable("XFORMS_TEST_PAGE");
            TestApp.ThemeModeSwitches = Environment.GetEnvironmentVariable("XFORMS_THEME_SWITCHES");

            Win32Application.Run("XForms Test (Win32)", (platform) => new TestApp(platform));
        }
    }
}
