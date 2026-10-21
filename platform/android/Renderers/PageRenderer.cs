using System;
using XForms.Android;
using XForms.Layouts;
using Android.Graphics.Drawables;
using XForms.Platform;

namespace XForms.Android.Renderers
{
    public class PageRenderer : ViewRenderer<NativeLayoutView>, IPageRenderer
    {
        private NativeLayoutView _nativePage;
        private global::Android.Views.View _layout;

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
            // NOTE: A lightweight layout has no native view; the page draws it
            // and hosts the native views inside it
            if (null != this._layout)
            {
                this._nativePage.RemoveView(this._layout);
            }

            this._layout = (global::Android.Views.View)layoutRenderer.NativeElement;
            if (null != this._layout)
            {
                this._nativePage.AddView(this._layout, 0);
            }

            this._nativePage.Invalidate();
        }
    }
}
