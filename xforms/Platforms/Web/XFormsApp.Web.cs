using System;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using XForms.Controls;
using XForms.Toolkit;
using XForms.Web;

namespace XForms
{
    public static partial class XFormsApp
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
