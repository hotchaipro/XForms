using System;
using UIKit;
using XForms.Controls;

namespace XForms.iOS.Renderers
{
    internal class UserControlRenderer : ControlRenderer<OwnerDrawControl>, IUserControlRenderer
    {
        private OwnerDrawControl _nativeOwnerDrawControl;

        public UserControlRenderer(
            UserControl userControl)
            : base(userControl)
        {
            this._nativeOwnerDrawControl = new OwnerDrawControl(userControl);

            this.SetNativeElement(this._nativeOwnerDrawControl);
        }

        public override Color BackgroundColor
        {
            get
            {
                return base.BackgroundColor;
            }

            set
            {
                // NOTE: The control paints its own background in DrawBackground,
                // so the native view stays transparent
            }
        }

        public void Invalidate()
        {
            this._nativeOwnerDrawControl.SetNeedsDisplay();
        }

        void IUserControlRenderer.SetContent(
            IViewRenderer content)
        {
            foreach (var subview in this._nativeOwnerDrawControl.Subviews)
            {
                subview.RemoveFromSuperview();
            }

            var nativeContent = (UIView)content?.NativeElement;
            if (null != nativeContent)
            {
                this._nativeOwnerDrawControl.AddSubview(nativeContent);
            }
        }
    }
}
