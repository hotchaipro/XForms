using System;

namespace XForms.Controls
{
    /// <summary>
    /// A value picked by dragging a thumb along a track, with an optional
    /// header above it. A mouse press moves the thumb there and drags it; a
    /// touch drags it once it moves sideways (a vertical drag scrolls), and a
    /// tap moves it. With keyboard focus, the arrow keys step it by
    /// <see cref="SmallChange"/> (Left and Right follow the flow direction),
    /// Page Up and Page Down by <see cref="LargeChange"/>, and Home and End go
    /// to the ends. Its theme's template draws it, in the layout the slider
    /// measures: the header, then the track across the middle of a
    /// <see cref="TouchHeight"/> band, inset by half a thumb at each end.
    /// </summary>
    public class Slider : RangeBase
    {
        public static readonly ViewProperty<Slider, double> ValueProperty = new(nameof(Value), v => v.Value, (v, x) => v.Value = x);

        /// <summary>
        /// The thumb's diameter.
        /// </summary>
        public const float ThumbSize = 20.0f;

        /// <summary>
        /// The height of the band the track runs across, which takes presses.
        /// </summary>
        public const float TouchHeight = 32.0f;

        private const float DefaultWidth = 200.0f;
        private const float HeaderGap = 4.0f;

        private string _header;
        private double _stepFrequency = 1;
        private bool _isDragging;
        private int? _pendingTouch;
        private Point _touchStart;

        public Slider()
        {
            this.Focusable = true;
        }

        /// <summary>
        /// Text above the track, which also names the slider for assistive
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
        /// The steps the value snaps to, from the minimum (0 for none).
        /// </summary>
        public double StepFrequency
        {
            get
            {
                return this._stepFrequency;
            }

            set
            {
                this._stepFrequency = Math.Max(0, value);
                this.Value = this.Value;
            }
        }

        public double SmallChange
        {
            get;
            set;
        } = 1;

        public double LargeChange
        {
            get;
            set;
        } = 10;

        /// <summary>
        /// Whether the thumb is being dragged.
        /// </summary>
        public bool IsDragging
        {
            get
            {
                return this._isDragging;
            }
        }

        protected override AccessibilityRole DefaultAccessibilityRole
        {
            get
            {
                return AccessibilityRole.Slider;
            }
        }

        protected override string DefaultAccessibilityLabel
        {
            get
            {
                return this._header;
            }
        }

        public override bool IsAccessibilityActionSupported(
            AccessibilityAction action)
        {
            if ((action == AccessibilityAction.Increment) || (action == AccessibilityAction.Decrement))
            {
                return this.IsEnabled;
            }

            // Focusable, but nothing to invoke
            return (action != AccessibilityAction.Invoke) && base.IsAccessibilityActionSupported(action);
        }

        public override bool PerformAccessibilityAction(
            AccessibilityAction action)
        {
            if (!this.IsAccessibilityActionSupported(action))
            {
                return false;
            }

            switch (action)
            {
                case AccessibilityAction.Increment:
                    this.Step(this.SmallChange);
                    return true;

                case AccessibilityAction.Decrement:
                    this.Step(-this.SmallChange);
                    return true;

                default:
                    return base.PerformAccessibilityAction(action);
            }
        }

        public override bool SetAccessibilityRangeValue(
            double value)
        {
            if (!this.IsEnabled)
            {
                return false;
            }

            this.Value = value;
            return true;
        }

        protected override double CoerceValue(
            double value)
        {
            double step = this._stepFrequency;
            if ((step > 0) && !double.IsNaN(value))
            {
                value = this.Minimum + (Math.Round((value - this.Minimum) / step) * step);
            }

            return value;
        }

        // User state: the value, when it changed since it was first attached
        private double _initialValue;

        protected override void OnAttaching(
            Theme theme)
        {
            base.OnAttaching(theme);
            this._initialValue = this.Value;
        }

        protected override object GetUserState()
        {
            return (this.Value != this._initialValue) ? this.Value : null;
        }

        protected override void SetUserState(
            object state)
        {
            if (state is double value)
            {
                this.Value = value;
            }
        }

        private TextFormat HeaderFormat
        {
            get
            {
                return this.ActualTheme.Typography.Label.ToTextFormat();
            }
        }

        /// <summary>
        /// Where the header is, or an empty rectangle without one.
        /// </summary>
        public Rect HeaderBounds
        {
            get
            {
                if (string.IsNullOrEmpty(this._header))
                {
                    return new Rect(0, 0, this.Bounds.Width, 0);
                }

                float height = TextLayout.Measure(this._header, this.HeaderFormat).Height;
                return new Rect(0, 0, this.Bounds.Width, height);
            }
        }

        private float BandTop
        {
            get
            {
                float headerHeight = this.HeaderBounds.Height;
                return (headerHeight > 0) ? headerHeight + HeaderGap : 0;
            }
        }

