using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using XForma.Platform;

namespace XForma
{
    // How a view draws into its layer instead of having a native view of its own, and keyboard focus among such views
    public partial class View
    {
        /// <summary>
        /// Whether views of this type can be lightweight: containers (which
        /// draw themselves or only lay out children) and text blocks. Views
        /// the platforms render natively override it.
        /// </summary>
        internal virtual bool IsLightweightType
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Whether the view's children must have native objects: a scrolling
        /// view's content, for example, is a layer of its own.
        /// </summary>
        internal virtual bool RequiresNativeChildren
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Whether the view is lightweight when its renderer is created: a
        /// lightweight type, with no animator, inside a parent that allows it.
        /// Platforms check this when they create the view's renderer.
        /// </summary>
        internal bool CanBeLightweight
        {
            get
            {
                if (AppContext.TryGetSwitch("XForma.DisableLightweightViews", out bool isDisabled) && isDisabled)
                {
                    return false;
                }

                if (!this.IsLightweightType || (null != this._animator) || (null != this._ownAnimators) || this.HasFrameworkAnimator)
                {
                    return false;
                }

                // A view that clips its children clips them natively (its
                // layer can't clip a native child, such as an animated one)
                if (this._clipToBounds)
                {
                    return false;
                }

                // A view raised over its siblings is a layer of its own: its
                // layer draws lightweight views beneath every native view in it
                if (0 != this._zIndex)
                {
                    return false;
                }

                return (this.Parent is View parent) && !parent.RequiresNativeChildren;
            }
        }

        /// <summary>
        /// Whether the view is lightweight: it has no native object, and draws
        /// into its layer.
        /// </summary>
        internal bool IsLightweight
        {
            get
            {
                return this.CreatedRenderer is LightweightRenderer;
            }
        }

        /// <summary>
        /// The view's children in drawing order. Lightweight drawing walks
        /// these every frame, so they're lists (indexed without allocating).
        /// </summary>
        internal virtual IReadOnlyList<View> DrawingOrderChildren
        {
            get
            {
                if (null != this._drawingOrder)
                {
                    return this._drawingOrder;
                }

                List<View>? children = null;
                foreach (var child in this.GetChildElements())
                {
                    if (child is View view)
                    {
                        children ??= new List<View>();
                        children.Add(view);
                    }
                }

                return (IReadOnlyList<View>)children ?? Array.Empty<View>();
            }
        }

