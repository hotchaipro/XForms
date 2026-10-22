using System;
using System.Runtime.InteropServices.JavaScript;
using System.Threading.Tasks;
using System.Web;
using XForms.Themes.Simple;

namespace XForms.Test.Web
{
    [System.Runtime.Versioning.SupportedOSPlatform("browser")]
    public static class Program
    {
        public static async Task Main()
        {
            // ?page=lists picks another page, as XFORMS_TEST_PAGE does on Win32
            using var location = JSHost.GlobalThis.GetPropertyAsJSObject("location");
            var query = HttpUtility.ParseQueryString(location.GetPropertyAsString("search") ?? string.Empty);
            TestApp.StartPage = query["page"];
            TestApp.ThemeModeSwitches = query["themes"];

            await XFormsApp.RunAsync("XForms Test (Web)", new TestApp(), new SimpleTheme());
        }
    }
}
