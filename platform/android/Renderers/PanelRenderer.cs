using System;
using AndroidView = global::Android.Views.View;
using AndroidViewGroup = global::Android.Views.ViewGroup;

namespace XForms.Android.Renderers
{
    public abstract class PanelRenderer<TNativeLayout> : ViewRenderer<TNativeLayout>
        where TNativeLayout : AndroidViewGroup
    {
        protected PanelRenderer(
            global::Android.Content.Context context,
            XForms.View view)
            : base(context, view)
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

        public virtual void ClearChildren()
        {
            this.NativeElement.RemoveViews(0, this.NativeElement.ChildCount);
        }

        public virtual void AddChild(
            IViewRenderer childRenderer)
        {
            this.NativeElement.AddView((AndroidView)childRenderer.NativeElement);
        }

        public virtual void InsertChild(
            int index,
            IViewRenderer childRenderer)
        {
            this.NativeElement.AddView((AndroidView)childRenderer.NativeElement, index);
        }

        public virtual void RemoveChildAt(
            int index)
        {
            this.NativeElement.RemoveViewAt(index);
        }

        public virtual void ReplaceChild(
            int index,
            IViewRenderer childRenderer)
        {
            this.NativeElement.RemoveViewAt(index);
            this.NativeElement.AddView((AndroidView)childRenderer.NativeElement, index);
        }
    }

    /// <summary>
    /// Renderer for every XForms layout: the layout itself positions its
    /// children, so the platform only needs a generic container.
    /// </summary>
    public class PanelRenderer : PanelRenderer<NativeLayoutView>, IPanelRenderer
    {
        public PanelRenderer(
            global::Android.Content.Context context,
            XForms.Panel layout)
            : base(context, layout)
        {
            this.SetNativeElement(new NativeLayoutView(context, layout));
        }
    }
}
