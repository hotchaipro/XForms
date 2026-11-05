using System;

namespace XForms.Input
{
    public interface ISwipeGestureDelegate : IGestureRecognizerDelegate
    {
        bool OnSwipeBegan(
            int direction);

        bool OnSwipeMoved(
            int direction,
            float dx);

        bool OnSwipeEnded(
            int direction,
            float dx);

        bool OnSwipeCanceled(
            int direction);
    }

    /// <summary>
    /// A sideways swipe. A touch that moves the drag slop from where it pressed
    /// is a swipe if it went mostly sideways (and the gesture then claims the
    /// pointer from a scrolling parent), or fails if it went mostly up or down
    /// (the parent scrolls). A quick swipe that lifts before any move arrives
    /// is still a swipe. Follow one with <see cref="Swiping"/>, on any view, or
    /// with a delegate. Add a <see cref="TapGestureRecognizer"/> too for taps:
    /// a swipe cancels it.
    /// </summary>
    public class SwipeGestureRecognizer : GestureRecognizer
    {
        // A swipe is sideways if it moved at least this much sideways for
        // each point up or down (about 53 degrees from level, or less)
        private const float SidewaysRatio = 0.75f;

        private readonly ISwipeGestureDelegate _delegate;
        private readonly VelocityTracker _velocity = new VelocityTracker();
        private int _swipeDirection;
        private Point _gestureStart;

        /// <summary>
        /// A swipe for its <see cref="Swiping"/> event, with no delegate.
        /// </summary>
        public SwipeGestureRecognizer()
        {
        }

        public SwipeGestureRecognizer(
            ISwipeGestureDelegate swipeDelegate)
            : base(swipeDelegate)
        {
            this._delegate = swipeDelegate ?? throw new ArgumentNullException(nameof(swipeDelegate));
        }

        /// <summary>
        /// A sideways swipe began, moved, ended or was canceled.
        /// </summary>
        public event EventHandler<SwipingEventArgs> Swiping;

        /// <summary>
        /// How fast the swipe was moving its way as it lifted, in points a
        /// second (negative going back): for a delegate deciding, in
        /// OnSwipeEnded, whether a short swipe was a flick.
        /// </summary>
        public float Velocity
        {
            get;
            private set;
        }

        // A flick on a view inside the swiped one (a row's button) swipes it
        protected override bool IsDragGesture
        {
            get
            {
                return true;
            }
        }

        protected override void OnPointerInput(
            PointerEventArgs touch)
        {
            if (touch.State == PointerInputState.Began)
            {
                this._swipeDirection = 0;
                this._gestureStart = touch.Position;
                this.Velocity = 0;
                this._velocity.Clear();
                this._velocity.Add(touch.Position);
                return;
            }

            if (touch.State == PointerInputState.Canceled)
            {
                if (this.State == GestureState.Recognized)
                {
                    this.Swiping?.Invoke(this, new SwipingEventArgs(SwipePhase.Canceled, this._swipeDirection, 0, 0));
                    this._delegate?.OnSwipeCanceled(this._swipeDirection);
                }

                return;
            }

            this._velocity.Add(touch.Position);

            float rawDx = touch.Position.X - this._gestureStart.X;
            float rawDy = touch.Position.Y - this._gestureStart.Y;

            // Past the slop, the touch is a swipe if it went mostly sideways
            if (this.State == GestureState.Possible)
            {
                float slop = Application.GetDragSlop(touch.DeviceType);
                if (((rawDx * rawDx) + (rawDy * rawDy)) < (slop * slop))
                {
                    return;
                }

                if (Math.Abs(rawDx) < Math.Abs(rawDy) * SidewaysRatio)
                {
                    // Up or down: the parent scrolls
                    this.Fail();
                    return;
                }

                this._swipeDirection = Math.Sign(rawDx);
                this.Recognize();
                this.Swiping?.Invoke(this, new SwipingEventArgs(SwipePhase.Began, this._swipeDirection, 0, 0));
                this._delegate?.OnSwipeBegan(this._swipeDirection);
            }

            // The distance its way (0 once it's gone back past where it began)
            float dx = Math.Max(0, rawDx * this._swipeDirection);

            this.Swiping?.Invoke(this, new SwipingEventArgs(SwipePhase.Moved, this._swipeDirection, dx, 0));
            this._delegate?.OnSwipeMoved(this._swipeDirection, dx);

            if (touch.State == PointerInputState.Ended)
            {
                this.Velocity = this._velocity.Velocity.X * this._swipeDirection;
                this.Swiping?.Invoke(this, new SwipingEventArgs(SwipePhase.Ended, this._swipeDirection, dx, this.Velocity));
                this._delegate?.OnSwipeEnded(this._swipeDirection, dx);
            }
        }
    }
}
