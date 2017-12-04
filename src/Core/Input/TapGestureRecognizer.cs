using System;

namespace XForms.Input
{
    public interface ITapGestureDelegate : IGestureRecognizerDelegate
    {
        void OnTapBegan();

        void OnTapEnded();

        void OnTapped();
    }

    public class TapGestureRecognizer : GestureRecognizer
    {
        private ITapGestureDelegate _delegate;
        private Point _gestureStart;

        public TapGestureRecognizer(
            ITapGestureDelegate gestureDelegate)
            : base(gestureDelegate)
        {
            if (null == gestureDelegate)
            {
                throw new ArgumentNullException(nameof(gestureDelegate));
            }

            this._delegate = gestureDelegate;
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

                    this._gestureStart = touch.Point;

                    this._delegate.OnTapBegan();
                }
            }
            else if (touch.State == PointerInputState.Moved)
            {
                if (this.State == GestureState.Began)
                {
                    double distance = touch.Point.DistanceTo(this._gestureStart);

                    if (distance > Application.TouchSlop)
                    {
                        this.State = GestureState.Failed;

                        this._delegate.OnTapEnded();
                    }
                }
            }
            else if (touch.State == PointerInputState.Ended)
            {
                if (this.State == GestureState.Began)
                {
                    double distance = touch.Point.DistanceTo(this._gestureStart);

                    if (distance <= Application.TouchSlop)
                    {
                        this.State = GestureState.Recognized;

                        this._delegate.OnTapped();
                    }
                    else
                    {
                        this.State = GestureState.Failed;
                    }

                    this._delegate.OnTapEnded();
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
                    this.State = GestureState.Failed;

                    this._delegate.OnTapEnded();
                }
                else
                {
                    this.State = GestureState.Failed;
                }
            }
        }
    }
}
