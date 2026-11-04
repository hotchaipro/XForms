using System;
using XForms.Controls;
using XForms.Primitives;
using XForms.Layouts;

namespace XForms.Test
{
    /// <summary>
    /// Tabs over a frame of same-colored pages (their edges show by their
    /// shadows as they slide), a floating button whose sideways swipe switches
    /// tabs as it moves, and a flyout button.
    /// </summary>
    public class TabbedPage : Page
    {
        protected override View Build()
        {
            var pageLayout = new DockPanel()
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
            };

            var frame = new Frame();
            var tabBar = new TabStrip()
            {
                Height = 70,
                Frame = frame,
            };
            pageLayout.Children.Add(tabBar, Dock.Top);

            string[] names = { "one", "two", "three", "four" };
            TabStripItem first = null;
            foreach (var name in names)
            {
                var page = new TabContentPage(name);
                var item = new TabStripItem()
                {
                    Width = 70,
                    Height = 70,
                    Text = name,
                    Icon = ThemeResources.Default.AboutLogo,
                };
                tabBar.AddButton(item, page);
                frame.Push(page);
                first ??= item;
            }

            pageLayout.Children.Add(frame, Dock.CenterOverlay);

            var tray = new DockPanel()
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Height = 120,
            };
            pageLayout.Children.Add(tray, Dock.BottomOverlay);

            var addButton = new FloatingButton()
            {
                Width = 68,
                Height = 68,
                Icon = ThemeResources.Default.AboutLogo,
                AccessibilityLabel = "Add (swipe to switch tabs)",
                // A bigger touch target than it looks
                HitTestOutset = new Thickness(24),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            };
            addButton.Swiping += (sender, e) => tabBar.FollowSwipe(e);
            tray.Children.Add(addButton, Dock.CenterOverlay);

            var menuButton = new FlyoutButton()
            {
                Width = 52,
                Height = 52,
                Icon = ThemeResources.Default.AboutLogo,
                AccessibilityLabel = "Menu",
                IsSubtleStyle = false,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(20, 0),
            };
            for (int i = 0; i < 3; i += 1)
            {
                menuButton.AddButton(new FloatingButton()
                {
                    Width = 52,
                    Height = 52,
                    Icon = ThemeResources.Default.AboutLogo,
                    AccessibilityLabel = "Item " + (i + 1),
                });
            }
            tray.Children.Add(menuButton, Dock.Right);

            first.IsChecked = true;
            return pageLayout;
        }
    }

    // A tab's page: its name, on the background
    public class TabContentPage : Page
    {
        private readonly string _name;

        public TabContentPage(
            string name)
        {
            this._name = name;
        }

        protected override View Build()
        {
            // Tab two is a list of rows with a title and (on most) a detail
            // line, hidden until a moment after the page is built, while the
            // page is still hidden (it fills in from the first frame of a swipe
            // to it)
            if (this._name == "two")
            {
                var rows = new System.Collections.Generic.List<int>();
                for (int i = 1; i <= 30; i += 1)
                {
                    rows.Add(i);
                }

                var list = new ListView<int>()
                {
                    ItemsSource = rows,
                    IsVisible = false,
                    ExtendsIntoSafeArea = SafeAreaEdges.Bottom | SafeAreaEdges.Left | SafeAreaEdges.Right,
                    ItemViewFactory = ItemViewFactory<int>.Create(
                        () =>
                        {
                            var row = new StackPanel() { Margin = new Thickness(20, 12), Spacing = 2 };
                            row.Children.Add(new TextBlock() { FontSize = 21, TextWrapping = TextWrapping.Wrap });
                            row.Children.Add(new TextBlock() { FontSize = 14 });
                            return row;
                        },
                        (view, row) =>
                        {
                            ((TextBlock)view.Children[0]).Text = "Old task number " + row;
                            var detail = (TextBlock)view.Children[1];
                            detail.Text = "Wednesday, November " + row + " (in " + row + " days)";
                            detail.IsVisible = (row % 3) != 0;
                        }),
                };

                var timer = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(500) };
                timer.Tick += (sender, e) =>
                {
                    timer.Stop();
                    list.IsVisible = true;
                };
                timer.Start();
                return list;
            }

            // Tab three is a command list, its second row showing its command
            // bar: four commands, and edit at the end
            if (this._name == "three")
            {
                var list = new CommandListView() { CommandBarButtonSpacing = 12 };
                foreach (var text in new[] { "never", "schedule", "soon", "done" })
                {
                    list.CommandBarCommands.PrimaryCommands.Add(new UICommand() { Text = text, Icon = ThemeResources.Default.AboutLogo, Command = new RelayCommand(() => { }) });
                }

                list.CommandBarCommands.SecondaryCommands.Add(new UICommand() { Text = "edit", Icon = ThemeResources.Default.AboutLogo, Command = new RelayCommand(() => { }) });
                list.Bind(new RowsSource(12));
                return list;
            }

            return new TextBlock()
            {
                Text = "Tab " + this._name,
                FontSize = 28,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            };
        }

        // Rows numbered from 1
        private sealed class RowsSource : XForms.Primitives.ItemsViewSource
        {
            private readonly int _count;

            public RowsSource(
                int count)
            {
                this._count = count;
            }

            public override int Count => this._count;

            public override object this[int index] => index + 1;

            public override int IndexOf(
                object item)
            {
                return (item is int row) ? row - 1 : -1;
            }

            public override View GetItemView(
                View reuseView,
                object item)
            {
                var view = reuseView as RowView ?? new RowView();
                view.Item = item;
                return view;
            }
        }

        // A row's title; the second row shows its command bar
        private sealed class RowView : CommandListViewItem
        {
            private readonly TextBlock _title = new TextBlock() { FontSize = 18, Margin = new Thickness(20, 14) };

            public RowView()
            {
                this.Content = this._title;
            }

            protected override void OnItemChanged()
            {
                int row = (int)this.Item;
                this._title.Text = "Task " + row;
                this.IsCommandBarVisible = (row == 2);
            }
        }
    }
}
