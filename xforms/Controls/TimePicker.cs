using System;
using System.Globalization;
using XForms.Primitives;

namespace XForms.Controls
{
    /// <summary>
    /// A field showing the selected time; clicking it opens a popup with a
    /// column each for the hour, the minute and (on a 12-hour clock) AM or PM,
    /// and buttons to accept or dismiss the change.
    /// </summary>
    public class TimePicker : PickerField
    {
        public static readonly ViewProperty<TimePicker, TimeSpan?> SelectedTimeProperty = new(nameof(SelectedTime), v => v.SelectedTime, (v, x) => v.SelectedTime = x);

        /// <summary>
        /// A <see cref="ClockIdentifier"/>: hours from 1 to 12, with AM and PM.
        /// </summary>
        public const string TwelveHourClock = "12HourClock";

        /// <summary>
        /// A <see cref="ClockIdentifier"/>: hours from 0 to 23.
        /// </summary>
        public const string TwentyFourHourClock = "24HourClock";

        private TimeSpan? _selectedTime;
        private string _clockIdentifier;
        private int _minuteIncrement = 1;
        private Popup _popup;
        private TimePickerView _view;

        public TimePicker()
        {
        }

        /// <summary>
        /// Raised when the user accepts a time.
        /// </summary>
        public event EventHandler<TimeSpan?> SelectedTimeChanged;

        // User state: the time chosen, when that changed since it was first
        // attached (null is a time too: none)
        private TimeSpan? _initialSelectedTime;

        protected override void OnAttaching(
            Theme theme)
        {
            base.OnAttaching(theme);
            this._initialSelectedTime = this.SelectedTime;
        }

        protected override object GetUserState()
        {
            return (this.SelectedTime != this._initialSelectedTime) ? new TimeState(this.SelectedTime) : null;
        }

        protected override void SetUserState(
            object state)
        {
            if (state is TimeState timeState)
            {
                this.SelectedTime = timeState.Time;
            }
        }

        private sealed record TimeState(TimeSpan? Time);

        /// <summary>
        /// The time of day chosen (hours and minutes), or null for none.
        /// </summary>
        public TimeSpan? SelectedTime
        {
            get
            {
                return this._selectedTime;
            }

            set
            {
                TimeSpan? time = value.HasValue ? new TimeSpan(value.Value.Hours, value.Value.Minutes, 0) : null;
                if (time == this._selectedTime)
                {
                    return;
                }

                this._selectedTime = time;
                this.InvalidateVisual();
                this.OnPropertyChanged();
                this.NotifyAccessibilityStateChanged();
            }
        }

        /// <summary>
        /// The clock the hours follow: <see cref="TwelveHourClock"/> or
        /// <see cref="TwentyFourHourClock"/>; by default, the culture's.
        /// </summary>
        public string ClockIdentifier
        {
            get
            {
                return this._clockIdentifier ?? (CultureInfo.CurrentCulture.DateTimeFormat.ShortTimePattern.Contains('H') ? TwentyFourHourClock : TwelveHourClock);
            }

            set
            {
                this._clockIdentifier = value;
                this.InvalidateMeasure();
                this.InvalidateVisual();
            }
        }

        /// <summary>
        /// Whether the hours run from 0 to 23.
        /// </summary>
        public bool Is24HourClock
        {
            get
            {
                return this.ClockIdentifier == TwentyFourHourClock;
            }
        }

        /// <summary>
        /// The steps the minutes can be chosen in.
        /// </summary>
        public int MinuteIncrement
        {
            get
            {
                return this._minuteIncrement;
            }

            set
            {
                this._minuteIncrement = Math.Clamp(value, 1, 30);
            }
        }

        /// <summary>
        /// Shown while no time is selected.
        /// </summary>
        public string PlaceholderText
        {
            get;
            set;
        } = "Select a time";

        /// <summary>
        /// The time as the field shows it.
        /// </summary>
        public string FormatTime(
            TimeSpan time)
        {
            var date = DateTime.Today.Add(time);
            var culture = CultureInfo.CurrentCulture;
            if (this._clockIdentifier == null)
            {
                return date.ToString("t", culture);
            }

            return date.ToString(this.Is24HourClock ? "HH:mm" : "h:mm tt", culture);
        }

        protected override string DefaultAccessibilityLabel
        {
            get
            {
                return this.DisplayText;
            }
        }

