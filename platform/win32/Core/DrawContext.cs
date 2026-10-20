using System;
using Windows.Win32.Graphics.Direct2D;
using Windows.Win32.Graphics.Direct2D.Common;
using Windows.Win32.Graphics.DirectWrite;
using XForms.Win32.Renderers;

namespace XForms.Win32
{
    /// <summary>
    /// Draws XForms owner-drawn content with Direct2D, and text with DirectWrite
    /// (the system's own text renderer).
    /// </summary>
    internal sealed unsafe class DrawContext : DrawingContext
    {
        private readonly ID2D1DeviceContext* _context;
        private readonly Rect _bounds;
        private ID2D1SolidColorBrush* _brush;

        public DrawContext(
            ID2D1DeviceContext* context,
            Rect bounds)
        {
            this._context = context;
            this._bounds = bounds;
        }

        private ID2D1Brush* Brush(
            Color color)
        {
            var d2dColor = color.ToD2DColor();

            if (null == this._brush)
            {
                ID2D1SolidColorBrush* brush;
                this._context->CreateSolidColorBrush(d2dColor, null, &brush);
                this._brush = brush;
            }
            else
            {
                this._brush->SetColor(d2dColor);
            }

            return (ID2D1Brush*)this._brush;
        }

        public void Release()
        {
            if (null != this._brush)
            {
                this._brush->Release();
                this._brush = null;
            }
        }

        #region Brushes, paths and pushes

        private enum PushKind
        {
            Clip,
            Transform,
            Opacity,
        }

        private readonly System.Collections.Generic.Stack<(PushKind Kind, D2D_MATRIX_3X2_F Saved)> _pushes = new();

        // A native brush for the brush, filling the bounds; gradient brushes are
        // created for the call, and must be released with ReleaseBrush
        private ID2D1Brush* CreateBrush(
            Brush brush,
            Rect bounds,
            out bool mustRelease)
        {
            mustRelease = false;

            if (brush is LinearGradientBrush gradient)
            {
                var stops = new D2D1_GRADIENT_STOP[gradient.Stops.Count];
                for (int i = 0; i < stops.Length; i += 1)
                {
                    stops[i] = new D2D1_GRADIENT_STOP() { position = gradient.Stops[i].Offset, color = gradient.Stops[i].Color.ToD2DColor() };
                }

                ID2D1GradientStopCollection* collection;
                fixed (D2D1_GRADIENT_STOP* stopsPointer = stops)
                {
                    this._context->CreateGradientStopCollection(stopsPointer, (uint)stops.Length, D2D1_GAMMA.D2D1_GAMMA_2_2, D2D1_EXTEND_MODE.D2D1_EXTEND_MODE_CLAMP, &collection);
                }

                gradient.GetAbsolutePoints(bounds, out var start, out var end);
                var properties = new D2D1_LINEAR_GRADIENT_BRUSH_PROPERTIES()
                {
                    startPoint = new D2D_POINT_2F() { x = start.X, y = start.Y },
                    endPoint = new D2D_POINT_2F() { x = end.X, y = end.Y },
                };

                ID2D1LinearGradientBrush* linear;
                this._context->CreateLinearGradientBrush(&properties, null, collection, &linear);
                collection->Release();

                mustRelease = true;
                return (ID2D1Brush*)linear;
            }

            var solid = (SolidColorBrush)brush;
            return this.Brush(solid.Color);
        }

        private static void ReleaseBrush(
            ID2D1Brush* brush,
            bool mustRelease)
        {
            if (mustRelease)
            {
                brush->Release();
            }
        }

        public override void FillRectangle(
            Rect rectangle,
            Brush brush)
        {
            var native = this.CreateBrush(brush, rectangle, out bool mustRelease);
            this._context->FillRectangle(rectangle.ToD2DRect(), native);
            ReleaseBrush(native, mustRelease);
        }

        public override void FillRoundedRectangle(
            Rect rectangle,
            float cornerRadius,
            Brush brush)
        {
            var native = this.CreateBrush(brush, rectangle, out bool mustRelease);
            var rounded = new D2D1_ROUNDED_RECT() { rect = rectangle.ToD2DRect(), radiusX = cornerRadius, radiusY = cornerRadius };
            this._context->FillRoundedRectangle(&rounded, native);
            ReleaseBrush(native, mustRelease);
        }

