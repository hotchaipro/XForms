using System;

namespace XForms.Themes.Basic
{
    /// <summary>
    /// A popup's card: the page's background in light mode, which its shadow
    /// lifts; in dark mode, where a shadow can't show, a lighter surface with
    /// an edge (as iOS's elevated dark background). What's drawn in it
    /// (highlighted rows) tints the card, so it reads on either.
    /// </summary>
    internal static class PopupColors
    {
        private static readonly Color ElevatedDark = Color.FromRgb(0x2c, 0x2c, 0x2e);

        public static Color Card(
            Theme theme)
        {
            return (theme.Mode == ThemeMode.Dark) ? ElevatedDark : theme.Colors.Background;
        }

        /// <summary>
        /// The card's edge, or transparent for none (light mode: its shadow).
        /// </summary>
        public static Color Edge(
            Theme theme)
        {
            return (theme.Mode == ThemeMode.Light) ? Colors.Transparent : theme.Colors.Outline;
        }

        /// <summary>
        /// A highlighted row in a popup: the surface in light mode, and a
        /// light tint of the foreground otherwise.
        /// </summary>
        public static Color Highlight(
            Theme theme)
        {
            return (theme.Mode == ThemeMode.Light) ? theme.Colors.Surface : theme.Colors.Foreground.MultiplyAlpha(0.1f);
        }
    }
}
