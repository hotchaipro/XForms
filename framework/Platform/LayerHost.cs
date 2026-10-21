using System;
using System.Collections.Generic;

namespace XForms.Platform
{
    /// <summary>
    /// The framework's side of the platform contract for one view the platform
    /// renders: what a platform calls to draw a layer's lightweight views, to
    /// deliver input, to move focus, and to describe the view to assistive
    /// technology. Get one with <see cref="For"/>; it holds nothing but the
    /// view.
    /// </summary>
    /// <remarks>
    /// A layer is a view with a native object that lightweight views draw into.
    /// Most of what a platform does with a layer is work on those lightweight
    /// views (painting them, hit testing them, routing keys among them, exposing
    /// them to a screen reader), so the framework can change how they work
    /// without changing a platform. See <see cref="ApplicationHost"/> for the
    /// rest of the contract.
    /// </remarks>
    internal readonly struct LayerHost
    {
        private readonly View _view;

        private LayerHost(
            View view)
        {
            this._view = view;
        }

        /// <summary>
        /// The host for a view the platform renders (or one of its lightweight
        /// descendants).
        /// </summary>
        public static LayerHost For(
            View view)
        {
            ArgumentNullException.ThrowIfNull(view);
            return new LayerHost(view);
        }

        /// <summary>
        /// The view.
        /// </summary>
        public View View
        {
            get
            {
                return this._view;
            }
        }

        // ── Lightweight views ──────────────────────────────────────────────

        /// <summary>
        /// Whether the view draws into its layer instead of having a native
        /// object of its own.
        /// </summary>
        public bool IsLightweight
        {
            get
            {
                return this._view.IsLightweight;
            }
        }

        /// <summary>
        /// Whether the view may be drawn into its layer (it has no animator of
        /// its own and nothing that needs a native object). Platforms that draw
        /// lightweight views themselves (Win32) check this; others get the
        /// framework's lightweight renderers.
        /// </summary>
        public bool CanBeLightweight
        {
            get
            {
                return this._view.CanBeLightweight;
            }
        }

        /// <summary>
        /// Whether lightweight views draw into this view.
        /// </summary>
        public bool HasLightweightChildren
        {
            get
            {
                return this._view.HasLightweightChildren;
            }
        }

        /// <summary>
        /// The view's children, in drawing order.
        /// </summary>
        public IReadOnlyList<View> Children
        {
            get
            {
                return this._view.DrawingOrderChildren;
            }
        }

        /// <summary>
        /// The renderer of the layer the view draws into (its own, for a view
        /// with a native object).
        /// </summary>
        public IViewRenderer Layer
        {
            get
            {
                return this._view.FindLayer();
            }
        }

        /// <summary>
        /// Where the view is in its layer's coordinates.
        /// </summary>
        public Point OffsetInLayer
        {
            get
            {
                return this._view.OffsetInLayer;
            }
        }

        // ── Drawing ────────────────────────────────────────────────────────

        /// <summary>
        /// Whether the view draws its own content (in <see cref="Render"/>),
        /// beneath its children. A platform makes a drawing surface for it.
        /// </summary>
        public bool DrawsContent
        {
            get
            {
                return this._view.DrawsContentInternal;
            }
        }

        /// <summary>
        /// Draws a view that draws itself (its <c>Render</c>, with its drawn
        /// animations) within the bounds.
        /// </summary>
        public void Render(
            DrawingContext context,
            Rect bounds)
        {
            this._view.RenderContent(context, bounds);
        }

        /// <summary>
        /// Draws the lightweight views inside this layer (in drawing order,
        /// each at its position), beneath its native children. The context
        /// draws in the layer's coordinates.
        /// </summary>
        public void PaintLightweightViews(
            DrawingContext context)
        {
            LightweightPainter.PaintDescendants(this._view, context);
        }

        /// <summary>
        /// Draws a ring around this layer's focused lightweight view, above its
        /// lightweight content.
        /// </summary>
        public void PaintFocusRing(
            DrawingContext context)
        {
            this._view.PaintLightweightFocusRing(context);
        }

        /// <summary>
        /// For a platform that draws lightweight views itself: the area a
        /// lightweight view draws in (in its own coordinates), which its
        /// <see cref="DrawingContext.Clear"/> fills instead of clearing the
        /// layer. Returns the previous area, to restore afterwards.
        /// </summary>
        public static Rect? SetLightweightBounds(
            DrawingContext context,
            Rect? bounds)
        {
            var previous = context.LightweightBounds;
            context.LightweightBounds = bounds;
            return previous;
        }

        // ── Layout ─────────────────────────────────────────────────────────

        /// <summary>
        /// Reports the size the platform gave a root view it lays out natively.
        /// </summary>
        public void NotifyRootSize(
            Size size)
        {
            this._view.NotifyRootSize(size);
        }

        // ── Input ──────────────────────────────────────────────────────────

        /// <summary>
        /// Delivers a pointer event (in the view's coordinates) to the view:
        /// to the lightweight view under it (or holding the pointer) when it's a
        /// layer, or to the view itself. Returns whether the view takes the
        /// pointer (for a press) or claims its moves (for a move).
        /// </summary>
        public bool DispatchPointer(
            PointerEventArgs e)
        {
            return this._view.DispatchLayerPointerEvent(e);
        }

        /// <summary>
        /// Delivers a wheel event to the view: to the lightweight view under it
        /// and its ancestors when it's a layer, then the view itself. Returns
        /// whether it was handled; otherwise the platform offers it to the
        /// view's ancestors, then scrolls.
        /// </summary>
        public bool DispatchWheel(
            PointerEventArgs e)
        {
            return this._view.DispatchLayerPointerWheel(e);
        }

        /// <summary>
        /// The deepest visible, hit-testable lightweight view under the point
        /// (in this view's coordinates), or null.
        /// </summary>
        public View HitTestLightweight(
            Point point)
        {
            return this._view.HitTestLightweight(point);
        }

        /// <summary>
        /// The deepest view under the point (in this view's coordinates) that a
        /// hovering pointer is over: a lightweight view drawn into this one, or
        /// this view.
        /// </summary>
        public View HitTestPointerOver(
            Point point)
        {
            return this._view.HitTestPointerOver(point);
        }

        // ── Keys and focus ─────────────────────────────────────────────────

        /// <summary>
        /// Activates the view, as Enter or Space would while it has focus (for
        /// a platform that handles those keys itself).
        /// </summary>
        public void Activate()
        {
            this._view.Activate();
        }

        /// <summary>
        /// Delivers a key press to the view alone (a platform that tracks focus
        /// itself offers it to the focused view, then its ancestors). Returns
        /// whether it was handled.
        /// </summary>
        public bool DispatchKeyDown(
            KeyEventArgs e)
        {
            return this._view.DispatchKeyDown(e);
        }

        /// <summary>
        /// Delivers a key release to the view alone.
        /// </summary>
        public bool DispatchKeyUp(
            KeyEventArgs e)
        {
            return this._view.DispatchKeyUp(e);
        }

        /// <summary>
        /// Delivers a key press from the layer's native view: to its focused
        /// lightweight view, or to the layer, then their ancestors. Tab moves
        /// among the lightweight views. Returns whether the key was used.
        /// </summary>
        public bool RouteKeyDown(
            KeyEventArgs e)
        {
            return this._view.RouteLayerKeyDown(e);
        }

        /// <summary>
        /// Delivers a key release from the layer's native view.
        /// </summary>
        public bool RouteKeyUp(
            KeyEventArgs e)
        {
            return this._view.RouteLayerKeyUp(e);
        }

        /// <summary>
        /// Moves keyboard focus to the next (or previous) focusable view after
        /// this one, among lightweight and native views alike, in view order.
        /// Returns whether it moved.
        /// </summary>
        public bool MoveFocus(
            bool isForward)
        {
            return this._view.MoveFocus(isForward);
        }

        /// <summary>
        /// The layer's native view gained or lost focus.
        /// </summary>
        public void NativeFocusChanged(
            bool hasFocus,
            bool isBackward)
        {
            this._view.OnNativeFocusChanged(hasFocus, isBackward);
        }

        /// <summary>
        /// The lightweight view in this layer that has keyboard focus, or null.
        /// </summary>
        public View FocusedLightweight
        {
            get
            {
                return this._view.FocusedLightweight;
            }
        }

        /// <summary>
        /// Whether a lightweight view in this layer can take keyboard focus.
        /// </summary>
        public bool HasFocusableLightweight
        {
            get
            {
                return this._view.HasFocusableLightweight;
            }
        }

        // ── Accessibility ──────────────────────────────────────────────────

        /// <summary>
        /// Raised when a view's accessible state, value or label changes.
        /// </summary>
        public static event Action<View> AccessibilityStateChanged
        {
            add { View.AccessibilityStateChanged += value; }
            remove { View.AccessibilityStateChanged -= value; }
        }

        /// <summary>
        /// Whether the view is left out of the accessibility tree (hidden, or
        /// scaled or faded to nothing).
        /// </summary>
        public bool IsHiddenFromAccessibility
        {
            get
            {
                return this._view.IsHiddenFromAccessibility;
            }
        }

        /// <summary>
        /// What a screen reader reads for the view: its label, or the text of
        /// what's inside it.
        /// </summary>
        public string AccessibilityText
        {
            get
            {
                return this._view.AccessibilityText;
            }
        }

        /// <summary>
        /// Whether a screen reader's activate gesture does anything.
        /// </summary>
        public bool IsAccessibilityActionable
        {
            get
            {
                return this._view.IsAccessibilityActionable;
            }
        }

        /// <summary>
        /// What a screen reader's activate gesture does: toggles, selects or
        /// invokes. Returns whether it did anything.
        /// </summary>
        public bool PerformDefaultAccessibilityAction()
        {
            return this._view.PerformDefaultAccessibilityAction();
        }
    }
}
