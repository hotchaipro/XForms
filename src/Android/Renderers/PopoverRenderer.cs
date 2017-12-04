using System;
using AndroidPopover = global::Android.Widget.PopupWindow;
using XForms.Controls;

namespace XForms.Android.Renderers
{
    public class PopoverRenderer : ElementRenderer<AndroidPopover>, IPopoverRenderer
    {
        private View _content;
        private AndroidPopover _nativePopoverControl;

        public PopoverRenderer(
            global::Android.Content.Context context,
            Popover popover)
            : base(context, popover)
        {
            this._nativePopoverControl = new AndroidPopover(context)
            {
            };

            this._nativePopoverControl.DismissEvent += _PopoverControl_DismissEvent;

            //this.SetNativeElement(this._nativePopoverControl);
        }

        private void _PopoverControl_DismissEvent(
            object sender,
            EventArgs e)
        {
            ((IPopoverDelegate)this.Element).NotifyClosed();
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

                this._nativePopoverControl.ContentView = value?.Renderer.NativeElement as global::Android.Views.View;
            }
        }

        public void ShowAsync()
        {
            var parentView = ((ApplicationRenderer)XForms.Application.Current.Renderer).Activity.Window.DecorView.RootView;
            this._nativePopoverControl.ShowAtLocation(parentView, global::Android.Views.GravityFlags.Center, 0, 0);
        }

        public void Close()
        {
            this._nativePopoverControl.Dismiss();
        }

        //private sealed class PopupDismissListener : Java.Lang.Object, PopupMenu.IOnDismissListener
        //{
        //    public void OnDismiss(
        //        PopupMenu menu)
        //    {
        //    }
        //}
    }
}
