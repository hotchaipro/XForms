using System;
using CoreGraphics;
using UIKit;
using XForms.Controls;

namespace XForms.iOS
{
    public sealed class DrawContext : IDrawContext
    {
        private static CGFont[] FontRamp;
        private static readonly object InitSyncObject = new object();

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
            this._graphicsContext.DrawImage(rect, nativeImage.CGImage);

            if (!tint.IsTransparent)
            {
                this._graphicsContext.SetBlendMode(CGBlendMode.SourceIn);
                this._graphicsContext.SetFillColor(tint.ToCGColor());
                this._graphicsContext.FillRect(rect);
                this._graphicsContext.SetBlendMode(CGBlendMode.Normal);
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

            var font = GetCGFontFromTextFormat(textFormat);
            this._graphicsContext.SetFont(font);
            this._graphicsContext.SetFontSize(textFormat.FontSize);

            float originX;
            if (textFormat.HorizontalAlignment == LayoutAlignment.Start)
            {
                originX = 0;
            }
            else if (textFormat.HorizontalAlignment == LayoutAlignment.Center)
            {
                originX = 0.5f;
            }
            else if (textFormat.HorizontalAlignment == LayoutAlignment.End)
            {
                originX = 1.0f;
            }
            else if (textFormat.HorizontalAlignment == LayoutAlignment.Fill)
            {
                originX = 0.5f;
            }
            else
            {
                throw new NotSupportedException("Unsupported alignment.");
            }

            float originY;
            if (textFormat.VerticalAlignment == LayoutAlignment.Start)
            {
                originY = 0;
            }
            else if (textFormat.VerticalAlignment == LayoutAlignment.Center)
            {
                originY = 0.5f;
            }
            else if (textFormat.VerticalAlignment == LayoutAlignment.End)
            {
                originY = 1.0f;
            }
            else if (textFormat.VerticalAlignment == LayoutAlignment.Fill)
            {
                originY = 0.5f;
            }
            else
            {
                throw new NotSupportedException("Unsupported alignment.");
            }

            float x, y;

            if ((originX == 0) && (originY == 0))
            {
                x = (float)rect.Left;
                y = (float)rect.Top;
            }
            else
            {
                // TODO: NSStringDrawing.DrawString(, , new UIStringAttributes() { })

                // Measure text
                this._graphicsContext.SetTextDrawingMode(CGTextDrawingMode.Invisible);
                var startingPoint = this._graphicsContext.TextPosition;
                this._graphicsContext.ShowText(text);
                var endingPoint = this._graphicsContext.TextPosition;
                this._graphicsContext.SetTextDrawingMode(CGTextDrawingMode.Fill);
                var textBounds = new CGRect(startingPoint.X, startingPoint.Y, endingPoint.X - startingPoint.X, endingPoint.Y - startingPoint.Y);

                x = ((float)rect.Width - (float)textBounds.Width * originX) + (float)rect.Left;
                y = ((float)rect.Height - (float)textBounds.Height * originY) + (float)rect.Top;
            }

            this._graphicsContext.SetFillColor(color.ToCGColor());
            this._graphicsContext.ShowTextAtPoint(x, y, text);
        }

        private static void InitTypefaceWeightRamp()
        {
            lock (InitSyncObject)
            {
                if (null == FontRamp)
                {
                   CGFont[] fontRamp = new CGFont[5];

                    // Weight 100

                    fontRamp[0] = CGFont.CreateWithFontName("AvenirNext-UltraLight");

                    // Weight 300

                    fontRamp[1] = CGFont.CreateWithFontName("AvenirNext-Regular");

                    // Weight 400

                    fontRamp[2] = CGFont.CreateWithFontName("AvenirNext-Medium");

                    // Weight 500

                    fontRamp[3] = CGFont.CreateWithFontName("AvenirNext-DemiBold");

                    // Weight 700

                    fontRamp[4] = CGFont.CreateWithFontName("AvenirNext-Bold");

                    FontRamp = fontRamp;
                }
            }
        }

        private CGFont GetCGFontFromTextFormat(
            TextFormat textFormat)
        {
            if (null == textFormat)
            {
                throw new ArgumentNullException(nameof(textFormat));
            }

            if (null == FontRamp)
            {
                InitTypefaceWeightRamp();
            }

            CGFont uiFont;

            if (textFormat.FontWeight == FontWeight.Light)
            {
                uiFont = FontRamp[0];
            }
            else if (textFormat.FontWeight == FontWeight.Normal)
            {
                uiFont = FontRamp[1];
            }
            else if (textFormat.FontWeight == FontWeight.Medium)
            {
                uiFont = FontRamp[2];
            }
            else if (textFormat.FontWeight == FontWeight.SemiBold)
            {
                uiFont = FontRamp[3];
            }
            else if (textFormat.FontWeight == FontWeight.Bold)
            {
                uiFont = FontRamp[4];
            }
            else
            {
                throw new NotSupportedException("Unsupported font weight.");
            }

            return uiFont;
        }
    }
}
