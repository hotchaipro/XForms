using System;
using System.Globalization;

namespace XForms
{
    public struct Point
    {
        public static readonly Point Zero = new Point(0, 0);

        private float _x, _y;

        public Point(
            float x,
            float y)
        {
            this._x = x;
            this._y = y;
        }

        public float X
        {
            get { return this._x; }
        }

        public float Y
        {
            get { return this._y; }
        }

        public float DistanceTo(
            Point other)
        {
            double dx = other.X - this.X;
            double dy = other.Y - this.Y;

            return (float)Math.Sqrt((dx * dx) + (dy * dy));
        }

        public float DistanceSquaredTo(
            Point other)
        {
            double dx = other.X - this.X;
            double dy = other.Y - this.Y;

            return (float)((dx * dx) + (dy * dy));
        }

        public Point Translate(
            float dx,
            float dy)
        {
            return new Point(this._x + dx, this._y + dy);
        }

        public Point Translate(
            Angle angle,
            float distance)
        {
            return Translate(this, angle, distance);
        }

        public static Point Translate(
            Point point,
            float dx,
            float dy)
        {
            return new Point(point._x + dx, point._y + dy);
        }

        public static Point Translate(
            Point point,
            Angle angle,
            float distance)
        {
            float x = (float)(distance * Math.Cos(angle.Radians)) + point._x;
            float y = (float)(distance * Math.Sin(angle.Radians)) + point._y;

            return new Point(x, y);
        }

        public override bool Equals(
            object obj)
        {
            if (obj is Point)
            {
                return this == (Point)obj;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return unchecked((17 * 23 + this._x.GetHashCode()) * 23 + this._y.GetHashCode());
        }

        public static bool operator == (Point a, Point b)
        {
            return (a._x == b._x) && (a._y == b._y);
        }
        public static bool operator != (Point a, Point b)
        {
            return (a._x != b._x) || (a._y != b._y);
        }

        public static Point operator * (Point a, float v)
        {
            return new Point(a.X * v, a.Y * v);
        }

        public static Point operator * (float v, Point a)
        {
            return new Point(a.X * v, a.Y * v);
        }

        public static Point operator / (Point a, float v)
        {
            return new Point(a.X / v, a.Y / v);
        }

        public static Point operator + (Point a, Point v)
        {
            return new Point(a.X + v.X, a.Y + v.Y);
        }

        public static Point operator - (Point a)
        {
            return new Point(-a.X, -a.Y);
        }

        public static Point operator - (Point a, Point v)
        {
            return new Point(a.X - v.X, a.Y - v.Y);
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "Point ({0}, {1})", this._x, this._y);
        }
    }
}
