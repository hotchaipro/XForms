using System;
using System.Collections.Generic;
using XForms.Platform;

namespace XForms
{
    /// <summary>
    /// Base for layout containers. A panel creates a <see cref="ViewCollection"/>
    /// (or a <see cref="ViewCollection{TLayout}"/>, for children with layout
    /// parameters) as its <c>Children</c>, and positions them in
    /// <see cref="MeasureChildren"/> and <see cref="ArrangeChildren"/>.
    /// </summary>
    public abstract class Panel : View
    {
        // The children in the order they were added (layout order), and in
        // drawing order (by ZIndex, then layout order), which the native
        // children follow
        private readonly List<View> _children;
        private readonly List<View> _nativeOrder;

        // Where each child was last arranged, for layout transitions
        private Dictionary<View, Point> _lastPositions;

        protected Panel()
        {
            this._children = new List<View>();
            this._nativeOrder = new List<View>();
        }

        [PlatformContract]
        internal new IPanelRenderer Renderer
        {
            get
            {
                return (IPanelRenderer)base.Renderer;
            }
        }

        private IPanelRenderer Native
        {
            get
            {
                return (IPanelRenderer)this.CreatedRenderer;
            }
        }

        /// <summary>
        /// How the children animate when the layout changes, or null (the
        /// default) for no animation. Set it before adding children.
        /// </summary>
        public LayoutTransition ChildTransitions
        {
            get;
            set;
        }

        /// <summary>
        /// Space between the panel's edges and its children.
        /// </summary>
        public Thickness Padding
        {
            get
            {
                return this.ContentPadding;
            }

            set
            {
                this.ContentPadding = value;
            }
        }

        internal IReadOnlyList<View> ReadOnlyChildren
        {
            get
            {
                return this._children;
            }
        }

        /// <summary>
        /// The children in the order they were added.
        /// </summary>
        protected IReadOnlyList<View> LayoutChildren
        {
            get
            {
                return this._children;
            }
        }

        internal override void OnRendererCreated()
        {
            base.OnRendererCreated();

            var native = this.Native;
            foreach (var child in this._nativeOrder)
            {
                native.AddChild(child.AttachTo(this));
            }

            foreach (var child in this._children)
            {
                child.NotifyAttached();
            }
        }

        // Where the child goes in drawing order: after every sibling with a lower
        // ZIndex, or the same ZIndex and an earlier place in layout order
        private int NativeInsertIndex(
            View child)
        {
            int zIndex = child.ZIndex;
            int layoutIndex = this._children.IndexOf(child);

            int index = 0;
            foreach (var sibling in this._nativeOrder)
            {
                if ((sibling.ZIndex < zIndex)
                    || ((sibling.ZIndex == zIndex) && (this._children.IndexOf(sibling) < layoutIndex)))
                {
                    index += 1;
                }
            }

            return index;
        }

        private void AddNative(
            View child)
        {
            int index = this.NativeInsertIndex(child);
            this._nativeOrder.Insert(index, child);

            var native = this.Native;
            if (null != native)
            {
                native.InsertChild(index, child.AttachTo(this));
                child.NotifyAttached();
            }
        }

        private void RemoveNative(
            View child)
        {
            int index = this._nativeOrder.IndexOf(child);
            this._nativeOrder.RemoveAt(index);

            var native = this.Native;
            if (null != native)
            {
                native.RemoveChildAt(index);
                child.NotifyDetached();
            }
        }

        internal void OnChildZIndexChanged(
            View child)
        {
            int oldIndex = this._nativeOrder.IndexOf(child);
            this._nativeOrder.RemoveAt(oldIndex);
            int newIndex = this.NativeInsertIndex(child);
            this._nativeOrder.Insert(newIndex, child);

            var native = this.Native;
            if ((null != native) && (newIndex != oldIndex))
            {
                // Move the native child without detaching the view
                native.RemoveChildAt(oldIndex);
                native.InsertChild(newIndex, child.Renderer);
            }
        }

