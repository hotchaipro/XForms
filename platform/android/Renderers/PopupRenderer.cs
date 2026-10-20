using System;
using Android.Graphics.Drawables;
using Android.Views;
using AndroidPopupWindow = global::Android.Widget.PopupWindow;
using XForms.Controls;

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
            if (this.PlacementTarget?.Renderer.NativeElement is global::Android.Views.View anchor)
            {
                int gap = (int)NativeConversions.DevicePixelsToPixels(this.NativeContext, AnchorGap);
                this._nativePopup.ShowAsDropDown(anchor, 0, gap);
                return;
            }

            var parentView = ((ApplicationRenderer)XForms.Application.Current.Renderer).Activity.Window.DecorView.RootView;
            this._nativePopup.ShowAtLocation(parentView, GravityFlags.Center, 0, 0);
        }

        public void Close()
        {
            this._nativePopup.Dismiss();
        }
    }
}
