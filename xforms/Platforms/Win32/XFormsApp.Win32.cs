using System;
using XForms.Controls;
using XForms.Toolkit;
using XForms.Win32;

namespace XForms
{
    public static partial class XFormsApp
    {
        /// <summary>
        /// Runs the app in a new window, with its theme, and returns when the
        /// app exits. Call it from <c>Main</c>, on an STA thread.
        /// </summary>
        public static void Run(
            string title,
            Application application,
            AppTheme theme)
        {
            Prepare(application, theme);
            Win32ToolkitServices.Register(() => Win32Application.Current?.MainWindowHandle ?? 0);
            Win32Application.Run(title, application);
        }
    }
}
