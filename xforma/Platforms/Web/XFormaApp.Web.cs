using System;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using XForma.Controls;
using XForma.Toolkit;
using XForma.Web;

namespace XForma
{
    public static partial class XFormaApp
    {
        /// <summary>
        /// Runs the app in the page, with its theme. Call it once, from
        /// <c>Main</c>.
        /// </summary>
        [SupportedOSPlatform("browser")]
        public static async Task RunAsync(
            string title,
            Application application,
            AppTheme theme)
        {
            Prepare(application, theme);
            await WebToolkitServices.InitializeAsync();
            await WebApplication.RunAsync(title, application);
        }
    }
}
