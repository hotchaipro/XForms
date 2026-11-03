using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using XForms.Primitives;

namespace XForms.Platform
{
    internal interface IApplicationRenderer
    {
        void SetRootFrame(Frame frame);

        void OnNavigationComplete();

        void InvokeOnUIThreadAsync(Action action);

        /// <summary>
        /// Calls back once, on the UI thread, when the display is about to show
        /// its next frame, with the frame's time.
        /// </summary>
        void RequestFrame(Action<TimeSpan> onFrame);

        void Exit();

        /// <summary>
        /// Styles the platform's own parts (title bar, system bars) for the
        /// application's theme.
        /// </summary>
        void ApplyTheme(Theme theme);

        /// <summary>
        /// The system's theme mode: dark mode, or a high-contrast setting.
        /// </summary>
        ThemeMode SystemThemeMode { get; }

        /// <summary>
        /// The system's accent color, or null where there isn't one.
        /// </summary>
        Color? SystemAccentColor { get; }

        /// <summary>
        /// The system's high-contrast colors, or null to use XForms' own.
        /// </summary>
        ColorScheme SystemHighContrastColors { get; }

        /// <summary>
        /// The system's text size, as a scale (1 is the default). The platform
        /// applies it to every font size, as Android's scaled pixels do.
        /// </summary>
        float SystemTextScale { get; }

        string WindowTitle { set; }

        bool CanResizeWindow { get; }

        Size MinimumWindowSize { set; }

        void ResizeWindow(Size size);

        /// <summary>
        /// The color under the system's bar at the bottom of the screen, below
        /// a page that uses the safe area (its <see cref="Page.BottomSafeAreaColor"/>),
        /// or null for the page's background: the page draws it, and the
        /// platform makes the bar's buttons contrast with it.
        /// </summary>
        Color? BottomSafeAreaColor { set; }

        /// <summary>
        /// Takes keyboard focus off the text entry that has it, if one does,
        /// hiding the soft keyboard.
        /// </summary>
        void EndTextEntry();

        /// <summary>
        /// Whether the app can open windows besides its main one.
        /// </summary>
        bool CanOpenWindows { get; }

        /// <summary>
        /// Opens a window showing the frame. The platform reports its size and
        /// its closing to the window (<c>OnSizeChanged</c>, <c>OnClosed</c>).
        /// </summary>
        IWindowRenderer OpenWindow(Window window, Frame frame, string title, Size size);

        Color? TitleBarAccentColor { get; set; }

        Color? TitleBarForegroundColor { get; set; }

        Version GetVersion();
    }

    /// <summary>
    /// A window other than the main one (on the desktop).
    /// </summary>
    internal interface IWindowRenderer
    {
        string Title { set; }

        Size MinimumSize { set; }

        void Resize(Size size);

        /// <summary>
        /// Closes the window; the platform then calls the window's
        /// <c>OnClosed</c>.
        /// </summary>
        void Close();
    }
}
