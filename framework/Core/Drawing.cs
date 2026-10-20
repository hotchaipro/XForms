using System;
using System.Collections.Generic;
using System.Numerics;

namespace XForms
{
    /// <summary>
    /// What fills a shape. Brushes are immutable, so they can be shared and
    /// cached.
    /// </summary>
    public abstract class Brush
    {
        private protected Brush()
        {
        }
    }

    public sealed class SolidColorBrush : Brush
    {
        public SolidColorBrush(
            Color color)
        {
            this.Color = color;
        }

        public Color Color
        {
            get;
        }
    }

    public readonly struct GradientStop
    {
        public GradientStop(
            Color color,
            float offset)
        {
            this.Color = color;
            this.Offset = offset;
        }

        public Color Color
        {
            get;
        }

        /// <summary>
        /// Where the color is along the gradient, from 0 to 1.
        /// </summary>
        public float Offset
        {
            get;
        }
    }

    /// <summary>
    /// A gradient along a line. The start and end points are relative to the
    /// bounds of the shape filled: (0, 0) is its top left corner, (1, 1) its
    /// bottom right.
    /// </summary>
    public sealed class LinearGradientBrush : Brush
    {
        private readonly GradientStop[] _stops;

        public LinearGradientBrush(
            Point startPoint,
            Point endPoint,
            params GradientStop[] stops)
        {
            if ((null == stops) || (stops.Length < 2))
            {
                throw new ArgumentException("A gradient needs at least two stops.", nameof(stops));
            }

            this.StartPoint = startPoint;
            this.EndPoint = endPoint;
            this._stops = (GradientStop[])stops.Clone();
            Array.Sort(this._stops, (a, b) => a.Offset.CompareTo(b.Offset));
        }

        /// <summary>
        /// A gradient from the first color at the top to the second at the
        /// bottom.
        /// </summary>
        public static LinearGradientBrush Vertical(
            Color top,
            Color bottom)
        {
            return new LinearGradientBrush(new Point(0, 0), new Point(0, 1), new GradientStop(top, 0), new GradientStop(bottom, 1));
        }

        public Point StartPoint
        {
            get;
        }

        public Point EndPoint
        {
            get;
        }

        public IReadOnlyList<GradientStop> Stops
        {
            get
            {
                return this._stops;
            }
        }

        /// <summary>
        /// The start and end points within the bounds, in the bounds' coordinates.
        /// </summary>
        public void GetAbsolutePoints(
            Rect bounds,
            out Point start,
            out Point end)
        {
            start = new Point(bounds.X + (this.StartPoint.X * bounds.Width), bounds.Y + (this.StartPoint.Y * bounds.Height));
            end = new Point(bounds.X + (this.EndPoint.X * bounds.Width), bounds.Y + (this.EndPoint.Y * bounds.Height));
        }
    }

    public enum PathSegmentKind
    {
        MoveTo,
        LineTo,
        QuadraticTo,
        CubicTo,
        Close,
    }

    /// <summary>
    /// A segment of a path: the end point, and the control points of a curve.
    /// </summary>
    public readonly struct PathSegment
    {
        internal PathSegment(PathSegmentKind kind, Point point, Point control1 = default, Point control2 = default)
        {
            this.Kind = kind;
            this.Point = point;
            this.Control1 = control1;
            this.Control2 = control2;
        }

        public PathSegmentKind Kind { get; }

        public Point Point { get; }

        public Point Control1 { get; }

        public Point Control2 { get; }
    }

    /// <summary>
    /// A shape made of lines and curves, built with <c>MoveTo</c>,
    /// <c>LineTo</c>, curves and <c>Close</c>, then filled or stroked by a
    /// <see cref="DrawingContext"/>.
    /// </summary>
    public sealed class PathGeometry
    {
        private readonly List<PathSegment> _segments = new List<PathSegment>();
        private Point _current;
        private Point _figureStart;
        private float _left = float.PositiveInfinity, _top = float.PositiveInfinity;
        private float _right = float.NegativeInfinity, _bottom = float.NegativeInfinity;

        public IReadOnlyList<PathSegment> Segments
        {
            get
            {
                return this._segments;
            }
        }

        /// <summary>
        /// A rectangle holding every point of the path (including control points).
        /// </summary>
        public Rect Bounds
        {
            get
            {
                return float.IsInfinity(this._left) ? new Rect(0, 0, 0, 0) : new Rect(this._left, this._top, this._right - this._left, this._bottom - this._top);
            }
        }

        private void Include(
            Point point)
        {
            this._left = Math.Min(this._left, point.X);
            this._top = Math.Min(this._top, point.Y);
            this._right = Math.Max(this._right, point.X);
            this._bottom = Math.Max(this._bottom, point.Y);
        }

        /// <summary>
        /// Starts a new figure at the point.
        /// </summary>
        public PathGeometry MoveTo(
            Point point)
        {
            this._segments.Add(new PathSegment(PathSegmentKind.MoveTo, point));
            this._current = point;
            this._figureStart = point;
            this.Include(point);
            return this;
        }

