using System;

namespace XForms.Controls
{
    /// <summary>
    /// A text style in a theme's type scale.
    /// </summary>
    public sealed record TextStyle
    {
        public string FontFamily { get; init; } = FontFamilies.SansSerif;

        public float FontSize { get; init; } = 15;

        public FontWeight FontWeight { get; init; } = FontWeight.Normal;

        /// <summary>
        /// A new text format with the style's family, size and weight.
        /// </summary>
        public TextFormat ToTextFormat()
        {
            return new TextFormat()
            {
                FontFamily = this.FontFamily,
                FontSize = this.FontSize,
                FontWeight = this.FontWeight,
            };
        }
    }

    /// <summary>
    /// A theme's type scale.
    /// </summary>
    public sealed record Typography
    {
        /// <summary>Large numbers and headlines.</summary>
        public TextStyle Display { get; init; } = new TextStyle() { FontSize = 34, FontWeight = FontWeight.Light };

        /// <summary>Page and section titles.</summary>
        public TextStyle Title { get; init; } = new TextStyle() { FontSize = 20, FontWeight = FontWeight.SemiBold };

        /// <summary>Body text.</summary>
        public TextStyle Body { get; init; } = new TextStyle() { FontSize = 15 };

        /// <summary>Text in controls: buttons, tabs, fields.</summary>
        public TextStyle Label { get; init; } = new TextStyle() { FontSize = 14 };

        /// <summary>Small supporting text.</summary>
        public TextStyle Caption { get; init; } = new TextStyle() { FontSize = 12 };
    }

    /// <summary>
    /// A theme's corner radii.
    /// </summary>
    public sealed record Shapes
    {
        /// <summary>Small controls: chips, check boxes, text boxes.</summary>
        public float SmallCornerRadius { get; init; } = 4;

        /// <summary>Buttons, cards and popups.</summary>
        public float MediumCornerRadius { get; init; } = 8;

        /// <summary>Sheets and dialogs.</summary>
        public float LargeCornerRadius { get; init; } = 16;
    }

    /// <summary>
    /// A theme's animation durations and easings.
    /// </summary>
    public sealed record Motion
    {
        /// <summary>Press feedback.</summary>
        public TimeSpan Short { get; init; } = TimeSpan.FromMilliseconds(100);

        /// <summary>Most transitions.</summary>
        public TimeSpan Medium { get; init; } = TimeSpan.FromMilliseconds(250);

        /// <summary>Large movements, such as pages and sheets.</summary>
        public TimeSpan Long { get; init; } = TimeSpan.FromMilliseconds(400);

        /// <summary>Things that move from one place to another.</summary>
        public EasingFunction Standard { get; init; } = new CubicEase(EasingMode.EaseInOut);

        /// <summary>Things that arrive.</summary>
        public EasingFunction Decelerate { get; init; } = new CubicEase(EasingMode.EaseOut);

        /// <summary>Things that leave.</summary>
        public EasingFunction Accelerate { get; init; } = new CubicEase(EasingMode.EaseIn);

        /// <summary>Playful arrivals that overshoot and settle.</summary>
        public EasingFunction Spring { get; init; } = new SpringEase(EasingMode.EaseOut);
    }

    /// <summary>
    /// XForms.Controls' tokens in a theme (its extensions), with defaults when
    /// the theme has none: <c>theme.Typography</c>, <c>theme.Shapes</c>,
    /// <c>theme.Motion</c>.
    /// </summary>
    public static class ThemeTokens
    {
        private static readonly Typography DefaultTypography = new Typography();
        private static readonly Shapes DefaultShapes = new Shapes();
        private static readonly Motion DefaultMotion = new Motion();

        extension(Theme theme)
        {
            /// <summary>
            /// The type scale.
            /// </summary>
            public Typography Typography => theme.GetExtension<Typography>() ?? DefaultTypography;

            /// <summary>
            /// Corner radii.
            /// </summary>
            public Shapes Shapes => theme.GetExtension<Shapes>() ?? DefaultShapes;

            /// <summary>
            /// Durations and easing curves.
            /// </summary>
            public Motion Motion => theme.GetExtension<Motion>() ?? DefaultMotion;
        }
    }
}
