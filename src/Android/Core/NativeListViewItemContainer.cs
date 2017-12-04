using System;
using System.Collections.Specialized;
using XForms.Controls;
using Android.Views;

namespace XForms.Android
{
    internal sealed class NativeListViewItemContainer : global::Android.Widget.FrameLayout
    {
        private ListViewItemContainer _itemContainer;

        internal NativeListViewItemContainer(
            global::Android.Content.Context context)
            : base(context)
        {
        }

        internal ListViewItemContainer ItemContainer
        {
            get
            {
                return this._itemContainer;
            }

            set
            {
                if (value != this._itemContainer)
                {
                    this._itemContainer = value;

                    this.RemoveAllViews();
                    if (null != value)
                    {
                        this.AddView((global::Android.Views.View)value.Renderer.NativeElement);
                    }
                }
            }
        }
    }
}
