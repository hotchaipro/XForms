using System;
using System.Diagnostics;
using XForms.Controls;
using XForms.Layouts;
using XForms.Primitives;

namespace XForms.Test
{
    /// <summary>
    /// Measures the cost of native objects per view: 1,000 owner-drawn tiles,
    /// either one view per tile (one native object each) or one view that
    /// draws every tile itself (a stand-in for lightweight views).
    /// </summary>
    /// <remarks>
    /// The status line reports how long the page took to build, and when the
    /// first frame finished drawing (the last tile's first draw). Measure memory
    /// from outside the process.
    /// </remarks>
    public class BenchmarkPage : Page
    {
        public const int Columns = 10;
        public const int Rows = 100;
        public const float TileSize = 56;
        public const float TileSpacing = 8;

        private static readonly Stopwatch Clock = new Stopwatch();
        private static long _lastFirstDrawTicks;

        private readonly TextBlock _status;
        private readonly long _builtTicks;

        public BenchmarkPage(
            bool drawAsOneView)
        {
            Clock.Restart();
            _lastFirstDrawTicks = 0;

            var pageLayout = new DockPanel();

            this._status = new TextBlock()
            {
                Text = drawAsOneView ? "Drawn as one view: measuring..." : "One view per tile: measuring...",
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(16, 8, 16, 8),
            };
            pageLayout.Children.Add(this._status, Dock.Top);

            View content;
            if (drawAsOneView)
            {
                content = new TileSheet(Rows * Columns);
            }
            else
            {
                var rowsLayout = new DockPanel()
                {
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top,
                };

                int number = 0;
                for (int row = 0; row < Rows; row += 1)
                {
                    var rowLayout = new UniformGrid()
                    {
                        HorizontalAlignment = HorizontalAlignment.Left,
                        Width = Columns * (TileSize + TileSpacing),
                        Height = TileSize + TileSpacing,
                    };

                    for (int column = 0; column < Columns; column += 1)
                    {
                        rowLayout.Children.Add(new Tile(number));
                        number += 1;
                    }

                    rowsLayout.Children.Add(rowLayout, Dock.Top);
                }

                content = rowsLayout;
            }

            var scrollView = new ScrollViewer()
            {
                IsVerticalScrollingEnabled = true,
                Content = content,
            };
            pageLayout.Children.Add(scrollView, Dock.CenterOverlay);

            this.Content = pageLayout;

            this._builtTicks = Clock.ElapsedTicks;

            // Report once the first frames have drawn
            var timer = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(2) };
            timer.Tick += (sender, e) =>
            {
                timer.Stop();
                this.Report();
            };
            timer.Start();
        }

        private void Report()
        {
            this.Application.Dispatcher.Post(() =>
            {
                double built = this._builtTicks * 1000.0 / Stopwatch.Frequency;
                double drawn = _lastFirstDrawTicks * 1000.0 / Stopwatch.Frequency;

                // Live managed objects, without garbage
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                double managed = GC.GetTotalMemory(true) / (1024.0 * 1024.0);

                string message = $"built {built:F0} ms, first frame drawn {drawn:F0} ms, managed heap {managed:F1} MB";
                this._status.Text = message;
                Console.WriteLine("XFORMS_BENCHMARK " + message);
            });
        }

        private static void DrawTile(
            DrawingContext drawContext,
            Rect bounds,
            int number,
            Theme theme)
        {
            drawContext.FillRectangle(bounds, theme.Colors.Surface);
            drawContext.DrawRectangle(bounds, theme.Colors.Accent, 1);
            drawContext.DrawText(
                number.ToString(),
                bounds,
                theme.Colors.Foreground,
                new TextFormat()
                {
                    FontSize = 14,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                });
        }

        /// <summary>
        /// One owner-drawn tile: one view, so one native object.
        /// </summary>
        private sealed class Tile : UserControl
        {
            private readonly int _number;
            private bool _hasDrawn;

            public Tile(
                int number)
            {
                this._number = number;
                this.Width = TileSize;
                this.Height = TileSize;
            }

            protected override void Render(
                DrawingContext drawContext)
            {
                var bounds = new Rect(0, 0, this.Bounds.Width, this.Bounds.Height);
                drawContext.Clear(Colors.Transparent);
                DrawTile(drawContext, bounds, this._number, this.ActualTheme);

                if (!this._hasDrawn)
                {
                    this._hasDrawn = true;
                    _lastFirstDrawTicks = Math.Max(_lastFirstDrawTicks, Clock.ElapsedTicks);
                }
            }
        }

        /// <summary>
        /// Every tile drawn by one view, as lightweight views would be.
        /// </summary>
        private sealed class TileSheet : UserControl
        {
            private readonly int _count;
            private bool _hasDrawn;

            public TileSheet(
                int count)
            {
                this._count = count;
                this.HorizontalAlignment = HorizontalAlignment.Left;
                this.VerticalAlignment = VerticalAlignment.Top;
                this.Width = Columns * (TileSize + TileSpacing);
                this.Height = (count / Columns) * (TileSize + TileSpacing);
            }

            protected override void Render(
                DrawingContext drawContext)
            {
                var bounds = new Rect(0, 0, this.Bounds.Width, this.Bounds.Height);
                drawContext.Clear(Colors.Transparent);

                // NOTE: Spaced like the per-view layout (tiles centered in their slots)
                float slot = TileSize + TileSpacing;
                for (int i = 0; i < this._count; i += 1)
                {
                    float x = ((i % Columns) * slot) + (TileSpacing / 2);
                    float y = ((i / Columns) * slot) + (TileSpacing / 2);
                    DrawTile(drawContext, new Rect(x, y, TileSize, TileSize), i, this.ActualTheme);
                }

                if (!this._hasDrawn)
                {
                    this._hasDrawn = true;
                    _lastFirstDrawTicks = Math.Max(_lastFirstDrawTicks, Clock.ElapsedTicks);
                }
            }
        }
    }
}
