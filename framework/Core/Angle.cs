using System;

namespace XForms
{
    public struct Angle
    {
        public static readonly Angle Zero = new Angle(0f);

        private static readonly float DegreesPerRadian = (float)(180 / Math.PI);
        private static readonly float RadiansPerDegree = (float)(Math.PI / 180);

        private float _radians;

        private Angle(
            float radians)
        {
            this._radians = radians;
        }

        public static Angle FromDegrees(
            float degrees)
        {
            return new Angle(degrees * RadiansPerDegree);
        }

        public static Angle FromRadians(
            float radians)
        {
            return new Angle(radians);
        }

        public float Degrees
        {
            get
            {
                return this._radians * DegreesPerRadian;
            }
        }

        public float Radians
        {
            get
            {
                return this._radians;
            }
        }

        public override bool Equals(
            object obj)
        {
            if (obj is Angle)
            {
                return this._radians == ((Angle)obj)._radians;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return this._radians.GetHashCode();
        }

        public static bool operator ==(Angle a, Angle b)
        {
            return (a._radians == b._radians);
        }

        public static bool operator !=(Angle a, Angle b)
        {
            return (a._radians != b._radians);
        }
    }
}
