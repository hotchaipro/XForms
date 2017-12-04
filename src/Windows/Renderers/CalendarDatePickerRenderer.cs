using System;
using Windows.UI.Text;
using XForms.Controls;
using XamlCalendarDatePicker = global::Windows.UI.Xaml.Controls.CalendarDatePicker;

namespace XForms.Windows.Renderers
{
    public class CalendarDatePickerRenderer : ControlRenderer, ICalendarDatePickerRenderer
    {
        private XamlCalendarDatePicker _datePicker;

        public CalendarDatePickerRenderer(
            CalendarDatePicker datePicker)
            : base(datePicker)
        {
            this._datePicker = new XamlCalendarDatePicker()
            {
                DateFormat = global::Windows.Globalization.DateTimeFormatting.DateTimeFormatter.LongDate.Template,
                FirstDayOfWeek = global::Windows.Globalization.DayOfWeek.Monday,
                IsTabStop = true,
                FontWeight = FontWeights.SemiLight,
            };
            this._datePicker.Tapped += DatePicker_Tapped;
            this._datePicker.DateChanged += DatePicker_DateChanged;

            this.SetNativeElement(this._datePicker);
        }

        private void DatePicker_Tapped(
            object sender,
            global::Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            this._datePicker.Focus(global::Windows.UI.Xaml.FocusState.Pointer);
        }

        public DateTime? SelectedDate
        {
            get
            {
                return this._datePicker.Date?.DateTime;
            }

            set
            {
                this._datePicker.Date = value;
            }
        }

        public DateTime MinimumDate
        {
            get
            {
                return this._datePicker.MinDate.DateTime;
            }

            set
            {
                this._datePicker.MinDate = value;
            }
        }

        public override void OnApplyTheme(
            AppTheme theme)
        {
            base.OnApplyTheme(theme);

            //// HACK: Try to get a RequestedTheme change to take effect without restarting the app (failed)
            //var foregroundColor = (theme == AppTheme.Dark ? Colors.White.ToXamlColor() : Colors.Black.ToXamlColor());
            //global::Windows.UI.Xaml.Application.Current.Resources["CalendarViewCalendarItemForeground"] = foregroundColor;
            //global::Windows.UI.Xaml.Application.Current.Resources["CalendarViewOutOfScopeForeground"] = foregroundColor;
            //global::Windows.UI.Xaml.Application.Current.Resources["CalendarViewSelectedForeground"] = foregroundColor;
        }

        private void DatePicker_DateChanged(
            XamlCalendarDatePicker sender,
            global::Windows.UI.Xaml.Controls.CalendarDatePickerDateChangedEventArgs args)
        {
            if (args.NewDate == null)
            {
                // HACK: Re-select the unselected date, since the control does not have a setting 
                // to disable unselecting a date.
                this._datePicker.Date = args.OldDate;
                this._datePicker.IsCalendarOpen = false;
            }
            else
            {
                ((ICalendarDatePickerDelegate)this.Element).NotifySelectedDateChanged(args.NewDate);
            }

            this._datePicker.Focus(global::Windows.UI.Xaml.FocusState.Pointer);
        }
    }
}
