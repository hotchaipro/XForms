using System;
using XForms.Controls;
using AndroidListView = global::Android.Widget.ListView;

namespace XForms.Android.Renderers
{
    public class ListViewRenderer : ControlRenderer<AndroidListView>, IListViewRenderer
    {
        private AndroidListView _nativeListView;

        public ListViewRenderer(
            global::Android.Content.Context context,
            ListView listView)
            : base(context, listView)
        {
            this._nativeListView = new AndroidListView(context)
            {
                ChoiceMode = global::Android.Widget.ChoiceMode.Single,
                Divider = null,
                DividerHeight = 0,
            };
            this._nativeListView.SetClipToPadding(false);
            this._nativeListView.SetSelector(global::Android.Resource.Color.Transparent);

            this.SetNativeElement(this._nativeListView);
        }

        public float FooterHeight
        {
            get
            {
                return this.PixelsToDevicePixels(this._nativeListView.PaddingBottom);
            }

            set
            {
                this._nativeListView.SetPadding(
                    this._nativeListView.PaddingLeft,
                    this._nativeListView.PaddingTop,
                    this._nativeListView.PaddingRight,
                    (int)this.DevicePixelsToPixels(value));
            }
        }

        //private void NativeListView_ItemClick(
        //    object sender,
        //    ItemClickEventArgs e)
        //{
        //    if (null == e)
        //    {
        //        return;
        //    }

        //    var item = e.ClickedItem as CustomItem;
        //    if (null == item)
        //    {
        //        return;
        //    }

        //    ((IListViewDelegate)this.Element).NotifyItemClicked(null, null, item.Item, item.Container?.View);
        //}

        public void Bind(
            ListViewSource source)
        {
            this._nativeListView?.Adapter?.Dispose();

            this._nativeListView.Adapter = new NativeListViewAdapter((ListView)this.Element, this.NativeContext, source);
        }

        public void ScrollToItem(
            object item)
        {
            if (null == item)
            {
                return;
            }

            int position = ((NativeListViewAdapter)this._nativeListView.Adapter)?.GetPosition(item) ?? -1;
            if (position >= 0)
            {
                this._nativeListView.SmoothScrollToPosition(position);
            }
        }
    }
}
