using System;
using System.Globalization;
using XForms.Controls;
using XForms.Primitives;

namespace XForms.Controls
{
    /// <summary>
    /// A month calendar: a header with the month and arrows to change it, the
    /// days of the week, and six weeks of days (weeks start on Monday). Days
    /// before the minimum date can't be chosen. With keyboard focus, the arrow
    /// keys move between days, Page Up and Page Down change the month, and Enter
    /// or Space chooses the day.
    /// </summary>
    public class CalendarView : TemplatedControl
    {
        private const float Inset = 8.0f;
        private const float HeaderHeight = 40.0f;
        private const float WeekdayHeight = 28.0f;

        /// <summary>
        /// The size of a day's cell.
        /// </summary>
        public const float CellSize = 40.0f;

        private DateTime _month;
        private DateTime? _selectedDate;
        private DateTime _minimumDate = DateTime.MinValue;
        private DateTime _keyboardDate;
        private CalendarViewPart _pressedPart;
        private DateTime _pressedDate;

        public CalendarView()
        {
            this._month = FirstOfMonth(DateTime.Today);
            this._keyboardDate = DateTime.Today;
            this.Focusable = true;
        }

        /// <summary>
        /// Raised when the user chooses a day.
        /// </summary>
        public event EventHandler<DateTime?> SelectedDateChanged;

        public DateTime? SelectedDate
        {
            get
            {
                return this._selectedDate;
            }

            set
            {
                this._selectedDate = value?.Date;

                // Show the selected day's month
                var shown = value?.Date ?? DateTime.Today;
                this._month = FirstOfMonth(shown);
                this._keyboardDate = shown;
                this.InvalidateVisual();
            }
        }

        public DateTime MinimumDate
        {
            get
            {
                return this._minimumDate;
            }

            set
            {
                this._minimumDate = value.Date;
                if (this._keyboardDate < this._minimumDate)
                {
                    this._keyboardDate = this._minimumDate;
                }

                this.InvalidateVisual();
            }
        }

        protected override AccessibilityRole DefaultAccessibilityRole
        {
            get
            {
                return AccessibilityRole.Group;
            }
        }

        protected override string DefaultAccessibilityLabel
        {
            get
            {
                return this._selectedDate?.ToLongDateString();
            }
        }

        /// <summary>
        /// The calendar's size.
        /// </summary>
        public static Size NaturalSize
        {
            get
            {
                return new Size(
                    (7 * CellSize) + (2 * Inset),
                    HeaderHeight + WeekdayHeight + (6 * CellSize) + (2 * Inset));
            }
        }

        protected override Size MeasureOverride(
            Size availableSize)
        {
            var size = NaturalSize;
            return new Size(Math.Min(size.Width, availableSize.Width), Math.Min(size.Height, availableSize.Height));
        }

        private static DateTime FirstOfMonth(
            DateTime date)
        {
            return new DateTime(date.Year, date.Month, 1);
        }

        /// <summary>
        /// The first day shown: the Monday on or before the first of the month.
        /// </summary>
        public DateTime FirstShownDay
        {
            get
            {
                int daysSinceMonday = ((int)this._month.DayOfWeek + 6) % 7;
                return this._month.AddDays(-daysSinceMonday);
            }
        }

        /// <summary>
        /// Whether the day can be chosen (it's on or after the minimum date).
        /// </summary>
        public bool IsDateEnabled(
            DateTime date)
        {
            return date >= this._minimumDate;
        }

        /// <summary>
        /// Whether there's an earlier month to show.
        /// </summary>
        public bool CanShowPreviousMonth
        {
            get
            {
                return this._month > FirstOfMonth(this._minimumDate);
            }
        }

        /// <summary>
        /// The grid's left edge, centering it when the view is wider than needed.
        /// </summary>
        private float GridLeft
        {
            get
            {
                return Math.Max(Inset, (this.Bounds.Width - (7 * CellSize)) / 2);
            }
        }

        private float GridTop
        {
            get
            {
                return Inset + HeaderHeight + WeekdayHeight;
            }
        }

        /// <summary>
        /// Where the button for the previous month is.
        /// </summary>
        public Rect PreviousButtonBounds
        {
            get
            {
                float right = this.GridLeft + (7 * CellSize);
                return new Rect(right - (2 * CellSize), Inset, CellSize, HeaderHeight);
            }
        }

        /// <summary>
        /// Where the button for the next month is.
        /// </summary>
        public Rect NextButtonBounds
        {
            get
            {
                float right = this.GridLeft + (7 * CellSize);
                return new Rect(right - CellSize, Inset, CellSize, HeaderHeight);
            }
        }

        /// <summary>
        /// Where the month's name is, in the header.
        /// </summary>
        public Rect HeaderBounds
        {
            get
            {
                return new Rect(this.GridLeft + 8, Inset, (5 * CellSize) - 8, HeaderHeight);
            }
        }

        /// <summary>
        /// Where the name of the day of the week in the column (from 0,
        /// Monday) is.
        /// </summary>
        public Rect GetWeekdayBounds(
            int column)
        {
            return new Rect(this.GridLeft + (column * CellSize), Inset + HeaderHeight, CellSize, WeekdayHeight);
        }

        /// <summary>
        /// Where the day in the row and column (from 0) is: the days start
        /// with <see cref="FirstShownDay"/>.
        /// </summary>
        public Rect GetDayBounds(
            int row,
            int column)
        {
            return new Rect(this.GridLeft + (column * CellSize), this.GridTop + (row * CellSize), CellSize, CellSize);
        }

        /// <summary>
        /// The month shown (its first day).
        /// </summary>
        public DateTime DisplayedMonth
        {
            get
            {
                return this._month;
            }
        }

