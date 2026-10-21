using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.Versioning;
using XForms.Web.Renderers;

namespace XForms.Web
{
    /// <summary>
    /// Records drawing as one command list, which the page replays on a
    /// layer's canvas (see draw in xforms.js): one call into JavaScript per
    /// layer drawn, rather than one per shape.
    /// </summary>
    [SupportedOSPlatform("browser")]
    internal sealed class DrawContext : DrawingContext
    {
        private readonly List<double> _numbers = new List<double>(256);
        private readonly List<string> _strings = new List<string>();
        private readonly Dictionary<string, int> _stringIndexes = new Dictionary<string, int>();
        private readonly Rect _bounds;

        public DrawContext(
            Rect bounds)
        {
            this._bounds = bounds;
        }

        /// <summary>
        /// Draws what was recorded onto the canvas.
        /// </summary>
        public void Flush(
            int canvasId)
        {
            Js.Draw(canvasId, this._bounds.Width, this._bounds.Height, this._numbers.ToArray(), this._strings.ToArray());
        }

        private void Add(params double[] values)
        {
            this._numbers.AddRange(values);
        }

        private static double Pack(
            Color color)
        {
            return color.ToArgb();
        }

        private int String(
            string value)
        {
            if (!this._stringIndexes.TryGetValue(value, out int index))
            {
                index = this._strings.Count;
                this._strings.Add(value);
                this._stringIndexes[value] = index;
            }

            return index;
        }

        protected override void ClearCore(
            Color color)
        {
            this.Add(30, Pack(color));
        }

        public override void DrawRectangle(Rect rectangle, Color color, float strokeWidth = 1.0f)
        {
            this.Add(11, rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height, Pack(color), strokeWidth);
        }

        public override void FillRectangle(Rect rectangle, Color color)
        {
            this.Add(10, rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height, Pack(color));
        }

        public override void DrawRoundedRectangle(Rect rectangle, float cornerRadius, Color color, float strokeWidth = 1.0f)
        {
            this.Add(13, rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height, cornerRadius, Pack(color), strokeWidth);
        }

        public override void FillRoundedRectangle(Rect rectangle, float cornerRadius, Color color)
        {
            this.Add(12, rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height, cornerRadius, Pack(color));
        }

        public override void DrawEllipse(Point point, float radiusX, float radiusY, Color color, float strokeWidth = 1.0f)
        {
            this.Add(15, point.X, point.Y, radiusX, radiusY, Pack(color), strokeWidth);
        }

        public override void FillEllipse(Point point, float radiusX, float radiusY, Color color)
        {
            this.Add(14, point.X, point.Y, radiusX, radiusY, Pack(color));
        }

        public override void FillEllipse(Rect rectangle, Color color)
        {
            this.FillEllipse(new Point(rectangle.X + (rectangle.Width / 2), rectangle.Y + (rectangle.Height / 2)), rectangle.Width / 2, rectangle.Height / 2, color);
        }

        public override void FillEllipseShadow(Rect rectangle, Color color, float blurAmount = 8.0f)
        {
            this.Add(22, rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height, Pack(color), blurAmount);
        }

        public override void DrawImage(Bitmap bitmap, Rect destinationRectangle, Color tint)
        {
            if ((bitmap?.Renderer is BitmapRenderer renderer) && (null != renderer.Image))
            {
                this.Add(19, renderer.Image.Id, destinationRectangle.X, destinationRectangle.Y, destinationRectangle.Width, destinationRectangle.Height, tint.IsTransparent ? 0 : Pack(tint));
            }
        }

        public override void DrawLine(Point start, Point end, Color color, float strokeWidth = 1.0f)
        {
            this.Add(16, start.X, start.Y, end.X, end.Y, Pack(color), strokeWidth);
        }

        public override void DrawArc(Point center, float radius, float startDegrees, float sweepDegrees, Color color, float strokeWidth = 1.0f)
        {
            this.Add(17, center.X, center.Y, radius, startDegrees, sweepDegrees, Pack(color), strokeWidth);
        }

