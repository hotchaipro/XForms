using System;
using System.Diagnostics;
using XForms.Win32.Renderers;

namespace XForms.Win32
{
    /// <summary>
    /// A renderer whose content scrolls.
    /// </summary>
    internal interface IScrollableRenderer
    {
        ScrollController Scroller { get; }

        void ScrollBy(float delta);
    }

    /// <summary>
    /// Routes pointer input (mouse, touch and pen) to XForms views the way the
    /// mobile platforms do: the deepest view that accepts the touch receives the
    /// rest of it, and a touch drag that no view claims scrolls the nearest
    /// scrolling ancestor, then glides when released.
    /// </summary>
    internal sealed class PointerDispatcher
    {
        private const float DragSlop = 8.0f;
        private const double VelocityWindowSeconds = 0.1;
        private const int MaximumSamples = 16;

        private readonly Win32Host _host;
        private readonly double[] _sampleTimes = new double[MaximumSamples];
        private readonly float[] _sampleOffsets = new float[MaximumSamples];
        private int _sampleCount;

        private bool _isActive;
        private uint _pointerId;
        private bool _isDirect;
        private ViewRenderer _capture;
        private Point _captureOrigin;
        private IScrollableRenderer _scrollable;
        private bool _isDragScrolling;
        private Point _dragStart;
        private float _dragStartOffset;

        public PointerDispatcher(
            Win32Host host)
        {
            this._host = host;
        }

        public bool IsActive
        {
            get
            {
                return this._isActive;
            }
        }

        public bool IsCapturedByView
        {
            get
            {
                return null != this._capture;
            }
        }

        private static double Now
        {
            get
            {
                return Stopwatch.GetTimestamp() / (double)Stopwatch.Frequency;
            }
        }

        /// <summary>
        /// A pointer touched down (or a mouse button was pressed). Direct pointers
        /// (touch and pen) can drag content to scroll it.
        /// </summary>
        public void Down(
            uint pointerId,
            Point point,
            bool isDirect)
        {
            if (this._isActive)
            {
                // NOTE: Only the first pointer is tracked; extra fingers are ignored
                return;
            }

            this._isActive = true;
            this._pointerId = pointerId;
            this._isDirect = isDirect;
            this._capture = null;
            this._isDragScrolling = false;
            this._sampleCount = 0;

            this._host.Focus.OnPointerInput();

            if (!this._host.HitTest(point, out var hit))
            {
                // Clicking outside a popup closes it, and does nothing else
                this._host.Overlays.Top.Dismiss();
                this._isActive = false;
                return;
            }

            // A click on a text entry (such as on its placeholder) focuses it
            if (hit is TextEntryRenderer textEntry)
            {
                this._host.Focus.SetFocus(textEntry, showRing: false);
                this._isActive = false;
                return;
            }

            this._scrollable = null;
            for (var renderer = hit; null != renderer; renderer = renderer.ParentRenderer)
            {
                if (renderer is IScrollableRenderer scrollable)
                {
                    this._scrollable = scrollable;
                    break;
                }
            }

            // Touching moving content stops it, and doesn't count as a tap
            if (isDirect && (null != this._scrollable) && this._scrollable.Scroller.IsAnimating)
            {
                var scroller = this._scrollable.Scroller;
                scroller.SetOffset(scroller.Offset);
                this.BeginDragScrolling(point);
                return;
            }

            // Offer the touch to the deepest view first, then its ancestors
            for (var renderer = hit; null != renderer; renderer = renderer.ParentRenderer)
            {
                if ((renderer.Element is View) && renderer.IsInputVisible)
                {
                    var origin = renderer.AbsoluteOrigin();
                    var local = new Point(point.X - origin.X, point.Y - origin.Y);

                    if (Deliver(renderer, local, PointerInputState.Began))
                    {
                        this._capture = renderer;
                        this._captureOrigin = origin;
                        break;
                    }
                }
            }

            this._dragStart = point;
        }

