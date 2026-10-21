using System;

namespace XForms.Input
{
    public interface ITapGestureDelegate : IGestureRecognizerDelegate
    {
        void OnTapBegan();

        void OnTapEnded();

        void OnTapped();
    }

    public sealed class TappedEventArgs : EventArgs
    {
        public TappedEventArgs(
            Point position,
            PointerDeviceType deviceType)
        {
            this.Position = position;
            this.DeviceType = deviceType;
        }

        /// <summary>
        /// Where the tap was, relative to the view.
        /// </summary>
        public Point Position
        {
            get;
        }

        public PointerDeviceType DeviceType
        {
            get;
        }
    }

    public enum HoldingState
    {
        /// <summary>
        /// The pointer has been held down long enough.
        /// </summary>
        Started = 0,

        /// <summary>
        /// The pointer was released after holding.
        /// </summary>
        Completed,

        /// <summary>
        /// The pointer moved away or was canceled after holding.
        /// </summary>
        Canceled,
    }

    public sealed class HoldingEventArgs : EventArgs
    {
        public HoldingEventArgs(
            Point position,
            HoldingState state)
        {
            this.Position = position;
            this.HoldingState = state;
        }

        public Point Position
        {
            get;
        }

        public HoldingState HoldingState
        {
            get;
        }
    }

    /// <summary>
    /// Recognizes taps (a press and release without moving), double taps (a
    /// second tap soon after a first, close to it) and holds (a press held
    /// without moving). A control handles them through its
    /// <see cref="ITapGestureDelegate"/>, which also tells it when a press
    /// begins and ends (for pressed feedback); an app handles the events.
    /// Activating the view (Enter, Space, a screen reader) taps it, so a view
    /// with a tap recognizer is focusable.
    /// </summary>
    public class TapGestureRecognizer : GestureRecognizer
    {
        private static readonly TimeSpan HoldTime = TimeSpan.FromMilliseconds(500);
        private static readonly TimeSpan DoubleTapTime = TimeSpan.FromMilliseconds(500);

        private readonly ITapGestureDelegate _delegate;
        private Point _gestureStart;
        private PointerDeviceType _deviceType;
        private DispatcherTimer _holdTimer;
        private bool _isHolding;
        private DateTime _lastTapTime;
        private Point _lastTapPosition;

        /// <summary>
        /// A recognizer whose taps an app handles with its events.
        /// </summary>
        public TapGestureRecognizer()
        {
        }

        /// <summary>
        /// A recognizer whose taps a control handles through its delegate.
        /// </summary>
        public TapGestureRecognizer(
            ITapGestureDelegate gestureDelegate)
            : base(gestureDelegate)
        {
            this._delegate = gestureDelegate ?? throw new ArgumentNullException(nameof(gestureDelegate));
        }

        /// <summary>
        /// Raised when a pointer is pressed and released over the view without
        /// moving, or the view is activated.
        /// </summary>
        public event EventHandler<TappedEventArgs> Tapped;

        /// <summary>
        /// Raised for a second tap soon after a first, close to it.
        /// </summary>
        public event EventHandler<TappedEventArgs> DoubleTapped;

        /// <summary>
        /// Raised when a pointer is held down on the view without moving
        /// (<see cref="HoldingState.Started"/>), and again when it's released
        /// or moves away. A held pointer doesn't also tap.
        /// </summary>
        public event EventHandler<HoldingEventArgs> Holding;

        /// <summary>
        /// Whether a press that may yet be a tap is down on the view (for
        /// pressed feedback).
        /// </summary>
        public bool IsPressed
        {
            get
            {
                return this.State == GestureState.Began;
            }
        }

        public override void Reset()
        {
            base.Reset();

            this._gestureStart = Point.Zero;
        }

        // Activation (Enter, Space, a screen reader) is a tap, which also
        // makes the view focusable
        protected override void OnAttached(
            View view)
        {
            view.Activated += this.View_Activated;
        }

