using System;
using CoreGraphics;
using Foundation;
using UIKit;
using XForms.Controls;

using XForms.iOS.Renderers;

namespace XForms.iOS
{
    public sealed class DrawContext : DrawingContext
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

        #region Brushes, paths and pushes

        private readonly System.Collections.Generic.Stack<bool> _pushes = new System.Collections.Generic.Stack<bool>();

        // Fills the shape with the brush; gradients are drawn clipped to it
        private void FillShape(
            Action addShape,
            Brush brush,
            Rect bounds)
        {
            var context = this._graphicsContext;

            if (brush is LinearGradientBrush gradient)
            {
                context.SaveState();
                addShape();
                context.Clip();

                var colors = new CGColor[gradient.Stops.Count];
                var locations = new nfloat[gradient.Stops.Count];
                for (int i = 0; i < colors.Length; i += 1)
                {
                    colors[i] = gradient.Stops[i].Color.ToCGColor();
                    locations[i] = gradient.Stops[i].Offset;
                }

                using (var colorSpace = CGColorSpace.CreateDeviceRGB())
                using (var cgGradient = new CGGradient(colorSpace, colors, locations))
                {
                    gradient.GetAbsolutePoints(bounds, out var start, out var end);
                    context.DrawLinearGradient(
                        cgGradient,
                        new CGPoint(start.X, start.Y),
                        new CGPoint(end.X, end.Y),
                        CGGradientDrawingOptions.DrawsBeforeStartLocation | CGGradientDrawingOptions.DrawsAfterEndLocation);
                }

                context.RestoreState();
                return;
            }

            context.SetFillColor(((SolidColorBrush)brush).Color.ToCGColor());
            addShape();
            context.FillPath();
        }

        public override void FillRectangle(
            Rect rectangle,
            Brush brush)
        {
            this.FillShape(() => this._graphicsContext.AddRect(rectangle.ToCGRect()), brush, rectangle);
        }

        public override void FillRoundedRectangle(
            Rect rectangle,
            float cornerRadius,
            Brush brush)
        {
            this.FillShape(() =>
            {
                using (var path = UIBezierPath.FromRoundedRect(rectangle.ToCGRect(), cornerRadius))
                {
                    this._graphicsContext.AddPath(path.CGPath);
                }
            }, brush, rectangle);
        }

        public override void FillEllipse(
            Rect rectangle,
            Brush brush)
        {
            this.FillShape(() => this._graphicsContext.AddEllipseInRect(rectangle.ToCGRect()), brush, rectangle);
        }

        private static CGPath CreatePath(
            PathGeometry geometry)
        {
            var path = new CGPath();
            foreach (var segment in geometry.Segments)
            {
                switch (segment.Kind)
                {
                    case PathSegmentKind.MoveTo:
                        path.MoveToPoint(segment.Point.X, segment.Point.Y);
                        break;

                    case PathSegmentKind.LineTo:
                        path.AddLineToPoint(segment.Point.X, segment.Point.Y);
                        break;

                    case PathSegmentKind.QuadraticTo:
                        path.AddQuadCurveToPoint(segment.Control1.X, segment.Control1.Y, segment.Point.X, segment.Point.Y);
                        break;

                    case PathSegmentKind.CubicTo:
                        path.AddCurveToPoint(segment.Control1.X, segment.Control1.Y, segment.Control2.X, segment.Control2.Y, segment.Point.X, segment.Point.Y);
                        break;

                    case PathSegmentKind.Close:
                        path.CloseSubpath();
                        break;
                }
            }

            return path;
        }

        public override void FillGeometry(
            PathGeometry geometry,
            Brush brush)
        {
            using (var path = CreatePath(geometry))
            {
                this.FillShape(() => this._graphicsContext.AddPath(path), brush, geometry.Bounds);
            }
        }

        public override void DrawGeometry(
            PathGeometry geometry,
            Color color,
            float strokeWidth = 1.0f)
        {
            using (var path = CreatePath(geometry))
            {
                var context = this._graphicsContext;
                context.SetStrokeColor(color.ToCGColor());
                context.SetLineWidth(strokeWidth);
                context.SetLineCap(CGLineCap.Round);
                context.SetLineJoin(CGLineJoin.Round);
                context.AddPath(path);
                context.StrokePath();
            }
        }

        protected override void PushClipCore(
            Rect clip)
        {
            this._graphicsContext.SaveState();
            this._graphicsContext.ClipToRect(clip.ToCGRect());
            this._pushes.Push(false);
        }

