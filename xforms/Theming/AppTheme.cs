using System;

namespace XForms.Controls
{
    /// <summary>
    /// A theme package's entry, which an app passes to <c>XFormsApp.Run</c>:
    /// it gives the palette for each mode, and adds the package's tokens,
    /// styles and control templates to the theme the framework builds from it.
    /// </summary>
    public abstract class AppTheme
    {
        /// <summary>
        /// The package's colors for a mode, around an accent color: plain ones
        /// (<see cref="ColorScheme.Plain"/>) unless overridden. Where the
        /// system gives high-contrast colors, the application uses those
        /// instead, in that mode.
        /// </summary>
        public virtual ColorScheme CreateColors(
            ThemeMode mode,
            Color accentColor)
        {
            return ColorScheme.Plain(mode, accentColor);
        }

        /// <summary>
        /// The theme, with this package's tokens, styles and templates added.
        /// What the theme already has (an app's own) is kept.
        /// </summary>
        public abstract Theme Apply(
            Theme theme);

        /// <summary>
        /// A whole theme for a mode, in this package's palette and look: for a
        /// section with a theme of its own (see <see cref="View.Theme"/>),
        /// whatever the app's mode.
        /// </summary>
        public Theme Create(
            ThemeMode mode,
            Color accentColor)
        {
            return this.Apply(new Theme() { Mode = mode, Colors = this.CreateColors(mode, accentColor) });
        }
    }
}
