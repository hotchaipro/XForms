using System;
using Windows.Win32.Graphics.Direct2D.Common;
using Windows.Win32.Graphics.DirectComposition;
using XForms.Primitives;
using XForms.Platform;

namespace XForms.Win32.Renderers
{
    /// <summary>
    /// Scrolls its content vertically on the compositor.
    /// </summary>
    internal sealed unsafe class ScrollViewerRenderer : ViewRenderer, IScrollViewerRenderer, IScrollableRenderer, IScrollContent
    {
        // Scrolled-out content takes no input
        protected override bool ClipsHitTesting => true;

        public bool IsHorizontal => false;

        private readonly IDCompositionVisual2* _contentVisual;
        private readonly ScrollController _scroller;
        private View _content;

        public ScrollViewerRenderer(
            ScrollViewer scrollView)
            : base(scrollView, isContainer: false)
        {
            IDCompositionVisual2* contentVisual;
            this.Host.CompositionDevice->CreateVisual(&contentVisual);
            this._contentVisual = contentVisual;

            this.Visual->AddVisual((IDCompositionVisual*)contentVisual, true, null);

            this._scroller = new ScrollController(this);
        }

        protected override IDCompositionVisual2* ChildVisual
        {
            get
            {
                return this._contentVisual;
            }
        }

        public override Point ChildOffset
        {
            get
            {
                return new Point(0, -this._scroller.Offset);
            }
        }

        public ScrollController Scroller
        {
            get
            {
                return this._scroller;
            }
        }

        public View Content
        {
            get
            {
                return this._content;
            }

            set
            {
                this.ClearChildren();
                this._content = value;

                if (value?.Renderer is ViewRenderer child)
                {
                    this.AddChild(child);
                }
            }
        }

        public bool IsVerticalScrollingEnabled
        {
            get;
            set;
        } = true;

        public bool IsHorizontalScrollingEnabled
        {
            get;
            set;
        }

        IDCompositionVisual2* IScrollContent.ContentVisual
        {
            get
            {
                return this._contentVisual;
            }
        }

        public float ViewportHeight
        {
            get
            {
                return this.Frame.Height;
            }
        }

        public float ContentHeight
        {
            get
            {
                return this._content?.Bounds.Height ?? 0;
            }
        }

        public void OnScrollRange(
            float fromOffset,
            float toOffset)
        {
            // A tall content layer draws what's about to come into view
            (this._content?.Renderer as ViewRenderer)?.EnsureDrawn(fromOffset, toOffset + this.ViewportHeight);
        }

        public void OnScrollSettled()
        {
            float offset = this._scroller.Offset;
            (this._content?.Renderer as ViewRenderer)?.EnsureDrawn(offset, offset + this.ViewportHeight);
            this.Host.UpdateTextBoxPositions();
        }

        public float VerticalOffset
        {
            get
            {
                return this._scroller.Offset;
            }
        }

        public void ScrollTo(
            float verticalOffset,
            bool animated)
        {
            this._scroller.ScrollTo(verticalOffset, animated);
        }

        public void ScrollBy(
            float delta)
        {
            if (this.IsVerticalScrollingEnabled)
            {
                this._scroller.ScrollBy(delta);
            }
        }

        public override Size MeasureNative(
            Size availableSize)
        {
            return new Size(
                float.IsInfinity(availableSize.Width) ? 0 : availableSize.Width,
                float.IsInfinity(availableSize.Height) ? 0 : availableSize.Height);
        }

        public override void ArrangeNative(
            Rect frame)
        {
            base.ArrangeNative(frame);

            float scale = this.Host.Scale;
            this.Visual->SetClip(new D2D_RECT_F() { left = 0, top = 0, right = frame.Width * scale, bottom = frame.Height * scale });

            var content = this._content;
            if (null != content)
            {
                content.Measure(new Size(
                    frame.Width,
                    this.IsVerticalScrollingEnabled ? float.PositiveInfinity : frame.Height));

                // Content smaller than the viewport still fills it
                float height = Math.Max(content.DesiredSize.Height, frame.Height);
                content.Arrange(new Rect(0, 0, frame.Width, height));
            }

            this._scroller.Coerce();
        }
    }
}
