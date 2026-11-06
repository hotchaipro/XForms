using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using XForma.Platform;

namespace XForma
{
    // Keys and keyboard focus
    public partial class View
    {
        private bool? _isKeyboardFocusable;
        private bool _isFocused;

        public event EventHandler? GotFocus;

        public event EventHandler? LostFocus;

        public event EventHandler<KeyEventArgs>? KeyDown;

        public event EventHandler<KeyEventArgs>? KeyUp;

        /// <summary>
        /// Whether keyboard focus can move to the view (with Tab). By default,
        /// views that respond to taps are focusable.
        /// </summary>
        public bool Focusable
        {
            get
            {
                return this._isKeyboardFocusable ?? (null != this._activated);
            }

            set
            {
                this._isKeyboardFocusable = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Where Tab reaches the view among its siblings: lower first, and
        /// siblings with the same index in view order. A view's descendants
        /// come before its next sibling. By default <see cref="int.MaxValue"/>,
        /// so views given an index come before the rest, as in WinUI.
        /// </summary>
        public int TabIndex
        {
            get
            {
                return this._tabIndex;
            }

            set
            {
                this._tabIndex = value;
                this.OnPropertyChanged();
            }
        }

        private int _tabIndex = int.MaxValue;

        /// <summary>
        /// The view's children in the order Tab reaches them: by
        /// <see cref="TabIndex"/>, then in drawing order.
        /// </summary>
        internal IReadOnlyList<View> TabOrderChildren
        {
            get
            {
                var children = this.DrawingOrderChildren;
                bool isOrdered = true;
                for (int i = 1; isOrdered && (i < children.Count); i += 1)
                {
                    isOrdered = children[i - 1]._tabIndex <= children[i]._tabIndex;
                }

                if (isOrdered)
                {
                    return children;
                }

                // A stable sort: the same index keeps drawing order
                var ordered = new List<View>(children);
                for (int i = 1; i < ordered.Count; i += 1)
                {
                    var child = ordered[i];
                    int j = i - 1;
                    while ((0 <= j) && (ordered[j]._tabIndex > child._tabIndex))
                    {
                        ordered[j + 1] = ordered[j];
                        j -= 1;
                    }

                    ordered[j + 1] = child;
                }

                return ordered;
            }
        }

        /// <summary>
        /// Counts the moves of keyboard focus, asked for or made, so a tap can
        /// tell whether it moved focus itself.
        /// </summary>
        internal static int FocusRequests;

        // The view with keyboard focus, and whether its ring shows
        private static View? _focusedView;
        private static bool _isFocusRingVisible;

        /// <summary>
        /// Whether focus rings show: after keys move focus (Tab, the arrows),
        /// not after a tap or a click, as the web's :focus-visible.
        /// </summary>
        internal static bool IsFocusRingVisible
        {
            get
            {
                return _isFocusRingVisible;
            }
        }

        // The keyboard shows focus rings, a pointer hides them
        internal static void SetFocusRingVisible(
            bool isVisible)
        {
            if (isVisible == _isFocusRingVisible)
            {
                return;
            }

            _isFocusRingVisible = isVisible;
            var focused = _focusedView;
            if (null != focused)
            {
                var layer = focused.IsLightweight ? focused.LayerView : focused;
                layer?.Native?.InvalidateLayerContent();
            }
        }

        /// <summary>
        /// Whether the view has keyboard focus.
        /// </summary>
        public bool IsFocused
        {
            get
            {
                return this._isFocused;
            }

            internal set
            {
                if (value != this._isFocused)
                {
                    this._isFocused = value;
                    if (value)
                    {
                        _focusedView = this;
                        FocusRequests += 1;
                    }
                    else if (_focusedView == this)
                    {
                        _focusedView = null;
                    }

                    this.OnFocusChangedCore(value);

                    if (value)
                    {
                        this.OnGotFocus();
                        this.GotFocus?.Invoke(this, EventArgs.Empty);
                    }
                    else
                    {
                        this.OnLostFocus();
                        this.LostFocus?.Invoke(this, EventArgs.Empty);
                    }

                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Moves keyboard focus to the view. Returns false where the platform
        /// doesn't support keyboard focus for the view.
        /// </summary>
        public bool Focus()
        {
            FocusRequests += 1;

            // A layer with focus inside it takes it back from its lightweight
            // view
            if (this._hasNativeFocus && (null != this._focusedLightweight))
            {
                this.SetLightweightFocus(null);
                this.IsFocused = true;
                return true;
            }

            return this.Native?.Focus() ?? false;
        }

        /// <summary>
        /// Called when the view gains or loses keyboard focus, before
        /// <see cref="OnGotFocus"/> or <see cref="OnLostFocus"/>.
        /// </summary>
        internal virtual void OnFocusChangedCore(
            bool isFocused)
        {
        }

        protected virtual void OnGotFocus()
        {
        }

        protected virtual void OnLostFocus()
        {
        }

        /// <summary>
        /// Called when a key is pressed while the view (or a descendant, which
        /// didn't handle it) has keyboard focus. Set <c>e.Handled</c> to stop it
        /// going to the view's ancestors.
        /// </summary>
        protected virtual void OnKeyDown(
            KeyEventArgs e)
        {
        }

        /// <summary>
        /// Called when a key is released while the view (or a descendant) has
        /// keyboard focus.
        /// </summary>
        protected virtual void OnKeyUp(
            KeyEventArgs e)
        {
        }

        /// <summary>
        /// Delivers a key press to the view; platforms call it for the focused
        /// view, then its ancestors until one handles it.
        /// </summary>
        internal bool DispatchKeyDown(
            KeyEventArgs e)
        {
            e.OriginalSource ??= this;

            if ((e.OriginalSource == this) && this.Application.FilterKeyInput(this, e))
            {
                return true;
            }

            this.OnKeyDown(e);
            this.KeyDown?.Invoke(this, e);
            return e.Handled;
        }

        internal bool DispatchKeyUp(
            KeyEventArgs e)
        {
            e.OriginalSource ??= this;

            this.OnKeyUp(e);
            this.KeyUp?.Invoke(this, e);
            return e.Handled;
        }

        /// <summary>
        /// Whether a single-line text box offers the key to its ancestors
        /// before handling it (it has no use for it): Up, Down, Page Up, Page
        /// Down and Escape, for a suggestion list or a number box around it.
        /// </summary>
        [PlatformContract]
        internal static bool IsTextBoxNavigationKey(
            Key key)
        {
            return (key == Key.Up) || (key == Key.Down) || (key == Key.PageUp) || (key == Key.PageDown) || (key == Key.Escape);
        }

        /// <summary>
        /// Delivers a key press to the focused view and then its ancestors, until
        /// one handles it. Enter and Space that nothing handles activate the
        /// focused view. Returns whether the key was used.
        /// </summary>
        internal bool RouteKeyDown(
            KeyEventArgs e)
        {
            SetFocusRingVisible(true);

            if (DragDropSession.HandleKey(this, e))
            {
                return true;
            }

            for (var view = this; null != view; view = view.KeyboardParent)
            {
                if (view.DispatchKeyDown(e))
                {
                    return true;
                }
            }

            // The Menu key and Shift+F10 ask for the focused view's context menu
            if ((e.Key == Key.Menu) || ((e.Key == Key.F10) && e.IsShiftDown))
            {
                return this.RouteContextRequested(new ContextRequestedEventArgs(null, null));
            }

            if ((e.Key == Key.Enter) || (e.Key == Key.Space))
            {
                this.OnActivate();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Raised when the user asks for the view's context menu (a
        /// right-click, a long press, the Menu key or Shift+F10), and then for
        /// its ancestors until one handles it.
        /// </summary>
        public event EventHandler<ContextRequestedEventArgs>? ContextRequested;

        /// <summary>
        /// Called when the user asks for the view's (or, unhandled, a
        /// descendant's) context menu. Set <c>e.Handled</c> to answer it.
        /// </summary>
        protected virtual void OnContextRequested(
            ContextRequestedEventArgs e)
        {
        }

        /// <summary>
        /// Delivers a context request to the view, then its ancestors, until
        /// one handles it. Returns whether one did.
        /// </summary>
        internal bool RouteContextRequested(
            ContextRequestedEventArgs e)
        {
            e.OriginalSource ??= this;
            for (var view = this; null != view; view = view.KeyboardParent)
            {
                view.OnContextRequested(e);
                view.ContextRequested?.Invoke(view, e);
                if (e.Handled)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Asks for the context menu of the view under the point (in this
        /// view's coordinates; a lightweight one when this is a layer), or,
        /// without a point, of the focused view.
        /// </summary>
        internal bool RequestContext(
            Point? position,
            PointerDeviceType? deviceType)
        {
            var target = this;
            Point? local = position;
            if (position.HasValue)
            {
                var hit = this.HasLightweightChildren ? this.HitTestLightweight(position.Value) : null;
                if ((null != hit) && (hit != this))
                {
                    target = hit;
                    local = this.ToLocal(hit, new PointerEventArgs(position.Value, PointerInputState.Ended, false)).Position;
                }
            }
            else if (null != this._focusedLightweight)
            {
                target = this._focusedLightweight;
            }

            return target.RouteContextRequested(new ContextRequestedEventArgs(local, deviceType));
        }

        internal bool RouteKeyUp(
            KeyEventArgs e)
        {
            for (var view = this; null != view; view = view.KeyboardParent)
            {
                if (view.DispatchKeyUp(e))
                {
                    return true;
                }
            }

            return false;
        }

        private EventHandler? _activated;

        /// <summary>
        /// Raised when the view is activated without a pointer: Enter or Space
        /// while it has focus, or a screen reader's or UI Automation's activate
        /// command. Handling it says the view does something when activated,
        /// so it's focusable unless <see cref="Focusable"/> says otherwise.
        /// Gestures that respond to taps handle it, as a tap.
        /// </summary>
        public event EventHandler Activated
        {
            add
            {
                bool wasFocusable = this.Focusable;
                this._activated += value;
                if (wasFocusable != this.Focusable)
                {
                    this.OnPropertyChanged(nameof(this.Focusable));
                }
            }

            remove
            {
                bool wasFocusable = this.Focusable;
                this._activated -= value;
                if (wasFocusable != this.Focusable)
                {
                    this.OnPropertyChanged(nameof(this.Focusable));
                }
            }
        }

        /// <summary>
        /// Activates the view, as Enter or Space would while it has focus.
        /// </summary>
        internal void Activate()
        {
            this.OnActivate();
        }

        /// <summary>
        /// Called for Enter or Space on a focused view, and for a screen
        /// reader's activate command. By default this raises
        /// <see cref="Activated"/>; a control that does something else
        /// overrides it.
        /// </summary>
        protected internal virtual void OnActivate()
        {
            this._activated?.Invoke(this, EventArgs.Empty);
        }
    }
}
