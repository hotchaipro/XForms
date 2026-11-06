using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using XForma.Platform;

namespace XForma
{
    // Pointer events and gesture recognizers
    public partial class View
    {
        private HashSet<int>? _capturedPointers;

        public event EventHandler<PointerEventArgs>? PointerPressed;

        public event EventHandler<PointerEventArgs>? PointerMoved;

        public event EventHandler<PointerEventArgs>? PointerReleased;

        public event EventHandler<PointerEventArgs>? PointerCanceled;

        public event EventHandler<PointerEventArgs>? PointerWheelChanged;

        /// <summary>
        /// Raised when a hovering pointer (a mouse or pen, not a touch) moves
        /// over the view or one of its descendants.
        /// </summary>
        public event EventHandler<PointerEventArgs>? PointerEntered;

        /// <summary>
        /// Raised when a hovering pointer leaves the view and its descendants,
        /// or the window.
        /// </summary>
        public event EventHandler<PointerEventArgs>? PointerExited;

        private bool _isPointerOver;

        /// <summary>
        /// Whether a hovering pointer is over the view or one of its
        /// descendants.
        /// </summary>
        public bool IsPointerOver
        {
            get
            {
                return this._isPointerOver;
            }
        }

        protected virtual void OnPointerEntered(
            PointerEventArgs e)
        {
        }

        protected virtual void OnPointerExited(
            PointerEventArgs e)
        {
        }

        internal void SetPointerOver(
            bool isPointerOver,
            PointerEventArgs e)
        {
            if (isPointerOver == this._isPointerOver)
            {
                return;
            }

            this._isPointerOver = isPointerOver;
            if (isPointerOver)
            {
                this.OnPointerEntered(e);
                this.PointerEntered?.Invoke(this, e);
            }
            else
            {
                this.OnPointerExited(e);
                this.PointerExited?.Invoke(this, e);
            }

            this.OnPropertyChanged(nameof(this.IsPointerOver));
        }

        /// <summary>
        /// The deepest view under the point (in this view's coordinates) that a
        /// hovering pointer is over: a lightweight view drawn into this one,
        /// or this view.
        /// </summary>
        internal View HitTestPointerOver(
            Point point)
        {
            return (this.HasLightweightChildren ? this.HitTestLightweight(point) : null) ?? this;
        }

        protected virtual void OnPointerPressed(
            PointerEventArgs e)
        {
        }

        protected virtual void OnPointerMoved(
            PointerEventArgs e)
        {
        }

        protected virtual void OnPointerReleased(
            PointerEventArgs e)
        {
        }

        protected virtual void OnPointerCanceled(
            PointerEventArgs e)
        {
        }

        protected virtual void OnPointerWheelChanged(
            PointerEventArgs e)
        {
        }

        /// <summary>
        /// Keeps the pointer's moves for this view, so a scrolling parent can't
        /// take them over (as it would for an unhandled drag). The capture ends
        /// when the pointer is released or canceled.
        /// </summary>
        public bool CapturePointer(
            PointerEventArgs e)
        {
            if (null == e)
            {
                throw new ArgumentNullException(nameof(e));
            }

            this._capturedPointers ??= new HashSet<int>();
            this._capturedPointers.Add(e.PointerId);
            return true;
        }

        public void ReleasePointerCapture(
            PointerEventArgs e)
        {
            if (null == e)
            {
                throw new ArgumentNullException(nameof(e));
            }

            this._capturedPointers?.Remove(e.PointerId);
        }

        /// <summary>
        /// Whether the view has captured the pointer.
        /// </summary>
        public bool HasPointerCapture(
            int pointerId)
        {
            return this._capturedPointers?.Contains(pointerId) ?? false;
        }

