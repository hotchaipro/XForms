using System;
using UIKit;
using XForms.Platform;

namespace XForms.iOS.Renderers
{
    internal class PageRenderer : ViewRenderer<NativeLayoutView>, IPageRenderer
    {
        public PageRenderer(
            Page page)
            : base(page)
        {
            var nativePage = new NativeLayoutView(page)
            {
                // The page background fills the screen; its content stays clear
                // of the status bar, notch and home indicator
                RespectsSafeArea = true,
            };

            this.SetNativeElement(nativePage);
        }

        public void SetLayout(
            IPanelRenderer layoutRenderer)
        {
            // NOTE: A lightweight layout has no native view; the page draws it
            // and hosts the native views inside it
            this._layout?.RemoveFromSuperview();
            this._layout = (UIView)layoutRenderer.NativeElement;
            if (null != this._layout)
            {
                this.NativeElement.InsertSubview(this._layout, 0);
            }

            this.NativeElement.SetNeedsDisplay();
        }

        private UIView _layout;
    }
}
