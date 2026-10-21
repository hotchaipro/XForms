using System;
using XForms.Controls;
using AndroidView = global::Android.Views.View;

namespace XForms.Android.Renderers
{
    public abstract class ControlRenderer<TElementType> : ViewRenderer<TElementType>, IControlRenderer
        where TElementType : AndroidView
    {
        public ControlRenderer(
            global::Android.Content.Context context,
            Control control)
            : base(context, control)
        {
        }
    }
}
