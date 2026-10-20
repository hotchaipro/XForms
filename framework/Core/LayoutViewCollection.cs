using System;
using System.Collections;
using System.Collections.Generic;

namespace XForms
{
    public class LayoutViewCollection : IEnumerable<View>
    {
        private Layout _parentLayout;

        internal LayoutViewCollection(
            Layout parent)
        {
            if (null == parent)
            {
                throw new ArgumentNullException(nameof(parent));
            }

            this._parentLayout = parent;
        }

        public virtual int Count
        {
            get
            {
                return this._parentLayout.ReadOnlyChildren.Count;
            }
        }

        public virtual View this[int index]
        {
            get
            {
                return this._parentLayout.ReadOnlyChildren[index];
            }
        }

        public virtual void Clear()
        {
            this._parentLayout.InternalClearChildren();
        }

        public virtual void Add(
            View item)
        {
            this._parentLayout.InternalAddChild(item);
        }

        public virtual void Insert(
            int index,
            View item)
        {
            this._parentLayout.InternalInsertChild(index, item);
        }

        public virtual void RemoveAt(
            int index)
        {
            this._parentLayout.InternalRemoveChildAt(index);
        }

        public virtual void Replace(
            int index,
            View item)
        {
            this._parentLayout.InternalReplaceChild(index, item);
        }

        public IEnumerator<View> GetEnumerator()
        {
            return this._parentLayout.ReadOnlyChildren.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this._parentLayout.ReadOnlyChildren.GetEnumerator();
        }
    }
}
