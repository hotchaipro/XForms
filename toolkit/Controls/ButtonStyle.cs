using System;

namespace XForms.Toolkit
{
    /// <summary>
    /// How a <see cref="Button"/> looks. Unset values come from the theme's
    /// default <c>ButtonStyle</c> (<see cref="Theme.GetStyle{TStyle}"/>), and
    /// then from the theme's tokens.
    /// </summary>
    /// <example>
    /// <code>
    /// // One button
    /// new Button { Text = "Delete", Style = new ButtonStyle { Background = Colors.Red } };
    ///
    /// // Every button under a view
    /// view.Theme = theme.WithStyle(new ButtonStyle { CornerRadius = 20 });
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
