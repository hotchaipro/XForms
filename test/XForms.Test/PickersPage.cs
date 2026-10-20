using System;
using XForms.Controls;
using XForms.Toolkit;
using XForms.Layouts;

namespace XForms.Test
{
    /// <summary>
    /// Pickers, a calendar, a popover, a progress ring and an image.
    /// </summary>
    public class PickersPage : Page
    {
        private readonly TextView _status;

        public PickersPage()
        {
            var layout = new DockLayout()
            {
                Margin = new Thickness(20),
            };

            this._status = new TextView()
            {
                Text = "Nothing chosen yet",
                HorizontalAlignment = LayoutAlignment.Start,
                Margin = new Thickness(0, 0, 0, 12),
            };
            layout.Children.Add(this._status, DockRegion.Top);

            layout.Children.Add(
                new TextEntry()
                {
                    PlaceholderText = "Name",
                    HorizontalAlignment = LayoutAlignment.Fill,
                    Margin = new Thickness(0, 0, 0, 12),
                },
                DockRegion.Top);

            var unitPicker = new ListPicker()
            {
                HorizontalAlignment = LayoutAlignment.Start,
                Margin = new Thickness(0, 0, 0, 12),
            };
            foreach (var unit in new[] { "days", "weeks", "months", "years" })
            {
                unitPicker.Items.Add(new ListPickerItem(unit));
            }
            unitPicker.SelectionChanged += (sender, item) => this._status.Text = "Unit: " + item.Text;
            layout.Children.Add(unitPicker, DockRegion.Top);

            var countPicker = new ListPicker()
            {
                HorizontalAlignment = LayoutAlignment.Start,
                Margin = new Thickness(0, 0, 0, 12),
            };
            for (int count = 1; count <= 30; count += 1)
            {
                countPicker.Items.Add(new ListPickerItem(count.ToString()));
            }
            countPicker.SelectionChanged += (sender, item) => this._status.Text = "Count: " + item.Text;
            layout.Children.Add(countPicker, DockRegion.Top);

            var datePicker = new CalendarDatePicker()
            {
                HorizontalAlignment = LayoutAlignment.Start,
                Margin = new Thickness(0, 0, 0, 12),
                MinimumDate = DateTime.Today,
            };
            datePicker.SelectedDateChanged += (sender, date) => this._status.Text = "Start: " + date?.ToLongDateString();
            layout.Children.Add(datePicker, DockRegion.Top);

            var popoverButton = new Button()
            {
                Text = "Schedule...",
                HorizontalAlignment = LayoutAlignment.Start,
                Size = new Size(150, 44),
                Margin = new Thickness(0, 0, 0, 12),
            };
            popoverButton.Clicked += (sender, e) => new SchedulePopover(this._status).ShowAsync();
            layout.Children.Add(popoverButton, DockRegion.Top);

            layout.Children.Add(

                new ProgressRing()
                {
                    HorizontalAlignment = LayoutAlignment.Start,
                    Margin = new Thickness(0, 0, 0, 12),
                },
                DockRegion.Top);

            layout.Children.Add(

                new Image()
                {
                    Source = "AboutLogo.png",
                    HorizontalAlignment = LayoutAlignment.Start,
                    VerticalAlignment = LayoutAlignment.Start,
                    MaximumSize = new Size(96, 96),
                },
                DockRegion.Left);

            layout.Children.Add(
                new CalendarControl()
                {
                    HorizontalAlignment = LayoutAlignment.Start,
                    VerticalAlignment = LayoutAlignment.Start,
                    Margin = new Thickness(20, 0, 0, 0),
                },
                DockRegion.Left);

            this.Layout = layout;
        }

        private sealed class SchedulePopover : Popover
        {
            private readonly TextView _status;

            public SchedulePopover(
                TextView status)
            {
                this._status = status;

                var calendar = new CalendarControl()
                {
                    MinimumDate = DateTime.Today.AddDays(1),
                };
                calendar.SelectedDateChanged += this.Calendar_SelectedDateChanged;
                this.Content = calendar;
            }

            private void Calendar_SelectedDateChanged(
                object sender,
                DateTime? date)
            {
                this._status.Text = "Scheduled: " + date?.ToLongDateString();
                this.Close();
            }
        }
    }
}
