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
        private const float DefaultSize = 32.0f;
        private static readonly TimeSpan RevolutionPeriod = TimeSpan.FromSeconds(1);

        private bool _isActive;

        public ProgressRing()
        {
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
                    this.StartContinuousRotation(RevolutionPeriod);
                }
                else
                {
                    this.StopContinuousRotation();
                }
            }
        }

        protected override Size MeasureOverride(
            Size availableSize)
        {
            float size = Math.Min(DefaultSize, Math.Min(availableSize.Width, availableSize.Height));
            return new Size(size, size);
        }

        protected override void DrawBackground(
            IDrawContext drawContext,
            Rectangle bounds)
        {
            drawContext.Clear(Colors.Transparent);

            float size = Math.Min(bounds.Width, bounds.Height);
            float strokeWidth = Math.Max(2, size / 10);
            drawContext.DrawArc(bounds.Center, (size - strokeWidth) / 2, 0, 270, this.Theme.AccentColor, strokeWidth);
        }

        protected override void OnApplyTheme(
            AppTheme theme)
        {
            base.OnApplyTheme(theme);

            this.Invalidate();
        }
    }
}
