using System;
using XForms.Controls;
using XForms.Layouts;
using XForms.Primitives;
using XForms.Toolkit;

namespace XForms.Test
{
    /// <summary>
    /// Controls with accessible states, values and actions (a toggle, radio
    /// buttons, a picker and a selectable list), for screen readers and
    /// keyboard focus.
    /// </summary>
    public class AccessibilityPage : Page
    {
        private readonly TextBlock _status;

        public AccessibilityPage()
        {
            var layout = new StackPanel() { Spacing = 10, Padding = new Thickness(16) };

            layout.Children.Add(new TextBlock() { Text = "Accessibility", FontSize = 22, FontWeight = FontWeight.SemiBold, AccessibilityRole = AccessibilityRole.Heading });

            this._status = new TextBlock() { FontSize = 13, Text = "Ready" };
            layout.Children.Add(this._status);

            var row = new WrapPanel() { ItemSpacing = 8, LineSpacing = 8 };

            var button = new Button() { Text = "Say hello", Width = 120, Height = 34 };
            button.Clicked += (sender, e) => this.Show("Hello");
            button.PointerEntered += (sender, e) => this.Show($"Pointer over the button at {e.Position.X:0},{e.Position.Y:0}");
            button.PointerExited += (sender, e) => this.Show("Pointer left the button");
            row.Children.Add(button);

            var toggle = new ToggleButton() { Text = "Bold" };
            toggle.IsCheckedChanged += (sender, e) => this.Show($"Bold: {e.Value}");
            row.Children.Add(toggle);

            var picker = new ComboBox() { HorizontalAlignment = HorizontalAlignment.Left };
            foreach (var size in new[] { "Small", "Medium", "Large" })
            {
                picker.Items.Add(new ComboBoxItem(size));
            }

            picker.Select(1);
            picker.SelectionChanged += (sender, e) => this.Show($"Size: {picker.SelectedItem?.Text}");
            row.Children.Add(picker);
            layout.Children.Add(row);

            var radios = new RadioButtons() { HorizontalAlignment = HorizontalAlignment.Left };
            foreach (var color in new[] { "Red", "Green", "Blue" })
            {
                radios.Items.Add(new RadioButtonItem() { Text = color, Value = color });
            }

            radios.SelectedItem = "Red";
            radios.SelectionChanged += (sender, e) => this.Show($"Color: {radios.SelectedItem}");
            layout.Children.Add(radios);

            var list = new ListView<string>()
            {
                ItemsSource = new[] { "Apple", "Banana", "Cherry", "Damson" },
                ItemViewFactory = ItemViewFactory<string>.Create(
                    () => new TextBlock() { Margin = new Thickness(16, 8, 12, 8), FontSize = 16 },
                    (view, fruit) => view.Text = fruit),
                SelectionMode = SelectionMode.Single,
                Height = 200,
            };
            list.SelectionChanged += (sender, e) => this.Show($"Fruit: {list.SelectedItem}");

            var page = new DockPanel();
            page.Children.Add(layout, Dock.Top);
            page.Children.Add(list, Dock.CenterOverlay);
            this.Content = page;
        }

        private void Show(
            string message)
        {
            this._status.Text = message;
        }
    }
}
