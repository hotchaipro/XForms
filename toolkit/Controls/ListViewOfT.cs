using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using XForms.Primitives;

namespace XForms.Controls
{
    /// <summary>
    /// How a list makes item views and shows an item in one. A list creates a
    /// view for each visible row once, and binds it to item after item as it
    /// scrolls, so binding must replace everything the view shows.
    /// </summary>
    public sealed class ItemViewFactory<T>
    {
        private readonly Func<View> _create;
        private readonly Action<View, T> _bind;

        // The views this factory made: only those are reused for it (a group
        // header and an item may be the same kind of view)
        private readonly System.Runtime.CompilerServices.ConditionalWeakTable<View, object> _views = new System.Runtime.CompilerServices.ConditionalWeakTable<View, object>();

        private ItemViewFactory(
            Func<View> create,
            Action<View, T> bind)
        {
            this._create = create;
            this._bind = bind;
        }

        /// <summary>
        /// A factory from a function that creates a view, and one that shows
        /// an item in it.
        /// </summary>
        public static ItemViewFactory<T> Create<TView>(
            Func<TView> create,
            Action<TView, T> bind)
            where TView : View
        {
            if (null == create)
            {
                throw new ArgumentNullException(nameof(create));
            }

            if (null == bind)
            {
                throw new ArgumentNullException(nameof(bind));
            }

            return new ItemViewFactory<T>(() => create(), (view, item) => bind((TView)view, item));
        }

        // The view bound to the item: the reused view if it's this factory's
        // kind, or a new one
        internal View GetView(
            View reuseView,
            T item)
        {
            View view;
            if ((null != reuseView) && this._views.TryGetValue(reuseView, out _))
            {
                view = reuseView;
            }
            else
            {
                view = this._create();
                this._views.Add(view, null);
            }

            this._bind(view, item);
            return view;
        }
    }

    /// <summary>
    /// A virtualized list of typed items. Set <see cref="ItemsSource"/> to the
    /// items (an observable collection updates the list as it changes) and
    /// <see cref="ItemViewFactory"/> to how item views are made and bound.
    /// Consecutive items with the same <see cref="GroupKeySelector"/> key form
    /// a group under a header.
    /// </summary>
    /// <example>
    /// <code>
    /// var list = new ListView&lt;TaskItem&gt;
    /// {
    ///     ItemsSource = tasks,
    ///     ItemViewFactory = ItemViewFactory&lt;TaskItem&gt;.Create(
    ///         () => new TextBlock(),
    ///         (view, task) => view.Text = task.Title),
    /// };
    /// </code>
    /// </example>
    public class ListView<T> : ListView
    {
        private IReadOnlyList<T> _itemsSource;
        private ItemViewFactory<T> _itemViewFactory;
        private Func<T, object> _groupKeySelector;
        private ItemViewFactory<object> _groupHeaderFactory;
        private readonly Source _source;

        public ListView()
        {
            this._source = new Source(this);
            this.Bind(this._source);
        }

        /// <summary>
        /// The items. A collection that raises
        /// <see cref="INotifyCollectionChanged"/> (such as an
        /// <c>ObservableCollection&lt;T&gt;</c>) updates the list as it changes.
        /// </summary>
        public IReadOnlyList<T> ItemsSource
        {
            get
            {
                return this._itemsSource;
            }

            set
            {
                if (this._itemsSource is INotifyCollectionChanged oldSource)
                {
                    oldSource.CollectionChanged -= this.ItemsSource_CollectionChanged;
                }

                this._itemsSource = value;

                if (value is INotifyCollectionChanged newSource)
                {
                    newSource.CollectionChanged += this.ItemsSource_CollectionChanged;
                }

                this.ClearSelection();
                this._source.Rebuild(null);
                this.OnPropertyChanged();
            }
        }

        public ItemViewFactory<T> ItemViewFactory
        {
            get
            {
                return this._itemViewFactory;
            }

            set
            {
                this._itemViewFactory = value;
                this._source.Rebuild(null);
            }
        }

        /// <summary>
        /// The key that groups items, or null (the default) for no groups.
        /// The items should already be in group order: each run of items with
        /// the same key is a group, under a header.
        /// </summary>
        public Func<T, object> GroupKeySelector
        {
            get
            {
                return this._groupKeySelector;
            }

            set
            {
                this._groupKeySelector = value;
                this._source.Rebuild(null);
            }
        }

