using System;
using Android.Views;
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
        }

        void IFrameRenderer.Push(
            Page page)
        {
        }

        public void SetContent(
            Page newPage,
            bool hideCurrentPage)
        {
            var nativePage = newPage.Renderer.NativeElement as global::Android.Views.View;
            this._nativeFrame.RemoveAllViews();
            this._nativeFrame.AddView(nativePage, new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent));
        }
    }
}
