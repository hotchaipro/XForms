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
            var view = new NativeView();

            this._viewController = new UIViewController();
            this._viewController.View = view;

            this.SetNativeElement(view);
        }

        internal UIViewController ViewController
        {
            get
            {
                return this._viewController;
            }
        }

        void IFrameRenderer.Push(
            Page page)
        {
        }

        public void SetContent(
            Page newPage,
            bool hideCurrentPage)
        {
            for (int i = 0; i < this.NativeElement.Subviews.Length; i++)
            {
                this.NativeElement.Subviews[i].RemoveFromSuperview();
            }
            var nativePage = (NativeView)newPage.Renderer.NativeElement;
            this.NativeElement.AddSubview(nativePage);
            nativePage.Frame = this.NativeElement.Frame;
        }
    }
}
