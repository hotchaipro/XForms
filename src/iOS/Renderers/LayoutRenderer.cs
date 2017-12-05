using System;
using NativeView = UIKit.UIView;

namespace XForms.iOS.Renderers
{
    public abstract class LayoutRenderer<TNativeLayout> : ViewRenderer<TNativeLayout>, ILayoutRenderer
        where TNativeLayout : NativeLayout
    {
        protected LayoutRenderer(
            Layout layout)
            : base(layout)
        {
        }

        protected override void OnNativeElementSet(
            TNativeLayout oldElement,
            TNativeLayout newElement)
        {
            base.OnNativeElementSet(oldElement, newElement);

            if (null != newElement)
            {
                // Default to no clipping
                //this.IsClippedToBounds = false;
            }
        }

        protected virtual void ClearChildren()
        {
            foreach (var subView in this.NativeElement.Subviews)
            {
                subView?.RemoveFromSuperview();
            }
        }

        protected virtual NativeView AddChild(
            IElementRenderer childRenderer)
        {
            var nativeChild = (NativeView)childRenderer.NativeElement;
            this.NativeElement.AddSubview(nativeChild);
            return nativeChild;
        }

        protected virtual NativeView InsertChild(
            int index,
            IElementRenderer childRenderer)
        {
            var nativeChild = (NativeView)childRenderer.NativeElement;
            this.NativeElement.InsertSubview(nativeChild, index);
            return nativeChild;
        }

        protected virtual void RemoveChildAt(
            int index)
        {
            this.NativeElement.Subviews[index]?.RemoveFromSuperview();
        }

        protected virtual NativeView ReplaceChild(
            int index,
            IElementRenderer childRenderer)
        {
            var nativeChild = (NativeView)childRenderer.NativeElement;
            this.NativeElement.Subviews[index]?.RemoveFromSuperview();
            this.NativeElement.InsertSubview(nativeChild, index);
            return nativeChild;
        }
    }
}
