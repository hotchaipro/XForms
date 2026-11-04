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
            // Tab two is a list of two-line rows (it fills in from the first
            // frame of a swipe to it)
            if (this._name == "two")
            {
                var rows = new System.Collections.Generic.List<int>();
                for (int i = 1; i <= 30; i += 1)
                {
                    rows.Add(i);
                }

                return new ListView<int>()
                {
                    ItemsSource = rows,
                    ItemViewFactory = ItemViewFactory<int>.Create(
                        () => new TextBlock() { Margin = new Thickness(20, 10), FontSize = 18 },
                        (view, row) => view.Text = "Task " + row + "
Wednesday, November " + row + " (in " + row + " days)"),
                };
            }

            return new TextBlock()
            {
                Text = "Tab " + this._name,
                FontSize = 28,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            };
        }
    }
}