        public override void DrawText(TextLayout layout, Point origin, Color color)
        {
            if (!(layout.Native is WebTextLayout text))
            {
                return;
            }

            float y = origin.Y;
            foreach (var line in text.Lines)
            {
                float baseline = y + text.Ascent;
                foreach (var run in line.Runs)
                {
                    if (run.Text.Length == 0)
                    {
                        continue;
                    }

                    var runColor = run.Foreground ?? color;
                    this.Add(18, origin.X + run.X, baseline, this.String(run.Text), this.String(run.Font), Pack(runColor));

                    if (run.Underline)
                    {
                        this.DrawLine(new Point(origin.X + run.X, baseline + 2), new Point(origin.X + run.X + run.Width, baseline + 2), runColor, 1);
                    }

                    if (run.Strikethrough)
                    {
                        float middle = baseline - (text.Ascent * 0.3f);
                        this.DrawLine(new Point(origin.X + run.X, middle), new Point(origin.X + run.X + run.Width, middle), runColor, 1);
                    }
                }

                y += text.LineHeight;
            }
        }

        // Brushes: a solid color, or a gradient set up for the next fill
        private Color UseBrush(
            Brush brush,
            Rect bounds)
        {
            switch (brush)
            {
                case SolidColorBrush solid:
                    return solid.Color;

                case LinearGradientBrush gradient:
                    gradient.GetAbsolutePoints(bounds, out var start, out var end);
                    this.Add(21, start.X, start.Y, end.X, end.Y, gradient.Stops.Count);
                    foreach (var stop in gradient.Stops)
                    {
                        this.Add(stop.Offset, Pack(stop.Color));
                    }

                    return Colors.Black;

                default:
                    return Colors.Transparent;
            }
        }

        public override void FillRectangle(Rect rectangle, Brush brush)
        {
            this.FillRectangle(rectangle, this.UseBrush(brush, rectangle));
        }

        public override void FillRoundedRectangle(Rect rectangle, float cornerRadius, Brush brush)
        {
            this.FillRoundedRectangle(rectangle, cornerRadius, this.UseBrush(brush, rectangle));
        }

        public override void FillEllipse(Rect rectangle, Brush brush)
        {
            this.FillEllipse(rectangle, this.UseBrush(brush, rectangle));
        }

        private void AddPath(
            PathGeometry geometry)
        {
            this.Add(20, geometry.Segments.Count);
            foreach (var segment in geometry.Segments)
            {
                switch (segment.Kind)
                {
                    case PathSegmentKind.MoveTo:
                        this.Add(0, segment.Point.X, segment.Point.Y);
                        break;

                    case PathSegmentKind.LineTo:
                        this.Add(1, segment.Point.X, segment.Point.Y);
                        break;

                    case PathSegmentKind.QuadraticTo:
                        this.Add(2, segment.Control1.X, segment.Control1.Y, segment.Point.X, segment.Point.Y);
                        break;

                    case PathSegmentKind.CubicTo:
                        this.Add(3, segment.Control1.X, segment.Control1.Y, segment.Control2.X, segment.Control2.Y, segment.Point.X, segment.Point.Y);
                        break;

                    case PathSegmentKind.Close:
                        this.Add(4);
                        break;
                }
            }
        }

        public override void FillGeometry(PathGeometry geometry, Brush brush)
        {
            // NOTE: A gradient goes before the path's command
            var color = this.UseBrush(brush, geometry.Bounds);
            this.AddPath(geometry);
            this.Add(0, Pack(color));
        }

        public override void DrawGeometry(PathGeometry geometry, Color color, float strokeWidth = 1.0f)
        {
            this.AddPath(geometry);
            this.Add(1, Pack(color), strokeWidth);
        }

        public override void Pop()
        {
            this.Add(2);
        }

        protected override void PushClipCore(Rect clip)
        {
            this.Add(1, 3, clip.X, clip.Y, clip.Width, clip.Height);
        }

        protected override void PushTransformCore(Matrix3x2 transform)
        {
            this.Add(1, 4, transform.M11, transform.M12, transform.M21, transform.M22, transform.M31, transform.M32);
        }

        protected override void PushOpacityCore(float opacity)
        {
            this.Add(1, 5, opacity);
        }
    }
}
