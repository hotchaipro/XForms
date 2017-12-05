using System;
using XForms.Controls;
using XForms.Layouts;

namespace XForms.Test
{
    public class TabbedPage : Page
    {
        private TabPage1 _tabPage1;

        public TabbedPage()
        {
            Size tabButtonSize = new Size(60, 60);

            this._tabPage1 = new TabPage1();

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
                    Icon = ThemeResources.Default.AboutLogo,
                },
                this._tabPage1);

            tabBar.AddButton(
                new TabButton()
                {
                    Size = tabButtonSize,
                    Text = "two",
                    Icon = ThemeResources.Default.AboutLogo,
                },
                this._tabPage1);

            tabBar.AddButton(
                new TabButton()
                {
                    Size = tabButtonSize,
                    Text = "three",
                    Icon = ThemeResources.Default.AboutLogo,
                },
                this._tabPage1);

            tabBar.AddButton(
                new TabButton()
                {
                    Size = tabButtonSize,
                    Text = "four",
                    Icon = ThemeResources.Default.AboutLogo,
                },
                this._tabPage1);

            var menuBar = new MenuBar()
            {
            };
            pageLayout.Children.Add(menuBar, DockRegion.Bottom);

            menuBar.MenuItems.Add(new MenuItem()
            {
                Text = "cancel",
                Icon = ThemeResources.Default.AboutLogo,
            });

            var deleteMenuItem = new MenuItem()
            {
                Text = "delete",
                Icon = ThemeResources.Default.AboutLogo,
            };
            menuBar.MenuItems.Add(deleteMenuItem);

            menuBar.MenuItems.Add(new MenuItem()
            {
                Text = "save",
                Icon = ThemeResources.Default.AboutLogo,
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
