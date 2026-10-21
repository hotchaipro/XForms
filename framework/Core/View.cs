using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace XForms
{
    internal interface IViewDelegate
    {
        bool DispatchPointerEvent(PointerEventArgs e);

        bool DispatchPointerWheel(PointerEventArgs e);
    }

    public interface IViewRenderer : IElementRenderer
    {
        bool IsVisible { get; set; }

        bool IsHitTestVisible { get; set; }

        /// <summary>
        /// Measures the native view within the available size (which excludes the
        /// view's margin and may be infinite). A native container measures its
        /// children by calling back into <see cref="View.MeasureContent"/>.
        /// </summary>
        Size MeasureNative(Size availableSize);

        /// <summary>
        /// Positions the native view at the frame, relative to its parent view. A
        /// native container arranges its children by calling back into
        /// <see cref="View.LayoutContent"/>.
        /// </summary>
        void ArrangeNative(Rect frame);

        /// <summary>
        /// Asks the platform to lay this view (and its ancestors) out again.
        /// </summary>
        void InvalidateMeasure();

        /// <summary>
        /// Whether the native view clips its children and drawing to its bounds.
        /// </summary>
        bool ClipToBounds { set; }

        /// <summary>
        /// Creates the view's animation layers, one per animator, outermost
        /// first: each wraps the next, and the innermost wraps the view's own
        /// content and children. Called once, when the native view is created,
        /// and only for views with animators.
        /// </summary>
        IAnimationLayer[] CreateAnimationLayers(int count);

        /// <summary>
        /// Adds the native object of a view inside lightweight descendants of
        /// this view, which have none of their own to add it to, at its place
        /// in drawing order among the views it hosts. Hosted views are drawn
        /// above the lightweight content and this view's own children.
        /// </summary>
        void AddHostedView(IViewRenderer child, int index);

        void RemoveHostedView(IViewRenderer child);

        /// <summary>
        /// Draws the lightweight views inside this view again.
        /// </summary>
        void InvalidateLayerContent();

        /// <summary>
        /// Moves keyboard focus to the view. Returns false where the platform
        /// has no keyboard focus for owner-drawn views.
        /// </summary>
        bool Focus();
    }

    public abstract class View : Element, IViewDelegate, INotifyPropertyChanged
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
                return (this.Parent is Panel panel) && (null != panel.ChildTransitions);
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
            if ((this is Frame) && (null == this.Parent))
            {
                this.Application.MainWindow.OnSizeChanged(size);
            }
        }

        internal override void OnTextScaleChangedCore()
        {
            this.InvalidateMeasure();
        }

        #region Lightweight views

        /// <summary>
        /// Whether views of this type can be lightweight: they draw themselves
        /// or only lay out children (owner-drawn and content controls, panels,
        /// text blocks).
        /// </summary>
        internal virtual bool IsLightweightType
        {
            get
            {
                return false;
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
        internal Animator CreateOwnAnimatorCore()
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
        private protected Thickness ContentPadding
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
                    (this.Parent as Panel)?.OnChildZIndexChanged(this);
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
            return Size.Empty;
        }

        /// <summary>
        /// Arranges the children (by calling their <see cref="Arrange"/>) within
        /// the final size, inside the view's padding: a child arranged at (0, 0)
        /// sits at the padding's top left corner.
        /// </summary>
        protected virtual void ArrangeOverride(
            Size finalSize)
        {
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

        public GestureRecognizer GestureRecognizer
        {
            get;
            set;
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
        /// Raised when a pointer is pressed and released over the view without
        /// moving. Subscribing makes the view take the pointers pressed on it.
        /// </summary>
        public event EventHandler<TappedEventArgs> Tapped
        {
            add { this._tapped += value; this.EnsureTapTracker(); }
            remove { this._tapped -= value; }
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

        bool IViewDelegate.DispatchPointerEvent(
            PointerEventArgs e)
        {
            if (this.HasLightweightChildren || (0 < (this._pointerOwners?.Count ?? 0)))
            {
                return this.DispatchToLightweight(e);
            }

            return this.DispatchPointerEvent(e);
        }

        bool IViewDelegate.DispatchPointerWheel(
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
                return this._isKeyboardFocusable
                    ?? ((this.GestureRecognizer is Input.TapGestureRecognizer) || (null != this._tapped));
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

        /// <summary>
        /// Does what a tap would: called for Enter or Space on a focused view,
        /// and for a screen reader's activate command. By default this sends a
        /// press and release to the center of the view.
        /// </summary>
        protected internal virtual void OnActivate()
        {
            var center = new Point(this.Bounds.Width / 2, this.Bounds.Height / 2);
            this.DispatchPointerEvent(new PointerEventArgs(-1, PointerDeviceType.Mouse, center, PointerInputState.Began, true, KeyModifiers.None));
            this.DispatchPointerEvent(new PointerEventArgs(-1, PointerDeviceType.Mouse, center, PointerInputState.Ended, true, KeyModifiers.None));
        }

        #endregion Keyboard

        internal override IEnumerable<Element> GetChildElements()
        {
            yield break;
        }
    }
}
