using System;
using XForma.Layouts;
using XForma.Controls;
using XForma.Primitives;

namespace XForma.Controls
{
    /// <summary>
    /// A bar of commands: a button for each item, sized and placed as the
    /// theme's <see cref="MenuBarStyle"/> says. If they don't fit across the
    /// bar they go side by side, then share its width. Its theme's template
    /// draws it.
    /// </summary>
    public class MenuBar : TemplatedContentControl, IMenuItemContainer
    {
        private StackPanel _buttonsLayout;

        public MenuBar()
        {
            this._buttonsLayout = new StackPanel()
            {
                Orientation = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignment.Stretch,
            };

            this.HorizontalAlignment = HorizontalAlignment.Stretch;
            this.VerticalAlignment = VerticalAlignment.Top;

            this.MenuItems = new MenuItemCollection(this);

            this.Content = this._buttonsLayout;
        }

        public MenuItemCollection MenuItems
        {
            get;
        }

        void IMenuItemContainer.Add(
            MenuItem menuItem)
        {
            if (null == menuItem)
            {
                throw new ArgumentNullException(nameof(menuItem));
            }

            var menuButton = new MenuButton(this, menuItem);
            this._buttonsLayout.Children.Add(menuButton);
            this.InvalidateMeasure();
        }

        protected override void OnThemeChanged(
            Theme theme)
        {
            base.OnThemeChanged(theme);
            this.InvalidateMeasure();
        }

        protected override Size MeasureOverride(
            Size availableSize)
        {
            var style = this.ActualTheme.MenuBarStyle;
            this.FitButtons(style, availableSize.Width);

            float height = Math.Min(style.Height, availableSize.Height);
            var size = base.MeasureOverride(new Size(availableSize.Width, height));
            return new Size(size.Width, height);
        }

        // Sizes and spaces the buttons for the width: as the style says if the
        // visible ones fit, side by side if not, and sharing it if they still
        // don't
        private void FitButtons(
            MenuBarStyle style,
            float width)
        {
            int count = 0;
            foreach (var child in this._buttonsLayout.Children)
            {
                count += child.IsVisible ? 1 : 0;
            }

            float buttonWidth = style.ItemWidth;
            float spacing = Math.Max(0, style.ItemSpacing);
            if (!float.IsInfinity(width) && (0 < count))
            {
                if (((count * buttonWidth) + ((count - 1) * spacing)) > width)
                {
                    spacing = 0;
                }

                buttonWidth = Math.Min(buttonWidth, width / count);
            }

            if (spacing != this._buttonsLayout.Spacing)
            {
                this._buttonsLayout.Spacing = spacing;
            }

            if (style.ItemsAlignment != this._buttonsLayout.HorizontalAlignment)
            {
                this._buttonsLayout.HorizontalAlignment = style.ItemsAlignment;
            }

            foreach (var child in this._buttonsLayout.Children)
            {
                if (buttonWidth != child.Width)
                {
                    child.Width = buttonWidth;
                }
            }
        }
    }
}