        /// <summary>
        /// How group header views are made and shown, given a group's key. By
        /// default a header is the key as text.
        /// </summary>
        public ItemViewFactory<object> GroupHeaderFactory
        {
            get
            {
                return this._groupHeaderFactory;
            }

            set
            {
                this._groupHeaderFactory = value;
                this._source.Rebuild(null);
            }
        }

        /// <summary>
        /// The selected item (the first, with several), or the default value.
        /// </summary>
        public new T SelectedItem
        {
            get
            {
                return (base.SelectedItem is T item) ? item : default;
            }

            set
            {
                base.SelectedItem = value;
            }
        }

        /// <summary>
        /// The selected items, in the order they were selected.
        /// </summary>
        public new IReadOnlyList<T> SelectedItems
        {
            get
            {
                var items = new List<T>();
                foreach (var item in base.SelectedItems)
                {
                    items.Add((T)item);
                }

                return items;
            }
        }

        /// <summary>
        /// Shows the item, scrolling it into view.
        /// </summary>
        public void ScrollIntoView(
            T item)
        {
            this.ScrollToItem(item);
        }

        internal override bool IsItemRow(
            object row)
        {
            return !(row is GroupHeader);
        }

        private void ItemsSource_CollectionChanged(
            object sender,
            NotifyCollectionChangedEventArgs e)
        {
            // Drop removed items from the selection
            if ((null != e.OldItems) || (e.Action == NotifyCollectionChangedAction.Reset))
            {
                this.RetainSelected((item) => Contains(this._itemsSource, item));
            }

            this._source.Rebuild(e);
        }

        private static bool Contains(
            IReadOnlyList<T> items,
            object item)
        {
            if (null == items)
            {
                return false;
            }

            foreach (var other in items)
            {
                if (Equals(other, item))
                {
                    return true;
                }
            }

            return false;
        }

        // A group's header row
        private sealed class GroupHeader
        {
            public GroupHeader(
                object key)
            {
                this.Key = key;
            }

            public object Key { get; }

            public override bool Equals(object obj) => (obj is GroupHeader other) && Equals(other.Key, this.Key);

            public override int GetHashCode() => this.Key?.GetHashCode() ?? 0;
        }

        // The rows the platform list shows: the items, with a header before each
        // group when grouping
        private sealed class Source : ItemsViewSource
        {
            private static readonly ItemViewFactory<object> DefaultHeaderFactory = ItemViewFactory<object>.Create(
                () => new TextBlock()
                {
                    FontWeight = FontWeight.SemiBold,
                    Margin = new Thickness(12, 12, 12, 4),
                },
                (view, key) => view.Text = key?.ToString());

            private readonly ListView<T> _list;
            private readonly List<object> _rows = new List<object>();

            public Source(
                ListView<T> list)
            {
                this._list = list;
            }

            public override int Count
            {
                get
                {
                    return this._rows.Count;
                }
            }

            public override object this[int index]
            {
                get
                {
                    return this._rows[index];
                }
            }

            public override int IndexOf(
                object item)
            {
                for (int i = 0; i < this._rows.Count; i += 1)
                {
                    if (Equals(this._rows[i], item))
                    {
                        return i;
                    }
                }

                return -1;
            }

            public override View GetItemView(
                View reuseView,
                object row)
            {
                if (row is GroupHeader header)
                {
                    return (this._list._groupHeaderFactory ?? DefaultHeaderFactory).GetView(reuseView, header.Key);
                }

                var factory = this._list._itemViewFactory
                    ?? throw new InvalidOperationException("Set the list's ItemViewFactory to show its items.");
                return factory.GetView(reuseView, (T)row);
            }

            // Rebuilds the rows from the items. Without groups, a change passes
            // through, so the platform list can animate it; with groups, the
            // list reloads.
            public void Rebuild(
                NotifyCollectionChangedEventArgs change)
            {
                this._rows.Clear();
                var items = this._list._itemsSource;
                var keySelector = this._list._groupKeySelector;
                if (null != items)
                {
                    bool isFirst = true;
                    object lastKey = null;
                    foreach (var item in items)
                    {
                        if (null != keySelector)
                        {
                            object key = keySelector(item);
                            if (isFirst || !Equals(key, lastKey))
                            {
                                this._rows.Add(new GroupHeader(key));
                                lastKey = key;
                                isFirst = false;
                            }
                        }

                        this._rows.Add(item);
                    }
                }

                if ((null != change) && (null == keySelector))
                {
                    this.NotifyCollectionChanged(this, change);
                }
                else
                {
                    this.NotifyCollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                }
            }
        }
    }
}
