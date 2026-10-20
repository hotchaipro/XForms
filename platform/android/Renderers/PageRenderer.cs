using System;
using XForms.Android;
using XForms.Layouts;
using Android.Graphics.Drawables;

namespace XForms.Android.Renderers
{
    public class PageRenderer : ViewRenderer<NativeLayoutView>, IPageRenderer
    {
        private NativeLayoutView _nativePage;

        public PageRenderer(
            global::Android.Content.Context context,
            Page page)
            : base(context, page)
        {
            this._nativePage = new NativeLayoutView(context, page);
            this._nativePage.SetClipChildren(false);
            this._nativePage.SetClipToPadding(false);

            this.SetNativeElement(this._nativePage);

#if DEBUG_LAYOUT
            this._nativePage.SetWillNotDraw(false);
            this._nativePage.SetBackgroundColor(new global::Android.Graphics.Color(0xff, 0, 0, 0x80));
#endif
        }

        public void SetLayout(
            IPanelRenderer layoutRenderer)
        {
            this._nativePage.RemoveAllViews();
            this._nativePage.AddView((global::Android.Views.View)layoutRenderer.NativeElement);
        }
    }
}