        public override void FillEllipse(
            Rect rectangle,
            Brush brush)
        {
            var native = this.CreateBrush(brush, rectangle, out bool mustRelease);
            this._context->FillEllipse(Ellipse(rectangle.Center, rectangle.Width / 2, rectangle.Height / 2), native);
            ReleaseBrush(native, mustRelease);
        }

        private ID2D1PathGeometry* CreateGeometry(
            PathGeometry path)
        {
            ID2D1Factory* factory;
            this._context->GetFactory(&factory);

            ID2D1PathGeometry* geometry;
            factory->CreatePathGeometry(&geometry);
            factory->Release();

            ID2D1GeometrySink* sink;
            geometry->Open(&sink);

            bool isInFigure = false;
            foreach (var segment in path.Segments)
            {
                var point = new D2D_POINT_2F() { x = segment.Point.X, y = segment.Point.Y };
                switch (segment.Kind)
                {
                    case PathSegmentKind.MoveTo:
                        if (isInFigure)
                        {
                            sink->EndFigure(D2D1_FIGURE_END.D2D1_FIGURE_END_OPEN);
                        }

                        sink->BeginFigure(point, D2D1_FIGURE_BEGIN.D2D1_FIGURE_BEGIN_FILLED);
                        isInFigure = true;
                        break;

                    case PathSegmentKind.LineTo:
                        sink->AddLine(point);
                        break;

                    case PathSegmentKind.QuadraticTo:
                        {
                            var quadratic = new D2D1_QUADRATIC_BEZIER_SEGMENT()
                            {
                                point1 = new D2D_POINT_2F() { x = segment.Control1.X, y = segment.Control1.Y },
                                point2 = point,
                            };
                            sink->AddQuadraticBezier(&quadratic);
                        }

                        break;

                    case PathSegmentKind.CubicTo:
                        {
                            var cubic = new D2D1_BEZIER_SEGMENT()
                            {
                                point1 = new D2D_POINT_2F() { x = segment.Control1.X, y = segment.Control1.Y },
                                point2 = new D2D_POINT_2F() { x = segment.Control2.X, y = segment.Control2.Y },
                                point3 = point,
                            };
                            sink->AddBezier(&cubic);
                        }

                        break;

                    case PathSegmentKind.Close:
                        if (isInFigure)
                        {
                            sink->EndFigure(D2D1_FIGURE_END.D2D1_FIGURE_END_CLOSED);
                            isInFigure = false;
                        }

                        break;
                }
            }

            if (isInFigure)
            {
                sink->EndFigure(D2D1_FIGURE_END.D2D1_FIGURE_END_OPEN);
            }

            sink->Close();
            sink->Release();
            return geometry;
        }

        public override void FillGeometry(
            PathGeometry geometry,
            Brush brush)
        {
            var native = this.CreateGeometry(geometry);
            var nativeBrush = this.CreateBrush(brush, geometry.Bounds, out bool mustRelease);
            this._context->FillGeometry((ID2D1Geometry*)native, nativeBrush, null);
            ReleaseBrush(nativeBrush, mustRelease);
            native->Release();
        }

        public override void DrawGeometry(
            PathGeometry geometry,
            Color color,
            float strokeWidth = 1.0f)
        {
            var native = this.CreateGeometry(geometry);
            this._context->DrawGeometry((ID2D1Geometry*)native, this.Brush(color), strokeWidth, TextResources.RoundStrokeStyle);
            native->Release();
        }

        protected override void PushClipCore(
            Rect clip)
        {
            this._context->PushAxisAlignedClip(clip.ToD2DRect(), D2D1_ANTIALIAS_MODE.D2D1_ANTIALIAS_MODE_PER_PRIMITIVE);
            this._pushes.Push((PushKind.Clip, default));
        }

