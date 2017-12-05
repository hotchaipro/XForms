using System;
using System.Collections.Generic;
using System.Text;
using XForms.Controls;
using XForms.Graphics;
using XForms.Input;
using XForms.Layouts;
using XForms.Pages;

namespace XForms.Test
{
    public class TabbedPage : Page
    {
        private TabPage1 _tabPage1;

        public TabbedPage()
        {
            Size tabButtonSize = new Size(60, 60);

            this._tabPage1 = new TabPage1();

            var iconBitmap = new Bitmap("AboutLogo");
            iconBitmap.LoadAsync();

            var pageLayout = new DockLayout()
            {
                Size = new Size(400, 400),
            };

            var tabBar = new TabBar()
            {
                Size = new Size(Dimension.Auto, 60),
            };

            pageLayout.Children.Add(tabBar, DockRegion.Top);

            tabBar.AddButton(
                new TabButton()
                {
                    Size = tabButtonSize,
                    Text = "one",
                    Icon = iconBitmap,
                },
                this._tabPage1);

            tabBar.AddButton(
                new TabButton()
                {
                    Size = tabButtonSize,
                    Text = "two",
                    Icon = iconBitmap,
                },
                this._tabPage1);

            tabBar.AddButton(
                new TabButton()
                {
                    Size = tabButtonSize,
                    Text = "three",
                    Icon = iconBitmap,
                },
                this._tabPage1);

            tabBar.AddButton(
                new TabButton()
                {
                    Size = tabButtonSize,
                    Text = "four",
                    Icon = iconBitmap,
                },
                this._tabPage1);

            var menuBar = new MenuBar()
            {
            };
            pageLayout.Children.Add(menuBar, DockRegion.Bottom);

            menuBar.MenuItems.Add(new MenuItem()
            {
                Text = "cancel",
                Icon = ThemeResources.Default.CancelIcon,
            });

            var deleteMenuItem = new MenuItem()
            {
                Text = "delete",
                Icon = ThemeResources.Default.DeleteIcon,
            };
            menuBar.MenuItems.Add(deleteMenuItem);

            menuBar.MenuItems.Add(new MenuItem()
            {
                Text = "save",
                Icon = ThemeResources.Default.SaveIcon,
            });

            this.Layout = pageLayout;
        }
    }

    public class TabPage1 : Page
    {
        public TabPage1()
        {
        }
    }
}
