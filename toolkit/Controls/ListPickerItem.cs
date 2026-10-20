using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

namespace XForms.Toolkit
{
    /// <summary>
    /// The items of a <see cref="ListPicker"/>.
    /// </summary>
    public sealed class ListPickerItemCollection : IReadOnlyList<ListPickerItem>
    {
        private readonly ListPicker _listPicker;
        private readonly List<ListPickerItem> _items = new List<ListPickerItem>();

        internal ListPickerItemCollection(
            ListPicker listPicker)
        {
            this._listPicker = listPicker ?? throw new ArgumentNullException(nameof(listPicker));
        }

        public int Count
        {
            get
            {
                return this._items.Count;
            }
        }

        public ListPickerItem this[int index]
        {
            get
            {
                return this._items[index];
            }
        }

        public int IndexOf(
            ListPickerItem item)
        {
            return this._items.IndexOf(item);
        }

        public void Add(
            ListPickerItem listPickerItem)
        {
            if (null == listPickerItem)
            {
                throw new ArgumentNullException(nameof(listPickerItem));
            }

            this._items.Add(listPickerItem);
            listPickerItem.PropertyChanged += this.Item_PropertyChanged;
            this._listPicker.OnItemsChanged();
        }

        public void Clear()
        {
            foreach (var item in this._items)
            {
                item.PropertyChanged -= this.Item_PropertyChanged;
            }

            this._items.Clear();
            this._listPicker.OnItemsChanged();
        }

        private void Item_PropertyChanged(
            object sender,
            PropertyChangedEventArgs e)
        {
            this._listPicker.OnItemsChanged();
        }

        public IEnumerator<ListPickerItem> GetEnumerator()
        {
            return this._items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this._items.GetEnumerator();
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