        public override string DisplayText
        {
            get
            {
                return this._selectedTime.HasValue ? this.FormatTime(this._selectedTime.Value) : this.PlaceholderText;
            }
        }

        public override bool IsPlaceholder
        {
            get
            {
                return !this._selectedTime.HasValue;
            }
        }

        protected override float MaximumTextWidth
        {
            get
            {
                var format = this.CreateTextFormat();
                return Math.Max(
                    TextLayout.Measure(this.FormatTime(new TimeSpan(22, 58, 0)), format).Width,
                    TextLayout.Measure(this.PlaceholderText, format).Width);
            }
        }

        protected override bool IsPickerOpen
        {
            get
            {
                return this._popup?.IsOpen ?? false;
            }
        }

        protected override void ClosePicker()
        {
            this._popup?.Close();
        }

        protected override void OpenPicker()
        {
            if (!this.IsEnabled)
            {
                return;
            }

            if (null == this._popup)
            {
                this._view = new TimePickerView(this);
                this._popup = new Popup()
                {
                    PlacementTarget = this,
                    Content = new TimeFrame(this._view),
                };
                this._popup.Closed += (sender, e) => this.NotifyAccessibilityStateChanged();
            }

            if (this._popup.IsOpen)
            {
                return;
            }

            // The time shown, or now
            var now = DateTime.Now;
            this._view.Reset(this._selectedTime ?? new TimeSpan(now.Hour, now.Minute, 0));

            bool hasKeyboardFocus = this.IsFocused;
            this._popup.Show();
            this.NotifyAccessibilityStateChanged();

            // From the keyboard, the arrow keys change the columns
            if (hasKeyboardFocus)
            {
                this._view.Focus();
            }
        }

        internal void Accept(
            TimeSpan time)
        {
            this.Dismiss();
            this.SelectedTime = time;
            this.SelectedTimeChanged?.Invoke(this, this._selectedTime);
        }

        // Closes the popup; keyboard focus in it goes back to the field
        internal void Dismiss()
        {
            bool hadFocus = this._view?.IsFocused ?? false;
            this._popup?.Close();
            if (hadFocus)
            {
                this.Focus();
            }
        }

        /// <summary>
        /// The popup's frame around the columns.
        /// </summary>
        private sealed class TimeFrame : PopupFrame
        {
            public TimeFrame(
                TimePickerView view)
            {
                view.Margin = new Thickness(BorderThickness);
                this.Content = view;
            }
        }
    }

    /// <summary>
    /// The part of a time picker's popup being pressed.
    /// </summary>
    public enum TimePickerViewPart
    {
        None,
        Column,
        Accept,
        Dismiss,
    }

    /// <summary>
    /// A time picker's popup: a column each for the hour, the minute and (on
    /// a 12-hour clock) AM or PM, each showing its value in the middle row
    /// with its neighbors above and below, and Accept and Dismiss buttons
    /// beneath. Tapping a neighbor (or the wheel) moves a column to it. With
    /// keyboard focus, Up and Down change the focused column, Left and Right
    /// move between columns, Enter accepts and Escape dismisses. Its theme's
    /// template draws it, in the layout it measures.
    /// </summary>
    public sealed class TimePickerView : TemplatedControl
    {
        /// <summary>
        /// The height of a row in a column.
        /// </summary>
        public const float RowHeight = 40.0f;

        /// <summary>
        /// The rows shown in a column: the value in the middle, and two above
        /// and below.
        /// </summary>
        public const int VisibleRows = 5;

        private const float ColumnWidth = 72.0f;
        private const float ButtonHeight = 44.0f;

        private readonly TimePicker _picker;
        private int _hour;
        private int _minute;
        private int _focusedColumn;
        private TimePickerViewPart _pressedPart;
        private int _pressedColumn = -1;
        private int _pressedRow = -1;

        internal TimePickerView(
            TimePicker picker)
        {
            this._picker = picker;
            this.Focusable = true;
            this.AccessibilityRole = AccessibilityRole.Group;
        }

        public TimePicker TimePicker
        {
            get
            {
                return this._picker;
            }
        }

        /// <summary>
        /// The number of columns: hour, minute, and AM or PM on a 12-hour
        /// clock.
        /// </summary>
        public int ColumnCount
        {
            get
            {
                return this._picker.Is24HourClock ? 2 : 3;
            }
        }

