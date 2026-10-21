using System;
using Android.Graphics;
using XForms.Controls;
using XForms.Platform;

namespace XForms.Android
{
    public sealed class OwnerDrawControl : NativeLayoutView
    {
        private View _drawDelegate;

        internal OwnerDrawControl(
            global::Android.Content.Context context,
            View view)
            : base(context, view)
        {
            this._drawDelegate = view;

            // A view group skips its own drawing unless told otherwise
            this.SetWillNotDraw(false);
        }

        protected override void OnDraw(
            Canvas canvas)
        {
            base.OnDraw(canvas);

            var drawingRect = new global::Android.Graphics.Rect();
            this.GetDrawingRect(drawingRect);

            if ((drawingRect.Width() > 0) && drawingRect.Height() > 0)
            {
                // NOTE: ClipRect must be explicitly set on every OnDraw call
                canvas.ClipRect(drawingRect);

                XForms.Rect bounds = new XForms.Rect(
                    XForms.Android.NativeConversions.PixelsToDevicePixels(this.Context, drawingRect.Left),
                    XForms.Android.NativeConversions.PixelsToDevicePixels(this.Context, drawingRect.Top),
                    XForms.Android.NativeConversions.PixelsToDevicePixels(this.Context, drawingRect.Width()),
                    XForms.Android.NativeConversions.PixelsToDevicePixels(this.Context, drawingRect.Height()));

                var drawContext = new DrawContext(this.Context, canvas);
                LayerHost.For(this._drawDelegate).Render(drawContext, bounds);
            }
        }
    }
}
