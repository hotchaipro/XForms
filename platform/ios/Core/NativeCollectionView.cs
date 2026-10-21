using System;
using System.Collections.Specialized;
using CoreGraphics;
using Foundation;
using UIKit;
using XForms.Primitives;

namespace XForms.iOS
{
    /// <summary>
    /// Holds a list's native scroll view (a table or a collection view),
    /// filling itself with it.
    /// </summary>
    internal sealed class NativeListHost : UIView
    {
        public NativeListHost()
        {
            this.BackgroundColor = UIColor.Clear;
        }

        public UIScrollView ScrollView
        {
            get
            {
                return (0 < this.Subviews.Length) ? this.Subviews[0] as UIScrollView : null;
            }

            set
            {
                foreach (var subview in this.Subviews)
                {
                    subview.RemoveFromSuperview();
                }

                if (null != value)
                {
                    value.Frame = this.Bounds;
                    value.AutoresizingMask = UIViewAutoresizing.FlexibleDimensions;
                    this.AddSubview(value);
                }
            }
        }

        public override CGSize SizeThatFits(
            CGSize size)
        {
            // A list takes whatever room it is given
            return size;
        }
    }

    /// <summary>
    /// A horizontal list or a grid: a collection view with a flow layout,
    /// whose cells hold XForms item containers sized by measuring them.
    /// </summary>
    internal sealed class NativeCollectionView : UICollectionView
    {
        public NativeCollectionView(
            ListLayout layout)
            : base(CGRect.Empty, CreateLayout(layout))
        {
            this.BackgroundColor = UIColor.Clear;
            this.RegisterClassForCell(typeof(NativeCollectionViewCell), NativeCollectionViewCell.CellReuseIdentifier);
        }

        private static UICollectionViewFlowLayout CreateLayout(
            ListLayout layout)
        {
            return new UICollectionViewFlowLayout()
            {
                ScrollDirection = (layout.Orientation == Orientation.Horizontal) ? UICollectionViewScrollDirection.Horizontal : UICollectionViewScrollDirection.Vertical,
                MinimumInteritemSpacing = 0,
                MinimumLineSpacing = 0,
            };
        }
    }

    internal sealed class NativeCollectionViewSource : UICollectionViewDelegateFlowLayout, IUICollectionViewDataSource
    {
        private readonly ListView _listView;
        private readonly ListViewSource _items;
        private readonly ListLayout _layout;
        private UICollectionView _collectionView;

        // Binds a spare container to an item to measure it
        private ListViewItem _measuringContainer;

        public NativeCollectionViewSource(
            ListView listView,
            ListViewSource items,
            ListLayout layout)
        {
            this._listView = listView;
            this._items = items;
            this._layout = layout;
            this._items.CollectionChanged += this.Items_CollectionChanged;
        }

        public void Attach(
            UICollectionView collectionView)
        {
            this._collectionView = collectionView;
            collectionView.DataSource = this;
            collectionView.Delegate = this;
        }

        public void Detach()
        {
            this._items.CollectionChanged -= this.Items_CollectionChanged;
            this._collectionView = null;
        }

        private void Items_CollectionChanged(
            object sender,
            NotifyCollectionChangedEventArgs e)
        {
            this._collectionView?.ReloadData();
        }

        public int GetPosition(
            object item)
        {
            return this._items.IndexOf(item);
        }

        public nint GetItemsCount(
            UICollectionView collectionView,
            nint section)
        {
            return this._items.Count;
        }

        public UICollectionViewCell GetCell(
            UICollectionView collectionView,
            NSIndexPath indexPath)
        {
            var cell = (NativeCollectionViewCell)collectionView.DequeueReusableCell(NativeCollectionViewCell.CellReuseIdentifier, indexPath);
            object item = this._items[(int)indexPath.Item];
            cell.ItemContainer = this._items.GetItemContainer(this._listView, cell.ItemContainer, item);
            return cell;
        }

        public override CGSize GetSizeForItem(
            UICollectionView collectionView,
            UICollectionViewLayout layout,
            NSIndexPath indexPath)
        {
            var bounds = collectionView.Bounds;
            object item = this._items[(int)indexPath.Item];
            this._measuringContainer = this._items.GetItemContainer(this._listView, this._measuringContainer, item);

            // A horizontal list's items are as tall as the list; a grid's fill
            // their column
            if (this._layout.Orientation == Orientation.Horizontal)
            {
                float height = (float)bounds.Height;
                this._measuringContainer.Measure(new Size(float.PositiveInfinity, height));
                return new CGSize(this._measuringContainer.DesiredSize.Width, height);
            }

            float width = (float)Math.Floor(bounds.Width / this._layout.Columns);
            this._measuringContainer.Measure(new Size(width, float.PositiveInfinity));
            return new CGSize(width, this._measuringContainer.DesiredSize.Height);
        }

        protected override void Dispose(
            bool disposing)
        {
            if (disposing)
            {
                this._items.CollectionChanged -= this.Items_CollectionChanged;
            }

            base.Dispose(disposing);
        }
    }

    internal sealed class NativeCollectionViewCell : UICollectionViewCell
    {
        public static readonly NSString CellReuseIdentifier = new NSString("XForms.ListViewItem.Collection");

        private ListViewItem _itemContainer;

        [Export("initWithFrame:")]
        public NativeCollectionViewCell(
            CGRect frame)
            : base(frame)
        {
            this.ClipsToBounds = true;
        }

        internal ListViewItem ItemContainer
        {
            get
            {
                return this._itemContainer;
            }

            set
            {
                if (value == this._itemContainer)
                {
                    return;
                }

                ((UIView)this._itemContainer?.Renderer?.NativeElement)?.RemoveFromSuperview();
                this._itemContainer = value;

                var nativeContent = (UIView)value?.Renderer?.NativeElement;
                if (null != nativeContent)
                {
                    this.ContentView.AddSubview(nativeContent);
                }

                this.SetNeedsLayout();
            }
        }

        public override void PrepareForReuse()
        {
            base.PrepareForReuse();
            this._itemContainer?.Recycle();
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();

            var bounds = this.ContentView.Bounds;
            this._itemContainer?.Measure(new Size((float)bounds.Width, (float)bounds.Height));
            this._itemContainer?.Arrange(new Rect(0, 0, (float)bounds.Width, (float)bounds.Height));
        }
    }
}
