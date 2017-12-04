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

    public class SwipeGestureRecognizer : GestureRecognizer
    {
        private ISwipeGestureDelegate _delegate;
        private int _swipeDirection;
        private Point _gestureStart;

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

        public override void Reset()
        {
            base.Reset();

            this._gestureStart = Point.Zero;
        }

        protected override void OnPointerInput(
            PointerInputEvent touch,
            ref GestureEventResult result)
        {
            result.DidHandleEvent = true;

            if (touch.State == PointerInputState.Began)
            {
                if (this.State == GestureState.Possible)
                {
                    this.State = GestureState.Began;

                    this._swipeDirection = 0;
                    this._gestureStart = touch.Point;

                    this._delegate.OnTapBegan();
                }
            }
            else if (this.State != GestureState.Possible)
            {
                float dx = (float)(touch.Point.X - this._gestureStart.X);

                if (this._swipeDirection == 0)
                {
                    this._swipeDirection = Math.Sign(dx);
                }

                if (Math.Sign(dx) != this._swipeDirection)
                {
                    dx = 0;
                }
                else
                {
                    dx = Math.Abs(dx);
                }

                double dy = Math.Abs(touch.Point.Y - this._gestureStart.Y);

                if (touch.State == PointerInputState.Moved)
                {
                    if (this.State == GestureState.Began)
                    {
                        if (dx >= Application.TouchSlop)
                        {
                            this._delegate.OnSwipeBegan(this._swipeDirection);

                            this.State = GestureState.Changed;
                        }
                        else if (dy >= Application.TouchSlop)
                        {
                            this.State = GestureState.Failed;

                            this._delegate.OnTapEnded();
                        }
                    }
                    else if (this.State == GestureState.Changed)
                    {
                        this._delegate.OnSwipeMoved(this._swipeDirection, dx);
                    }
                }
                else if (touch.State == PointerInputState.Ended)
                {
                    if (this.State == GestureState.Began)
                    {
                        this.State = GestureState.Recognized;

                        this._delegate.OnTapped();

                        this._delegate.OnTapEnded();
                    }
                    else if (this.State == GestureState.Changed)
                    {
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
                        this._delegate.OnSwipeCanceled(this._swipeDirection);

                        this._delegate.OnTapEnded();
                    }

                    if (this.State != GestureState.Possible)
                    {
                        this.State = GestureState.Canceled;
                    }
                }
            }
        }
    }
}
