using System;
using Microsoft.Win32;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Dwm;
using Windows.Win32.Graphics.Gdi;
using Windows.Win32.UI.Accessibility;
using Windows.Win32.UI.WindowsAndMessaging;

namespace XForms.Win32
{
    /// <summary>
    /// Windows' theme settings: dark mode, the accent color and high contrast.
    /// </summary>
    internal static unsafe class SystemTheme
    {
        private const string PersonalizeKey = @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";
        private const string DwmKey = @"Software\Microsoft\Windows\DWM";

        // DWMWA_USE_IMMERSIVE_DARK_MODE (Windows 10 20H1 and later)
        private const int UseImmersiveDarkMode = 20;

        private static float? _textScale;

        /// <summary>
        /// Windows' "make text bigger" setting, as a scale (1 to 2.25). Cached
        /// until the settings change.
        /// </summary>
        public static float TextScale
        {
            get
            {
                if (!_textScale.HasValue)
                {
                    object value = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Accessibility")?.GetValue("TextScaleFactor");
                    _textScale = (value is int percent) ? Math.Max(1, percent / 100.0f) : 1;
                }

                return _textScale.Value;
            }
        }

        /// <summary>
        /// Rereads the settings next time.
        /// </summary>
        public static void Invalidate()
        {
            _textScale = null;
        }

        public static bool IsHighContrast
        {
            get
            {
                var highContrast = new HIGHCONTRASTW() { cbSize = (uint)sizeof(HIGHCONTRASTW) };
                return PInvoke.SystemParametersInfo(SYSTEM_PARAMETERS_INFO_ACTION.SPI_GETHIGHCONTRAST, highContrast.cbSize, &highContrast, 0)
                    && highContrast.dwFlags.HasFlag(HIGHCONTRASTW_FLAGS.HCF_HIGHCONTRASTON);
            }
        }

        public static ThemeMode Mode
        {
            get
            {
                if (IsHighContrast)
                {
                    return ThemeMode.HighContrast;
                }

                // Apps are light unless the user chose dark ("AppsUseLightTheme" = 0)
                object value = Registry.CurrentUser.OpenSubKey(PersonalizeKey)?.GetValue("AppsUseLightTheme");
                return ((value is int useLight) && (0 == useLight)) ? ThemeMode.Dark : ThemeMode.Light;
            }
        }

        public static Color? AccentColor
        {
            get
            {
                // AccentColor is stored as 0xAABBGGRR
                object value = Registry.CurrentUser.OpenSubKey(DwmKey)?.GetValue("AccentColor");
                if (value is int abgr)
                {
                    uint bits = unchecked((uint)abgr);
                    return Color.FromArgb(0xFF, (byte)bits, (byte)(bits >> 8), (byte)(bits >> 16));
                }

                return null;
            }
        }

        /// <summary>
        /// The colors of the active contrast theme, or null without one.
        /// </summary>
        public static ColorScheme HighContrastColors
        {
            get
            {
                if (!IsHighContrast)
                {
                    return null;
                }

                var window = SysColor(SYS_COLOR_INDEX.COLOR_WINDOW);
                var text = SysColor(SYS_COLOR_INDEX.COLOR_WINDOWTEXT);
                var highlight = SysColor(SYS_COLOR_INDEX.COLOR_HIGHLIGHT);
                var highlightText = SysColor(SYS_COLOR_INDEX.COLOR_HIGHLIGHTTEXT);
                var buttonFace = SysColor(SYS_COLOR_INDEX.COLOR_BTNFACE);
                var buttonText = SysColor(SYS_COLOR_INDEX.COLOR_BTNTEXT);
                var grayText = SysColor(SYS_COLOR_INDEX.COLOR_GRAYTEXT);

                return new ColorScheme()
                {
                    Accent = highlight,
                    OnAccent = highlightText,
                    Background = window,
                    Foreground = text,
                    SubtleForeground = text,
                    Surface = buttonFace,
                    OnSurface = buttonText,
                    Outline = text,
                    Error = highlight,
                    OnError = highlightText,
                    Disabled = grayText,
                };
            }
        }

        /// <summary>
        /// Makes the window's title bar dark or light.
        /// </summary>
        public static void ApplyToWindow(
            HWND hwnd,
            ThemeMode mode)
        {
            BOOL isDark = (mode != ThemeMode.Light);
            PInvoke.DwmSetWindowAttribute(hwnd, (DWMWINDOWATTRIBUTE)UseImmersiveDarkMode, &isDark, (uint)sizeof(BOOL));
        }

        private static Color SysColor(
            SYS_COLOR_INDEX index)
        {
            // COLORREF is 0x00BBGGRR
            uint bits = PInvoke.GetSysColor(index);
            return Color.FromArgb(0xFF, (byte)bits, (byte)(bits >> 8), (byte)(bits >> 16));
        }
    }
}
