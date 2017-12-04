using System;
using XForms.Controls;
using XamlCalendarView = global::Windows.UI.Xaml.Controls.CalendarView;

namespace XForms.Windows.Renderers
{
    public class CalendarControlRenderer : ControlRenderer, ICalendarControlRenderer
    {
        private XamlCalendarView _calendarView;

        public CalendarControlRenderer(
            CalendarControl calendarControl)
            : base(calendarControl)
        {
            this._calendarView = new XamlCalendarView()
            {
                SelectionMode = global::Windows.UI.Xaml.Controls.CalendarViewSelectionMode.Single,
                FirstDayOfWeek = global::Windows.Globalization.DayOfWeek.Monday,
            };

            this._calendarView.SelectedDatesChanged += CalendarView_SelectedDatesChanged;
            this.SetNativeElement(this._calendarView);

            var xamlApplicationRenderer = ((ApplicationRenderer)Application.Current.Renderer);
            xamlApplicationRenderer.ApplyThemeTo(this._calendarView);
        }

        public DateTime? SelectedDate
        {
            get
            {
                return this._calendarView.SelectedDates?[0].DateTime;
            }

            set
            {
                var selectedDates = this._calendarView.SelectedDates;
                selectedDates.Clear();
                if (value.HasValue)
                {
                    selectedDates.Add(new DateTimeOffset(value.Value));
                    this._calendarView.SetDisplayDate(value.Value);
                }
            }
        }

        public DateTime MinimumDate
        {
            get
            {
                return this._calendarView.MinDate.DateTime;
            }

            set
            {
                this._calendarView.MinDate = value;
            }
        }

        private void CalendarView_SelectedDatesChanged(
            XamlCalendarView sender,
            global::Windows.UI.Xaml.Controls.CalendarViewSelectedDatesChangedEventArgs args)
        {
            var selectedDates = args.AddedDates;
            if ((null != selectedDates) && (selectedDates.Count > 0))
            {
                var selectedDate = selectedDates[0];

                ((ICalendarControlDelegate)this.Element).NotifySelectedDateChanged(selectedDate);
            }
        }
    }
}
