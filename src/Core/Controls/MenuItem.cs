using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

namespace XForms.Controls
{
    public interface IMenuItemContainer
    {
        void Add(MenuItem menuItem);
    }

    public sealed class MenuItemCollection : IEnumerable<MenuItem>
    {
        private IMenuItemContainer _itemContainer;
        private List<MenuItem> _items;

        internal MenuItemCollection(
            IMenuItemContainer itemContainer)
        {
            if (null == itemContainer)
            {
                throw new ArgumentNullException(nameof(itemContainer));
            }

            this._itemContainer = itemContainer;
            this._items = new List<MenuItem>();
        }

        public void Add(
            MenuItem menuItem)
        {
            if (null == menuItem)
            {
                throw new ArgumentNullException(nameof(menuItem));
            }

            this._itemContainer.Add(menuItem);
        }

        public IEnumerator<MenuItem> GetEnumerator()
        {
            return ((IEnumerable<MenuItem>)_items).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<MenuItem>)_items).GetEnumerator();
        }
    }

    public class MenuItem : INotifyPropertyChanged
    {
        private ICommand _command;
        private Bitmap _icon;
        private string _text;
        private bool _isVisible;

        public event PropertyChangedEventHandler PropertyChanged;

        public MenuItem()
        {
            this._isVisible = true;
        }

        public ICommand Command
        {
            get
            {
                return this._command;
            }

            set
            {
                this._command = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Command)));
            }
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

        public bool IsVisible
        {
            get
            {
                return this._isVisible;
            }

            set
            {
                this._isVisible = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsVisible)));
            }
        }
    }
}
