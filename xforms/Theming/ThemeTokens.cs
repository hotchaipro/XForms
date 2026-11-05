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
    /// A theme's control sizes.
    /// </summary>
    public sealed record Sizes
    {
        /// <summary>
        /// The rows of a popup list (a combo box's, a menu flyout's, a text
        /// box's suggestions), or null (the default) for the platform's
        /// primary input: 48 for touch, 36 for a mouse.
        /// </summary>
        public float? PopupRowHeight { get; init; }
    }

    /// <summary>
    /// How a theme lays out a <see cref="MenuBar"/>.
    /// </summary>
    public sealed record MenuBarStyle
    {
        /// <summary>The bar's height.</summary>
        public float Height { get; init; } = 50;

        /// <summary>Each item's width.</summary>
        public float ItemWidth { get; init; } = 72;

        /// <summary>The space between items.</summary>
        public float ItemSpacing { get; init; }

        /// <summary>Where the items sit across the bar.</summary>
        public HorizontalAlignment ItemsAlignment { get; init; } = HorizontalAlignment.Left;
    }

    /// <summary>
    /// How a theme lays out a <see cref="CommandBar"/> (a command list's rows
    /// have one too).
    /// </summary>
    public sealed record CommandBarStyle
    {
        /// <summary>
        /// The space between neighbouring buttons' icons and labels. Each
        /// button's touch area takes its share of it, so a tap between two
        /// lands on the nearer. Without room for it across the bar, the
        /// buttons go side by side.
        /// </summary>
        public float ButtonSpacing { get; init; }

        /// <summary>The buttons' height.</summary>
        public float ButtonHeight { get; init; } = 48;

        /// <summary>The labels' font size, or null (the default) for the template's.</summary>
        public float? LabelFontSize { get; init; }
    }

    /// <summary>
    /// XForms.Controls' tokens in a theme (its extensions), with defaults when
    /// the theme has none: <c>theme.Typography</c>, <c>theme.Shapes</c>,
    /// <c>theme.Motion</c>, <c>theme.MenuBarStyle</c>,
    /// <c>theme.CommandBarStyle</c>, <c>theme.PopupRowHeight</c>.
    /// </summary>
    public static class ThemeTokens
    {
        private static readonly Typography DefaultTypography = new Typography();
        private static readonly Shapes DefaultShapes = new Shapes();
        private static readonly Motion DefaultMotion = new Motion();
        private static readonly MenuBarStyle DefaultMenuBarStyle = new MenuBarStyle();
        private static readonly CommandBarStyle DefaultCommandBarStyle = new CommandBarStyle();

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

            /// <summary>
            /// How menu bars are laid out.
            /// </summary>
            public MenuBarStyle MenuBarStyle => theme.GetExtension<MenuBarStyle>() ?? DefaultMenuBarStyle;

            /// <summary>
            /// How command bars are laid out.
            /// </summary>
            public CommandBarStyle CommandBarStyle => theme.GetExtension<CommandBarStyle>() ?? DefaultCommandBarStyle;

            /// <summary>
            /// The height of a popup list's rows (see <see cref="Sizes.PopupRowHeight"/>).
            /// </summary>
            public float PopupRowHeight => theme.GetExtension<Sizes>()?.PopupRowHeight ?? ((Application.Current?.IsTouchPrimary ?? false) ? 48 : 36);
        }
    }
}
