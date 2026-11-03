using System;
using XForms.Controls;
using XForms.Layouts;
using XForms.Primitives;

namespace XForms.Test
{
    /// <summary>
    /// Bars that reach into the safe area (View.ExtendsIntoSafeArea): the top
    /// bar fills the status bar's area, the bottom one the home indicator's,
    /// and each slides off the screen and back as one view.
    /// </summary>
    public class SafeAreaPage : Page
    {
        private Border _topBar;
        private Border _bottomBar;
        private bool _areBarsHidden;

        protected override View Build()
        {
            var accent = this.ActualTheme.Colors.Accent;

            this._topBar = new Border()
            {
                Animator = new(),
                Background = accent,
                ExtendsIntoSafeArea = SafeAreaEdges.Top | SafeAreaEdges.Left | SafeAreaEdges.Right,
                Child = new TextBlock() { Text = "Top bar", Foreground = Colors.White, FontSize = 18, Margin = new Thickness(16, 12) },
            };

            this._bottomBar = new Border()
            {
                Animator = new(),
                Background = accent,
                ExtendsIntoSafeArea = SafeAreaEdges.Bottom | SafeAreaEdges.Left | SafeAreaEdges.Right,
                Child = new TextBlock() { Text = "Bottom bar", Foreground = Colors.White, FontSize = 18, Margin = new Thickness(16, 12) },
            };

            var toggle = new Button() { Text = "Slide the bars away", HorizontalAlignment = HorizontalAlignment.Left, Width = 220, Height = 40 };
            toggle.Clicked += (sender, e) => this.ToggleBars(toggle);

            var content = new StackPanel() { Spacing = 12, Padding = new Thickness(16) };
            content.Children.Add(new TextBlock() { Text = "The bars reach the screen's edges; their text stays clear of the system's bars.", TextWrapping = TextWrapping.Wrap });
            content.Children.Add(toggle);

            var layout = new DockPanel();
            layout.Children.Add(this._topBar, Dock.Top);
            layout.Children.Add(this._bottomBar, Dock.Bottom);
            layout.Children.Add(content, Dock.CenterOverlay);
            return layout;
        }

        private void ToggleBars(
            Button toggle)
        {
            this._areBarsHidden = !this._areBarsHidden;
            var duration = TimeSpan.FromMilliseconds(300);
            float top = this._areBarsHidden ? -this._topBar.Bounds.Height : 0;
            float bottom = this._areBarsHidden ? this._bottomBar.Bounds.Height : 0;
            _ = this._topBar.Animator.TranslateTo(new Point(0, top), duration);
            _ = this._bottomBar.Animator.TranslateTo(new Point(0, bottom), duration);
            toggle.Text = this._areBarsHidden ? "Bring the bars back" : "Slide the bars away";
        }
    }

    /// <summary>
    /// A list that reaches under the home indicator: its rows scroll beneath
    /// it, and the last one scrolls clear of it.
    /// </summary>
    public class SafeAreaListPage : Page
    {
        protected override View Build()
        {
            var topBar = new Border()
            {
                Background = this.ActualTheme.Colors.Accent,
                ExtendsIntoSafeArea = SafeAreaEdges.Top | SafeAreaEdges.Left | SafeAreaEdges.Right,
                Child = new TextBlock() { Text = "List under the home indicator", Foreground = Colors.White, FontSize = 18, Margin = new Thickness(16, 12) },
            };

            var items = new System.Collections.Generic.List<string>();
            for (int i = 1; i <= 60; i += 1)
            {
                items.Add($"Row {i}");
            }

            var list = new ListView<string>()
            {
                ItemsSource = items,
                ItemViewFactory = ItemViewFactory<string>.Create(
                    () => new TextBlock() { Margin = new Thickness(16, 10), FontSize = 16 },
                    (view, item) => view.Text = item),
                ExtendsIntoSafeArea = SafeAreaEdges.Bottom | SafeAreaEdges.Left | SafeAreaEdges.Right,
            };

            var layout = new DockPanel();
            layout.Children.Add(topBar, Dock.Top);
            layout.Children.Add(list, Dock.CenterOverlay);
            return layout;
        }
    }
}
