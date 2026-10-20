using System;
using XForms.Layouts;

namespace XForms.Controls
{
    public class ListViewItem : ContentControl
    {
        public ListViewItem()
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
