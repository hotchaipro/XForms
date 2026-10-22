using System;
using XForms.Controls;

namespace XForms.Themes.Simple
{
    // An arc in the accent color, spinning once a second while the ring is
    // active
    internal sealed class ProgressRingTemplate : ControlTemplate<ProgressRing>
    {
        private const float DefaultSize = 32.0f;
        private static readonly TimeSpan RevolutionPeriod = TimeSpan.FromSeconds(1);

        private sealed class Spin
        {
            public Animator Animator;
            public bool IsSpinning;
        }

        // The ring spins on a layer of its own
        protected override void Apply(
            ProgressRing control,
            TemplateContext context)
        {
            context.State = new Spin() { Animator = context.CreateOwnAnimator() };
        }

        protected override void OnStateChanged(
            ProgressRing control,
            TemplateContext context)
        {
            var spin = (Spin)context.State;
            if (control.IsActive == spin.IsSpinning)
            {
                return;
            }

            spin.IsSpinning = control.IsActive;
            if (spin.IsSpinning)
            {
                // One turn per period, until stopped
                spin.Animator.RotateFrom(Angle.Zero, Angle.FromDegrees(360), RevolutionPeriod, loop: true);
            }
            else
            {
                spin.Animator.Stop();
                spin.Animator.Rotation = Angle.Zero;
            }
        }

        protected override Size? Measure(
            ProgressRing control,
            Size availableSize)
        {
            float size = Math.Min(DefaultSize, Math.Min(availableSize.Width, availableSize.Height));
            return new Size(size, size);
        }

        protected override void Render(
            ProgressRing control,
            DrawingContext drawContext)
        {
            var bounds = new Rect(0, 0, control.Bounds.Width, control.Bounds.Height);
            drawContext.Clear(Colors.Transparent);
            float size = Math.Min(bounds.Width, bounds.Height);
            float strokeWidth = Math.Max(2, size / 10);
            drawContext.DrawArc(bounds.Center, (size - strokeWidth) / 2, 0, 270, control.ActualTheme.Colors.Accent, strokeWidth);
        }
    }
}
