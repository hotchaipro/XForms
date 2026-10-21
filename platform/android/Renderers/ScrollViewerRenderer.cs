using System;
using XForms.Controls;
using XForms.Platform;
using AndroidScrollView = global::Android.Widget.ScrollView;

namespace XForms.Android.Renderers
{
    public class ScrollViewerRenderer : ViewRenderer<AndroidScrollView>, IScrollViewerRenderer
    {
        public float VerticalOffset
        {
            get
            {
                return NativeConversions.PixelsToDevicePixels(this.NativeContext, this._nativeScrollView.ScrollY);
            }
        }

        public void ScrollTo(
            float verticalOffset,
            bool animated)
        {
            int y = (int)Math.Round(NativeConversions.DevicePixelsToPixels(this.NativeContext, Math.Max(0, verticalOffset)));
            if (animated)
            {
                this._nativeScrollView.SmoothScrollTo(0, y);
            }
            else
            {
                this._nativeScrollView.ScrollTo(0, y);
            }
        }

        private NativeScrollView _nativeScrollView;
        private View _content;

        public ScrollViewerRenderer(
            global::Android.Content.Context context,
            ScrollViewer scrollView)
            : base(context, scrollView)
        {
            this._nativeScrollView = new NativeScrollView(context);

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
                this._nativeScrollView.Content = value;

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
