using System;
using XForms.Controls;
using AndroidScrollView = global::Android.Widget.ScrollView;

namespace XForms.Android.Renderers
{
    public class ScrollViewRenderer : ViewRenderer<AndroidScrollView>, IScrollViewRenderer
    {
        private AndroidScrollView _nativeScrollView;
        private View _content;

        public ScrollViewRenderer(
            global::Android.Content.Context context,
            ScrollView scrollView)
            : base(context, scrollView)
        {
            this._nativeScrollView = new AndroidScrollView(context);

            this.SetNativeElement(this._nativeScrollView);
        }

        public View Content
        {
            get
            {
                return this._content;
            }

            set
            {
                this._content = value;

                if (null != value)
                {
                    this._nativeScrollView.RemoveAllViews();
                    this._nativeScrollView.AddView((global::Android.Views.View)value.Renderer?.NativeElement);
                }
                else
                {
                    this._nativeScrollView.RemoveAllViews();
                }
            }
        }

        public bool IsVerticalScrollingEnabled
        {
            get
            {
                return this._nativeScrollView.VerticalScrollBarEnabled;
            }

            set
            {
                this._nativeScrollView.VerticalScrollBarEnabled = value;
            }
        }

        public bool IsHorizontalScrollingEnabled
        {
            get
            {
                return this._nativeScrollView.HorizontalScrollBarEnabled;
            }

            set
            {
                this._nativeScrollView.HorizontalScrollBarEnabled = value;
            }
        }
    }
}
