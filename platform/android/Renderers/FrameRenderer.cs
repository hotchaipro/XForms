using System;
using Android.Views;
using XForms.Platform;
using AndroidFrameLayout = global::Android.Widget.FrameLayout;

namespace XForms.Android.Renderers
{
    public class FrameRenderer : ViewRenderer<AndroidFrameLayout>, IFrameRenderer
    {
        private AndroidFrameLayout _nativeFrame;

        public FrameRenderer(
            global::Android.Content.Context context,
            Frame frame)
            : base(context, frame)
        {
            this._nativeFrame = new AndroidFrameLayout(context);

#if DEBUG_LAYOUT
            this._nativeFrame.SetWillNotDraw(false);
            this._nativeFrame.SetBackgroundColor(global::Android.Graphics.Color.Yellow);
#endif

            this.SetNativeElement(this._nativeFrame);

            // The root frame is the window's content: report its size
            this._nativeFrame.LayoutChange += (sender, e) => LayerHost.For(frame).NotifyRootSize(new Size(
                this.PixelsToDevicePixels(e.Right - e.Left),
                this.PixelsToDevicePixels(e.Bottom - e.Top)));
        }

        // Pages stay in the frame (hidden when not showing), so transitions can
        // show two at once
        public void AddPage(
            Page page)
        {
            var nativePage = (global::Android.Views.View)page.Renderer.NativeElement;
            if (nativePage.Parent != this._nativeFrame)
            {
                this._nativeFrame.AddView(nativePage, new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent));
            }
        }

        public void RemovePage(
            Page page)
        {
            this._nativeFrame.RemoveView((global::Android.Views.View)page.Renderer.NativeElement);
        }

        public void BringToFront(
            Page page)
        {
            var nativePage = (global::Android.Views.View)page.Renderer.NativeElement;
            this._nativeFrame.BringChildToFront(nativePage);
            this._nativeFrame.Invalidate();
        }
    }
}
