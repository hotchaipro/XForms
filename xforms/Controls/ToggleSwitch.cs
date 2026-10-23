using System;
using XForms.Input;

namespace XForms.Controls
{
    /// <summary>
    /// An on and off switch, with an optional header above it and its state's
    /// text beside it, turned by a tap, Space or Enter. Its theme's template
    /// draws it, in the layout the switch measures: the header, then a
    /// <see cref="TrackWidth"/> by <see cref="TrackHeight"/> track at the start
    /// of a <see cref="TouchHeight"/> band, then the text after
    /// <see cref="TextGap"/>.
    /// </summary>
    public class ToggleSwitch : TemplatedControl, ITapGestureDelegate
    {
        public static readonly ViewProperty<ToggleSwitch, bool> IsOnProperty = new(nameof(IsOn), v => v.IsOn, (v, x) => v.IsOn = x);

        public const float TrackWidth = 40.0f;
        public const float TrackHeight = 20.0f;
        public const float TouchHeight = 32.0f;
        public const float TextGap = 12.0f;

        private const float HeaderGap = 4.0f;

        private string _header;
        private string _onContent = "On";
        private string _offContent = "Off";
        private bool _isOn;

        public ToggleSwitch()
        {
            // As wide as its header and text, so a press beside it isn't on it
            this.HorizontalAlignment = HorizontalAlignment.Left;
            this.Focusable = true;
            this.GestureRecognizers.Add(new TapGestureRecognizer(this));
        }

        /// <summary>
        /// Raised when <see cref="IsOn"/> changes.
        /// </summary>
        public event EventHandler<ToggledEventArgs> Toggled;

        /// <summary>
        /// Text above the switch, which also names it for assistive
        /// technology.
        /// </summary>
        public string Header
        {
            get
            {
                return this._header;
            }

            set
            {
                this._header = value;
                this.InvalidateMeasure();
                this.InvalidateVisual();
            }
        }

        /// <summary>
        /// The text beside the switch while it's on.
        /// </summary>
        public string OnContent
        {
            get
            {
                return this._onContent;
            }

            set
            {
                this._onContent = value;
                this.InvalidateMeasure();
                this.InvalidateVisual();
            }
        }

        /// <summary>
        /// The text beside the switch while it's off.
        /// </summary>
        public string OffContent
        {
            get
            {
                return this._offContent;
            }

            set
            {
                this._offContent = value;
                this.InvalidateMeasure();
                this.InvalidateVisual();
            }
        }

        public bool IsOn
        {
            get
            {
                return this._isOn;
            }

            set
            {
                if (value != this._isOn)
                {
                    this._isOn = value;
                    this.OnStateChanged();
                    this.OnPropertyChanged();
                    this.NotifyAccessibilityStateChanged();
                    this.Toggled?.Invoke(this, new ToggledEventArgs(value));
                }
            }
        }

        /// <summary>
        /// The text beside the switch now.
        /// </summary>
        public string StateText
        {
            get
            {
                return this._isOn ? this._onContent : this._offContent;
            }
        }

        protected override AccessibilityRole DefaultAccessibilityRole
        {
            get
            {
                return AccessibilityRole.Switch;
            }
        }

        protected override string DefaultAccessibilityLabel
        {
            get
            {
                return this._header ?? this.StateText;
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
                return this._isOn;
            }
        }

        // User state: whether it's on, when that changed since it was first
        // attached
        private bool _initialIsOn;

        protected override void OnAttaching(
            Theme theme)
        {
            base.OnAttaching(theme);
            this._initialIsOn = this.IsOn;
        }

        protected override object GetUserState()
        {
            return (this.IsOn != this._initialIsOn) ? this.IsOn : null;
        }

        protected override void SetUserState(
            object state)
        {
            if (state is bool isOn)
            {
                this.IsOn = isOn;
            }
        }

        public TextFormat CreateHeaderFormat()
        {
            return this.ActualTheme.Typography.Label.ToTextFormat();
        }

        public TextFormat CreateTextFormat()
        {
            var format = this.ActualTheme.Typography.Body.ToTextFormat();
            format.HorizontalAlignment = this.IsRightToLeft ? HorizontalAlignment.Right : HorizontalAlignment.Left;
            format.VerticalAlignment = VerticalAlignment.Center;
            return format;
        }

        /// <summary>
        /// Where the header is (empty without one).
        /// </summary>
        public Rect HeaderBounds
        {
            get
            {
                float height = string.IsNullOrEmpty(this._header) ? 0 : TextLayout.Measure(this._header, this.CreateHeaderFormat()).Height;
                return new Rect(0, 0, this.Bounds.Width, height);
            }
        }

        private float BandTop
        {
            get
            {
                float height = this.HeaderBounds.Height;
                return (height > 0) ? height + HeaderGap : 0;
            }
        }

        private bool IsRightToLeft
        {
            get
            {
                return this.FlowDirection == FlowDirection.RightToLeft;
            }
        }

        /// <summary>
        /// Where the track is: at the start of the band.
        /// </summary>
        public Rect TrackBounds
        {
            get
            {
                float y = this.BandTop + ((TouchHeight - TrackHeight) / 2);
                float x = this.IsRightToLeft ? this.Bounds.Width - TrackWidth : 0;
                return new Rect(x, y, TrackWidth, TrackHeight);
            }
        }

        /// <summary>
        /// Where the state's text is: after the track.
        /// </summary>
        public Rect TextBounds
        {
            get
            {
                float width = Math.Max(0, this.Bounds.Width - TrackWidth - TextGap);
                float x = this.IsRightToLeft ? 0 : TrackWidth + TextGap;
                return new Rect(x, this.BandTop, width, TouchHeight);
            }
        }

        protected override Size MeasureOverride(
            Size availableSize)
        {
            var format = this.CreateTextFormat();
            float textWidth = Math.Max(
                TextLayout.Measure(this._onContent ?? string.Empty, format).Width,
                TextLayout.Measure(this._offContent ?? string.Empty, format).Width);
            float width = TrackWidth + ((textWidth > 0) ? TextGap + textWidth : 0);
            float height = TouchHeight;
            if (!string.IsNullOrEmpty(this._header))
            {
                var headerSize = TextLayout.Measure(this._header, this.CreateHeaderFormat());
                width = Math.Max(width, headerSize.Width);
                height += headerSize.Height + HeaderGap;
            }

            return new Size(Math.Min(width, availableSize.Width), Math.Min(height, availableSize.Height));
        }

        private void Toggle()
        {
            if (this.IsEnabled)
            {
                this.IsOn = !this._isOn;
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
