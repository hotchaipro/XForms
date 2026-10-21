using System;
using System.Collections.Generic;
using Foundation;
using UIKit;
using XForms.Platform;

namespace XForms.iOS
{
    /// <summary>
    /// Translates UIKit touches into XForms pointer events for a single native view.
    /// </summary>
    /// <remarks>
    /// Mirrors the Android model: a view that handles a touch receives the rest
    /// of that touch, and a view that declines it lets the touch travel up the
    /// responder chain to its superview. Each finger is its own pointer.
    /// </remarks>
    internal sealed class NativePointerInput
    {
        private static int NextPointerId = 1;

        private readonly Dictionary<UITouch, int> _trackedTouches = new Dictionary<UITouch, int>();

        public bool TouchesBegan(
            UIView view,
            View viewDelegate,
            NSSet touches)
        {
            if ((null == viewDelegate) || (!view.UserInteractionEnabled))
            {
                return false;
            }

            bool handled = false;
            foreach (var item in touches)
            {
                if (!(item is UITouch touch) || this._trackedTouches.ContainsKey(touch))
                {
                    continue;
                }

                int pointerId = NextPointerId++;
                if (Dispatch(view, viewDelegate, touch, pointerId, PointerInputState.Began))
                {
                    this._trackedTouches[touch] = pointerId;
                    handled = true;
                }
            }

            return handled;
        }

        public bool TouchesMoved(
            UIView view,
            View viewDelegate,
            NSSet touches)
        {
            bool tracked = false;
            foreach (var item in touches)
            {
                if ((item is UITouch touch) && this._trackedTouches.TryGetValue(touch, out int pointerId))
                {
                    Dispatch(view, viewDelegate, touch, pointerId, PointerInputState.Moved);
                    tracked = true;
                }
            }

            return tracked;
        }

        public bool TouchesEnded(
            UIView view,
            View viewDelegate,
            NSSet touches)
        {
            return this.EndTracking(view, viewDelegate, touches, PointerInputState.Ended);
        }

        public bool TouchesCancelled(
            UIView view,
            View viewDelegate,
            NSSet touches)
        {
            return this.EndTracking(view, viewDelegate, touches, PointerInputState.Canceled);
        }

        private bool EndTracking(
            UIView view,
            View viewDelegate,
            NSSet touches,
            PointerInputState state)
        {
            bool tracked = false;
            foreach (var item in touches)
            {
                if ((item is UITouch touch) && this._trackedTouches.Remove(touch, out int pointerId))
                {
                    Dispatch(view, viewDelegate, touch, pointerId, state);
                    tracked = true;
                }
            }

            return tracked;
        }

        private static bool Dispatch(
            UIView view,
            View viewDelegate,
            UITouch touch,
            int pointerId,
            PointerInputState state)
        {
            if (null == viewDelegate)
            {
                return false;
            }

            var location = touch.LocationInView(view);
            var point = new Point((float)location.X, (float)location.Y);

            var deviceType = (touch.Type == UITouchType.Stylus) ? PointerDeviceType.Pen
                : (touch.Type == UITouchType.IndirectPointer) ? PointerDeviceType.Mouse
                : PointerDeviceType.Touch;

            return LayerHost.For(viewDelegate).DispatchPointer(new PointerEventArgs(pointerId, deviceType, point, state, true, KeyModifiers.None));
        }
    }
}
