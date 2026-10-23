using System;
using XForms.Controls;

namespace XForms.Themes.Basic
{
    /// <summary>
    /// How the Basic theme draws a <see cref="Button"/>. Unset values come
    /// from the theme's tokens. Set one on a theme for every button under a
    /// view.
    /// </summary>
    /// <example>
    /// <code>
    /// view.Theme = theme.WithExtension(new ButtonStyle { CornerRadius = 20 });
    /// </code>
    /// </example>
    public sealed record ButtonStyle
    {
        /// <summary>The fill: the theme's accent color by default.</summary>
        public Color? Background { get; init; }

        /// <summary>The text: the color on the accent by default.</summary>
        public Color? Foreground { get; init; }

        /// <summary>The corners: the theme's small corner radius by default.</summary>
        public float? CornerRadius { get; init; }

        /// <summary>The text style: the theme's label style by default.</summary>
        public TextStyle TextStyle { get; init; }

        /// <summary>
        /// Combines two styles: this one's values, and the other's where this
        /// one has none.
        /// </summary>
        public ButtonStyle Over(
            ButtonStyle other)
        {
            if (null == other)
            {
                return this;
            }

            return new ButtonStyle()
            {
                Background = this.Background ?? other.Background,
                Foreground = this.Foreground ?? other.Foreground,
                CornerRadius = this.CornerRadius ?? other.CornerRadius,
                TextStyle = this.TextStyle ?? other.TextStyle,
            };
        }
    }
}
