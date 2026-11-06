using System;
using System.Collections.Generic;

namespace XForma
{
    /// <summary>
    /// Where a gesture recognizer is in following the pointer on its view.
    /// </summary>
    public enum GestureState
    {
        /// <summary>
        /// No pointer is down: the recognizer waits for a press.
        /// </summary>
        Ready = 0,

        /// <summary>
        /// A pointer is down, and may yet turn out to be the gesture.
        /// </summary>
        Possible,

        /// <summary>
        /// The pointer is the gesture. The recognizer claims its moves, and the
        /// view's other recognizers have stopped following it.
        /// </summary>
        Recognized,

        /// <summary>
        /// The pointer isn't the gesture: the recognizer ignores it until it's
        /// released.
        /// </summary>
        Failed,
    }

    /// <summary>
    /// What a control implements to hear when a touch begins and ends on its gesture recognizer.
    /// </summary>
    public interface IGestureRecognizerDelegate
    {
        void OnTouchBegan();

        void OnTouchEnded();

        void OnTouchCanceled();
    }

    /// <summary>
    /// Interprets the pointer input of the view it's added to. A press makes it
    /// <see cref="GestureState.Possible"/>; it then calls
    /// <see cref="Recognize"/> or <see cref="Fail"/>, and is
    /// <see cref="GestureState.Ready"/> again once the pointer is released or
    /// canceled.
    /// </summary>
    /// <remarks>
    /// Recognizers on one view are exclusive: the first to recognize cancels the
    /// others. A recognizer claims a press unless it fails on it, and claims
    /// moves once it has recognized; until then a scrolling parent can take the
    /// pointer, which ends as canceled.
    /// </remarks>
    public abstract class GestureRecognizer
    {
        private readonly IGestureRecognizerDelegate? _delegate;
        private readonly HashSet<int> _pointers = new HashSet<int>();
        private PointerEventArgs? _lastEvent;

        /// <summary>
        /// A recognizer that reports through its own events.
        /// </summary>
        protected GestureRecognizer()
        {
        }

        /// <summary>
        /// A recognizer that reports to a delegate (the control it belongs to).
        /// </summary>
        protected GestureRecognizer(
            IGestureRecognizerDelegate gestureRecognizerDelegate)
        {
            this._delegate = gestureRecognizerDelegate ?? throw new ArgumentNullException(nameof(gestureRecognizerDelegate));
        }

        public GestureState State
        {
            get;
            private set;
        }

        /// <summary>
        /// The view the recognizer is attached to, or null.
        /// </summary>
        protected View? View
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
            this._pointers.Clear();
            this.State = GestureState.Ready;
        }

        /// <summary>
        /// Called when the recognizer becomes a view's
        /// <see cref="View.GestureRecognizers"/>.
        /// </summary>
        protected virtual void OnAttached(
            View view)
        {
        }

        /// <summary>
        /// Called when the recognizer stops being a view's
        /// <see cref="View.GestureRecognizers"/>.
        /// </summary>
        protected virtual void OnDetached(
            View view)
        {
        }

        /// <summary>
        /// Whether the gesture follows drags (a swipe, a pan). Only these are
        /// offered a drag that starts on a view inside theirs (as a sideways
        /// drag on a row's button swipes the row), and they see each event
        /// before the view's other recognizers, so a quick flick that lifts
        /// before a move arrives is theirs, not a tap.
        /// </summary>
        protected virtual bool IsDragGesture
        {
            get
            {
                return false;
            }
        }

        internal bool FollowsDrags
        {
            get
            {
                return this.IsDragGesture;
            }
        }

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

