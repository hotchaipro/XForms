using System;
using Android.Graphics.Drawables;
using Android.Views;
using AndroidPopupWindow = global::Android.Widget.PopupWindow;
using XForms.Primitives;
using XForms.Platform;

namespace XForms.Android.Renderers
{
    /// <summary>
    /// Shows a popup in a PopupWindow: centered in the window, or below (or,
    /// without room, above) its placement target. Touching outside it or
    /// pressing Back dismisses it.
    /// </summary>
    public class PopupRenderer : ElementRenderer<AndroidPopupWindow>, IPopupRenderer
    {
        private const float AnchorGap = 4.0f;

        private View _content;
        private AndroidPopupWindow _nativePopup;

        public PopupRenderer(
            global::Android.Content.Context context,
            Popup popup)
            : base(context, popup)
        {
            this._nativePopup = new AndroidPopupWindow(context)
            {
                Width = ViewGroup.LayoutParams.WrapContent,
                Height = ViewGroup.LayoutParams.WrapContent,

                // Focusable so Back and outside touches dismiss it; the content draws its own look
                Focusable = true,
                OutsideTouchable = true,
            };
            this._nativePopup.SetBackgroundDrawable(new ColorDrawable(global::Android.Graphics.Color.Transparent));

            this._nativePopup.DismissEvent += NativePopup_Dismiss;
        }

        private void NativePopup_Dismiss(
            object sender,
            EventArgs e)
        {
            ((IPopupDelegate)this.Element).NotifyClosed();
        }

        public View Content
        {
            get
            {
                return this._content;
            }

            set
            {
                this._content = value;

                this._nativePopup.ContentView = value?.Renderer.NativeElement as global::Android.Views.View;
            }
        }

        public View PlacementTarget
        {
            get;
            set;
        }

        public void Show()
        {
            var target = this.PlacementTarget;
            int gap = (int)NativeConversions.DevicePixelsToPixels(this.NativeContext, AnchorGap);
            if (target?.Renderer.NativeElement is global::Android.Views.View anchor)
            {
                this._nativePopup.ShowAsDropDown(anchor, 0, gap);
                return;
            }

            // A lightweight target has no native view: place the popup below its
            // place in its layer's view (or above, with more room there)
            if ((null != target) && (null != this._content) && (LayerHost.For(target).Layer?.NativeElement is global::Android.Views.View layer))
            {
                this.ShowAt(layer, target);
                return;
            }

            var parentView = AndroidApplication.Current.Activity.Window.DecorView.RootView;
            this._nativePopup.ShowAtLocation(parentView, GravityFlags.Center, 0, 0);
        }

        private void ShowAt(
            global::Android.Views.View layer,
            View target)
        {
            var context = this.NativeContext;
            float Dips(float pixels) => NativeConversions.PixelsToDevicePixels(context, pixels);
            int Pixels(float dips) => (int)Math.Round(NativeConversions.DevicePixelsToPixels(context, dips));

            var root = AndroidApplication.Current.Activity.Window.DecorView.RootView;
            var layerLocation = new int[2];
            layer.GetLocationInWindow(layerLocation);

            var offset = LayerHost.For(target).OffsetInLayer;
            float left = Dips(layerLocation[0]) + offset.X;
            float top = Dips(layerLocation[1]) + offset.Y;
            float bottom = top + target.Bounds.Height;
            float windowWidth = Dips(root.Width);
            float windowHeight = Dips(root.Height);

            float below = Math.Max(0, windowHeight - bottom - AnchorGap);
            float above = Math.Max(0, top - AnchorGap);

            this._content.Measure(new Size(windowWidth, below));
            var size = this._content.DesiredSize;
            float y = bottom + AnchorGap;
            if ((size.Height > below) && (above > below))
            {
                this._content.Measure(new Size(windowWidth, above));
                size = this._content.DesiredSize;
                y = top - AnchorGap - size.Height;
            }

            float x = Math.Max(0, Math.Min(left, windowWidth - size.Width));
            this._nativePopup.Width = Pixels(size.Width);
            this._nativePopup.Height = Pixels(size.Height);
            this._nativePopup.ShowAtLocation(root, GravityFlags.Top | GravityFlags.Left, Pixels(x), Pixels(y));
        }

        public void Close()
        {
            this._nativePopup.Dismiss();
        }
    }
}