        /// <summary>
        /// Delivers a pointer event from the platform: the application's preview,
        /// then the overrides and events, then gestures. Returns whether the view
        /// takes the pointer (for a press) or claims its moves (for a move).
        /// </summary>
        internal virtual bool DispatchPointerEvent(
            PointerEventArgs e)
        {
            e.OriginalSource ??= this;
            int focusRequests = FocusRequests;

            // A drag an ancestor's gesture took over goes on to it
            Drag? drag = null;
            _drags?.TryGetValue(e.PointerId, out drag);
            if ((null != drag) && (drag.Origin == this) && (null != drag.Taker))
            {
                if ((e.State == PointerInputState.Ended) || (e.State == PointerInputState.Canceled))
                {
                    _drags.Remove(e.PointerId);
                }

                drag.Taker.DispatchToGestureRecognizers(drag.ToTaker(e));
                return true;
            }

            // A quick drag that lifts past the slop before a move handed it
            // over goes to an ancestor's drag gesture first (a flick on a row's
            // button swipes the row; the button's press is canceled)
            if ((e.State == PointerInputState.Ended) && (null != drag) && (drag.Origin == this) && (null == drag.Taker) && this.OfferDragToAncestors(drag, e))
            {
                _drags.Remove(e.PointerId);
                return true;
            }

            if (this.Application?.FilterPointerInput(this, e) ?? false)
            {
                return true;
            }

            switch (e.State)
            {
                case PointerInputState.Began:
                    this.OnPointerPressed(e);
                    this.PointerPressed?.Invoke(this, e);
                    break;

                case PointerInputState.Moved:
                    this.OnPointerMoved(e);
                    this.PointerMoved?.Invoke(this, e);
                    break;

                case PointerInputState.Ended:
                    this.OnPointerReleased(e);
                    this.PointerReleased?.Invoke(this, e);
                    break;

                case PointerInputState.Canceled:
                    this.OnPointerCanceled(e);
                    this.PointerCanceled?.Invoke(this, e);
                    break;
            }

            bool isHandled = this.DispatchToGestureRecognizers(e);

            bool isCaptured = this.HasPointerCapture(e.PointerId);
            if ((e.State == PointerInputState.Ended) || (e.State == PointerInputState.Canceled))
            {
                this._capturedPointers?.Remove(e.PointerId);
            }

            bool isClaimed = e.Handled || isHandled || isCaptured;
            switch (e.State)
            {
                case PointerInputState.Began:
                    SetFocusRingVisible(false);

                    // The view that takes a press gets its moves: remember
                    // where it started, for an ancestor's gesture to take over
                    if (isClaimed)
                    {
                        _drags ??= new Dictionary<int, Drag>();
                        _drags[e.PointerId] = new Drag() { Origin = this, Start = e.Position };
                    }

                    break;

                case PointerInputState.Moved:
                    if ((!isClaimed) && (null != drag) && (drag.Origin == this))
                    {
                        isClaimed = this.OfferDragToAncestors(drag, e);
                    }

                    break;

                default:
                    if ((null != drag) && (drag.Origin == this))
                    {
                        _drags.Remove(e.PointerId);

                        // A control tapped takes focus (without a ring), and
                        // typing in a text box ends: the soft keyboard hides.
                        // On release, after the tap has run, so the layout
                        // the keyboard leaves doesn't move the control from
                        // under the press
                        // (Not when the tap moved focus itself, such as to a
                        // text box on the page it opened)
                        if ((e.State == PointerInputState.Ended) && (focusRequests == FocusRequests) && this.Focusable && !(this is Primitives.TextBox) && !this.IsFocused)
                        {
                            this.Application?.Renderer.EndTextEntry();
                            this.Focus();
                        }
                    }

                    break;
            }

            return isClaimed;
        }

        // Returns whether a gesture recognizer claimed the event. Drag gestures
        // come first: one that recognizes as a quick flick lifts cancels a tap
        // that would still have accepted it
        private bool DispatchToGestureRecognizers(
            PointerEventArgs e)
        {
            return this.DispatchToDragRecognizers(e) | this.DispatchToOtherRecognizers(e);
        }

        private bool DispatchToDragRecognizers(
            PointerEventArgs e)
        {
            return this.DispatchToRecognizers(e, followingDrags: true);
        }

        private bool DispatchToOtherRecognizers(
            PointerEventArgs e)
        {
            return this.DispatchToRecognizers(e, followingDrags: false);
        }

