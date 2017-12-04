using System;
using Windows.UI.Xaml;
using XForms.Controls;

namespace XForms.Windows.Renderers
{
    public class ListViewRenderer : ControlRenderer, IListViewRenderer
    {
        private NativeListView _nativeListView;

        public ListViewRenderer(
            XForms.Controls.ListView listView)
            : base(listView)
        {
            this._nativeListView = new NativeListView(listView);
            this._nativeListView.IsSynchronizedWithCurrentItem = false;
            this._nativeListView.SelectionMode = global::Windows.UI.Xaml.Controls.ListViewSelectionMode.None;

            var itemTemplate = (DataTemplate)global::Windows.UI.Xaml.Application.Current.Resources["CustomListViewItemTemplate"];
            this._nativeListView.ItemTemplate = itemTemplate;

            var itemStyle = (Style)global::Windows.UI.Xaml.Application.Current.Resources["CustomListViewItemContainerStyle"];
            this._nativeListView.ItemContainerStyle = itemStyle;

            //var listStyle = (Style)global::Windows.UI.Xaml.Application.Current.Resources["CustomListViewStyle"];
            //this._nativeListView.Style = listStyle;

            //this._nativeListView.IsItemClickEnabled = true;
            //this._nativeListView.ItemClick += NativeListView_ItemClick;

            this.SetNativeElement(this._nativeListView);
        }

        public float FooterHeight
        {
            get
            {
                return this._nativeListView.FooterHeight;
            }

            set
            {
                this._nativeListView.FooterHeight = value;
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
            this._nativeListView.Bind(source);
        }

        public void ScrollToItem(
            object item)
        {
            if (null == item)
            {
                return;
            }

            this._nativeListView.UpdateLayout();

            this._nativeListView.ScrollIntoView(item, global::Windows.UI.Xaml.Controls.ScrollIntoViewAlignment.Default);
        }
    }
}
