using System;
using System.Diagnostics;

namespace XForms.Input
{
    /// <summary>
    /// A pointer's speed as it lifts, from its recent positions. A finger that
    /// stopped before it lifted has none.
    /// </summary>
    internal sealed class VelocityTracker
    {
        private const int Capacity = 16;

        // How far back counts, in milliseconds
        private const double Window = 100;

        // A gap this long before the last position is a finger that stopped
        private const double StationaryTime = 50;

        // Points a second
        private const float MaxSpeed = 8000;

        private readonly Point[] _positions = new Point[Capacity];
        private readonly double[] _times = new double[Capacity];
        private int _next;
        private int _count;

        public void Clear()
        {
            this._next = 0;
            this._count = 0;
        }

        public void Add(
            Point position)
        {
            this.Add(position, Stopwatch.GetTimestamp() * 1000.0 / Stopwatch.Frequency);
        }

        public void Add(
            Point position,
            double milliseconds)
        {
            this._positions[this._next] = position;
            this._times[this._next] = milliseconds;
            this._next = (this._next + 1) % Capacity;
            this._count = Math.Min(this._count + 1, Capacity);
        }

        /// <summary>
        /// The velocity, in points a second.
        /// </summary>
        public Point Velocity
        {
            get
            {
                if (this._count < 2)
                {
                    return Point.Zero;
                }

                int newest = (this._next - 1 + Capacity) % Capacity;
                if ((this._times[newest] - this._times[(newest - 1 + Capacity) % Capacity]) > StationaryTime)
                {
                    return Point.Zero;
                }

                int oldest = newest;
                for (int i = 1; i < this._count; i += 1)
                {
                    int index = (newest - i + Capacity) % Capacity;
                    if ((this._times[newest] - this._times[index]) > Window)
                    {
                        break;
                    }

                    oldest = index;
                }

                double seconds = (this._times[newest] - this._times[oldest]) / 1000;
                if (seconds <= 0)
                {
                    return Point.Zero;
                }

                float x = (float)((this._positions[newest].X - this._positions[oldest].X) / seconds);
                float y = (float)((this._positions[newest].Y - this._positions[oldest].Y) / seconds);

                float speed = MathF.Sqrt((x * x) + (y * y));
                return (speed > MaxSpeed)
                    ? new Point(x * MaxSpeed / speed, y * MaxSpeed / speed)
                    : new Point(x, y);
            }
        }
    }
}
