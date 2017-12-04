using System;
using XForms.Layouts;

namespace XForms.Android.Renderers
{
    public class DistributedStackLayoutRenderer : LayoutRenderer<NativeDistributedStackLayout>, IDistributedStackLayoutRenderer
    {
        private NativeDistributedStackLayout _layout;

        public DistributedStackLayoutRenderer(
            global::Android.Content.Context context,
            XForms.Layouts.DistributedStackLayout layout)
            : base(context, layout)
        {
            this._layout = new NativeDistributedStackLayout(context);

            // NOTE: Currently only horizontal orientation is supported

            this.SetNativeElement(this._layout);
        }

        void IDistributedStackLayoutRenderer.AddChild(
            IElementRenderer childRenderer)
        {
            base.AddChild(childRenderer);
        }

        void IDistributedStackLayoutRenderer.ClearChildren()
        {
            base.ClearChildren();
        }

        void IDistributedStackLayoutRenderer.InsertChild(
            int index,
            IElementRenderer childRenderer)
        {
            base.InsertChild(index, childRenderer);
        }

        void IDistributedStackLayoutRenderer.RemoveChildAt(
            int index)
        {
            base.RemoveChildAt(index);
        }

        void IDistributedStackLayoutRenderer.ReplaceChild(
            int index,
            IElementRenderer childRenderer)
        {
            base.ReplaceChild(index, childRenderer);
        }
    }
}