        protected override void PushTransformCore(
            System.Numerics.Matrix3x2 transform)
        {
            D2D_MATRIX_3X2_F current;
            this._context->GetTransform(&current);
            this._pushes.Push((PushKind.Transform, current));

            // The new transform applies first, then the current one
            var c = new System.Numerics.Matrix3x2(
                current.Anonymous.Anonymous1.m11, current.Anonymous.Anonymous1.m12,
                current.Anonymous.Anonymous1.m21, current.Anonymous.Anonymous1.m22,
                current.Anonymous.Anonymous1.dx, current.Anonymous.Anonymous1.dy);
            var m = transform * c;

            var combined = new D2D_MATRIX_3X2_F();
            combined.Anonymous.Anonymous1.m11 = m.M11;
            combined.Anonymous.Anonymous1.m12 = m.M12;
            combined.Anonymous.Anonymous1.m21 = m.M21;
            combined.Anonymous.Anonymous1.m22 = m.M22;
            combined.Anonymous.Anonymous1.dx = m.M31;
            combined.Anonymous.Anonymous1.dy = m.M32;
            this._context->SetTransform(&combined);
        }

        protected override void PushOpacityCore(
            float opacity)
        {
            var identity = new D2D_MATRIX_3X2_F();
            identity.Anonymous.Anonymous1.m11 = 1;
            identity.Anonymous.Anonymous1.m22 = 1;

            var parameters = new D2D1_LAYER_PARAMETERS1()
            {
                contentBounds = new D2D_RECT_F() { left = float.NegativeInfinity, top = float.NegativeInfinity, right = float.PositiveInfinity, bottom = float.PositiveInfinity },
                maskAntialiasMode = D2D1_ANTIALIAS_MODE.D2D1_ANTIALIAS_MODE_PER_PRIMITIVE,
                maskTransform = identity,
                opacity = opacity,
                layerOptions = D2D1_LAYER_OPTIONS1.D2D1_LAYER_OPTIONS1_NONE,
            };

            this._context->PushLayer(&parameters, null);
            this._pushes.Push((PushKind.Opacity, default));
        }

        public override void Pop()
        {
            if (0 == this._pushes.Count)
            {
                throw new InvalidOperationException("Pop without a matching Push.");
            }

            var (kind, saved) = this._pushes.Pop();
            switch (kind)
            {
                case PushKind.Clip:
                    this._context->PopAxisAlignedClip();
                    break;

                case PushKind.Transform:
                    this._context->SetTransform(&saved);
                    break;

                case PushKind.Opacity:
                    this._context->PopLayer();
                    break;
            }
        }

        #endregion Brushes, paths and pushes

        public override void Clear(
            Color color)
        {
            this._context->Clear(color.ToD2DColor());
        }

        public override void DrawRectangle(
            Rect rectangle,
            Color color,
            float strokeWidth = 1.0f)
        {
            this._context->DrawRectangle(rectangle.ToD2DRect(), this.Brush(color), strokeWidth, null);
        }

        public override void FillRectangle(
            Rect rectangle,
            Color color)
        {
            this._context->FillRectangle(rectangle.ToD2DRect(), this.Brush(color));
        }

        public override void DrawRoundedRectangle(
            Rect rectangle,
            float cornerRadius,
            Color color,
            float strokeWidth = 1.0f)
        {
            var rounded = new D2D1_ROUNDED_RECT() { rect = rectangle.ToD2DRect(), radiusX = cornerRadius, radiusY = cornerRadius };
            this._context->DrawRoundedRectangle(&rounded, this.Brush(color), strokeWidth, null);
        }

        public override void FillRoundedRectangle(
            Rect rectangle,
            float cornerRadius,
            Color color)
        {
            var rounded = new D2D1_ROUNDED_RECT() { rect = rectangle.ToD2DRect(), radiusX = cornerRadius, radiusY = cornerRadius };
            this._context->FillRoundedRectangle(&rounded, this.Brush(color));
        }

        public override void DrawEllipse(
            Point point,
            float radiusX,
            float radiusY,
            Color color,
            float strokeWidth = 1.0f)
        {
            this._context->DrawEllipse(Ellipse(point, radiusX, radiusY), this.Brush(color), strokeWidth, null);
        }

        public override void FillEllipse(
            Point point,
            float radiusX,
            float radiusY,
            Color color)
        {
            this._context->FillEllipse(Ellipse(point, radiusX, radiusY), this.Brush(color));
        }

        public override void FillEllipse(
            Rect rectangle,
            Color color)
        {
            this.FillEllipse(rectangle.Center, rectangle.Width / 2, rectangle.Height / 2, color);
        }

