using System;
using AndroidView = global::Android.Views.View;
using AndroidViewGroup = global::Android.Views.ViewGroup;

namespace XForms.Android.Renderers
{
    public abstract class LayoutRenderer<TNativeLayout> : ViewRenderer<TNativeLayout>, ILayoutRenderer
        where TNativeLayout : AndroidViewGroup
    {
        protected LayoutRenderer(
            global::Android.Content.Context context,
            XForms.Layout layout)
            : base(context, layout)
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
                this.IsClippedToBounds = false;

#if DEBUG_LAYOUT
                newElement.SetWillNotDraw(false); // Enable drawing for the background color
                newElement.SetBackgroundColor(new global::Android.Graphics.Color(0, 0, 0xff, 0x80));
#endif
            }
        }

        public bool IsClippedToBounds
        {
            get
            {
                return ((this.NativeElement.ClipChildren) && (this.NativeElement.ClipToPadding));
            }

            set
            {
                this.NativeElement.SetClipChildren(value);
                this.NativeElement.SetClipToPadding(value);
            }
        }

        protected virtual void ClearChildren()
        {
            this.NativeElement.RemoveViews(0, this.NativeElement.ChildCount);
        }

        protected virtual AndroidView AddChild(
            IElementRenderer childRenderer)
        {
            var nativeChild = (AndroidView)childRenderer.NativeElement;
            this.NativeElement.AddView(nativeChild);
            return nativeChild;
        }

        protected virtual AndroidView InsertChild(
            int index,
            IElementRenderer childRenderer)
        {
            var nativeChild = (AndroidView)childRenderer.NativeElement;
            this.NativeElement.AddView(nativeChild, index);
            return nativeChild;
        }

        protected virtual void RemoveChildAt(
            int index)
        {
            this.NativeElement.RemoveViewAt(index);
        }

        protected virtual AndroidView ReplaceChild(
            int index,
            IElementRenderer childRenderer)
        {
            var nativeChild = (AndroidView)childRenderer.NativeElement;
            this.NativeElement.RemoveViewAt(index);
            this.NativeElement.AddView(nativeChild, index);
            return nativeChild;
        }
    }
}
