using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.Versioning;
using XForms.Primitives;
using XForms.Platform;

namespace XForms.Web.Renderers
{
    [SupportedOSPlatform("browser")]
    internal abstract class ElementRenderer : IElementRenderer
    {
        protected ElementRenderer(
            Element element)
        {
            this.Element = element ?? throw new ArgumentNullException(nameof(element));
        }

        public Element Element
        {
            get;
        }

        public abstract object NativeElement
        {
            get;
        }

        public virtual void OnThemeChanged(
            Theme theme)
        {
        }
    }

    /// <summary>
    /// A view with a DOM element of its own, positioned absolutely in its
    /// parent's. A layer draws its own content and its lightweight
    /// descendants into a canvas beneath its child elements.
    /// </summary>
    [SupportedOSPlatform("browser")]
    internal class ViewRenderer : ElementRenderer, IViewRenderer
    {
        private readonly bool _isContainer;

        // The view's children, in order (lightweight ones have no element),
        // and the native views of lightweight descendants it hosts, whose
        // elements come after its own children's
        private readonly List<IViewRenderer> _ownChildren = new List<IViewRenderer>();
        private readonly List<ViewRenderer> _hostedViews = new List<ViewRenderer>();

        private int _canvasId;
        private Rect _frame;
        private bool _isVisible = true;
        private bool _isHitTestVisible = true;
        private Color _backgroundColor = Colors.Transparent;
        private AnimationLayerStack _layers;

        protected ViewRenderer(
            View view,
            string tag,
            bool isContainer)
            : base(view)
        {
            this._isContainer = isContainer;
            this.Id = Js.Create(tag);
            WebHost.Current.Register(this);

            // Keyboard focus can move to focusable views (with Tab)
            if (view.Focusable)
            {
                this.EnsureFocusable();
            }

            this.ApplyAccessibility();
        }

        /// <summary>
        /// The element's id in the page's table.
        /// </summary>
        public int Id
        {
            get;
        }

        public override object NativeElement
        {
            get
            {
                return this.Id;
            }
        }

        protected WebHost Host
        {
            get
            {
                return WebHost.Current;
            }
        }

        protected View View
        {
            get
            {
                return (View)this.Element;
            }
        }

        public Rect Frame
        {
            get
            {
                return this._frame;
            }
        }

        public ViewRenderer ParentRenderer
        {
            get;
            private set;
        }

        /// <summary>
        /// The children with elements of their own, in order.
        /// </summary>
        protected IReadOnlyList<ViewRenderer> Children
        {
            get
            {
                var children = new List<ViewRenderer>();
                foreach (var child in this._ownChildren)
                {
                    if (child is ViewRenderer native)
                    {
                        children.Add(native);
                    }
                }

                return children;
            }
        }

        protected static string Px(
            float value)
        {
            return value.ToString("0.###", CultureInfo.InvariantCulture) + "px";
        }

        protected static string Css(
            Color color)
        {
            return string.Format(CultureInfo.InvariantCulture, "rgba({0},{1},{2},{3:0.###})", color.R, color.G, color.B, color.A / 255.0);
        }

        #region Children

        // The element that the element of the child at the index goes before:
        // the next child's, or the first hosted view's (or none, to append).
        // Elements for accessibility may sit between them.
        private int NextElementId(
            int index)
        {
            for (int i = index + 1; i < this._ownChildren.Count; i += 1)
            {
                if (this._ownChildren[i] is ViewRenderer next)
                {
                    return next.Id;
                }
            }

            return (this._hostedViews.Count > 0) ? this._hostedViews[0].Id : 0;
        }

        protected void InsertChild(
            int index,
            IViewRenderer child)
        {
            this._ownChildren.Insert(index, child);
            if (child is ViewRenderer native)
            {
                native.ParentRenderer?.Detach(native);
                native.ParentRenderer = this;
                Js.InsertBefore(this.Id, native.Id, this.NextElementId(index));
                if (null != this._accessibleElements)
                {
                    // Places the accessible elements around it
                    this.InvalidateRender();
                }
            }
            else
            {
                // A lightweight child draws into this view's canvas
                this.InvalidateRender();
            }

            this.Host.RequestLayout();
        }

        protected void AddChild(
            IViewRenderer child)
        {
            this.InsertChild(this._ownChildren.Count, child);
        }

        protected void RemoveChildAt(
            int index)
        {
            var child = this._ownChildren[index];
            this._ownChildren.RemoveAt(index);
            if (child is ViewRenderer native)
            {
                native.ParentRenderer = null;
                Js.Detach(native.Id);
            }
            else
            {
                this.InvalidateRender();
            }

            this.Host.RequestLayout();
        }

