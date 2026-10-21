using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

namespace XForms.Controls
{
    /// <summary>
    /// The items of a <see cref="ComboBox"/>.
    /// </summary>
    public sealed class ComboBoxItemCollection : IReadOnlyList<ComboBoxItem>
    {
        private readonly ComboBox _listPicker;
        private readonly List<ComboBoxItem> _items = new List<ComboBoxItem>();

        internal ComboBoxItemCollection(
            ComboBox listPicker)
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

        public ComboBoxItem this[int index]
        {
            get
            {
                return this._items[index];
            }
        }

        public int IndexOf(
            ComboBoxItem item)
        {
            return this._items.IndexOf(item);
        }

        public void Add(
            ComboBoxItem listPickerItem)
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

        public IEnumerator<ComboBoxItem> GetEnumerator()
        {
            return this._items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this._items.GetEnumerator();
        }
    }

    public class ComboBoxItem : INotifyPropertyChanged
    {
        private string _text;

        public event PropertyChangedEventHandler PropertyChanged;

        public ComboBoxItem()
        {
        }

        public ComboBoxItem(
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
