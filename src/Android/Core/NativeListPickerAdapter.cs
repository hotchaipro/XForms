using System;
using System.Collections.Generic;
using Android.Views;
using Android.Widget;
using AndroidTextView = global::Android.Widget.TextView;
using XForms.Controls;

namespace XForms.Android
{
    internal sealed class NativeListPickerAdapter : BaseAdapter<ListPickerItem>
    {
        private global::Android.Content.Context _context;
        private List<ListPickerItem> _items;

        public NativeListPickerAdapter(
            global::Android.Content.Context context)
        {
            if (null == context)
            {
                throw new ArgumentNullException(nameof(context));
            }

            this._context = context;
            this._items = new List<ListPickerItem>();
        }

        public override ListPickerItem this[int position]
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
            ListPickerItem item)
        {
            return this._items.IndexOf(item);
        }

        public override global::Android.Views.View GetView(
            int position,
            global::Android.Views.View convertView,
            ViewGroup parent)
        {
            AndroidTextView view;

            ListPickerItem item = this._items[position];

            if (convertView == null)
            {
                view = new AndroidTextView(this._context)
                {
                    Text = item.Text,
                };
            }
            else
            {
                view = convertView as AndroidTextView;
                view.Text = item.Text;
            }

            return view;
        }

        public void Add(
            ListPickerItem item)
        {
            if (null == item)
            {
                throw new ArgumentNullException(nameof(item));
            }

            this._items.Add(item);
            this.NotifyDataSetChanged();
        }

        public void Clear()
        {
            this._items.Clear();
            this.NotifyDataSetChanged();
        }
    }
}
