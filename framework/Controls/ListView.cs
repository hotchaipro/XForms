using System;
using System.Collections.Generic;
using XForms.Layouts;

namespace XForms.Controls
{
    public interface IListViewRenderer : IElementRenderer
    {
        float FooterHeight { get; set; }

        void Bind(ListViewSource source);

        void ScrollToItem(object item);

        /// <summary>
        /// Lays the items out in a vertical list (the default), a horizontal
        /// one, or a grid.
        /// </summary>
        ListLayout Layout { set; }

        /// <summary>
        /// Whether pulling the list down past its top asks for a refresh. The
        /// platform calls <see cref="ListView.NotifyRefreshRequested"/>.
        /// </summary>
        bool IsRefreshEnabled { set; }

        /// <summary>
        /// Whether the refresh indicator shows.
        /// </summary>
        bool IsRefreshing { set; }
    }

    /// <summary>
    /// How a list lays its items out: in a vertical or horizontal line, or a
    /// grid of columns that scrolls vertically.
    /// </summary>
    public sealed class ListLayout
    {
        private ListLayout(
            Orientation orientation,
            int columns)
        {
            this.Orientation = orientation;
            this.Columns = columns;
        }

        public static ListLayout Vertical { get; } = new ListLayout(Orientation.Vertical, 1);

        public static ListLayout Horizontal { get; } = new ListLayout(Orientation.Horizontal, 1);

        /// <summary>
        /// A grid with the number of items across, each as wide as the list
        /// over the columns.
        /// </summary>
        public static ListLayout Grid(
            int columns)
        {
            if (columns < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(columns));
            }

            return new ListLayout(Orientation.Vertical, columns);
        }

        /// <summary>
        /// The direction the list scrolls.
        /// </summary>
        public Orientation Orientation { get; }

        /// <summary>
        /// The number of items across: 1 for a list.
        /// </summary>
        public int Columns { get; }

