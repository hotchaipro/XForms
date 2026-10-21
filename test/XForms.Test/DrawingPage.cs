using System;
using System.IO;
using System.Numerics;
using XForms.Controls;
using XForms.Layouts;
using XForms.Primitives;

namespace XForms.Test
{
    /// <summary>
    /// Drawing and text: gradients, paths, clips, transforms and group opacity,
    /// text layouts, and text blocks with runs, font families and line limits.
    /// </summary>
    public class DrawingPage : Page
    {
        public DrawingPage()
        {
            var layout = new StackPanel()
            {
                Spacing = 12,
                Padding = new Thickness(16),
            };

            layout.Children.Add(new Shapes() { Height = 230, HorizontalAlignment = HorizontalAlignment.Stretch });

            // Text blocks: runs with their own styles
            var runs = new TextBlock() { FontSize = 16, TextWrapping = TextWrapping.Wrap };
            runs.Inlines.Add(new Run("Runs: "));
            runs.Inlines.Add(new Run("bold") { FontWeight = FontWeight.Bold });
            runs.Inlines.Add(new Run(", "));
            runs.Inlines.Add(new Run("italic") { FontStyle = FontStyle.Italic });
            runs.Inlines.Add(new Run(", "));
            runs.Inlines.Add(new Run("blue") { Foreground = Color.FromArgb(0xFF2196F3) });
            runs.Inlines.Add(new Run(", "));
            runs.Inlines.Add(new Run("underlined") { Underline = true });
            runs.Inlines.Add(new Run(" and "));
            runs.Inlines.Add(new Run("struck") { Strikethrough = true });
            layout.Children.Add(runs);

            // Generic families: a matching system font on each platform
            layout.Children.Add(new TextBlock() { Text = "Serif: The quick brown fox", FontSize = 16, FontFamily = FontFamilies.Serif });
            layout.Children.Add(new TextBlock() { Text = "Monospace: 0O1lI|", FontSize = 16, FontFamily = FontFamilies.Monospace });
            layout.Children.Add(new TextBlock() { Text = "Sans-serif: The quick brown fox", FontSize = 16, FontFamily = FontFamilies.SansSerif });

            // A font registered from a file (on Windows, a copy of an installed
            // font under another name, so it can only come from the file)
            string bundled = RegisterTestFont(this.Application);
            layout.Children.Add(new TextBlock()
            {
                Text = (null != bundled) ? "A registered font file (\"BundledTest\")" : "No font file to register on this platform",
                FontSize = 16,
                FontFamily = bundled,
            });

            // Line limits
            layout.Children.Add(new TextBlock()
            {
                Text = "MaxLines 2: XForms lays out text with each platform's own text engine, so this long sentence wraps onto more lines than it may show, and ends with an ellipsis where it's cut off.",
                FontSize = 14,
                TextWrapping = TextWrapping.Wrap,
                MaxLines = 2,
                MaxWidth = 360,
                HorizontalAlignment = HorizontalAlignment.Left,
            });

            var page = new DockPanel();
            page.Children.Add(layout, Dock.CenterOverlay);
            this.Content = page;
        }

        private static string RegisterTestFont(
            Application application)
        {
            if (!OperatingSystem.IsWindows())
            {
                return null;
            }

            string source = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "consola.ttf");
            if (!File.Exists(source))
            {
                return null;
            }

            string copy = Path.Combine(Path.GetTempPath(), "xforms-bundled-test.ttf");
            File.Copy(source, copy, overwrite: true);
            application.RegisterFont("BundledTest", copy);
            return "BundledTest";
        }

        // Shapes and text drawn with a DrawingContext
        private sealed class Shapes : UserControl
        {
            public override Color BackgroundColor => Colors.Transparent;

            protected override void Render(
                DrawingContext context)
            {
                context.Clear(Colors.Transparent);

                // A gradient
                var gradient = new LinearGradientBrush(new Point(0, 0), new Point(1, 1),
                    new GradientStop(Color.FromArgb(0xFF2196F3), 0),
                    new GradientStop(Color.FromArgb(0xFF9C27B0), 1));
                context.FillRoundedRectangle(new Rect(0, 0, 100, 100), 16, gradient);

                // A path: a star, filled and outlined
                var star = new PathGeometry();
                for (int i = 0; i < 10; i += 1)
                {
                    double angle = (Math.PI / 5 * i) - (Math.PI / 2);
                    float radius = (i % 2 == 0) ? 48 : 20;
                    var point = new Point(170 + (float)(radius * Math.Cos(angle)), 50 + (float)(radius * Math.Sin(angle)));
                    if (i == 0) star.MoveTo(point); else star.LineTo(point);
                }

                star.Close();
                context.FillGeometry(star, LinearGradientBrush.Vertical(Color.FromArgb(0xFFFFC107), Color.FromArgb(0xFFFF5722)));
                context.DrawGeometry(star, Color.FromArgb(0xFF795548), 2);

                // A path with an arc and a curve
                var wave = new PathGeometry()
                    .MoveTo(new Point(230, 80))
                    .ArcTo(new Point(290, 80), 30, 30, false, true)
                    .CubicTo(new Point(300, 20), new Point(330, 120), new Point(340, 60));
                context.DrawGeometry(wave, Color.FromArgb(0xFF4CAF50), 4);

                // On a narrow screen, the rest goes on a second row
                bool isNarrow = this.Bounds.Width < 640;
                var row = context.PushTransform(isNarrow ? Matrix3x2.CreateTranslation(-360, 150) : Matrix3x2.Identity);

                // A clip: only the inside of the square shows of the circle
                using (context.PushClip(new Rect(360, 10, 60, 60)))
                {
                    context.FillEllipse(new Rect(370, 20, 80, 80), new SolidColorBrush(Color.FromArgb(0xFFE91E63)));
                }

                context.DrawRectangle(new Rect(360, 10, 60, 60), Color.FromArgb(0xFF9E9E9E));

                // A transform: a rotated square
                using (context.PushTransform(Matrix3x2.CreateRotation((float)(Math.PI / 6), new Vector2(480, 45))))
                {
                    context.FillRectangle(new Rect(455, 20, 50, 50), Color.FromArgb(0xFF009688));
                }

                // Group opacity: the overlap isn't darker
                using (context.PushOpacity(0.5f))
                {
                    context.FillEllipse(new Rect(530, 10, 60, 60), new SolidColorBrush(Colors.Black));
                    context.FillEllipse(new Rect(560, 10, 60, 60), new SolidColorBrush(Colors.Black));
                }

                row.Dispose();

                // Text laid out once, then drawn: wrapped and trimmed
                var format = new TextFormat()
                {
                    FontSize = 13,
                    TextWrapping = TextWrapping.Wrap,
                    MaxLines = 2,
                };
                using (var text = new TextLayout("A TextLayout wraps within its width and stops after two lines, with an ellipsis for the rest of the text.", format, 220))
                {
                    context.DrawRectangle(new Rect(0, 106, 220, text.Size.Height), Color.FromArgb(0xFFDDDDDD));
                    context.DrawText(text, new Point(0, 106), Colors.Black);
                }
            }
        }
    }
}
