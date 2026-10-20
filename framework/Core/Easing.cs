using System;

namespace XForms
{
    public enum EasingMode
    {
        EaseIn,
        EaseOut,
        EaseInOut,
    }

    /// <summary>
    /// The shape of an animation's progress over time. Platforms sample it to
    /// play the animation on their compositor.
    /// </summary>
    public abstract class EasingFunction
    {
        /// <summary>
        /// Constant speed.
        /// </summary>
        public static readonly EasingFunction Linear = new LinearEase();

        protected EasingFunction(
            EasingMode easingMode)
        {
            this.EasingMode = easingMode;
        }

        public EasingMode EasingMode
        {
            get;
            private set;
        }

        /// <summary>
        /// The progress (0 at the start, 1 at the end, possibly overshooting)
        /// at a normalized time from 0 to 1.
        /// </summary>
        public float Ease(
            float t)
        {
            t = Math.Max(0, Math.Min(1, t));
            switch (this.EasingMode)
            {
                case EasingMode.EaseIn:
                    return this.EaseInCore(t);

                case EasingMode.EaseOut:
                    return 1 - this.EaseInCore(1 - t);

                default:
                    return (t < 0.5f)
                        ? this.EaseInCore(t * 2) / 2
                        : 1 - (this.EaseInCore((1 - t) * 2) / 2);
            }
        }

        /// <summary>
        /// The ease-in curve, from (0, 0) to (1, 1).
        /// </summary>
        protected abstract float EaseInCore(
            float t);

        private sealed class LinearEase : EasingFunction
        {
            public LinearEase()
                : base(EasingMode.EaseIn)
            {
            }

            protected override float EaseInCore(float t) => t;
        }
    }

    public class CubicEase : EasingFunction
    {
        public CubicEase(
            EasingMode easingMode)
            : base(easingMode)
        {
        }

        protected override float EaseInCore(float t) => t * t * t;
    }

    /// <summary>
    /// A spring: it overshoots and settles, oscillating the given number of
    /// times (as WPF's ElasticEase).
    /// </summary>
    public class SpringEase : EasingFunction
    {
        public SpringEase(
            EasingMode easingMode)
            : base(easingMode)
        {
            this.Oscillations = 1;
            this.Springiness = 6;
        }

        public int Oscillations
        {
            get;
            set;
        }

        /// <summary>
        /// How quickly the oscillation dies away: larger is stiffer.
        /// </summary>
        public float Springiness
        {
            get;
            set;
        }

        protected override float EaseInCore(
            float t)
        {
            double oscillations = Math.Max(0, this.Oscillations);
            double springiness = Math.Max(0, this.Springiness);
            double expo = (springiness == 0) ? t : (Math.Exp(springiness * t) - 1) / (Math.Exp(springiness) - 1);
            return (float)(expo * Math.Sin(((2 * Math.PI * oscillations) + (Math.PI / 2)) * t));
        }
    }
}
