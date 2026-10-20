using System;
using CoreGraphics;
using Foundation;
using UIKit;
using XForms.Controls;

using XForms.iOS.Renderers;

namespace XForms.iOS
{
    public sealed class DrawContext : IDrawContext
    {
        private CGContext _graphicsContext;
        private CGRect _bounds;

        public DrawContext(
            CGContext graphicsContext,
            CGRect bounds)
        {
            if (null == graphicsContext)
            {
                throw new ArgumentNullException(nameof(graphicsContext));
            }

            this._graphicsContext = graphicsContext;
            this._bounds = bounds;
        }

        public void Clear(
            Color color)
        {
            this._graphicsContext.ClearRect(this._bounds);

            if (!color.IsTransparent)
            {
                this._graphicsContext.SetFillColor(color.ToCGColor());
                this._graphicsContext.FillRect(this._bounds);
            }
        }

        public void DrawRectangle(
            Rectangle rectangle,
            Color color,
            float strokeWidth = 1.0f)
        {
            var rect = rectangle.ToCGRect();

            this._graphicsContext.SetStrokeColor(color.ToCGColor());
            this._graphicsContext.StrokeRectWithWidth(rect, strokeWidth);
        }

        public void FillRectangle(
            Rectangle rectangle,
            Color color)
        {
            var rect = rectangle.ToCGRect();
            this._graphicsContext.SetFillColor(color.ToCGColor());
            this._graphicsContext.FillRect(rect);
        }

        public void DrawEllipse(
            Point centerPoint,
            float radiusX,
            float radiusY,
            Color color,
            float strokeWidth = 1.0f)
        {
            var rect = new Rectangle(
                centerPoint.X - radiusX,
                centerPoint.Y - radiusY,
                2 * radiusX,
                2 * radiusY);

            this.DrawEllipse(rect, color, strokeWidth);
        }

        public void DrawEllipse(
            Rectangle rectangle,
            Color color,
            float strokeWidth = 1.0f)
        {
            var rect = rectangle.ToCGRect();
            this._graphicsContext.SetStrokeColor(color.ToCGColor());
            this._graphicsContext.StrokeEllipseInRect(rect);
        }

        public void FillEllipse(
            Point centerPoint,
            float radiusX,
            float radiusY,
            Color color)
        {
            var rect = new Rectangle(
                centerPoint.X - radiusX,
                centerPoint.Y - radiusY,
                2 * radiusX,
                2 * radiusY);

            this.FillEllipse(rect, color);
        }

        public void FillEllipse(
            Rectangle rectangle,
            Color color)
        {
            var rect = rectangle.ToCGRect();
            this._graphicsContext.SetFillColor(color.ToCGColor());
            this._graphicsContext.FillEllipseInRect(rect);
        }

        public void DrawImage(
            Bitmap bitmap,
            Rectangle destinationRectangle,
            Color tint)
        {
            if (null == bitmap)
            {
                throw new ArgumentNullException(nameof(bitmap));
            }

            var rect = destinationRectangle.ToCGRect();
            var nativeImage = ((BitmapRenderer)bitmap.Renderer).NativeImage;

            if (tint.IsTransparent)
            {
                // NOTE: UIImage.Draw accounts for the flipped UIKit coordinate system
                nativeImage.Draw(rect);
            }
            else
            {
                // Use the image as a mask so only its opaque pixels take the tint
                this._graphicsContext.SaveState();
                this._graphicsContext.TranslateCTM(rect.X, rect.Y + rect.Height);
                this._graphicsContext.ScaleCTM(1, -1);
                var maskRect = new CGRect(0, 0, rect.Width, rect.Height);
                this._graphicsContext.ClipToMask(maskRect, nativeImage.CGImage);
                this._graphicsContext.SetFillColor(tint.ToCGColor());
                this._graphicsContext.FillRect(maskRect);
                this._graphicsContext.RestoreState();
            }
        }

        // TODO: Replace with a Shadow property on View
        public void FillEllipseShadow(
            Rectangle rectangle,
            Color color,
            float shadowBlur = 8.0f)
        {
            rectangle = rectangle.Inflate(-2 * shadowBlur, -2 * shadowBlur);

            var rect = rectangle.ToCGRect();

            //this._graphicsContext.SaveState();
            this._graphicsContext.SetShadow(new CGSize(shadowBlur, shadowBlur), shadowBlur, color.ToCGColor());
            this._graphicsContext.SetFillColor(color.ToCGColor());
            this._graphicsContext.FillEllipseInRect(rect);
            this._graphicsContext.SetShadow(CGSize.Empty, 0, null);
            //this._graphicsContext.RestoreState();
        }

