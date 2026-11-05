using System;

namespace XForma.Input
{
    public interface IPanGestureDelegate : IGestureRecognizerDelegate
    {
        void OnPanStarted();

        /// <summary>
        /// The pointer moved; the translation is from where the pan started.
        /// </summary>
        void OnPanChanged(Point translation);

        void OnPanCompleted(Point translation);

        void OnPanCanceled();
    }

    /// <summary>
    /// Recognizes a drag. Once the pointer has moved past the drag slop, the
    /// pan takes the pointer from scrolling parents, unless
    /// <see cref="ShouldBegin"/> declines it.
    /// </summary>
    public class PanGestureRecognizer : GestureRecognizer
    {
        private readonly IPanGestureDelegate _delegate;
        private readonly VelocityTracker _velocity = new VelocityTracker();
        private Point _start;

        public PanGestureRecognizer(
            IPanGestureDelegate gestureDelegate)
            : base(gestureDelegate)
        {
            this._delegate = gestureDelegate ?? throw new ArgumentNullException(nameof(gestureDelegate));
        }

        /// <summary>
        /// Asked once, as the pointer passes the drag slop, with how far it has
        /// moved: false leaves the pointer to others, such as a scrolling
        /// parent (a pan that only goes sideways declines a drag that went
        /// mostly up or down). Null always begins.
        /// </summary>
        public Func<Point, bool> ShouldBegin
        {
            get;
            set;
        }

        /// <summary>
        /// How fast the pan was moving as it lifted, in points a second: for a
        /// delegate deciding, in OnPanCompleted, whether it was a flick.
        /// </summary>
        public Point Velocity
        {
            get;
            private set;
        }

        // A drag on a view inside the panned one pans it
        protected override bool IsDragGesture
        {
            get
            {
                return true;
            }
        }

        private Point Translation(
            Point position)
        {
            return new Point(position.X - this._start.X, position.Y - this._start.Y);
        }

        protected override void OnPointerInput(
            PointerEventArgs e)
        {
            switch (e.State)
            {
                case PointerInputState.Began:
                    this._start = e.Position;
                    this.Velocity = Point.Zero;
                    this._velocity.Clear();
                    this._velocity.Add(e.Position);
                    break;

                case PointerInputState.Moved:
                    this._velocity.Add(e.Position);

                    if (this.State == GestureState.Recognized)
                    {
                        this._delegate.OnPanChanged(this.Translation(e.Position));
                    }
                    else if (e.Position.DistanceTo(this._start) >= Application.GetDragSlop(e.DeviceType))
                    {
                        if (this.ShouldBegin?.Invoke(this.Translation(e.Position)) ?? true)
                        {
                            this.Recognize();
                            this._start = e.Position;
                            this._delegate.OnPanStarted();
                        }
                        else
                        {
                            this.Fail();
                        }
                    }

                    break;

                case PointerInputState.Ended:
                    if (this.State == GestureState.Recognized)
                    {
                        this._velocity.Add(e.Position);
                        this.Velocity = this._velocity.Velocity;
                        this._delegate.OnPanCompleted(this.Translation(e.Position));
                    }

                    break;

                case PointerInputState.Canceled:
                    if (this.State == GestureState.Recognized)
                    {
                        this._delegate.OnPanCanceled();
                    }

                    break;
            }
        }
    }
}
