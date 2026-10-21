using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using XForms.Platform;

namespace XForms
{
    public abstract class View : Element, INotifyPropertyChanged
    {
        public static readonly ViewProperty<View, bool> IsVisibleProperty = new(nameof(IsVisible), v => v.IsVisible, (v, x) => v.IsVisible = x);
        public static readonly ViewProperty<View, bool> IsHitTestVisibleProperty = new(nameof(IsHitTestVisible), v => v.IsHitTestVisible, (v, x) => v.IsHitTestVisible = x);
        public static readonly ViewProperty<View, float> WidthProperty = new(nameof(Width), v => v.Width, (v, x) => v.Width = x);
        public static readonly ViewProperty<View, float> HeightProperty = new(nameof(Height), v => v.Height, (v, x) => v.Height = x);
        public static readonly ViewProperty<View, Thickness> MarginProperty = new(nameof(Margin), v => v.Margin, (v, x) => v.Margin = x);
        public static readonly ViewProperty<View, string> AccessibilityLabelProperty = new(nameof(AccessibilityLabel), v => v.AccessibilityLabel, (v, x) => v.AccessibilityLabel = x);

        private bool _isVisible = true;
        private bool _isHitTestVisible = true;
        private Animator _animator;
        private List<Animator> _ownAnimators;

        protected View()
        {
        }

        [PlatformContract]
        internal new IViewRenderer Renderer
        {
            get
            {
                return (IViewRenderer)base.Renderer;
            }
        }

        private IViewRenderer Native
        {
            get
            {
                return (IViewRenderer)this.CreatedRenderer;
            }
        }

        public bool IsVisible
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
                    this.Native?.IsVisible = value && this.AreLightweightAncestorsVisible;

                    // A hidden view takes no space, so its parent must lay out again
                    this.InvalidateMeasure();
                }

                this.OnPropertyChanged();
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
                this.Native?.IsHitTestVisible = value;

