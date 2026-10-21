using System;
using CoreGraphics;
using UIKit;
using XForms.Controls;
using XForms.Platform;

namespace XForms.iOS
{
    /// <summary>
    /// Native host for a custom-drawn control: draws through the control's
    /// <see cref="IDrawDelegate"/> and lays out the control's content on top.
    /// </summary>
    internal sealed class OwnerDrawControl : NativeLayoutView
    {
        private UserControl _drawDelegate;

        internal OwnerDrawControl(
            UserControl userControl)
            : base(userControl)
        {
            this._drawDelegate = userControl;

            this.Opaque = false;
            this.BackgroundColor = UIColor.Clear;
            this.ContentMode = UIViewContentMode.Redraw;
        }

        public override void Draw(
            CGRect area)
        {
            var bounds = this.Bounds;
            if ((bounds.Width > 0) && (bounds.Height > 0))
            {
                var graphicsContext = UIGraphics.GetCurrentContext();

                var drawContext = new DrawContext(graphicsContext, bounds);

                LayerHost.For(this._drawDelegate).Render(drawContext, bounds.ToRectangle());
            }

            // Lightweight content on top of the control's own drawing
            base.Draw(area);
        }
    }
}
