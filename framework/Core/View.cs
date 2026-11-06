using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using XForma.Platform;

namespace XForma
{
    public abstract partial class View : Element, INotifyPropertyChanged
    {
        private bool _isVisible = true;
        private bool _isHitTestVisible = true;
        private Animator? _animator;
        private List<Animator>? _ownAnimators;

        protected View()
        {
        }

        [PlatformContract]
        internal new IViewRenderer? Renderer
        {
            get
            {
                return (IViewRenderer)base.Renderer;
            }
        }

        private IViewRenderer? Native
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

                    // A lightweight view isn't drawn under a hidden ancestor
                    // anyway; a native one inside lightweight ones is hidden
                    // with them (and shown again with them)
                    this.Native?.IsVisible = value && (this.IsLightweight || this.AreLightweightAncestorsVisible);

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

            if (null != this._shadow)
            {
                native.Shadow = this._shadow;
            }

            if (Thickness.Zero != this._hitTestOutset)
            {
                native.HitTestOutset = this._hitTestOutset;
            }

            this.CreateAnimationLayers(native);

            // A container gets its children and background
            this.AttachVisualChildren();
            this.InvalidateBackground();
        }

        // One native layer per animator: the app's (or the framework's) outside,
        // the control's own inside it
        private void CreateAnimationLayers(
            IViewRenderer? native)
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

