using System;
using Android.Graphics;
using XForms.Controls;

namespace XForms.Android
{
    public sealed class OwnerDrawControl : NativeLayoutView
    {
        private IDrawDelegate _drawDelegate;

        internal OwnerDrawControl(
            global::Android.Content.Context context,
            UserControl userControl)
            : base(context, userControl)
        {
            this._drawDelegate = userControl;

            // A view group skips its own drawing unless told otherwise
            this.SetWillNotDraw(false);
        }

        protected override void OnDraw(
            Canvas canvas)
        {
            base.OnDraw(canvas);

            var drawingRect = new Rect();
            this.GetDrawingRect(drawingRect);

            if ((drawingRect.Width() > 0) && drawingRect.Height() > 0)
            {
                // NOTE: ClipRect must be explicitly set on every OnDraw call
                canvas.ClipRect(drawingRect);

                Rectangle bounds = new Rectangle(
                    XForms.Android.NativeConversions.PixelsToDevicePixels(this.Context, drawingRect.Left),
                    XForms.Android.NativeConversions.PixelsToDevicePixels(this.Context, drawingRect.Top),
                    XForms.Android.NativeConversions.PixelsToDevicePixels(this.Context, drawingRect.Width()),
                    XForms.Android.NativeConversions.PixelsToDevicePixels(this.Context, drawingRect.Height()));

                var drawContext = new DrawContext(this.Context, canvas);
                this._drawDelegate.InvokeDrawBackground(drawContext, bounds);
            }
        }
    }
}
