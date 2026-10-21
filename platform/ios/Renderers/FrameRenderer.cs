using System;
using UIKit;
using NativeView = UIKit.UIView;

namespace XForms.iOS.Renderers
{
    public class FrameRenderer : ViewRenderer<NativeView>, IFrameRenderer
    {
        private UIViewController _viewController;

        public FrameRenderer(
            Frame frame)
            : base(frame)
        {
            var view = new RootView(frame)
            {
                BackgroundColor = UIColor.SystemBackground,
            };

            this._viewController = new UIViewController();
            this._viewController.View = view;

            this.SetNativeElement(view);
        }

        // Reports its size: the root frame is the window's content
        private sealed class RootView : NativeView
        {
            private readonly Frame _frame;

            public RootView(
                Frame frame)
            {
                this._frame = frame;
            }

            public override void LayoutSubviews()
            {
                base.LayoutSubviews();
                this._frame.NotifyRootSize(new Size((float)this.Bounds.Width, (float)this.Bounds.Height));
            }
        }

        internal UIViewController ViewController
        {
            get
            {
                return this._viewController;
            }
        }

        // Pages stay in the frame (hidden when not showing), so transitions can
        // show two at once
        public void AddPage(
            Page page)
        {
            var nativePage = (NativeView)page.Renderer.NativeElement;
            if (nativePage.Superview != this.NativeElement)
            {
                this.NativeElement.AddSubview(nativePage);
                nativePage.Frame = this.NativeElement.Bounds;
                nativePage.AutoresizingMask = UIViewAutoresizing.FlexibleDimensions;
            }
        }

        public void RemovePage(
            Page page)
        {
            ((NativeView)page.Renderer.NativeElement).RemoveFromSuperview();
        }

        public void BringToFront(
            Page page)
        {
            this.NativeElement.BringSubviewToFront((NativeView)page.Renderer.NativeElement);
        }
    }
}
