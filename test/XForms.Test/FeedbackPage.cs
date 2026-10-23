using System;
using XForms.Controls;
using XForms.Layouts;
using XForms.Primitives;

namespace XForms.Test
{
    /// <summary>
    /// Feedback and disclosure: tooltips, a context menu, info bars of each
    /// severity, and expanders, and a switch that lays the page out right to
    /// left.
    /// </summary>
    public class FeedbackPage : Page
    {
        private TextBlock _status;

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

            // Tooltips: on hover, keyboard focus and a long press
            var row = new StackPanel() { Orientation = Orientation.Horizontal, Spacing = 12 };
            var save = new Button() { Text = "Save", Width = 120, Height = 36, ToolTip = "Saves the document (Ctrl+S)" };
            save.Clicked += (sender, e) => this.Show("Saved");
            row.Children.Add(save);

            // A context menu: a right-click, a long press, the Menu key or
            // Shift+F10
            var copy = new MenuItem() { Text = "Copy" };
            copy.Click += (sender, e) => this.Show("Menu: Copy");
            var paste = new MenuItem() { Text = "Paste (unavailable)", Command = new RelayCommand(() => { }, () => false) };
            var delete = new MenuItem() { Text = "Delete" };
            delete.Click += (sender, e) => this.Show("Menu: Delete");
            var file = new Button()
            {
                Text = "Report.docx",
                Width = 160,
                Height = 36,
                ToolTip = "Right-click or long-press for more",
                ContextFlyout = new MenuFlyout() { Items = { copy, paste, delete } },
            };
            file.Clicked += (sender, e) => this.Show("Opened Report.docx");
            row.Children.Add(file);
            layout.Children.Add(row);

            // Info bars, one of each severity
            var bars = new[]
            {
                new InfoBar() { Title = "Update available", Message = "Restart to finish installing the update.", Severity = InfoBarSeverity.Informational, IsOpen = true },
                new InfoBar() { Title = "Saved", Message = "Your changes were saved.", Severity = InfoBarSeverity.Success, IsOpen = true },
                new InfoBar() { Title = "Low battery", Message = "Connect a charger soon; the device will sleep at 5 percent, and anything not saved by then may be lost.", Severity = InfoBarSeverity.Warning, IsOpen = true, IsClosable = false },
                new InfoBar() { Title = "Couldn't sync", Message = "Check your connection.", Severity = InfoBarSeverity.Error, IsOpen = true },
            };
            var retry = new Button() { Text = "Retry", Width = 90, Height = 32 };
            retry.Clicked += (sender, e) => this.Show("Retrying");
            bars[3].ActionButton = retry;
            foreach (var bar in bars)
            {
                bar.Closed += (sender, e) => this.Show("Closed: " + ((InfoBar)sender).Title);
                layout.Children.Add(bar);
            }

            var reopen = new Button() { Text = "Show the info bars", Width = 180, Height = 36, HorizontalAlignment = HorizontalAlignment.Left };
            reopen.Clicked += (sender, e) =>
            {
                foreach (var bar in bars)
                {
                    bar.IsOpen = true;
                }

                this.Show("Info bars shown");
            };
            layout.Children.Add(reopen);

            // Expanders
            var options = new Expander()
            {
                Header = "Advanced options",
                Content = new StackPanel()
                {
                    Spacing = 8,
                    Children =
                    {
                        new CheckBox() { Text = "Compress files" },
                        new Slider() { Header = "Quality", Value = 80, HorizontalAlignment = HorizontalAlignment.Stretch },
                    },
                },
            };
            options.Expanding += (sender, e) => this.Show("Expanded: Advanced options");
            options.Collapsed += (sender, e) => this.Show("Collapsed: Advanced options");
            layout.Children.Add(options);

            layout.Children.Add(new Expander()
            {
                Header = "Details",
                IsExpanded = true,
                Content = new TextBlock() { Text = "Created today; 3 pages; 1,204 words.", TextWrapping = TextWrapping.Wrap },
            });

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
