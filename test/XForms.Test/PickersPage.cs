using System;
using XForms.Controls;
using XForms.Primitives;
using XForms.Toolkit;
using XForms.Layouts;

namespace XForms.Test
{
    /// <summary>
    /// Pickers, a calendar, a popover, a progress ring and an image.
    /// </summary>
    public class PickersPage : Page
    {
        private readonly TextBlock _status;

        public PickersPage()
        {
            var layout = new DockPanel()
            {
                Margin = new Thickness(20),
            };

            this._status = new TextBlock()
            {
                Text = "Nothing chosen yet",
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(0, 0, 0, 12),
            };
            layout.Children.Add(this._status, Dock.Top);

            layout.Children.Add(
                new TextBox()
                {
                    PlaceholderText = "Name",
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    Margin = new Thickness(0, 0, 0, 12),
                },
                Dock.Top);

            var unitPicker = new ComboBox()
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(0, 0, 0, 12),
            };
            foreach (var unit in new[] { "days", "weeks", "months", "years" })
            {
                unitPicker.Items.Add(new ComboBoxItem(unit));
            }
            unitPicker.SelectionChanged += (sender, item) => this._status.Text = "Unit: " + item.Text;
            layout.Children.Add(unitPicker, Dock.Top);

            var countPicker = new ComboBox()
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(0, 0, 0, 12),
            };
            for (int count = 1; count <= 30; count += 1)
            {
                countPicker.Items.Add(new ComboBoxItem(count.ToString()));
            }
            countPicker.SelectionChanged += (sender, item) => this._status.Text = "Count: " + item.Text;
            layout.Children.Add(countPicker, Dock.Top);

            var datePicker = new CalendarDatePicker()
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(0, 0, 0, 12),
                MinimumDate = DateTime.Today,
            };
            datePicker.SelectedDateChanged += (sender, date) => this._status.Text = "Start: " + date?.ToLongDateString();
            layout.Children.Add(datePicker, Dock.Top);

            var popoverButton = new Button()
            {
                Text = "Schedule...",
                HorizontalAlignment = HorizontalAlignment.Left,
                Width = 150,
                Height = 44,
                Margin = new Thickness(0, 0, 0, 12),
            };
            popoverButton.Clicked += (sender, e) => new ScheduleFlyout(this._status).ShowAsync();
            layout.Children.Add(popoverButton, Dock.Top);

            layout.Children.Add(

                new ProgressRing()
                {
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Margin = new Thickness(0, 0, 0, 12),
                },
                Dock.Top);

            layout.Children.Add(

                new Image()
                {
                    Source = "AboutLogo.png",
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top,
                    MaxWidth = 96,
                    MaxHeight = 96,
                },
                Dock.Left);

            layout.Children.Add(
                new CalendarView()
                {
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top,
                    Margin = new Thickness(20, 0, 0, 0),
                },
                Dock.Left);

            this.Content = layout;
        }

        private sealed class ScheduleFlyout : Flyout
        {
            private readonly TextBlock _status;

            public ScheduleFlyout(
                TextBlock status)
            {
                this._status = status;

                var calendar = new CalendarView()
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