        protected void RemoveChild(
            IViewRenderer child)
        {
            int index = this._ownChildren.IndexOf(child);
            if (index >= 0)
            {
                this.RemoveChildAt(index);
            }
        }

        protected void ClearChildren()
        {
            for (int i = this._ownChildren.Count - 1; i >= 0; i -= 1)
            {
                this.RemoveChildAt(i);
            }
        }

        // Takes a native view out of this one (when it moves elsewhere)
        private void Detach(
            ViewRenderer child)
        {
            if (this._hostedViews.Remove(child))
            {
                child.ParentRenderer = null;
                return;
            }

            int index = this._ownChildren.IndexOf(child);
            if (index >= 0)
            {
                this._ownChildren.RemoveAt(index);
                child.ParentRenderer = null;
            }
        }

        // The native views of lightweight descendants, after the view's own
        // children, in drawing order among themselves
        public void AddHostedView(
            IViewRenderer child,
            int index)
        {
            var renderer = (ViewRenderer)child;
            renderer.ParentRenderer?.Detach(renderer);

            this._hostedViews.Insert(index, renderer);
            renderer.ParentRenderer = this;
            Js.InsertBefore(this.Id, renderer.Id, (index + 1 < this._hostedViews.Count) ? this._hostedViews[index + 1].Id : 0);
        }

        public void RemoveHostedView(
            IViewRenderer child)
        {
            var renderer = (ViewRenderer)child;
            if (this._hostedViews.Remove(renderer))
            {
                renderer.ParentRenderer = null;
                Js.Detach(renderer.Id);
            }
        }

        #endregion Children

        #region Layout

        public virtual Size MeasureNative(
            Size availableSize)
        {
            if (this._isContainer)
            {
                return this.View.MeasureContent(availableSize);
            }

            return Size.Empty;
        }

        public virtual void ArrangeNative(
            Rect frame)
        {
            bool isResized = (frame.Width != this._frame.Width) || (frame.Height != this._frame.Height);
            this._frame = frame;
            Js.SetBounds(this.Id, frame.X, frame.Y, frame.Width, frame.Height);

            if (this._isContainer)
            {
                this.View.LayoutContent(frame.Size);
            }

            if (isResized)
            {
                this._layers?.Update();
                if (this.HasContent || (0 != this._canvasId))
                {
                    this.InvalidateRender();
                }
            }
        }

        public virtual void InvalidateMeasure()
        {
            this.Host.RequestLayout();
        }

        public bool ClipToBounds
        {
            set
            {
                Js.SetStyle(this.Id, "overflow", value ? "hidden" : null);
            }
        }

        #endregion Layout

        #region Properties

        public virtual bool IsVisible
        {
            get
            {
                return this._isVisible;
            }

            set
            {
                if (value != this._isVisible)
                {
                    this._isVisible = value;
                    Js.SetStyle(this.Id, "display", value ? null : "none");
                    this.Host.RequestLayout();
                }
            }
        }

        public bool IsHitTestVisible
        {
            get
            {
                return this._isHitTestVisible;
            }

            set
            {
                this._isHitTestVisible = value;
                Js.SetStyle(this.Id, "pointer-events", value ? null : "none");
            }
        }

        public virtual Color BackgroundColor
        {
            get
            {
                return this._backgroundColor;
            }

            set
            {
                this._backgroundColor = value;
                Js.SetStyle(this.Id, "background-color", value.IsTransparent ? null : Css(value));
            }
        }

        #endregion Properties

        #region Drawing

        /// <summary>
        /// Whether the view draws content of its own (in addition to its
        /// lightweight descendants).
        /// </summary>
        protected virtual bool HasContent
        {
            get
            {
                return false;
            }
        }

        public void InvalidateRender()
        {
            this.Host.RequestRender(this);
        }

        public void InvalidateLayerContent()
        {
            this.InvalidateRender();
        }

        /// <summary>
        /// Draws the content and the lightweight descendants into the canvas.
        /// </summary>
        public void Render()
        {
            var view = this.View;
            bool hasDrawing = this.HasContent || LayerHost.For(view).HasLightweightChildren;
            if (!hasDrawing)
            {
                if (0 != this._canvasId)
                {
                    Js.SetStyle(this._canvasId, "display", "none");
                }

                return;
            }

            if (0 == this._canvasId)
            {
                this._canvasId = Js.Create("canvas");
                Js.SetStyle(this._canvasId, "pointer-events", "none");
                Js.Insert(this.Id, this._canvasId, 0);
            }

            Js.SetStyle(this._canvasId, "display", null);

            var bounds = new Rect(0, 0, this._frame.Width, this._frame.Height);
            var drawContext = new DrawContext(bounds);
            this.DrawContent(drawContext);
            LayerHost.For(view).PaintLightweightViews(drawContext);
            LayerHost.For(view).PaintFocusRing(drawContext);
            drawContext.Flush(this._canvasId);

            foreach (var image in drawContext.PendingImages)
            {
                image.WhenLoaded(this.InvalidateRender);
            }

            this.UpdateAccessibleElements();
        }

