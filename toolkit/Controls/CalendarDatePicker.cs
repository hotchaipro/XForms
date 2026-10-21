using System;
using System.Globalization;
using XForms.Controls;
using XForms.Primitives;

namespace XForms.Toolkit
{
    /// <summary>
    /// A field showing the selected date; clicking it opens a calendar below it.
    /// </summary>
    public class CalendarDatePicker : PickerField
    {
        public static readonly ViewProperty<CalendarDatePicker, DateTime?> SelectedDateProperty = new(nameof(SelectedDate), v => v.SelectedDate, (v, x) => v.SelectedDate = x);

        private DateTime? _selectedDate;
        private Popup _popup;
        private CalendarView _calendar;

        public CalendarDatePicker()
        {
        }

        public event EventHandler<DateTime?> SelectedDateChanged;

        public DateTime? SelectedDate
        {
            get
            {
                return this._selectedDate;
            }

            set
            {
                var date = value?.Date;
                if (date == this._selectedDate)
                {
                    return;
                }

                this._selectedDate = date;
                this.InvalidateVisual();
                this.OnPropertyChanged();
                this.NotifyAccessibilityStateChanged();
            }
        }

        public DateTime MinimumDate
        {
            get;
            set;
        }

        /// <summary>
        /// Shown while no date is selected.
        /// </summary>
        public string PlaceholderText
        {
            get;
            set;
        } = "Select a date";

        protected override string DefaultAccessibilityLabel
        {
            get
            {
                return this._selectedDate?.ToLongDateString() ?? this.PlaceholderText;
            }
        }

        protected override string DisplayText
        {
            get
            {
                return this._selectedDate?.ToString("D", CultureInfo.CurrentCulture) ?? this.PlaceholderText;
            }
        }

        protected override bool IsPlaceholder
        {
            get
            {
                return !this._selectedDate.HasValue;
            }
        }

        protected override float MaximumTextWidth
        {
            get
            {
                // The longest dates are in September, on a Wednesday
                var longDate = new DateTime(2025, 9, 24).ToString("D", CultureInfo.CurrentCulture);
                var format = this.CreateTextFormat();
                return Math.Max(
                    TextLayout.Measure(longDate, format).Width,
                    TextLayout.Measure(this.PlaceholderText, format).Width);
            }
        }

        /// <summary>
        /// A calendar page: a box with a band across the top and two rings.
        /// </summary>
        protected override void DrawGlyph(
            DrawingContext drawContext,
            Rect bounds,
            Color color)
        {
            var center = bounds.Center;
            var page = new Rect(center.X - 7, center.Y - 5.5f, 14, 12);
            drawContext.DrawRectangle(page, color, 1.0f);
            drawContext.DrawLine(new Point(page.X, page.Y + 3.5f), new Point(page.Right, page.Y + 3.5f), color, 1.0f);
            drawContext.DrawLine(new Point(page.X + 3.5f, page.Y - 2), new Point(page.X + 3.5f, page.Y + 1), color, 1.0f);
            drawContext.DrawLine(new Point(page.Right - 3.5f, page.Y - 2), new Point(page.Right - 3.5f, page.Y + 1), color, 1.0f);
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
            if (null == this._popup)
            {
                this._calendar = new CalendarView();
                this._calendar.SelectedDateChanged += this.Calendar_SelectedDateChanged;

                this._popup = new Popup()
                {
                    PlacementTarget = this,
                    Content = new CalendarFrame(this._calendar),
                };
                this._popup.Closed += (sender, e) => this.NotifyAccessibilityStateChanged();
            }

            if (this._popup.IsOpen)
            {
                return;
            }

            this._calendar.MinimumDate = this.MinimumDate;
            this._calendar.SelectedDate = this._selectedDate;

            bool hasKeyboardFocus = this.IsFocused;
            this._popup.Show();
            this.NotifyAccessibilityStateChanged();

            // From the keyboard, the arrow keys move through the days
            if (hasKeyboardFocus)
            {
                this._calendar.Focus();
            }
        }

        private void Calendar_SelectedDateChanged(
            object sender,
            DateTime? date)
        {
            this._popup.Close();

            this.SelectedDate = date;
            this.SelectedDateChanged?.Invoke(this, date);
        }

        /// <summary>
        /// The popup's frame around the calendar.
        /// </summary>
        private sealed class CalendarFrame : UserControl
        {
            public CalendarFrame(
                CalendarView calendar)
            {
                calendar.Margin = new Thickness(PopupBorderThickness);
                this.Content = calendar;
            }

            protected override void Render(
                DrawingContext drawContext)
            {
                var bounds = new Rect(0, 0, this.Bounds.Width, this.Bounds.Height);
                drawContext.Clear(Colors.Transparent);
                DrawPopupFrame(drawContext, bounds, this.ActualTheme);
            }
        }
    }
}