                this.OnPropertyChanged();
            }
        }

        internal override void OnRendererCreated()
        {
            base.OnRendererCreated();

            var native = this.Native;
            if (!this._isVisible)
            {
                native.IsVisible = false;
            }

            if (!this._isHitTestVisible)
            {
                native.IsHitTestVisible = false;
            }

            if (this._clipToBounds)
            {
                native.ClipToBounds = true;
            }

            this.CreateAnimationLayers(native);

            // A container gets its children and background
            this.AttachVisualChildren();
            this.InvalidateBackground();
        }

        // One native layer per animator: the app's (or the framework's) outside,
        // the control's own inside it
        private void CreateAnimationLayers(
            IViewRenderer native)
        {
            if ((null == this._animator) && this.HasFrameworkAnimator)
            {
                this._animator = new Animator();
                this._animator.Attach(this);
            }

            var animators = new List<Animator>();
            if (null != this._animator)
            {
                animators.Add(this._animator);
            }

            if (null != this._ownAnimators)
            {
                animators.AddRange(this._ownAnimators);
            }

            if (0 == animators.Count)
            {
                return;
            }

            var layers = native.CreateAnimationLayers(animators.Count);
            for (int i = 0; i < animators.Count; i += 1)
            {
                animators[i].Bind(layers[i]);
            }
        }

        /// <summary>
        /// Whether the framework animates the view (pages, for transitions, and
        /// children of panels with layout transitions), so it needs an animator
        /// layer even when the app gives it no animator.
        /// </summary>
        internal virtual bool HasFrameworkAnimator
        {
            get
            {
                return (this.Parent is View parent) && parent.AnimatesChildren;
            }
        }

        #region Property changes and bindings

        private List<Binding> _bindings;

        /// <summary>
        /// Raised when a property of the view changes, whether code or the user
        /// (typing, toggling, choosing) changed it.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(
            [CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Sets a property's field and raises <see cref="PropertyChanged"/> if
        /// the value changed. Returns whether it changed.
        /// </summary>
        protected bool SetProperty<T>(
            ref T field,
            T value,
            [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
            {
                return false;
            }

            field = value;
            this.OnPropertyChanged(propertyName);
            return true;
        }

        // The view keeps its bindings alive; sources refer to them weakly
        internal void AddBinding(
            Binding binding)
        {
            this._bindings ??= new List<Binding>();
            this._bindings.Add(binding);
        }

        internal void RemoveBinding(
            Binding binding)
        {
            this._bindings?.Remove(binding);
        }

        #endregion Property changes and bindings

        #region Attach

        /// <summary>
        /// The view this view was added to, or null.
        /// </summary>
        public Element Parent
        {
            get;
            private set;
        }

        /// <summary>
        /// Raised when the view is added to a parent (or shown), once its native
        /// object exists.
        /// </summary>
        public event EventHandler Attached;

        /// <summary>
        /// Raised when the view is removed from its parent.
        /// </summary>
        public event EventHandler Detached;

        /// <summary>
        /// Attaches the view to its parent: creates its native object if it
        /// doesn't have one yet, and returns it for the parent to add.
        /// </summary>
        internal IViewRenderer AttachTo(
            Element parent)
        {
            this.Parent = parent;
            return this.Renderer;
        }

        /// <summary>
        /// Called by the parent once it has added the view's native object.
        /// </summary>
        internal void NotifyAttached()
        {
            // A view that moved may now inherit a different theme
            this.UpdateTheme();

            // Lightweight views have no native object for their native
            // descendants to go in; their layer hosts them
            this.HostNativeDescendants();
            if (this.IsLightweight)
            {
                this.FindLayer()?.InvalidateLayerContent();
            }

            this.OnAttached();
            this.Attached?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Called by the parent once it has removed the view's native object.
        /// </summary>
        internal void NotifyDetached()
        {
            if (null == this.Parent)
            {
                return;
            }

            if (this.IsLightweight)
            {
                this.FindLayer()?.InvalidateLayerContent();
            }

            this.UnhostNativeDescendants();

            this.Parent = null;
            this.OnDetached();
            this.Detached?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnAttached()
        {
        }

        protected virtual void OnDetached()
        {
        }

        #endregion Attach

        /// <summary>
        /// Reports the size a platform gave a root view it lays out natively.
        /// </summary>
        internal void NotifyRootSize(
            Size size)
        {
            if (!(this.Parent is View))
            {
                this._frame = new Rect(this._frame.X, this._frame.Y, size.Width, size.Height);
                this.OnRootSizeChanged(size);
            }
        }

        // The application's root frame is the window's content
        private void OnRootSizeChanged(
            Size size)
        {
            if ((this is Frame frame) && (null == this.Parent))
            {
                (frame.Window ?? this.Application.MainWindow).OnSizeChanged(size);
            }
        }

        internal override void OnTextScaleChangedCore()
        {
            this.InvalidateMeasure();
            if (this._drawsContent)
            {
                this.InvalidateVisual();
            }
        }

        #region Children

        // The children in the order they were added (layout order), and in
        // drawing order (by ZIndex, then layout order), which the native
        // children follow; null until the first child
        private List<View> _children;
        private List<View> _drawingOrder;

        /// <summary>
        /// The view's children, in the order they were added. A view with
        /// children is a container on every platform: a native view, or a
        /// lightweight one drawn into its layer.
        /// </summary>
        protected IReadOnlyList<View> VisualChildren
        {
            get
            {
                return (IReadOnlyList<View>)this._children ?? Array.Empty<View>();
            }
        }

        private IContainerRenderer Container
        {
            get
            {
                return this.CreatedRenderer as IContainerRenderer;
            }
        }

        /// <summary>
        /// Adds a child after the others.
        /// </summary>
        protected void AddVisualChild(
            View child)
        {
            this.InsertVisualChild(this.VisualChildren.Count, child);
        }

        /// <summary>
        /// Inserts a child at the index, in layout order.
        /// </summary>
        protected void InsertVisualChild(
            int index,
            View child)
        {
            ArgumentNullException.ThrowIfNull(child);
            this._children ??= new List<View>();
            if (this._children.Contains(child))
            {
                throw new InvalidOperationException("The view is already a child of this view.");
            }

            this._children.Insert(index, child);
            this.AddToDrawingOrder(child);
            this.InvalidateMeasure();
        }

        /// <summary>
        /// Removes the child at the index, in layout order.
        /// </summary>
        protected void RemoveVisualChildAt(
            int index)
        {
            var child = this._children[index];
            this.RemoveFromDrawingOrder(child);
            this._children.RemoveAt(index);
            this.InvalidateMeasure();
        }

        /// <summary>
        /// Removes the child. Returns whether it was a child.
        /// </summary>
        protected bool RemoveVisualChild(
            View child)
        {
            int index = this._children?.IndexOf(child) ?? -1;
            if (index < 0)
            {
                return false;
            }

            this.RemoveVisualChildAt(index);
            return true;
        }

        /// <summary>
        /// Replaces the child at the index with another.
        /// </summary>
        protected void ReplaceVisualChild(
            int index,
            View child)
        {
            ArgumentNullException.ThrowIfNull(child);
            var oldChild = this._children[index];
            this.RemoveFromDrawingOrder(oldChild);
            this._children[index] = child;
            this.AddToDrawingOrder(child);
            this.InvalidateMeasure();
        }

        /// <summary>
        /// Removes every child.
        /// </summary>
        protected void ClearVisualChildren()
        {
            if ((null == this._children) || (0 == this._children.Count))
            {
                return;
            }

            var oldChildren = this._children.ToArray();
            this._children.Clear();
            this._drawingOrder.Clear();

            var container = this.Container;
            if (null != container)
            {
                container.ClearChildren();
                foreach (var child in oldChildren)
                {
                    child.NotifyDetached();
                }
            }

            this.InvalidateMeasure();
        }

        // Where the child goes in drawing order: after every sibling with a lower
        // ZIndex, or the same ZIndex and an earlier place in layout order
        private int DrawingInsertIndex(
            View child)
        {
            int zIndex = child.ZIndex;
            int layoutIndex = this._children.IndexOf(child);

            int index = 0;
            foreach (var sibling in this._drawingOrder)
            {
                if ((sibling.ZIndex < zIndex)
                    || ((sibling.ZIndex == zIndex) && (this._children.IndexOf(sibling) < layoutIndex)))
                {
                    index += 1;
                }
            }

            return index;
        }

        private void AddToDrawingOrder(
            View child)
        {
            this._drawingOrder ??= new List<View>();
            int index = this.DrawingInsertIndex(child);
            this._drawingOrder.Insert(index, child);

            var container = this.Container;
            if (null != container)
            {
                container.InsertChild(index, child.AttachTo(this));
                child.NotifyAttached();
            }
        }

        private void RemoveFromDrawingOrder(
            View child)
        {
            int index = this._drawingOrder.IndexOf(child);
            this._drawingOrder.RemoveAt(index);

            var container = this.Container;
            if (null != container)
            {
                container.RemoveChildAt(index);
                child.NotifyDetached();
            }
        }

        internal void OnChildZIndexChanged(
            View child)
        {
            if (null == this._drawingOrder)
            {
                return;
            }

            int oldIndex = this._drawingOrder.IndexOf(child);
            if (oldIndex < 0)
            {
                return;
            }

            this._drawingOrder.RemoveAt(oldIndex);
            int newIndex = this.DrawingInsertIndex(child);
            this._drawingOrder.Insert(newIndex, child);

            var container = this.Container;
            if ((null != container) && (newIndex != oldIndex))
            {
                // Move the native child without detaching the view
                container.RemoveChildAt(oldIndex);
                container.InsertChild(newIndex, child.Renderer);
            }
        }

        // Once the container exists, it gets the children's renderers
        private void AttachVisualChildren()
        {
            var container = this.Container;
            if ((null == container) || (null == this._drawingOrder))
            {
                return;
            }

            for (int i = 0; i < this._drawingOrder.Count; i += 1)
            {
                container.InsertChild(i, this._drawingOrder[i].AttachTo(this));
            }

            foreach (var child in this._children)
            {
                child.NotifyAttached();
            }
        }

        /// <summary>
        /// Whether the framework animates the children's moves (a panel with
        /// layout transitions), so each needs an animator layer.
        /// </summary>
        protected virtual bool AnimatesChildren
        {
            get
            {
                return false;
            }
        }

        #endregion Children

        #region Drawing

        private bool _drawsContent;
        private List<DrawnAnimator> _drawnAnimators;

        internal bool DrawsContentInternal
        {
            get
            {
                return this._drawsContent;
            }
        }

        /// <summary>
        /// Whether the view draws its own content, in <see cref="Render"/>,
        /// beneath its children. Set it in the constructor.
        /// </summary>
        protected bool DrawsContent
        {
            get
            {
                return this._drawsContent;
            }

            set
            {
                if (this.HasRenderer)
                {
                    throw new InvalidOperationException("Set DrawsContent before the view is attached.");
                }

                this._drawsContent = value;
            }
        }

        /// <summary>
        /// The color the view is filled with beneath its children, when it
        /// doesn't draw itself: transparent unless a subclass says otherwise.
        /// Call <see cref="InvalidateBackground"/> when it changes.
        /// </summary>
        protected virtual Color GetBackgroundColor()
        {
            return Colors.Transparent;
        }

        /// <summary>
        /// Applies a change to <see cref="GetBackgroundColor"/> (or draws a view
        /// that draws itself again).
        /// </summary>
        protected void InvalidateBackground()
        {
            var container = this.Container;
            if (null == container)
            {
                return;
            }

            if (this._drawsContent)
            {
                container.BackgroundColor = Colors.Transparent;
                container.Invalidate();
            }
            else
            {
                container.BackgroundColor = this.GetBackgroundColor();
            }
        }

        /// <summary>
        /// Draws the view, within its bounds (from 0, 0), when it
        /// <see cref="DrawsContent"/>. By default it fills the view with its
        /// background color. Call <see cref="InvalidateVisual"/> to draw again.
        /// </summary>
        protected virtual void Render(
            DrawingContext drawingContext)
        {
            drawingContext.Clear(this.GetBackgroundColor());
        }

        /// <summary>
        /// Draws the view again.
        /// </summary>
        public virtual void InvalidateVisual()
        {
            this.Container?.Invalidate();
        }

        /// <summary>
        /// Creates a scale and opacity the view applies to its own drawing,
        /// animated without a native layer (see <see cref="DrawnAnimator"/>).
        /// For small effects, such as press feedback, that shouldn't cost the
        /// view a layer of its own, as an <see cref="Animator"/> would.
        /// </summary>
        protected DrawnAnimator CreateDrawnAnimator()
        {
            var animator = new DrawnAnimator(this.InvalidateVisual);
            this._drawnAnimators ??= new List<DrawnAnimator>();
            this._drawnAnimators.Add(animator);
            return animator;
        }

        /// <summary>
        /// Creates an animator that the view keeps privately, in its own field,
        /// to animate its own content (such as a spinning ring). Call it from
        /// the view's constructor: it throws once the view has been attached.
        /// Each call creates another animator, so a subclass can add its own.
        /// The public <see cref="Animator"/> always belongs to the app.
        /// </summary>
        protected Animator CreateOwnAnimator()
        {
            return this.CreateOwnAnimatorCore();
        }

        // Draws the view's own content (with its drawn animations), for the
        // platform or the lightweight painter
        internal void RenderContent(
            DrawingContext context,
            Rect bounds)
        {
            var animators = this._drawnAnimators;
            if (null == animators)
            {
                this.Render(context);
                return;
            }

            var scopes = new DrawnAnimator.Scope[animators.Count];
            for (int i = 0; i < animators.Count; i += 1)
            {
                scopes[i] = animators[i].Apply(context, bounds);
            }

            try
            {
                this.Render(context);
            }
            finally
            {
                for (int i = animators.Count - 1; i >= 0; i -= 1)
                {
                    scopes[i].Dispose();
                }
            }
        }

        /// <summary>
        /// Creates the view's renderer: by default a container, which holds the
        /// view's children (and draws it, if it <see cref="DrawsContent"/>).
        /// Views the platforms render natively override it.
        /// </summary>
        protected override IElementRenderer CreateRenderer()
        {
            var platform = this.Application.Platform;
            return (platform.UsesLightweightRenderers && this.CanBeLightweight)
                ? new LightweightContainerRenderer(this)
                : platform.CreateContainerRenderer(this);
        }

        protected override void OnThemeChanged(
            Theme theme)
        {
            base.OnThemeChanged(theme);
            this.InvalidateBackground();
        }

        #endregion Drawing

        #region Lightweight views

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
                if (AppContext.TryGetSwitch("XForms.DisableLightweightViews", out bool isDisabled) && isDisabled)
                {
                    return false;
                }

                if (!this.IsLightweightType || (null != this._animator) || (null != this._ownAnimators) || this.HasFrameworkAnimator)
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

                List<View> children = null;
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
        internal IViewRenderer FindLayer()
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
        private IViewRenderer _hostLayer;

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
        internal View HitTestLightweight(
            Point point)
        {
            View hit = null;
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
            }

            return hit;
        }

        #region Lightweight focus

        // On Android and iOS, a lightweight view has no native object to take
        // keyboard focus: its layer's native view takes it, and the layer
        // keeps track of which lightweight view inside it is focused.
        private View _focusedLightweight;
        private View _pendingLightweightFocus;
        private bool _hasNativeFocus;

        /// <summary>
        /// The lightweight view inside this layer that has keyboard focus, or
        /// null.
        /// </summary>
        internal View FocusedLightweight
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
        internal View LayerView
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
            var children = parent.DrawingOrderChildren;
            for (int i = 0; i < children.Count; i += 1)
            {
                var child = children[i];
                if (!child.IsLightweight || !child._isVisible || !child._isHitTestVisible)
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
            View view)
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
        /// Moves keyboard focus from this view to the next (or previous)
        /// focusable view in the tree it is in, wrapping around: lightweight
        /// and native views alike, in view order, as on Win32. (Android and
        /// iOS order native views by position, which puts a layer before the
        /// views inside it.)
        /// </summary>
        internal bool MoveFocus(
            bool isForward)
        {
            var root = this;
            while (root.Parent is View parent)
            {
                root = parent;
            }

            var focusable = new List<View>();
            CollectFocusable(root, focusable);
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
            var children = parent.DrawingOrderChildren;
            for (int i = 0; i < children.Count; i += 1)
            {
                var child = children[i];
                if (!child._isVisible || !child._isHitTestVisible || !child.HasRenderer)
                {
                    continue;
                }

                if (child.Focusable || (child is Primitives.TextBox))
                {
                    result.Add(child);
                }

                CollectFocusable(child, result);
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
            if ((null == focused) || !focused.IsLightweight || (focused.LayerView != this))
            {
                return;
            }

            const float outset = 2.0f;
            var offset = focused.OffsetInLayer;
            var size = focused.Bounds;
            context.DrawRoundedRectangle(
                new Rect(offset.X - outset, offset.Y - outset, size.Width + (outset * 2), size.Height + (outset * 2)),
                4.0f,
                this.ActualTheme.Colors.Foreground,
                2.0f);
        }

        #endregion Lightweight focus

        #endregion Lightweight views

        #region Theme

        private Theme _theme;

        /// <summary>
        /// A theme for this view and its descendants, or null (the default) to
        /// inherit its parent's. Use it for a section that looks different,
        /// such as a dark banner in a light app.
        /// </summary>
        public Theme Theme
        {
            get
            {
                return this._theme;
            }

            set
            {
                this._theme = value;
                this.UpdateTheme();
                this.OnPropertyChanged();
            }
        }

        internal override Theme LocalTheme
        {
            get
            {
                return this._theme;
            }
        }

        internal override Element ParentElement
        {
            get
            {
                return this.Parent;
            }
        }

        #endregion Theme

        #region Animation

        /// <summary>
        /// The app's animator for the view: its translation, scale, rotation and
        /// opacity, set directly or animated. Null (the default) for a view that
        /// never animates. Set it when the view is created, for example
        /// <c>new ProgressRing { Animator = new() }</c>. Controls never set their
        /// own; they use <see cref="Controls.Control.CreateOwnAnimator"/>.
        /// </summary>
        public Animator Animator
        {
            get
            {
                return this._animator;
            }

            init
            {
                if (null != this._animator)
                {
                    throw new InvalidOperationException("The view already has an Animator.");
                }

                if (null != value)
                {
                    value.Attach(this);
                }

                this._animator = value;
            }
        }

        /// <summary>
        /// Creates one of a control's own animators (see
        /// <see cref="Controls.Control.CreateOwnAnimator"/>).
        /// </summary>
        private Animator CreateOwnAnimatorCore()
        {
            if (this.HasRenderer)
            {
                throw new InvalidOperationException("CreateOwnAnimator must be called before the view is attached, from its constructor.");
            }

            var animator = new Animator();
            animator.Attach(this);

            this._ownAnimators ??= new List<Animator>();
            this._ownAnimators.Add(animator);

            return animator;
        }

        /// <summary>
        /// The view's animator, creating one for the framework's own animated
        /// views (pages, popups, scrolled content and list rows).
        /// </summary>
        internal Animator GetOrCreateAnimator()
        {
            if (null == this._animator)
            {
                if (this.HasRenderer)
                {
                    throw new InvalidOperationException("The view was attached without an animator; give it one when it's created, or override HasFrameworkAnimator.");
                }
                var animator = new Animator();
                animator.Attach(this);
                this._animator = animator;
            }

            return this._animator;
        }

        #endregion Animation

        #region Accessibility

        private AccessibilityRole? _accessibilityRole;
        private string _accessibilityLabel;

        /// <summary>
        /// What the view is to assistive technology. Controls provide a default.
        /// </summary>
        public AccessibilityRole AccessibilityRole
        {
            get
            {
                return this._accessibilityRole ?? this.DefaultAccessibilityRole;
            }

            set
            {
                this._accessibilityRole = value;

                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// What assistive technology reads for the view. Controls with text
        /// provide a default.
        /// </summary>
        public string AccessibilityLabel
        {
            get
            {
                return this._accessibilityLabel ?? this.DefaultAccessibilityLabel;
            }

            set
            {
                this._accessibilityLabel = value;

                this.OnPropertyChanged();
            }
        }

        protected virtual AccessibilityRole DefaultAccessibilityRole
        {
            get
            {
                return AccessibilityRole.None;
            }
        }

        protected virtual string DefaultAccessibilityLabel
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// Whether the view is checked, or null when it has no checked state.
        /// </summary>
        public virtual bool? AccessibilityIsChecked
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// Whether the view (a list item, tab or radio button) is selected.
        /// </summary>
        public virtual bool AccessibilityIsSelected
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Whether the view (such as a picker) is expanded, or null when it has
        /// no expanded state.
        /// </summary>
        public virtual bool? AccessibilityIsExpanded
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// Whether assistive technology should skip the view and its
        /// descendants: it or an ancestor is hidden, or scaled or faded to
        /// nothing by an animator.
        /// </summary>
        internal bool IsHiddenFromAccessibility
        {
            get
            {
                for (var view = this; null != view; view = view.Parent as View)
                {
                    if (!view._isVisible || IsAnimatedAway(view._animator))
                    {
                        return true;
                    }

                    if (null != view._ownAnimators)
                    {
                        foreach (var animator in view._ownAnimators)
                        {
                            if (IsAnimatedAway(animator))
                            {
                                return true;
                            }
                        }
                    }
                }

                return false;
            }
        }

        private static bool IsAnimatedAway(
            Animator animator)
        {
            const float Nothing = 0.001f;
            return (null != animator)
                && ((Math.Abs(animator.ScaleX) < Nothing) || (Math.Abs(animator.ScaleY) < Nothing) || (animator.Opacity < Nothing));
        }

        /// <summary>
        /// The value assistive technology reads after the label, such as a
        /// picker's current choice, or null.
        /// </summary>
        public virtual string AccessibilityValue
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// Whether assistive technology can ask the view to do the action.
        /// </summary>
        public virtual bool IsAccessibilityActionSupported(
            AccessibilityAction action)
        {
            switch (action)
            {
                case AccessibilityAction.Invoke:
                    return this.Focusable || (this.AccessibilityRole == AccessibilityRole.ListItem);

                case AccessibilityAction.Toggle:
                    return this.AccessibilityIsChecked.HasValue;

                case AccessibilityAction.Select:
                    return (this.AccessibilityRole == AccessibilityRole.ListItem)
                        || (this.AccessibilityRole == AccessibilityRole.TabItem)
                        || (this.AccessibilityRole == AccessibilityRole.RadioButton);

                case AccessibilityAction.Expand:
                case AccessibilityAction.Collapse:
                    return this.AccessibilityIsExpanded.HasValue;

                default:
                    return false;
            }
        }

        /// <summary>
        /// Does an action for assistive technology. By default, each action
        /// does what a tap would (toggling, selecting); selecting something
        /// already selected does nothing. Returns whether the action was done.
        /// </summary>
        public virtual bool PerformAccessibilityAction(
            AccessibilityAction action)
        {
            if (!this.IsAccessibilityActionSupported(action))
            {
                return false;
            }

            // Already done
            if (((action == AccessibilityAction.Select) && this.AccessibilityIsSelected)
                || ((action == AccessibilityAction.Expand) && (this.AccessibilityIsExpanded == true))
                || ((action == AccessibilityAction.Collapse) && (this.AccessibilityIsExpanded == false)))
            {
                return true;
            }

            this.OnActivate();
            return true;
        }

        /// <summary>
        /// The text a screen reader reads for the view: its label, then its
        /// value. A list item without a label is named by the text inside it.
        /// </summary>
        internal string AccessibilityText
        {
            get
            {
                string label = this.AccessibilityLabel;
                if (string.IsNullOrEmpty(label) && (this.AccessibilityRole == AccessibilityRole.ListItem))
                {
                    var text = new System.Text.StringBuilder();
                    AppendAccessibilityText(this, text);
                    label = text.ToString();
                }

                // A picker may be labeled by its value
                string value = this.AccessibilityValue;
                if (string.IsNullOrEmpty(value) || (value == label))
                {
                    return label;
                }

                return string.IsNullOrEmpty(label) ? value : label + ", " + value;
            }
        }

        private static void AppendAccessibilityText(
            View parent,
            System.Text.StringBuilder text)
        {
            var children = parent.DrawingOrderChildren;
            for (int i = 0; i < children.Count; i += 1)
            {
                var child = children[i];
                if (!child.IsVisible)
                {
                    continue;
                }

                if ((child.AccessibilityRole == AccessibilityRole.Text) && !string.IsNullOrEmpty(child.AccessibilityLabel))
                {
                    if (text.Length > 0)
                    {
                        text.Append(", ");
                    }

                    text.Append(child.AccessibilityLabel);
                }
                else
                {
                    AppendAccessibilityText(child, text);
                }
            }
        }

        /// <summary>
        /// Whether a screen reader's activate gesture does anything.
        /// </summary>
        internal bool IsAccessibilityActionable
        {
            get
            {
                return this.IsAccessibilityActionSupported(AccessibilityAction.Invoke)
                    || this.IsAccessibilityActionSupported(AccessibilityAction.Toggle)
                    || this.IsAccessibilityActionSupported(AccessibilityAction.Select);
            }
        }

        /// <summary>
        /// What a screen reader's activate gesture (a double tap) does: toggles,
        /// selects or invokes.
        /// </summary>
        internal bool PerformDefaultAccessibilityAction()
        {
            if (this.IsAccessibilityActionSupported(AccessibilityAction.Toggle))
            {
                return this.PerformAccessibilityAction(AccessibilityAction.Toggle);
            }

            if (this.IsAccessibilityActionSupported(AccessibilityAction.Select))
            {
                return this.PerformAccessibilityAction(AccessibilityAction.Select);
            }

            return this.PerformAccessibilityAction(AccessibilityAction.Invoke);
        }

        /// <summary>
        /// Raised (for the platforms) when a view's accessible state, value or
        /// label changes.
        /// </summary>
        internal static event Action<View> AccessibilityStateChanged;

        /// <summary>
        /// Tells assistive technology that the view's checked or selected
        /// state, value or label changed.
        /// </summary>
        protected void NotifyAccessibilityStateChanged()
        {
            if (null != this.Renderer)
            {
                AccessibilityStateChanged?.Invoke(this);
            }
        }

        #endregion Accessibility

        #region Layout

        private Size _requestedSize = new Size(Dimension.Auto, Dimension.Auto);
        private Size _minimumSize = Size.Empty;
        private Size _maximumSize = new Size(Dimension.Auto, Dimension.Auto);
        private Thickness _margin = Thickness.Zero;
        private Thickness _padding = Thickness.Zero;
        private HorizontalAlignment _horizontalAlignment = HorizontalAlignment.Stretch;
        private VerticalAlignment _verticalAlignment = VerticalAlignment.Stretch;
        private FlowDirection? _flowDirection;
        private int _zIndex;
        private bool _clipToBounds;
        private Rect _frame;
        private Size _contentMeasureSize = new Size(float.NaN, float.NaN);
        private Size _contentArrangeSize;

        /// <summary>
        /// The width the view asks for, or <see cref="Dimension.Auto"/> (the default) to size to its content.
        /// </summary>
        public float Width
        {
            get
            {
                return this._requestedSize.Width;
            }

            set
            {
                this._requestedSize = new Size(value, this._requestedSize.Height);
                this.InvalidateMeasure();

                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// The height the view asks for, or <see cref="Dimension.Auto"/> (the default) to size to its content.
        /// </summary>
        public float Height
        {
            get
            {
                return this._requestedSize.Height;
            }

            set
            {
                this._requestedSize = new Size(this._requestedSize.Width, value);
                this.InvalidateMeasure();

                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// The least width the view takes (0 by default).
        /// </summary>
        public float MinWidth
        {
            get
            {
                return this._minimumSize.Width;
            }

            set
            {
                this._minimumSize = new Size(value, this._minimumSize.Height);
                this.InvalidateMeasure();

                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// The least height the view takes (0 by default).
        /// </summary>
        public float MinHeight
        {
            get
            {
                return this._minimumSize.Height;
            }

            set
            {
                this._minimumSize = new Size(this._minimumSize.Width, value);
                this.InvalidateMeasure();

                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// The most width the view takes, or <see cref="Dimension.Auto"/> (the default) for no limit.
        /// </summary>
        public float MaxWidth
        {
            get
            {
                return this._maximumSize.Width;
            }

            set
            {
                this._maximumSize = new Size(value, this._maximumSize.Height);
                this.InvalidateMeasure();

                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// The most height the view takes, or <see cref="Dimension.Auto"/> (the default) for no limit.
        /// </summary>
        public float MaxHeight
        {
            get
            {
                return this._maximumSize.Height;
            }

            set
            {
                this._maximumSize = new Size(this._maximumSize.Width, value);
                this.InvalidateMeasure();

                this.OnPropertyChanged();
            }
        }

        public Thickness Margin
        {
            get
            {
                return this._margin;
            }

            set
            {
                this._margin = value;
                this.InvalidateMeasure();

                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Space between the view's edges and its children. Containers expose
        /// it as <c>Padding</c>.
        /// </summary>
        protected Thickness ContentPadding
        {
            get
            {
                return this._padding;
            }

            set
            {
                this._padding = value;
                this.InvalidateMeasure();
            }
        }

        public HorizontalAlignment HorizontalAlignment
        {
            get
            {
                return this._horizontalAlignment;
            }

            set
            {
                this._horizontalAlignment = value;
                this.InvalidateMeasure();

                this.OnPropertyChanged();
            }
        }

        public VerticalAlignment VerticalAlignment
        {
            get
            {
                return this._verticalAlignment;
            }

            set
            {
                this._verticalAlignment = value;
                this.InvalidateMeasure();

                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Whether the view's children are laid out left to right or right to
        /// left. Inherited from the parent unless set.
        /// </summary>
        /// <remarks>
        /// Right to left mirrors the positions of the children; text and
        /// drawing aren't mirrored.
        /// </remarks>
        public FlowDirection FlowDirection
        {
            get
            {
                return this._flowDirection ?? (this.Parent as View)?.FlowDirection ?? FlowDirection.LeftToRight;
            }

            set
            {
                this._flowDirection = value;
                this.InvalidateMeasure();

                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// The view's order among its siblings in a panel: views with a higher
        /// index draw above (and are hit before) views with a lower one. Views
        /// with the same index keep the order they were added in.
        /// </summary>
        public int ZIndex
        {
            get
            {
                return this._zIndex;
            }

            set
            {
                if (value != this._zIndex)
                {
                    this._zIndex = value;
                    (this.Parent as View)?.OnChildZIndexChanged(this);
                }

                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Whether the view clips its children (and drawing) to its bounds.
        /// False by default: children can draw outside their parent.
        /// </summary>
        public bool ClipToBounds
        {
            get
            {
                return this._clipToBounds;
            }

            set
            {
                this._clipToBounds = value;
                this.Native?.ClipToBounds = value;

                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// The size the view asked for in the last measure, including its margin.
        /// </summary>
        public Size DesiredSize
        {
            get;
            private set;
        }

        /// <summary>
        /// The view's position and size from the last arrange, relative to its parent.
        /// </summary>
        public Rect Bounds
        {
            get
            {
                return this._frame;
            }
        }

        /// <summary>
        /// Lays the view out again because its size may have changed: its parent
        /// measures it again, and arranges it.
        /// </summary>
        public void InvalidateMeasure()
        {
            this._contentMeasureSize = new Size(float.NaN, float.NaN);
            this.Native?.InvalidateMeasure();
        }

        /// <summary>
        /// Arranges the view's children again, without changing the view's size.
        /// </summary>
        public void InvalidateArrange()
        {
            this.Native?.InvalidateMeasure();
        }

        /// <summary>
        /// Measures the view within the available size, which may be infinite, and
        /// updates <see cref="DesiredSize"/>. Called by the parent view.
        /// </summary>
        public void Measure(
            Size availableSize)
        {
            if (!this.IsVisible)
            {
                this.DesiredSize = Size.Empty;
                return;
            }

            var margin = this._margin;

            float availableWidth = ConstrainLength(
                Math.Max(0, availableSize.Width - margin.Left - margin.Right),
                this._requestedSize.Width,
                this._minimumSize.Width,
                this._maximumSize.Width);

            float availableHeight = ConstrainLength(
                Math.Max(0, availableSize.Height - margin.Top - margin.Bottom),
                this._requestedSize.Height,
                this._minimumSize.Height,
                this._maximumSize.Height);

            Size measuredSize = this.Renderer.MeasureNative(new Size(availableWidth, availableHeight));

            float width = ConstrainLength(measuredSize.Width, this._requestedSize.Width, this._minimumSize.Width, this._maximumSize.Width);
            float height = ConstrainLength(measuredSize.Height, this._requestedSize.Height, this._minimumSize.Height, this._maximumSize.Height);

            this.DesiredSize = new Size(
                width + margin.Left + margin.Right,
                height + margin.Top + margin.Bottom);
        }

        /// <summary>
        /// Positions the view within the rectangle (relative to the parent view)
        /// according to its margin, alignment and size. Called by the parent view
        /// after <see cref="Measure"/>.
        /// </summary>
        public void Arrange(
            Rect finalRect)
        {
            var margin = this._margin;

            float slotX = finalRect.X + margin.Left;
            float slotY = finalRect.Y + margin.Top;
            float slotWidth = Math.Max(0, finalRect.Width - margin.Left - margin.Right);
            float slotHeight = Math.Max(0, finalRect.Height - margin.Top - margin.Bottom);

            var desiredSize = this.DesiredSize;

            float width = ArrangeLength(
                slotWidth,
                desiredSize.Width - margin.Left - margin.Right,
                (int)this._horizontalAlignment,
                this._requestedSize.Width,
                this._minimumSize.Width,
                this._maximumSize.Width);

            float height = ArrangeLength(
                slotHeight,
                desiredSize.Height - margin.Top - margin.Bottom,
                (int)this._verticalAlignment,
                this._requestedSize.Height,
                this._minimumSize.Height,
                this._maximumSize.Height);

            float x = slotX + AlignmentOffset((int)this._horizontalAlignment, slotWidth, width);
            float y = slotY + AlignmentOffset((int)this._verticalAlignment, slotHeight, height);

            // The parent's padding offsets its children, and a right-to-left
            // parent mirrors them
            if (this.Parent is View parent)
            {
                var padding = parent._padding;
                x += padding.Left;
                y += padding.Top;

                if (parent.FlowDirection == FlowDirection.RightToLeft)
                {
                    x = parent._contentArrangeSize.Width - x - width;
                }
            }

            this._frame = new Rect(x, y, width, height);
            if (!(this.Parent is View))
            {
                this.OnRootSizeChanged(new Size(width, height));
            }

            // A native view inside lightweight ancestors is positioned in their
            // layer
            var nativeFrame = this._frame;
            if ((this.Parent is View host) && host.IsLightweight && !this.IsLightweight)
            {
                var offset = host.OffsetInLayer;
                nativeFrame = new Rect(nativeFrame.X + offset.X, nativeFrame.Y + offset.Y, nativeFrame.Width, nativeFrame.Height);
            }

            this.Renderer.ArrangeNative(nativeFrame);
        }

        /// <summary>
        /// Measures the view's own children within the available size and returns
        /// the size they need, plus the view's padding. Called by native
        /// containers from their measure pass; views without layout children
        /// return their padding.
        /// </summary>
        public Size MeasureContent(
            Size availableSize)
        {
            this._contentMeasureSize = availableSize;

            var padding = this._padding;
            var size = this.MeasureOverride(new Size(
                Math.Max(0, availableSize.Width - padding.Left - padding.Right),
                Math.Max(0, availableSize.Height - padding.Top - padding.Bottom)));

            return new Size(
                size.Width + padding.Left + padding.Right,
                size.Height + padding.Top + padding.Bottom);
        }

        /// <summary>
        /// Positions the view's own children within the given size. Called by
        /// native containers from their layout pass; remeasures the children first
        /// if the final size differs from the size they were measured at.
        /// </summary>
        public void LayoutContent(
            Size size)
        {
            // A root view (a popup's content, say) has no parent to arrange it:
            // its size is its native view's
            if (!(this.Parent is View))
            {
                this._frame = new Rect(this._frame.X, this._frame.Y, size.Width, size.Height);
                this.OnRootSizeChanged(size);
            }

            if (size != this._contentMeasureSize)
            {
                this.MeasureContent(size);
            }

            this.ArrangeContent(size);
        }

        /// <summary>
        /// Positions the view's own children within the given size using their
        /// last measurement. For platforms whose layout system guarantees a measure
        /// pass before every arrange (remeasuring during arrange there would make
        /// the platform lay out again).
        /// </summary>
        public void ArrangeContent(
            Size size)
        {
            this._contentArrangeSize = size;

            var padding = this._padding;
            this.ArrangeOverride(new Size(
                Math.Max(0, size.Width - padding.Left - padding.Right),
                Math.Max(0, size.Height - padding.Top - padding.Bottom)));
        }

        /// <summary>
        /// Measures the children (by calling their <see cref="Measure"/>) and
        /// returns the size the view needs for them, within its padding.
        /// </summary>
        protected virtual Size MeasureOverride(
            Size availableSize)
        {
            // By default the children overlap, each in the whole area
            float width = 0, height = 0;
            foreach (var child in this.VisualChildren)
            {
                child.Measure(availableSize);
                width = Math.Max(width, child.DesiredSize.Width);
                height = Math.Max(height, child.DesiredSize.Height);
            }

            return new Size(width, height);
        }

        /// <summary>
        /// Arranges the children (by calling their <see cref="Arrange"/>) within
        /// the final size, inside the view's padding: a child arranged at (0, 0)
        /// sits at the padding's top left corner.
        /// </summary>
        protected virtual void ArrangeOverride(
            Size finalSize)
        {
            foreach (var child in this.VisualChildren)
            {
                child.Arrange(new Rect(0, 0, finalSize.Width, finalSize.Height));
            }
        }

        // The alignment enums share these values: start (left or top), center,
        // end (right or bottom) and stretch
        private const int Start = 0;
        private const int End = 2;
        private const int Stretch = 3;

        private static float ConstrainLength(
            float length,
            float requestedLength,
            float minimumLength,
            float maximumLength)
        {
            if (!float.IsNaN(requestedLength))
            {
                length = requestedLength;
            }

            if (!float.IsNaN(maximumLength))
            {
                length = Math.Min(length, maximumLength);
            }

            return Math.Max(length, minimumLength);
        }

        private static float ArrangeLength(
            float slotLength,
            float desiredLength,
            int alignment,
            float requestedLength,
            float minimumLength,
            float maximumLength)
        {
            float length;

            if (!float.IsNaN(requestedLength))
            {
                length = requestedLength;
            }
            else if (alignment == Stretch)
            {
                length = slotLength;
            }
            else
            {
                length = Math.Min(desiredLength, slotLength);
            }

            if (!float.IsNaN(maximumLength))
            {
                length = Math.Min(length, maximumLength);
            }

            return Math.Max(Math.Max(0, length), minimumLength);
        }

        private static float AlignmentOffset(
            int alignment,
            float slotLength,
            float length)
        {
            switch (alignment)
            {
                case Start:
                    return 0;

                case End:
                    return slotLength - length;

                default:
                    // Center, or Stretch when the length is limited
                    return (slotLength - length) / 2;
            }
        }

        #endregion Layout

        private GestureRecognizer _gestureRecognizer;

        /// <summary>
        /// The gesture recognizer that interprets the view's pointer input.
        /// </summary>
        public GestureRecognizer GestureRecognizer
        {
            get
            {
                return this._gestureRecognizer;
            }

            set
            {
                if (value == this._gestureRecognizer)
                {
                    return;
                }

                this._gestureRecognizer?.Detach(this);
                this._gestureRecognizer = value;
                value?.Attach(this);
            }
        }

        #region Pointer input

        private HashSet<int> _capturedPointers;
        private TapTracker _tapTracker;
        private EventHandler<TappedEventArgs> _tapped;
        private EventHandler<TappedEventArgs> _doubleTapped;
        private EventHandler<HoldingEventArgs> _holding;

        public event EventHandler<PointerEventArgs> PointerPressed;

        public event EventHandler<PointerEventArgs> PointerMoved;

        public event EventHandler<PointerEventArgs> PointerReleased;

        public event EventHandler<PointerEventArgs> PointerCanceled;

        public event EventHandler<PointerEventArgs> PointerWheelChanged;

        /// <summary>
        /// Raised when a hovering pointer (a mouse or pen, not a touch) moves
        /// over the view or one of its descendants.
        /// </summary>
        public event EventHandler<PointerEventArgs> PointerEntered;

        /// <summary>
        /// Raised when a hovering pointer leaves the view and its descendants,
        /// or the window.
        /// </summary>
        public event EventHandler<PointerEventArgs> PointerExited;

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

        /// <summary>
        /// Raised when a pointer is pressed and released over the view without
        /// moving. Subscribing makes the view take the pointers pressed on it.
        /// </summary>
        public event EventHandler<TappedEventArgs> Tapped
        {
            add
            {
                if (null == this._tapped)
                {
                    this.Activated += this.OnActivatedAsTap;
                }

                this._tapped += value;
                this.EnsureTapTracker();
            }

            remove
            {
                this._tapped -= value;
                if (null == this._tapped)
                {
                    this.Activated -= this.OnActivatedAsTap;
                }
            }
        }

        /// <summary>
        /// Raised for a second tap soon after a first, close to it.
        /// </summary>
        public event EventHandler<TappedEventArgs> DoubleTapped
        {
            add { this._doubleTapped += value; this.EnsureTapTracker(); }
            remove { this._doubleTapped -= value; }
        }

        /// <summary>
        /// Raised when a pointer is held down on the view without moving
        /// (<see cref="HoldingState.Started"/>), and again when it's released
        /// or moves away. A held pointer doesn't also tap.
        /// </summary>
        public event EventHandler<HoldingEventArgs> Holding
        {
            add { this._holding += value; this.EnsureTapTracker(); }
            remove { this._holding -= value; }
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

        private void EnsureTapTracker()
        {
            this._tapTracker ??= new TapTracker(this);
        }

        // Activation (Enter, Space, a screen reader) taps the view's center
        private void OnActivatedAsTap(
            object sender,
            EventArgs e)
        {
            var center = new Point(this.Bounds.Width / 2, this.Bounds.Height / 2);
            this._tapped?.Invoke(this, new TappedEventArgs(center, PointerDeviceType.Mouse));
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

            if (this.Application.FilterPointerInput(this, e))
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

            bool takesPress = false;
            if (null != this._tapTracker)
            {
                takesPress = this._tapTracker.OnPointer(e);
            }

            var result = new GestureEventResult();
            this.GestureRecognizer?.HandlePointerInput(e, ref result);

            bool isCaptured = this.HasPointerCapture(e.PointerId);
            if ((e.State == PointerInputState.Ended) || (e.State == PointerInputState.Canceled))
            {
                this._capturedPointers?.Remove(e.PointerId);
            }

            return e.Handled
                || result.DidHandleEvent
                || isCaptured
                || ((e.State == PointerInputState.Began) && takesPress);
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

        // Recognizes taps, double taps and holds of the view's first pointer
        private sealed class TapTracker
        {
            private static readonly TimeSpan HoldTime = TimeSpan.FromMilliseconds(500);
            private static readonly TimeSpan DoubleTapTime = TimeSpan.FromMilliseconds(500);

            private readonly View _view;
            private DispatcherTimer _holdTimer;
            private int? _pointerId;
            private PointerDeviceType _deviceType;
            private Point _start;
            private bool _isHolding;
            private DateTime _lastTapTime;
            private Point _lastTapPosition;

            public TapTracker(
                View view)
            {
                this._view = view;
            }

            // Returns whether the view takes a press
            public bool OnPointer(
                PointerEventArgs e)
            {
                switch (e.State)
                {
                    case PointerInputState.Began:
                        if (null != this._pointerId)
                        {
                            // A second finger isn't a tap
                            this.Cancel();
                            return true;
                        }

                        this._pointerId = e.PointerId;
                        this._deviceType = e.DeviceType;
                        this._start = e.Position;
                        this._isHolding = false;

                        if (null != this._view._holding)
                        {
                            this._holdTimer ??= new DispatcherTimer(HoldTime, this.OnHoldTime);
                            this._holdTimer.Start();
                        }

                        return true;

                    case PointerInputState.Moved:
                        if ((e.PointerId == this._pointerId) && (e.Position.DistanceTo(this._start) > Application.TouchSlop))
                        {
                            this.Cancel();
                        }

                        break;

                    case PointerInputState.Ended:
                        if (e.PointerId == this._pointerId)
                        {
                            this._holdTimer?.Stop();
                            this._pointerId = null;

                            if (this._isHolding)
                            {
                                this._isHolding = false;
                                this._view._holding?.Invoke(this._view, new HoldingEventArgs(e.Position, HoldingState.Completed));
                            }
                            else if (e.Position.DistanceTo(this._start) <= Application.TouchSlop)
                            {
                                this.OnTap(e.Position);
                            }
                        }

                        break;

                    case PointerInputState.Canceled:
                        if (e.PointerId == this._pointerId)
                        {
                            this.Cancel();
                        }

                        break;
                }

                return false;
            }

            private void OnTap(
                Point position)
            {
                var now = DateTime.UtcNow;
                bool isDouble = ((now - this._lastTapTime) <= DoubleTapTime)
                    && (position.DistanceTo(this._lastTapPosition) <= 2 * Application.TouchSlop);

                var args = new TappedEventArgs(position, this._deviceType);
                this._view._tapped?.Invoke(this._view, args);

                if (isDouble)
                {
                    // A third tap starts over
                    this._lastTapTime = DateTime.MinValue;
                    this._view._doubleTapped?.Invoke(this._view, args);
                }
                else
                {
                    this._lastTapTime = now;
                    this._lastTapPosition = position;
                }
            }

            private void OnHoldTime()
            {
                if (null == this._pointerId)
                {
                    return;
                }

                this._holdTimer.Stop();
                this._isHolding = true;
                this._view._holding?.Invoke(this._view, new HoldingEventArgs(this._start, HoldingState.Started));
            }

            private void Cancel()
            {
                this._holdTimer?.Stop();
                this._pointerId = null;

                if (this._isHolding)
                {
                    this._isHolding = false;
                    this._view._holding?.Invoke(this._view, new HoldingEventArgs(this._start, HoldingState.Canceled));
                }
            }
        }

        /// <summary>
        /// Delivers a pointer event from the platform to this view as a layer:
        /// to the lightweight view under it (or holding it), or to this view.
        /// </summary>
        internal bool DispatchLayerPointerEvent(
            PointerEventArgs e)
        {
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
        private Dictionary<int, View> _pointerOwners;

        // Routes a pointer event from the platform, which delivered it to this
        // layer, to the lightweight view under it: the deepest one, then its
        // ancestors, then this view. The view that takes a press gets the
        // pointer's moves and release.
        private bool DispatchToLightweight(
            PointerEventArgs e)
        {
            View owner = null;
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

        #endregion Pointer input

        #region Keyboard

        private bool? _isKeyboardFocusable;
        private bool _isFocused;

        public event EventHandler GotFocus;

        public event EventHandler LostFocus;

        public event EventHandler<KeyEventArgs> KeyDown;

        public event EventHandler<KeyEventArgs> KeyUp;

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
        /// Delivers a key press to the focused view and then its ancestors, until
        /// one handles it. Enter and Space that nothing handles activate the
        /// focused view. Returns whether the key was used.
        /// </summary>
        internal bool RouteKeyDown(
            KeyEventArgs e)
        {
            for (var view = this; null != view; view = view.Parent as View)
            {
                if (view.DispatchKeyDown(e))
                {
                    return true;
                }
            }

            if ((e.Key == Key.Enter) || (e.Key == Key.Space))
            {
                this.OnActivate();
                return true;
            }

            return false;
        }

        internal bool RouteKeyUp(
            KeyEventArgs e)
        {
            for (var view = this; null != view; view = view.Parent as View)
            {
                if (view.DispatchKeyUp(e))
                {
                    return true;
                }
            }

            return false;
        }

        private EventHandler _activated;

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

        #endregion Keyboard

        internal override IEnumerable<Element> GetChildElements()
        {
            if (null != this._children)
            {
                foreach (var child in this._children)
                {
                    yield return child;
                }
            }
        }
    }
}
