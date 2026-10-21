using System;
using System.Globalization;
using XForms.Controls;

namespace XForms.Toolkit
{
    /// <summary>
    /// A month calendar: a header with the month and arrows to change it, the
    /// days of the week, and six weeks of days (weeks start on Monday). Days
    /// before the minimum date can't be chosen. With keyboard focus, the arrow
    /// keys move between days, Page Up and Page Down change the month, and Enter
    /// or Space chooses the day.
    /// </summary>
    public class CalendarView : UserControl
    {
        private const float Inset = 8.0f;
        private const float HeaderHeight = 40.0f;
        private const float WeekdayHeight = 28.0f;
        private const float CellSize = 40.0f;
        private const float DayFontSize = 14.0f;

        private enum Part
        {
            None,
            PreviousMonth,
            NextMonth,
            Day,
        }

        private DateTime _month;
        private DateTime? _selectedDate;
        private DateTime _minimumDate = DateTime.MinValue;
        private DateTime _keyboardDate;
        private Part _pressedPart;
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
        private DateTime FirstShownDay
        {
            get
            {
                int daysSinceMonday = ((int)this._month.DayOfWeek + 6) % 7;
                return this._month.AddDays(-daysSinceMonday);
            }
        }

        private bool IsEnabled(
            DateTime date)
        {
            return date >= this._minimumDate;
        }

        private bool CanShowPreviousMonth
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

        private Rect PreviousButtonBounds
        {
            get
            {
                float right = this.GridLeft + (7 * CellSize);
                return new Rect(right - (2 * CellSize), Inset, CellSize, HeaderHeight);
            }
        }

        private Rect NextButtonBounds
        {
            get
            {
                float right = this.GridLeft + (7 * CellSize);
                return new Rect(right - CellSize, Inset, CellSize, HeaderHeight);
            }
        }

        private Part PartAt(
            Point point,
            out DateTime date)
        {
            date = default;

            if (Contains(this.PreviousButtonBounds, point))
            {
                return Part.PreviousMonth;
            }

            if (Contains(this.NextButtonBounds, point))
            {
                return Part.NextMonth;
            }

            float x = point.X - this.GridLeft;
            float y = point.Y - this.GridTop;
            if ((x >= 0) && (y >= 0) && (x < 7 * CellSize) && (y < 6 * CellSize))
            {
                int column = (int)(x / CellSize);
                int row = (int)(y / CellSize);
                date = this.FirstShownDay.AddDays((row * 7) + column);
                return Part.Day;
            }

            return Part.None;
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
            e.Handled = (this._pressedPart != Part.None);
        }

