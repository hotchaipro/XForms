using System;
using Foundation;
using UIKit;
using XForms.Primitives;
using XForms.Platform;

namespace XForms.iOS.Renderers
{
    /// <summary>
    /// A table view for a vertical list, and a collection view for a
    /// horizontal list or a grid, with a refresh control for pull-to-refresh.
    /// </summary>
    internal class ItemsViewRenderer : ViewRenderer<NativeListHost>, IItemsViewRenderer
    {
        private readonly NativeListHost _host;
        private readonly UIRefreshControl _refreshControl = new UIRefreshControl();
        private ListLayout _layout = ListLayout.Vertical;
        private NativeListView _tableView;
        private NativeListViewSource _tableSource;
        private NativeCollectionView _collectionView;
        private NativeCollectionViewSource _collectionSource;
        private ItemsViewSource _source;
        private Thickness _padding;
        private bool _isRefreshEnabled;

        public ItemsViewRenderer(
            ItemsView listView)
            : base(listView)
        {
            this._host = new NativeListHost();
            this._refreshControl.ValueChanged += (sender, e) =>
            {
                if (this._refreshControl.Refreshing)
                {
                    listView.NotifyRefreshRequested();
                }
            };

            this.CreateList();
            this.SetNativeElement(this._host);
        }

        private bool IsVertical
        {
            get
            {
                return (this._layout.Orientation == Orientation.Vertical) && !this._layout.IsGrid;
            }
        }

        private UIScrollView ScrollView
        {
            get
            {
                return (UIScrollView)this._tableView ?? this._collectionView;
            }
        }

        public Thickness Padding
        {
            set
            {
                this._padding = value;
                this.ScrollView.ContentInset = new UIEdgeInsets(value.Top, value.Left, value.Bottom, value.Right);
            }
        }

        public Color AccentColor
        {
            set
            {
                this._refreshControl.TintColor = value.ToUIColor();
            }
        }

        public ListLayout Layout
        {
            set
            {
                if (value != this._layout)
                {
                    this._layout = value;
                    this.CreateList();
                    this.Bind(this._source);
                }
            }
        }

        public bool IsRefreshEnabled
        {
            set
            {
                this._isRefreshEnabled = value;
                this.ScrollView.RefreshControl = value ? this._refreshControl : null;
            }
        }

        public bool IsRefreshing
        {
            set
            {
                if (value)
                {
                    this._refreshControl.BeginRefreshing();
                }
                else
                {
                    this._refreshControl.EndRefreshing();
                }
            }
        }

        // The native list for the layout
        private void CreateList()
        {
            this.Unbind();
            this._tableView = null;
            this._collectionView = null;

            if (this.IsVertical)
            {
                this._tableView = new NativeListView();
            }
            else
            {
                this._collectionView = new NativeCollectionView(this._layout);
            }

            this._host.ScrollView = this.ScrollView;
            this.Padding = this._padding;
            this.IsRefreshEnabled = this._isRefreshEnabled;
        }

        private void Unbind()
        {
            this._tableSource?.Detach();
            this._tableSource?.Dispose();
            this._tableSource = null;
            this._collectionSource?.Detach();
            this._collectionSource?.Dispose();
            this._collectionSource = null;
        }

        public void Bind(
            ItemsViewSource source)
        {
            this.Unbind();
            this._source = source;
            var listView = (ItemsView)this.Element;

            if (null != this._tableView)
            {
                if (null != source)
                {
                    this._tableSource = new NativeListViewSource(listView, source);
                    this._tableSource.Attach(this._tableView);
                }

                this._tableView.Source = this._tableSource;
                this._tableView.ReloadData();
            }
            else
            {
                if (null != source)
                {
                    this._collectionSource = new NativeCollectionViewSource(listView, source, this._layout);
                    this._collectionSource.Attach(this._collectionView);
                }

                this._collectionView.ReloadData();
            }
        }

        public int FirstVisibleIndex
        {
            get
            {
                var paths = (null != this._tableView) ? this._tableView.IndexPathsForVisibleRows : this._collectionView?.IndexPathsForVisibleItems;
                if ((null == paths) || (0 == paths.Length))
                {
                    return 0;
                }

                int first = int.MaxValue;
                foreach (var path in paths)
                {
                    first = Math.Min(first, (int)((null != this._tableView) ? path.Row : path.Item));
                }

                return first;
            }
        }

        public void ScrollToIndex(
            int index)
        {
            if ((index < 0) || (index >= (this._source?.Count ?? 0)))
            {
                return;
            }

            if (null != this._tableView)
            {
                this._tableView.ScrollToRow(NSIndexPath.FromRowSection(index, 0), UITableViewScrollPosition.Top, false);
            }
            else
            {
                this._collectionView?.ScrollToItem(NSIndexPath.FromItemSection(index, 0), UICollectionViewScrollPosition.Top, false);
            }
        }

        public void ScrollToItem(
            object item)
        {
            int position = ((null == item) || (null == this._source)) ? -1 : this._source.IndexOf(item);
            if (position < 0)
            {
                return;
            }

            if (null != this._tableView)
            {
                this._tableView.ScrollToRow(NSIndexPath.FromRowSection(position, 0), UITableViewScrollPosition.None, true);
            }
            else
            {
                this._collectionView.ScrollToItem(NSIndexPath.FromItemSection(position, 0), UICollectionViewScrollPosition.None, true);
            }
        }
    }
}