        private bool DispatchToRecognizers(
            PointerEventArgs e,
            bool followingDrags)
        {
            bool isHandled = false;
            var recognizers = this._gestureRecognizers;
            if (null != recognizers)
            {
                for (int i = 0; i < recognizers.Count; i += 1)
                {
                    if (recognizers[i].FollowsDrags == followingDrags)
                    {
                        isHandled |= recognizers[i].HandlePointerInput(e);
                    }
                }
            }

            return isHandled;
        }

        /// <summary>
        /// Whether a touch pressed on the view may yet become a drag: the view
        /// or an ancestor has a drag gesture, or an ancestor scrolls. A control
        /// holds back its pressed look briefly then, so a press that turns into
        /// a swipe or a scroll doesn't flash it.
        /// </summary>
        public bool IsDragPossible
        {
            get
            {
                if (this.HasDragRecognizer)
                {
                    return true;
                }

                var view = this.ParentForDrag;
                while (null != view)
                {
                    if (view.ScrollsContent || view.HasDragRecognizer)
                    {
                        return true;
                    }

                    view = view.ParentForDrag;
                }

                return false;
            }
        }

        // The view a drag goes on to: a list's item containers may be held by
        // native cells, so they name their list
        private View? ParentForDrag
        {
            get
            {
                return (this.Parent as View) ?? (this as Primitives.ItemContainer)?.ItemsView;
            }
        }

        /// <summary>
        /// Whether the view scrolls what's in it, taking drags on it.
        /// </summary>
        internal virtual bool ScrollsContent
        {
            get
            {
                return false;
            }
        }

        private bool HasDragRecognizer
        {
            get
            {
                var recognizers = this._gestureRecognizers;
                if (null != recognizers)
                {
                    for (int i = 0; i < recognizers.Count; i += 1)
                    {
                        if (recognizers[i].FollowsDrags)
                        {
                            return true;
                        }
                    }
                }

                return false;
            }
        }

        // A pressed pointer, from the view that took the press, and the
        // ancestor whose gesture took the drag over, if one did
        private sealed class Drag
        {
            public View? Origin;
            public Point Start;
            public View? Taker;

            // The event in the taker's coordinates: the views between move
            // (the platforms give a view's positions as it is, translated by
            // its animators), so the pressed view's own position drifts with a
            // row's content as it follows a swipe, but the taker's doesn't
            public PointerEventArgs ToTaker(
                PointerEventArgs e)
            {
                float x = e.Position.X, y = e.Position.Y;
                for (var view = this.Origin; (null != view) && (view != this.Taker); view = view.Parent as View)
                {
                    var translation = view.AnimatorTranslation;
                    x += view._frame.X + translation.X;
                    y += view._frame.Y + translation.Y;
                }

                return new PointerEventArgs(
                    e.PointerId,
                    e.DeviceType,
                    new Point(x, y),
                    e.State,
                    e.IsInContact,
                    e.KeyModifiers)
                {
                    OriginalSource = e.OriginalSource,
                };
            }
        }

        private static Dictionary<int, Drag>? _drags;

