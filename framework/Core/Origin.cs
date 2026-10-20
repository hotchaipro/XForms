using System;

namespace XForms
{
    public struct Origin
    {
        public static Origin TopLeft = new Origin(0, 0);
        public static Origin TopCenter = new Origin(0.5f, 0);
        public static Origin TopRight = new Origin(1.0f, 0);
        public static Origin MiddleLeft = new Origin(0, 0.5f);
        public static Origin Center = new Origin(0.5f, 0.5f);
        public static Origin MiddleRight = new Origin(1.0f, 0.5f);
        public static Origin BottomLeft = new Origin(0, 1.0f);
        public static Origin BottomCenter = new Origin(0.5f, 1.0f);
        public static Origin BottomRight = new Origin(1.0f, 1.0f);

        private float _x, _y;

        public Origin(
            float x,
            float y)
        {
            if ((x < 0f) || (x > 1.0f))
            {
                throw new ArgumentOutOfRangeException("x");
            }

            if ((y < 0f) || (y > 1.0f))
            {
                throw new ArgumentOutOfRangeException("y");
            }

            this._x = x;
            this._y = y;
        }

        public float X
        {
            get
            {
                return this._x;
            }
        }

        public float Y
        {
            get
            {
                return this._y;
            }
        }

        public override bool Equals(
            object obj)
        {
            if (obj is Origin)
            {
                return this == (Origin)obj;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return unchecked((17 * 23 + this._x.GetHashCode()) * 23 + this._y.GetHashCode());
        }

        public static bool operator ==(Origin a, Origin b)
        {
            return (a._x == b._x) && (a._y == b._y);
        }

        public static bool operator !=(Origin a, Origin b)
        {
            return (a._x != b._x) || (a._y != b._y);
        }
    }
}
