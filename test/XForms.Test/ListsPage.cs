using System;
using System.Collections.ObjectModel;
using System.Linq;
using XForms.Controls;
using XForms.Layouts;
using XForms.Toolkit;

namespace XForms.Test
{
    /// <summary>
    /// Lists: typed items in groups with headers, item clicks, the three
    /// selection modes, and an observable collection that changes.
    /// </summary>
    public class ListsPage : Page
    {
        private static readonly string[] Fruits =
        {
            "Apple", "Apricot", "Avocado", "Banana", "Blackberry", "Blueberry", "Cherry", "Clementine",
            "Coconut", "Date", "Dragonfruit", "Elderberry", "Fig", "Grape", "Grapefruit", "Guava",
            "Kiwi", "Lemon", "Lime", "Lychee", "Mango", "Melon", "Nectarine", "Orange", "Papaya",
            "Peach", "Pear", "Pineapple", "Plum", "Pomegranate", "Raspberry", "Strawberry",
        };

        private readonly ObservableCollection<string> _fruits = new ObservableCollection<string>(Fruits);
        private readonly ListView<string> _list;
        private readonly TextBlock _status;
        private string _lastClick = "none";
        private int _added;

        public ListsPage()
        {
            var page = new DockPanel();

            var header = new StackPanel() { Spacing = 8, Padding = new Thickness(12) };
            this._status = new TextBlock() { FontSize = 13, TextWrapping = TextWrapping.Wrap };
            header.Children.Add(this._status);

            var modes = new WrapPanel() { ItemSpacing = 8, LineSpacing = 8 };
            modes.Children.Add(this.CreateButton("None", () => this._list.SelectionMode = SelectionMode.None));
            modes.Children.Add(this.CreateButton("Single", () => this._list.SelectionMode = SelectionMode.Single));
            modes.Children.Add(this.CreateButton("Multiple", () => this._list.SelectionMode = SelectionMode.Multiple));
            modes.Children.Add(this.CreateButton("Add", this.AddFruit));
            modes.Children.Add(this.CreateButton("Remove selected", this.RemoveSelected));
            header.Children.Add(modes);
            page.Children.Add(header, Dock.Top);

            // Grouped by first letter, under a header for each
            this._list = new ListView<string>()
            {
                ItemsSource = this._fruits,
                ItemViewFactory = ItemViewFactory<string>.Create(
                    () => new TextBlock() { Margin = new Thickness(24, 10, 12, 10), FontSize = 16 },
                    (view, fruit) => view.Text = fruit),
                GroupKeySelector = (fruit) => fruit.Substring(0, 1),
            };
            this._list.ItemClick += (sender, e) => { this._lastClick = (string)e.ClickedItem; this.UpdateStatus(); };
            this._list.SelectionChanged += (sender, e) => this.UpdateStatus();
            page.Children.Add(this._list, Dock.CenterOverlay);

            this.Content = page;
            this.UpdateStatus();
        }

        private Button CreateButton(
            string text,
            Action action)
        {
            var button = new Button() { Text = text, Height = 32, Width = 8 + (text.Length * 9) + 24 };
            button.Clicked += (sender, e) => { action(); this.UpdateStatus(); };
            return button;
        }

        private void AddFruit()
        {
            // Keep the groups in order: insert in alphabetical order
            this._added += 1;
            string fruit = $"Banana {this._added}";
            int index = 0;
            while ((index < this._fruits.Count) && (string.CompareOrdinal(this._fruits[index], fruit) < 0))
            {
                index += 1;
            }

            this._fruits.Insert(index, fruit);
        }

        private void RemoveSelected()
        {
            foreach (var fruit in this._list.SelectedItems.ToList())
            {
                this._fruits.Remove(fruit);
            }
        }

        private void UpdateStatus()
        {
            var selected = this._list.SelectedItems;
            this._status.Text = $"Mode {this._list.SelectionMode}; clicked {this._lastClick}; "
                + $"selected {selected.Count}: {string.Join(", ", selected)}";
        }
    }
}
