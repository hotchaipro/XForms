using System;
using UIKit;
using XForms.Controls;

namespace XForms.iOS.Renderers
{
    internal class ContentControlRenderer : ControlRenderer<NativeLayoutView>, IContentControlRenderer
    {
        private NativeLayoutView _nativeContentControl;

        public ContentControlRenderer(
            ContentControl contentControl)
            : base(contentControl)
        {
            this._nativeContentControl = new NativeLayoutView(contentControl);

            this.SetNativeElement(this._nativeContentControl);
        }

        void IContentControlRenderer.SetContent(
            IViewRenderer content)
        {
            // NOTE: Leave views hosted for lightweight content in place
            this._content?.RemoveFromSuperview();
            this._content = (UIView)content?.NativeElement;
            if (null != this._content)
            {
                this._nativeContentControl.InsertSubview(this._content, 0);
            }

            this._nativeContentControl.SetNeedsDisplay();
        }

        private UIView _content;
    }
}
