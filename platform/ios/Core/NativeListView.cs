using System;
using System.Collections.Specialized;
using CoreGraphics;
using Foundation;
using UIKit;
using XForms.Controls;

namespace XForms.iOS
{
    /// <summary>
    /// A UITableView whose rows are XForms list view item containers. Rows are
    /// recycled by UIKit and rebound through the <see cref="ListViewSource"/>, and
    /// each row's height comes from measuring its XForms content.
    /// </summary>
    internal sealed class NativeListView : UITableView
    {
        private bool _isRowHeightUpdatePending;
        private readonly System.Collections.Generic.List<Action> _rowHeightUpdateCompletions = new System.Collections.Generic.List<Action>();

        public NativeListView()
            : base(CGRect.Empty, UITableViewStyle.Plain)
        {
            this.SeparatorStyle = UITableViewCellSeparatorStyle.None;
            this.AllowsSelection = false;
            this.BackgroundColor = UIColor.Clear;
            this.RowHeight = UITableView.AutomaticDimension;
            this.EstimatedRowHeight = 60;
            this.KeyboardDismissMode = UIScrollViewKeyboardDismissMode.OnDrag;
        }

        public override CGSize SizeThatFits(
            CGSize size)
        {
            // A list view takes whatever room it is given
            return size;
        }

        /// <summary>
        /// Lets the table pick up rows whose content changed height, such as an
        /// item that revealed its command bar.
        /// </summary>
        internal void InvalidateRowHeights(
            Action completion = null)
        {
            if (null != completion)
            {
                this._rowHeightUpdateCompletions.Add(completion);
            }

            if (this._isRowHeightUpdatePending)
            {
                return;
            }

            this._isRowHeightUpdatePending = true;

            this.BeginInvokeOnMainThread(() =>
            {
                this._isRowHeightUpdatePending = false;

                var completions = this._rowHeightUpdateCompletions.ToArray();
                this._rowHeightUpdateCompletions.Clear();

                // An empty batch update re-queries row heights and animates the change
                this.PerformBatchUpdates(
                    null,
                    (finished) =>
                    {
                        foreach (var action in completions)
                        {
                            action();
                        }
                    });
            });
        }
    }

    internal sealed class NativeListViewSource : UITableViewSource
    {
        private readonly ListView _listView;
        private readonly ListViewSource _items;
        private NativeListView _tableView;

        public NativeListViewSource(
            ListView listView,
            ListViewSource items)
        {
            if (null == listView)
            {
                throw new ArgumentNullException(nameof(listView));
            }

            if (null == items)
            {
                throw new ArgumentNullException(nameof(items));
            }

            this._listView = listView;
            this._items = items;

            this._items.CollectionChanged += this.Items_CollectionChanged;
        }

        internal void Attach(
            NativeListView tableView)
        {
            this._tableView = tableView;
        }

        internal void Detach()
        {
            this._items.CollectionChanged -= this.Items_CollectionChanged;
            this._tableView = null;
        }

        public int GetPosition(
            object item)
        {
            return this._items.IndexOf(item);
        }

        public override nint RowsInSection(
            UITableView tableView,
            nint section)
        {
            return this._items.Count;
        }

        public override UITableViewCell GetCell(
            UITableView tableView,
            NSIndexPath indexPath)
        {
            var cell = tableView.DequeueReusableCell(NativeListViewCell.CellReuseIdentifier) as NativeListViewCell;
            if (null == cell)
            {
                cell = new NativeListViewCell();
            }

            object item = this._items[indexPath.Row];

            // Rebind the recycled container (or create one) for this item
            cell.ItemContainer = this._items.GetItemContainer(this._listView, cell.ItemContainer, item);

            return cell;
        }

        private void Items_CollectionChanged(
            object sender,
            NotifyCollectionChangedEventArgs e)
        {
            this._tableView?.ReloadData();
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

    internal sealed class NativeListViewCell : UITableViewCell
    {
        public static readonly NSString CellReuseIdentifier = new NSString("XForms.ListViewItem");

        private ListViewItemContainer _itemContainer;
        private UIView _closingSnapshot;

        public NativeListViewCell()
            : base(UITableViewCellStyle.Default, CellReuseIdentifier)
        {
            this.BackgroundColor = UIColor.Clear;
            this.SelectionStyle = UITableViewCellSelectionStyle.None;
            this.ClipsToBounds = true;
        }

        internal ListViewItemContainer ItemContainer
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

                this.NativeContent?.RemoveFromSuperview();

                this._itemContainer = value;

                var nativeContent = this.NativeContent;
                if (null != nativeContent)
                {
                    this.ContentView.AddSubview(nativeContent);
                }

                this.SetNeedsLayout();
            }
        }

        private UIView NativeContent
        {
            get
            {
                return (UIView)this._itemContainer?.Renderer?.NativeElement;
            }
        }

        private void RemoveClosingSnapshot()
        {
            this._closingSnapshot?.RemoveFromSuperview();
            this._closingSnapshot = null;
        }

        public override void PrepareForReuse()
        {
            base.PrepareForReuse();

            this.RemoveClosingSnapshot();

            this._itemContainer?.Recycle();
        }

        public override CGSize SystemLayoutSizeFittingSize(
            CGSize targetSize,
            float horizontalFittingPriority,
            float verticalFittingPriority)
        {
            float width = (float)targetSize.Width;

            return new CGSize(width, this.MeasureContentHeight(width));
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();

            var nativeContent = this.NativeContent;
            if (null == nativeContent)
            {
                return;
            }

            var bounds = this.ContentView.Bounds;

            // Ask the table for a new row height when the content no longer fits,
            // for example after an item shows or hides its command bar
            float requiredHeight = this.MeasureContentHeight((float)bounds.Width);
            if (Math.Abs(requiredHeight - (float)bounds.Height) > 0.5f)
            {
                Action completion = null;

                // Closing: the content has already lost what it hid (such as the
                // command bar), so cover it with a picture of the last frame shown
                // while the rows below slide back up over it
                if ((requiredHeight < (float)bounds.Height) && (null == this._closingSnapshot) && (null != this.Window))
                {
                    var snapshot = this.ContentView.SnapshotView(false);
                    if (null != snapshot)
                    {
                        snapshot.Frame = bounds;
                        this.ContentView.AddSubview(snapshot);
                        this._closingSnapshot = snapshot;

                        completion = this.RemoveClosingSnapshot;
                    }
                }

                this.FindListView()?.InvalidateRowHeights(completion);
            }

            // Lay the content out at its full height and let the cell clip it, so
            // when the table animates the row taller the rows below slide away to
            // uncover the content (rather than the content stretching)
            this._itemContainer.Arrange(new Rect(0, 0, (float)bounds.Width, requiredHeight));

            // While a row closes, fill the gap with the item background
            var theme = XForms.Application.Current?.Theme;
            if (null != theme)
            {
                this.BackgroundColor = theme.BackgroundColor.ToUIColor();
            }
        }

        private NativeListView FindListView()
        {
            var view = this.Superview;
            while ((null != view) && !(view is NativeListView))
            {
                view = view.Superview;
            }

            return (NativeListView)view;
        }

        private float MeasureContentHeight(
            float width)
        {
            var itemContainer = this._itemContainer;
            if (null == itemContainer)
            {
                return 0;
            }

            itemContainer.Measure(new Size(width, float.PositiveInfinity));

            return itemContainer.DesiredSize.Height;
        }
    }
}
