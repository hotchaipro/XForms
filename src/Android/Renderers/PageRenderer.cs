using System;
using XForms.Android;
using XForms.Layouts;
using Android.Graphics.Drawables;
using AndroidFrameLayout = global::Android.Widget.FrameLayout;

namespace XForms.Android.Renderers
{
    public class PageRenderer : ViewRenderer<AndroidFrameLayout>, IPageRenderer
    {
        private AndroidFrameLayout _nativePage;

        public PageRenderer(
            global::Android.Content.Context context,
            Page page)
            : base(context, page)
        {
            this._nativePage = new AndroidFrameLayout(context);
            this._nativePage.SetClipChildren(false);
            this._nativePage.SetClipToPadding(false);

            this.SetNativeElement(this._nativePage);

#if DEBUG_LAYOUT
            this._nativePage.SetWillNotDraw(false);
            this._nativePage.SetBackgroundColor(new global::Android.Graphics.Color(0xff, 0, 0, 0x80));
#endif
        }

        public void SetLayout(
            ILayoutRenderer layoutRenderer)
        {
            var layout = ((global::Android.Views.ViewGroup)layoutRenderer.NativeElement);
            this._nativePage.AddView(layout);

            // NOTE: The layout already has AndroidBaseLayoutParameters defined, so 
            // do not overwrite them here.
        }
    }
}
