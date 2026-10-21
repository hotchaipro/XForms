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
            // NOTE: Leave views hosted for lightweight content in place
            this._content?.RemoveFromSuperview();
            this._content = (UIView)content?.NativeElement;
            if (null != this._content)
            {
                this._nativeOwnerDrawControl.InsertSubview(this._content, 0);
            }

            this._nativeOwnerDrawControl.SetNeedsDisplay();
        }

        private UIView _content;
    }
}
