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
            foreach (var subview in this._nativeContentControl.Subviews)
            {
                subview.RemoveFromSuperview();
            }

            var nativeContent = (UIView)content?.NativeElement;
            if (null != nativeContent)
            {
                this._nativeContentControl.AddSubview(nativeContent);
            }
        }
    }
}
