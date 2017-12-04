using System;
using System.Collections.Generic;

namespace XForms
{
    public interface ILayoutRenderer : IViewRenderer
    {
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

        internal void InternalClearChildren()
        {
            this._children.Clear();

            this.OnChildrenCleared();
        }

        internal void InternalAddChild(
            View child)
        {
            if (null == child)
            {
                throw new ArgumentNullException(nameof(child));
            }

            this._children.Add(child);

            this.OnChildAdded(child);
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

            this.OnChildInserted(index, child);
        }

        internal void InternalRemoveChildAt(
            int index)
        {
            this._children.RemoveAt(index);

            this.OnChildRemoved(index);
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

            this.OnChildReplaced(index, child);
        }

        protected abstract void OnChildrenCleared();

        protected abstract void OnChildAdded(
            View child);

        protected abstract void OnChildInserted(
            int index,
            View child);

        protected abstract void OnChildRemoved(
            int index);

        protected abstract void OnChildReplaced(
            int index,
            View child);

        internal override IEnumerable<Element> GetChildElements()
        {
            foreach (var child in this._children)
            {
                yield return child;
            }
        }
    }
}
