using System;
using Android.Widget;
using AndroidListPicker = global::Android.Widget.Spinner;
using XForms.Controls;

namespace XForms.Android.Renderers
{
    public class ListPickerRenderer : ControlRenderer<AndroidListPicker>, IListPickerRenderer
    {
        private AndroidListPicker _nativeListPicker;
        private NativeListPickerAdapter _nativeAdapter;

        public ListPickerRenderer(
            global::Android.Content.Context context,
            ListPicker listPicker)
            : base(context, listPicker)
        {
            this._nativeAdapter = new NativeListPickerAdapter(context);

            this._nativeListPicker = new AndroidListPicker(context)
            {
                Adapter = this._nativeAdapter,
            };

            this._nativeListPicker.ItemSelected += NativeListPicker_ItemSelected;

            this.SetNativeElement(this._nativeListPicker);
        }

        public int Count
        {
            get
            {
                return this._nativeListPicker.Count;
            }
        }

        public int SelectedIndex
        {
            get
            {
                return this._nativeListPicker.SelectedItemPosition;
            }

            set
            {
                this._nativeListPicker.SetSelection(value);
            }
        }

        public ListPickerItem SelectedValue
        {
            get
            {
                return this._nativeAdapter[this.SelectedIndex];
            }

            set
            {
                int position = this._nativeAdapter.GetPosition(value);
                this._nativeListPicker.SetSelection(position);
            }
        }

        public void AddItem(
            ListPickerItem item)
        {
            this._nativeAdapter.Add(item);
        }

        public void Clear()
        {
            this._nativeAdapter.Clear();
        }

        private void NativeListPicker_ItemSelected(
            object sender,
            AdapterView.ItemSelectedEventArgs e)
        {
            var item = this._nativeAdapter[e.Position];
            if (null != item)
            {
                ((IListPickerDelegate)this.Element).NotifyItemSelected(item);
            }
        }
    }
}