        public void DrawLine(
            Point start,
            Point end,
            Color color,
            float strokeWidth = 1.0f)
        {
            this._graphicsContext.SaveState();
            this._graphicsContext.SetStrokeColor(color.ToCGColor());
            this._graphicsContext.SetLineWidth(strokeWidth);
            this._graphicsContext.SetLineCap(CGLineCap.Round);
            this._graphicsContext.MoveTo(start.X, start.Y);
            this._graphicsContext.AddLineToPoint(end.X, end.Y);
            this._graphicsContext.StrokePath();
            this._graphicsContext.RestoreState();
        }

        public void DrawArc(
            Point center,
            float radius,
            float startDegrees,
            float sweepDegrees,
            Color color,
            float strokeWidth = 1.0f)
        {
            // NOTE: Angles start at 3 o'clock; in UIKit's flipped coordinates,
            // increasing angles (clockwise: false) turn clockwise on screen
            double start = (startDegrees - 90) * Math.PI / 180;
            double end = start + (sweepDegrees * Math.PI / 180);

            this._graphicsContext.SaveState();
            this._graphicsContext.SetStrokeColor(color.ToCGColor());
            this._graphicsContext.SetLineWidth(strokeWidth);
            this._graphicsContext.SetLineCap(CGLineCap.Round);
            this._graphicsContext.AddArc(center.X, center.Y, radius, (nfloat)start, (nfloat)end, false);
            this._graphicsContext.StrokePath();
            this._graphicsContext.RestoreState();
        }

        /// <summary>
        /// The size of a single line of text, as <see cref="DrawText"/> draws it.
        /// </summary>
        internal static Size MeasureText(
            string text,
            TextFormat textFormat)
        {
            var font = UIFont.SystemFontOfSize(textFormat.FontSize, textFormat.FontWeight.ToUIFontWeight());
            if (string.IsNullOrEmpty(text))
            {
                return new Size(0, (float)Math.Ceiling(font.LineHeight));
            }

            var attributedText = new NSAttributedString(text, new UIStringAttributes() { Font = font });
            var size = attributedText.Size;
            return new Size((float)Math.Ceiling(size.Width), (float)Math.Ceiling(size.Height));
        }

        public void DrawText(
            string text,
            Rectangle bounds,
            Color color,
            TextFormat textFormat)
        {
            if (null == textFormat)
            {
                throw new ArgumentNullException(nameof(textFormat));
            }

            if (String.IsNullOrEmpty(text))
            {
                return;
            }

            var rect = bounds.ToCGRect();

            var paragraphStyle = new NSMutableParagraphStyle()
            {
                Alignment = textFormat.HorizontalAlignment.ToUITextAlignment(),
                LineBreakMode = UILineBreakMode.WordWrap,
            };

            var attributes = new UIStringAttributes()
            {
                Font = UIFont.SystemFontOfSize(textFormat.FontSize, textFormat.FontWeight.ToUIFontWeight()),
                ForegroundColor = color.ToUIColor(),
                ParagraphStyle = paragraphStyle,
            };

            var attributedText = new NSAttributedString(text, attributes);

            // Position the text block vertically; horizontal alignment is handled
            // by the paragraph style
            var textSize = attributedText.GetBoundingRect(
                new CGSize(rect.Width, nfloat.MaxValue),
                NSStringDrawingOptions.UsesLineFragmentOrigin | NSStringDrawingOptions.UsesFontLeading,
                null).Size;

            nfloat textHeight = (nfloat)Math.Min(Math.Ceiling(textSize.Height), rect.Height);
            nfloat y;
            switch (textFormat.VerticalAlignment)
            {
                case LayoutAlignment.Center:
                case LayoutAlignment.Fill:
                    y = rect.Y + ((rect.Height - textHeight) / 2);
                    break;

                case LayoutAlignment.End:
                    y = rect.Bottom - textHeight;
                    break;

                default:
                    y = rect.Y;
                    break;
            }

            attributedText.DrawString(
                new CGRect(rect.X, y, rect.Width, textHeight),
                NSStringDrawingOptions.UsesLineFragmentOrigin | NSStringDrawingOptions.TruncatesLastVisibleLine,
                null);
        }
    }
}
