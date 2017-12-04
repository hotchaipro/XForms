using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

namespace XForms.Controls
{
    public sealed class ListPickerItemCollection : IEnumerable<ListPickerItem>
    {
        private ListPicker _listPicker;
        private List<ListPickerItem> _items;

        internal ListPickerItemCollection(
            ListPicker listPicker)
        {
            if (null == listPicker)
            {
                throw new ArgumentNullException(nameof(listPicker));
            }

            this._listPicker = listPicker;
            this._items = new List<ListPickerItem>();
        }

        public int Count
        {
            get
            {
                return this._listPicker.Count;
            }
        }

        public void Add(
            ListPickerItem listPickerItem)
        {
            if (null == listPickerItem)
            {
                throw new ArgumentNullException(nameof(listPickerItem));
            }

            this._listPicker.Add(listPickerItem);
        }

        public void Clear()
        {
            this._listPicker.Clear();
        }

        public IEnumerator<ListPickerItem> GetEnumerator()
        {
            return ((IEnumerable<ListPickerItem>)_items).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<ListPickerItem>)_items).GetEnumerator();
        }
    }

    public class ListPickerItem : INotifyPropertyChanged
    {
        private string _text;

        public event PropertyChangedEventHandler PropertyChanged;

        public ListPickerItem()
        {
        }

        public ListPickerItem(
            string text)
        {
            this._text = text;
        }

        public string Text
        {
            get
            {
                return this._text;
            }

            set
            {
                this._text = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Text)));
            }
        }

        public object Value
        {
            get;
            set;
        }

        public override string ToString()
        {
            return this._text;
        }
    }
}
