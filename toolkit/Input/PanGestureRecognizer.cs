using System;

namespace XForms.Input
{
    [Flags]
    public enum PanDirections
    {
        Horizontal = 1,
        Vertical = 2,
        Both = Horizontal | Vertical,
    }

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
    /// Recognizes a drag in the allowed directions. Once the pointer has moved
    /// past the touch slop that way, the pan takes the pointer from scrolling
    /// parents.
    /// </summary>
    public class PanGestureRecognizer : GestureRecognizer
    {
        private readonly IPanGestureDelegate _delegate;
        private Point _start;

        public PanGestureRecognizer(
            IPanGestureDelegate gestureDelegate)
            : base(gestureDelegate)
        {
            this._delegate = gestureDelegate ?? throw new ArgumentNullException(nameof(gestureDelegate));
        }

        /// <summary>
        /// The directions the pan follows (both by default). A vertical-only pan
        /// leaves horizontal drags to others, and the other way around.
        /// </summary>
        public PanDirections Directions
        {
            get;
            set;
        } = PanDirections.Both;

        private Point Translation(
            Point position)
        {
            float dx = this.Directions.HasFlag(PanDirections.Horizontal) ? position.X - this._start.X : 0;
            float dy = this.Directions.HasFlag(PanDirections.Vertical) ? position.Y - this._start.Y : 0;
            return new Point(dx, dy);
        }

        protected override void OnPointerInput(
            PointerEventArgs e,
            ref GestureEventResult result)
        {
            switch (e.State)
            {
                case PointerInputState.Began:
                    this._start = e.Position;
                    result.DidHandleEvent = true;
                    break;

                case PointerInputState.Moved:
                    if (this.State == GestureState.Possible)
                    {
                        float dx = Math.Abs(e.Position.X - this._start.X);
                        float dy = Math.Abs(e.Position.Y - this._start.Y);
                        bool horizontal = this.Directions.HasFlag(PanDirections.Horizontal) && (dx > Application.TouchSlop) && (dx >= dy);
                        bool vertical = this.Directions.HasFlag(PanDirections.Vertical) && (dy > Application.TouchSlop) && (dy >= dx);

                        if (horizontal || vertical)
                        {
                            this.State = GestureState.Began;
                            this._start = e.Position;
                            this._delegate.OnPanStarted();
                        }
                    }
                    else if ((this.State == GestureState.Began) || (this.State == GestureState.Changed))
                    {
                        this.State = GestureState.Changed;
                        this._delegate.OnPanChanged(this.Translation(e.Position));
                    }

                    // Claim the moves once panning
                    result.DidHandleEvent = (this.State == GestureState.Began) || (this.State == GestureState.Changed);
                    break;

                case PointerInputState.Ended:
                    if ((this.State == GestureState.Began) || (this.State == GestureState.Changed))
                    {
                        this.State = GestureState.Recognized;
                        this._delegate.OnPanCompleted(this.Translation(e.Position));
                    }
                    else
                    {
                        this.State = GestureState.Failed;
                    }

                    break;

                case PointerInputState.Canceled:
                    if ((this.State == GestureState.Began) || (this.State == GestureState.Changed))
                    {
                        this._delegate.OnPanCanceled();
                    }

                    this.State = GestureState.Canceled;
                    break;
            }
        }
    }
}
