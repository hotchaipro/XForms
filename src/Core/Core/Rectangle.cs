using System;

namespace XForms
{
    public struct Rectangle
    {
        private float _x, _y, _width, _height;

        public Rectangle(
            float x,
            float y,
            float width,
            float height)
        {
            if (width < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(width), width, "Width must be a nonnegative value.");
            }

            if (height < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(height), height, "Height must be a nonnegative value.");
            }

            this._x = x;
            this._y = y;
            this._width = width;
            this._height = height;
        }

        public static Rectangle FromLTRB(
            float left,
            float top,
            float right,
            float bottom)
        {
            return new Rectangle(left, top, right - left, bottom - top);
        }

        public float X
        {
            get { return this._x; }
            set { this._x = value; }
        }

        public float Y
        {
            get { return this._y; }
            set { this._y = value; }
        }

        public float Top
        {
            get { return this._y; }
            set { this._y = value; }
        }

        public float Left
        {
            get { return this._x; }
            set { this._x = value; }
        }

        public float Width
        {
            get { return this._width; }
            set { this._width = value; }
        }

        public float Height
        {
            get { return this._height; }
            set { this._height = value; }
        }

        public float Right
        {
            get { return this._x + this._width; }
        }

        public float Bottom
        {
            get { return this._y + this._height; }
        }

        public Point Center
        {
            get
            {
                return new Point(this._x + (this._width / 2.0f), this._y + (this._height / 2.0f));
            }
        }

        public Point Location
        {
            get
            {
                return new Point(this._x, this._y);
            }
        }

        public Size Size
        {
            get
            {
                return new Size(this._width, this._height);
            }
        }

        public Rectangle Inflate(
            float dx,
            float dy)
        {
            return new Rectangle(this._x - dx, this._y - dy, this._width + (2 * dx), this._height + (2 * dy));
        }

        public Rectangle Deflate(
            float dx,
            float dy)
        {
            return new Rectangle(this._x + dx, this._y + dy, this._width - (2 * dx), this._height - (2 * dy));
        }

        public Rectangle Offset(
            float x,
            float y)
        {
            return new Rectangle(this._x + x, this._y + y, this._width, this._height);
        }

        public Rectangle Union(
            Rectangle other)
        {
            float minX = Math.Min(this.X, other.X);
            float minY = Math.Min(this.Y, other.Y);
            float maxX = Math.Max(this.Right, other.Right);
            float maxY = Math.Max(this.Bottom, other.Bottom);

            return new Rectangle(minX, minY, maxX - minX, maxY - minY);
        }

        public override bool Equals(
            object obj)
        {
            if (obj is Rectangle)
            {
                return this == (Rectangle)obj;
            }
            return false;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + this._x.GetHashCode();
                hash = hash * 23 + this._y.GetHashCode();
                hash = hash * 23 + this._width.GetHashCode();
                hash = hash * 23 + this._height.GetHashCode();
                return hash;
            }
        }

        public static bool operator == (Rectangle a, Rectangle b)
        {
            return (a._x == b._x) && (a._y == b._y) && (a._width == b._width) && (a._height == b._height);
        }

        public static bool operator !=(Rectangle a, Rectangle b)
        {
            return (a._x != b._x) || (a._y != b._y) || (a._width != b._width) || (a._height != b._height);
        }
    }
}
