using System;
using System.Collections.Generic;
using XForms.Controls;
using XForms.Toolkit;
using XForms.Layouts;

namespace XForms.Test
{
    /// <summary>
    /// Exercises native text entry, scrolling and a recycled list: the pieces a
    /// task app like SoonerOrLater depends on.
    /// </summary>
    public class TextAndListPage : Page
    {
        private readonly TaskSource _tasks;
        private readonly TextBox _titleEntry;
        private readonly TextBox _notesEntry;

        public TextAndListPage()
        {
            this.BackgroundColor = this.Application.Theme.BackgroundColor;

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

            // List of tasks filling the rest of the page
            this._tasks = new TaskSource();
            for (int i = 1; i <= 40; i += 1)
            {
                this._tasks.Add($"Task {i}" + ((i % 5 == 0) ? "\nwith a second line of detail" : String.Empty));
            }

            var listView = new ListView();
            listView.Bind(this._tasks);
            pageLayout.Children.Add(listView, Dock.CenterOverlay);

            this.Content = pageLayout;
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

        private sealed class TaskSource : ListViewSource
        {
            private readonly List<string> _items = new List<string>();

            public override int Count
            {
                get
                {
                    return this._items.Count;
                }
            }

            public override object this[int index]
            {
                get
                {
                    return this._items[index];
                }
            }

            public void Add(
                string item)
            {
                this._items.Add(item);
            }

            public void Insert(
                int index,
                string item)
            {
                this._items.Insert(index, item);

                this.NotifyCollectionChanged(
                    this,
                    new System.Collections.Specialized.NotifyCollectionChangedEventArgs(
                        System.Collections.Specialized.NotifyCollectionChangedAction.Add,
                        item,
                        index));
            }

            public override int IndexOf(
                object item)
            {
                return this._items.IndexOf((string)item);
            }

            public override ListViewItem GetItemView(
                View reuseView,
                object item)
            {
                var itemView = reuseView as TaskItemView ?? new TaskItemView();
                itemView.Text = (string)item;
                return itemView;
            }
        }

        private sealed class TaskItemView : ListViewItem
        {
            private readonly TextBlock _textView;

            public TaskItemView()
            {
                this.HorizontalAlignment = HorizontalAlignment.Stretch;

                this._textView = new TextBlock()
                {
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(16, 12, 16, 12),
                    TextWrapping = TextWrapping.Wrap,
                    FontSize = 17,
                };

                this.Content = this._textView;
            }

            public string Text
            {
                get
                {
                    return this._textView.Text;
                }

                set
                {
                    this._textView.Text = value;
                }
            }
        }
    }
}
