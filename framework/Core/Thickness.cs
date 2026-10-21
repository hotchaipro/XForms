using System;

namespace XForms
{
    public struct Thickness
    {
        public static readonly Thickness Zero = new Thickness(0, 0, 0, 0);

        public Thickness(
            float all)
            : this(all, all, all, all)
        {
        }

        public Thickness(
            float leftAndRight,
            float topAndBottom)
            : this(leftAndRight, topAndBottom, leftAndRight, topAndBottom)
        {
        }

        public Thickness(
            float left,
            float top,
            float right,
            float bottom)
        {
            this.Left = left;
            this.Top = top;
            this.Right = right;
            this.Bottom = bottom;
        }

        public float Left;
        public float Top;
        public float Right;
        public float Bottom;

        public float Horizontal
        {
            get
            {
                return this.Left + this.Right;
            }
        }

        public float Vertical
        {
            get
            {
                return this.Top + this.Bottom;
            }
        }

        public override bool Equals(
            object obj)
        {
            if (obj is Thickness)
            {
                return this == (Thickness)obj;
            }
            return false;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + this.Left.GetHashCode();
                hash = hash * 23 + this.Top.GetHashCode();
                hash = hash * 23 + this.Right.GetHashCode();
                hash = hash * 23 + this.Bottom.GetHashCode();
                return hash;
            }
        }

        public static bool operator ==(Thickness a, Thickness b)
        {
            return (a.Left == b.Left) && (a.Top == b.Top) && (a.Right == b.Right) && (a.Bottom == b.Bottom);
        }

        public static bool operator !=(Thickness a, Thickness b)
        {
            return (a.Left != b.Left) || (a.Top != b.Top) || (a.Right != b.Right) || (a.Bottom != b.Bottom);
        }
    }
}
