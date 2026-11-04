using System;

namespace XForms.Themes.Basic
{
    /// <summary>
    /// The accent where it must read: accent fills with text or marks on
    /// them use the strong accent in both modes (white reads on it); accent
    /// text and icons use it in light mode (it reads on white), and the
    /// accent in dark mode (lightened only as far as it must be to read on
    /// black). So both modes show the chosen color wherever it reads. Marks
    /// that only show where something is (a selection bar, a progress track)
    /// stay the accent.
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
            return theme.Colors.StrongAccent;
        }

        /// <summary>
        /// Text and marks on an accent <see cref="Fill"/>.
        /// </summary>
        public static Color OnFill(
            Theme theme)
        {
            return theme.Colors.OnStrongAccent;
        }
    }
}