        protected override void OnPointerReleased(
            PointerEventArgs e)
        {
            var part = this.PartAt(e.Position, out var date);
            var pressedPart = this._pressedPart;
            var pressedDate = this._pressedDate;
            this._pressedPart = Part.None;
            this.InvalidateVisual();
            e.Handled = true;

            if (part != pressedPart)
            {
                return;
            }

            switch (part)
            {
                case Part.PreviousMonth:
                    this.ShowMonth(-1);
                    break;

                case Part.NextMonth:
                    this.ShowMonth(1);
                    break;

                case Part.Day:
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
            this._pressedPart = Part.None;
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
            if (this.IsEnabled(this._keyboardDate))
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
            if (!this.IsEnabled(date))
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
            this._keyboardDate = this.IsEnabled(date) ? date : this._minimumDate;

            this.InvalidateVisual();
        }

        private void Choose(
            DateTime date)
        {
            if (!this.IsEnabled(date))
            {
                return;
            }

            this._selectedDate = date;
            this._keyboardDate = date;
            this._month = FirstOfMonth(date);
            this.InvalidateVisual();

            this.SelectedDateChanged?.Invoke(this, date);
        }

        protected override void Render(
            DrawingContext drawContext)
        {
            var bounds = new Rect(0, 0, this.Bounds.Width, this.Bounds.Height);
            var theme = this.ActualTheme;
            var culture = CultureInfo.CurrentCulture;
            float gridLeft = this.GridLeft;

            drawContext.Clear(Colors.Transparent);

            // Header: the month, and the arrows to change it
            drawContext.DrawText(
                this._month.ToString("MMMM yyyy", culture),
                new Rect(gridLeft + 8, Inset, (5 * CellSize) - 8, HeaderHeight),
                theme.Colors.Foreground,
                new TextFormat()
                {
                    FontSize = 16,
                    FontWeight = FontWeight.SemiBold,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Center,
                });

            this.DrawArrowButton(drawContext, this.PreviousButtonBounds, -1, this.CanShowPreviousMonth, Part.PreviousMonth);
            this.DrawArrowButton(drawContext, this.NextButtonBounds, 1, true, Part.NextMonth);

            // Days of the week, starting on Monday
            var weekdayFormat = new TextFormat()
            {
                FontSize = 12,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            };

            for (int column = 0; column < 7; column += 1)
            {
                var dayOfWeek = (DayOfWeek)((column + 1) % 7);
                drawContext.DrawText(
                    culture.DateTimeFormat.GetShortestDayName(dayOfWeek),
                    new Rect(gridLeft + (column * CellSize), Inset + HeaderHeight, CellSize, WeekdayHeight),
                    theme.Colors.SubtleForeground,
                    weekdayFormat);
            }

            // Days
            var dayFormat = new TextFormat()
            {
                FontSize = DayFontSize,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            };

            var today = DateTime.Today;
            var date = this.FirstShownDay;

            for (int row = 0; row < 6; row += 1)
            {
                for (int column = 0; column < 7; column += 1, date = date.AddDays(1))
                {
                    var cell = new Rect(gridLeft + (column * CellSize), this.GridTop + (row * CellSize), CellSize, CellSize);
                    var center = cell.Center;
                    const float Radius = (CellSize / 2) - 3;

                    bool isEnabled = this.IsEnabled(date);
                    bool isInMonth = date.Month == this._month.Month;
                    bool isSelected = this._selectedDate == date;

                    var textColor = isInMonth ? theme.Colors.Foreground : theme.Colors.SubtleForeground;
                    if (!isEnabled)
                    {
                        textColor = theme.Colors.SubtleForeground.MultiplyAlpha(0.45f);
                    }

                    if (isSelected)
                    {
                        drawContext.FillEllipse(center, Radius, Radius, theme.Colors.Accent);
                        textColor = theme.Colors.Background;
                    }
                    else if ((this._pressedPart == Part.Day) && (this._pressedDate == date) && isEnabled)
                    {
                        drawContext.FillEllipse(center, Radius, Radius, theme.Colors.Foreground.MultiplyAlpha(0.12f));
                    }

                    if (date == today)
                    {
                        drawContext.DrawEllipse(center, Radius, Radius, isSelected ? theme.Colors.Background : theme.Colors.Accent, 1.5f);
                    }

                    if (this.IsFocused && (date == this._keyboardDate))
                    {
                        drawContext.DrawEllipse(center, Radius + 1.5f, Radius + 1.5f, theme.Colors.Foreground, 2);
                    }

                    drawContext.DrawText(date.Day.ToString(culture), cell, textColor, dayFormat);
                }
            }
        }

        /// <summary>
        /// The month arrows, which point up and down as the months scroll in
        /// Windows' calendar.
        /// </summary>
        private void DrawArrowButton(
            DrawingContext drawContext,
            Rect bounds,
            int direction,
            bool isEnabled,
            Part part)
        {
            var theme = this.ActualTheme;
            var center = bounds.Center;

            if (isEnabled && (this._pressedPart == part))
            {
                drawContext.FillEllipse(center, 16, 16, theme.Colors.Foreground.MultiplyAlpha(0.12f));
            }

            var color = isEnabled ? theme.Colors.Foreground : theme.Colors.SubtleForeground.MultiplyAlpha(0.45f);
            float tip = center.Y + (direction * 2.5f);
            float tail = center.Y - (direction * 2.5f);
            drawContext.DrawLine(new Point(center.X - 5, tail), new Point(center.X, tip), color, 1.25f);
            drawContext.DrawLine(new Point(center.X, tip), new Point(center.X + 5, tail), color, 1.25f);
        }
    }
}