        protected override void OnDetached(
            View view)
        {
            view.Activated -= this.View_Activated;
        }

        private void View_Activated(
            object sender,
            EventArgs e)
        {
            this._delegate?.OnTapped();

            var view = this.View;
            var center = new Point(view.Bounds.Width / 2, view.Bounds.Height / 2);
            this.Tapped?.Invoke(view, new TappedEventArgs(center, PointerDeviceType.Mouse));
        }

        protected override void OnPointerInput(
            PointerEventArgs touch,
            ref GestureEventResult result)
        {
            result.DidHandleEvent = true;

            if (touch.State == PointerInputState.Began)
            {
                if (this.State == GestureState.Possible)
                {
                    this.State = GestureState.Began;
                    this._gestureStart = touch.Position;
                    this._deviceType = touch.DeviceType;
                    this._isHolding = false;

                    if (null != this.Holding)
                    {
                        this._holdTimer ??= new DispatcherTimer(HoldTime, this.OnHoldTime);
                        this._holdTimer.Start();
                    }

                    this._delegate?.OnTapBegan();
                }
            }
            else if (touch.State == PointerInputState.Moved)
            {
                if (this.State == GestureState.Began)
                {
                    double distance = touch.Position.DistanceTo(this._gestureStart);

                    if (distance > Application.TouchSlop)
                    {
                        this.Fail();
                    }
                }

                // NOTE: A tap never claims the pointer exclusively, so a drag that
                // starts on a tappable view can still scroll its parent
                result.DidHandleEvent = false;
            }
            else if (touch.State == PointerInputState.Ended)
            {
                if (this.State == GestureState.Began)
                {
                    this._holdTimer?.Stop();
                    double distance = touch.Position.DistanceTo(this._gestureStart);

                    if (this._isHolding)
                    {
                        this._isHolding = false;
                        this.State = GestureState.Failed;
                        this.Holding?.Invoke(this.View, new HoldingEventArgs(touch.Position, HoldingState.Completed));
                    }
                    else if (distance <= Application.TouchSlop)
                    {
                        this.State = GestureState.Recognized;
                        this._delegate?.OnTapped();
                        this.OnTap(touch.Position);
                    }
                    else
                    {
                        this.State = GestureState.Failed;
                    }

                    this._delegate?.OnTapEnded();
                }
                else
                {
                    this.State = GestureState.Failed;
                }
            }
            else if (touch.State == PointerInputState.Canceled)
            {
                if (this.State == GestureState.Began)
                {
                    this.Fail();
                }
                else
                {
                    this.State = GestureState.Failed;
                }
            }
        }

        // A tap, and a double tap if it's the second soon after a first
        private void OnTap(
            Point position)
        {
            var now = DateTime.UtcNow;
            bool isDouble = ((now - this._lastTapTime) <= DoubleTapTime)
                && (position.DistanceTo(this._lastTapPosition) <= 2 * Application.TouchSlop);

            var args = new TappedEventArgs(position, this._deviceType);
            this.Tapped?.Invoke(this.View, args);

            if (isDouble)
            {
                // A third tap starts over
                this._lastTapTime = DateTime.MinValue;
                this.DoubleTapped?.Invoke(this.View, args);
            }
            else
            {
                this._lastTapTime = now;
                this._lastTapPosition = position;
            }
        }

        private void OnHoldTime()
        {
            this._holdTimer.Stop();
            if (this.State != GestureState.Began)
            {
                return;
            }

            this._isHolding = true;
            this.Holding?.Invoke(this.View, new HoldingEventArgs(this._gestureStart, HoldingState.Started));
        }

        // The press moved away or was canceled: no tap (or the end of a hold)
        private void Fail()
        {
            this._holdTimer?.Stop();
            this.State = GestureState.Failed;

            if (this._isHolding)
            {
                this._isHolding = false;
                this.Holding?.Invoke(this.View, new HoldingEventArgs(this._gestureStart, HoldingState.Canceled));
            }

            this._delegate?.OnTapEnded();
        }
    }
}
