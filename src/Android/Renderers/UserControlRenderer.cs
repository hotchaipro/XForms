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

        public void Invalidate()
        {
            this._nativeOwnerDrawControl.Invalidate();
        }

        void IUserControlRenderer.SetContent(
            IViewRenderer content)
        {
            var nativeContent = (global::Android.Views.View)content.NativeElement;

            this._nativeOwnerDrawControl.AddView(nativeContent);
        }
    }
}
