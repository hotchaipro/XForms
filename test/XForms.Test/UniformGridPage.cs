using System;
using XForms.Controls;
using XForms.Primitives;
using XForms.Layouts;

namespace XForms.Test
{
    public class UniformGridPage : Page
    {
        public UniformGridPage()
        {
            var stackLayout = new UniformGrid()
            {
            };

            stackLayout.Children.Add(
                new TextBlock()
                {
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Text = "ITEM1",
                    Foreground = Colors.White,
                    BackgroundColor = Colors.Red,
                });

            stackLayout.Children.Add(
                new Image()
                {
                    Source = ThemeResources.Default.AboutLogo,
                    Width = 50,
                    Height = 50,
                    HorizontalAlignment = HorizontalAlignment.Center,
                });

            stackLayout.Children.Add(
                new TextBlock()
                {
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Text = "ITEM2",
                    Foreground = Colors.White,
                    BackgroundColor = Colors.Red,
                });

            var distributedStackLayout = new UniformGrid()
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
            };

            distributedStackLayout.Children.Add(
                new TextBlock()
                {
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Text = "ITEM1",
                    Foreground = Colors.White,
                    BackgroundColor = Colors.Red,
                });

            distributedStackLayout.Children.Add(
                new Image()
                {
                    Source = ThemeResources.Default.AboutLogo,
                    Width = 50,
                    Height = 50,
                    HorizontalAlignment = HorizontalAlignment.Center,
                });

            distributedStackLayout.Children.Add(
                new TextBlock()
                {
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Text = "ITEM2",
                    Foreground = Colors.White,
                    BackgroundColor = Colors.Red,
                });

            var pageLayout = new DockPanel()
            {
            };

            pageLayout.Children.Add(stackLayout, Dock.Top);
            pageLayout.Children.Add(distributedStackLayout, Dock.Top);

            this.Content = pageLayout;
        }
    }
}
