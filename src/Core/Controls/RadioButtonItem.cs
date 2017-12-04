using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

namespace XForms.Controls
{
    public sealed class RadioButtonItemCollection : IEnumerable<RadioButtonItem>
    {
        private RadioButtonPicker _radioButtonGroup;
        private List<RadioButtonItem> _items;

        internal RadioButtonItemCollection(
            RadioButtonPicker radioButtonGroup)
        {
            if (null == radioButtonGroup)
            {
                throw new ArgumentNullException(nameof(radioButtonGroup));
            }

            this._radioButtonGroup = radioButtonGroup;
            this._items = new List<RadioButtonItem>();
        }

        public void Add(
            RadioButtonItem radioButtonItem)
        {
            if (null == radioButtonItem)
            {
                throw new ArgumentNullException(nameof(radioButtonItem));
            }

            this._radioButtonGroup.Add(radioButtonItem);
        }

        public IEnumerator<RadioButtonItem> GetEnumerator()
        {
            return ((IEnumerable<RadioButtonItem>)_items).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<RadioButtonItem>)_items).GetEnumerator();
        }
    }

    public class RadioButtonItem : INotifyPropertyChanged
    {
        private Bitmap _icon;
        private string _text;

        public event PropertyChangedEventHandler PropertyChanged;

        public RadioButtonItem()
        {
        }

        public Bitmap Icon
        {
            get
            {
                return this._icon;
            }

            set
            {
                this._icon = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Icon)));
            }
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
    }
}
