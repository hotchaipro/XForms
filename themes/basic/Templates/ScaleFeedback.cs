using System;
using XForms.Controls;

namespace XForms.Themes.Basic
{
    /// <summary>
    /// A control's drawn scale and opacity, set at once when its template is
    /// applied and animated afterward, and only toward a new target (state
    /// changes such as hover don't restart it).
    /// </summary>
    internal sealed class ScaleFeedback
    {
        private readonly DrawnAnimator _animator;
        private float? _target;

        public ScaleFeedback(
            TemplateContext context)
        {
            this._animator = context.CreateDrawnAnimator();
        }

        public DrawnAnimator Animator
        {
            get
            {
                return this._animator;
            }
        }

        public float Scale
        {
            get
            {
                return this._animator.Scale;
            }
        }

        public void To(
            float scale,
            TimeSpan duration,
            EasingFunction easing)
        {
            if (!this._target.HasValue)
            {
                // Applying the template: no animation
                this._target = scale;
                this._animator.Scale = scale;
                return;
            }

            if (scale != this._target.Value)
            {
                this._target = scale;
                this._animator.ScaleTo(scale, duration, easing);
            }
        }

        public static ScaleFeedback For(
            TemplateContext context)
        {
            return (context.State as ScaleFeedback) ?? (ScaleFeedback)(context.State = new ScaleFeedback(context));
        }
    }
}