        // A drag past the slop that the pressed view leaves unclaimed (a tap
        // on a button fails) is offered to its ancestors' drag gestures,
        // nearest first, as if pressed where it started: the one that claims
        // it (a row's swipe, under the button) takes it over, and the pressed
        // view's press is canceled. As a scroll viewer takes over a drag. A
        // drag that lifted past the slop before a move handed it over (too
        // quick for moves to arrive) is offered the same way, as it lifts.
        private bool OfferDragToAncestors(
            Drag drag,
            PointerEventArgs e)
        {
            float slop = Application.GetDragSlop(e.DeviceType);
            float dx = e.Position.X - drag.Start.X;
            float dy = e.Position.Y - drag.Start.Y;
            if (((dx * dx) + (dy * dy)) < (slop * slop))
            {
                return false;
            }

            for (var ancestor = this.Parent as View; null != ancestor; ancestor = ancestor.Parent as View)
            {
                if (!ancestor.HasDragRecognizer)
                {
                    continue;
                }

                var offered = new Drag() { Origin = this, Start = drag.Start, Taker = ancestor };
                var start = new PointerEventArgs(e.PointerId, e.DeviceType, drag.Start, PointerInputState.Began, true, e.KeyModifiers) { OriginalSource = e.OriginalSource };
                ancestor.DispatchToDragRecognizers(offered.ToTaker(start));
                if (ancestor.DispatchToDragRecognizers(offered.ToTaker(e)))
                {
                    this.DispatchPointerEvent(new PointerEventArgs(e.PointerId, e.DeviceType, e.Position, PointerInputState.Canceled, false, e.KeyModifiers));
                    if (e.State == PointerInputState.Moved)
                    {
                        _drags[e.PointerId] = offered;
                    }

                    return true;
                }

                ancestor.DispatchToDragRecognizers(offered.ToTaker(new PointerEventArgs(e.PointerId, e.DeviceType, e.Position, PointerInputState.Canceled, false, e.KeyModifiers)));
            }

            return false;
        }

        /// <summary>
        /// Delivers a wheel event from the platform. Returns whether the view
        /// handled it; otherwise the platform offers it to the view's ancestors,
        /// then scrolls.
        /// </summary>
        internal bool DispatchPointerWheel(
            PointerEventArgs e)
        {
            e.OriginalSource ??= this;

            if (this.Application.FilterPointerInput(this, e))
            {
                return true;
            }

            this.OnPointerWheelChanged(e);
            this.PointerWheelChanged?.Invoke(this, e);
            return e.Handled;
        }

        /// <summary>
        /// Delivers a pointer event from the platform to this view as a layer:
        /// to the lightweight view under it (or holding it), or to this view.
        /// </summary>
        internal bool DispatchLayerPointerEvent(
            PointerEventArgs e)
        {
            if (this.TrackLongPress(ref e))
            {
                return true;
            }

            if (this.HasLightweightChildren || (0 < (this._pointerOwners?.Count ?? 0)))
            {
                return this.DispatchToLightweight(e);
            }

            return this.DispatchPointerEvent(e);
        }

        /// <summary>
        /// Delivers a wheel event from the platform to this view as a layer: to
        /// the lightweight view under it and its ancestors, then this view.
        /// </summary>
        internal bool DispatchLayerPointerWheel(
            PointerEventArgs e)
        {
            if (this.HasLightweightChildren)
            {
                // The deepest lightweight view under the pointer, then its
                // ancestors, then this view
                for (var view = this.HitTestLightweight(e.Position); (null != view) && (view != this); view = view.Parent as View)
                {
                    if (view.DispatchPointerWheel(this.ToLocal(view, e)))
                    {
                        return true;
                    }
                }
            }

            return this.DispatchPointerWheel(e);
        }

        // The lightweight views that took pointers pressed on this layer
        private Dictionary<int, View>? _pointerOwners;

        /// <summary>
        /// How long a touch or pen is held still to ask for the context menu.
        /// </summary>
        internal static readonly TimeSpan LongPressDuration = TimeSpan.FromMilliseconds(500);

        // A touch or pen pressed and held: once held long enough, a context
        // request for the view under it; if answered, the press ends as
        // canceled (so it isn't also a tap)
        private sealed class LongPress
        {
            public View? Layer;
            public int PointerId;
            public long StartTicks;
            public Point Start;
            public View? Target;
            public Point TargetPosition;
            public PointerDeviceType DeviceType;
            public DispatcherTimer? Timer;
            public bool IsAnswered;
        }

        private static LongPress? _longPress;

