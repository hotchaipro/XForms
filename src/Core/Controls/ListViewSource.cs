using System;
using System.Collections;
using System.Collections.Specialized;

namespace XForms.Controls
{
    public abstract class ListViewSource : IEnumerable, INotifyCollectionChanged
    {
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        protected ListViewSource()
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

        public abstract ListViewItem GetItemView(
            View reuseView,
            object item);

        public virtual ListViewItemContainer GetItemContainer(
            ListView listView,
            ListViewItemContainer reuseContainer,
            object item)
        {
            if (null == listView)
            {
                throw new ArgumentNullException(nameof(listView));
            }

            if (null == reuseContainer)
            {
                reuseContainer = new ListViewItemContainer();
            }

            reuseContainer.Bind(listView, item);

            var itemView = this.GetItemView(
                reuseContainer.Content,
                item);
            reuseContainer.Content = itemView;

            itemView.BindingContext = item;

            return reuseContainer;
        }

        public IEnumerator GetEnumerator()
        {
            return new ListViewSourceEnumerator(this);
        }

        protected virtual void NotifyCollectionChanged(
            object sender,
            NotifyCollectionChangedEventArgs e)
        {
            this.CollectionChanged?.Invoke(sender, e);
        }

        private sealed class ListViewSourceEnumerator : IEnumerator
        {
            private ListViewSource _source;
            private int _index;

            internal ListViewSourceEnumerator(
                ListViewSource source)
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
