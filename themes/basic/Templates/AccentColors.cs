using System;

namespace XForms.Themes.Basic
{
    /// <summary>
    /// The accent where it must read: text and icons in the accent, and fills
    /// with text or marks on them, use the strong accent in light mode (white
    /// reads on it, and it reads on white). Dark mode's accent is already
    /// lightened to read on the dark background. Marks that only show where
    /// something is (a selection bar, a progress track) stay the accent.
    /// </summary>
    internal static class AccentColors
    {
        /// <summary>
        /// Text and icons in the accent, on the background or a light tint.
        /// </summary>
        public static Color Text(
            Theme theme)
        {
            return (theme.Mode == ThemeMode.Light) ? theme.Colors.StrongAccent : theme.Colors.Accent;
        }

        /// <summary>
        /// A fill in the accent, with <see cref="OnFill"/> on it.
        /// </summary>
        public static Color Fill(
            Theme theme)
        {
            return (theme.Mode == ThemeMode.Light) ? theme.Colors.StrongAccent : theme.Colors.Accent;
        }

        /// <summary>
        /// Text and marks on an accent <see cref="Fill"/>.
        /// </summary>
        public static Color OnFill(
            Theme theme)
        {
            return (theme.Mode == ThemeMode.Light) ? theme.Colors.OnStrongAccent : theme.Colors.OnAccent;
        }
    }
}
