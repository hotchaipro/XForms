using System;

namespace XForms.Controls
{
    /// <summary>
    /// How far a task has got: a bar filled to its value, or, while
    /// indeterminate, a segment moving along it. Its theme's template draws
    /// it.
    /// </summary>
    public class ProgressBar : RangeBase
    {
        public static readonly ViewProperty<ProgressBar, double> ValueProperty = new(nameof(Value), v => v.Value, (v, x) => v.Value = x);

        /// <summary>
        /// The bar's height, unless it's given one.
        /// </summary>
        public const float DefaultHeight = 4.0f;

        private const float DefaultWidth = 200.0f;

        private bool _isIndeterminate;
        private bool _showError;
        private bool _showPaused;

        public ProgressBar()
        {
        }

        /// <summary>
        /// Whether the bar shows that work is happening, without how far it
        /// has got.
        /// </summary>
        public bool IsIndeterminate
        {
            get
            {
                return this._isIndeterminate;
            }

            set
            {
                if (value != this._isIndeterminate)
                {
                    this._isIndeterminate = value;
                    this.OnStateChanged();
                    this.OnPropertyChanged();
                    this.NotifyAccessibilityStateChanged();
                }
            }
        }

        /// <summary>
        /// Whether the bar shows that the task failed.
        /// </summary>
        public bool ShowError
        {
            get
            {
                return this._showError;
            }

            set
            {
                this._showError = value;
                this.OnStateChanged();
            }
        }

        /// <summary>
        /// Whether the bar shows that the task is paused.
        /// </summary>
        public bool ShowPaused
        {
            get
            {
                return this._showPaused;
            }

            set
            {
                this._showPaused = value;
                this.OnStateChanged();
            }
        }

        protected override AccessibilityRole DefaultAccessibilityRole
        {
            get
            {
                return AccessibilityRole.ProgressBar;
            }
        }

        // Indeterminate: no value to read
        public override AccessibilityRange? AccessibilityRange
        {
            get
            {
                return this._isIndeterminate ? null : base.AccessibilityRange;
            }
        }

        protected override Size MeasureOverride(
            Size availableSize)
        {
            float width = float.IsInfinity(availableSize.Width) ? DefaultWidth : availableSize.Width;
            return new Size(width, Math.Min(DefaultHeight, availableSize.Height));
        }
    }
}
