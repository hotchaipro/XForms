using System;
using UIKit;
using XForms.Controls;

namespace XForms.iOS.Renderers
{
    internal class ScrollViewerRenderer : ViewRenderer<NativeScrollView>, IScrollViewerRenderer
    {
        private readonly NativeScrollView _nativeScrollView;
        private View _content;

        public ScrollViewerRenderer(
            ScrollViewer scrollView)
            : base(scrollView)
        {
            this._nativeScrollView = new NativeScrollView();

            this.SetNativeElement(this._nativeScrollView);
        }

        public float VerticalOffset
        {
            get
            {
                return (float)this._nativeScrollView.ContentOffset.Y;
            }
        }

        public void ScrollTo(
            float verticalOffset,
            bool animated)
        {
            var scrollView = this._nativeScrollView;
            double maximum = Math.Max(0, scrollView.ContentSize.Height - scrollView.Bounds.Height);
            double y = Math.Max(0, Math.Min(verticalOffset, maximum));
            scrollView.SetContentOffset(new CoreGraphics.CGPoint(scrollView.ContentOffset.X, y), animated);
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
            }
        }

        public bool IsVerticalScrollingEnabled
        {
            get
            {
                return this._nativeScrollView.IsVerticalScrollingEnabled;
            }

            set
            {
                this._nativeScrollView.IsVerticalScrollingEnabled = value;
                this._nativeScrollView.ShowsVerticalScrollIndicator = value;
                this._nativeScrollView.AlwaysBounceVertical = value;
                this._nativeScrollView.SetNeedsLayout();
            }
        }

        public bool IsHorizontalScrollingEnabled
        {
            get
            {
                return this._nativeScrollView.IsHorizontalScrollingEnabled;
            }

            set
            {
                this._nativeScrollView.IsHorizontalScrollingEnabled = value;
                this._nativeScrollView.ShowsHorizontalScrollIndicator = value;
                this._nativeScrollView.SetNeedsLayout();
            }
        }
    }
}
