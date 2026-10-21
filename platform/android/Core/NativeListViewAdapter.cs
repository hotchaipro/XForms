using System;
using System.Collections.Specialized;
using XForms.Primitives;
using Android.Views;

namespace XForms.Android
{
    internal class NativeListViewAdapter : global::Android.Widget.BaseAdapter<object>, IDisposable
    {
        private ItemsView _listView;
        private global::Android.Content.Context _context;
        private ItemsViewSource _items;

        public NativeListViewAdapter(
            ItemsView listView,
            global::Android.Content.Context context,
            ItemsViewSource source)
        {
            if (null == listView)
            {
                throw new ArgumentNullException(nameof(listView));
            }

            if (null == context)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (null == source)
            {
                throw new ArgumentNullException(nameof(source));
            }

            this._listView = listView;
            this._context = context;
            this._items = source;

            this._items.CollectionChanged += Source_CollectionChanged;
        }

        private void Source_CollectionChanged(
            object sender,
            NotifyCollectionChangedEventArgs e)
        {
            this.NotifyDataSetChanged();
        }

        public override object this[int position]
        {
            get
            {
                return this._items[position];
            }
        }

        public override int Count
        {
            get
            {
                return this._items.Count;
            }
        }

        public override long GetItemId(
            int position)
        {
            return position;
        }

        public int GetPosition(
            object item)
        {
            return this._items.IndexOf(item);
        }

        public override global::Android.Views.View GetView(
            int position,
            global::Android.Views.View convertView,
            ViewGroup parent)
        {
            object item = this._items[position];

            // Attempt to reuse the existing native view
            NativeListViewItemContainer nativeItemContainer = convertView as NativeListViewItemContainer;
            if (null == nativeItemContainer)
            {
                // Create a new native view
                nativeItemContainer = new NativeListViewItemContainer(this._context);
            }

            nativeItemContainer.Item = item;

            // Get the non-native item container
            var itemContainer = this._items.GetItemContainer(this._listView, nativeItemContainer.ItemContainer, item);

            // Save a reference to the non-native item container for later reuse
            nativeItemContainer.ItemContainer = itemContainer;

            return nativeItemContainer;
        }

        protected override void Dispose(
            bool disposing)
        {
            base.Dispose(disposing);

            this._items.CollectionChanged -= Source_CollectionChanged;
        }
    }
}