        protected virtual void DrawContent(
            DrawContext drawContext)
        {
        }

        #endregion Drawing

        #region Animation and focus

        public IAnimationLayer[] CreateAnimationLayers(
            int count)
        {
            if (null != this._layers)
            {
                throw new InvalidOperationException("The view's animation layers already exist.");
            }

            this._layers = new AnimationLayerStack(this, count);
            return this._layers.Layers;
        }

        private bool _isFocusable;

        // Lets the element take focus (from the keyboard when the view is
        // focusable; programmatically otherwise)
        private void EnsureFocusable()
        {
            if (!this._isFocusable)
            {
                this._isFocusable = true;
                Js.SetAttribute(this.Id, "tabindex", this.View.Focusable ? "0" : "-1");
            }
        }

        public virtual bool Focus()
        {
            this.EnsureFocusable();
            Js.Focus(this.Id);
            return true;
        }

        #endregion Animation and focus

        public override void OnThemeChanged(
            Theme theme)
        {
            base.OnThemeChanged(theme);
            if (this.HasContent)
            {
                this.InvalidateRender();
            }
        }

        #region Accessibility

        /// <summary>
        /// Describes the view to assistive technology with ARIA attributes.
        /// </summary>
        public void ApplyAccessibility()
        {
            ApplyAccessibility(this.Id, this.View, isTextElement: this is TextBlockRenderer);
        }

        private static string Number(
            double value)
        {
            return value.ToString("0.###", CultureInfo.InvariantCulture);
        }

        // ARIA for an element standing for a view: its role, label, and
        // checked, selected or expanded state
        private static void ApplyAccessibility(
            int id,
            View view,
            bool isTextElement)
        {
            var role = view.AccessibilityRole;
            if (LayerHost.For(view).IsHiddenFromAccessibility)
            {
                Js.SetAttribute(id, "aria-hidden", "true");
                return;
            }

            Js.SetAttribute(id, "aria-hidden", null);
            if (role == AccessibilityRole.None)
            {
                return;
            }

            string ariaRole = role switch
            {
                AccessibilityRole.Button => "button",
                AccessibilityRole.ToggleButton => "checkbox",
                AccessibilityRole.RadioButton => "radio",
                AccessibilityRole.TabItem => "tab",
                AccessibilityRole.List => "listbox",
                AccessibilityRole.ListItem => "option",
                AccessibilityRole.Image => "img",
                AccessibilityRole.ComboBox => "combobox",
                AccessibilityRole.Heading => "heading",
                AccessibilityRole.Group => "group",
                AccessibilityRole.CheckBox => "checkbox",
                AccessibilityRole.Switch => "switch",
                AccessibilityRole.Slider => "slider",
                AccessibilityRole.ProgressBar => "progressbar",
                AccessibilityRole.SpinButton => "spinbutton",
                AccessibilityRole.ToolTip => "tooltip",
                AccessibilityRole.Alert => "status",
                AccessibilityRole.Menu => "menu",
                AccessibilityRole.MenuItem => "menuitem",
                AccessibilityRole.Tree => "tree",
                AccessibilityRole.TreeItem => "treeitem",
                _ => null,
            };
            Js.SetAttribute(id, "role", ariaRole);

            // Text elements read their own text
            Js.SetAttribute(id, "aria-label", isTextElement ? null : LayerHost.For(view).AccessibilityText);
            Js.SetAttribute(id, "aria-level", (role == AccessibilityRole.Heading) ? "1" : null);
            Js.SetAttribute(id, "aria-checked", (role == AccessibilityRole.RadioButton) ? (view.AccessibilityIsSelected ? "true" : "false")
                : view.AccessibilityIsMixed ? "mixed"
                : view.AccessibilityIsChecked.HasValue ? (view.AccessibilityIsChecked.Value ? "true" : "false")
                : null);

            // A range's value and bounds (none while a progress bar is
            // indeterminate)
            var range = view.AccessibilityRange;
            Js.SetAttribute(id, "aria-valuemin", range.HasValue ? Number(range.Value.Minimum) : null);
            Js.SetAttribute(id, "aria-valuemax", range.HasValue ? Number(range.Value.Maximum) : null);
            Js.SetAttribute(id, "aria-valuenow", range.HasValue ? Number(range.Value.Value) : null);
            Js.SetAttribute(id, "aria-valuetext", range.HasValue ? view.AccessibilityValue : null);
            Js.SetAttribute(id, "aria-selected", ((role == AccessibilityRole.TabItem) || (role == AccessibilityRole.ListItem)) ? (view.AccessibilityIsSelected ? "true" : "false") : null);
            Js.SetAttribute(id, "aria-expanded", view.AccessibilityIsExpanded.HasValue ? (view.AccessibilityIsExpanded.Value ? "true" : "false") : null);
            Js.SetAttribute(id, "aria-disabled", view.IsHitTestVisible ? null : "true");
        }

