using System;
using UIKit;
using XForms.Controls;

namespace XForms.iOS
{
    internal sealed class OwnerDrawControl : UIView
    {
        private IDrawDelegate _drawDelegate;

        internal OwnerDrawControl(
            IDrawDelegate drawDelegate)
        {
            if (null == drawDelegate)
            {
                throw new ArgumentNullException(nameof(drawDelegate));
            }

            this._drawDelegate = drawDelegate;
        }

        public override void DrawRect(
            CoreGraphics.CGRect area,
            UIViewPrintFormatter formatter)
        {
            base.DrawRect(area, formatter);

            if ((area.Width > 0) && area.Height > 0)
            {
                Rectangle bounds = new Rectangle((float)area.X, (float)area.Y, (float)area.Width, (float)area.Height);

                var graphicsContext = UIGraphics.GetCurrentContext();

                var drawContext = new NativeDrawContext(graphicsContext, area);

                this._drawDelegate.InvokeDrawBackground(drawContext, bounds);
            }
        }
    }
}
