using System;
using NativeView = UIKit.UIView;

namespace XForms.iOS.Renderers
{
    public class DockLayoutRenderer : LayoutRenderer<NativeDockLayout>, IDockLayoutRenderer
    {
        private NativeDockLayout _dockLayout;

        public DockLayoutRenderer(
            DockLayout layout)
            : base(layout)
        {
            this._dockLayout = new NativeDockLayout();

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
            this.SetNativeDockRegion(child, dockRegion);
        }

        void IDockLayoutRenderer.InsertChild(
            int index,
            IElementRenderer childRenderer,
            DockRegion dockRegion)
        {
            var child = this.InsertChild(index, childRenderer);
            this.SetNativeDockRegion(child, dockRegion);
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
            SetNativeDockRegion(child, dockRegion);
        }

        private void SetNativeDockRegion(
            NativeView nativeView,
            DockRegion dockRegion)
        {
            if (null == nativeView)
            {
                throw new ArgumentNullException(nameof(nativeView));
            }

            var layoutParameters = (NativeDockLayout.LayoutParams)nativeView.LayoutParameters();
            layoutParameters.DockRegion = dockRegion;
        }
    }
}
