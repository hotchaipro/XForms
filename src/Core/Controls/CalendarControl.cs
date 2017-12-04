using System;

namespace XForms.Controls
{
    public interface ICalendarControlDelegate
    {
        void NotifySelectedDateChanged(DateTimeOffset? date);
    }

    public interface ICalendarControlRenderer : IControlRenderer
    {
        DateTime? SelectedDate { get; set; }

        DateTime MinimumDate { get; set; }
    }

    public class CalendarControl : Control, ICalendarControlDelegate
    {
        public CalendarControl()
        {
        }

        public event EventHandler<DateTime?> SelectedDateChanged;

        public new ICalendarControlRenderer Renderer
        {
            get
            {
                return (ICalendarControlRenderer)base.Renderer;
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
            return this.Application.Platform.CreateCalendarControlRenderer(this);
        }

        void ICalendarControlDelegate.NotifySelectedDateChanged(
            DateTimeOffset? date)
        {
            this.SelectedDateChanged?.Invoke(this, date?.DateTime.Date);
        }
    }
}
