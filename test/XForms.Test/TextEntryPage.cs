using System;
using XForms.Controls;
using XForms.Layouts;
using XForms.Primitives;

namespace XForms.Test
{
    /// <summary>
    /// Text entry: submitting, a password, a maximum length, read-only text,
    /// and the selection.
    /// </summary>
    public class TextEntryPage : Page
    {
        private readonly TextBlock _status;
        private readonly TextBox _selectable;
        private int _submits;

        public TextEntryPage()
        {
            var layout = new StackPanel()
            {
                Spacing = 10,
                Padding = new Thickness(16),
            };

            this._status = new TextBlock() { Text = "Type, press Enter, or select text", FontSize = 13 };
            layout.Children.Add(this._status);

            // Enter submits a single-line box
            var submit = new TextBox() { PlaceholderText = "Type and press Enter", HorizontalAlignment = HorizontalAlignment.Stretch };
            submit.Submitted += (sender, e) =>
            {
                this._submits += 1;
                this._status.Text = $"Submitted {this._submits}: \"{submit.Text}\"";
            };
            layout.Children.Add(submit);

            // A password
            var password = new TextBox() { PlaceholderText = "Password", Text = "secret", IsPassword = true, HorizontalAlignment = HorizontalAlignment.Stretch };
            password.TextChanged += (sender, e) => this._status.Text = $"Password has {password.Text?.Length ?? 0} characters";
            layout.Children.Add(password);

            // At most five characters
            var limited = new TextBox() { PlaceholderText = "At most 5 characters", MaxLength = 5, HorizontalAlignment = HorizontalAlignment.Stretch };
            layout.Children.Add(limited);

            // Read-only: selectable, not editable
            var readOnly = new TextBox() { Text = "Read-only: select me, but you can't change me", IsReadOnly = true, HorizontalAlignment = HorizontalAlignment.Stretch };
            layout.Children.Add(readOnly);

            // The selection
            this._selectable = new TextBox() { Text = "The quick brown fox jumps", HorizontalAlignment = HorizontalAlignment.Stretch };
            this._selectable.SelectionChanged += (sender, e) => this.ShowSelection();
            layout.Children.Add(this._selectable);

            var buttons = new StackPanel() { Orientation = Orientation.Horizontal, Spacing = 8 };
            var selectAll = new Button() { Text = "Select all", Width = 110, Height = 36 };
            selectAll.Clicked += (sender, e) => { this._selectable.Focus(); this._selectable.SelectAll(); this.ShowSelection(); };
            buttons.Children.Add(selectAll);
            var selectWord = new Button() { Text = "Select \"quick\"", Width = 130, Height = 36 };
            selectWord.Clicked += (sender, e) => { this._selectable.Focus(); this._selectable.Select(4, 5); this.ShowSelection(); };
            buttons.Children.Add(selectWord);
            layout.Children.Add(buttons);

            // Multiline: Enter is a new line, not a submit
            var multiline = new TextBox() { PlaceholderText = "Multiline: Enter adds a line", AcceptsReturn = true, HorizontalAlignment = HorizontalAlignment.Stretch };
            multiline.Submitted += (sender, e) => this._status.Text = "A multiline box submitted (it shouldn't)";
            layout.Children.Add(multiline);

            var page = new DockPanel();
            page.Children.Add(layout, Dock.CenterOverlay);
            this.Content = page;
        }

        private void ShowSelection()
        {
            var box = this._selectable;
            this._status.Text = $"Selection {box.SelectionStart}+{box.SelectionLength}: \"{box.SelectedText}\"";
        }
    }
}
