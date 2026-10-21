using System;
using System.Collections;
using System.Collections.Specialized;

namespace XForms.Primitives
{
    public abstract class ItemsViewSource : IEnumerable, INotifyCollectionChanged
    {
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        protected ItemsViewSource()
        {
        }

        public abstract int Count
        {
            get;
        }

        public abstract object this[int index]
        {
            get;
        }

        public abstract int IndexOf(
            object item);

        /// <summary>
        /// The view for an item: the view it reuses (made for an earlier item),
        /// bound to this item, or a new one.
        /// </summary>
        public abstract View GetItemView(
            View reuseView,
            object item);

        public virtual ItemContainer GetItemContainer(
            ItemsView itemsView,
            ItemContainer reuseContainer,
            object item)
        {
            if (null == itemsView)
            {
                throw new ArgumentNullException(nameof(itemsView));
            }

            if (null == reuseContainer)
            {
                reuseContainer = itemsView.CreateItemContainer();
            }

            reuseContainer.Bind(itemsView, item);

            var itemView = this.GetItemView(
                reuseContainer.Content,
                item);
            reuseContainer.Content = itemView;

            return reuseContainer;
        }

        public IEnumerator GetEnumerator()
        {
            return new ItemsViewSourceEnumerator(this);
        }

        protected virtual void NotifyCollectionChanged(
            object sender,
            NotifyCollectionChangedEventArgs e)
        {
            this.CollectionChanged?.Invoke(sender, e);
        }

        private sealed class ItemsViewSourceEnumerator : IEnumerator
        {
            private ItemsViewSource _source;
            private int _index;

            internal ItemsViewSourceEnumerator(
                ItemsViewSource source)
            {
                if (null == source)
                {
                    throw new ArgumentNullException(nameof(source));
                }

                this._source = source;
                this._index = -1;
            }

            public object Current
            {
                get
                {
                    if (this._index < 0)
                    {
                        throw new InvalidOperationException("MoveNext must be called before Current.");
                    }

                    if (this._index >= this._source.Count)
                    {
                        throw new InvalidOperationException("Past the end of the enumeration.");
                    }

                    return this._source[this._index];
                }
            }

            public bool MoveNext()
            {
                this._index += 1;

                if (this._index >= this._source.Count)
                {
                    this._index = this._source.Count;

                    return false;
                }

                return true;
            }

            public void Reset()
            {
                this._index = -1;
            }
        }
    }
}
