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
        // Where each child was last arranged, for layout transitions
        private Dictionary<View, Point> _lastPositions;

        protected Panel()
        {
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

        protected override bool AnimatesChildren
        {
            get
            {
                return null != this.ChildTransitions;
            }
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
                return this.VisualChildren;
            }
        }

        /// <summary>
        /// The children in the order they were added.
        /// </summary>
        protected IReadOnlyList<View> LayoutChildren
        {
            get
            {
                return this.VisualChildren;
            }
        }

        internal void InternalClearChildren()
        {
            this.ClearVisualChildren();
        }

        internal void InternalAddChild(
            View child)
        {
            this.AddVisualChild(child);
        }

        internal void InternalInsertChild(
            int index,
            View child)
        {
            this.InsertVisualChild(index, child);
        }

        internal void InternalRemoveChildAt(
            int index)
        {
            this.RemoveVisualChildAt(index);
        }

        internal void InternalReplaceChild(
            int index,
            View child)
        {
            this.ReplaceVisualChild(index, child);
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
            foreach (var child in this.VisualChildren)
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
    }
}