        public override void FillEllipseShadow(
            Rect rectangle,
            Color color,
            float blurAmount = 8.0f)
        {
            // NOTE: The prototype approximates the blurred shadow with a soft offset ellipse
            var shadowColor = Color.FromArgb((byte)(color.A / 4), color.R, color.G, color.B);
            var shadowBounds = rectangle.Deflate(blurAmount, blurAmount).Offset(0, blurAmount / 4);
            this.FillEllipse(shadowBounds, shadowColor);
        }

        public override void DrawImage(
            Bitmap bitmap,
            Rect destinationRectangle,
            Color tint)
        {
            this.DrawBitmap(((BitmapRenderer)bitmap?.Renderer)?.NativeBitmap, destinationRectangle, tint);
        }

        /// <summary>
        /// Draws the bitmap stretched to the rectangle, or (with a tint) paints the
        /// tint through the bitmap's alpha.
        /// </summary>
        public void DrawBitmap(
            ID2D1Bitmap1* nativeBitmap,
            Rect destinationRectangle,
            Color tint)
        {
            if (null == nativeBitmap)
            {
                return;
            }

            var destination = destinationRectangle.ToD2DRect();

            if (tint.IsTransparent)
            {
                this._context->DrawBitmap((ID2D1Bitmap*)nativeBitmap, destination, 1.0f, D2D1_INTERPOLATION_MODE.D2D1_INTERPOLATION_MODE_HIGH_QUALITY_CUBIC, null, null);
            }
            else
            {
                // Paint the tint through the image's alpha
                this._context->SetAntialiasMode(D2D1_ANTIALIAS_MODE.D2D1_ANTIALIAS_MODE_ALIASED);
                this._context->FillOpacityMask((ID2D1Bitmap*)nativeBitmap, this.Brush(tint), destination, null);
                this._context->SetAntialiasMode(D2D1_ANTIALIAS_MODE.D2D1_ANTIALIAS_MODE_PER_PRIMITIVE);
            }
        }

        public override void DrawLine(
            Point start,
            Point end,
            Color color,
            float strokeWidth = 1.0f)
        {
            this._context->DrawLine(
                new D2D_POINT_2F() { x = start.X, y = start.Y },
                new D2D_POINT_2F() { x = end.X, y = end.Y },
                this.Brush(color),
                strokeWidth,
                TextResources.RoundStrokeStyle);
        }

        /// <summary>
        /// Draws a circular arc with round ends, clockwise from the start angle
        /// (in degrees, 0 at the top).
        /// </summary>
        public override void DrawArc(
            Point center,
            float radius,
            float startDegrees,
            float sweepDegrees,
            Color color,
            float strokeWidth)
        {
            static D2D_POINT_2F PointAt(Point center, float radius, float degrees)
            {
                double radians = (degrees - 90) * Math.PI / 180;
                return new D2D_POINT_2F() { x = center.X + (float)(radius * Math.Cos(radians)), y = center.Y + (float)(radius * Math.Sin(radians)) };
            }

            ID2D1Factory* factory;
            this._context->GetFactory(&factory);

            ID2D1PathGeometry* geometry;
            factory->CreatePathGeometry(&geometry);
            factory->Release();

            ID2D1GeometrySink* sink;
            geometry->Open(&sink);
            sink->BeginFigure(PointAt(center, radius, startDegrees), D2D1_FIGURE_BEGIN.D2D1_FIGURE_BEGIN_HOLLOW);
            sink->AddArc(new D2D1_ARC_SEGMENT()
            {
                point = PointAt(center, radius, startDegrees + sweepDegrees),
                size = new D2D_SIZE_F() { width = radius, height = radius },
                rotationAngle = 0,
                sweepDirection = D2D1_SWEEP_DIRECTION.D2D1_SWEEP_DIRECTION_CLOCKWISE,
                arcSize = (sweepDegrees > 180) ? D2D1_ARC_SIZE.D2D1_ARC_SIZE_LARGE : D2D1_ARC_SIZE.D2D1_ARC_SIZE_SMALL,
            });
            sink->EndFigure(D2D1_FIGURE_END.D2D1_FIGURE_END_OPEN);
            sink->Close();
            sink->Release();

            this._context->DrawGeometry((ID2D1Geometry*)geometry, this.Brush(color), strokeWidth, TextResources.RoundStrokeStyle);
            geometry->Release();
        }

