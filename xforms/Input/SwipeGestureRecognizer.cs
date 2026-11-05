using System;

namespace XForms.Input
{
    public interface ISwipeGestureDelegate : ITapGestureDelegate
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
    /// A sideways swipe (or a tap). A touch that moves
    /// <see cref="Slop"/> from where it pressed is a swipe if it went mostly
    /// sideways (and the gesture then claims the pointer from a scrolling
    /// parent), or nothing if it went mostly up or down (the parent scrolls);
    /// one that lifts within the slop is a tap. A quick swipe that lifts
    /// before any move arrives is still a swipe. Follow one with
    /// <see cref="Swiping"/>, on any view, or with a delegate.
    /// </summary>
    public class SwipeGestureRecognizer : GestureRecognizer
    {
        /// <summary>
        /// How far a touch moves before it's a swipe (or a scroll), not a tap.
        /// </summary>
        public const float Slop = 10.0f;

        // A swipe is sideways if it moved at least this much sideways for
        // each point up or down (about 53 degrees from level, or less)
        private const float SidewaysRatio = 0.75f;

        // The recent moves, for the swipe's speed as it lifts
        private const int VelocitySamples = 4;
        private static readonly long VelocityWindow = System.Diagnostics.Stopwatch.Frequency / 10;
        private readonly long[] _sampleTimes = new long[VelocitySamples];
        private readonly float[] _sampleDistances = new float[VelocitySamples];
        private int _sampleCount;

        private ISwipeGestureDelegate _delegate;
        private int _swipeDirection;
        private Point _gestureStart;

        /// <summary>
        /// A swipe for its <see cref="Swiping"/> event, with no delegate.
        /// </summary>
        public SwipeGestureRecognizer()
            : this(new NoDelegate())
        {
        }

        public SwipeGestureRecognizer(
            ISwipeGestureDelegate swipeDelegate)
            : base(swipeDelegate)
        {
            if (null == swipeDelegate)
            {
                throw new ArgumentNullException(nameof(swipeDelegate));
            }

            this._delegate = swipeDelegate;
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
            this._delegate.OnTapped();
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

                    this._swipeDirection = 0;
                    this._gestureStart = touch.Position;
                    this._sampleCount = 0;
                    this.Velocity = 0;

                    this._delegate.OnTapBegan();
                }

                return;
            }

            if (this.State == GestureState.Possible)
            {
                return;
            }

            float rawDx = (float)(touch.Position.X - this._gestureStart.X);
            float rawDy = (float)(touch.Position.Y - this._gestureStart.Y);

            // Past the slop, the touch is a swipe if it went mostly sideways
            if ((this.State == GestureState.Began)
                && ((touch.State == PointerInputState.Moved) || (touch.State == PointerInputState.Ended))
                && (((rawDx * rawDx) + (rawDy * rawDy)) >= (Slop * Slop)))
            {
                if (Math.Abs(rawDx) >= Math.Abs(rawDy) * SidewaysRatio)
                {
                    this._swipeDirection = Math.Sign(rawDx);
                    this.State = GestureState.Changed;
                    this.AddSample(0);
                    this.Swiping?.Invoke(this, new SwipingEventArgs(SwipePhase.Began, this._swipeDirection, 0, 0));
                    this._delegate.OnSwipeBegan(this._swipeDirection);
                }
                else
                {
                    // Up or down: the parent scrolls, and it's no tap
                    this.State = GestureState.Failed;
                    this._delegate.OnTapEnded();
                    result.DidHandleEvent = false;
                    return;
                }
            }

            // The distance its way (0 once it's gone back past where it began)
            float dx = Math.Max(0, rawDx * this._swipeDirection);

            if (touch.State == PointerInputState.Moved)
            {
                if (this.State == GestureState.Changed)
                {
                    this.AddSample(dx);
                    this.Swiping?.Invoke(this, new SwipingEventArgs(SwipePhase.Moved, this._swipeDirection, dx, 0));
                    this._delegate.OnSwipeMoved(this._swipeDirection, dx);
                }

                // NOTE: A handled move claims the pointer exclusively, so only
                // claim it once a swipe is under way; until then (and after a
                // vertical drag fails the swipe) a scrolling parent may take over
                result.DidHandleEvent = (this.State == GestureState.Changed);
            }
            else if (touch.State == PointerInputState.Ended)
            {
                if (this.State == GestureState.Began)
                {
                    // Within the slop: a tap
                    this.State = GestureState.Recognized;

                    this._delegate.OnTapped();

                    this._delegate.OnTapEnded();
                }
                else if (this.State == GestureState.Changed)
                {
                    this.AddSample(dx);
                    this.Velocity = this.MeasureVelocity();
                    this.Swiping?.Invoke(this, new SwipingEventArgs(SwipePhase.Moved, this._swipeDirection, dx, 0));
                    this._delegate.OnSwipeMoved(this._swipeDirection, dx);
                    this.Swiping?.Invoke(this, new SwipingEventArgs(SwipePhase.Ended, this._swipeDirection, dx, this.Velocity));
                    this._delegate.OnSwipeEnded(this._swipeDirection, dx);

                    this._delegate.OnTapEnded();

                    this.State = GestureState.Recognized;
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
                    this._delegate.OnTapEnded();
                }
                else if (this.State == GestureState.Changed)
                {
                    this.Swiping?.Invoke(this, new SwipingEventArgs(SwipePhase.Canceled, this._swipeDirection, 0, 0));
                    this._delegate.OnSwipeCanceled(this._swipeDirection);

                    this._delegate.OnTapEnded();
                }

                if (this.State != GestureState.Possible)
                {
                    this.State = GestureState.Canceled;
                }
            }
        }

        private void AddSample(
            float distance)
        {
            if (this._sampleCount == VelocitySamples)
            {
                Array.Copy(this._sampleTimes, 1, this._sampleTimes, 0, VelocitySamples - 1);
                Array.Copy(this._sampleDistances, 1, this._sampleDistances, 0, VelocitySamples - 1);
                this._sampleCount -= 1;
            }

            this._sampleTimes[this._sampleCount] = System.Diagnostics.Stopwatch.GetTimestamp();
            this._sampleDistances[this._sampleCount] = distance;
            this._sampleCount += 1;
        }

        // Points a second, over the recent moves (a finger that stopped before
        // it lifted has none)
        private float MeasureVelocity()
        {
            int last = this._sampleCount - 1;
            if (last < 1)
            {
                return 0;
            }

            long now = this._sampleTimes[last];
            int first = last;
            while ((first > 0) && ((now - this._sampleTimes[first - 1]) <= VelocityWindow))
            {
                first -= 1;
            }

            if (first == last)
            {
                return 0;
            }

            double seconds = (double)(now - this._sampleTimes[first]) / System.Diagnostics.Stopwatch.Frequency;
            return (seconds > 0) ? (float)((this._sampleDistances[last] - this._sampleDistances[first]) / seconds) : 0;
        }

        private sealed class NoDelegate : ISwipeGestureDelegate
        {
            public bool OnSwipeBegan(int direction) => false;

            public bool OnSwipeMoved(int direction, float dx) => false;

            public bool OnSwipeEnded(int direction, float dx) => false;

            public bool OnSwipeCanceled(int direction) => false;

            public void OnTapBegan()
            {
            }

            public void OnTapEnded()
            {
            }

            public void OnTapped()
            {
            }

            public void OnTouchBegan()
            {
            }

            public void OnTouchEnded()
            {
            }

            public void OnTouchCanceled()
            {
            }
        }
    }
}
