using System;
using System.Collections.Generic;
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
        private PointerDeviceType _deviceType;

        // Pointers pressed while the first is down: each goes to the view that
        // took it, and none of them scrolls
        private readonly Dictionary<uint, (ViewRenderer Renderer, Point Origin, PointerDeviceType DeviceType)> _others = new();
        private ViewRenderer _capture;
        private Point _captureOrigin;
        private IScrollableRenderer _scrollable;
        private bool _isDragScrolling;
        private bool _isPulling;
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
            bool isDirect,
            PointerDeviceType deviceType)
        {
            if (this._isActive)
            {
                this.OtherDown(pointerId, point, deviceType);
                return;
            }

            this._isActive = true;
            this._pointerId = pointerId;
            this._isDirect = isDirect;
            this._deviceType = deviceType;
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
            if (hit is TextBoxRenderer textEntry)
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
                if ((renderer.Element is View) && renderer.IsHitTestVisible)
                {
                    var origin = renderer.AbsoluteOrigin();
                    var local = new Point(point.X - origin.X, point.Y - origin.Y);

                    if (Deliver(renderer, local, PointerInputState.Began, pointerId, deviceType))
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
            if (this._others.TryGetValue(pointerId, out var other))
            {
                Deliver(other.Renderer, Local(point, other.Origin), PointerInputState.Moved, pointerId, other.DeviceType);
                return;
            }

            if ((!this._isActive) || (pointerId != this._pointerId))
            {
                return;
            }

            if (this._isDragScrolling)
            {
                bool isHorizontal = this._scrollable.Scroller.IsHorizontal;
                float offset = this._dragStartOffset - (isHorizontal ? (point.X - this._dragStart.X) : (point.Y - this._dragStart.Y));
                this._scrollable.Scroller.SetOffset(offset);
                this.AddSample(offset);

                // Pulling a list down past its top refreshes it
                if ((this._scrollable is IPullToRefresh pullable) && (this._isPulling || pullable.IsPullEnabled))
                {
                    this._isPulling = offset < 0;
                    pullable.OnPull(Math.Max(0, -offset));
                }

                return;
            }

            bool isClaimed = false;

            var capture = this._capture;
            if (null != capture)
            {
                var local = new Point(point.X - this._captureOrigin.X, point.Y - this._captureOrigin.Y);
                isClaimed = Deliver(capture, local, PointerInputState.Moved, this._pointerId, this._deviceType);
            }

            // An unclaimed drag past the slop, mostly along the content's axis,
            // scrolls the content
            if ((!isClaimed) && this._isDirect && (null != this._scrollable))
            {
                float dx = point.X - this._dragStart.X;
                float dy = point.Y - this._dragStart.Y;
                if (this._scrollable.Scroller.IsHorizontal)
                {
                    (dx, dy) = (dy, dx);
                }

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
            if (this._others.Remove(pointerId, out var other))
            {
                Deliver(other.Renderer, Local(point, other.Origin), PointerInputState.Ended, pointerId, other.DeviceType);
                return;
            }

            if ((!this._isActive) || (pointerId != this._pointerId))
            {
                return;
            }

            if (this._isDragScrolling)
            {
                if (this._isPulling && (this._scrollable is IPullToRefresh pullable))
                {
                    pullable.OnPullReleased();
                    this._scrollable.Scroller.Fling(0);
                }
                else
                {
                    this._scrollable.Scroller.Fling(this.Velocity());
                }
            }
            else
            {
                var capture = this._capture;
                if (null != capture)
                {
                    var local = new Point(point.X - this._captureOrigin.X, point.Y - this._captureOrigin.Y);
                    Deliver(capture, local, PointerInputState.Ended, this._pointerId, this._deviceType);
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

            foreach (var pair in this._others)
            {
                Deliver(pair.Value.Renderer, Point.Zero, PointerInputState.Canceled, pair.Key, pair.Value.DeviceType);
            }

            this._others.Clear();

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
                Deliver(capture, Point.Zero, PointerInputState.Canceled, this._pointerId, this._deviceType);
            }
        }

        // Another pointer pressed while the first is down (a second finger):
        // offered to the deepest view under it, then its ancestors
        private void OtherDown(
            uint pointerId,
            Point point,
            PointerDeviceType deviceType)
        {
            if (!this._host.HitTest(point, out var hit))
            {
                return;
            }

            for (var renderer = hit; null != renderer; renderer = renderer.ParentRenderer)
            {
                if ((renderer.Element is View) && renderer.IsHitTestVisible)
                {
                    var origin = renderer.AbsoluteOrigin();
                    if (Deliver(renderer, Local(point, origin), PointerInputState.Began, pointerId, deviceType))
                    {
                        this._others[pointerId] = (renderer, origin, deviceType);
                        return;
                    }
                }
            }
        }

        private static Point Local(
            Point point,
            Point origin)
        {
            return new Point(point.X - origin.X, point.Y - origin.Y);
        }

        private static bool Deliver(
            ViewRenderer renderer,
            Point local,
            PointerInputState state,
            uint pointerId,
            PointerDeviceType deviceType)
        {
            var e = new PointerEventArgs((int)pointerId, deviceType, local, state, true, FocusManager.CurrentModifiers());
            return ((View)renderer.Element).DispatchPointerEvent(e);
        }

        private void BeginDragScrolling(
            Point point)
        {
            this._isDragScrolling = true;
            this._isPulling = false;
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
