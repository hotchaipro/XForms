using System;
using XForms.Controls;
using XForms.Primitives;
using XForms.Layouts;

namespace XForms.Test
{
    public class TabbedPage : Page
    {
        private TabPage1 _tabPage1;

        protected override View Build()
        {
            Size tabButtonSize = new Size(60, 60);

            this._tabPage1 = new TabPage1();

            var pageLayout = new DockPanel()
            {
                Width = 400,
                Height = 400,
            };

            var tabBar = new TabStrip()
            {
                Height = 60,
            };

            pageLayout.Children.Add(tabBar, Dock.Top);

            tabBar.AddButton(
                new TabStripItem()
                {
                    Width = tabButtonSize.Width,
                    Height = tabButtonSize.Height,
                    Text = "one",
                    Icon = ThemeResources.Default.AboutLogo,
                },
                this._tabPage1);

            tabBar.AddButton(
                new TabStripItem()
                {
                    Width = tabButtonSize.Width,
                    Height = tabButtonSize.Height,
                    Text = "two",
                    Icon = ThemeResources.Default.AboutLogo,
                },
                this._tabPage1);

            tabBar.AddButton(
                new TabStripItem()
                {
                    Width = tabButtonSize.Width,
                    Height = tabButtonSize.Height,
                    Text = "three",
                    Icon = ThemeResources.Default.AboutLogo,
                },
                this._tabPage1);

            tabBar.AddButton(
                new TabStripItem()
                {
                    Width = tabButtonSize.Width,
                    Height = tabButtonSize.Height,
                    Text = "four",
                    Icon = ThemeResources.Default.AboutLogo,
                },
                this._tabPage1);

            var menuBar = new MenuBar()
            {
            };
            pageLayout.Children.Add(menuBar, Dock.Bottom);

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

            return pageLayout;
        }
    }

    public class TabPage1 : Page
    {
        public TabPage1()
        {
        }
    }
}
