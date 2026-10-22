using System;
using System.Threading.Tasks;
using XForms.Web;

namespace XForms.Test.Web
{
    [System.Runtime.Versioning.SupportedOSPlatform("browser")]
    public static class Program
    {
        public static async Task Main()
        {
            await WebApplication.InitializeAsync();

            // ?page=lists picks another page, as XFORMS_TEST_PAGE does on Win32
            TestApp.StartPage = WebApplication.GetQueryParameter("page");
            TestApp.ThemeModeSwitches = WebApplication.GetQueryParameter("themes");

            await WebToolkitServices.InitializeAsync();
            await WebApplication.RunAsync("XForms Test (Web)", new TestApp());
        }
    }
}
