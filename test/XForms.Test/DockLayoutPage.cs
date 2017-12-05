using System;
using XForms.Controls;
using XForms.Layouts;

namespace XForms.Test
{
    public class DockLayoutPage : Page
    {
        public DockLayoutPage()
        {
            Color backgroundColor = Color.FromArgb(0x80, 0xff, 0, 0);
            Size wideSize = new Size(150, Dimension.Auto);
            Size tallSize = new Size(Dimension.Auto, 100);

            var pageLayout = new DockLayout()
            {
                //Size = new Size(300),
                //Size = new Size(Dimension.Auto, Dimension.Auto),
                //HorizontalAlignment = LayoutAlignment.Fill,
                //VerticalAlignment = LayoutAlignment.Fill,
            };

            pageLayout.Children.Add(
                new TextView()
                {
                    Text = "TOP-L",
                    ForegroundColor = Colors.Black,
                    BackgroundColor = backgroundColor,
                    HorizontalAlignment = LayoutAlignment.Start,
                    VerticalAlignment = LayoutAlignment.Start,
                    HorizontalTextAlignment = TextAlignment.Start,
                    VerticalTextAlignment = TextAlignment.Start,
                },
                DockRegion.Top);

            pageLayout.Children.Add(
                new TextView()
                {
                    Text = "TOP-LX",
                    Size = wideSize,
                    ForegroundColor = Colors.Black,
                    BackgroundColor = backgroundColor,
                    HorizontalAlignment = LayoutAlignment.Start,
                    VerticalAlignment = LayoutAlignment.Start,
                    HorizontalTextAlignment = TextAlignment.Start,
                    VerticalTextAlignment = TextAlignment.Start,
                },
                DockRegion.Top);

            pageLayout.Children.Add(
                new TextView()
                {
                    Text = "TOP-R",
                    ForegroundColor = Colors.Black,
                    BackgroundColor = backgroundColor,
                    HorizontalAlignment = LayoutAlignment.End,
                    VerticalAlignment = LayoutAlignment.Start,
                    HorizontalTextAlignment = TextAlignment.End,
                    VerticalTextAlignment = TextAlignment.End,
                },
                DockRegion.Top);

            pageLayout.Children.Add(
                new TextView()
                {
                    Text = "TOP-RX",
                    Size = wideSize,
                    ForegroundColor = Colors.Black,
                    BackgroundColor = backgroundColor,
                    HorizontalAlignment = LayoutAlignment.End,
                    VerticalAlignment = LayoutAlignment.Start,
                    HorizontalTextAlignment = TextAlignment.End,
                    VerticalTextAlignment = TextAlignment.End,
                },
                DockRegion.Top);

            pageLayout.Children.Add(
                new TextView()
                {
                    Text = "TOP-C",
                    ForegroundColor = Colors.Black,
                    BackgroundColor = backgroundColor,
                    HorizontalAlignment = LayoutAlignment.Center,
                    VerticalAlignment = LayoutAlignment.Start,
                    HorizontalTextAlignment = TextAlignment.Center,
                    VerticalTextAlignment = TextAlignment.Center,
                },
                DockRegion.Top);

            pageLayout.Children.Add(
                new TextView()
                {
                    Text = "TOP-CX",
                    Size = wideSize,
                    ForegroundColor = Colors.Black,
                    BackgroundColor = backgroundColor,
                    HorizontalAlignment = LayoutAlignment.Center,
                    VerticalAlignment = LayoutAlignment.Start,
                    HorizontalTextAlignment = TextAlignment.Center,
                    VerticalTextAlignment = TextAlignment.Center,
                },
                DockRegion.Top);

            pageLayout.Children.Add(
                new TextView()
                {
                    Text = "TOP-F",
                    ForegroundColor = Colors.Black,
                    BackgroundColor = backgroundColor,
                    HorizontalAlignment = LayoutAlignment.Fill,
                    VerticalAlignment = LayoutAlignment.Start,
                    HorizontalTextAlignment = TextAlignment.Center,
                    VerticalTextAlignment = TextAlignment.End,
                },
                DockRegion.Top);

            pageLayout.Children.Add(
                new TextView()
                {
                    Text = "TOP-FX",
                    Size = wideSize,
                    ForegroundColor = Colors.Black,
                    BackgroundColor = backgroundColor,
                    HorizontalAlignment = LayoutAlignment.Fill,
                    VerticalAlignment = LayoutAlignment.Start,
                    HorizontalTextAlignment = TextAlignment.Center,
                    VerticalTextAlignment = TextAlignment.End,
                },
                DockRegion.Top);

            pageLayout.Children.Add(
                new TextView()
                {
                    Text = "BOTTOM-L",
                    ForegroundColor = Colors.Black,
                    BackgroundColor = backgroundColor,
                    HorizontalAlignment = LayoutAlignment.Start,
                    VerticalAlignment = LayoutAlignment.Start,
                    HorizontalTextAlignment = TextAlignment.Start,
                    VerticalTextAlignment = TextAlignment.Start,
                },
                DockRegion.Bottom);

            pageLayout.Children.Add(
                new TextView()
                {
                    Text = "BOTTOM-LX",
                    Size = wideSize,
                    ForegroundColor = Colors.Black,
                    BackgroundColor = backgroundColor,
                    HorizontalAlignment = LayoutAlignment.Start,
                    VerticalAlignment = LayoutAlignment.Start,
                    HorizontalTextAlignment = TextAlignment.Start,
                    VerticalTextAlignment = TextAlignment.Start,
                },
                DockRegion.Bottom);

            pageLayout.Children.Add(
                new TextView()
                {
                    Text = "BOTTOM-R",
                    ForegroundColor = Colors.Black,
                    BackgroundColor = backgroundColor,
                    HorizontalAlignment = LayoutAlignment.End,
                    VerticalAlignment = LayoutAlignment.Start,
                    HorizontalTextAlignment = TextAlignment.End,
                    VerticalTextAlignment = TextAlignment.Start,
                },
                DockRegion.Bottom);

            pageLayout.Children.Add(
                new TextView()
                {
                    Text = "BOTTOM-RX",
                    Size = wideSize,
                    ForegroundColor = Colors.Black,
                    BackgroundColor = backgroundColor,
                    HorizontalAlignment = LayoutAlignment.End,
                    VerticalAlignment = LayoutAlignment.Start,
                    HorizontalTextAlignment = TextAlignment.End,
                    VerticalTextAlignment = TextAlignment.Start,
                },
                DockRegion.Bottom);

            pageLayout.Children.Add(
                new TextView()
                {
                    Text = "BOTTOM-C",
                    ForegroundColor = Colors.Black,
                    BackgroundColor = backgroundColor,
                    HorizontalAlignment = LayoutAlignment.Center,
                    VerticalAlignment = LayoutAlignment.Start,
                    HorizontalTextAlignment = TextAlignment.Center,
                    VerticalTextAlignment = TextAlignment.Start,
                },
                DockRegion.Bottom);

            pageLayout.Children.Add(
                new TextView()
                {
                    Text = "BOTTOM-CX",
                    Size = wideSize,
                    ForegroundColor = Colors.Black,
                    BackgroundColor = backgroundColor,
                    HorizontalAlignment = LayoutAlignment.Center,
                    VerticalAlignment = LayoutAlignment.Start,
                    HorizontalTextAlignment = TextAlignment.Center,
                    VerticalTextAlignment = TextAlignment.Start,
                },
                DockRegion.Bottom);

			pageLayout.Children.Add(
				new TextView()
				{
					Text = "BOTTOM-F",
					ForegroundColor = Colors.Black,
					BackgroundColor = backgroundColor,
					HorizontalAlignment = LayoutAlignment.Fill,
					VerticalAlignment = LayoutAlignment.Start,
					HorizontalTextAlignment = TextAlignment.Center,
					VerticalTextAlignment = TextAlignment.End,
				},
				DockRegion.Bottom);

			pageLayout.Children.Add(
				new TextView()
				{
					Text = "BOTTOM-FX",
					Size = wideSize,
					ForegroundColor = Colors.Black,
					BackgroundColor = backgroundColor,
					HorizontalAlignment = LayoutAlignment.Fill,
					VerticalAlignment = LayoutAlignment.Start,
					HorizontalTextAlignment = TextAlignment.Center,
					VerticalTextAlignment = TextAlignment.End,
				},
				DockRegion.Bottom);

			/*
						pageLayout.Children.Add(
							new TextView()
							{
								Text = "L-T",
								ForegroundColor = Colors.Black,
								BackgroundColor = backgroundColor,
								HorizontalAlignment = LayoutAlignment.Start,
								VerticalAlignment = LayoutAlignment.Start,
								HorizontalTextAlignment = TextAlignment.Start,
								VerticalTextAlignment = TextAlignment.Start,
							},
							DockRegion.Left);

						pageLayout.Children.Add(
							new TextView()
							{
								Text = "L-TX",
								Size = tallSize,
								ForegroundColor = Colors.Black,
								BackgroundColor = backgroundColor,
								HorizontalAlignment = LayoutAlignment.Start,
								VerticalAlignment = LayoutAlignment.Start,
								HorizontalTextAlignment = TextAlignment.Start,
								VerticalTextAlignment = TextAlignment.Start,
							},
							DockRegion.Left);

						pageLayout.Children.Add(
							new TextView()
							{
								Text = "L-B",
								ForegroundColor = Colors.Black,
								BackgroundColor = backgroundColor,
								HorizontalAlignment = LayoutAlignment.Start,
								VerticalAlignment = LayoutAlignment.End,
								HorizontalTextAlignment = TextAlignment.Start,
								VerticalTextAlignment = TextAlignment.End,
							},
							DockRegion.Left);

						pageLayout.Children.Add(
							new TextView()
							{
								Text = "L-BX",
								Size = tallSize,
								ForegroundColor = Colors.Black,
								BackgroundColor = backgroundColor,
								HorizontalAlignment = LayoutAlignment.Start,
								VerticalAlignment = LayoutAlignment.End,
								HorizontalTextAlignment = TextAlignment.Start,
								VerticalTextAlignment = TextAlignment.End,
							},
							DockRegion.Left);

						pageLayout.Children.Add(
							new TextView()
							{
								Text = "L-C",
								ForegroundColor = Colors.Black,
								BackgroundColor = backgroundColor,
								HorizontalAlignment = LayoutAlignment.Start,
								VerticalAlignment = LayoutAlignment.Center,
								HorizontalTextAlignment = TextAlignment.Start,
								VerticalTextAlignment = TextAlignment.Center,
							},
							DockRegion.Left);

						pageLayout.Children.Add(
							new TextView()
							{
								Text = "L-CX",
								Size = tallSize,
								ForegroundColor = Colors.Black,
								BackgroundColor = backgroundColor,
								HorizontalAlignment = LayoutAlignment.Start,
								VerticalAlignment = LayoutAlignment.Center,
								HorizontalTextAlignment = TextAlignment.Start,
								VerticalTextAlignment = TextAlignment.Center,
							},
							DockRegion.Left);
			*/
			pageLayout.Children.Add(
			    new TextView()
			    {
			        Text = "L-F",
			        ForegroundColor = Colors.Black,
			        BackgroundColor = backgroundColor,
			        HorizontalAlignment = LayoutAlignment.Start,
			        VerticalAlignment = LayoutAlignment.Fill,
			        HorizontalTextAlignment = TextAlignment.Start,
			        VerticalTextAlignment = TextAlignment.Center,
			    },
			    DockRegion.Left);

			pageLayout.Children.Add(
			    new TextView()
			    {
			        Text = "L-FX",
					Size = tallSize,
					ForegroundColor = Colors.Black,
			        BackgroundColor = backgroundColor,
			        HorizontalAlignment = LayoutAlignment.Start,
			        VerticalAlignment = LayoutAlignment.Fill,
			        HorizontalTextAlignment = TextAlignment.Start,
			        VerticalTextAlignment = TextAlignment.Center,
			    },
			    DockRegion.Left);
/*
			pageLayout.Children.Add(
				new TextView()
				{
					Text = "R-T",
					ForegroundColor = Colors.Black,
					BackgroundColor = backgroundColor,
					HorizontalAlignment = LayoutAlignment.Start,
					VerticalAlignment = LayoutAlignment.Start,
					HorizontalTextAlignment = TextAlignment.Start,
					VerticalTextAlignment = TextAlignment.Start,
				},
				DockRegion.Right);

			pageLayout.Children.Add(
				new TextView()
				{
					Text = "R-TX",
					Size = tallSize,
					ForegroundColor = Colors.Black,
					BackgroundColor = backgroundColor,
					HorizontalAlignment = LayoutAlignment.Start,
					VerticalAlignment = LayoutAlignment.Start,
					HorizontalTextAlignment = TextAlignment.Start,
					VerticalTextAlignment = TextAlignment.Start,
				},
				DockRegion.Right);

			pageLayout.Children.Add(
				new TextView()
				{
					Text = "R-B",
					ForegroundColor = Colors.Black,
					BackgroundColor = backgroundColor,
					HorizontalAlignment = LayoutAlignment.Start,
					VerticalAlignment = LayoutAlignment.End,
					HorizontalTextAlignment = TextAlignment.Start,
					VerticalTextAlignment = TextAlignment.End,
				},
				DockRegion.Right);

			pageLayout.Children.Add(
				new TextView()
				{
					Text = "R-BX",
					Size = tallSize,
					ForegroundColor = Colors.Black,
					BackgroundColor = backgroundColor,
					HorizontalAlignment = LayoutAlignment.Start,
					VerticalAlignment = LayoutAlignment.End,
					HorizontalTextAlignment = TextAlignment.Start,
					VerticalTextAlignment = TextAlignment.End,
				},
				DockRegion.Right);

			pageLayout.Children.Add(
				new TextView()
				{
					Text = "R-C",
					ForegroundColor = Colors.Black,
					BackgroundColor = backgroundColor,
					HorizontalAlignment = LayoutAlignment.Start,
					VerticalAlignment = LayoutAlignment.Center,
					HorizontalTextAlignment = TextAlignment.Start,
					VerticalTextAlignment = TextAlignment.Center,
				},
				DockRegion.Right);

			pageLayout.Children.Add(
				new TextView()
				{
					Text = "R-CX",
					Size = tallSize,
					ForegroundColor = Colors.Black,
					BackgroundColor = backgroundColor,
					HorizontalAlignment = LayoutAlignment.Start,
					VerticalAlignment = LayoutAlignment.Center,
					HorizontalTextAlignment = TextAlignment.Start,
					VerticalTextAlignment = TextAlignment.Center,
				},
				DockRegion.Right);
*/            
			pageLayout.Children.Add(
			    new TextView()
			    {
			        Text = "R-F",
			        ForegroundColor = Colors.Black,
			        BackgroundColor = backgroundColor,
			        HorizontalAlignment = LayoutAlignment.Start,
			        VerticalAlignment = LayoutAlignment.Fill,
			        HorizontalTextAlignment = TextAlignment.Start,
			        VerticalTextAlignment = TextAlignment.Center,
			    },
			    DockRegion.Right);

			pageLayout.Children.Add(
			    new TextView()
			    {
			        Text = "R-FX",
			        ForegroundColor = Colors.Black,
			        BackgroundColor = backgroundColor,
			        HorizontalAlignment = LayoutAlignment.Start,
			        VerticalAlignment = LayoutAlignment.Fill,
			        HorizontalTextAlignment = TextAlignment.Start,
			        VerticalTextAlignment = TextAlignment.Center,
			    },
			    DockRegion.Right);

			var image = new Image()
			{
			    Source = ThemeResources.Default.AboutLogo,
			};
			pageLayout.Children.Add(image, DockRegion.CenterOverlay);

			this.Layout = pageLayout;
        }
    }
}
