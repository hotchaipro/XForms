using System;
using XForms.Layouts;
using XForms.Controls;
using XForms.Primitives;

namespace XForms.Controls
{
    public class MenuBar : TemplatedContentControl, IMenuItemContainer
    {
        private DockPanel _buttonsLayout;

        public MenuBar()
        {
            this._buttonsLayout = new DockPanel()
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Stretch,
            };

            this.HorizontalAlignment = HorizontalAlignment.Stretch;
            this.VerticalAlignment = VerticalAlignment.Top;

            this.Height = 50;

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
            this._buttonsLayout.Children.Add(menuButton, Dock.Left);
        }
    }
}
