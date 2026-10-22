using System;

namespace XForms.Controls
{
    /// <summary>
    /// A theme package's entry, which an app passes to <c>XFormsApp.Run</c>:
    /// it adds the package's tokens, styles and control templates to the
    /// theme the framework builds for the mode and accent color.
    /// </summary>
    public abstract class AppTheme
    {
        /// <summary>
        /// The theme, with this package's tokens, styles and templates added.
        /// What the theme already has (an app's own) is kept.
        /// </summary>
        public abstract Theme Apply(
            Theme theme);
    }
}