        internal void InternalClearChildren()
        {
            var oldChildren = this._children.ToArray();
            this._children.Clear();
            this._nativeOrder.Clear();

            var native = this.Native;
            if (null != native)
            {
                native.ClearChildren();
                foreach (var child in oldChildren)
                {
                    child.NotifyDetached();
                }
            }

            this.InvalidateMeasure();
        }

        internal void InternalAddChild(
            View child)
        {
            this.InternalInsertChild(this._children.Count, child);
        }

        internal void InternalInsertChild(
            int index,
            View child)
        {
            if (null == child)
            {
                throw new ArgumentNullException(nameof(child));
            }

            if (this._children.Contains(child))
            {
                throw new InvalidOperationException("The view is already a child of this panel.");
            }

            this._children.Insert(index, child);
            this.AddNative(child);

            this.InvalidateMeasure();
        }

        internal void InternalRemoveChildAt(
            int index)
        {
            var child = this._children[index];
            this.RemoveNative(child);
            this._children.RemoveAt(index);

            this.InvalidateMeasure();
        }

        internal void InternalReplaceChild(
            int index,
            View child)
        {
            if (null == child)
            {
                throw new ArgumentNullException(nameof(child));
            }

            var oldChild = this._children[index];
            this.RemoveNative(oldChild);
            this._children[index] = child;
            this.AddNative(child);

            this.InvalidateMeasure();
        }

        protected sealed override Size MeasureOverride(
            Size availableSize)
        {
            // NOTE: A layout asks only for what its children need; a layout set to
            // fill is stretched when it is arranged, not when it is measured
            return this.MeasureChildren(availableSize);
        }

        protected sealed override void ArrangeOverride(
            Size finalSize)
        {
            this.ArrangeChildren(finalSize);

            if (null != this.ChildTransitions)
            {
                this.AnimateLayoutChanges(this.ChildTransitions);
            }
        }

        // Slides moved children from where they were, and fades added ones in
        private void AnimateLayoutChanges(
            LayoutTransition transition)
        {
            var last = this._lastPositions;
            var positions = new Dictionary<View, Point>();
            foreach (var child in this._children)
            {
                var bounds = child.Bounds;
                var position = new Point(bounds.X, bounds.Y);
                positions[child] = position;

                var animator = child.Animator;
                if ((null == animator) || (null == last) || !child.IsVisible)
                {
                    continue;
                }

                if (last.TryGetValue(child, out var previous))
                {
                    if (transition.AnimatesMoves && ((previous.X != position.X) || (previous.Y != position.Y)))
                    {
                        animator.TranslateByOffset(new Point(previous.X - position.X, previous.Y - position.Y), transition.Duration, transition.Easing);
                    }
                }
                else if (transition.AnimatesEntrances)
                {
                    animator.FadeFrom(0, transition.Duration, transition.Easing);
                }
            }

            this._lastPositions = positions;
        }

        /// <summary>
        /// Measures each child and returns the size they need together.
        /// </summary>
        protected abstract Size MeasureChildren(
            Size availableSize);

        /// <summary>
        /// Arranges each child within the layout's final size.
        /// </summary>
        protected abstract void ArrangeChildren(
            Size finalSize);

        protected override IElementRenderer CreateRenderer()
        {
            var platform = this.Application.Platform;
            return (platform.UsesLightweightRenderers && this.CanBeLightweight)
                ? new LightweightPanelRenderer(this)
                : platform.CreatePanelRenderer(this);
        }

        internal override bool IsLightweightType
        {
            get
            {
                return true;
            }
        }

        internal override IReadOnlyList<View> DrawingOrderChildren
        {
            get
            {
                return this._nativeOrder;
            }
        }

        internal override IEnumerable<Element> GetChildElements()
        {
            foreach (var child in this._children)
            {
                yield return child;
            }
        }
    }
}
