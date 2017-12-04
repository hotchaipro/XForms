using System;
using XForms.Controls;

namespace XForms.Windows.Renderers
{
    public class UserControlRenderer : ControlRenderer, IUserControlRenderer
    {
        private OwnerDrawControl _ownerDrawControl;

        public UserControlRenderer(
            UserControl userControl)
            : base(userControl)
        {
            this._ownerDrawControl = new OwnerDrawControl(userControl)
            {
                IsTabStop = true,
            };
            NativeDockPanel.SetDock(this._ownerDrawControl, NativeDockRegion.CenterOverlay);

            this._ownerDrawControl.Tapped += Control_Tapped;
            this.SetNativeElement(this._ownerDrawControl);
        }

        private void Control_Tapped(
            object sender,
            global::Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            this._ownerDrawControl.Focus(global::Windows.UI.Xaml.FocusState.Pointer);
        }

        public void Invalidate()
        {
            this._ownerDrawControl.Invalidate();
        }

        void IUserControlRenderer.SetContent(
            IViewRenderer content)
        {
            var uiElement = (global::Windows.UI.Xaml.UIElement)content.NativeElement;

            this._ownerDrawControl.Content = uiElement;
        }
    }
}
