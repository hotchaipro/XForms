using System;

namespace XForms.Controls
{
    public interface ICalendarDatePickerDelegate
    {
        void NotifySelectedDateChanged(DateTimeOffset? date);
    }

    public interface ICalendarDatePickerRenderer : IControlRenderer
    {
        DateTime? SelectedDate { get; set; }

        DateTime MinimumDate { get; set; }
    }

    public class CalendarDatePicker : Control, ICalendarDatePickerDelegate
    {
        public CalendarDatePicker()
        {
        }

        public event EventHandler<DateTime?> SelectedDateChanged;

        public new ICalendarDatePickerRenderer Renderer
        {
            get
            {
                return (ICalendarDatePickerRenderer)base.Renderer;
            }
        }

        public DateTime? SelectedDate
        {
            get
            {
                return this.Renderer.SelectedDate;
            }

            set
            {
                this.Renderer.SelectedDate = value;
            }
        }

        public DateTime MinimumDate
        {
            get
            {
                return this.Renderer.MinimumDate;
            }

            set
            {
                this.Renderer.MinimumDate = value;
            }
        }

        protected override IElementRenderer CreateRenderer()
        {
            return this.Application.Platform.CreateCalendarDatePickerRenderer(this);
        }

        void ICalendarDatePickerDelegate.NotifySelectedDateChanged(
            DateTimeOffset? date)
        {
            this.SelectedDateChanged?.Invoke(this, date?.DateTime);
        }
    }
}
