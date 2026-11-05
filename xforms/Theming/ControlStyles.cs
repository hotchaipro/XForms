namespace XForms.Controls
{
    // What each control lays out and hit-tests by. These declare only the
    // shape: the values are the theme package's. Every property is required,
    // so a style can't be half a look; to change part of one, copy the theme's
    // (theme.CheckBoxStyle with { BoxSize = 24 }).

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
}
