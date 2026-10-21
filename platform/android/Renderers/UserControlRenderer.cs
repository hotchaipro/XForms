using System;
using XForms.Controls;
using XForms.Platform;

namespace XForms.Android.Renderers
{
    public class UserControlRenderer : ControlRenderer<OwnerDrawControl>, IUserControlRenderer
    {
        private OwnerDrawControl _nativeOwnerDrawControl;
        private global::Android.Views.View _content;

        public UserControlRenderer(
            global::Android.Content.Context context,
            UserControl userControl)
            : base(context, userControl)
        {
            this._nativeOwnerDrawControl = new OwnerDrawControl(context, userControl);

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
                // so the native view stays transparent (as on Win32 and iOS)
            }
        }

        public void Invalidate()
        {
            this._nativeOwnerDrawControl.Invalidate();
        }

        void IUserControlRenderer.SetContent(
            IViewRenderer content)
        {
            // NOTE: Leave views hosted for lightweight content in place
            if (null != this._content)
            {
                this._nativeOwnerDrawControl.RemoveView(this._content);
            }

            this._content = (global::Android.Views.View)content?.NativeElement;
            if (null != this._content)
            {
                this._nativeOwnerDrawControl.AddView(this._content, 0);
            }

            this._nativeOwnerDrawControl.Invalidate();
        }
    }
}
