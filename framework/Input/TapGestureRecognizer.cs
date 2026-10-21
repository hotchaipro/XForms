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

                    this._gestureStart = touch.Position;

                    this._delegate.OnTapBegan();
                }
            }
            else if (touch.State == PointerInputState.Moved)
            {
                if (this.State == GestureState.Began)
                {
                    double distance = touch.Position.DistanceTo(this._gestureStart);

                    if (distance > Application.TouchSlop)
                    {
                        this.State = GestureState.Failed;

                        this._delegate.OnTapEnded();
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
                    double distance = touch.Position.DistanceTo(this._gestureStart);

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
