using System;

namespace XForma.Themes.Basic
{
    /// <summary>
    /// The colors the Basic theme draws with that a <see cref="ColorScheme"/>
    /// has no role for. Set one on a theme to change them, alongside a palette
    /// of its own.
    /// </summary>
    /// <example>
    /// <code>
    /// theme = theme.WithExtension(new BasicColors { Success = brandGreen });
    /// </code>
    /// </example>
    public sealed record BasicColors
    {
        // A subtle floating button stands out from a dark page
        internal static readonly Color DarkSubtleFloatingBackground = Color.FromRgb(0x33, 0x33, 0x33);

        internal static readonly BasicColors Default = new BasicColors();

        /// <summary>An info bar's success.</summary>
        public Color Success { get; init; } = Color.FromRgb(0x2E, 0x9E, 0x44);

        /// <summary>An info bar's warning.</summary>
        public Color Warning { get; init; } = Color.FromRgb(0xE8, 0x9E, 0x00);

        /// <summary>The mark in an info bar's icon, on the success color.</summary>
        public Color OnSuccess { get; init; } = Colors.White;

        /// <summary>The mark in an info bar's icon, on the warning color.</summary>
        public Color OnWarning { get; init; } = Colors.Black;

        /// <summary>
        /// The icon or letter of a round toggle while it's pressed, on the
        /// subtle foreground color.
        /// </summary>
        public Color OnPressed { get; init; } = Colors.White;

        /// <summary>
        /// The icon and text behind a swiped row, on its command's color,
        /// unless the command sets <see cref="XForma.Controls.UICommand.ForegroundColor"/>.
        /// </summary>
        public Color OnCommand { get; init; } = Colors.White;

        /// <summary>
        /// A subtle floating button's fill, or null (the default) for the
        /// page's background in light mode, and a dark gray otherwise.
        /// </summary>
        public Color? SubtleFloatingBackground { get; init; }

        /// <summary>A floating button's fill while it's pressed or disabled.</summary>
        public Color FloatingPressedBackground { get; init; } = Colors.White;

        /// <summary>A floating button's icon while it's pressed or disabled.</summary>
        public Color FloatingPressedForeground { get; init; } = Colors.Gray;

        /// <summary>
        /// The shadow beneath floating buttons, and beneath popups in light
        /// mode, with its opacity.
        /// </summary>
        public Color Shadow { get; init; } = Color.FromArgb(0x4d, 0, 0, 0);

        /// <summary>
        /// What shows around popups where a shadow wouldn't (dark and high
        /// contrast), with its opacity.
        /// </summary>
        public Color Glow { get; init; } = XForma.Shadow.GlowColor;
    }

    internal static class BasicColorsExtensions
    {
        public static BasicColors GetBasicColors(
            this Theme theme)
        {
            return theme.GetExtension<BasicColors>() ?? BasicColors.Default;
        }
    }
}