        // Elements standing for the accessible lightweight views drawn into
        // this layer: invisible, over each view, with its role and label
        private Dictionary<View, int> _accessibleElements;

        private void UpdateAccessibleElements()
        {
            var views = new List<View>();
            var order = new List<View>();
            CollectAccessible(this.View, views, order);
            if ((0 == views.Count) && (null == this._accessibleElements))
            {
                return;
            }

            this._accessibleElements ??= new Dictionary<View, int>();
            var stale = new HashSet<View>(this._accessibleElements.Keys);
            foreach (var view in views)
            {
                stale.Remove(view);
                if (!this._accessibleElements.TryGetValue(view, out int id))
                {
                    id = Js.Create("div");
                    Js.SetStyle(id, "pointer-events", "none");
                    Js.SetStyle(id, "color", "transparent");
                    Js.SetStyle(id, "overflow", "hidden");
                    Js.ListenClick(id);
                    this.Host.RegisterAccessibleElement(id, view);
                    this._accessibleElements[view] = id;
                }

                var offset = LayerHost.For(view).OffsetInLayer;
                var size = view.Bounds;
                Js.SetBounds(id, offset.X, offset.Y, size.Width, size.Height);
                ApplyAccessibility(id, view, isTextElement: view.AccessibilityRole == AccessibilityRole.Text);
                Js.SetText(id, (view.AccessibilityRole == AccessibilityRole.Text) ? view.AccessibilityLabel ?? string.Empty : string.Empty);
            }

            foreach (var view in stale)
            {
                int id = this._accessibleElements[view];
                this._accessibleElements.Remove(view);
                this.Host.UnregisterAccessibleElement(id);
                Js.Destroy(id);
            }

            // Assistive technology reads the elements in the views' order: the
            // accessible elements go between the native ones (which stay put)
            if (views.Count > 0)
            {
                var ids = new int[order.Count];
                for (int i = 0; i < order.Count; i += 1)
                {
                    var view = order[i];
                    ids[i] = LayerHost.For(view).IsLightweight ? this._accessibleElements[view] : -((ViewRenderer)view.Renderer).Id;
                }

                Js.PlaceAccessible(this.Id, ids);
            }

            this.UpdateActiveDescendant();
        }

        // The browser's focus is on the layer while a lightweight view inside
        // it has keyboard focus: the layer points assistive technology at the
        // view's element (and, having no role of its own, is a group, which
        // can)
        private void UpdateActiveDescendant()
        {
            var focused = LayerHost.For(this.View).FocusedLightweight;
            int element = 0;
            if ((null != focused) && (null != this._accessibleElements))
            {
                for (var view = focused; null != view; view = view.Parent as View)
                {
                    if (this._accessibleElements.TryGetValue(view, out element))
                    {
                        break;
                    }
                }
            }

            Js.SetAttribute(this.Id, "aria-activedescendant", (0 != element) ? "x" + element.ToString(CultureInfo.InvariantCulture) : null);
            if (this.View.AccessibilityRole == AccessibilityRole.None)
            {
                Js.SetAttribute(this.Id, "role", (0 != element) ? "group" : null);
            }
        }

        // The accessible lightweight views inside (an accessible view stands
        // for its own lightweight descendants), and those with the native
        // children, in order
        private static void CollectAccessible(
            View parent,
            List<View> lightweight,
            List<View> order)
        {
            var children = LayerHost.For(parent).Children;
            for (int i = 0; i < children.Count; i += 1)
            {
                var child = children[i];
                if (!LayerHost.For(child).IsLightweight)
                {
                    if (child.Renderer is ViewRenderer)
                    {
                        order.Add(child);
                    }

                    continue;
                }

                if (!child.IsVisible || LayerHost.For(child).IsHiddenFromAccessibility)
                {
                    continue;
                }

                if (child.AccessibilityRole != AccessibilityRole.None)
                {
                    lightweight.Add(child);
                    order.Add(child);
                }
                else
                {
                    CollectAccessible(child, lightweight, order);
                }
            }
        }

        #endregion Accessibility
    }
}
