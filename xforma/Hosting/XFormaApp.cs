using System;
using XForma.Controls;

namespace XForma
{
    /// <summary>
    /// Runs an XForma app, with its theme: on each platform, it starts the
    /// framework's host with the app, and registers XForma.Toolkit's services
    /// with what the host knows (its window, activity or view controller).
    /// </summary>
    public static partial class XFormaApp
    {
        // The theme package's palette and tokens go into every theme the app
        // builds
        private static void Prepare(
            Application application,
            AppTheme theme)
        {
            ArgumentNullException.ThrowIfNull(application);
            ArgumentNullException.ThrowIfNull(theme);

            application.ThemePalette = theme.CreateColors;
            application.ThemeExtender = theme.Apply;
        }
    }
}
