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
    /// begins and ends (for pressed feedback); an app handles the events. A
    /// touch that may yet become a drag (<see cref="View.IsDragPossible"/>)
    /// begins its press a moment later, or as it lifts, so a swipe or a scroll
    /// shows no pressed look.
    /// Activating the view (Enter, Space, a screen reader) taps it, so a view
    /// with a tap recognizer is focusable.
    /// </summary>
    public class TapGestureRecognizer : GestureRecognizer
    {
        private static readonly TimeSpan HoldTime = TimeSpan.FromMilliseconds(500);
        private static readonly TimeSpan DoubleTapTime = TimeSpan.FromMilliseconds(500);

        // How long a touch that may yet become a drag holds back its pressed look
        private static readonly TimeSpan PressDelay = TimeSpan.FromMilliseconds(100);

        private readonly ITapGestureDelegate _delegate;
        private Point _gestureStart;
        private PointerDeviceType _deviceType;
        private DispatcherTimer _holdTimer;
        private DispatcherTimer _pressTimer;
        private bool _isPressShown;
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
                return this._isPressShown;
            }
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
            PointerEventArgs touch)
        {
            if (touch.State == PointerInputState.Began)
            {
                this._gestureStart = touch.Position;
                this._deviceType = touch.DeviceType;
                this._isHolding = false;
                this._isPressShown = false;

                if (null != this.Holding)
                {
                    this._holdTimer ??= new DispatcherTimer(HoldTime, this.OnHoldTime);
                    this._holdTimer.Start();
                }

                if ((touch.DeviceType != PointerDeviceType.Mouse) && this.View.IsDragPossible)
                {
                    this._pressTimer ??= new DispatcherTimer(PressDelay, this.OnPressTime);
                    this._pressTimer.Start();
                }
                else
                {
                    this.ShowPress();
                }
            }
            else if (touch.State == PointerInputState.Moved)
            {
                // NOTE: A tap only recognizes as it lifts, so a drag that starts
                // on a tappable view can still scroll its parent
                if (touch.Position.DistanceTo(this._gestureStart) > Application.TapSlop)
                {
                    this.EndPress();
                }
            }
            else if (touch.State == PointerInputState.Ended)
            {
                this._holdTimer?.Stop();
                this._pressTimer?.Stop();

                if (this._isHolding)
                {
                    this._isHolding = false;
                    this.Fail();
                    this.Holding?.Invoke(this.View, new HoldingEventArgs(touch.Position, HoldingState.Completed));
                }
                else if (touch.Position.DistanceTo(this._gestureStart) <= Application.TapSlop)
                {
                    // A quick tap's press shows as it lifts
                    this.ShowPress();
                    this.Recognize();
                    this._delegate?.OnTapped();
                    this.OnTap(touch.Position);
                }
                else
                {
                    this.Fail();
                }

                this.HidePress();
            }
            else if (touch.State == PointerInputState.Canceled)
            {
                this.EndPress();
            }
        }

        // A tap, and a double tap if it's the second soon after a first
        private void OnTap(
            Point position)
        {
            var now = DateTime.UtcNow;
            bool isDouble = ((now - this._lastTapTime) <= DoubleTapTime)
                && (position.DistanceTo(this._lastTapPosition) <= 2 * Application.TapSlop);

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

        private void ShowPress()
        {
            if (!this._isPressShown)
            {
                this._isPressShown = true;
                this._delegate?.OnTapBegan();
            }
        }

        private void HidePress()
        {
            if (this._isPressShown)
            {
                this._isPressShown = false;
                this._delegate?.OnTapEnded();
            }
        }

        // Still a press after the delay: its look shows
        internal void OnPressTime()
        {
            this._pressTimer?.Stop();
            if (this.State == GestureState.Possible)
            {
                this.ShowPress();
            }
        }

        private void OnHoldTime()
        {
            this._holdTimer.Stop();
            if (this.State != GestureState.Possible)
            {
                return;
            }

            this._isHolding = true;
            this.Holding?.Invoke(this.View, new HoldingEventArgs(this._gestureStart, HoldingState.Started));
        }

        // The press moved away or was canceled: no tap (or the end of a hold)
        private void EndPress()
        {
            this._holdTimer?.Stop();
            this._pressTimer?.Stop();
            this.Fail();

            if (this._isHolding)
            {
                this._isHolding = false;
                this.Holding?.Invoke(this.View, new HoldingEventArgs(this._gestureStart, HoldingState.Canceled));
            }

            this.HidePress();
        }
    }
}
