using System;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Effects;
using Microsoft.Graphics.Canvas.Text;
using XForms.Windows.Renderers;

namespace XForms.Windows
{
    public sealed class DrawContext : IDrawContext
    {
        private CanvasDrawingSession _session;

        public DrawContext(
            CanvasDrawingSession session)
        {
            this._session = session;
        }

        public void Clear(
            Color color)
        {
            this._session.Clear(color.ToXamlColor());
        }

        public void DrawRectangle(
            Rectangle rectangle,
            Color color,
            float strokeWidth = 1.0f)
        {
            var winRect = new global::Windows.Foundation.Rect(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
            var winColor = global::Windows.UI.Color.FromArgb(color.A, color.R, color.G, color.B);

            this._session.DrawRectangle(winRect, winColor, strokeWidth);
        }

        public void FillRectangle(
            Rectangle rectangle,
            Color color)
        {
            var winRect = new global::Windows.Foundation.Rect(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
            var winColor = global::Windows.UI.Color.FromArgb(color.A, color.R, color.G, color.B);

            this._session.FillRectangle(winRect, winColor);
        }

        public void DrawEllipse(
            Point centerPoint,
            float radiusX,
            float radiusY,
            Color color,
            float strokeWidth = 1.0f)
        {
            var winPoint = new System.Numerics.Vector2(centerPoint.X, centerPoint.Y);
            var winColor = global::Windows.UI.Color.FromArgb(color.A, color.R, color.G, color.B);

            this._session.DrawEllipse(winPoint, radiusX, radiusY, winColor, strokeWidth, strokeStyle: null);
        }

        public void FillEllipse(
            Point centerPoint,
            float radiusX,
            float radiusY,
            Color color)
        {
            var winPoint = new System.Numerics.Vector2(centerPoint.X, centerPoint.Y);
            var winColor = global::Windows.UI.Color.FromArgb(color.A, color.R, color.G, color.B);

            this._session.FillEllipse(winPoint, radiusX, radiusY, winColor);
        }

        public void FillEllipse(
            Rectangle rectangle,
            Color color)
        {
            var centerPoint = rectangle.Center;
            float radiusX = rectangle.Width / 2;
            float radiusY = rectangle.Height / 2;
            var winColor = global::Windows.UI.Color.FromArgb(color.A, color.R, color.G, color.B);

            this._session.FillEllipse(centerPoint.X, centerPoint.Y, radiusX, radiusY, winColor);
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

            var nativeBitmap = ((BitmapRenderer)bitmap.Renderer).NativeElement;

            this.DrawImage(nativeBitmap, destinationRectangle, tint);
        }

        internal void DrawImage(
            CanvasBitmap nativeBitmap,
            Rectangle destinationRectangle,
            Color tint)
        {
            if (null == nativeBitmap)
            {
                throw new ArgumentNullException(nameof(nativeBitmap));
            }

            var destinationRect = new global::Windows.Foundation.Rect(destinationRectangle.X, destinationRectangle.Y, destinationRectangle.Width, destinationRectangle.Height);

            var sourceRect = new global::Windows.Foundation.Rect(0, 0, nativeBitmap.Size.Width, nativeBitmap.Size.Height);

            if (tint.IsTransparent)
            {
                this._session.DrawImage(nativeBitmap, destinationRect, sourceRect);
            }
            else
            {
                // TODO: An Effect parameter that wraps this matrix so it doesn't have to be created repeatedly
                ColorMatrixEffect tintEffect = new ColorMatrixEffect();
                tintEffect.Source = nativeBitmap;
                tintEffect.ColorMatrix = new Matrix5x4()
                {
                    M11 = (float)tint.R / 255f, M12 = 0, M13 = 0, M14 = 0,
                    M21 = 0, M22 = (float)tint.G / 255f, M23 = 0, M24 = 0,
                    M31 = 0, M32 = 0, M33 = (float)tint.B / 255f, M34 = 0,
                    M41 = 0, M42 = 0, M43 = 0, M44 = 1.0f,
                    M51 = 0, M52 = 0, M53 = 0, M54 = 0,
                };

                this._session.DrawImage(tintEffect, destinationRect, sourceRect);
            }
        }

        // TODO: Create a ShadowEffect : Effect that can be used as a parameter to add a shadow 
        // to other draw functions.
        public void FillEllipseShadow(
            Rectangle rectangle,
            Color color,
            float shadowBlur = 8.0f)
        {
            rectangle = rectangle.Inflate(-2 * shadowBlur, -2 * shadowBlur);

            var centerPoint = rectangle.Center;
            float radiusX = rectangle.Width / 2;
            float radiusY = rectangle.Height / 2f;
            var winColor = global::Windows.UI.Color.FromArgb(color.A, color.R, color.G, color.B);

            CanvasCommandList commandList = new CanvasCommandList(GraphicsManager.Shared.GetCanvasDevice());
            using (var drawingSession = commandList.CreateDrawingSession())
            {
                drawingSession.FillEllipse(centerPoint.X, centerPoint.Y, radiusX, radiusY, winColor);
                //this._session.FillEllipse(centerPoint.X, centerPoint.Y, radiusX, radiusY, winColor);
            };

            GaussianBlurEffect effect = new GaussianBlurEffect()
            {
                Source = commandList,
                BlurAmount = shadowBlur,
            };

            //var sourceRect = new global::Windows.Foundation.Rect(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
            //var destinationRect = new global::Windows.Foundation.Rect(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);

            this._session.DrawImage(effect); //, destinationRect, sourceRect);
            //this._session.DrawRectangle(destinationRect, winColor);
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

            if (null == text)
            {
                return;
            }

            var winTextFormat = new CanvasTextFormat()
            {
                HorizontalAlignment = textFormat.HorizontalAlignment.ToCanvasHorizontalAlignment(),
                VerticalAlignment = textFormat.VerticalAlignment.ToCanvasVerticalAlignment(),
                FontSize = textFormat.FontSize,
                FontWeight = textFormat.FontWeight.ToXamlFontWeight(),
                //Options = CanvasDrawTextOptions.NoPixelSnap, // NOTE: NoPixelSnap prevents jitter on mouseover
            };

            this._session.DrawText(text, bounds.ToXamlRect(), color.ToXamlColor(), winTextFormat);
        }
    }
}
