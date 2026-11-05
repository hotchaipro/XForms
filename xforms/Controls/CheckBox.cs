using System;
using XForms.Input;

namespace XForms.Controls
{
    /// <summary>
    /// A box and its text, checked or unchecked by a tap, Space or Enter, and,
    /// when <see cref="IsThreeState"/>, also indeterminate (neither): it goes
    /// from unchecked to checked to indeterminate. Its theme's template draws
    /// it, in the layout the check box measures from the theme's
    /// <see cref="CheckBoxStyle"/>: a box at the start, then the text.
    /// </summary>
    public class CheckBox : TemplatedControl, ITapGestureDelegate
    {
        public static readonly ViewProperty<CheckBox, bool?> IsCheckedProperty = new(nameof(IsChecked), v => v.IsChecked, (v, x) => v.IsChecked = x);

        private string _text;
        private bool? _isChecked = false;

        public CheckBox()
        {
            // As wide as its text, so a press beside it isn't on it
            this.HorizontalAlignment = HorizontalAlignment.Left;
            this.Focusable = true;
            this.GestureRecognizers.Add(new TapGestureRecognizer(this));
        }

        /// <summary>
        /// Raised when <see cref="IsChecked"/> changes.
        /// </summary>
        public event EventHandler IsCheckedChanged;

        public string Text
        {
            get
            {
                return this._text;
            }

            set
            {
                this._text = value;
                this.InvalidateMeasure();
                this.InvalidateVisual();
            }
        }

        /// <summary>
        /// Checked (true), unchecked (false) or indeterminate (null).
        /// </summary>
        public bool? IsChecked
        {
            get
            {
                return this._isChecked;
            }

            set
            {
                if (value != this._isChecked)
                {
                    this._isChecked = value;
                    this.OnStateChanged();
                    this.OnPropertyChanged();
                    this.NotifyAccessibilityStateChanged();
                    this.IsCheckedChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Whether a tap can make the box indeterminate.
        /// </summary>
        public bool IsThreeState
        {
            get;
            set;
        }

        protected override AccessibilityRole DefaultAccessibilityRole
        {
            get
            {
                return AccessibilityRole.CheckBox;
            }
        }

        protected override string DefaultAccessibilityLabel
        {
            get
            {
                return this._text;
            }
        }

        // Toggled, not invoked
        public override bool IsAccessibilityActionSupported(
            AccessibilityAction action)
        {
            return (action != AccessibilityAction.Invoke) && base.IsAccessibilityActionSupported(action);
        }

        public override bool? AccessibilityIsChecked
        {
            get
            {
                return this._isChecked ?? false;
            }
        }

        public override bool AccessibilityIsMixed
        {
            get
            {
                return !this._isChecked.HasValue;
            }
        }

        // User state: the checked state, when it changed since it was first
        // attached
        private bool? _initialIsChecked;

        protected override void OnAttaching(
            Theme theme)
        {
            base.OnAttaching(theme);
            this._initialIsChecked = this.IsChecked;
        }

        protected override object GetUserState()
        {
            return (this.IsChecked != this._initialIsChecked) ? new CheckState(this.IsChecked) : null;
        }

        protected override void SetUserState(
            object state)
        {
            if (state is CheckState checkState)
            {
                this.IsChecked = checkState.IsChecked;
            }
        }

        private sealed record CheckState(bool? IsChecked);

        /// <summary>
        /// The text's format, which the check box measures with.
        /// </summary>
        public TextFormat CreateTextFormat()
        {
            var format = this.ActualTheme.Typography.Body.ToTextFormat();
            format.HorizontalAlignment = this.IsRightToLeft ? HorizontalAlignment.Right : HorizontalAlignment.Left;
            format.VerticalAlignment = VerticalAlignment.Center;
            return format;
        }

        private bool IsRightToLeft
        {
            get
            {
                return this.FlowDirection == FlowDirection.RightToLeft;
            }
        }

        /// <summary>
        /// Where the box is: at the start, centered vertically.
        /// </summary>
        public Rect BoxBounds
        {
            get
            {
                float size = this.ActualTheme.CheckBoxStyle.BoxSize;
                float x = this.IsRightToLeft ? this.Bounds.Width - size : 0;
                return new Rect(x, (this.Bounds.Height - size) / 2, size, size);
            }
        }

        /// <summary>
        /// Where the text is: after the box.
        /// </summary>
        public Rect TextBounds
        {
            get
            {
                var style = this.ActualTheme.CheckBoxStyle;
                float width = Math.Max(0, this.Bounds.Width - style.BoxSize - style.TextGap);
                float x = this.IsRightToLeft ? 0 : style.BoxSize + style.TextGap;
                return new Rect(x, 0, width, this.Bounds.Height);
            }
        }

        protected override Size MeasureOverride(
            Size availableSize)
        {
            var style = this.ActualTheme.CheckBoxStyle;
            float width = style.BoxSize;
            float height = style.BoxSize;
            if (!string.IsNullOrEmpty(this._text))
            {
                var textSize = TextLayout.Measure(this._text, this.CreateTextFormat());
                width += style.TextGap + textSize.Width;
                height = Math.Max(height, textSize.Height);
            }

            return new Size(Math.Min(width, availableSize.Width), Math.Min(Math.Max(height, style.MinimumHeight), availableSize.Height));
        }

        /// <summary>
        /// Moves to the next state: unchecked, checked, then (when three-state)
        /// indeterminate.
        /// </summary>
        private void Toggle()
        {
            if (!this.IsEnabled)
            {
                return;
            }

            if (this._isChecked == false)
            {
                this.IsChecked = true;
            }
            else if ((this._isChecked == true) && this.IsThreeState)
            {
                this.IsChecked = null;
            }
            else
            {
                this.IsChecked = false;
            }
        }

        protected override void OnActivate()
        {
            this.Toggle();
        }

        #region ITapGestureDelegate implementation

        void ITapGestureDelegate.OnTapBegan()
        {
            this.IsPressed = this.IsEnabled;
        }

        void ITapGestureDelegate.OnTapped()
        {
            this.Toggle();
        }

        void ITapGestureDelegate.OnTapEnded()
        {
            this.IsPressed = false;
        }

        void IGestureRecognizerDelegate.OnTouchBegan()
        {
        }

        void IGestureRecognizerDelegate.OnTouchEnded()
        {
        }

        void IGestureRecognizerDelegate.OnTouchCanceled()
        {
        }

        #endregion
    }
}
