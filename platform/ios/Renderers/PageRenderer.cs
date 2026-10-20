using System;
using UIKit;

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
            ILayoutRenderer layoutRenderer)
        {
            foreach (var subview in this.NativeElement.Subviews)
            {
                subview.RemoveFromSuperview();
            }

            this.NativeElement.AddSubview((UIView)layoutRenderer.NativeElement);
        }
    }
}
