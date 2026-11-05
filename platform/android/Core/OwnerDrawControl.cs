using System;
using Android.Graphics;
using XForma.Primitives;
using XForma.Platform;

namespace XForma.Android
{
    internal sealed class OwnerDrawControl : NativeLayoutView
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

                XForma.Rect bounds = new XForma.Rect(
                    XForma.Android.NativeConversions.PixelsToDevicePixels(this.Context, drawingRect.Left),
                    XForma.Android.NativeConversions.PixelsToDevicePixels(this.Context, drawingRect.Top),
                    XForma.Android.NativeConversions.PixelsToDevicePixels(this.Context, drawingRect.Width()),
                    XForma.Android.NativeConversions.PixelsToDevicePixels(this.Context, drawingRect.Height()));

                var drawContext = new DrawContext(this.Context, canvas);
                LayerHost.For(this._drawDelegate).Render(drawContext, bounds);
            }
        }
    }
}