        /// <summary>
        /// Where the track runs: a line across the middle of the touch band,
        /// inset by half a thumb at each end.
        /// </summary>
        public Rect TrackBounds
        {
            get
            {
                const float Inset = ThumbSize / 2;
                return new Rect(Inset, this.BandTop + (TouchHeight / 2), Math.Max(0, this.Bounds.Width - (2 * Inset)), 0);
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
        /// Where along the track the value is: from its start (its right end,
        /// right to left).
        /// </summary>
        public float ValuePosition
        {
            get
            {
                var track = this.TrackBounds;
                float distance = (float)(this.Fraction * track.Width);
                return this.IsRightToLeft ? track.Right - distance : track.X + distance;
            }
        }

        /// <summary>
        /// The thumb's center.
        /// </summary>
        public Point ThumbCenter
        {
            get
            {
                return new Point(this.ValuePosition, this.TrackBounds.Y);
            }
        }

        protected override Size MeasureOverride(
            Size availableSize)
        {
            float width = float.IsInfinity(availableSize.Width) ? DefaultWidth : availableSize.Width;
            float headerHeight = string.IsNullOrEmpty(this._header) ? 0 : TextLayout.Measure(this._header, this.HeaderFormat).Height + HeaderGap;
            return new Size(width, Math.Min(headerHeight + TouchHeight, availableSize.Height));
        }

        private void Step(
            double change)
        {
            this.Value = this.Value + change;
        }

        private void MoveTo(
            Point position)
        {
            var track = this.TrackBounds;
            if (track.Width <= 0)
            {
                return;
            }

            double fraction = Math.Clamp((position.X - track.X) / track.Width, 0, 1);
            if (this.IsRightToLeft)
            {
                fraction = 1 - fraction;
            }

            this.Value = this.Minimum + (fraction * (this.Maximum - this.Minimum));
        }

        private void StartDragging(
            PointerEventArgs e)
        {
            this._isDragging = true;
            this._pendingTouch = null;
            this.CapturePointer(e);
            this.IsPressed = true;
            this.MoveTo(e.Position);
        }

        protected override void OnPointerPressed(
            PointerEventArgs e)
        {
            if (!this.IsEnabled)
            {
                return;
            }

            e.Handled = true;
            if (e.DeviceType == PointerDeviceType.Touch)
            {
                // A touch waits to see which way it moves
                this._pendingTouch = e.PointerId;
                this._touchStart = e.Position;
                return;
            }

            this.StartDragging(e);
        }

        protected override void OnPointerMoved(
            PointerEventArgs e)
        {
            if (this._isDragging)
            {
                this.MoveTo(e.Position);
                e.Handled = true;
                return;
            }

            if (this._pendingTouch == e.PointerId)
            {
                float dx = Math.Abs(e.Position.X - this._touchStart.X);
                float dy = Math.Abs(e.Position.Y - this._touchStart.Y);
                if ((dx > Application.TouchSlop) && (dx >= dy))
                {
                    this.StartDragging(e);
                    e.Handled = true;
                }
                else if (dy > Application.TouchSlop)
                {
                    // Vertical: the page scrolls
                    this._pendingTouch = null;
                }
            }
        }

        protected override void OnPointerReleased(
            PointerEventArgs e)
        {
            if (this._pendingTouch == e.PointerId)
            {
                // A tap moves the thumb there
                this._pendingTouch = null;
                this.MoveTo(e.Position);
            }
            else if (this._isDragging)
            {
                // Where it's let go (the last move may not have come)
                this.MoveTo(e.Position);
            }

            this.EndDragging(e);
            e.Handled = true;
        }

        protected override void OnPointerCanceled(
            PointerEventArgs e)
        {
            this._pendingTouch = null;
            this.EndDragging(e);
        }

        private void EndDragging(
            PointerEventArgs e)
        {
            if (this._isDragging)
            {
                this._isDragging = false;
                this.ReleasePointerCapture(e);
            }

            this.IsPressed = false;
        }

        protected override void OnKeyDown(
            KeyEventArgs keyInputEvent)
        {
            if (!this.IsEnabled)
            {
                return;
            }

            // Left steps toward the start: down left to right, up right to left
            double forward = this.IsRightToLeft ? -this.SmallChange : this.SmallChange;
            switch (keyInputEvent.Key)
            {
                case Key.Left:
                    this.Step(-forward);
                    break;

                case Key.Right:
                    this.Step(forward);
                    break;

                case Key.Up:
                    this.Step(this.SmallChange);
                    break;

                case Key.Down:
                    this.Step(-this.SmallChange);
                    break;

                case Key.PageUp:
                    this.Step(this.LargeChange);
                    break;

                case Key.PageDown:
                    this.Step(-this.LargeChange);
                    break;

                case Key.Home:
                    this.Value = this.Minimum;
                    break;

                case Key.End:
                    this.Value = this.Maximum;
                    break;

                default:
                    return;
            }

            keyInputEvent.Handled = true;
        }
    }
}
