using System;
using XForms.Controls;
using XForms.Layouts;

namespace XForms.Test
{
    /// <summary>
    /// The panels (StackPanel, Grid, WrapPanel, Canvas, Border) and the view
    /// basics: padding, minimum sizes, z-order, clipping and right to left.
    /// </summary>
    public class PanelsPage : Page
    {
        private static readonly Color Blue = Color.FromArgb(0xFF2196F3);
        private static readonly Color Green = Color.FromArgb(0xFF4CAF50);
        private static readonly Color Orange = Color.FromArgb(0xFFFF9800);
        private static readonly Color Purple = Color.FromArgb(0xFF9C27B0);
        private static readonly Color Gray = Color.FromArgb(0xFFE0E0E0);

        public PanelsPage()
        {
            var sections = new StackPanel()
            {
                Spacing = 16,
                Padding = new Thickness(16),
            };

            // StackPanel: horizontal, with spacing
            sections.Children.Add(Heading("StackPanel (horizontal, spacing 8)"));
            var stack = new StackPanel() { Orientation = Orientation.Horizontal, Spacing = 8 };
            stack.Children.Add(Box(Blue, 60, 40, "1"));
            stack.Children.Add(Box(Green, 80, 40, "2"));
            stack.Children.Add(Box(Orange, 40, 40, "3"));
            sections.Children.Add(stack);

            // Grid: Auto, star and fixed columns, a column span, spacing
            sections.Children.Add(Heading("Grid (Auto, *, 2*, 60; spans)"));
            var grid = new Grid() { RowSpacing = 4, ColumnSpacing = 4, Height = 100 };
            grid.ColumnDefinitions.AddRange("Auto,*,2*,60");
            grid.RowDefinitions.AddRange("Auto,*");
            grid.Children.Add(Label("Auto"), 0, 0);
            grid.Children.Add(Box(Blue, float.NaN, float.NaN, "*"), 0, 1);
            grid.Children.Add(Box(Green, float.NaN, float.NaN, "2*"), 0, 2);
            grid.Children.Add(Box(Orange, float.NaN, float.NaN, "60"), 0, 3);
            grid.Children.Add(Box(Purple, float.NaN, float.NaN, "row 1, columns 0-2"), 1, 0, columnSpan: 3);
            grid.Children.Add(Box(Gray, float.NaN, float.NaN, ""), 1, 3);
            sections.Children.Add(grid);

            // WrapPanel
            sections.Children.Add(Heading("WrapPanel (item 6, line 6)"));
            var wrap = new WrapPanel() { ItemSpacing = 6, LineSpacing = 6 };
            for (int i = 1; i <= 14; i += 1)
            {
                wrap.Children.Add(Box((i % 2 == 0) ? Blue : Green, 30 + ((i * 17) % 50), 30, i.ToString()));
            }

            sections.Children.Add(wrap);

            // Canvas with z-order, and clipping
            sections.Children.Add(Heading("Canvas: ZIndex (orange on top), clipped"));
            var canvas = new Canvas() { Height = 90, ClipToBounds = true, Width = 260, HorizontalAlignment = HorizontalAlignment.Left };
            var top = Box(Orange, 80, 60, "Z 1");
            top.ZIndex = 1;
            canvas.Children.Add(Box(Gray, 260, 90, ""), 0, 0);
            canvas.Children.Add(top, 30, 10);
            canvas.Children.Add(Box(Blue, 80, 60, "Z 0"), 70, 25);
            canvas.Children.Add(Box(Purple, 80, 60, "clipped"), 210, 50);
            sections.Children.Add(canvas);

            // Border: rounded, uniform and per-side widths, padding
            sections.Children.Add(Heading("Border (radius 12, padding 12; per-side widths)"));
            var borders = new StackPanel() { Orientation = Orientation.Horizontal, Spacing = 12 };
            borders.Children.Add(new Border()
            {
                Background = Color.FromArgb(0xFFE3F2FD),
                BorderBrush = Blue,
                BorderThickness = new Thickness(2),
                CornerRadius = 12,
                Padding = new Thickness(12),
                Child = Label("Rounded"),
            });
            borders.Children.Add(new Border()
            {
                BorderBrush = Green,
                BorderThickness = new Thickness(1, 1, 1, 6),
                Padding = new Thickness(12, 6),
                Child = Label("Per side"),
            });
            sections.Children.Add(borders);

            // Minimum size
            sections.Children.Add(Heading("MinWidth 160 (content is narrower)"));
            var min = Box(Green, float.NaN, 30, "min");
            min.MinWidth = 160;
            min.HorizontalAlignment = HorizontalAlignment.Left;
            sections.Children.Add(min);

            // Right to left
            sections.Children.Add(Heading("FlowDirection RightToLeft (1 is on the right)"));
            var rtl = new StackPanel() { Orientation = Orientation.Horizontal, Spacing = 8, FlowDirection = FlowDirection.RightToLeft };
            rtl.Children.Add(Box(Blue, 60, 30, "1"));
            rtl.Children.Add(Box(Green, 60, 30, "2"));
            rtl.Children.Add(Box(Orange, 60, 30, "3"));
            sections.Children.Add(rtl);

            var scroll = new ScrollViewer() { Content = sections };
            var layout = new DockPanel();
            layout.Children.Add(scroll, Dock.CenterOverlay);
            this.Content = layout;
        }

        private static TextBlock Heading(string text)
        {
            return new TextBlock()
            {
                Text = text,
                FontSize = 14,
                FontWeight = FontWeight.SemiBold,
                HorizontalAlignment = HorizontalAlignment.Left,
            };
        }

        private static TextBlock Label(string text)
        {
            return new TextBlock()
            {
                Text = text,
                FontSize = 14,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            };
        }

        private static Border Box(Color color, float width, float height, string text)
        {
            return new Border()
            {
                Background = color,
                Width = width,
                Height = height,
                Child = new TextBlock()
                {
                    Text = text,
                    FontSize = 12,
                    Foreground = Colors.White,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                },
            };
        }
    }
}
