using System;

namespace XForms.Controls
{
    /// <summary>
    /// A text style in a theme's type scale.
    /// </summary>
    public sealed record TextStyle
    {
        public string FontFamily { get; init; } = FontFamilies.SansSerif;

        public required float FontSize { get; init; }

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
        public required TextStyle Display { get; init; }

        /// <summary>Page and section titles.</summary>
        public required TextStyle Title { get; init; }

        /// <summary>Body text.</summary>
        public required TextStyle Body { get; init; }

        /// <summary>Text in controls: buttons, tabs, fields.</summary>
        public required TextStyle Label { get; init; }

        /// <summary>Small supporting text.</summary>
        public required TextStyle Caption { get; init; }
    }

    /// <summary>
    /// A theme's corner radii.
    /// </summary>
    public sealed record Shapes
    {
        /// <summary>Small controls: chips, check boxes, text boxes.</summary>
        public required float SmallCornerRadius { get; init; }

        /// <summary>Buttons, cards and popups.</summary>
        public required float MediumCornerRadius { get; init; }

        /// <summary>Sheets and dialogs.</summary>
        public required float LargeCornerRadius { get; init; }
    }

    /// <summary>
    /// A theme's animation durations and easings.
    /// </summary>
    public sealed record Motion
    {
        /// <summary>Press feedback.</summary>
        public required TimeSpan Short { get; init; }

        /// <summary>Most transitions.</summary>
        public required TimeSpan Medium { get; init; }

        /// <summary>Large movements, such as pages and sheets.</summary>
        public required TimeSpan Long { get; init; }

        /// <summary>Things that move from one place to another.</summary>
        public required EasingFunction Standard { get; init; }

        /// <summary>Things that arrive.</summary>
        public required EasingFunction Decelerate { get; init; }

        /// <summary>Things that leave.</summary>
        public required EasingFunction Accelerate { get; init; }

        /// <summary>Playful arrivals that overshoot and settle.</summary>
        public required EasingFunction Spring { get; init; }
    }

    /// <summary>
    /// XForms.Controls' tokens in a theme (its extensions): the type scale,
    /// shapes and motion (<c>theme.Typography</c> and so on) and the controls'
    /// styles (<c>theme.CheckBoxStyle</c> and so on). These declare only their
    /// shape: only a theme package has their values, and a theme without one
    /// that's needed throws, as a control with no template does.
    /// </summary>
    public static class ThemeTokens
    {
        extension(Theme theme)
        {
            /// <summary>The type scale.</summary>
            public Typography Typography => Require<Typography>(theme);

            /// <summary>Corner radii.</summary>
            public Shapes Shapes => Require<Shapes>(theme);

            /// <summary>Durations and easing curves.</summary>
            public Motion Motion => Require<Motion>(theme);

            /// <summary>How menu bars are laid out.</summary>
            public MenuBarStyle MenuBarStyle => Require<MenuBarStyle>(theme);

            /// <summary>How command bars are laid out.</summary>
            public CommandBarStyle CommandBarStyle => Require<CommandBarStyle>(theme);

            /// <summary>How popups are laid out.</summary>
            public PopupStyle PopupStyle => Require<PopupStyle>(theme);

            /// <summary>
            /// The height of a popup list's rows, for the platform's primary
            /// input (see <see cref="PopupStyle"/>).
            /// </summary>
            public float PopupRowHeight => (Application.Current?.IsTouchPrimary ?? false) ? Require<PopupStyle>(theme).TouchRowHeight : Require<PopupStyle>(theme).RowHeight;

            /// <summary>How check boxes are laid out.</summary>
            public CheckBoxStyle CheckBoxStyle => Require<CheckBoxStyle>(theme);

            /// <summary>How toggle switches are laid out.</summary>
            public ToggleSwitchStyle ToggleSwitchStyle => Require<ToggleSwitchStyle>(theme);

            /// <summary>How sliders are laid out.</summary>
            public SliderStyle SliderStyle => Require<SliderStyle>(theme);

            /// <summary>How progress bars are laid out.</summary>
            public ProgressBarStyle ProgressBarStyle => Require<ProgressBarStyle>(theme);

            /// <summary>How calendars are laid out.</summary>
            public CalendarViewStyle CalendarViewStyle => Require<CalendarViewStyle>(theme);

            /// <summary>How a time picker's popup is laid out.</summary>
            public TimePickerViewStyle TimePickerViewStyle => Require<TimePickerViewStyle>(theme);

            /// <summary>How picker fields are laid out.</summary>
            public PickerFieldStyle PickerFieldStyle => Require<PickerFieldStyle>(theme);

            /// <summary>How number boxes are laid out.</summary>
            public NumberBoxStyle NumberBoxStyle => Require<NumberBoxStyle>(theme);

            /// <summary>How password boxes are laid out.</summary>
            public PasswordBoxStyle PasswordBoxStyle => Require<PasswordBoxStyle>(theme);

            /// <summary>How menu flyouts are laid out.</summary>
            public MenuFlyoutStyle MenuFlyoutStyle => Require<MenuFlyoutStyle>(theme);

            /// <summary>How tooltips are laid out.</summary>
            public ToolTipStyle ToolTipStyle => Require<ToolTipStyle>(theme);

            /// <summary>How info bars are laid out.</summary>
            public InfoBarStyle InfoBarStyle => Require<InfoBarStyle>(theme);

            /// <summary>How expanders are laid out, and their content arrives.</summary>
            public ExpanderStyle ExpanderStyle => Require<ExpanderStyle>(theme);

            /// <summary>How tree views' items are laid out.</summary>
            public TreeViewStyle TreeViewStyle => Require<TreeViewStyle>(theme);

            /// <summary>How navigation views are laid out.</summary>
            public NavigationViewStyle NavigationViewStyle => Require<NavigationViewStyle>(theme);

            /// <summary>How tab strips are laid out.</summary>
            public TabStripStyle TabStripStyle => Require<TabStripStyle>(theme);

            /// <summary>How radio buttons are laid out.</summary>
            public RadioButtonStyle RadioButtonStyle => Require<RadioButtonStyle>(theme);

            /// <summary>How days-of-the-week pickers are laid out.</summary>
            public DaysOfWeekPickerStyle DaysOfWeekPickerStyle => Require<DaysOfWeekPickerStyle>(theme);

            /// <summary>How flip views are laid out.</summary>
            public FlipViewStyle FlipViewStyle => Require<FlipViewStyle>(theme);

            /// <summary>How pips pagers are laid out.</summary>
            public PipsPagerStyle PipsPagerStyle => Require<PipsPagerStyle>(theme);

            /// <summary>How list views' default group headers are laid out.</summary>
            public ListViewStyle ListViewStyle => Require<ListViewStyle>(theme);

            /// <summary>How flyout buttons move.</summary>
            public FlyoutButtonStyle FlyoutButtonStyle => Require<FlyoutButtonStyle>(theme);
        }

        private static TToken Require<TToken>(
            Theme theme)
            where TToken : class
        {
            return theme.GetExtension<TToken>()
                ?? throw new InvalidOperationException(
                    $"The theme has no {typeof(TToken).Name}. Pass a theme to XFormsApp.Run, such as new BasicTheme() (from XForms.Themes.Basic).");
        }
    }
}
