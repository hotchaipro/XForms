using System;
using Android.Icu.Util;
using AndroidCalendarView = global::Android.Widget.CalendarView;
using XForms.Controls;

namespace XForms.Android.Renderers
{
    public class CalendarControlRenderer : ControlRenderer<AndroidCalendarView>, ICalendarControlRenderer
    {
        private AndroidCalendarView _nativeCalendarView;

        public CalendarControlRenderer(
            global::Android.Content.Context context,
            CalendarControl calendarControl)
            : base(context, calendarControl)
        {
            this._nativeCalendarView = new AndroidCalendarView(context)
            {
                FirstDayOfWeek = Calendar.Monday,
            };

            this._nativeCalendarView.DateChange += NativeCalendarView_DateChange;

            this.SetNativeElement(this._nativeCalendarView);
        }

        public DateTime? SelectedDate
        {
            get
            {
                return NativeConversions.FromAndroidDateUtc(this._nativeCalendarView.Date);
            }

            set
            {
                if (value.HasValue)
                {
                    this._nativeCalendarView.Date = NativeConversions.ToAndroidDateUtc(value.Value);
                }
                else
                {
                    // TODO: this._nativeCalendarView.ClearSelection();
                }
            }
        }

        public DateTime MinimumDate
        {
            get
            {
                return NativeConversions.FromAndroidDateUtc(this._nativeCalendarView.MinDate);
            }

            set
            {
                this._nativeCalendarView.MinDate = NativeConversions.ToAndroidDateUtc(value);
            }
        }

        private void NativeCalendarView_DateChange(
            object sender,
            AndroidCalendarView.DateChangeEventArgs e)
        {
            var selectedDate = new DateTime(e.Year, e.Month, e.DayOfMonth, 0, 0, 0, DateTimeKind.Utc);
            ((ICalendarControlDelegate)this.Element).NotifySelectedDateChanged(selectedDate);
        }
    }
}
