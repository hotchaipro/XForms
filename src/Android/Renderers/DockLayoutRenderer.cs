using System;
using XForms.Layouts;
using AndroidView = global::Android.Views.View;

namespace XForms.Android.Renderers
{
    public class DockLayoutRenderer : LayoutRenderer<NativeDockLayout>, IDockLayoutRenderer
    {
        private NativeDockLayout _dockLayout;

        public DockLayoutRenderer(
            global::Android.Content.Context context,
            XForms.Layouts.DockLayout layout)
            : base(context, layout)
        {
            this._dockLayout = new NativeDockLayout(context);

            this.SetNativeElement(this._dockLayout);
        }

        void IDockLayoutRenderer.ClearChildren()
        {
            this.ClearChildren();
        }

        void IDockLayoutRenderer.AddChild(
            IElementRenderer childRenderer,
            DockRegion dockRegion)
        {
            var child = this.AddChild(childRenderer);
            this.SetAndroidDockRegion(child, dockRegion);
        }

        void IDockLayoutRenderer.InsertChild(
            int index,
            IElementRenderer childRenderer,
            DockRegion dockRegion)
        {
            var child = this.InsertChild(index, childRenderer);
            this.SetAndroidDockRegion(child, dockRegion);
        }

        void IDockLayoutRenderer.RemoveChildAt(
            int index)
        {
            this.RemoveChildAt(index);
        }

        void IDockLayoutRenderer.ReplaceChild(
            int index,
            IElementRenderer childRenderer,
            DockRegion dockRegion)
        {
            var child = this.ReplaceChild(index, childRenderer);
            SetAndroidDockRegion(child, dockRegion);
        }

        private void SetAndroidDockRegion(
            AndroidView nativeView,
            DockRegion dockRegion)
        {
            if (null == nativeView)
            {
                throw new ArgumentNullException(nameof(nativeView));
            }

            var layoutParameters = (NativeDockLayout.LayoutParams)nativeView.LayoutParameters;
            layoutParameters.DockRegion = dockRegion;
        }
    }
}