        protected override void PushTransformCore(
            System.Numerics.Matrix3x2 transform)
        {
            this._graphicsContext.SaveState();
            this._graphicsContext.ConcatCTM(new CGAffineTransform(transform.M11, transform.M12, transform.M21, transform.M22, transform.M31, transform.M32));
            this._pushes.Push(false);
        }

        protected override void PushOpacityCore(
            float opacity)
        {
            this._graphicsContext.SaveState();
            this._graphicsContext.SetAlpha(opacity);
            this._graphicsContext.BeginTransparencyLayer();
            this._pushes.Push(true);
        }

        public override void Pop()
        {
            if (0 == this._pushes.Count)
            {
                throw new InvalidOperationException("Pop without a matching Push.");
            }

            if (this._pushes.Pop())
            {
                this._graphicsContext.EndTransparencyLayer();
            }

            this._graphicsContext.RestoreState();
        }

        #endregion Brushes, paths and pushes

        protected override void ClearCore(
            Color color)
        {
            this._graphicsContext.ClearRect(this._bounds);

            if (!color.IsTransparent)
            {
                this._graphicsContext.SetFillColor(color.ToCGColor());
                this._graphicsContext.FillRect(this._bounds);
            }
        }

        public override void DrawRectangle(
            Rect rectangle,
            Color color,
            float strokeWidth = 1.0f)
        {
            var rect = rectangle.ToCGRect();

            this._graphicsContext.SetStrokeColor(color.ToCGColor());
            this._graphicsContext.StrokeRectWithWidth(rect, strokeWidth);
        }

        public override void FillRectangle(
            Rect rectangle,
            Color color)
        {
            var rect = rectangle.ToCGRect();
            this._graphicsContext.SetFillColor(color.ToCGColor());
            this._graphicsContext.FillRect(rect);
        }

        public override void DrawRoundedRectangle(
            Rect rectangle,
            float cornerRadius,
            Color color,
            float strokeWidth = 1.0f)
        {
            using (var path = UIBezierPath.FromRoundedRect(rectangle.ToCGRect(), cornerRadius))
            {
                this._graphicsContext.SetStrokeColor(color.ToCGColor());
                this._graphicsContext.SetLineWidth(strokeWidth);
                this._graphicsContext.AddPath(path.CGPath);
                this._graphicsContext.StrokePath();
            }
        }

        public override void FillRoundedRectangle(
            Rect rectangle,
            float cornerRadius,
            Color color)
        {
            using (var path = UIBezierPath.FromRoundedRect(rectangle.ToCGRect(), cornerRadius))
            {
                this._graphicsContext.SetFillColor(color.ToCGColor());
                this._graphicsContext.AddPath(path.CGPath);
                this._graphicsContext.FillPath();
            }
        }

        public override void DrawEllipse(
            Point centerPoint,
            float radiusX,
            float radiusY,
            Color color,
            float strokeWidth = 1.0f)
        {
            var rect = new Rect(
                centerPoint.X - radiusX,
                centerPoint.Y - radiusY,
                2 * radiusX,
                2 * radiusY);

            this.DrawEllipse(rect, color, strokeWidth);
        }

        public void DrawEllipse(
            Rect rectangle,
            Color color,
            float strokeWidth = 1.0f)
        {
            var rect = rectangle.ToCGRect();
            this._graphicsContext.SetStrokeColor(color.ToCGColor());
            this._graphicsContext.StrokeEllipseInRect(rect);
        }

        public override void FillEllipse(
            Point centerPoint,
            float radiusX,
            float radiusY,
            Color color)
        {
            var rect = new Rect(
                centerPoint.X - radiusX,
                centerPoint.Y - radiusY,
                2 * radiusX,
                2 * radiusY);

            this.FillEllipse(rect, color);
        }

        public override void FillEllipse(
            Rect rectangle,
            Color color)
        {
            var rect = rectangle.ToCGRect();
            this._graphicsContext.SetFillColor(color.ToCGColor());
            this._graphicsContext.FillEllipseInRect(rect);
        }

        public override void DrawImage(
            Bitmap bitmap,
            Rect destinationRectangle,
            Color tint)
        {
            // No image draws nothing, as on Win32
            if (null == bitmap)
            {
                return;
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
        public override void FillEllipseShadow(
            Rect rectangle,
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

        public override void DrawLine(
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

        public override void DrawArc(
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

        public override void DrawText(
            TextLayout layout,
            Point origin,
            Color color)
        {
            ((TextLayoutRenderer)layout.Native).Draw(origin, color);
        }
    }
}
