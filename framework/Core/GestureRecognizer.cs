using System;

namespace XForms
{
    public enum GestureState
    {
        Possible = 0,
        Failed,
        Began,
        Changed,
        Recognized,
        Canceled,
    }

    public interface IGestureRecognizerDelegate
    {
        void OnTouchBegan();

        void OnTouchEnded();

        void OnTouchCanceled();
    }

    public struct GestureEventResult
    {
        public bool DidHandleEvent;
    }

    public abstract class GestureRecognizer
    {
        private IGestureRecognizerDelegate _delegate;

        public GestureRecognizer(
            IGestureRecognizerDelegate gestureRecognizerDelegate)
        {
            if (null == gestureRecognizerDelegate)
            {
                throw new ArgumentNullException(nameof(gestureRecognizerDelegate));
            }

            this._delegate = gestureRecognizerDelegate;
            this.State = GestureState.Possible;
        }

        public GestureState State
        {
            get;
            protected set;
        }

        //public IGestureRecognizerDelegate Delegate
        //{
        //    get
        //    {
        //        return this._delegate;
        //    }
        //}

        public virtual void Reset()
        {
            this.State = GestureState.Possible;
        }

        /// <summary>
        /// The view the recognizer is attached to, or null.
        /// </summary>
        protected View View
        {
            get;
            private set;
        }

        internal void Attach(
            View view)
        {
            this.View = view;
            this.OnAttached(view);
        }

        internal void Detach(
            View view)
        {
            this.OnDetached(view);
            this.View = null;
        }

        /// <summary>
        /// Called when the recognizer becomes a view's
        /// <see cref="View.GestureRecognizer"/>.
        /// </summary>
        protected virtual void OnAttached(
            View view)
        {
        }

        /// <summary>
        /// Called when the recognizer stops being a view's
        /// <see cref="View.GestureRecognizer"/>.
        /// </summary>
        protected virtual void OnDetached(
            View view)
        {
        }

        private int? _pointerId;

        /// <summary>
        /// Whether the recognizer gets every pointer pressed on its view (as a
        /// pinch does). Otherwise it follows only the first one, until released.
        /// </summary>
        protected virtual bool TracksMultiplePointers
        {
            get
            {
                return false;
            }
        }

        public void HandlePointerInput(
            PointerEventArgs inputEvent,
            ref GestureEventResult result)
        {
            if (!this.TracksMultiplePointers)
            {
                if (inputEvent.State == PointerInputState.Began)
                {
                    if (null != this._pointerId)
                    {
                        // Another finger while one is already down: ignore it
                        return;
                    }

                    this._pointerId = inputEvent.PointerId;
                }
                else if (inputEvent.PointerId != this._pointerId)
                {
                    return;
                }

                if ((inputEvent.State == PointerInputState.Ended) || (inputEvent.State == PointerInputState.Canceled))
                {
                    this._pointerId = null;
                }
            }

            if (inputEvent.State == PointerInputState.Began)
            {
                this._delegate.OnTouchBegan();
            }
            else if (inputEvent.State == PointerInputState.Ended)
            {
                this._delegate.OnTouchEnded();
            }
            else if (inputEvent.State == PointerInputState.Canceled)
            {
                this._delegate.OnTouchCanceled();
            }

            this.OnPointerInput(inputEvent, ref result);

            if ((this.State == GestureState.Recognized)
                || (this.State == GestureState.Failed)
                || (this.State == GestureState.Canceled))
            {
                // Reset on reaching a terminal node in the state machine
                this.Reset();
            }
        }

        protected abstract void OnPointerInput(
            PointerEventArgs inputEvent,
            ref GestureEventResult result);
    }
}
