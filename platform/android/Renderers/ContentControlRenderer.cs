using System;
using XForms.Controls;

namespace XForms.Android.Renderers
{
    public class ContentControlRenderer : ControlRenderer<NativeLayoutView>, IContentControlRenderer
    {
        private NativeLayoutView _nativeContentControl;

        public ContentControlRenderer(
            global::Android.Content.Context context,
            ContentControl customControl)
            : base(context, customControl)
        {
            this._nativeContentControl = new NativeLayoutView(context, customControl);

#if DEBUG_LAYOUT
            this._nativeContentControl.SetWillNotDraw(false);
            this._nativeContentControl.SetBackgroundColor(new global::Android.Graphics.Color(0x80, 0xff, 0, 0));
#endif

            this.SetNativeElement(this._nativeContentControl);
        }

        void IContentControlRenderer.SetContent(
            IViewRenderer content)
        {
            this._nativeContentControl.RemoveAllViews();

            var nativeChildView = (global::Android.Views.View)content?.NativeElement;
            if (null != nativeChildView)
            {
                this._nativeContentControl.AddView(nativeChildView);
            }
        }
    }
}