        /// <summary>
        /// Raised when a property of the view changes, whether code or the user
        /// (typing, toggling, choosing) changed it.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(
            [CallerMemberName] string? propertyName = null)
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
            [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
            {
                return false;
            }

            field = value;
            this.OnPropertyChanged(propertyName);
            return true;
        }

        #endregion Property changes and bindings

        #region Attach

        /// <summary>
        /// The view this view was added to, or null.
        /// </summary>
        public Element? Parent
        {
            get;
            internal set;
        }

        /// <summary>
        /// Whether the view is in a window or an open popup: its ancestors
        /// reach a root (a frame or a popup, which have no parent). A view
        /// removed with its parent (a page's old content, after it's built
        /// again) isn't, though it still has the parent it was removed with.
        /// </summary>
        [PlatformContract]
        internal bool IsInVisualTree
        {
            get
            {
                Element top = this;
                while (top is View view && (null != view.Parent))
                {
                    top = view.Parent;
                }

                return (top is Frame) || (top is Primitives.Popup);
            }
        }

        /// <summary>
        /// Raised when the view is added to a parent (or shown), once its native
        /// object exists.
        /// </summary>
        public event EventHandler? Attached;

        /// <summary>
        /// Raised when the view is removed from its parent.
        /// </summary>
        public event EventHandler? Detached;

        /// <summary>
        /// Attaches the view to its parent: creates its native object if it
        /// doesn't have one yet, and returns it for the parent to add.
        /// </summary>
        internal IViewRenderer? AttachTo(
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

        /// <summary>
        /// The view's name, which identifies it among the views its page or
        /// control builds (XAML's <c>x:Name</c>). When a page or control builds
        /// again after a hot reload edit, a new view takes the user state of
        /// the old view with the same name, wherever it moved; an unnamed view
        /// takes that of the old view in the same place, of the same type (see
        /// <see cref="GetUserState"/>).
        /// </summary>
        public string? Name
        {
            get;
            set;
        }

        /// <summary>
        /// What has changed in the view since it was first attached, mostly
        /// through input (text typed, a selection, where it's scrolled), or
        /// null: carried over to the view that replaces it when its page or
        /// control builds again after a hot reload edit (see
        /// <see cref="SetUserState"/>). A control with such state overrides
        /// both.
        /// </summary>
        protected virtual object? GetUserState()
        {
            return null;
        }

        /// <summary>
        /// Takes over the user state of the view this one replaces (see
        /// <see cref="GetUserState"/>), after it's attached. Where the code
        /// and the user disagree, the user's state wins.
        /// </summary>
        protected virtual void SetUserState(
            object state)
        {
        }

        /// <summary>
        /// Records the user state and keyboard focus of views a page or
        /// control is about to replace, as it builds again: call it before
        /// they're removed (removing a focused view takes its focus), and pass
        /// what it returns to <see cref="RestoreUserState"/> once the new views
        /// are attached. A page and a <c>UserControl</c> do both around
        /// <c>Build</c>; a library control that builds views again itself
        /// calls them.
        /// </summary>
        protected static object? SaveUserState(
            View? views)
        {
            return (null != views) ? UserStateNode.Save(views) : null;
        }

        /// <summary>
        /// Gives the new views the user state and keyboard focus that
        /// <see cref="SaveUserState"/> recorded of the views they replace: by
        /// <see cref="Name"/>, then by place and type.
        /// </summary>
        protected static void RestoreUserState(
            object? saved,
            View newViews)
        {
            if (!(saved is UserStateNode root) || (null == newViews))
            {
                return;
            }

            var named = new Dictionary<string, View>(StringComparer.Ordinal);
            CollectNamed(newViews, named);

            View? focus = null;
            Restore(root, newViews, named, ref focus);

            // Once the new views are laid out and in place
            if (null != focus)
            {
                Application.Current?.RequestAnimationFrame((time) => focus.Focus());
            }
        }

        // A view's user state as recorded, with its children's
        private sealed class UserStateNode
        {
            public Type? Type;
            public string? Name;
            public object? State;
            public bool IsFocused;
            public List<UserStateNode> Children = new List<UserStateNode>();

            public static UserStateNode Save(
                View view)
            {
                var node = new UserStateNode()
                {
                    Type = view.GetType(),
                    Name = view.Name,
                    State = view.GetUserState(),
                    IsFocused = view.IsFocused,
                };

                foreach (var child in view.GetUserStateChildren())
                {
                    node.Children.Add(Save(child));
                }

                return node;
            }
        }

        private static void CollectNamed(
            View view,
            Dictionary<string, View> named)
        {
            if (null != view.Name)
            {
                named.TryAdd(view.Name, view);
            }

            foreach (var child in view.GetUserStateChildren())
            {
                CollectNamed(child, named);
            }
        }

        private static void Restore(
            UserStateNode from,
            View to,
            Dictionary<string, View> named,
            ref View? focus)
        {
            if (from.Type != to.GetType())
            {
                return;
            }

            if (null != from.State)
            {
                to.SetUserState(from.State);
            }

            if (from.IsFocused)
            {
                focus = to;
            }

            // Named children by name, wherever they moved
            var oldUnnamed = new List<UserStateNode>();
            foreach (var child in from.Children)
            {
                if (null == child.Name)
                {
                    oldUnnamed.Add(child);
                }
                else if (named.TryGetValue(child.Name, out var match))
                {
                    Restore(child, match, named, ref focus);
                }
            }

            var newUnnamed = new List<View>();
            foreach (var child in to.GetUserStateChildren())
            {
                if (null == child.Name)
                {
                    newUnnamed.Add(child);
                }
            }

            // Unnamed ones by place, as Flutter matches: from the start while
            // their types agree, then from the end, then the rest by type in
            // order (so a view added or removed doesn't shift the others)
            int start = 0;
            while ((start < oldUnnamed.Count) && (start < newUnnamed.Count) && (oldUnnamed[start].Type == newUnnamed[start].GetType()))
            {
                Restore(oldUnnamed[start], newUnnamed[start], named, ref focus);
                start += 1;
            }

            int oldEnd = oldUnnamed.Count - 1;
            int newEnd = newUnnamed.Count - 1;
            while ((oldEnd >= start) && (newEnd >= start) && (oldUnnamed[oldEnd].Type == newUnnamed[newEnd].GetType()))
            {
                Restore(oldUnnamed[oldEnd], newUnnamed[newEnd], named, ref focus);
                oldEnd -= 1;
                newEnd -= 1;
            }

            var unmatched = newUnnamed.GetRange(start, newEnd - start + 1);
            for (int i = start; i <= oldEnd; i += 1)
            {
                int index = unmatched.FindIndex((view) => view.GetType() == oldUnnamed[i].Type);
                if (index >= 0)
                {
                    Restore(oldUnnamed[i], unmatched[index], named, ref focus);
                    unmatched.RemoveAt(index);
                }
            }
        }

        // The children whose user state carries over: a list's rows don't
        // (the list keeps its place and selection itself)
        internal virtual IEnumerable<View> GetUserStateChildren()
        {
            foreach (var child in this.GetChildElements())
            {
                if (child is View view)
                {
                    yield return view;
                }
            }
        }

        /// <summary>
        /// Called after an edit while the app runs with hot reload, on every
        /// view being shown, parents first. A view that built something once
        /// from code (a template's parts, a control's views) runs that code
        /// again here; a page's <see cref="Page.Build"/>, and a list's item
        /// views, run again by themselves.
        /// </summary>
        protected virtual void OnHotReload()
        {
        }

        // Hot reload: the view runs OnHotReload, then so do the children it
        // kept (new ones are built already), and it's measured and drawn again
        internal void ApplyHotReload()
        {
            if (!this.HasRenderer)
            {
                // Built when it's attached
                return;
            }

            var before = new List<Element>(this.GetChildElements());
            this.OnHotReload();

            var after = new HashSet<Element>(this.GetChildElements());
            foreach (var child in before)
            {
                if ((child is View view) && after.Contains(child))
                {
                    view.ApplyHotReload();
                }
            }

            this.InvalidateMeasure();
            this.InvalidateVisual();
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
        private List<View>? _children;
        private List<View>? _drawingOrder;

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

        private IContainerRenderer? Container
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
        private List<DrawnAnimator>? _drawnAnimators;

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
        /// the view's constructor or <see cref="Element.OnAttaching"/>: it
        /// throws once the view's native object exists.
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
            this._drawnFlowDirection = this.FlowDirection;
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
        private protected override IElementRenderer CreateRenderer()
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

        #region Theme

        private Theme? _theme;

        /// <summary>
        /// A theme for this view and its descendants, or null (the default) to
        /// inherit its parent's. Use it for a section that looks different,
        /// such as a dark banner in a light app.
        /// </summary>
        public Theme? Theme
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

        internal override Theme? LocalTheme
        {
            get
            {
                return this._theme;
            }
        }

        internal override Element? ParentElement
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
        public Animator? Animator
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
        /// How far the view's animators move it now.
        /// </summary>
        internal Point AnimatorTranslation
        {
            get
            {
                var translation = this._animator?.Translation ?? Point.Zero;
                if (null != this._ownAnimators)
                {
                    foreach (var animator in this._ownAnimators)
                    {
                        translation = new Point(translation.X + animator.Translation.X, translation.Y + animator.Translation.Y);
                    }
                }

                return translation;
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
                throw new InvalidOperationException("CreateOwnAnimator must be called before the view's native object exists: from its constructor or OnAttaching.");
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

        private GestureRecognizerCollection? _gestureRecognizers;

        /// <summary>
        /// The gesture recognizers that interpret the view's pointer input
        /// (taps, swipes, pans and so on), each given every pointer event in
        /// turn.
        /// </summary>
        public IList<GestureRecognizer> GestureRecognizers
        {
            get
            {
                return this._gestureRecognizers ??= new GestureRecognizerCollection(this);
            }
        }

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
