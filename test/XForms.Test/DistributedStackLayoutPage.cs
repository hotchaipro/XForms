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
                HorizontalAlignment = LayoutAlignment.Fill,
                //Size = new Size(Dimension.Auto, 60),
                //Size = new Size(200, 60),
            };

            stackLayout.Children.Add(
                new TextView()
                {
                    //Size = new Size(Dimension.FitToContent),
                    HorizontalAlignment = LayoutAlignment.Center,
                    //HorizontalTextAlignment = TextAlignment.Center,
                    Text = "ITEM1",
                    ForegroundColor = Colors.White,
                    BackgroundColor = Colors.Red,
                });

            stackLayout.Children.Add(
                new TextView()
                {
                    //Size = new Size(Dimension.FitToContent),
                    HorizontalAlignment = LayoutAlignment.Center,
                    //HorizontalTextAlignment = TextAlignment.Center,
                    Text = "ITEM2",
                    ForegroundColor = Colors.White,
                    BackgroundColor = Colors.Red,
                });

            //stackLayout.Children.Add(
            //new Image()
            //{
            //    Source = ThemeResources.Default.AboutLogo,
            //    HorizontalAlignment = LayoutAlignment.Center,
            //});

            var pageLayout = new DockLayout()
            {
			};

			pageLayout.Children.Add(stackLayout, DockRegion.Top);

			this.Layout = pageLayout;
        }
    }
}
