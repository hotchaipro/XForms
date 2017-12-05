using System;
using XForms.Controls;
using XForms.Input;
using XForms.Layouts;

namespace XForms.Test
{
    public class ControlPage : Page, ITapGestureDelegate
    {
        public ControlPage()
        {
            var pageLayout = new DockLayout()
            {
            };

            var contentLayout = new DockLayout()
            {
            };

            contentLayout.Children.Add(
                new TextView()
                {
                    Text = "TOP1",
                    ForegroundColor = Colors.Red,
                    HorizontalAlignment = LayoutAlignment.Fill,
                    VerticalAlignment = LayoutAlignment.Start,
                    HorizontalTextAlignment = TextAlignment.Start,
                },
                DockRegion.Top);

            contentLayout.Children.Add(
                new TextEntry()
                {
                    PlaceholderText = "Placeholder",
                    HorizontalAlignment = LayoutAlignment.Fill,
                    VerticalAlignment = LayoutAlignment.Start,
                    //Text = "Hello, TextEntry",
                    //Size = new Size(200, 60),
                },
                DockRegion.Top);

            contentLayout.Children.Add(
                new DaysOfWeekPicker()
                {
                    HorizontalAlignment = LayoutAlignment.Center,
                },
                DockRegion.Top);

            contentLayout.Children.Add(
                new ProgressRing()
                {
                },
                DockRegion.Top);

            var listPicker = new ListPicker();
            listPicker.Items.Add(new ListPickerItem("Item One"));
            listPicker.Items.Add(new ListPickerItem("Item Two"));
            listPicker.Items.Add(new ListPickerItem("Item Three"));
            contentLayout.Children.Add(listPicker, DockRegion.Top);

            contentLayout.Children.Add(
                new CalendarControl()
                {
                },
                DockRegion.Top);

            contentLayout.Children.Add(
                new CalendarDatePicker()
                {
                },
                DockRegion.Top);

            contentLayout.Children.Add(
                new TextView()
                {
                    Text = "TOP2",
                    ForegroundColor = Colors.Red,
                    HorizontalAlignment = LayoutAlignment.Fill,
                    VerticalAlignment = LayoutAlignment.Start,
                    HorizontalTextAlignment = TextAlignment.Start,
                },
                DockRegion.Top);

            for (int i = 0; i < 50; i++)
            {
                contentLayout.Children.Add(
                    new TextView()
                    {
                        Text = "MORE",
                        ForegroundColor = Colors.Red,
                        HorizontalAlignment = LayoutAlignment.Fill,
                        VerticalAlignment = LayoutAlignment.Start,
                        HorizontalTextAlignment = TextAlignment.Start,
                    },
                    DockRegion.Top);
            }

            var scrollView = new ScrollView()
            {
                IsHorizontalScrollingEnabled = false,
                IsVerticalScrollingEnabled = true,
            };
            scrollView.Content = contentLayout;
            pageLayout.Children.Add(scrollView, DockRegion.CenterOverlay);

            this.Layout = pageLayout;
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
