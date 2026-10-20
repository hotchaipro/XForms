using System;
using UIKit;

namespace XForms.iOS.Renderers
{
    /// <summary>
    /// Renderer for every XForms layout: the layout itself positions its
    /// children, so the platform only keeps the native child list.
    /// </summary>
    internal class LayoutRenderer : ViewRenderer<NativeLayoutView>, ILayoutRenderer
    {
        public LayoutRenderer(
            Layout layout)
            : base(layout)
        {
            this.SetNativeElement(new NativeLayoutView(layout));
        }

        public void ClearChildren()
        {
            foreach (var subview in this.NativeElement.Subviews)
            {
                subview.RemoveFromSuperview();
            }
        }

        public void AddChild(
            IViewRenderer childRenderer)
        {
            this.NativeElement.AddSubview((UIView)childRenderer.NativeElement);
        }

        public void InsertChild(
            int index,
            IViewRenderer childRenderer)
        {
            this.NativeElement.InsertSubview((UIView)childRenderer.NativeElement, index);
        }

        public void RemoveChildAt(
            int index)
        {
            this.NativeElement.Subviews[index].RemoveFromSuperview();
        }

        public void ReplaceChild(
            int index,
            IViewRenderer childRenderer)
        {
            this.NativeElement.Subviews[index].RemoveFromSuperview();
            this.NativeElement.InsertSubview((UIView)childRenderer.NativeElement, index);
        }
    }
}