        internal bool HasLightweightChildren
        {
            get
            {
                var children = this.DrawingOrderChildren;
                for (int i = 0; i < children.Count; i += 1)
                {
                    if (children[i].IsLightweight)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        /// <summary>
        /// The native renderer a lightweight view draws into (or that hosts a
        /// native view inside lightweight ancestors): the nearest ancestor with
        /// a native object, or null before the view is attached to one.
        /// </summary>
        internal IViewRenderer? FindLayer()
        {
            for (var parent = this.Parent as View; null != parent; parent = parent.Parent as View)
            {
                if (!parent.HasRenderer)
                {
                    return null;
                }

                if (!parent.IsLightweight)
                {
                    return (IViewRenderer)parent.CreatedRenderer;
                }
            }

            return null;
        }

        /// <summary>
        /// Where a lightweight view's origin is in its layer's coordinates.
        /// </summary>
        internal Point OffsetInLayer
        {
            get
            {
                float x = 0, y = 0;
                for (var view = this; (null != view) && view.IsLightweight; view = view.Parent as View)
                {
                    x += view._frame.X;
                    y += view._frame.Y;
                }

                return new Point(x, y);
            }
        }

        // Whether every lightweight ancestor up to the layer is visible (a
        // hidden lightweight view can't hide the native views inside it)
        internal bool AreLightweightAncestorsVisible
        {
            get
            {
                for (var parent = this.Parent as View; (null != parent) && parent.IsLightweight; parent = parent.Parent as View)
                {
                    if (!parent._isVisible)
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        // The layer hosting this native view, if its parent is lightweight
        private IViewRenderer? _hostLayer;

        private void HostNativeDescendants()
        {
            if (!this.HasRenderer)
            {
                return;
            }

            if (!this.IsLightweight)
            {
                if ((this.Parent is View parent) && parent.IsLightweight)
                {
                    var layer = this.FindLayer();
                    if ((null != layer) && (layer != this._hostLayer))
                    {
                        this._hostLayer?.RemoveHostedView(this.Native);
                        this._hostLayer = null;

                        int index = 0;
                        ((View)layer.Element).CountHostedBefore(this, layer, ref index);
                        layer.AddHostedView(this.Native, index);
                        this._hostLayer = layer;
                        this.Native.IsVisible = this._isVisible && this.AreLightweightAncestorsVisible;
                    }
                }

                return;
            }

            var children = this.DrawingOrderChildren;
            for (int i = 0; i < children.Count; i += 1)
            {
                var child = children[i];
                child.HostNativeDescendants();
            }
        }

        // Counts the views the layer hosts that come before the view in drawing
        // order (a depth-first walk through lightweight views). Returns whether
        // it found the view.
        private bool CountHostedBefore(
            View view,
            IViewRenderer layer,
            ref int count)
        {
            var children = this.DrawingOrderChildren;
            for (int i = 0; i < children.Count; i += 1)
            {
                var child = children[i];
                if (child == view)
                {
                    return true;
                }

                if (child._hostLayer == layer)
                {
                    count += 1;
                }
                else if (child.IsLightweight && child.CountHostedBefore(view, layer, ref count))
                {
                    return true;
                }
            }

            return false;
        }

        private void UnhostNativeDescendants()
        {
            if (null != this._hostLayer)
            {
                this._hostLayer.RemoveHostedView(this.Native);
                this._hostLayer = null;
                return;
            }

            if (this.IsLightweight)
            {
                var children = this.DrawingOrderChildren;
                for (int i = 0; i < children.Count; i += 1)
                {
                    var child = children[i];
                    child.UnhostNativeDescendants();
                }
            }
        }

        /// <summary>
        /// Shows or hides the native views inside a lightweight view whose
        /// visibility changed.
        /// </summary>
        internal void UpdateHostedVisibility()
        {
            var children = this.DrawingOrderChildren;
            for (int i = 0; i < children.Count; i += 1)
            {
                var child = children[i];
                if (child.IsLightweight)
                {
                    child.UpdateHostedVisibility();
                }
                else if (null != child._hostLayer)
                {
                    child.Native.IsVisible = child._isVisible && child.AreLightweightAncestorsVisible;
                }
            }
        }

        /// <summary>
        /// The deepest visible, hit-testable lightweight view under the point
        /// (in this view's coordinates), or null.
        /// </summary>
        internal View? HitTestLightweight(
            Point point)
        {
            View? hit = null;
            var children = this.DrawingOrderChildren;
            for (int i = 0; i < children.Count; i += 1)
            {
                var child = children[i];
                // The last match is on top
                if (!child.IsLightweight || !child._isVisible || !child._isHitTestVisible)
                {
                    continue;
                }

                var frame = child._frame;
                if ((point.X >= frame.X) && (point.Y >= frame.Y) && (point.X < frame.Right) && (point.Y < frame.Bottom))
                {
                    hit = child.HitTestLightweight(new Point(point.X - frame.X, point.Y - frame.Y)) ?? child;
                }
                else
                {
                    // Its outset takes the press too
                    var outset = child._hitTestOutset;
                    if ((Thickness.Zero != outset)
                        && (point.X >= frame.X - outset.Left) && (point.Y >= frame.Y - outset.Top)
                        && (point.X < frame.Right + outset.Right) && (point.Y < frame.Bottom + outset.Bottom))
                    {
                        hit = child;
                    }
                }
            }

            return hit;
        }

        #region Lightweight focus

        // On Android and iOS, a lightweight view has no native object to take
        // keyboard focus: its layer's native view takes it, and the layer
        // keeps track of which lightweight view inside it is focused.
        private View? _focusedLightweight;
        private View? _pendingLightweightFocus;
        private bool _hasNativeFocus;

        /// <summary>
        /// The lightweight view inside this layer that has keyboard focus, or
        /// null.
        /// </summary>
        internal View? FocusedLightweight
        {
            get
            {
                return this._focusedLightweight;
            }
        }

        /// <summary>
        /// The layer view (the nearest ancestor with a native object) that a
        /// lightweight view draws into.
        /// </summary>
        internal View? LayerView
        {
            get
            {
                for (var parent = this.Parent as View; null != parent; parent = parent.Parent as View)
                {
                    if (!parent.IsLightweight)
                    {
                        return parent;
                    }
                }

                return null;
            }
        }

        /// <summary>
        /// Whether a lightweight view inside this layer can take keyboard focus.
        /// </summary>
        internal bool HasFocusableLightweight
        {
            get
            {
                return this.HasLightweightChildren && (0 < this.GetFocusableLightweight().Count);
            }
        }

        // The focusable lightweight views inside this layer, in order
        private List<View> GetFocusableLightweight()
        {
            var result = new List<View>();
            CollectFocusableLightweight(this, result);
            return result;
        }

        private static void CollectFocusableLightweight(
            View parent,
            List<View> result)
        {
            var children = parent.TabOrderChildren;
            for (int i = 0; i < children.Count; i += 1)
            {
                var child = children[i];
                if (!child.IsLightweight || !child._isVisible || !child._isHitTestVisible || !parent.IsInTabOrder(child))
                {
                    continue;
                }

                if (child.Focusable)
                {
                    result.Add(child);
                }

                CollectFocusableLightweight(child, result);
            }
        }

        private void SetLightweightFocus(
            View? view)
        {
            var previous = this._focusedLightweight;
            if (previous == view)
            {
                return;
            }

            this._focusedLightweight = view;
            if (null != previous)
            {
                previous.IsFocused = false;
            }

            if (null != view)
            {
                view.IsFocused = true;
            }

            // Draws the focus ring
            this.Native?.InvalidateLayerContent();
        }

        /// <summary>
        /// Moves keyboard focus to a lightweight view inside this layer, taking
        /// native focus for the layer if it hasn't it.
        /// </summary>
        internal bool FocusLightweight(
            View view)
        {
            if (this._hasNativeFocus)
            {
                this.IsFocused = false;
                this.SetLightweightFocus(view);
                return true;
            }

            this._pendingLightweightFocus = view;
            bool result = this.Native?.Focus() ?? false;
            this._pendingLightweightFocus = null;
            return result;
        }

        /// <summary>
        /// The layer's native view gained or lost keyboard focus. Focus goes to
        /// the lightweight view asked for, or to the layer itself when it is
        /// focusable, or else to its first (or, moving backward, last)
        /// focusable lightweight view.
        /// </summary>
        internal void OnNativeFocusChanged(
            bool hasFocus,
            bool isBackward)
        {
            this._hasNativeFocus = hasFocus;
            if (!hasFocus)
            {
                this.SetLightweightFocus(null);
                this.IsFocused = false;
                return;
            }

            var target = this._pendingLightweightFocus;
            if ((null == target) && !this.Focusable)
            {
                var focusable = this.GetFocusableLightweight();
                if (0 < focusable.Count)
                {
                    target = isBackward ? focusable[focusable.Count - 1] : focusable[0];
                }
            }

            if (null != target)
            {
                this.SetLightweightFocus(target);
            }
            else
            {
                this.IsFocused = true;
            }
        }

        /// <summary>
        /// Where keys and focus go after this view: its parent, or for an
        /// item's container (which isn't attached), its items view.
        /// </summary>
        internal View? KeyboardParent
        {
            get
            {
                return (this.Parent as View) ?? (this as Primitives.ItemContainer)?.ItemsView;
            }
        }
        /// <summary>
        /// Moves keyboard focus from this view to the next (or previous)
        /// focusable view in the tree it is in, wrapping around: lightweight
        /// and native views alike, in tab order (TabIndex, then view order),
        /// as on Win32. (Android and
        /// iOS order native views by position, which puts a layer before the
        /// views inside it.)
        /// </summary>
        internal bool MoveFocus(
            bool isForward)
        {
            SetFocusRingVisible(true);

            var root = this;
            while (root.KeyboardParent is View parent)
            {
                root = parent;
            }

            var focusable = new List<View>();
            CollectFocusable(root.ActiveTabScope, focusable);
            // From outside the list, every view is a candidate; from inside,
            // every other view
            int index = focusable.IndexOf(this);
            int count = (index < 0) ? focusable.Count + 1 : focusable.Count;
            for (int i = 1; i < count; i += 1)
            {
                int next = (index < 0)
                    ? (isForward ? i - 1 : focusable.Count - i)
                    : (index + (isForward ? i : -i) + (focusable.Count * 2)) % focusable.Count;
                if (focusable[next].Focus())
                {
                    return true;
                }
            }

            return false;
        }

        private static void CollectFocusable(
            View parent,
            List<View> result)
        {
            var children = parent.TabOrderChildren;
            for (int i = 0; i < children.Count; i += 1)
            {
                var child = children[i];
                if (!child._isVisible || !child._isHitTestVisible || !child.HasRenderer || !parent.IsInTabOrder(child))
                {
                    continue;
                }

                // A list is one stop: its current item
                if (child is Primitives.ItemsView itemsView)
                {
                    var stop = itemsView.TabStopContainer;
                    if (null != stop)
                    {
                        result.Add(stop);
                    }

                    continue;
                }

                if (child.IsTabStop || (child is Primitives.TextBox))
                {
                    result.Add(child);
                }

                CollectFocusable(child, result);
            }
        }

        /// <summary>
        /// Whether Tab can reach this child, and the views inside it: not
        /// content an open drawer covers, say.
        /// </summary>
        [PlatformContract]
        protected internal virtual bool IsInTabOrder(
            View child)
        {
            return true;
        }

        /// <summary>
        /// Whether Tab stays inside this view for now, wrapping around, as in
        /// an open drawer.
        /// </summary>
        protected internal virtual bool IsTabScope
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Where Tab moves among, in the tree under this view: the view inside
        /// it that's keeping Tab, or else this view.
        /// </summary>
        [PlatformContract]
        internal View ActiveTabScope
        {
            get
            {
                return this.FindTabScope() ?? this;
            }
        }

        private View? FindTabScope()
        {
            if (!this._isVisible)
            {
                return null;
            }

            if (this.IsTabScope)
            {
                return this;
            }

            var children = this.DrawingOrderChildren;
            for (int i = children.Count - 1; i >= 0; i -= 1)
            {
                var scope = children[i].FindTabScope();
                if (null != scope)
                {
                    return scope;
                }
            }

            return null;
        }

        /// <summary>
        /// Whether Tab stops at the view: it's focusable, and (for an item in a
        /// list) it's the list's current item. Arrow keys reach the rest.
        /// </summary>
        [PlatformContract]
        protected internal virtual bool IsTabStop
        {
            get
            {
                return this.Focusable;
            }
        }

        /// <summary>
        /// Delivers a key from the layer's native view: to its focused
        /// lightweight view, or to the layer. Tab moves among the lightweight
        /// views. Returns whether the key was used.
        /// </summary>
        internal bool RouteLayerKeyDown(
            KeyEventArgs e)
        {
            if (DragDropSession.HandleKey(this._focusedLightweight ?? this, e))
            {
                return true;
            }

            if ((e.Key == Key.Tab) && ((e.KeyModifiers & ~KeyModifiers.Shift) == KeyModifiers.None)
                && (this._focusedLightweight ?? this).MoveFocus(isForward: !e.KeyModifiers.HasFlag(KeyModifiers.Shift)))
            {
                return true;
            }

            return (this._focusedLightweight ?? this).RouteKeyDown(e);
        }

        internal bool RouteLayerKeyUp(
            KeyEventArgs e)
        {
            return (this._focusedLightweight ?? this).RouteKeyUp(e);
        }

        /// <summary>
        /// Draws a ring around the focused lightweight view, above the layer's
        /// lightweight content.
        /// </summary>
        internal void PaintLightweightFocusRing(
            DrawingContext context)
        {
            var focused = this._focusedLightweight;
            if ((null == focused) || !focused.IsLightweight || (focused.LayerView != this) || !_isFocusRingVisible)
            {
                return;
            }

            const float outset = 2.0f;
            const float inset = 1.0f;
            var offset = focused.OffsetInLayer;
            var size = focused.Bounds;

            // Around the view, but inside the layer, which clips it (a view
            // that fills a list's or a scroll viewer's width)
            float left = Math.Max(offset.X - outset, inset);
            float top = Math.Max(offset.Y - outset, inset);
            float right = Math.Min(offset.X + size.Width + outset, this.Bounds.Width - inset);
            float bottom = Math.Min(offset.Y + size.Height + outset, this.Bounds.Height - inset);
            context.DrawRoundedRectangle(
                new Rect(left, top, Math.Max(0, right - left), Math.Max(0, bottom - top)),
                4.0f,
                this.ActualTheme.Colors.Foreground,
                2.0f);
        }

        #endregion Lightweight focus
    }
}
