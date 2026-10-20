using System;
using XForms.Layouts;
using XForms.Controls;

namespace XForms.Toolkit
{
    public class MenuBar : ContentControl, IMenuItemContainer
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

        public override Color BackgroundColor
        {
            get
            {
                if (this.Theme == AppTheme.Dark)
                {
                    return this.Theme.SubtleBackgroundColor;
                }
                else
                {
                    return this.Theme.AccentColor;
                }
            }
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
