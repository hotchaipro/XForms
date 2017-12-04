using System;
using XForms.Controls;
using AndroidFrameLayout = global::Android.Widget.FrameLayout;

namespace XForms.Android.Renderers
{
    public class ContentControlRenderer : ControlRenderer<AndroidFrameLayout>, IContentControlRenderer
    {
        private AndroidFrameLayout _nativeContentControl;

        public ContentControlRenderer(
            global::Android.Content.Context context,
            ContentControl customControl)
            : base(context, customControl)
        {
            this._nativeContentControl = new AndroidFrameLayout(context);

#if DEBUG_LAYOUT
            this._nativeContentControl.SetWillNotDraw(false);
            this._nativeContentControl.SetBackgroundColor(new global::Android.Graphics.Color(0x80, 0xff, 0, 0));
#endif

            this.SetNativeElement(this._nativeContentControl);
        }

        void IContentControlRenderer.SetContent(
            IViewRenderer content)
        {
            var nativeChildView = (global::Android.Views.View)content.NativeElement;

            this._nativeContentControl.RemoveAllViews();
            this._nativeContentControl.AddView(nativeChildView);
        }
    }
}
