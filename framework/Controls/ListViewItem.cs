using System;
using XForms.Layouts;

namespace XForms.Controls
{
    public class ListViewItem : ContentControl
    {
        private object _item;

        public ListViewItem()
        {
        }

        /// <summary>
        /// The item the view shows, set by the list (null while the view is
        /// set aside for reuse).
        /// </summary>
        public object Item
        {
            get
            {
                return this._item;
            }

            set
            {
                if (value != this._item)
                {
                    this._item = value;
                    this.OnItemChanged();
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Called when the view starts showing another item: bind it here.
        /// </summary>
        protected virtual void OnItemChanged()
        {
        }

        public override Color BackgroundColor
        {
            get
            {
                return this.Theme.BackgroundColor;
            }
        }

        /// <summary>
        /// The container showing this view, while it shows an item.
        /// </summary>
        public ListViewItemContainer Container
        {
            get;
            internal set;
        }
    }
}
