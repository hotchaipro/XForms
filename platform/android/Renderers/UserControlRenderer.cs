using System;
using XForms.Controls;

namespace XForms.Android.Renderers
{
    public class UserControlRenderer : ControlRenderer<OwnerDrawControl>, IUserControlRenderer
    {
        private OwnerDrawControl _nativeOwnerDrawControl;

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
            this._nativeOwnerDrawControl.RemoveAllViews();

            var nativeContent = (global::Android.Views.View)content?.NativeElement;
            if (null != nativeContent)
            {
                this._nativeOwnerDrawControl.AddView(nativeContent);
            }
        }
    }
}
