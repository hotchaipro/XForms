using System;

namespace XForms
{
    public struct Size
    {
        private readonly float _width, _height;

        public static readonly Size Empty = new Size(0, 0);

        public Size(
            Dimension size)
            : this(size, size)
        {
        }

        public Size(
            Dimension width,
            Dimension height)
        {
            this._width = width;
            this._height = height;
        }

        public float Width
        {
            get { return this._width; }
        }

        public float Height
        {
            get { return this._height; }
        }

        public override bool Equals(
            object obj)
        {
            if (obj is Size)
            {
                return this == (Size)obj;
            }
            return false;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + this._width.GetHashCode();
                hash = hash * 23 + this._height.GetHashCode();
                return hash;
            }
        }

        public static bool operator ==(Size a, Size b)
        {
            return (a._width.Equals(b._width)) && (a._height.Equals(b._height));
        }

        public static bool operator !=(Size a, Size b)
        {
            return (!a._width.Equals(b._width)) || (!a._height.Equals(b._height));
        }
    }
}
