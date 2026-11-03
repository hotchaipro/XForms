using System;
using Android.Views;
using XForms.Platform;
using AndroidFrameLayout = global::Android.Widget.FrameLayout;

namespace XForms.Android.Renderers
{
    internal class FrameRenderer : ViewRenderer<AndroidFrameLayout>, IFrameRenderer
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
            Root = frame;
            RootNativeFrame = this._nativeFrame;

            // The root holds keyboard focus when nothing else does (at start,
            // and after a tap), as on iOS, so Tab starts in tab order: Android
            // would otherwise give focus to its first child (the drawing
            // order) or search by position, and use up the Tab doing it
            this._nativeFrame.Focusable = true;
            this._nativeFrame.FocusableInTouchMode = true;
            this._nativeFrame.DescendantFocusability = DescendantFocusability.BeforeDescendants;
            if (OperatingSystem.IsAndroidVersionAtLeast(26))
            {
                this._nativeFrame.DefaultFocusHighlightEnabled = false;
            }

            // The root frame is the window's content: report its size
            this._nativeFrame.LayoutChange += (sender, e) => LayerHost.For(frame).NotifyRootSize(new Size(
                this.PixelsToDevicePixels(e.Right - e.Left),
                this.PixelsToDevicePixels(e.Bottom - e.Top)));
        }

        /// <summary>
        /// The window's root frame (Android shows one window).
        /// </summary>
        internal static Frame Root
        {
            get;
            private set;
        }

        internal static AndroidFrameLayout RootNativeFrame
        {
            get;
            private set;
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
