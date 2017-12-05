using System;
using XForms.Controls;
using XForms.Layouts;

namespace XForms.Test
{
    public class DistributedStackLayoutPage : Page
    {
        public DistributedStackLayoutPage()
        {
            var stackLayout = new DistributedStackLayout()
            {
            };

            stackLayout.Children.Add(
                new TextView()
                {
                    HorizontalAlignment = LayoutAlignment.Center,
                    Text = "ITEM1",
                    ForegroundColor = Colors.White,
                    BackgroundColor = Colors.Red,
                });

            stackLayout.Children.Add(
                new Image()
                {
                    Source = ThemeResources.Default.AboutLogo,
                    Size = new Size(50),
                    HorizontalAlignment = LayoutAlignment.Center,
                });

            stackLayout.Children.Add(
                new TextView()
                {
                    HorizontalAlignment = LayoutAlignment.Center,
                    Text = "ITEM2",
                    ForegroundColor = Colors.White,
                    BackgroundColor = Colors.Red,
                });

            var distributedStackLayout = new DistributedStackLayout()
            {
                HorizontalAlignment = LayoutAlignment.Fill,
            };

            distributedStackLayout.Children.Add(
                new TextView()
                {
                    HorizontalAlignment = LayoutAlignment.Center,
                    Text = "ITEM1",
                    ForegroundColor = Colors.White,
                    BackgroundColor = Colors.Red,
                });

            distributedStackLayout.Children.Add(
                new Image()
                {
                    Source = ThemeResources.Default.AboutLogo,
                    Size = new Size(50),
                    HorizontalAlignment = LayoutAlignment.Center,
                });

            distributedStackLayout.Children.Add(
                new TextView()
                {
                    HorizontalAlignment = LayoutAlignment.Center,
                    Text = "ITEM2",
                    ForegroundColor = Colors.White,
                    BackgroundColor = Colors.Red,
                });

            var pageLayout = new DockLayout()
            {
            };

            pageLayout.Children.Add(stackLayout, DockRegion.Top);
            pageLayout.Children.Add(distributedStackLayout, DockRegion.Top);

            this.Layout = pageLayout;
        }
    }
}
