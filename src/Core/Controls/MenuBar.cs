using System;
using XForms.Layouts;

namespace XForms.Controls
{
    public class MenuBar : ContentControl, IMenuItemContainer
    {
        private DockLayout _buttonsLayout;

        public MenuBar()
        {
            this._buttonsLayout = new DockLayout()
            {
                HorizontalAlignment = LayoutAlignment.Center,
                VerticalAlignment = LayoutAlignment.Fill,
            };

            this.HorizontalAlignment = LayoutAlignment.Fill;
            this.VerticalAlignment = LayoutAlignment.Start;

            this.Size = new Size(Dimension.Auto, 50);

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
            this._buttonsLayout.Children.Add(menuButton, DockRegion.Left);
        }
    }
}