        public bool IsGrid
        {
            get
            {
                return this.Columns > 1;
            }
        }
    }

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

    public class ListView : Control
    {
        // Its content is a layer of its own
        internal override bool RequiresNativeChildren
        {
            get
            {
                return true;
            }
        }

        protected override AccessibilityRole DefaultAccessibilityRole
        {
            get
            {
                return AccessibilityRole.List;
            }
        }

        private readonly List<object> _selectedItems = new List<object>();

        /// <summary>
        /// Raised when the user taps an item (whatever the selection mode).
        /// </summary>
        public event EventHandler<ItemClickEventArgs> ItemClick;

        public event EventHandler<SelectionChangedEventArgs> SelectionChanged;

        public ListView()
        {
            this.HorizontalAlignment = HorizontalAlignment.Stretch;
            this.VerticalAlignment = VerticalAlignment.Stretch;
        }

        private float? _footerHeight;
        private ListViewSource _source;
        private object _pendingScrollItem;
        private ListLayout _layout = ListLayout.Vertical;
        private bool _isRefreshEnabled;
        private bool _isRefreshing;

        /// <summary>
        /// Raised when the user pulls the list down past its top (with
        /// <see cref="IsRefreshEnabled"/> set). <see cref="IsRefreshing"/> is
        /// true until the app sets it back to false when it's done.
        /// </summary>
        public event EventHandler RefreshRequested;

        /// <summary>
        /// How the items are laid out: a vertical list (the default), a
        /// horizontal one, or a grid.
        /// </summary>
        public ListLayout Layout
        {
            get
            {
                return this._layout;
            }

            set
            {
                this._layout = value ?? throw new ArgumentNullException(nameof(value));
                this.Native?.Layout = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Whether pulling the list down past its top refreshes it (with touch;
        /// apps can also call <see cref="RequestRefresh"/>).
        /// </summary>
        public bool IsRefreshEnabled
        {
            get
            {
                return this._isRefreshEnabled;
            }

            set
            {
                this._isRefreshEnabled = value;
                this.Native?.IsRefreshEnabled = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Whether a refresh is under way, which shows the refresh indicator.
        /// </summary>
        public bool IsRefreshing
        {
            get
            {
                return this._isRefreshing;
            }

            set
            {
                if (value != this._isRefreshing)
                {
                    this._isRefreshing = value;
                    this.Native?.IsRefreshing = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Refreshes as a pull would: shows the indicator and raises
        /// <see cref="RefreshRequested"/>.
        /// </summary>
        public void RequestRefresh()
        {
            if (!this._isRefreshing)
            {
                this.IsRefreshing = true;
                this.RefreshRequested?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Called by the platform when the user pulls to refresh (its indicator
        /// already shows).
        /// </summary>
        internal void NotifyRefreshRequested()
        {
            if (!this._isRefreshing)
            {
                this._isRefreshing = true;
                this.OnPropertyChanged(nameof(this.IsRefreshing));
                this.RefreshRequested?.Invoke(this, EventArgs.Empty);
            }
        }

        internal new IListViewRenderer Renderer
        {
            get
            {
                return (IListViewRenderer)base.Renderer;
            }
        }

        private IListViewRenderer Native
        {
            get
            {
                return (IListViewRenderer)this.CreatedRenderer;
            }
        }

        internal override void OnRendererCreated()
        {
            base.OnRendererCreated();

            var native = this.Native;
            if (this._footerHeight.HasValue)
            {
                native.FooterHeight = this._footerHeight.Value;
            }

            if (this._layout != ListLayout.Vertical)
            {
                native.Layout = this._layout;
            }

            if (this._isRefreshEnabled)
            {
                native.IsRefreshEnabled = true;
            }

            if (this._isRefreshing)
            {
                native.IsRefreshing = true;
            }

            if (null != this._source)
            {
                native.Bind(this._source);
            }

            if (null != this._pendingScrollItem)
            {
                native.ScrollToItem(this._pendingScrollItem);
                this._pendingScrollItem = null;
            }
        }

        public override Color BackgroundColor
        {
            get
            {
                return this.ActualTheme.Colors.Surface;
            }
        }

        public float FooterHeight
        {
            get
            {
                return this._footerHeight ?? this.Native?.FooterHeight ?? 0;
            }

            set
            {
                this._footerHeight = value;
                this.Native?.FooterHeight = value;
            }
        }

        /// <summary>
        /// Creates the container for an item's view. Control libraries override
        /// this to add behavior to every item, such as swipe gestures.
        /// </summary>
        public virtual ListViewItem CreateItemContainer()
        {
            return new ListViewItem();
        }

        public void Bind(
            ListViewSource source)
        {
            this._source = source;
            this.Native?.Bind(source);
        }

        public void ScrollToItem(
            object item)
        {
            var native = this.Native;
            if (null != native)
            {
                native.ScrollToItem(item);
            }
            else
            {
                this._pendingScrollItem = item;
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

        private SelectionMode _selectionMode = SelectionMode.Single;

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

            foreach (var container in this._realizedContainers)
            {
                container.IsSelected = this.IsSelected(container.Item);
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
        /// The container showing the item, if it's in view, or null.
        /// </summary>
        public ListViewItem ContainerFromItem(
            object item)
        {
            if (null == item)
            {
                return null;
            }

            foreach (var container in this._realizedContainers)
            {
                if (Equals(container.Item, item))
                {
                    return container;
                }
            }

            return null;
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

        protected override IElementRenderer CreateRenderer()
        {
            return this.Application.Platform.CreateListViewRenderer(this);
        }

        // The rows the platform has made, which it recycles
        private readonly HashSet<ListViewItem> _realizedContainers = new HashSet<ListViewItem>();

        internal void AddRealizedContainer(
            ListViewItem container)
        {
            if (this._realizedContainers.Add(container))
            {
                container.UpdateTheme();
            }
        }

        internal override IEnumerable<Element> GetChildElements()
        {
            return this._realizedContainers;
        }
    }
}
