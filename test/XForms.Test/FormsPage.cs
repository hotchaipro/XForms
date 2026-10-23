using System;
using System.Linq;
using XForms.Controls;
using XForms.Layouts;
using XForms.Primitives;

namespace XForms.Test
{
    /// <summary>
    /// The form controls: check boxes, a switch, a slider and progress bars,
    /// a time picker, a number box, a password box and an auto-suggest box,
    /// and a switch that lays the page out right to left.
    /// </summary>
    public class FormsPage : Page
    {
        private static readonly string[] Fruits =
        {
            "Apple", "Apricot", "Avocado", "Banana", "Blackberry", "Blueberry", "Cherry", "Clementine",
            "Coconut", "Cranberry", "Date", "Fig", "Grape", "Grapefruit", "Guava", "Kiwi", "Lemon", "Lime",
            "Mango", "Melon", "Nectarine", "Orange", "Papaya", "Peach", "Pear", "Pineapple", "Plum",
            "Pomegranate", "Raspberry", "Strawberry", "Watermelon",
        };

        private TextBlock _status;
        private ProgressBar _progress;

        protected override View Build()
        {
            var layout = new StackPanel() { Spacing = 12, Padding = new Thickness(16) };

            this._status = new TextBlock() { FontSize = 13, Text = "Ready", Name = "status" };
            layout.Children.Add(this._status);

            var rightToLeft = new ToggleSwitch() { Header = "Right to left" };
            rightToLeft.Toggled += (sender, e) =>
            {
                layout.FlowDirection = e.Value ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
                layout.InvalidateMeasure();
                this.Show("Right to left: " + e.Value);
            };
            layout.Children.Add(rightToLeft);

            var remember = new CheckBox() { Text = "Remember me" };
            remember.IsCheckedChanged += (sender, e) => this.Show("Remember me: " + remember.IsChecked);
            layout.Children.Add(remember);

            var all = new CheckBox() { Text = "Select all (three states)", IsThreeState = true };
            all.IsCheckedChanged += (sender, e) => this.Show("Select all: " + (all.IsChecked?.ToString() ?? "indeterminate"));
            layout.Children.Add(all);

            layout.Children.Add(new CheckBox() { Text = "Disabled", IsChecked = true, IsEnabled = false });

            var wifi = new ToggleSwitch() { Header = "Wi-Fi", IsOn = true };
            wifi.Toggled += (sender, e) => this.Show("Wi-Fi: " + (e.Value ? "on" : "off"));
            layout.Children.Add(wifi);

            var volume = new Slider() { Header = "Volume", Value = 40, HorizontalAlignment = HorizontalAlignment.Stretch };
            volume.ValueChanged += (sender, e) =>
            {
                this._progress.Value = e.NewValue;
                this.Show($"Volume: {e.NewValue:0}");
            };
            layout.Children.Add(volume);

            this._progress = new ProgressBar() { Value = 40, HorizontalAlignment = HorizontalAlignment.Stretch, AccessibilityLabel = "Volume level" };
            layout.Children.Add(this._progress);

            var loading = new ProgressBar() { IsIndeterminate = true, HorizontalAlignment = HorizontalAlignment.Stretch, AccessibilityLabel = "Loading" };
            layout.Children.Add(loading);

            // Stopping it lets Android's UI Automator find the page idle
            var busy = new CheckBox() { Text = "Busy", IsChecked = true };
            busy.IsCheckedChanged += (sender, e) =>
            {
                loading.IsIndeterminate = busy.IsChecked == true;
                this.Show("Busy: " + busy.IsChecked);
            };
            layout.Children.Add(busy);

            var alarm = new TimePicker() { HorizontalAlignment = HorizontalAlignment.Left, MinuteIncrement = 5 };
            alarm.SelectedTimeChanged += (sender, time) => this.Show("Alarm: " + (time.HasValue ? alarm.FormatTime(time.Value) : "none"));
            layout.Children.Add(alarm);

            var quantity = new NumberBox()
            {
                PlaceholderText = "Quantity (0 to 10)",
                Minimum = 0,
                Maximum = 10,
                SpinButtonPlacementMode = NumberBoxSpinButtonPlacementMode.Inline,
                HorizontalAlignment = HorizontalAlignment.Left,
                Width = 220,
            };
            quantity.ValueChanged += (sender, e) => this.Show($"Quantity: {e.NewValue}");
            layout.Children.Add(quantity);

            var password = new PasswordBox() { PlaceholderText = "Password", HorizontalAlignment = HorizontalAlignment.Left, Width = 220 };
            password.PasswordChanged += (sender, e) => this.Show($"Password: {password.Password.Length} characters");
            layout.Children.Add(password);

            var fruit = new AutoSuggestBox() { PlaceholderText = "Fruit", HorizontalAlignment = HorizontalAlignment.Left, Width = 220 };
            fruit.TextChanged += (sender, e) =>
            {
                if (e.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
                {
                    string text = fruit.Text.Trim();
                    fruit.ItemsSource = (text.Length == 0)
                        ? Array.Empty<string>()
                        : Fruits.Where(f => f.Contains(text, StringComparison.CurrentCultureIgnoreCase)).ToArray();
                }
            };
            fruit.QuerySubmitted += (sender, e) => this.Show($"Fruit: {e.QueryText}" + ((null != e.ChosenSuggestion) ? " (chosen)" : string.Empty));
            layout.Children.Add(fruit);

            return new ScrollViewer()
            {
                IsVerticalScrollingEnabled = true,
                Content = layout,
            };
        }

        private void Show(
            string text)
        {
            this._status.Text = text;
        }
    }
}
