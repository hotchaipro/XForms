using System;

namespace XForms
{
    public enum EasingMode
    {
        EaseIn,
        EaseOut,
        EaseInOut,
    }

    public abstract class EasingFunction
    {
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
    }

    public class CubicEase : EasingFunction
    {
        public CubicEase(
            EasingMode easingMode)
            : base(easingMode)
        {
        }
    }

    public class SpringEase : EasingFunction
    {
        public SpringEase(
            EasingMode easingMode)
            : base(easingMode)
        {
            this.Oscillations = 1;
        }

        public int Oscillations
        {
            get;
            set;
        }
    }
}
