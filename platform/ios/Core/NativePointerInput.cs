using System;
using Foundation;
using UIKit;

namespace XForms.iOS
{
    /// <summary>
    /// Translates UIKit touches into XForms pointer events for a single native view.
    /// </summary>
    /// <remarks>
    /// Mirrors the Android model: a view that handles the first touch receives the
    /// rest of that touch sequence, and a view that declines it lets the touch travel
    /// up the responder chain to its superview.
    /// </remarks>
    internal struct NativePointerInput
    {
        private UITouch _trackedTouch;

        public bool TouchesBegan(
            UIView view,
            IViewDelegate viewDelegate,
            NSSet touches)
        {
            if ((null != this._trackedTouch) || (null == viewDelegate) || (!view.UserInteractionEnabled))
            {
                return (null != this._trackedTouch);
            }

            var touch = touches.AnyObject as UITouch;
            if (null == touch)
            {
                return false;
            }

            bool handled = Dispatch(view, viewDelegate, touch, PointerInputState.Began);
            if (handled)
            {
                this._trackedTouch = touch;
            }

            return handled;
        }

        public bool TouchesMoved(
            UIView view,
            IViewDelegate viewDelegate,
            NSSet touches)
        {
            var touch = this.FindTrackedTouch(touches);
            if (null == touch)
            {
                return false;
            }

            Dispatch(view, viewDelegate, touch, PointerInputState.Moved);

            return true;
        }

        public bool TouchesEnded(
            UIView view,
            IViewDelegate viewDelegate,
            NSSet touches)
        {
            return this.EndTracking(view, viewDelegate, touches, PointerInputState.Ended);
        }

        public bool TouchesCancelled(
            UIView view,
            IViewDelegate viewDelegate,
            NSSet touches)
        {
            return this.EndTracking(view, viewDelegate, touches, PointerInputState.Canceled);
        }

        private bool EndTracking(
            UIView view,
            IViewDelegate viewDelegate,
            NSSet touches,
            PointerInputState state)
        {
            var touch = this.FindTrackedTouch(touches);
            if (null == touch)
            {
                return false;
            }

            this._trackedTouch = null;

            Dispatch(view, viewDelegate, touch, state);

            return true;
        }

        private UITouch FindTrackedTouch(
            NSSet touches)
        {
            var trackedTouch = this._trackedTouch;
            if ((null != trackedTouch) && (touches.Contains(trackedTouch)))
            {
                return trackedTouch;
            }

            return null;
        }

        private static bool Dispatch(
            UIView view,
            IViewDelegate viewDelegate,
            UITouch touch,
            PointerInputState state)
        {
            if (null == viewDelegate)
            {
                return false;
            }

            var location = touch.LocationInView(view);
            var point = new Point((float)location.X, (float)location.Y);

            return viewDelegate.HandlePointerEvent(new PointerInputEvent(point, state, true));
        }
    }
}