        // Returns whether the event was used up (the moves and release of an
        // answered long press); the release becomes a cancel for the views
        private bool TrackLongPress(
            ref PointerEventArgs e)
        {
            var press = _longPress;
            bool isThisPress = (null != press) && (press.PointerId == e.PointerId);

            switch (e.State)
            {
                case PointerInputState.Began:
                    if (e.DeviceType == PointerDeviceType.Mouse)
                    {
                        return false;
                    }

                    // The same press, offered to an ancestor layer
                    if (isThisPress && ((Environment.TickCount64 - press.StartTicks) < 100))
                    {
                        return false;
                    }

                    press?.Timer.Stop();

                    var hit = this.HasLightweightChildren ? this.HitTestLightweight(e.Position) : null;
                    var target = ((null != hit) && (hit != this)) ? hit : this;
                    var started = new LongPress()
                    {
                        Layer = this,
                        PointerId = e.PointerId,
                        StartTicks = Environment.TickCount64,
                        Start = e.Position,
                        Target = target,
                        TargetPosition = (target == this) ? e.Position : this.ToLocal(target, e).Position,
                        DeviceType = e.DeviceType,
                        Timer = new DispatcherTimer() { Interval = LongPressDuration },
                    };
                    started.Timer.Tick += (sender, args) =>
                    {
                        started.Timer.Stop();
                        if (_longPress == started)
                        {
                            started.IsAnswered = started.Target.RouteContextRequested(new ContextRequestedEventArgs(started.TargetPosition, started.DeviceType));
                        }
                    };
                    _longPress = started;
                    started.Timer.Start();
                    return false;

                case PointerInputState.Moved:
                    if (!isThisPress || (press.Layer != this))
                    {
                        return false;
                    }

                    if (press.IsAnswered)
                    {
                        return true;
                    }

                    float dx = e.Position.X - press.Start.X;
                    float dy = e.Position.Y - press.Start.Y;
                    if ((dx * dx) + (dy * dy) > (Application.TapSlop * Application.TapSlop))
                    {
                        press.Timer.Stop();
                        _longPress = null;
                    }

                    return false;

                default:
                    if (!isThisPress || (press.Layer != this))
                    {
                        return false;
                    }

                    press.Timer.Stop();
                    _longPress = null;
                    if (press.IsAnswered)
                    {
                        e = new PointerEventArgs(e.PointerId, e.DeviceType, e.Position, PointerInputState.Canceled, false, e.KeyModifiers);
                    }

                    return false;
            }
        }

        // Routes a pointer event from the platform, which delivered it to this
        // layer, to the lightweight view under it: the deepest one, then its
        // ancestors, then this view. The view that takes a press gets the
        // pointer's moves and release.
        private bool DispatchToLightweight(
            PointerEventArgs e)
        {
            View? owner = null;
            this._pointerOwners?.TryGetValue(e.PointerId, out owner);

            if ((null != owner) && (e.State != PointerInputState.Began))
            {
                if ((e.State == PointerInputState.Ended) || (e.State == PointerInputState.Canceled))
                {
                    this._pointerOwners.Remove(e.PointerId);
                }

                return owner.DispatchPointerEvent(this.ToLocal(owner, e));
            }

            // A press finds its view; a hover move (no pointer pressed) is for
            // whatever is under it
            if ((e.State == PointerInputState.Began) || ((e.State == PointerInputState.Moved) && !e.IsInContact))
            {
                for (var view = this.HitTestLightweight(e.Position); (null != view) && (view != this); view = view.Parent as View)
                {
                    if (view.DispatchPointerEvent(this.ToLocal(view, e)))
                    {
                        if (e.State == PointerInputState.Began)
                        {
                            this._pointerOwners ??= new Dictionary<int, View>();
                            this._pointerOwners[e.PointerId] = view;
                        }

                        return true;
                    }
                }
            }

            return this.DispatchPointerEvent(e);
        }

        // The event in a lightweight descendant's coordinates
        private PointerEventArgs ToLocal(
            View view,
            PointerEventArgs e)
        {
            var offset = view.OffsetInLayer;
            return new PointerEventArgs(
                e.PointerId,
                e.DeviceType,
                new Point(e.Position.X - offset.X, e.Position.Y - offset.Y),
                e.State,
                e.IsInContact,
                e.KeyModifiers,
                e.WheelDelta)
            {
                OriginalSource = e.OriginalSource,
            };
        }
    }
}
