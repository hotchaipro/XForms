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
    internal sealed unsafe class DrawContext : IDrawContext
    {
        private readonly ID2D1DeviceContext* _context;
        private readonly Rectangle _bounds;
        private ID2D1SolidColorBrush* _brush;

        public DrawContext(
            ID2D1DeviceContext* context,
            Rectangle bounds)
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

        public void Clear(
            Color color)
        {
            this._context->Clear(color.ToD2DColor());
        }

        public void DrawRectangle(
            Rectangle rectangle,
            Color color,
            float strokeWidth = 1.0f)
        {
            this._context->DrawRectangle(rectangle.ToD2DRect(), this.Brush(color), strokeWidth, null);
        }

        public void FillRectangle(
            Rectangle rectangle,
            Color color)
        {
            this._context->FillRectangle(rectangle.ToD2DRect(), this.Brush(color));
        }

        public void DrawEllipse(
            Point point,
            float radiusX,
            float radiusY,
            Color color,
            float strokeWidth = 1.0f)
        {
            this._context->DrawEllipse(Ellipse(point, radiusX, radiusY), this.Brush(color), strokeWidth, null);
        }

        public void FillEllipse(
            Point point,
            float radiusX,
            float radiusY,
            Color color)
        {
            this._context->FillEllipse(Ellipse(point, radiusX, radiusY), this.Brush(color));
        }

        public void FillEllipse(
            Rectangle rectangle,
            Color color)
        {
            this.FillEllipse(rectangle.Center, rectangle.Width / 2, rectangle.Height / 2, color);
        }

        public void FillEllipseShadow(
            Rectangle rectangle,
            Color color,
            float blurAmount = 8.0f)
        {
            // NOTE: The prototype approximates the blurred shadow with a soft offset ellipse
            var shadowColor = Color.FromArgb((byte)(color.A / 4), color.R, color.G, color.B);
            var shadowBounds = rectangle.Deflate(blurAmount, blurAmount).Offset(0, blurAmount / 4);
            this.FillEllipse(shadowBounds, shadowColor);
        }

        public void DrawImage(
            Bitmap bitmap,
            Rectangle destinationRectangle,
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
            Rectangle destinationRectangle,
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

        public void DrawLine(
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
        public void DrawArc(
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
            Rectangle bounds,
            Color color,
            float fontSize)
        {
            var textFormat = new TextFormat()
            {
                FontSize = fontSize,
                HorizontalAlignment = LayoutAlignment.Center,
                VerticalAlignment = LayoutAlignment.Center,
            };

            this.DrawText(glyph.ToString(), bounds, color, textFormat, TextResources.IconFontFamily);
        }

        public void DrawText(
            string text,
            Rectangle bounds,
            Color color,
            TextFormat textFormat)
        {
            this.DrawText(text, bounds, color, textFormat, TextResources.FontFamily);
        }

        public void DrawText(
            string text,
            Rectangle bounds,
            Color color,
            TextFormat textFormat,
            string fontFamily)
        {
            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            var host = Win32Host.Current;

            IDWriteTextFormat* format = TextResources.CreateTextFormat(textFormat.FontSize, textFormat.FontWeight, FontStyle.Normal, fontFamily);
            try
            {
                format->SetTextAlignment(textFormat.HorizontalAlignment.ToDWriteTextAlignment());
                format->SetParagraphAlignment(textFormat.VerticalAlignment.ToDWriteParagraphAlignment());
                format->SetWordWrapping(DWRITE_WORD_WRAPPING.DWRITE_WORD_WRAPPING_NO_WRAP);

                IDWriteTextLayout* layout;
                host.DWriteFactory->CreateTextLayout(text, (uint)text.Length, format, bounds.Width, bounds.Height, &layout);
                try
                {
                    this._context->DrawTextLayout(
                        new D2D_POINT_2F() { x = bounds.X, y = bounds.Y },
                        layout,
                        this.Brush(color),
                        D2D1_DRAW_TEXT_OPTIONS.D2D1_DRAW_TEXT_OPTIONS_ENABLE_COLOR_FONT);
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

        public void DrawTextLayout(
            IDWriteTextLayout* layout,
            Color color)
        {
            this._context->DrawTextLayout(
                new D2D_POINT_2F() { x = 0, y = 0 },
                layout,
                this.Brush(color),
                D2D1_DRAW_TEXT_OPTIONS.D2D1_DRAW_TEXT_OPTIONS_ENABLE_COLOR_FONT);
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
            this Rectangle rectangle)
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
            this LayoutAlignment alignment)
        {
            switch (alignment)
            {
                case LayoutAlignment.Center:
                case LayoutAlignment.Fill:
                    return DWRITE_TEXT_ALIGNMENT.DWRITE_TEXT_ALIGNMENT_CENTER;

                case LayoutAlignment.End:
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
            this LayoutAlignment alignment)
        {
            switch (alignment)
            {
                case LayoutAlignment.Center:
                case LayoutAlignment.Fill:
                    return DWRITE_PARAGRAPH_ALIGNMENT.DWRITE_PARAGRAPH_ALIGNMENT_CENTER;

                case LayoutAlignment.End:
                    return DWRITE_PARAGRAPH_ALIGNMENT.DWRITE_PARAGRAPH_ALIGNMENT_FAR;

                default:
                    return DWRITE_PARAGRAPH_ALIGNMENT.DWRITE_PARAGRAPH_ALIGNMENT_NEAR;
            }
        }
    }
}