        /// <summary>
        /// Draws a glyph from the system icon font (Segoe MDL2 Assets) centered in the bounds.
        /// </summary>
        public void DrawIcon(
            char glyph,
            Rect bounds,
            Color color,
            float fontSize)
        {
            var textFormat = new TextFormat()
            {
                FontSize = fontSize,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            };

            this.DrawText(glyph.ToString(), bounds, color, textFormat, TextResources.IconFontFamily);
        }

        public override void DrawText(
            TextLayout layout,
            Point origin,
            Color color)
        {
            var native = (TextLayoutRenderer)layout.Native;
            TextLayouts.Draw(this._context, native.Layout, origin, this.Brush(color), layout.Spans);
        }

        /// <summary>
        /// Draws text in a font family the platform names (such as the icon font).
        /// </summary>
        public void DrawText(
            string text,
            Rect bounds,
            Color color,
            TextFormat textFormat,
            string fontFamily)
        {
            var format = new TextFormat()
            {
                FontFamily = fontFamily,
                FontSize = textFormat.FontSize,
                FontWeight = textFormat.FontWeight,
                FontStyle = textFormat.FontStyle,
                HorizontalAlignment = textFormat.HorizontalAlignment,
                VerticalAlignment = textFormat.VerticalAlignment,
                TextWrapping = textFormat.TextWrapping,
                TextTrimming = textFormat.TextTrimming,
                MaxLines = textFormat.MaxLines,
            };
            this.DrawText(text, bounds, color, format);
        }

        public void DrawTextLayout(
            IDWriteTextLayout* layout,
            Color color,
            System.Collections.Generic.IReadOnlyList<TextSpan> spans = null)
        {
            TextLayouts.Draw(this._context, layout, Point.Zero, this.Brush(color), spans);
        }

        private static D2D1_ELLIPSE Ellipse(
            Point center,
            float radiusX,
            float radiusY)
        {
            return new D2D1_ELLIPSE()
            {
                point = new D2D_POINT_2F() { x = center.X, y = center.Y },
                radiusX = radiusX,
                radiusY = radiusY,
            };
        }
    }

    internal static unsafe class TextResources
    {
        public const string FontFamily = "Segoe UI";

        public const string IconFontFamily = "Segoe MDL2 Assets";

        private static ID2D1StrokeStyle* _roundStrokeStyle;

        /// <summary>
        /// Round line ends and joins.
        /// </summary>
        public static ID2D1StrokeStyle* RoundStrokeStyle
        {
            get
            {
                if (null == _roundStrokeStyle)
                {
                    ID2D1Factory* factory;
                    Win32Host.Current.ResourceContext->GetFactory(&factory);

                    var properties = new D2D1_STROKE_STYLE_PROPERTIES()
                    {
                        startCap = D2D1_CAP_STYLE.D2D1_CAP_STYLE_ROUND,
                        endCap = D2D1_CAP_STYLE.D2D1_CAP_STYLE_ROUND,
                        dashCap = D2D1_CAP_STYLE.D2D1_CAP_STYLE_ROUND,
                        lineJoin = D2D1_LINE_JOIN.D2D1_LINE_JOIN_ROUND,
                        miterLimit = 10,
                    };

                    ID2D1StrokeStyle* strokeStyle;
                    factory->CreateStrokeStyle(&properties, null, 0, &strokeStyle);
                    factory->Release();
                    _roundStrokeStyle = strokeStyle;
                }

                return _roundStrokeStyle;
            }
        }

        /// <summary>
        /// The size of a single line of text.
        /// </summary>
        public static Size MeasureText(
            string text,
            float fontSize,
            FontWeight fontWeight)
        {
            if (string.IsNullOrEmpty(text))
            {
                return new Size(0, (float)Math.Ceiling(fontSize * 1.33f));
            }

            IDWriteTextFormat* format = CreateTextFormat(fontSize, fontWeight, FontStyle.Normal);
            try
            {
                format->SetWordWrapping(DWRITE_WORD_WRAPPING.DWRITE_WORD_WRAPPING_NO_WRAP);

                IDWriteTextLayout* layout;
                Win32Host.Current.DWriteFactory->CreateTextLayout(text, (uint)text.Length, format, float.MaxValue, float.MaxValue, &layout);
                try
                {
                    DWRITE_TEXT_METRICS metrics;
                    layout->GetMetrics(&metrics);
                    return new Size((float)Math.Ceiling(metrics.widthIncludingTrailingWhitespace), (float)Math.Ceiling(metrics.height));
                }
                finally
                {
                    layout->Release();
                }
            }
            finally
            {
                format->Release();
            }
        }

