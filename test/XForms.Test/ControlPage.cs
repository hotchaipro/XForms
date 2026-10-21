using System;
using XForms.Controls;
using XForms.Toolkit;
using XForms.Input;
using XForms.Layouts;

namespace XForms.Test
{
    public class ControlPage : Page, ITapGestureDelegate
    {
        public ControlPage()
        {
            var pageLayout = new DockPanel()
            {
            };

            var contentLayout = new DockPanel()
            {
            };

            contentLayout.Children.Add(
                new TextBlock()
                {
                    Text = "TOP1",
                    Foreground = Colors.Red,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Top,
                    TextAlignment = TextAlignment.Start,
                },
                Dock.Top);

            contentLayout.Children.Add(
                new TextBox()
                {
                    PlaceholderText = "Placeholder",
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Top,
                    //Text = "Hello, TextEntry",
                    //Size = new Size(200, 60),
                },
                Dock.Top);

            contentLayout.Children.Add(
                new DaysOfWeekPicker()
                {
                    HorizontalAlignment = HorizontalAlignment.Center,
                },
                Dock.Top);

            contentLayout.Children.Add(
                new ProgressRing()
                {
                },
                Dock.Top);

            var listPicker = new ComboBox();
            listPicker.Items.Add(new ComboBoxItem("Item One"));
            listPicker.Items.Add(new ComboBoxItem("Item Two"));
            listPicker.Items.Add(new ComboBoxItem("Item Three"));
            contentLayout.Children.Add(listPicker, Dock.Top);

            contentLayout.Children.Add(
                new CalendarView()
                {
                },
                Dock.Top);

            contentLayout.Children.Add(
                new CalendarDatePicker()
                {
                },
                Dock.Top);

            contentLayout.Children.Add(
                new TextBlock()
                {
                    Text = "TOP2",
                    Foreground = Colors.Red,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Top,
                    TextAlignment = TextAlignment.Start,
                },
                Dock.Top);

            for (int i = 0; i < 50; i++)
            {
                contentLayout.Children.Add(
                    new TextBlock()
                    {
                        Text = "MORE",
                        Foreground = Colors.Red,
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                        VerticalAlignment = VerticalAlignment.Top,
                        TextAlignment = TextAlignment.Start,
                    },
                    Dock.Top);
            }

            var scrollView = new ScrollViewer()
            {
                IsHorizontalScrollingEnabled = false,
                IsVerticalScrollingEnabled = true,
            };
            scrollView.Content = contentLayout;
            pageLayout.Children.Add(scrollView, Dock.CenterOverlay);

            this.Content = pageLayout;
        }

        void ITapGestureDelegate.OnTapBegan()
        {
        }

        void ITapGestureDelegate.OnTapEnded()
        {
        }

        void ITapGestureDelegate.OnTapped()
        {
        }

        void IGestureRecognizerDelegate.OnTouchBegan()
        {
        }

        void IGestureRecognizerDelegate.OnTouchCanceled()
        {
        }

        void IGestureRecognizerDelegate.OnTouchEnded()
        {
        }
    }
}
