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

        /// <summary>
        /// The accent, deep enough for white text and icons on it (at least
        /// 4.5:1): filled buttons and bars. The same hue as the accent, darker
        /// if it needs to be (see <see cref="Theme.StrongColor"/>).
        /// </summary>
        public Color StrongAccent { get; init; }

        /// <summary>Text and icons on the strong accent color.</summary>
        public Color OnStrongAccent { get; init; }

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
        /// accent is lightened as far as it needs to be to read on the black
        /// background (see <see cref="BrightColor"/>).
        /// </summary>
        public static ColorScheme CreateColors(
            ThemeMode mode,
            Color accentColor)
        {
            switch (mode)
            {
                case ThemeMode.Dark:
                {
                    var accent = BrightColor(accentColor);
                    return new ColorScheme()
                    {
                        Accent = accent,
                        OnAccent = ContrastingColor(accent),
                        StrongAccent = StrongColor(accentColor),
                        OnStrongAccent = XForms.Colors.White,
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
                        StrongAccent = Color.FromRgb(0xff, 0xff, 0x00),
                        OnStrongAccent = XForms.Colors.Black,
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
                        StrongAccent = StrongColor(accentColor),
                        OnStrongAccent = XForms.Colors.White,
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
            return (Luminance(color) > 0.4) ? XForms.Colors.Black : XForms.Colors.White;
        }

        /// <summary>
        /// The color, darkened (keeping its hue and saturation) until white on
        /// it has a contrast of at least 4.5:1, the WCAG's for text; a color
        /// that already has it is returned as it is.
        /// </summary>
        public static Color StrongColor(
            Color color)
        {
            // White's contrast with a color is 1.05 / (L + 0.05)
            const double MaxLuminance = (1.05 / 4.5) - 0.05;
            if (Luminance(color) <= MaxLuminance)
            {
                return color;
            }

            // Lower the HSL lightness in small steps, so the color stays as
            // light as it can
            ToHsl(color, out double hue, out double saturation, out double lightness);
            var strong = color;
            while ((Luminance(strong) > MaxLuminance) && (lightness > 0))
            {
                lightness = Math.Max(0, lightness - 0.005);
                strong = FromHsl(color.A, hue, saturation, lightness);
            }

            return strong;
        }

        /// <summary>
        /// The color, lightened (keeping its hue and saturation) until it has
        /// a contrast of at least 4.5:1 on black, the WCAG's for text; a color
        /// that already has it is returned as it is. The mirror of
        /// <see cref="StrongColor"/>, for dark backgrounds.
        /// </summary>
        public static Color BrightColor(
            Color color)
        {
            // A color's contrast with black is (L + 0.05) / 0.05
            const double MinLuminance = (4.5 * 0.05) - 0.05;
            if (Luminance(color) >= MinLuminance)
            {
                return color;
            }

            // Raise the HSL lightness in small steps, so the color stays as
            // deep as it can
            ToHsl(color, out double hue, out double saturation, out double lightness);
            var bright = color;
            while ((Luminance(bright) < MinLuminance) && (lightness < 1))
            {
                lightness = Math.Min(1, lightness + 0.005);
                bright = FromHsl(color.A, hue, saturation, lightness);
            }

            return bright;
        }

        // Relative luminance (sRGB, linearized)
        private static double Luminance(
            Color color)
        {
            static double Channel(byte value)
            {
                double c = value / 255.0;
                return (c <= 0.04045) ? c / 12.92 : Math.Pow((c + 0.055) / 1.055, 2.4);
            }

            return (0.2126 * Channel(color.R)) + (0.7152 * Channel(color.G)) + (0.0722 * Channel(color.B));
        }

        private static void ToHsl(
            Color color,
            out double hue,
            out double saturation,
            out double lightness)
        {
            double r = color.R / 255.0, g = color.G / 255.0, b = color.B / 255.0;
            double max = Math.Max(r, Math.Max(g, b));
            double min = Math.Min(r, Math.Min(g, b));
            double delta = max - min;

            lightness = (max + min) / 2;
            if (delta == 0)
            {
                hue = 0;
                saturation = 0;
                return;
            }

            saturation = (lightness <= 0.5) ? delta / (max + min) : delta / (2 - max - min);
            if (max == r)
            {
                hue = ((g - b) / delta) + ((g < b) ? 6 : 0);
            }
            else if (max == g)
            {
                hue = ((b - r) / delta) + 2;
            }
            else
            {
                hue = ((r - g) / delta) + 4;
            }

            hue /= 6;
        }

        private static Color FromHsl(
            byte alpha,
            double hue,
            double saturation,
            double lightness)
        {
            double q = (lightness <= 0.5) ? lightness * (1 + saturation) : lightness + saturation - (lightness * saturation);
            double p = (2 * lightness) - q;

            static double Channel(double p, double q, double t)
            {
                t = (t < 0) ? t + 1 : ((t > 1) ? t - 1 : t);
                if (t < 1.0 / 6) return p + ((q - p) * 6 * t);
                if (t < 0.5) return q;
                if (t < 2.0 / 3) return p + ((q - p) * ((2.0 / 3) - t) * 6);
                return p;
            }

            static byte Byte(double value) => (byte)Math.Round(Math.Clamp(value, 0, 1) * 255);
            return Color.FromArgb(
                alpha,
                Byte(Channel(p, q, hue + (1.0 / 3))),
                Byte(Channel(p, q, hue)),
                Byte(Channel(p, q, hue - (1.0 / 3))));
        }
    }
}
