using System;
using XForms.Controls;

namespace XForms.Toolkit
{
    /// <summary>
    /// An indeterminate progress indicator: an arc in the accent color that
    /// spins while the ring is active. The spinning runs on the platform's
    /// compositor or animation thread, so it keeps turning while the UI thread
    /// is busy.
    /// </summary>
    public class ProgressRing : UserControl
    {
        // Spins the ring
        private readonly Animator _ownAnimator;

        private const float DefaultSize = 32.0f;
        private static readonly TimeSpan RevolutionPeriod = TimeSpan.FromSeconds(1);

        private bool _isActive;

        public ProgressRing()
        {
            this._ownAnimator = this.CreateOwnAnimator();

            this.IsActive = true;
        }

        /// <summary>
        /// Whether the ring shows and spins.
        /// </summary>
        public bool IsActive
        {
            get
            {
                return this._isActive;
            }

            set
            {
                this._isActive = value;
                this.IsVisible = value;

                if (value)
                {
                    this._ownAnimator.StartContinuousRotation(RevolutionPeriod);
                }
                else
                {
                    this._ownAnimator.StopContinuousRotation();
                }
            }
        }

        protected override Size MeasureOverride(
            Size availableSize)
        {
            float size = Math.Min(DefaultSize, Math.Min(availableSize.Width, availableSize.Height));
            return new Size(size, size);
        }

        protected override void Render(
            DrawingContext drawContext)
        {
            var bounds = new Rect(0, 0, this.Bounds.Width, this.Bounds.Height);
            drawContext.Clear(Colors.Transparent);

            float size = Math.Min(bounds.Width, bounds.Height);
            float strokeWidth = Math.Max(2, size / 10);
            drawContext.DrawArc(bounds.Center, (size - strokeWidth) / 2, 0, 270, this.Theme.AccentColor, strokeWidth);
        }

        protected override void OnApplyTheme(
            AppTheme theme)
        {
            base.OnApplyTheme(theme);

            this.InvalidateVisual();
        }
    }
}
