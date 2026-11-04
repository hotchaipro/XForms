using System;
using XForms.Layouts;
using XForms.Controls;
using XForms.Primitives;

namespace XForms.Controls
{
    /// <summary>
    /// A bar of commands, usually at the bottom of a page: each item's icon
    /// over its label, centered together a gap apart. Its theme's template
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
                Spacing = 40,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Stretch,
            };

            this.HorizontalAlignment = HorizontalAlignment.Stretch;
            this.VerticalAlignment = VerticalAlignment.Top;

            this.Height = 68;

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
        }
    }
}
