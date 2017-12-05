using System;
using UIKit;

namespace XForms.iOS.Renderers
{
    public class PageRenderer : ViewRenderer<NativePage>, IPageRenderer
    {
        public PageRenderer(
            Page page)
            : base(page)
        {
            var nativePage = new NativePage();

            this.SetNativeElement(nativePage);
        }

        public void SetLayout(
            ILayoutRenderer layoutRenderer)
        {
            for (int i = 0; i < this.NativeElement.Subviews.Length; i++)
            {
                this.NativeElement.Subviews[i].RemoveFromSuperview();
            }
            var nativeLayout = (NativeLayout)layoutRenderer.NativeElement;
            this.NativeElement.AddSubview(nativeLayout);
        }
    }
}
