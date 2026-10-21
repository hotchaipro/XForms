using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace XForms
{
    /// <summary>
    /// Whether the UI is light, dark or high contrast. The system reports its
    /// mode; an app can follow it or choose one.
    /// </summary>
    /// <remarks>
    /// MAUI's <c>AppTheme</c>, Avalonia's <c>ThemeVariant</c> and WinUI's
    /// <c>ElementTheme</c> are the same idea.
    /// </remarks>
    public enum ThemeMode
    {
        Light,
        Dark,
        HighContrast,
    }

    /// <summary>
    /// A theme's colors, by role. Most come in pairs: a color, and the color
    /// of text and icons drawn on it.
    /// </summary>
    /// <remarks>
    /// The roles follow Material 3's color roles, and map to WinUI's theme
    /// resources (accent fill, text fill, card background, control stroke).
    /// </remarks>
    public sealed record ColorScheme
    {
        /// <summary>The app's accent: selection, primary actions, focus.</summary>
        public Color Accent { get; init; }

        /// <summary>Text and icons on the accent color.</summary>
        public Color OnAccent { get; init; }

        /// <summary>A page's background.</summary>
        public Color Background { get; init; }

        /// <summary>Text and icons on the background.</summary>
        public Color Foreground { get; init; }

        /// <summary>Secondary text and icons on the background.</summary>
        public Color SubtleForeground { get; init; }

        /// <summary>
        /// Raised or set-apart areas: text boxes, fields, bars, popups.
        /// </summary>
        public Color Surface { get; init; }

        /// <summary>Text and icons on a surface.</summary>
        public Color OnSurface { get; init; }

        /// <summary>Borders and dividers.</summary>
        public Color Outline { get; init; }

        /// <summary>Errors and destructive actions.</summary>
        public Color Error { get; init; }

        /// <summary>Text and icons on the error color.</summary>
        public Color OnError { get; init; }

        /// <summary>Text and icons of disabled controls.</summary>
        public Color Disabled { get; init; }
    }

    /// <summary>
    /// A theme: the mode and colors for it, and whatever else a control
    /// library keeps in it (its type scale, shapes, motion, control styles)
    /// as extensions. Set one on the application, or on any view for it and
    /// its descendants.
    /// </summary>
    /// <remarks>
    /// The same idea as Flutter's <c>ThemeData</c> and Compose's
    /// <c>MaterialTheme</c>. A theme is immutable: make a variant with a
    /// <c>with</c> expression, for example
    /// <c>theme with { Colors = theme.Colors with { Accent = brand } }</c>.
    /// </remarks>
    public sealed record Theme
    {
        /// <summary>
        /// The accent color when neither the app nor the system gives one.
        /// </summary>
        public static readonly Color DefaultAccentColor = Color.FromRgb(0x2196f3);

        public ThemeMode Mode { get; init; }

        public ColorScheme Colors { get; init; }

        /// <summary>
        /// What control libraries keep in the theme, by type: a type scale,
        /// shapes, motion, default styles for controls (see
        /// <see cref="GetExtension{TExtension}"/>).
        /// </summary>
        /// <remarks>
        /// Flutter's <c>ThemeExtension</c> is the same idea: the framework
        /// knows only colors; each control library adds its own tokens.
        /// </remarks>
        public ImmutableDictionary<Type, object> Extensions { get; init; } = ImmutableDictionary<Type, object>.Empty;

        /// <summary>
        /// The theme's extension of a type (such as XForms.Controls'
        /// <c>Typography</c> or <c>ButtonStyle</c>), or null.
        /// </summary>
        public TExtension GetExtension<TExtension>()
            where TExtension : class
        {
            return this.Extensions.TryGetValue(typeof(TExtension), out var extension) ? (TExtension)extension : null;
        }

        /// <summary>
        /// The theme with an extension of a type (replacing any it had), or
        /// without one, for null.
        /// </summary>
        public Theme WithExtension<TExtension>(
            TExtension extension)
            where TExtension : class
        {
            return this with { Extensions = (null == extension) ? this.Extensions.Remove(typeof(TExtension)) : this.Extensions.SetItem(typeof(TExtension), extension) };
        }

        /// <summary>
        /// XForms' theme for a mode, around an accent color.
        /// </summary>
        public static Theme Create(
            ThemeMode mode,
            Color? accentColor = null)
        {
            return new Theme()
            {
                Mode = mode,
                Colors = CreateColors(mode, accentColor ?? DefaultAccentColor),
            };
        }

        /// <summary>
        /// XForms' colors for a mode, around an accent color. In dark mode the
        /// accent is lightened, so it stands out against the dark background.
        /// </summary>
        public static ColorScheme CreateColors(
            ThemeMode mode,
            Color accentColor)
        {
            switch (mode)
            {
                case ThemeMode.Dark:
                {
                    var accent = Mix(accentColor, XForms.Colors.White, 0.3f);
                    return new ColorScheme()
                    {
                        Accent = accent,
                        OnAccent = ContrastingColor(accent),
                        Background = XForms.Colors.Black,
                        Foreground = Color.FromRgb(0xcc, 0xcc, 0xcc),
                        SubtleForeground = Color.FromRgb(0x99, 0x99, 0x99),
                        Surface = Color.FromRgb(0x1c, 0x1c, 0x1c),
                        OnSurface = Color.FromRgb(0xdd, 0xdd, 0xdd),
                        Outline = Color.FromRgb(0x44, 0x44, 0x44),
                        Error = Color.FromRgb(0xff, 0x8a, 0x80),
                        OnError = XForms.Colors.Black,
                        Disabled = Color.FromRgb(0x5c, 0x5c, 0x5c),
                    };
                }

                case ThemeMode.HighContrast:
                    // White on black, as Windows' "Night sky" contrast theme;
                    // platforms replace these with the system's own colors
                    return new ColorScheme()
                    {
                        Accent = Color.FromRgb(0xff, 0xff, 0x00),
                        OnAccent = XForms.Colors.Black,
                        Background = XForms.Colors.Black,
                        Foreground = XForms.Colors.White,
                        SubtleForeground = XForms.Colors.White,
                        Surface = XForms.Colors.Black,
                        OnSurface = XForms.Colors.White,
                        Outline = XForms.Colors.White,
                        Error = Color.FromRgb(0xff, 0x80, 0x80),
                        OnError = XForms.Colors.Black,
                        Disabled = Color.FromRgb(0x3f, 0xf2, 0x3f),
                    };

                default:
                    return new ColorScheme()
                    {
                        Accent = accentColor,
                        OnAccent = ContrastingColor(accentColor),
                        Background = XForms.Colors.White,
                        Foreground = XForms.Colors.Black,
                        SubtleForeground = Color.FromRgb(0x66, 0x66, 0x66),
                        Surface = Color.FromRgb(0xf4, 0xf4, 0xf4),
                        OnSurface = XForms.Colors.Black,
                        Outline = Color.FromRgb(0xcc, 0xcc, 0xcc),
                        Error = Color.FromRgb(0xc6, 0x28, 0x28),
                        OnError = XForms.Colors.White,
                        Disabled = Color.FromRgb(0xa0, 0xa0, 0xa0),
                    };
            }
        }

        /// <summary>
        /// Black or white, whichever reads better on the color.
        /// </summary>
        public static Color ContrastingColor(
            Color color)
        {
            // Relative luminance (sRGB, roughly linearized)
            double Channel(byte value)
            {
                double c = value / 255.0;
                return (c <= 0.03928) ? c / 12.92 : Math.Pow((c + 0.055) / 1.055, 2.4);
            }

            double luminance = (0.2126 * Channel(color.R)) + (0.7152 * Channel(color.G)) + (0.0722 * Channel(color.B));
            return (luminance > 0.4) ? XForms.Colors.Black : XForms.Colors.White;
        }

        private static Color Mix(
            Color from,
            Color to,
            float amount)
        {
            byte Lerp(byte a, byte b) => (byte)Math.Round(a + ((b - a) * amount));
            return Color.FromArgb(from.A, Lerp(from.R, to.R), Lerp(from.G, to.G), Lerp(from.B, to.B));
        }
    }
}
