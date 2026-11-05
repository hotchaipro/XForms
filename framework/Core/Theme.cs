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
    /// The framework knows the roles, which primitives and platforms use; the
    /// values are a theme package's (<c>AppTheme.CreateColors</c>).
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

        /// <summary>
        /// What dims the window behind a flyout or a drawer, with its opacity.
        /// </summary>
        public Color Scrim { get; init; }

        /// <summary>
        /// Plain colors for a mode, for an app with no theme package: black
        /// and white and a gray, around the accent color. A theme package has
        /// its own palette (<c>AppTheme.CreateColors</c>).
        /// </summary>
        public static ColorScheme Plain(
            ThemeMode mode,
            Color accentColor)
        {
            bool isLight = mode == ThemeMode.Light;
            bool isHighContrast = mode == ThemeMode.HighContrast;
            Color background = isLight ? Colors.White : Colors.Black;
            Color foreground = isLight ? Colors.Black : Colors.White;
            Color subtle = isHighContrast ? foreground : Colors.Gray;
            Color accent = isHighContrast ? foreground : accentColor;
            Color error = isLight ? Color.FromRgb(0xc0, 0x00, 0x00) : Color.FromRgb(0xff, 0x60, 0x60);

            return new ColorScheme()
            {
                Accent = accent,
                OnAccent = Theme.ContrastingColor(accent),
                Background = background,
                Foreground = foreground,
                SubtleForeground = subtle,
                Surface = background,
                OnSurface = foreground,
                Outline = subtle,
                Error = error,
                OnError = Theme.ContrastingColor(error),
                Disabled = Colors.Gray,
                Scrim = Color.FromArgb(0x80, 0x00, 0x00, 0x00),
            };
        }
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
        /// The shadow along a sliding page's moving edges, so they show over a
        /// page of the same color; null (the default) for none.
        /// </summary>
        public Shadow PageEdgeShadow { get; init; }

        /// <summary>
        /// How a frame's pages come and go when navigating doesn't name a
        /// transition; null (the default) for <see cref="XForms.PageTransition.Slide"/>.
        /// A switch following a gesture always slides: over the current page
        /// for <see cref="XForms.PageTransition.SlideOver"/>, otherwise
        /// together with it.
        /// </summary>
        public PageTransition PageTransition { get; init; }

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
        /// Black or white, whichever reads better on the color.
        /// </summary>
        public static Color ContrastingColor(
            Color color)
        {
            return (Luminance(color) > 0.4) ? XForms.Colors.Black : XForms.Colors.White;
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
    }
}