        public static IDWriteTextFormat* CreateTextFormat(
            float fontSize,
            FontWeight fontWeight,
            FontStyle fontStyle)
        {
            return CreateTextFormat(fontSize, fontWeight, fontStyle, FontFamily);
        }

        public static IDWriteTextFormat* CreateTextFormat(
            float fontSize,
            FontWeight fontWeight,
            FontStyle fontStyle,
            string fontFamily)
        {
            IDWriteTextFormat* format;
            Win32Host.Current.DWriteFactory->CreateTextFormat(
                fontFamily,
                null,
                (DWRITE_FONT_WEIGHT)(int)fontWeight,
                (fontStyle == FontStyle.Italic) ? DWRITE_FONT_STYLE.DWRITE_FONT_STYLE_ITALIC : DWRITE_FONT_STYLE.DWRITE_FONT_STYLE_NORMAL,
                DWRITE_FONT_STRETCH.DWRITE_FONT_STRETCH_NORMAL,
                Math.Max(1, fontSize),
                "",
                &format);

            return format;
        }
    }

    internal static class NativeConversions
    {
        public static D2D1_COLOR_F ToD2DColor(
            this Color color)
        {
            return new D2D1_COLOR_F()
            {
                r = color.R / 255.0f,
                g = color.G / 255.0f,
                b = color.B / 255.0f,
                a = color.A / 255.0f,
            };
        }

        public static D2D_RECT_F ToD2DRect(
            this Rect rectangle)
        {
            return new D2D_RECT_F()
            {
                left = rectangle.Left,
                top = rectangle.Top,
                right = rectangle.Right,
                bottom = rectangle.Bottom,
            };
        }

        public static DWRITE_TEXT_ALIGNMENT ToDWriteTextAlignment(
            this HorizontalAlignment alignment)
        {
            switch (alignment)
            {
                case HorizontalAlignment.Center:
                case HorizontalAlignment.Stretch:
                    return DWRITE_TEXT_ALIGNMENT.DWRITE_TEXT_ALIGNMENT_CENTER;

                case HorizontalAlignment.Right:
                    return DWRITE_TEXT_ALIGNMENT.DWRITE_TEXT_ALIGNMENT_TRAILING;

                default:
                    return DWRITE_TEXT_ALIGNMENT.DWRITE_TEXT_ALIGNMENT_LEADING;
            }
        }

        public static DWRITE_TEXT_ALIGNMENT ToDWriteTextAlignment(
            this TextAlignment alignment)
        {
            switch (alignment)
            {
                case TextAlignment.Center:
                    return DWRITE_TEXT_ALIGNMENT.DWRITE_TEXT_ALIGNMENT_CENTER;

                case TextAlignment.End:
                    return DWRITE_TEXT_ALIGNMENT.DWRITE_TEXT_ALIGNMENT_TRAILING;

                default:
                    return DWRITE_TEXT_ALIGNMENT.DWRITE_TEXT_ALIGNMENT_LEADING;
            }
        }

        public static DWRITE_PARAGRAPH_ALIGNMENT ToDWriteParagraphAlignment(
            this VerticalAlignment alignment)
        {
            switch (alignment)
            {
                case VerticalAlignment.Center:
                case VerticalAlignment.Stretch:
                    return DWRITE_PARAGRAPH_ALIGNMENT.DWRITE_PARAGRAPH_ALIGNMENT_CENTER;

                case VerticalAlignment.Bottom:
                    return DWRITE_PARAGRAPH_ALIGNMENT.DWRITE_PARAGRAPH_ALIGNMENT_FAR;

                default:
                    return DWRITE_PARAGRAPH_ALIGNMENT.DWRITE_PARAGRAPH_ALIGNMENT_NEAR;
            }
        }
    }
}