        /// <summary>
        /// The column the keyboard is on.
        /// </summary>
        public int FocusedColumn
        {
            get
            {
                return this._focusedColumn;
            }
        }

        public TimePickerViewPart PressedPart
        {
            get
            {
                return this._pressedPart;
            }
        }

        /// <summary>
        /// The column and row being pressed, when <see cref="PressedPart"/> is
        /// a column.
        /// </summary>
        public int PressedColumn
        {
            get
            {
                return this._pressedColumn;
            }
        }

        public int PressedRow
        {
            get
            {
                return this._pressedRow;
            }
        }

        /// <summary>
        /// The time shown.
        /// </summary>
        public TimeSpan Time
        {
            get
            {
                return new TimeSpan(this._hour, this._minute, 0);
            }
        }

        public override string AccessibilityValue
        {
            get
            {
                return this._picker.FormatTime(this.Time);
            }
        }

        internal void Reset(
            TimeSpan time)
        {
            int increment = this._picker.MinuteIncrement;
            this._hour = time.Hours;
            this._minute = (time.Minutes / increment) * increment;
            this._focusedColumn = 0;
            this._pressedPart = TimePickerViewPart.None;
            this.InvalidateMeasure();
            this.InvalidateVisual();
        }

        protected override Size MeasureOverride(
            Size availableSize)
        {
            return new Size(
                Math.Min(this.ColumnCount * ColumnWidth, availableSize.Width),
                Math.Min((VisibleRows * RowHeight) + ButtonHeight, availableSize.Height));
        }

        private bool IsRightToLeft
        {
            get
            {
                return this.FlowDirection == FlowDirection.RightToLeft;
            }
        }

        private float ColumnStep
        {
            get
            {
                return this.Bounds.Width / this.ColumnCount;
            }
        }

        /// <summary>
        /// Where a column is (the first is the hour's, at the start).
        /// </summary>
        public Rect GetColumnBounds(
            int column)
        {
            float step = this.ColumnStep;
            int place = this.IsRightToLeft ? this.ColumnCount - 1 - column : column;
            return new Rect(place * step, 0, step, VisibleRows * RowHeight);
        }

        /// <summary>
        /// Where a row of a column is (row 2 is the middle, the value).
        /// </summary>
        public Rect GetRowBounds(
            int column,
            int row)
        {
            var bounds = this.GetColumnBounds(column);
            return new Rect(bounds.X, row * RowHeight, bounds.Width, RowHeight);
        }

        public Rect AcceptBounds
        {
            get
            {
                float half = this.Bounds.Width / 2;
                return new Rect(this.IsRightToLeft ? half : 0, VisibleRows * RowHeight, half, ButtonHeight);
            }
        }

        public Rect DismissBounds
        {
            get
            {
                float half = this.Bounds.Width / 2;
                return new Rect(this.IsRightToLeft ? 0 : half, VisibleRows * RowHeight, half, ButtonHeight);
            }
        }

        /// <summary>
        /// The text of a column's value moved by the offset: the hour, the
        /// minute, or AM or PM.
        /// </summary>
        public string GetText(
            int column,
            int offset)
        {
            var culture = CultureInfo.CurrentCulture;
            switch (column)
            {
                case 0:
                    int hour = Wrap(this._hour + offset, 24);
                    if (this._picker.Is24HourClock)
                    {
                        return hour.ToString("00", culture);
                    }

                    int twelve = hour % 12;
                    return ((twelve == 0) ? 12 : twelve).ToString(culture);

                case 1:
                    int increment = this._picker.MinuteIncrement;
                    return Wrap(this._minute + (offset * increment), 60 - (60 % increment)).ToString("00", culture);

                default:
                    // Only AM and PM: no neighbors beyond them
                    int period = (this._hour >= 12 ? 1 : 0) + offset;
                    if ((period < 0) || (period > 1))
                    {
                        return string.Empty;
                    }

                    return (period == 0) ? culture.DateTimeFormat.AMDesignator : culture.DateTimeFormat.PMDesignator;
            }
        }

        private static int Wrap(
            int value,
            int count)
        {
            return ((value % count) + count) % count;
        }

