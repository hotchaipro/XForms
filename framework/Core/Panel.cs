using System;
using System.Collections.Generic;

namespace XForms
{
    /// <summary>
    /// Hosts the native views of a layout's children. Positioning is done in
    /// Core by the layout itself; the platform only keeps the native child list.
    /// </summary>
    public interface IPanelRenderer : IViewRenderer
    {
        void ClearChildren();

        void AddChild(IViewRenderer childRenderer);

        void InsertChild(int index, IViewRenderer childRenderer);

        void RemoveChildAt(int index);

        void ReplaceChild(int index, IViewRenderer childRenderer);
    }

    public abstract class Panel : View
    {
        private List<View> _children;

        protected Panel()
        {
            this._children = new List<View>();
        }

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
            foreach (var child in this._children)
            {
                native.AddChild(child.AttachTo(this));
            }

            foreach (var child in this._children)
            {
                child.NotifyAttached();
            }
        }

        internal void InternalClearChildren()
        {
            var oldChildren = this._children.ToArray();
            this._children.Clear();

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
            if (null == child)
            {
                throw new ArgumentNullException(nameof(child));
            }

            this._children.Add(child);

            var native = this.Native;
            if (null != native)
            {
                native.AddChild(child.AttachTo(this));
                child.NotifyAttached();
            }

            this.InvalidateMeasure();
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

            var native = this.Native;
            if (null != native)
            {
                native.InsertChild(index, child.AttachTo(this));
                child.NotifyAttached();
            }

            this.InvalidateMeasure();
        }

        internal void InternalRemoveChildAt(
            int index)
        {
            var child = this._children[index];
            this._children.RemoveAt(index);

            var native = this.Native;
            if (null != native)
            {
                native.RemoveChildAt(index);
                child.NotifyDetached();
            }

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
            this._children[index] = child;

            var native = this.Native;
            if (null != native)
            {
                native.ReplaceChild(index, child.AttachTo(this));
                oldChild.NotifyDetached();
                child.NotifyAttached();
            }

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
            return this.Application.Platform.CreatePanelRenderer(this);
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
