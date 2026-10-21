using System;
using Foundation;
using UIKit;
using XForms.Controls;

namespace XForms.iOS.Renderers
{
    internal class ListViewRenderer : ControlRenderer<NativeListView>, IListViewRenderer
    {
        private readonly NativeListView _nativeListView;
        private NativeListViewSource _source;

        public ListViewRenderer(
            ListView listView)
            : base(listView)
        {
            this._nativeListView = new NativeListView();

            this.SetNativeElement(this._nativeListView);
        }

        public float FooterHeight
        {
            get
            {
                return (float)this._nativeListView.ContentInset.Bottom;
            }

            set
            {
                var inset = this._nativeListView.ContentInset;
                inset.Bottom = value;
                this._nativeListView.ContentInset = inset;
            }
        }

        public void Bind(
            ListViewSource source)
        {
            var oldSource = this._source;
            if (null != oldSource)
            {
                oldSource.Detach();
                this._source = null;
            }

            if (null != source)
            {
                this._source = new NativeListViewSource((ListView)this.Element, source);
                this._source.Attach(this._nativeListView);
            }

            this._nativeListView.Source = this._source;
            this._nativeListView.ReloadData();

            oldSource?.Dispose();
        }

        public void ScrollToItem(
            object item)
        {
            if (null == item)
            {
                return;
            }

            int position = this._source?.GetPosition(item) ?? -1;
            if (position >= 0)
            {
                this._nativeListView.ScrollToRow(
                    NSIndexPath.FromRowSection(position, 0),
                    UITableViewScrollPosition.None,
                    true);
            }
        }
    }
}