        public PathGeometry LineTo(
            Point point)
        {
            this._segments.Add(new PathSegment(PathSegmentKind.LineTo, point));
            this._current = point;
            this.Include(point);
            return this;
        }

        public PathGeometry QuadraticTo(
            Point control,
            Point point)
        {
            this._segments.Add(new PathSegment(PathSegmentKind.QuadraticTo, point, control));
            this._current = point;
            this.Include(control);
            this.Include(point);
            return this;
        }

        public PathGeometry CubicTo(
            Point control1,
            Point control2,
            Point point)
        {
            this._segments.Add(new PathSegment(PathSegmentKind.CubicTo, point, control1, control2));
            this._current = point;
            this.Include(control1);
            this.Include(control2);
            this.Include(point);
            return this;
        }

        /// <summary>
        /// An elliptical arc from the current point to the point, as in SVG: the
        /// radii, whether it's the larger of the two possible arcs, and whether
        /// it turns clockwise. Arcs become cubic curves.
        /// </summary>
        public PathGeometry ArcTo(
            Point point,
            float radiusX,
            float radiusY,
            bool isLargeArc,
            bool isClockwise)
        {
            foreach (var curve in ArcToCubics(this._current, point, Math.Abs(radiusX), Math.Abs(radiusY), isLargeArc, isClockwise))
            {
                this.CubicTo(curve.Item1, curve.Item2, curve.Item3);
            }

            this._current = point;
            return this;
        }

        /// <summary>
        /// Closes the figure with a line back to its start.
        /// </summary>
        public PathGeometry Close()
        {
            this._segments.Add(new PathSegment(PathSegmentKind.Close, this._figureStart));
            this._current = this._figureStart;
            return this;
        }

        // Converts an SVG endpoint arc to cubic Béziers (one per quarter turn or less)
        private static IEnumerable<(Point, Point, Point)> ArcToCubics(
            Point start,
            Point end,
            float rx,
            float ry,
            bool isLargeArc,
            bool isClockwise)
        {
            if ((start == end) || (rx == 0) || (ry == 0))
            {
                yield return (start, end, end);
                yield break;
            }

            double x1p = (start.X - end.X) / 2.0;
            double y1p = (start.Y - end.Y) / 2.0;

            // Scale the radii up if they can't reach
            double lambda = ((x1p * x1p) / (rx * rx)) + ((y1p * y1p) / (ry * ry));
            if (lambda > 1)
            {
                rx = (float)(rx * Math.Sqrt(lambda));
                ry = (float)(ry * Math.Sqrt(lambda));
            }

            double numerator = (rx * rx * ry * ry) - (rx * rx * y1p * y1p) - (ry * ry * x1p * x1p);
            double denominator = (rx * rx * y1p * y1p) + (ry * ry * x1p * x1p);
            double coefficient = Math.Sqrt(Math.Max(0, numerator / denominator));
            if (isLargeArc == isClockwise)
            {
                coefficient = -coefficient;
            }

            double cxp = coefficient * ((rx * y1p) / ry);
            double cyp = coefficient * -((ry * x1p) / rx);
            double cx = cxp + ((start.X + end.X) / 2.0);
            double cy = cyp + ((start.Y + end.Y) / 2.0);

            double startAngle = Math.Atan2((y1p - cyp) / ry, (x1p - cxp) / rx);
            double endAngle = Math.Atan2((-y1p - cyp) / ry, (-x1p - cxp) / rx);
            double sweep = endAngle - startAngle;
            if (isClockwise && (sweep < 0))
            {
                sweep += 2 * Math.PI;
            }
            else if (!isClockwise && (sweep > 0))
            {
                sweep -= 2 * Math.PI;
            }

            int count = (int)Math.Ceiling(Math.Abs(sweep) / (Math.PI / 2));
            double step = sweep / count;
            double k = (4.0 / 3.0) * Math.Tan(step / 4);

            double angle = startAngle;
            for (int i = 0; i < count; i += 1)
            {
                double a1 = angle, a2 = angle + step;
                double cos1 = Math.Cos(a1), sin1 = Math.Sin(a1), cos2 = Math.Cos(a2), sin2 = Math.Sin(a2);

                var c1 = new Point((float)(cx + (rx * (cos1 - (k * sin1)))), (float)(cy + (ry * (sin1 + (k * cos1)))));
                var c2 = new Point((float)(cx + (rx * (cos2 + (k * sin2)))), (float)(cy + (ry * (sin2 - (k * cos2)))));
                var p2 = (i == count - 1) ? end : new Point((float)(cx + (rx * cos2)), (float)(cy + (ry * sin2)));

                yield return (c1, c2, p2);
                angle = a2;
            }
        }
    }

    /// <summary>
    /// Undoes a <c>Push</c> on a <see cref="DrawingContext"/> when disposed, for
    /// use with <c>using</c>.
    /// </summary>
    public readonly struct DrawingContextState : IDisposable
    {
        private readonly DrawingContext _context;

        internal DrawingContextState(
            DrawingContext context)
        {
            this._context = context;
        }

        public void Dispose()
        {
            this._context?.Pop();
        }
    }
}
