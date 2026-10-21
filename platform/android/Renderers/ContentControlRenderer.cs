using System;
using XForms.Controls;

namespace XForms.Android.Renderers
{
    public class ContentControlRenderer : ControlRenderer<NativeLayoutView>, IContentControlRenderer
    {
        private NativeLayoutView _nativeContentControl;
        private global::Android.Views.View _content;

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
            // NOTE: Leave views hosted for lightweight content in place
            if (null != this._content)
            {
                this._nativeContentControl.RemoveView(this._content);
            }

            this._content = (global::Android.Views.View)content?.NativeElement;
            if (null != this._content)
            {
                this._nativeContentControl.AddView(this._content, 0);
            }

            this._nativeContentControl.Invalidate();
        }
    }
}
