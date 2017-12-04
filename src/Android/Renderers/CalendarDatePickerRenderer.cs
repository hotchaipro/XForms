using System;
using Android.App;
using Android.Icu.Util;
using Android.Widget;
using AndroidDateView = global::Android.Widget.EditText;
using AndroidDatePickerDialog = global::Android.App.DatePickerDialog;
using XForms.Controls;

namespace XForms.Android.Renderers
{
    public class CalendarDatePickerRenderer : ControlRenderer<AndroidDateView>, ICalendarDatePickerRenderer
    {
        private DateTime? _selectedDate;
        private AndroidDateView _nativeDateView;
        private AndroidDatePickerDialog _nativeDatePickerDialog;

        public CalendarDatePickerRenderer(
            global::Android.Content.Context context,
            CalendarDatePicker datePicker)
            : base(context, datePicker)
        {
            this._nativeDateView = new AndroidDateView(context)
            {
                Focusable = false,
                FocusableInTouchMode = false,
                Clickable = true,
                LongClickable = false,
            };
            this._nativeDateView.SetCursorVisible(false);
#if DEBUG_LAYOUT
            this._nativeDateView.SetTextColor(global::Android.Graphics.Color.DarkGreen);
#endif
            this._nativeDateView.Click += NativeDateView_Click;

            DateTime selectedDate = DateTime.Now.Date;

            this._nativeDatePickerDialog = new AndroidDatePickerDialog(
                context, new DatePickerCallback(this), selectedDate.Year, selectedDate.Month, selectedDate.Day);
            {
            };
            this._nativeDatePickerDialog.DatePicker.FirstDayOfWeek = Calendar.Monday;

            this.SetNativeElement(this._nativeDateView);
        }

        public DateTime? SelectedDate
        {
            get
            {
                return this._selectedDate;
            }

            set
            {
                this._selectedDate = value;

                if (value.HasValue)
                {
                    this._nativeDateView.Text = value.Value.ToLongDateString();
                }
                else
                {
                    this._nativeDateView.Text = String.Empty;
                }
            }
        }

        public DateTime MinimumDate
        {
            get;
            set;
        }


        private void NativeDateView_Click(
            object sender,
            EventArgs e)
        {
            this._nativeDatePickerDialog.DatePicker.MinDate = NativeConversions.ToAndroidDateUtc(this.MinimumDate);
            if (this.SelectedDate.HasValue)
            {
                DateTime selectedDate = this.SelectedDate.Value;
                this._nativeDatePickerDialog.DatePicker.UpdateDate(selectedDate.Year, selectedDate.Month, selectedDate.Day);
                this._nativeDatePickerDialog.Show();
            }
        }

        private void OnDateSet(
            DateTime date)
        {
            this.SelectedDate = date;
            ((ICalendarDatePickerDelegate)this.Element).NotifySelectedDateChanged(date);
        }

        private sealed class DatePickerCallback : Java.Lang.Object, AndroidDatePickerDialog.IOnDateSetListener
        {
            private CalendarDatePickerRenderer _renderer;

            internal DatePickerCallback(
                CalendarDatePickerRenderer renderer)
            {
                if (null == renderer)
                {
                    throw new ArgumentNullException(nameof(renderer));
                }

                this._renderer = renderer;
            }

            public void OnDateSet(
                DatePicker view,
                int year,
                int month,
                int dayOfMonth)
            {
                var date = new DateTime(year, month, dayOfMonth);
                this._renderer.OnDateSet(date);
            }
        }
    }
}
