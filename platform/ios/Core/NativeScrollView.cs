using System;
using CoreGraphics;
using UIKit;

namespace XForms.iOS
{
    /// <summary>
    /// A UIScrollView that measures its XForms content with an unbounded size
    /// along each scrolling axis and scrolls whatever overflows.
    /// </summary>
    internal sealed class NativeScrollView : UIScrollView
    {
        private View _content;

        public NativeScrollView()
        {
            this.IsVerticalScrollingEnabled = true;
            this.KeyboardDismissMode = UIScrollViewKeyboardDismissMode.Interactive;
            this.AlwaysBounceVertical = true;
        }

        public bool IsVerticalScrollingEnabled
        {
            get;
            set;
        }

        public bool IsHorizontalScrollingEnabled
        {
            get;
            set;
        }

        public View Content
        {
            get
            {
                return this._content;
            }

            set
            {
                (this._content?.Renderer?.NativeElement as UIView)?.RemoveFromSuperview();

                this._content = value;

                if (value?.Renderer?.NativeElement is UIView nativeContent)
                {
                    this.AddSubview(nativeContent);
                }

                this.SetNeedsLayout();
            }
        }

        public override CGSize SizeThatFits(
            CGSize size)
        {
            // A scroll view takes whatever room it is given
            return size;
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();

            var content = this._content;
            if (null == content)
            {
                this.ContentSize = CGSize.Empty;
                return;
            }

            var viewport = this.AdjustedContentInset.InsetRect(this.Bounds);
            float viewportWidth = (float)Math.Max(0, viewport.Width);
            float viewportHeight = (float)Math.Max(0, viewport.Height);

            content.Measure(new Size(
                this.IsHorizontalScrollingEnabled ? float.PositiveInfinity : viewportWidth,
                this.IsVerticalScrollingEnabled ? float.PositiveInfinity : viewportHeight));

            var desiredSize = content.DesiredSize;

            // Content smaller than the viewport still fills it
            float contentWidth = this.IsHorizontalScrollingEnabled ? Math.Max(desiredSize.Width, viewportWidth) : viewportWidth;
            float contentHeight = this.IsVerticalScrollingEnabled ? Math.Max(desiredSize.Height, viewportHeight) : viewportHeight;

            content.Arrange(new Rectangle(0, 0, contentWidth, contentHeight));

            this.ContentSize = new CGSize(contentWidth, contentHeight);
        }
    }
}
