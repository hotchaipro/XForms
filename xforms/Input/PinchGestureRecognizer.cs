using System;
using System.Collections.Generic;

namespace XForms.Input
{
    public interface IPinchGestureDelegate : IGestureRecognizerDelegate
    {
        void OnPinchStarted();

        /// <summary>
        /// The fingers moved: the scale is their distance over the distance
        /// when the pinch started, around the point midway between them.
        /// </summary>
        void OnPinchChanged(float scale, Point center);

        void OnPinchCompleted(float scale);
    }

    /// <summary>
    /// Recognizes two fingers moving apart or together on the view. A single
    /// finger is left to scroll as usual.
    /// </summary>
    public class PinchGestureRecognizer : GestureRecognizer
    {
        private readonly IPinchGestureDelegate _delegate;
        private readonly Dictionary<int, Point> _pointers = new Dictionary<int, Point>();
        private bool _isPinching;
        private int _first, _second;
        private float _startDistance;
        private float _scale = 1.0f;

        public PinchGestureRecognizer(
            IPinchGestureDelegate gestureDelegate)
            : base(gestureDelegate)
        {
            this._delegate = gestureDelegate ?? throw new ArgumentNullException(nameof(gestureDelegate));
        }

        protected override bool TracksMultiplePointers
        {
            get
            {
                return true;
            }
        }

        protected override void OnDetached(
            View view)
        {
            this._pointers.Clear();
            this._isPinching = false;
        }

        protected override void OnPointerInput(
            PointerEventArgs e)
        {
            switch (e.State)
            {
                case PointerInputState.Began:
                    this._pointers[e.PointerId] = e.Position;
                    if ((this._pointers.Count == 2) && !this._isPinching)
                    {
                        using (var ids = this._pointers.Keys.GetEnumerator())
                        {
                            ids.MoveNext();
                            this._first = ids.Current;
                            ids.MoveNext();
                            this._second = ids.Current;
                        }

                        this._startDistance = Math.Max(1, this._pointers[this._first].DistanceTo(this._pointers[this._second]));
                        this._scale = 1.0f;
                        this._isPinching = true;
                        this.Recognize();
                        this._delegate.OnPinchStarted();
                    }

                    break;

                case PointerInputState.Moved:
                    if (this._pointers.ContainsKey(e.PointerId))
                    {
                        this._pointers[e.PointerId] = e.Position;
                    }

                    if (this._isPinching && ((e.PointerId == this._first) || (e.PointerId == this._second)))
                    {
                        var a = this._pointers[this._first];
                        var b = this._pointers[this._second];
                        this._scale = a.DistanceTo(b) / this._startDistance;
                        this._delegate.OnPinchChanged(this._scale, new Point((a.X + b.X) / 2, (a.Y + b.Y) / 2));
                    }

                    break;

                case PointerInputState.Ended:
                case PointerInputState.Canceled:
                    this._pointers.Remove(e.PointerId);
                    if (this._isPinching && ((e.PointerId == this._first) || (e.PointerId == this._second)))
                    {
                        this._isPinching = false;
                        this._delegate.OnPinchCompleted(this._scale);
                    }

                    break;
            }
        }
    }
}
