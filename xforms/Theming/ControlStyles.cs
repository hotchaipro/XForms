namespace XForms.Controls
{
    // What each control lays out and hit-tests by, and its decorative motion.
    // These declare only the shape: the values are the theme package's. Every
    // property is required, so a style can't be half a look; to change part of
    // one, copy the theme's (theme.CheckBoxStyle with { BoxSize = 24 }).

    /// <summary>
    /// How a theme lays out a <see cref="MenuBar"/>.
    /// </summary>
    public sealed record MenuBarStyle
    {
        /// <summary>The bar's height.</summary>
        public required float Height { get; init; }

        /// <summary>Each item's width.</summary>
        public required float ItemWidth { get; init; }

        /// <summary>The space between items.</summary>
        public required float ItemSpacing { get; init; }

        /// <summary>Where the items sit across the bar.</summary>
        public required HorizontalAlignment ItemsAlignment { get; init; }
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
        public required float ButtonSpacing { get; init; }

        /// <summary>The buttons' width, before their share of the spacing.</summary>
        public required float ButtonWidth { get; init; }

        /// <summary>The buttons' height.</summary>
        public required float ButtonHeight { get; init; }

        /// <summary>The labels' font size, or null (the default) for the template's.</summary>
        public float? LabelFontSize { get; init; }

        /// <summary>The size of the icon on a command list's swiped row.</summary>
        public required Size SwipeIconSize { get; init; }
    }

    /// <summary>
    /// How a theme lays out a popup: its frame and the lists in it.
    /// </summary>
    public sealed record PopupStyle
    {
        /// <summary>How far inside the frame's edges its content stays, clear of its corners.</summary>
        public required float ContentInset { get; init; }

        /// <summary>The space above and below a drop-down's list, beyond the inset.</summary>
        public required float ListVerticalPadding { get; init; }

        /// <summary>
        /// The rows of a popup list (a combo box's, a menu flyout's, a text
        /// box's suggestions) where a mouse is the primary input.
        /// </summary>
        public required float RowHeight { get; init; }

        /// <summary>The rows of a popup list where touch is the primary input.</summary>
        public required float TouchRowHeight { get; init; }
    }

    /// <summary>
    /// How a theme lays out a <see cref="CheckBox"/>: a box at the start, then
    /// its text.
    /// </summary>
    public sealed record CheckBoxStyle
    {
        /// <summary>The box's size.</summary>
        public required float BoxSize { get; init; }

        /// <summary>The space between the box and the text.</summary>
        public required float TextGap { get; init; }

        /// <summary>The check box's least height, which takes presses.</summary>
        public required float MinimumHeight { get; init; }
    }

    /// <summary>
    /// How a theme lays out a <see cref="ToggleSwitch"/>: its header, then a
    /// track at the start of a band, then the state's text.
    /// </summary>
    public sealed record ToggleSwitchStyle
    {
        /// <summary>The track's width.</summary>
        public required float TrackWidth { get; init; }

        /// <summary>The track's height.</summary>
        public required float TrackHeight { get; init; }

        /// <summary>The height of the band the track is in, which takes presses.</summary>
        public required float TouchHeight { get; init; }

        /// <summary>The space between the track and the text.</summary>
        public required float TextGap { get; init; }

        /// <summary>The space between the header and the band.</summary>
        public required float HeaderGap { get; init; }
    }

    /// <summary>
    /// How a theme lays out a <see cref="Slider"/>: its header, then the track
    /// across the middle of a band, inset by half a thumb at each end.
    /// </summary>
    public sealed record SliderStyle
    {
        /// <summary>The thumb's diameter.</summary>
        public required float ThumbSize { get; init; }

        /// <summary>The height of the band the track runs across, which takes presses.</summary>
        public required float TouchHeight { get; init; }

        /// <summary>The space between the header and the band.</summary>
        public required float HeaderGap { get; init; }

        /// <summary>The slider's width when it's offered any.</summary>
        public required float DefaultWidth { get; init; }
    }

    /// <summary>
    /// How a theme lays out a <see cref="ProgressBar"/>.
    /// </summary>
    public sealed record ProgressBarStyle
    {
        /// <summary>The bar's height.</summary>
        public required float Height { get; init; }

        /// <summary>The bar's width when it's offered any.</summary>
        public required float DefaultWidth { get; init; }
    }

    /// <summary>
    /// How a theme lays out a <see cref="CalendarView"/>: a header with the
    /// month and its arrows, the days of the week, and six weeks of days.
    /// </summary>
    public sealed record CalendarViewStyle
    {
        /// <summary>The size of a day's cell, and the width of an arrow button.</summary>
        public required float CellSize { get; init; }

        /// <summary>The height of the header with the month and its arrows.</summary>
        public required float HeaderHeight { get; init; }

        /// <summary>The height of the row with the days of the week.</summary>
        public required float WeekdayHeight { get; init; }

        /// <summary>The space around the calendar.</summary>
        public required float Inset { get; init; }

        /// <summary>The space before the month's name.</summary>
        public required float HeaderInset { get; init; }
    }

    /// <summary>
    /// How a theme lays out a <see cref="TimePickerView"/>: its columns of
    /// rows, then Accept and Dismiss.
    /// </summary>
    public sealed record TimePickerViewStyle
    {
        /// <summary>The height of a row.</summary>
        public required float RowHeight { get; init; }

        /// <summary>The width of a column.</summary>
        public required float ColumnWidth { get; init; }

        /// <summary>The height of Accept and Dismiss.</summary>
        public required float ButtonHeight { get; init; }
    }

    /// <summary>
    /// How a theme lays out a <see cref="PickerField"/>: the text after an
    /// inset, and a glyph at the end.
    /// </summary>
    public sealed record PickerFieldStyle
    {
        /// <summary>The space before the text.</summary>
        public required float TextInset { get; init; }

        /// <summary>The width of the glyph's area.</summary>
        public required float GlyphWidth { get; init; }
    }

    /// <summary>
    /// How a theme lays out a <see cref="NumberBox"/>.
    /// </summary>
    public sealed record NumberBoxStyle
    {
        /// <summary>The width of each spin button.</summary>
        public required float SpinButtonWidth { get; init; }
    }

    /// <summary>
    /// How a theme lays out a <see cref="PasswordBox"/>.
    /// </summary>
    public sealed record PasswordBoxStyle
    {
        /// <summary>The width of the reveal button.</summary>
        public required float RevealButtonWidth { get; init; }
    }

    /// <summary>
    /// How a theme lays out a <see cref="MenuFlyout"/>'s rows.
    /// </summary>
    public sealed record MenuFlyoutStyle
    {
        /// <summary>The space before an item's text, which leaves room for its icon.</summary>
        public required float TextInset { get; init; }

        /// <summary>The space after an item's text.</summary>
        public required float TextEndInset { get; init; }

        /// <summary>The menu's least width.</summary>
        public required float MinimumWidth { get; init; }
    }

    /// <summary>
    /// How a theme lays out a <see cref="ToolTip"/>.
    /// </summary>
    public sealed record ToolTipStyle
    {
        /// <summary>The widest a tooltip grows before its text wraps.</summary>
        public required float MaximumWidth { get; init; }

        /// <summary>The space around the text.</summary>
        public required Thickness Padding { get; init; }
    }

    /// <summary>
    /// How a theme lays out an <see cref="InfoBar"/>: an icon, then the title,
    /// message and action, and a close button in the corner.
    /// </summary>
    public sealed record InfoBarStyle
    {
        /// <summary>The space around and between its parts.</summary>
        public required float Spacing { get; init; }

        /// <summary>The icon's size.</summary>
        public required float IconSize { get; init; }

        /// <summary>How far the icon sits below the top of the text.</summary>
        public required float IconOffset { get; init; }

        /// <summary>The close button's size.</summary>
        public required float CloseButtonSize { get; init; }

        /// <summary>How far the close button sits out into the spacing at the corner.</summary>
        public required float CloseButtonOutset { get; init; }

        /// <summary>The bar's width when it's offered any.</summary>
        public required float DefaultWidth { get; init; }

        /// <summary>The title's weight.</summary>
        public required FontWeight TitleFontWeight { get; init; }
    }

    /// <summary>
    /// How a theme lays out an <see cref="Expander"/>, and how its content
    /// arrives.
    /// </summary>
    public sealed record ExpanderStyle
    {
        /// <summary>The header's height.</summary>
        public required float HeaderHeight { get; init; }

        /// <summary>The space beside the header's text when it sizes to it, which holds the chevron.</summary>
        public required float HeaderChromeWidth { get; init; }

        /// <summary>The space around the content.</summary>
        public required Thickness ContentPadding { get; init; }

        /// <summary>How far above its place the content starts as it slides and fades in.</summary>
        public required float RevealOffset { get; init; }
    }

    /// <summary>
    /// How a theme lays out a <see cref="TreeView"/>'s items.
    /// </summary>
    public sealed record TreeViewStyle
    {
        /// <summary>An item's height.</summary>
        public required float ItemHeight { get; init; }

        /// <summary>How far each level is indented.</summary>
        public required float Indent { get; init; }

        /// <summary>The width of the chevron's area, which takes presses.</summary>
        public required float ChevronWidth { get; init; }

        /// <summary>The space at each end of an item.</summary>
        public required float ItemInset { get; init; }
    }

    /// <summary>
    /// How a theme lays out a <see cref="NavigationView"/>.
    /// </summary>
    public sealed record NavigationViewStyle
    {
        /// <summary>The pane's width.</summary>
        public required float PaneWidth { get; init; }

        /// <summary>The height of the bar across the top of a narrow view, and the width of its menu button.</summary>
        public required float BarHeight { get; init; }

        /// <summary>The height of the pane's title.</summary>
        public required float TitleHeight { get; init; }

        /// <summary>An item's height.</summary>
        public required float ItemHeight { get; init; }

        /// <summary>The space around an item.</summary>
        public required Thickness ItemMargin { get; init; }

        /// <summary>The space between items.</summary>
        public required float ItemSpacing { get; init; }

        /// <summary>The view's size when it's offered any.</summary>
        public required Size DefaultSize { get; init; }
    }

    /// <summary>
    /// How a theme lays out a <see cref="TabStrip"/>.
    /// </summary>
    public sealed record TabStripStyle
    {
        /// <summary>The widest the tabs spread across the strip.</summary>
        public required float MaximumWidth { get; init; }
    }

    /// <summary>
    /// How a theme lays out a <see cref="RadioButtons"/> group.
    /// </summary>
    public sealed record RadioButtonStyle
    {
        /// <summary>A button's width.</summary>
        public required float Width { get; init; }

        /// <summary>A button's, and the group's, height.</summary>
        public required float Height { get; init; }
    }

    /// <summary>
    /// How a theme lays out a <see cref="DaysOfWeekPicker"/>.
    /// </summary>
    public sealed record DaysOfWeekPickerStyle
    {
        /// <summary>A day button's size, and the picker's height.</summary>
        public required float ButtonSize { get; init; }

        /// <summary>The widest the buttons spread.</summary>
        public required float MaximumWidth { get; init; }
    }

    /// <summary>
    /// How a theme lays out a <see cref="FlipView"/>.
    /// </summary>
    public sealed record FlipViewStyle
    {
        /// <summary>The width of the previous and next buttons.</summary>
        public required float ButtonWidth { get; init; }

        /// <summary>The tallest the previous and next buttons grow.</summary>
        public required float ButtonMaximumHeight { get; init; }

        /// <summary>The view's size when it's offered any.</summary>
        public required Size DefaultSize { get; init; }
    }

    /// <summary>
    /// How a theme lays out a <see cref="PipsPager"/>.
    /// </summary>
    public sealed record PipsPagerStyle
    {
        /// <summary>The distance between dots, each of which takes presses across it.</summary>
        public required float DotSpacing { get; init; }

        /// <summary>The pager's height.</summary>
        public required float Height { get; init; }
    }

    /// <summary>
    /// How a theme lays out a <see cref="ListView{T}"/>'s default group headers.
    /// </summary>
    public sealed record ListViewStyle
    {
        /// <summary>The space around a group header.</summary>
        public required Thickness GroupHeaderMargin { get; init; }

        /// <summary>A group header's weight.</summary>
        public required FontWeight GroupHeaderFontWeight { get; init; }
    }

    /// <summary>
    /// How a theme moves a <see cref="FlyoutButton"/>: the button turning and
    /// shrinking as it opens, and its buttons fanning out on an arc.
    /// </summary>
    public sealed record FlyoutButtonStyle
    {
        /// <summary>How far the button turns as it opens.</summary>
        public required Angle OpenRotation { get; init; }

        /// <summary>The button's scale while it's open.</summary>
        public required float OpenScale { get; init; }

        /// <summary>The scale its buttons grow from and shrink back to.</summary>
        public required float CollapsedButtonScale { get; init; }

        /// <summary>The direction of the arc's middle, from the left (a quarter turn is straight up).</summary>
        public required Angle ArcCenter { get; init; }

        /// <summary>How far an odd number of buttons spreads.</summary>
        public required Angle ArcSweep { get; init; }

        /// <summary>How far an even number of buttons spreads.</summary>
        public required Angle EvenArcSweep { get; init; }

        /// <summary>The least space between the button and its buttons.</summary>
        public required float ButtonClearance { get; init; }

        /// <summary>The space between neighbouring buttons on the arc, unless the button sets its own.</summary>
        public required float ButtonSpacing { get; init; }
    }
}
