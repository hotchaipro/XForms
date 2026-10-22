using System;
using System.Collections.Generic;
using XForms.Primitives;

namespace XForms.Controls
{
    /// <summary>
    /// How tapping a list's items selects them.
    /// </summary>
    public enum SelectionMode
    {
        /// <summary>Items are clicked, not selected.</summary>
        None,

        /// <summary>One item at a time; tapping it again unselects it.</summary>
        Single,

        /// <summary>Tapping an item adds it to the selection or removes it.</summary>
        Multiple,
    }

    public class ItemClickEventArgs : EventArgs
    {
        public ItemClickEventArgs(
            object clickedItem,
            View itemView)
        {
            this.ClickedItem = clickedItem;
            this.ItemView = itemView;
        }

        /// <summary>
        /// The item that was clicked (as WinUI's <c>ItemClickEventArgs</c>).
        /// </summary>
        public object ClickedItem
        {
            get;
        }

        /// <summary>
        /// The view showing the item.
        /// </summary>
        public View ItemView
        {
            get;
        }
    }

    public class SelectionChangedEventArgs : EventArgs
    {
        private static readonly object[] None = Array.Empty<object>();

        public SelectionChangedEventArgs(
            object oldItem,
            object newItem)
        {
            this.OldItem = oldItem;
            this.NewItem = newItem;
            this.RemovedItems = (null == oldItem) ? None : new[] { oldItem };
            this.AddedItems = (null == newItem) ? None : new[] { newItem };
        }

        public SelectionChangedEventArgs(
            IReadOnlyList<object> removedItems,
            IReadOnlyList<object> addedItems,
            object oldItem,
            object newItem)
        {
            this.RemovedItems = removedItems ?? None;
            this.AddedItems = addedItems ?? None;
            this.OldItem = oldItem;
            this.NewItem = newItem;
        }

        /// <summary>
        /// The item selected before (the first, with several), or null.
        /// </summary>
        public object OldItem
        {
            get;
        }

        /// <summary>
        /// The item selected now (the first, with several), or null.
        /// </summary>
        public object NewItem
        {
            get;
        }

        public IReadOnlyList<object> AddedItems
        {
            get;
        }

        public IReadOnlyList<object> RemovedItems
        {
            get;
        }
    }

    /// <summary>
    /// An items view whose items the user taps to click and select them.
    /// </summary>
    public class ListView : ItemsView
    {
        private readonly List<object> _selectedItems = new List<object>();
        private SelectionMode _selectionMode = SelectionMode.Single;

        /// <summary>
        /// Raised when the user taps an item (whatever the selection mode).
        /// </summary>
        public event EventHandler<ItemClickEventArgs> ItemClick;

        public event EventHandler<SelectionChangedEventArgs> SelectionChanged;

        public ListView()
        {
        }

        /// <summary>
        /// Creates the container for an item's view: a
        /// <see cref="ListViewItem"/>. Control libraries override this to add
        /// behavior to every item, such as swipe gestures.
        /// </summary>
        public override ListViewItem CreateItemContainer()
        {
            return new ListViewItem();
        }

        /// <summary>
        /// The container showing the item, if it's in view, or null.
        /// </summary>
        public new ListViewItem ContainerFromItem(
            object item)
        {
            return base.ContainerFromItem(item) as ListViewItem;
        }

        protected override void OnContainerBound(
            ItemContainer container)
        {
            base.OnContainerBound(container);
            if (container is ListViewItem listViewItem)
            {
                listViewItem.IsSelected = this.IsSelected(container.Item);
            }
        }

        /// <summary>
        /// How tapping items selects them: one at a time (the default), several,
        /// or none.
        /// </summary>
        public SelectionMode SelectionMode
        {
            get
            {
                return this._selectionMode;
            }

            set
            {
                this._selectionMode = value;
                if (value == SelectionMode.None)
                {
                    this.ClearSelection();
                }
                else if ((value == SelectionMode.Single) && (1 < this._selectedItems.Count))
                {
                    this.SetSelection(new[] { this._selectedItems[0] });
                }

                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// The selected item (the first, with several), or null.
        /// </summary>
        public object SelectedItem
        {
            get
            {
                return (0 < this._selectedItems.Count) ? this._selectedItems[0] : null;
            }

            set
            {
                this.SetSelection((null == value) ? Array.Empty<object>() : new[] { value });
            }
        }

        /// <summary>
        /// The selected items, in the order they were selected.
        /// </summary>
        public IReadOnlyList<object> SelectedItems
        {
            get
            {
                return this._selectedItems;
            }
        }

        public void ClearSelection()
        {
            this.SetSelection(Array.Empty<object>());
        }

        // Keeps only the selected items that pass the test
        internal void RetainSelected(
            Func<object, bool> keep)
        {
            var items = new List<object>();
            foreach (var item in this._selectedItems)
            {
                if (keep(item))
                {
                    items.Add(item);
                }
            }

            if (items.Count != this._selectedItems.Count)
            {
                this.SetSelection(items);
            }
        }

        internal bool IsSelected(
            object item)
        {
            return (null != item) && this._selectedItems.Contains(item);
        }

        // Replaces the selection, updating the visible containers and raising
        // SelectionChanged if it changed
        private void SetSelection(
            IReadOnlyList<object> items)
        {
            var removed = new List<object>();
            foreach (var item in this._selectedItems)
            {
                if (!Contains(items, item))
                {
                    removed.Add(item);
                }
            }

            var added = new List<object>();
            foreach (var item in items)
            {
                if (!this._selectedItems.Contains(item) && !added.Contains(item))
                {
                    added.Add(item);
                }
            }

            if ((0 == removed.Count) && (0 == added.Count))
            {
                return;
            }

            object oldItem = this.SelectedItem;
            foreach (var item in removed)
            {
                this._selectedItems.Remove(item);
            }

            this._selectedItems.AddRange(added);

            foreach (var container in this.RealizedContainers)
            {
                if (container is ListViewItem listViewItem)
                {
                    listViewItem.IsSelected = this.IsSelected(container.Item);
                }
            }

            this.SelectionChanged?.Invoke(this, new SelectionChangedEventArgs(removed, added, oldItem, this.SelectedItem));
            this.OnPropertyChanged(nameof(this.SelectedItem));
        }

        private static bool Contains(
            IReadOnlyList<object> items,
            object item)
        {
            foreach (var other in items)
            {
                if (Equals(other, item))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Whether a row is something other than an item (such as a group
        /// header), which isn't clicked or selected.
        /// </summary>
        internal virtual bool IsItemRow(
            object row)
        {
            return true;
        }

        /// <summary>
        /// Clicks a container's item and selects it as the selection mode says:
        /// called by a container when it's tapped.
        /// </summary>
        internal void OnItemTapped(
            ListViewItem container)
        {
            object item = container.Item;
            if (!this.IsItemRow(item))
            {
                return;
            }

            this.ItemClick?.Invoke(this, new ItemClickEventArgs(item, container.Content));

            switch (this._selectionMode)
            {
                case SelectionMode.Single:
                    this.SetSelection(this.IsSelected(item) ? Array.Empty<object>() : new[] { item });
                    break;

                case SelectionMode.Multiple:
                {
                    var items = new List<object>(this._selectedItems);
                    if (!items.Remove(item))
                    {
                        items.Add(item);
                    }

                    this.SetSelection(items);
                    break;
                }
            }
        }
    }
}