        public void Move(
            uint pointerId,
            Point point)
        {
            if ((!this._isActive) || (pointerId != this._pointerId))
            {
                return;
            }

            if (this._isDragScrolling)
            {
                float offset = this._dragStartOffset - (point.Y - this._dragStart.Y);
                this._scrollable.Scroller.SetOffset(offset);
                this.AddSample(offset);
                return;
            }

            bool isClaimed = false;

            var capture = this._capture;
            if (null != capture)
            {
                var local = new Point(point.X - this._captureOrigin.X, point.Y - this._captureOrigin.Y);
                isClaimed = Deliver(capture, local, PointerInputState.Moved);
            }

            // An unclaimed, mostly vertical drag past the slop scrolls the content
            if ((!isClaimed) && this._isDirect && (null != this._scrollable))
            {
                float dx = point.X - this._dragStart.X;
                float dy = point.Y - this._dragStart.Y;

                if ((Math.Abs(dy) > DragSlop) && (Math.Abs(dy) > Math.Abs(dx)))
                {
                    this.CancelCapture();
                    this.BeginDragScrolling(point);
                }
            }
        }

        public void Up(
            uint pointerId,
            Point point)
        {
            if ((!this._isActive) || (pointerId != this._pointerId))
            {
                return;
            }

            if (this._isDragScrolling)
            {
                this._scrollable.Scroller.Fling(this.Velocity());
            }
            else
            {
                var capture = this._capture;
                if (null != capture)
                {
                    var local = new Point(point.X - this._captureOrigin.X, point.Y - this._captureOrigin.Y);
                    Deliver(capture, local, PointerInputState.Ended);
                }
            }

            this.Reset();
        }

        public void Cancel()
        {
            if (!this._isActive)
            {
                return;
            }

            this.CancelCapture();

            if (this._isDragScrolling)
            {
                this._scrollable.Scroller.Fling(0);
            }

            this.Reset();
        }

        private void CancelCapture()
        {
            var capture = this._capture;
            if (null != capture)
            {
                this._capture = null;
                Deliver(capture, Point.Zero, PointerInputState.Canceled);
            }
        }

        private static bool Deliver(
            ViewRenderer renderer,
            Point local,
            PointerInputState state)
        {


            return ((View)renderer.Element).HandlePointerEvent(new PointerInputEvent(local, state, true));
        }

        private void BeginDragScrolling(
            Point point)
        {
            this._isDragScrolling = true;
            this._dragStart = point;
            this._dragStartOffset = this._scrollable.Scroller.Offset;
            this.AddSample(this._dragStartOffset);
        }

        private void Reset()
        {
            this._isActive = false;
            this._capture = null;
            this._scrollable = null;
            this._isDragScrolling = false;
        }

        private void AddSample(
            float offset)
        {
            if (this._sampleCount == MaximumSamples)
            {
                Array.Copy(this._sampleTimes, 1, this._sampleTimes, 0, MaximumSamples - 1);
                Array.Copy(this._sampleOffsets, 1, this._sampleOffsets, 0, MaximumSamples - 1);
                this._sampleCount -= 1;
            }

            this._sampleTimes[this._sampleCount] = Now;
            this._sampleOffsets[this._sampleCount] = offset;
            this._sampleCount += 1;
        }

        /// <summary>
        /// The scroll velocity over the last moments of the drag, in dips per second.
        /// </summary>
        private float Velocity()
        {
            if (this._sampleCount < 2)
            {
                return 0;
            }

            int last = this._sampleCount - 1;
            double now = Now;

            // A finger that stopped before lifting doesn't fling
            if (now - this._sampleTimes[last] > VelocityWindowSeconds)
            {
                return 0;
            }

            int first = last;
            while ((first > 0) && (this._sampleTimes[last] - this._sampleTimes[first - 1] <= VelocityWindowSeconds))
            {
                first -= 1;
            }

            double elapsed = this._sampleTimes[last] - this._sampleTimes[first];
            if (elapsed <= 0)
            {
                return 0;
            }

            return (float)((this._sampleOffsets[last] - this._sampleOffsets[first]) / elapsed);
        }
    }
}
