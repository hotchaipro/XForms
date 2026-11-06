using System;
using System.Runtime.InteropServices.JavaScript;
using System.Threading.Tasks;
using System.Web;
using XForma.Themes.Basic;

namespace XForma.Test.Web
{
    [System.Runtime.Versioning.SupportedOSPlatform("browser")]
    public static partial class Program
    {
        // Does what a hot reload edit does (without the edit), for checking
        // what carries over a rebuild, from script: (await
        // getDotnetRuntime(0).getAssemblyExports('XForma.Test.Web.dll'))
        // .XForma.Test.Web.Program.SimulateHotReload()
        [JSExport]
        internal static void SimulateHotReload()
        {
            HotReloadLog.Simulate();
        }

        [JSImport("globalThis.document.createElement")]
        private static partial JSObject CreateElement(string tag);

        public static async Task Main()
        {
            // ?page=lists picks another page, as XFORMA_TEST_PAGE does on Win32
            using var location = JSHost.GlobalThis.GetPropertyAsJSObject("location");
            var query = HttpUtility.ParseQueryString(location.GetPropertyAsString("search") ?? string.Empty);
            TestApp.StartPage = query["page"];
            TestApp.ThemeModeSwitches = query["themes"];

            WebViewPage.CreateNativeSample = () =>
            {
                var element = CreateElement("div");
                element.SetProperty("textContent", "A native element");
                return element;
            };

            await XFormaApp.RunAsync("XForma Test (Web)", new TestApp(), new BasicTheme());
        }
    }
}