        /// <summary>
        /// The pointer is the gesture: the view's other recognizers stop
        /// following it, and the recognizer claims its moves. Does nothing
        /// unless the state is <see cref="GestureState.Possible"/>.
        /// </summary>
        protected void Recognize()
        {
            if (this.State != GestureState.Possible)
            {
                return;
            }

            this.State = GestureState.Recognized;

            var recognizers = this.View?.GestureRecognizers;
            if ((null == recognizers) || (recognizers.Count < 2))
            {
                return;
            }

            var e = this._lastEvent;
            foreach (int pointerId in new List<int>(this._pointers))
            {
                for (int i = 0; i < recognizers.Count; i += 1)
                {
                    if (recognizers[i] != this)
                    {
                        recognizers[i].HandlePointerInput(new PointerEventArgs(pointerId, e.DeviceType, e.Position, PointerInputState.Canceled, false, e.KeyModifiers) { OriginalSource = e.OriginalSource });
                    }
                }
            }
        }

        /// <summary>
        /// The pointer isn't the gesture: the recognizer gets no more of its
        /// input, leaving it to others.
        /// </summary>
        protected void Fail()
        {
            if (this.State != GestureState.Ready)
            {
                this.State = GestureState.Failed;
            }
        }

        // Returns whether the recognizer claims the event
        internal bool HandlePointerInput(
            PointerEventArgs e)
        {
            if (e.State == PointerInputState.Began)
            {
                if (this._pointers.Contains(e.PointerId))
                {
                    // A press for a pointer that never ended (its view was
                    // taken away from under it, say) starts over
                    this._pointers.Clear();
                }

                if (0 == this._pointers.Count)
                {
                    this.State = GestureState.Possible;
                }
                else if (!this.TracksMultiplePointers)
                {
                    // Another finger while one is already down: ignore it
                    return false;
                }

                this._pointers.Add(e.PointerId);
            }
            else if (!this._pointers.Contains(e.PointerId))
            {
                return false;
            }

            this._lastEvent = e;

            if (e.State == PointerInputState.Began)
            {
                this._delegate?.OnTouchBegan();
            }
            else if (e.State == PointerInputState.Ended)
            {
                this._delegate?.OnTouchEnded();
            }
            else if (e.State == PointerInputState.Canceled)
            {
                this._delegate?.OnTouchCanceled();
            }

            if ((this.State == GestureState.Possible) || (this.State == GestureState.Recognized))
            {
                this.OnPointerInput(e);
            }

            bool isClaimed = (e.State == PointerInputState.Began)
                ? (this.State != GestureState.Failed)
                : ((e.State != PointerInputState.Canceled) && (this.State == GestureState.Recognized));

            if ((e.State == PointerInputState.Ended) || (e.State == PointerInputState.Canceled))
            {
                this._pointers.Remove(e.PointerId);
                if (0 == this._pointers.Count)
                {
                    this.State = GestureState.Ready;
                    this._lastEvent = null;
                }
            }

            return isClaimed;
        }

        /// <summary>
        /// A pointer event for the gesture, while it's
        /// <see cref="GestureState.Possible"/> or
        /// <see cref="GestureState.Recognized"/>.
        /// </summary>
        protected abstract void OnPointerInput(
            PointerEventArgs e);
    }

    /// <summary>
    /// A view's gesture recognizers: adding one attaches it to the view,
    /// removing it detaches it.
    /// </summary>
    internal sealed class GestureRecognizerCollection : System.Collections.ObjectModel.Collection<GestureRecognizer>
    {
        private readonly View _view;

        public GestureRecognizerCollection(
            View view)
        {
            this._view = view;
        }

        protected override void InsertItem(
            int index,
            GestureRecognizer item)
        {
            ArgumentNullException.ThrowIfNull(item);
            base.InsertItem(index, item);
            item.Attach(this._view);
        }

        protected override void SetItem(
            int index,
            GestureRecognizer item)
        {
            ArgumentNullException.ThrowIfNull(item);
            this[index].Detach(this._view);
            base.SetItem(index, item);
            item.Attach(this._view);
        }

        protected override void RemoveItem(
            int index)
        {
            this[index].Detach(this._view);
            base.RemoveItem(index);
        }

        protected override void ClearItems()
        {
            foreach (var item in this)
            {
                item.Detach(this._view);
            }

            base.ClearItems();
        }
    }
}
