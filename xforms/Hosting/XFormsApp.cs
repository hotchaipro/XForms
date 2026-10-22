using System;
using XForms.Controls;

namespace XForms
{
    /// <summary>
    /// Runs an XForms app, with its theme: on each platform, it starts the
    /// framework's host with the app, and registers XForms.Toolkit's services
    /// with what the host knows (its window, activity or view controller).
    /// </summary>
    public static partial class XFormsApp
    {
        // The theme package goes into every theme the app builds
        private static void Prepare(
            Application application,
            AppTheme theme)
        {
            ArgumentNullException.ThrowIfNull(application);
            ArgumentNullException.ThrowIfNull(theme);

            application.ThemeExtender = theme.Apply;
        }
    }
}