        /// <summary>
        /// The day the keyboard is on.
        /// </summary>
        public DateTime FocusedDate
        {
            get
            {
                return this._keyboardDate;
            }
        }

        /// <summary>
        /// The part of the calendar being pressed.
        /// </summary>
        public CalendarViewPart PressedPart
        {
            get
            {
                return this._pressedPart;
            }
        }

        /// <summary>
        /// The day being pressed, when <see cref="PressedPart"/> is a day.
        /// </summary>
        public DateTime PressedDate
        {
            get
            {
                return this._pressedDate;
            }
        }

        private CalendarViewPart PartAt(
            Point point,
            out DateTime date)
        {
            date = default;

            if (Contains(this.PreviousButtonBounds, point))
            {
                return CalendarViewPart.PreviousMonth;
            }

            if (Contains(this.NextButtonBounds, point))
            {
                return CalendarViewPart.NextMonth;
            }

            float x = point.X - this.GridLeft;
            float y = point.Y - this.GridTop;
            if ((x >= 0) && (y >= 0) && (x < 7 * CellSize) && (y < 6 * CellSize))
            {
                int column = (int)(x / CellSize);
                int row = (int)(y / CellSize);
                date = this.FirstShownDay.AddDays((row * 7) + column);
                return CalendarViewPart.Day;
            }

            return CalendarViewPart.None;
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
            this._pressedPart = this.PartAt(e.Position, out this._pressedDate);
            this.InvalidateVisual();
            e.Handled = (this._pressedPart != CalendarViewPart.None);
        }

        protected override void OnPointerReleased(
            PointerEventArgs e)
        {
            var part = this.PartAt(e.Position, out var date);
            var pressedPart = this._pressedPart;
            var pressedDate = this._pressedDate;
            this._pressedPart = CalendarViewPart.None;
            this.InvalidateVisual();
            e.Handled = true;

            if (part != pressedPart)
            {
                return;
            }

            switch (part)
            {
                case CalendarViewPart.PreviousMonth:
                    this.ShowMonth(-1);
                    break;

                case CalendarViewPart.NextMonth:
                    this.ShowMonth(1);
                    break;

                case CalendarViewPart.Day:
                    if (date == pressedDate)
                    {
                        this.Choose(date);
                    }
                    break;
            }
        }

        protected override void OnPointerCanceled(
            PointerEventArgs e)
        {
            this._pressedPart = CalendarViewPart.None;
            this.InvalidateVisual();
            e.Handled = true;
        }

        protected override void OnKeyDown(
            KeyEventArgs keyInputEvent)
        {
            switch (keyInputEvent.Key)
            {
                case Key.Left:
                    this.MoveKeyboardDate(this._keyboardDate.AddDays(-1));
                    keyInputEvent.Handled = true;
                    return;

                case Key.Right:
                    this.MoveKeyboardDate(this._keyboardDate.AddDays(1));
                    keyInputEvent.Handled = true;
                    return;

                case Key.Up:
                    this.MoveKeyboardDate(this._keyboardDate.AddDays(-7));
                    keyInputEvent.Handled = true;
                    return;

                case Key.Down:
                    this.MoveKeyboardDate(this._keyboardDate.AddDays(7));
                    keyInputEvent.Handled = true;
                    return;

                case Key.PageUp:
                    this.MoveKeyboardDate(this._keyboardDate.AddMonths(-1));
                    keyInputEvent.Handled = true;
                    return;

                case Key.PageDown:
                    this.MoveKeyboardDate(this._keyboardDate.AddMonths(1));
                    keyInputEvent.Handled = true;
                    return;

                case Key.Enter:
                case Key.Space:
                    this.OnActivate();
                    keyInputEvent.Handled = true;
                    return;
            }

            return;
        }

        protected override void OnActivate()
        {
            if (this.IsDateEnabled(this._keyboardDate))
            {
                this.Choose(this._keyboardDate);
            }
        }

        protected override void OnGotFocus()
        {
            this.OnFocusChanged();
        }

        protected override void OnLostFocus()
        {
            this.OnFocusChanged();
        }

        private void OnFocusChanged()
        {
            this.InvalidateVisual();
        }

        protected override void OnThemeChanged(
            Theme theme)
        {
            base.OnThemeChanged(theme);

            this.InvalidateVisual();
        }

        private void MoveKeyboardDate(
            DateTime date)
        {
            if (!this.IsDateEnabled(date))
            {
                return;
            }

            this._keyboardDate = date;
            this._month = FirstOfMonth(date);
            this.InvalidateVisual();
        }

        private void ShowMonth(
            int delta)
        {
            if ((delta < 0) && !this.CanShowPreviousMonth)
            {
                return;
            }

            this._month = this._month.AddMonths(delta);

            // Keep the keyboard day in the shown month
            int day = Math.Min(this._keyboardDate.Day, DateTime.DaysInMonth(this._month.Year, this._month.Month));
            var date = new DateTime(this._month.Year, this._month.Month, day);
            this._keyboardDate = this.IsDateEnabled(date) ? date : this._minimumDate;

            this.InvalidateVisual();
        }

        private void Choose(
            DateTime date)
        {
            if (!this.IsDateEnabled(date))
            {
                return;
            }

            this._selectedDate = date;
            this._keyboardDate = date;
            this._month = FirstOfMonth(date);
            this.InvalidateVisual();

            this.SelectedDateChanged?.Invoke(this, date);
        }
    }

    /// <summary>
    /// A part of a <see cref="CalendarView"/> that can be pressed.
    /// </summary>
    public enum CalendarViewPart
    {
        None,
        PreviousMonth,
        NextMonth,
        Day,
    }
}