        // Moves a column's value by the steps
        private void Move(
            int column,
            int steps)
        {
            switch (column)
            {
                case 0:
                    this._hour = Wrap(this._hour + steps, 24);
                    break;

                case 1:
                    int increment = this._picker.MinuteIncrement;
                    this._minute = Wrap(this._minute + (steps * increment), 60 - (60 % increment));
                    break;

                default:
                    bool isPm = this._hour >= 12;
                    if ((steps > 0) && !isPm)
                    {
                        this._hour += 12;
                    }
                    else if ((steps < 0) && isPm)
                    {
                        this._hour -= 12;
                    }

                    break;
            }

            this.InvalidateVisual();
            this.NotifyAccessibilityStateChanged();
        }

        private TimePickerViewPart PartAt(
            Point point,
            out int column,
            out int row)
        {
            column = -1;
            row = -1;
            if (Contains(this.AcceptBounds, point))
            {
                return TimePickerViewPart.Accept;
            }

            if (Contains(this.DismissBounds, point))
            {
                return TimePickerViewPart.Dismiss;
            }

            for (int i = 0; i < this.ColumnCount; i += 1)
            {
                var bounds = this.GetColumnBounds(i);
                if (Contains(bounds, point))
                {
                    column = i;
                    row = Math.Clamp((int)(point.Y / RowHeight), 0, VisibleRows - 1);
                    return TimePickerViewPart.Column;
                }
            }

            return TimePickerViewPart.None;
        }

        private static bool Contains(
            Rect rectangle,
            Point point)
        {
            return (point.X >= rectangle.X) && (point.Y >= rectangle.Y) && (point.X < rectangle.Right) && (point.Y < rectangle.Bottom);
        }

        protected override void OnPointerPressed(
            PointerEventArgs e)
        {
            this._pressedPart = this.PartAt(e.Position, out this._pressedColumn, out this._pressedRow);
            this.InvalidateVisual();
            e.Handled = (this._pressedPart != TimePickerViewPart.None);
        }

        protected override void OnPointerReleased(
            PointerEventArgs e)
        {
            var part = this.PartAt(e.Position, out int column, out int row);
            bool isSame = (part == this._pressedPart) && (column == this._pressedColumn) && (row == this._pressedRow);
            this._pressedPart = TimePickerViewPart.None;
            this._pressedColumn = -1;
            this._pressedRow = -1;
            this.InvalidateVisual();
            e.Handled = true;

            if (!isSame)
            {
                return;
            }

            switch (part)
            {
                case TimePickerViewPart.Accept:
                    this._picker.Accept(this.Time);
                    break;

                case TimePickerViewPart.Dismiss:
                    this._picker.Dismiss();
                    break;

                case TimePickerViewPart.Column:
                    this._focusedColumn = column;
                    this.Move(column, row - (VisibleRows / 2));
                    break;
            }
        }

        protected override void OnPointerCanceled(
            PointerEventArgs e)
        {
            this._pressedPart = TimePickerViewPart.None;
            this.InvalidateVisual();
            e.Handled = true;
        }

        protected override void OnPointerWheelChanged(
            PointerEventArgs e)
        {
            if (this.PartAt(e.Position, out int column, out _) == TimePickerViewPart.Column)
            {
                this.Move(column, (e.WheelDelta > 0) ? -1 : 1);
                e.Handled = true;
            }
        }

        protected override void OnKeyDown(
            KeyEventArgs keyInputEvent)
        {
            int forward = this.IsRightToLeft ? -1 : 1;
            switch (keyInputEvent.Key)
            {
                case Key.Up:
                    this.Move(this._focusedColumn, -1);
                    break;

                case Key.Down:
                    this.Move(this._focusedColumn, 1);
                    break;

                case Key.Left:
                    this._focusedColumn = Math.Clamp(this._focusedColumn - forward, 0, this.ColumnCount - 1);
                    this.InvalidateVisual();
                    break;

                case Key.Right:
                    this._focusedColumn = Math.Clamp(this._focusedColumn + forward, 0, this.ColumnCount - 1);
                    this.InvalidateVisual();
                    break;

                case Key.Enter:
                case Key.Space:
                    this._picker.Accept(this.Time);
                    break;

                case Key.Escape:
                    this._picker.Dismiss();
                    break;

                default:
                    return;
            }

            keyInputEvent.Handled = true;
        }

        protected override void OnActivate()
        {
            this._picker.Accept(this.Time);
        }
    }
}
