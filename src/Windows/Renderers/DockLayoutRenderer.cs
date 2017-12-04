using System;
using Windows.UI.Xaml;
using XForms.Layouts;

namespace XForms.Windows.Renderers
{
    public class DockLayoutRenderer : ViewRenderer, IDockLayoutRenderer
    {
        private NativeDockPanel _panel;

        public DockLayoutRenderer(
            XForms.Layouts.DockLayout layout)
            : base(layout)
        {
            this._panel = new NativeDockPanel();
            this._panel.HorizontalAlignment = global::Windows.UI.Xaml.HorizontalAlignment.Stretch;
            this._panel.VerticalAlignment = global::Windows.UI.Xaml.VerticalAlignment.Stretch;

            this.SetNativeElement(this._panel);
        }

        protected virtual void ClearChildren()
        {
            this._panel.Children.Clear();
        }

        protected virtual void AddChild(
            IElementRenderer childRenderer,
            DockRegion dockRegion)
        {
            var nativeChild = (FrameworkElement)childRenderer.NativeElement;
            NativeDockPanel.SetDock(nativeChild, ToXamlDockRegion(dockRegion));
            this._panel.Children.Add(nativeChild);
        }

        protected virtual void InsertChild(
            int index,
            IElementRenderer childRenderer,
            DockRegion dockRegion)
        {
            var nativeChild = (FrameworkElement)childRenderer.NativeElement;
            NativeDockPanel.SetDock(nativeChild, ToXamlDockRegion(dockRegion));
            this._panel.Children.Insert(index, nativeChild);
        }

        protected virtual void RemoveChildAt(
            int index)
        {
            this._panel.Children.RemoveAt(index);
        }

        protected virtual void ReplaceChild(
            int index,
            IElementRenderer childRenderer,
            DockRegion dockRegion)
        {
            var nativeChild = (FrameworkElement)childRenderer.NativeElement;
            NativeDockPanel.SetDock(nativeChild, ToXamlDockRegion(dockRegion));
            this._panel.Children[index] = nativeChild;
        }

        void IDockLayoutRenderer.ClearChildren()
        {
            this.ClearChildren();
        }

        void IDockLayoutRenderer.AddChild(
            IElementRenderer childRenderer,
            DockRegion dockRegion)
        {
            this.AddChild(childRenderer, dockRegion);
        }

        void IDockLayoutRenderer.InsertChild(
            int index,
            IElementRenderer childRenderer,
            DockRegion dockRegion)
        {
            this.InsertChild(index, childRenderer, dockRegion);
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
            this.ReplaceChild(index, childRenderer, dockRegion);
        }

        private static NativeDockRegion ToXamlDockRegion(
            DockRegion dockRegion)
        {
            NativeDockRegion xamlDockRegion;

            if (dockRegion == DockRegion.CenterOverlay)
            {
                xamlDockRegion = NativeDockRegion.CenterOverlay;
            }
            else if (dockRegion == DockRegion.Left)
            {
                xamlDockRegion = NativeDockRegion.Left;
            }
            else if (dockRegion == DockRegion.LeftOverlay)
            {
                xamlDockRegion = NativeDockRegion.LeftOverlay;
            }
            else if (dockRegion == DockRegion.Top)
            {
                xamlDockRegion = NativeDockRegion.Top;
            }
            else if (dockRegion == DockRegion.TopOverlay)
            {
                xamlDockRegion = NativeDockRegion.TopOverlay;
            }
            else if (dockRegion == DockRegion.Right)
            {
                xamlDockRegion = NativeDockRegion.Right;
            }
            else if (dockRegion == DockRegion.RightOverlay)
            {
                xamlDockRegion = NativeDockRegion.RightOverlay;
            }
            else if (dockRegion == DockRegion.Bottom)
            {
                xamlDockRegion = NativeDockRegion.Bottom;
            }
            else if (dockRegion == DockRegion.BottomOverlay)
            {
                xamlDockRegion = NativeDockRegion.BottomOverlay;
            }
            else
            {
                throw new NotSupportedException();
            }

            return xamlDockRegion;
        }
    }
}
