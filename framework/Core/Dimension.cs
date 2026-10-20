using System;

namespace XForms
{
    public struct Dimension
    {
        // TODO: Consider using float.PositiveInfinity instead of float.NaN since the == operator does not return true for float.NaN == float.NaN
        // NOTE: This will require some change in the UWP renderer since UWP uses float.NaN to represent auto-sized content
        public static readonly Dimension Auto = new Dimension(float.NaN);

        public Dimension(
            float value)
        {
            this.Value = value;
        }

        public float Value;

        public static implicit operator float(
            Dimension dimension)
        {
            return dimension.Value;
        }

        public static implicit operator Dimension(
            float value)
        {
            return new Dimension(value);
        }

        public override bool Equals(
            object obj)
        {
            if (obj is Dimension)
            {
                return this.Value == ((Dimension)obj).Value;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return this.Value.GetHashCode();
        }

        public static bool operator ==(Dimension a, Dimension b)
        {
            // NOTE: float.NaN == float.NaN is false, but float.NaN.Equals(float.NaN) is true
            return (a.Value.Equals(b.Value));
        }

        public static bool operator !=(Dimension a, Dimension b)
        {
            return !(a.Value.Equals(b.Value));
        }
    }
}
