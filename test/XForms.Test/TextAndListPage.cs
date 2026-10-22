using System;
using System.Collections.ObjectModel;
using XForms.Controls;
using XForms.Primitives;
using XForms.Layouts;

namespace XForms.Test
{
    /// <summary>
    /// Exercises native text entry, scrolling and a recycled list: the pieces a
    /// task app like SoonerOrLater depends on.
    /// </summary>
    public class TextAndListPage : Page
    {
        private ObservableCollection<string> _tasks;
        private TextBox _titleEntry;
        private TextBox _notesEntry;

        protected override View Build()
        {
            var pageLayout = new DockPanel();

            // Editor: a single-line title, a multiline note and an add button
            var editorLayout = new DockPanel()
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(16, 8, 16, 8),
            };

            this._titleEntry = new TextBox()
            {
                PlaceholderText = "New task",
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Margin = new Thickness(0, 0, 0, 8),
            };
            editorLayout.Children.Add(this._titleEntry, Dock.Top);

            this._notesEntry = new TextBox()
            {
                PlaceholderText = "Notes (multiline)",
                AcceptsReturn = true,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Margin = new Thickness(0, 0, 0, 8),
            };
            editorLayout.Children.Add(this._notesEntry, Dock.Top);

            var addButton = new Button()
            {
                Text = "Add",
                Width = 120,
                Height = 40,
                HorizontalAlignment = HorizontalAlignment.Right,
            };
            addButton.Clicked += this.AddButton_Clicked;
            editorLayout.Children.Add(addButton, Dock.Top);

            pageLayout.Children.Add(editorLayout, Dock.Top);

            // List of tasks filling the rest of the page: an observable
            // collection, shown by item views made once and reused
            this._tasks = new ObservableCollection<string>();
            for (int i = 1; i <= 40; i += 1)
            {
                this._tasks.Add($"Task {i}" + ((i % 5 == 0) ? "\nwith a second line of detail" : String.Empty));
            }

            var listView = new ListView<string>()
            {
                ItemsSource = this._tasks,
                ItemViewFactory = ItemViewFactory<string>.Create(
                    () => new TextBlock()
                    {
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                        VerticalAlignment = VerticalAlignment.Center,
                        Margin = new Thickness(16, 12, 16, 12),
                        TextWrapping = TextWrapping.Wrap,
                        FontSize = 17,
                    },
                    (view, task) => view.Text = task),
            };
            pageLayout.Children.Add(listView, Dock.CenterOverlay);

            return pageLayout;
        }

        private void AddButton_Clicked(
            object sender,
            EventArgs e)
        {
            string title = this._titleEntry.Text;
            if (String.IsNullOrWhiteSpace(title))
            {
                this._titleEntry.Focus();
                return;
            }

            string notes = this._notesEntry.Text;
            this._tasks.Insert(0, String.IsNullOrWhiteSpace(notes) ? title : title + "\n" + notes);

            this._titleEntry.Text = String.Empty;
            this._notesEntry.Text = String.Empty;
        }
    }
}
