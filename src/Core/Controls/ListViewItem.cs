using System;
using XForms.Layouts;

namespace XForms.Controls
{
    public class ListViewItem : ContentControl
    {
        public ListViewItem()
        {
        }

        internal ListViewItemContainer Container
        {
            get;
            set;
        }

        public bool IsCommandBarVisible
        {
            get
            {
                var container = this.Container;
                if (null == container)
                {
                    return false;
                }

                return container.IsCommandBarVisible;
            }

            set
            {
                var container = this.Container;
                if (null != container)
                {
                    container.IsCommandBarVisible = value;
                }
            }
        }

        public Thickness CommandBarMargin
        {
            get
            {
                var container = this.Container;
                if (null == container)
                {
                    return Thickness.Zero;
                }

                return container.CommandBarMargin;
            }

            set
            {
                var container = this.Container;
                if (null != container)
                {
                    container.CommandBarMargin = value;
                }
            }
        }
    }
}
