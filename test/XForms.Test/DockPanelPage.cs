using System;
using XForms.Controls;
using XForms.Toolkit;
using XForms.Layouts;

namespace XForms.Test
{
    public class DockPanelPage : Page
    {
        public DockPanelPage()
        {
            Color backgroundColor = Color.FromArgb(0x80, 0xff, 0, 0);
            Size wideSize = new Size(150, Dimension.Auto);
            Size tallSize = new Size(Dimension.Auto, 100);

            var pageLayout = new DockPanel()
            {
                //Size = new Size(300),
                //Size = new Size(Dimension.Auto, Dimension.Auto),
                //HorizontalAlignment = HorizontalAlignment.Stretch,
                //VerticalAlignment = VerticalAlignment.Stretch,
            };

            pageLayout.Children.Add(
                new TextBlock()
                {
                    Text = "TOP-L",
                    Foreground = Colors.Black,
                    BackgroundColor = backgroundColor,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top,
                    TextAlignment = TextAlignment.Start,
                    VerticalTextAlignment = TextAlignment.Start,
                },
                Dock.Top);

            pageLayout.Children.Add(
                new TextBlock()
                {
                    Text = "TOP-LZ",
                    Width = wideSize.Width,
                    Height = wideSize.Height,
                    Foreground = Colors.Black,
                    BackgroundColor = backgroundColor,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top,
                    TextAlignment = TextAlignment.Start,
                    VerticalTextAlignment = TextAlignment.Start,
                },
                Dock.Top);

            pageLayout.Children.Add(
                new TextBlock()
                {
                    Text = "TOP-R",
                    Foreground = Colors.Black,
                    BackgroundColor = backgroundColor,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    VerticalAlignment = VerticalAlignment.Top,
                    TextAlignment = TextAlignment.End,
                    VerticalTextAlignment = TextAlignment.End,
                },
                Dock.Top);

            pageLayout.Children.Add(
                new TextBlock()
                {
                    Text = "TOP-RZ",
                    Width = wideSize.Width,
                    Height = wideSize.Height,
                    Foreground = Colors.Black,
                    BackgroundColor = backgroundColor,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    VerticalAlignment = VerticalAlignment.Top,
                    TextAlignment = TextAlignment.End,
                    VerticalTextAlignment = TextAlignment.End,
                },
                Dock.Top);

            pageLayout.Children.Add(
                new TextBlock()
                {
                    Text = "TOP-C",
                    Foreground = Colors.Black,
                    BackgroundColor = backgroundColor,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Top,
                    TextAlignment = TextAlignment.Center,
                    VerticalTextAlignment = TextAlignment.Center,
                },
                Dock.Top);

            pageLayout.Children.Add(
                new TextBlock()
                {
                    Text = "TOP-CZ",
                    Width = wideSize.Width,
                    Height = wideSize.Height,
                    Foreground = Colors.Black,
                    BackgroundColor = backgroundColor,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Top,
                    TextAlignment = TextAlignment.Center,
                    VerticalTextAlignment = TextAlignment.Center,
                },
                Dock.Top);

            pageLayout.Children.Add(
                new TextBlock()
                {
                    Text = "TOP-F",
                    Foreground = Colors.Black,
                    BackgroundColor = backgroundColor,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Top,
                    TextAlignment = TextAlignment.Center,
                    VerticalTextAlignment = TextAlignment.End,
                },
                Dock.Top);

            pageLayout.Children.Add(
                new TextBlock()
                {
                    Text = "TOP-FZ",
                    Width = wideSize.Width,
                    Height = wideSize.Height,
                    Foreground = Colors.Black,
                    BackgroundColor = backgroundColor,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Top,
                    TextAlignment = TextAlignment.Center,
                    VerticalTextAlignment = TextAlignment.End,
                },
                Dock.Top);

            pageLayout.Children.Add(
                new TextBlock()
                {
                    Text = "BOTTOM-L",
                    Foreground = Colors.Black,
                    BackgroundColor = backgroundColor,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top,
                    TextAlignment = TextAlignment.Start,
                    VerticalTextAlignment = TextAlignment.Start,
                },
                Dock.Bottom);

            pageLayout.Children.Add(
                new TextBlock()
                {
                    Text = "BOTTOM-LZ",
                    Width = wideSize.Width,
                    Height = wideSize.Height,
                    Foreground = Colors.Black,
                    BackgroundColor = backgroundColor,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top,
                    TextAlignment = TextAlignment.Start,
                    VerticalTextAlignment = TextAlignment.Start,
                },
                Dock.Bottom);

            pageLayout.Children.Add(
                new TextBlock()
                {
                    Text = "BOTTOM-R",
                    Foreground = Colors.Black,
                    BackgroundColor = backgroundColor,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    VerticalAlignment = VerticalAlignment.Top,
                    TextAlignment = TextAlignment.End,
                    VerticalTextAlignment = TextAlignment.Start,
                },
                Dock.Bottom);

            pageLayout.Children.Add(
                new TextBlock()
                {
                    Text = "BOTTOM-RZ",
                    Width = wideSize.Width,
                    Height = wideSize.Height,
                    Foreground = Colors.Black,
                    BackgroundColor = backgroundColor,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    VerticalAlignment = VerticalAlignment.Top,
                    TextAlignment = TextAlignment.End,
                    VerticalTextAlignment = TextAlignment.Start,
                },
                Dock.Bottom);

            pageLayout.Children.Add(
                new TextBlock()
                {
                    Text = "BOTTOM-C",
                    Foreground = Colors.Black,
                    BackgroundColor = backgroundColor,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Top,
                    TextAlignment = TextAlignment.Center,
                    VerticalTextAlignment = TextAlignment.Start,
                },
                Dock.Bottom);

            pageLayout.Children.Add(
                new TextBlock()
                {
                    Text = "BOTTOM-CZ",
                    Width = wideSize.Width,
                    Height = wideSize.Height,
                    Foreground = Colors.Black,
                    BackgroundColor = backgroundColor,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Top,
                    TextAlignment = TextAlignment.Center,
                    VerticalTextAlignment = TextAlignment.Start,
                },
                Dock.Bottom);

            pageLayout.Children.Add(
                new TextBlock()
                {
                    Text = "BOTTOM-F",
                    Foreground = Colors.Black,
                    BackgroundColor = backgroundColor,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Top,
                    TextAlignment = TextAlignment.Center,
                    VerticalTextAlignment = TextAlignment.End,
                },
                Dock.Bottom);

            pageLayout.Children.Add(
                new TextBlock()
                {
                    Text = "BOTTOM-FZ",
                    Width = wideSize.Width,
                    Height = wideSize.Height,
                    Foreground = Colors.Black,
                    BackgroundColor = backgroundColor,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Top,
                    TextAlignment = TextAlignment.Center,
                    VerticalTextAlignment = TextAlignment.End,
                },
                Dock.Bottom);


            pageLayout.Children.Add(
                new TextBlock()
                {
                    Text = "L-T",
                    Foreground = Colors.Black,
                    BackgroundColor = backgroundColor,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top,
                    TextAlignment = TextAlignment.Start,
                    VerticalTextAlignment = TextAlignment.Start,
                },
                Dock.Left);

            pageLayout.Children.Add(
                new TextBlock()
                {
                    Text = "L-TZ",
                    Width = tallSize.Width,
                    Height = tallSize.Height,
                    Foreground = Colors.Black,
                    BackgroundColor = backgroundColor,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top,
                    TextAlignment = TextAlignment.Start,
                    VerticalTextAlignment = TextAlignment.Start,
                },
                Dock.Left);

            pageLayout.Children.Add(
                new TextBlock()
                {
                    Text = "L-B",
                    Foreground = Colors.Black,
                    BackgroundColor = backgroundColor,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Bottom,
                    TextAlignment = TextAlignment.Start,
                    VerticalTextAlignment = TextAlignment.End,
                },
                Dock.Left);

            pageLayout.Children.Add(
                new TextBlock()
                {
                    Text = "L-BZ",
                    Width = tallSize.Width,
                    Height = tallSize.Height,
                    Foreground = Colors.Black,
                    BackgroundColor = backgroundColor,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Bottom,
                    TextAlignment = TextAlignment.Start,
                    VerticalTextAlignment = TextAlignment.End,
                },
                Dock.Left);

            pageLayout.Children.Add(
                new TextBlock()
                {
                    Text = "L-C",
                    Foreground = Colors.Black,
                    BackgroundColor = backgroundColor,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Center,
                    TextAlignment = TextAlignment.Start,
                    VerticalTextAlignment = TextAlignment.Center,
                },
                Dock.Left);

            pageLayout.Children.Add(
                new TextBlock()
                {
                    Text = "L-CZ",
                    Width = tallSize.Width,
                    Height = tallSize.Height,
                    Foreground = Colors.Black,
                    BackgroundColor = backgroundColor,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Center,
                    TextAlignment = TextAlignment.Start,
                    VerticalTextAlignment = TextAlignment.Center,
                },
                Dock.Left);

            pageLayout.Children.Add(
                new TextBlock()
                {
                    Text = "L-F",
                    Foreground = Colors.Black,
                    BackgroundColor = backgroundColor,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Stretch,
                    TextAlignment = TextAlignment.Start,
                    VerticalTextAlignment = TextAlignment.Center,
                },
                Dock.Left);

            pageLayout.Children.Add(
                new TextBlock()
                {
                    Text = "L-FZ",
                    Width = tallSize.Width,
                    Height = tallSize.Height,
                    Foreground = Colors.Black,
                    BackgroundColor = backgroundColor,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Stretch,
                    TextAlignment = TextAlignment.Start,
                    VerticalTextAlignment = TextAlignment.Center,
                },
                Dock.Left);

            pageLayout.Children.Add(
                new TextBlock()
                {
                    Text = "R-T",
                    Foreground = Colors.Black,
                    BackgroundColor = backgroundColor,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top,
                    TextAlignment = TextAlignment.Start,
                    VerticalTextAlignment = TextAlignment.Start,
                },
                Dock.Right);

            pageLayout.Children.Add(
                new TextBlock()
                {
                    Text = "R-TZ",
                    Width = tallSize.Width,
                    Height = tallSize.Height,
                    Foreground = Colors.Black,
                    BackgroundColor = backgroundColor,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top,
                    TextAlignment = TextAlignment.Start,
                    VerticalTextAlignment = TextAlignment.Start,
                },
                Dock.Right);

            pageLayout.Children.Add(
                new TextBlock()
                {
                    Text = "R-B",
                    Foreground = Colors.Black,
                    BackgroundColor = backgroundColor,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Bottom,
                    TextAlignment = TextAlignment.Start,
                    VerticalTextAlignment = TextAlignment.End,
                },
                Dock.Right);

            pageLayout.Children.Add(
                new TextBlock()
                {
                    Text = "R-BZ",
                    Width = tallSize.Width,
                    Height = tallSize.Height,
                    Foreground = Colors.Black,
                    BackgroundColor = backgroundColor,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Bottom,
                    TextAlignment = TextAlignment.Start,
                    VerticalTextAlignment = TextAlignment.End,
                },
                Dock.Right);

            pageLayout.Children.Add(
                new TextBlock()
                {
                    Text = "R-C",
                    Foreground = Colors.Black,
                    BackgroundColor = backgroundColor,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Center,
                    TextAlignment = TextAlignment.Start,
                    VerticalTextAlignment = TextAlignment.Center,
                },
                Dock.Right);

            pageLayout.Children.Add(
                new TextBlock()
                {
                    Text = "R-CZ",
                    Width = tallSize.Width,
                    Height = tallSize.Height,
                    Foreground = Colors.Black,
                    BackgroundColor = backgroundColor,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Center,
                    TextAlignment = TextAlignment.Start,
                    VerticalTextAlignment = TextAlignment.Center,
                },
                Dock.Right);

            pageLayout.Children.Add(
                new TextBlock()
                {
                    Text = "R-F",
                    Foreground = Colors.Black,
                    BackgroundColor = backgroundColor,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Stretch,
                    TextAlignment = TextAlignment.Start,
                    VerticalTextAlignment = TextAlignment.Center,
                },
                Dock.Right);

            pageLayout.Children.Add(
                new TextBlock()
                {
                    Text = "R-FZ",
                    Width = tallSize.Width,
                    Height = tallSize.Height,
                    Foreground = Colors.Black,
                    BackgroundColor = backgroundColor,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Stretch,
                    TextAlignment = TextAlignment.Start,
                    VerticalTextAlignment = TextAlignment.Center,
                },
                Dock.Right);

            var image = new Image()
            {
                Source = ThemeResources.Default.AboutLogo,
            };
            pageLayout.Children.Add(image, Dock.CenterOverlay);

            this.Content = pageLayout;
        }
    }
}
