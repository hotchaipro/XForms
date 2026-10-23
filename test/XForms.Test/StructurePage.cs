using System;
using XForms.Controls;
using XForms.Layouts;
using XForms.Primitives;

namespace XForms.Test
{
    /// <summary>
    /// The structural controls: a navigation view whose items show a flip
    /// view with pips, a tree, and a list to pull to refresh (and a switch
    /// that lays the page out right to left).
    /// </summary>
    public class StructurePage : Page
    {
        private TextBlock _status;
        private NavigationView _navigation;
        private Border _section;
        private int _refreshes;

        protected override View Build()
        {
            this._status = new TextBlock() { FontSize = 13, Text = "Ready", Name = "status", Margin = new Thickness(16, 8, 16, 8) };

            this._section = new Border();
            var content = new DockPanel();
            content.Children.Add(this._status, Dock.Top);
            content.Children.Add(this._section, Dock.CenterOverlay);
            this._navigation = new NavigationView() { PaneTitle = "Structure", Content = content };
            var pages = new[] { ("Flip view", this.BuildFlipView()), ("Tree", this.BuildTree()), ("Refresh", this.BuildRefresh()), ("Settings", this.BuildSettings()) };
            foreach (var (text, view) in pages)
            {
                this._navigation.MenuItems.Add(new NavigationViewItem() { Text = text, Tag = view });
            }

            this._navigation.SelectionChanged += (sender, e) =>
            {
                this.ShowSection((View)e.SelectedItem.Tag);
                this.Show("Selected: " + e.SelectedItem.Text);
            };
            this._navigation.SelectedItem = this._navigation.MenuItems[0];
            this.ShowSection((View)this._navigation.MenuItems[0].Tag);
            return this._navigation;
        }

        private void ShowSection(
            View section)
        {
            this._section.Child = section;
        }

        private View BuildFlipView()
        {
            var colors = new[] { Color.FromRgb(0x2F, 0x80, 0xED), Color.FromRgb(0x27, 0xAE, 0x60), Color.FromRgb(0xF2, 0x99, 0x4A), Color.FromRgb(0x9B, 0x51, 0xE0) };
            var flip = new FlipView() { Height = 220, Margin = new Thickness(16, 0, 16, 8), AccessibilityLabel = "Photos" };
            for (int i = 0; i < colors.Length; i += 1)
            {
                flip.Items.Add(new Border()
                {
                    Background = colors[i],
                    Child = new TextBlock() { Text = $"Photo {i + 1}", FontSize = 28, Foreground = Colors.White, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center },
                });
            }

            var pips = new PipsPager() { NumberOfPages = colors.Length, HorizontalAlignment = HorizontalAlignment.Center };
            flip.SelectionChanged += (sender, e) =>
            {
                pips.SelectedPageIndex = flip.SelectedIndex;
                this.Show($"Photo {flip.SelectedIndex + 1}");
            };
            pips.SelectedIndexChanged += (sender, e) => flip.Select(pips.SelectedPageIndex, animated: true);

            return new StackPanel() { Children = { flip, pips } };
        }

        private View BuildTree()
        {
            var tree = new TreeView() { Margin = new Thickness(8, 0, 8, 0), AccessibilityLabel = "Files" };
            var documents = new TreeViewNode() { Content = "Documents", IsExpanded = true };
            documents.Children.Add(new TreeViewNode() { Content = "Report.docx" });
            var photos = new TreeViewNode() { Content = "Photos" };
            photos.Children.Add(new TreeViewNode() { Content = "Beach.jpg" });
            photos.Children.Add(new TreeViewNode() { Content = "Mountains.jpg" });
            documents.Children.Add(photos);
            var music = new TreeViewNode() { Content = "Music" };
            music.Children.Add(new TreeViewNode() { Content = "Song.mp3" });
            tree.RootNodes.Add(documents);
            tree.RootNodes.Add(music);
            tree.RootNodes.Add(new TreeViewNode() { Content = "Notes.txt" });
            tree.ItemInvoked += (sender, e) => this.Show("Invoked: " + e.Node.Content);
            return new ScrollViewer() { IsVerticalScrollingEnabled = true, Content = tree };
        }

        private View BuildRefresh()
        {
            var messages = new System.Collections.ObjectModel.ObservableCollection<string>();
            for (int i = 1; i <= 30; i += 1)
            {
                messages.Add($"Message {i}");
            }

            var refresh = new RefreshContainer()
            {
                Content = new ListView<string>()
                {
                    ItemsSource = messages,
                    ItemViewFactory = ItemViewFactory<string>.Create(
                        () => new TextBlock() { Margin = new Thickness(16, 8, 12, 8) },
                        (view, message) => view.Text = message),
                },
            };
            refresh.RefreshRequested += async (sender, e) =>
            {
                var done = e.GetDeferral();
                this.Show("Refreshing...");
                await System.Threading.Tasks.Task.Delay(1500);
                this._refreshes += 1;
                messages.Insert(0, $"New message {this._refreshes}");
                this.Show($"Refreshed {this._refreshes} times");
                done();
            };

            var button = new Button() { Text = "Refresh", Width = 120, Height = 36, Margin = new Thickness(16, 0, 16, 8), HorizontalAlignment = HorizontalAlignment.Left };
            button.Clicked += (sender, e) => refresh.RequestRefresh();

            var layout = new DockPanel();
            layout.Children.Add(button, Dock.Top);
            layout.Children.Add(refresh, Dock.CenterOverlay);
            return layout;
        }

        private View BuildSettings()
        {
            var rightToLeft = new ToggleSwitch() { Header = "Right to left", Margin = new Thickness(16, 0, 16, 0) };
            rightToLeft.Toggled += (sender, e) =>
            {
                this._navigation.FlowDirection = e.Value ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
                this._navigation.InvalidateMeasure();
                this.Show("Right to left: " + e.Value);
            };
            return new StackPanel() { Children = { rightToLeft } };
        }

        private void Show(
            string text)
        {
            this._status.Text = text;
        }
    }
}
