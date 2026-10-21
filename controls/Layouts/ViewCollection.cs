using System;
using System.Collections;
using System.Collections.Generic;

namespace XForms.Controls
{
    /// <summary>
    /// A panel's children, in the order they were added. A panel creates its
    /// collection in its constructor and exposes it as <c>Children</c>.
    /// </summary>
    public class ViewCollection : IReadOnlyList<View>
    {
        private readonly Panel _owner;

        public ViewCollection(
            Panel owner)
        {
            if (null == owner)
            {
                throw new ArgumentNullException(nameof(owner));
            }

            this._owner = owner;
        }

        public int Count
        {
            get
            {
                return this._owner.ReadOnlyChildren.Count;
            }
        }

        public View this[int index]
        {
            get
            {
                return this._owner.ReadOnlyChildren[index];
            }
        }

        public void Add(
            View child)
        {
            this._owner.InternalAddChild(child);
            this.OnChildAdded(child);
        }

        public void Insert(
            int index,
            View child)
        {
            this._owner.InternalInsertChild(index, child);
            this.OnChildAdded(child);
        }

        public void Replace(
            int index,
            View child)
        {
            var oldChild = this[index];
            this._owner.InternalReplaceChild(index, child);
            this.OnChildRemoved(oldChild);
            this.OnChildAdded(child);
        }

        public void RemoveAt(
            int index)
        {
            var child = this[index];
            this._owner.InternalRemoveChildAt(index);
            this.OnChildRemoved(child);
        }

        public bool Remove(
            View child)
        {
            int index = this.IndexOf(child);
            if (index < 0)
            {
                return false;
            }

            this.RemoveAt(index);
            return true;
        }

        public void Clear()
        {
            var children = new List<View>(this._owner.ReadOnlyChildren);
            this._owner.InternalClearChildren();
            foreach (var child in children)
            {
                this.OnChildRemoved(child);
            }
        }

        public int IndexOf(
            View child)
        {
            var children = this._owner.ReadOnlyChildren;
            for (int i = 0; i < children.Count; i += 1)
            {
                if (children[i] == child)
                {
                    return i;
                }
            }

            return -1;
        }

        public bool Contains(
            View child)
        {
            return (this.IndexOf(child) >= 0);
        }

        private protected virtual void OnChildAdded(
            View child)
        {
        }

        private protected virtual void OnChildRemoved(
            View child)
        {
        }

        public IEnumerator<View> GetEnumerator()
        {
            return this._owner.ReadOnlyChildren.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this._owner.ReadOnlyChildren.GetEnumerator();
        }
    }

    /// <summary>
    /// A panel's children, each with the layout parameters the panel places it
    /// by, such as a <see cref="Layouts.Dock"/> or a grid cell. Adding a child
    /// without them gives it the defaults.
    /// </summary>
    public class ViewCollection<TLayout> : ViewCollection
    {
        private readonly Dictionary<View, TLayout> _layouts = new Dictionary<View, TLayout>();
        private readonly TLayout _defaultLayout;

        public ViewCollection(
            Panel owner,
            TLayout defaultLayout = default)
            : base(owner)
        {
            this._defaultLayout = defaultLayout;
        }

        public void Add(
            View child,
            TLayout layout)
        {
            this.SetLayoutCore(child, layout);
            this.Add(child);
        }

        public void Insert(
            int index,
            View child,
            TLayout layout)
        {
            this.SetLayoutCore(child, layout);
            this.Insert(index, child);
        }

        public void Replace(
            int index,
            View child,
            TLayout layout)
        {
            this.SetLayoutCore(child, layout);
            this.Replace(index, child);
        }

        /// <summary>
        /// The child's layout parameters.
        /// </summary>
        public TLayout GetLayout(
            View child)
        {
            return this._layouts.TryGetValue(child, out var layout) ? layout : this._defaultLayout;
        }

        /// <summary>
        /// Changes a child's layout parameters, and lays the panel out again.
        /// </summary>
        public void SetLayout(
            View child,
            TLayout layout)
        {
            if (!this.Contains(child))
            {
                throw new ArgumentException("The view isn't a child of this panel.", nameof(child));
            }

            this._layouts[child] = layout;
            child.InvalidateMeasure();
        }

        private void SetLayoutCore(
            View child,
            TLayout layout)
        {
            if (null == child)
            {
                throw new ArgumentNullException(nameof(child));
            }

            this._layouts[child] = layout;
        }

        private protected override void OnChildRemoved(
            View child)
        {
            if (!this.Contains(child))
            {
                this._layouts.Remove(child);
            }
        }
    }
}
