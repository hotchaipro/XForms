using System;
using System.Collections.Generic;

namespace XForms
{
    /// <summary>
    /// Hosts the native views of a layout's children. Positioning is done in
    /// Core by the layout itself; the platform only keeps the native child list.
    /// </summary>
    public interface ILayoutRenderer : IViewRenderer
    {
        void ClearChildren();

        void AddChild(IViewRenderer childRenderer);

        void InsertChild(int index, IViewRenderer childRenderer);

        void RemoveChildAt(int index);

        void ReplaceChild(int index, IViewRenderer childRenderer);
    }

    public abstract class Layout : View
    {
        private List<View> _children;

        protected Layout()
        {
            this._children = new List<View>();
        }

        public new ILayoutRenderer Renderer
        {
            get
            {
                return (ILayoutRenderer)base.Renderer;
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

        internal void InternalClearChildren()
        {
            this._children.Clear();

            this.Renderer.ClearChildren();
            this.InvalidateLayout();
        }

        internal void InternalAddChild(
            View child)
        {
            if (null == child)
            {
                throw new ArgumentNullException(nameof(child));
            }

            this._children.Add(child);

            this.Renderer.AddChild(child.Renderer);
            this.InvalidateLayout();
        }

        internal void InternalInsertChild(
            int index,
            View child)
        {
            if (null == child)
            {
                throw new ArgumentNullException(nameof(child));
            }

            this._children.Insert(index, child);

            this.Renderer.InsertChild(index, child.Renderer);
            this.InvalidateLayout();
        }

        internal void InternalRemoveChildAt(
            int index)
        {
            this._children.RemoveAt(index);

            this.Renderer.RemoveChildAt(index);
            this.InvalidateLayout();
        }

        internal void InternalReplaceChild(
            int index,
            View child)
        {
            if (null == child)
            {
                throw new ArgumentNullException(nameof(child));
            }

            this._children[index] = child;

            this.Renderer.ReplaceChild(index, child.Renderer);
            this.InvalidateLayout();
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
            return this.Application.Platform.CreateLayoutRenderer(this);
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
