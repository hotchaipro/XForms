using System;
using System.Collections.Generic;
using XForms.Platform;

namespace XForms.Primitives
{
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
    /// A scrolling, virtualizing view of items: the platform realizes a
    /// container (an <see cref="ItemContainer"/>) for each item near the
    /// viewport and recycles them as the view scrolls. It lays the items out
    /// in a line or a grid, and can refresh when pulled. Control libraries add
    /// behavior such as selection (the toolkit's <c>ListView</c>).
    /// </summary>
    /// <remarks>
    /// As WinUI's <c>ItemsView</c> (with its <c>ItemContainer</c>), without
    /// selection.
    /// </remarks>
    public class ItemsView : View
    {
        private Thickness _padding;
        private Color? _backgroundColor;
        private Color? _accentColor;
        private ItemsViewSource _source;
        private object _pendingScrollItem;
        private ListLayout _layout = ListLayout.Vertical;
        private bool _isRefreshEnabled;
        private bool _isRefreshing;

        // The containers the platform has made, which it recycles
        private readonly HashSet<ItemContainer> _realizedContainers = new HashSet<ItemContainer>();

        public ItemsView()
        {
            this.HorizontalAlignment = HorizontalAlignment.Stretch;
            this.VerticalAlignment = VerticalAlignment.Stretch;
        }

        // Its content is a layer of its own
        internal override bool RequiresNativeChildren
        {
            get
            {
                return true;
            }
        }

        // The platforms render it natively
        internal override bool IsLightweightType
        {
            get
            {
                return false;
            }
        }

        protected override AccessibilityRole DefaultAccessibilityRole
        {
            get
            {
                return AccessibilityRole.List;
            }
        }

        /// <summary>
        /// Raised when the user pulls the view down past its top (with
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
        /// Whether pulling the view down past its top refreshes it (with touch;
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
        [PlatformContract]
        internal void NotifyRefreshRequested()
        {
            if (!this._isRefreshing)
            {
                this._isRefreshing = true;
                this.OnPropertyChanged(nameof(this.IsRefreshing));
                this.RefreshRequested?.Invoke(this, EventArgs.Empty);
            }
        }

        [PlatformContract]
        internal new IItemsViewRenderer Renderer
        {
            get
            {
                return (IItemsViewRenderer)base.Renderer;
            }
        }

        private IItemsViewRenderer Native
        {
            get
            {
                return (IItemsViewRenderer)this.CreatedRenderer;
            }
        }

        protected override void OnThemeChanged(
            Theme theme)
        {
            base.OnThemeChanged(theme);
            this.ApplyColors();
        }

        private void ApplyColors()
        {
            var native = this.Native;
            if (null != native)
            {
                native.BackgroundColor = this.BackgroundColor;
                native.AccentColor = this.AccentColor;
            }
        }

        internal override void OnRendererCreated()
        {
            base.OnRendererCreated();

            var native = this.Native;
            this.ApplyColors();
            if (!this._padding.Equals(Thickness.Zero))
            {
                native.Padding = this._padding;
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

        /// <summary>
        /// The view's background: by default, the theme's surface color.
        /// </summary>
        public Color BackgroundColor
        {
            get
            {
                return this._backgroundColor ?? this.ActualTheme.Colors.Surface;
            }

            set
            {
                this._backgroundColor = value;
                this.ApplyColors();
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// The refresh indicator's color: by default, the theme's accent color.
        /// </summary>
        public Color AccentColor
        {
            get
            {
                return this._accentColor ?? this.ActualTheme.Colors.Accent;
            }

            set
            {
                this._accentColor = value;
                this.ApplyColors();
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Space around the items, inside the scrolling area: the items start
        /// after the leading padding, and the last can scroll clear of
        /// whatever covers the trailing edge (such as a floating bar).
        /// </summary>
        public Thickness Padding
        {
            get
            {
                return this._padding;
            }

            set
            {
                this._padding = value;
                this.Native?.Padding = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Creates the container for an item's view. Control libraries override
        /// this to add behavior to every item, such as selection or swipe
        /// gestures.
        /// </summary>
        public virtual ItemContainer CreateItemContainer()
        {
            return new ItemContainer();
        }

        public void Bind(
            ItemsViewSource source)
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
        /// The container showing the item, if it's in view, or null.
        /// </summary>
        public ItemContainer ContainerFromItem(
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
        /// The containers the platform has made (for items in view, or set
        /// aside to be reused).
        /// </summary>
        protected IReadOnlyCollection<ItemContainer> RealizedContainers
        {
            get
            {
                return this._realizedContainers;
            }
        }

        /// <summary>
        /// Called when a container starts showing an item (it may be recycled).
        /// </summary>
        protected internal virtual void OnContainerBound(
            ItemContainer container)
        {
        }

        private protected override IElementRenderer CreateRenderer()
        {
            return this.Application.Platform.CreateItemsViewRenderer(this);
        }

        // Hot reload: each item showing gets a new view from the source (its
        // code may have changed), and containers set aside drop theirs, so
        // the source doesn't reuse a view built by the old code
        protected override void OnHotReload()
        {
            base.OnHotReload();
            var source = this._source;
            foreach (var container in this._realizedContainers)
            {
                container.Content = ((null != source) && (null != container.Item)) ? source.GetItemView(null, container.Item) : null;
            }
        }

        internal void AddRealizedContainer(
            ItemContainer container)
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
