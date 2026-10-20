using System;
using Android.Content;
using Android.Graphics;
using XForms.Android.Renderers;

namespace XForms.Android
{
    public sealed class DrawContext : IDrawContext
    {
        private static Typeface[] TypefaceWeightRamp;
        private static readonly object InitSyncObject = new object();

        private Canvas _canvas;
        private Context _context;

        public DrawContext(
            global::Android.Content.Context context,
            Canvas canvas)
        {
            if (null == canvas)
            {
                throw new ArgumentNullException(nameof(canvas));
            }

            if (null == context)
            {
                throw new ArgumentNullException(nameof(context));
            }

            this._canvas = canvas;
            this._context = context;
        }

        private Context NativeContext
        {
            get
            {
                return this._context;
            }
        }

        private RectF ToNativeRectFPixels(
            Rect rectangle)
        {
            return new RectF(
                NativeConversions.DevicePixelsToPixels(this.NativeContext, rectangle.X),
                NativeConversions.DevicePixelsToPixels(this.NativeContext, rectangle.Y),
                NativeConversions.DevicePixelsToPixels(this.NativeContext, rectangle.Right),
                NativeConversions.DevicePixelsToPixels(this.NativeContext, rectangle.Bottom));
        }

        private PointF ToNativePointFPixels(
            Point point)
        {
            return new PointF(
                NativeConversions.DevicePixelsToPixels(this.NativeContext, point.X),
                NativeConversions.DevicePixelsToPixels(this.NativeContext, point.Y));
        }

        public void Clear(
            Color color)
        {
            // NOTE: A view's drawing starts empty, so clearing to transparent
            // leaves it empty, as Direct2D's Clear does. (Replacing pixels with
            // a transparent mode would punch through to the window, since views
            // draw into their parent's display list.)
            if (color.IsTransparent)
            {
                return;
            }

            this._canvas.DrawColor(color.ToAndroidColor());
        }

        public void DrawRectangle(
            Rect rectangle,
            Color color,
            float strokeWidth = 1.0f)
        {
            var rect = ToNativeRectFPixels(rectangle);

            var paint = new Paint()
            {
                Color = color.ToAndroidColor(),
                StrokeWidth = NativeConversions.DevicePixelsToPixels(this.NativeContext, strokeWidth),
                AntiAlias = true,
            };
            paint.SetStyle(Paint.Style.Stroke);

            this._canvas.DrawRect(rect, paint);

            paint.Dispose();
        }

        public void FillRectangle(
            Rect rectangle,
            Color color)
        {
            var rect = ToNativeRectFPixels(rectangle);

            var paint = new Paint()
            {
                Color = color.ToAndroidColor(),
                AntiAlias = true,
            };
            paint.SetStyle(Paint.Style.Fill);

            this._canvas.DrawRect(rect, paint);

            paint.Dispose();
        }

        public void DrawRoundedRectangle(
            Rect rectangle,
            float cornerRadius,
            Color color,
            float strokeWidth = 1.0f)
        {
            var rect = ToNativeRectFPixels(rectangle);
            float radius = NativeConversions.DevicePixelsToPixels(this.NativeContext, cornerRadius);

            var paint = new Paint()
            {
                Color = color.ToAndroidColor(),
                StrokeWidth = NativeConversions.DevicePixelsToPixels(this.NativeContext, strokeWidth),
                AntiAlias = true,
            };
            paint.SetStyle(Paint.Style.Stroke);

            this._canvas.DrawRoundRect(rect, radius, radius, paint);

            paint.Dispose();
        }

        public void FillRoundedRectangle(
            Rect rectangle,
            float cornerRadius,
            Color color)
        {
            var rect = ToNativeRectFPixels(rectangle);
            float radius = NativeConversions.DevicePixelsToPixels(this.NativeContext, cornerRadius);

            var paint = new Paint()
            {
                Color = color.ToAndroidColor(),
                AntiAlias = true,
            };
            paint.SetStyle(Paint.Style.Fill);

            this._canvas.DrawRoundRect(rect, radius, radius, paint);

            paint.Dispose();
        }

        public void DrawEllipse(
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

        public void FillEllipse(
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

        public void DrawEllipse(
            Rect rectangle,
            Color color,
            float strokeWidth = 1.0f)
        {
            var rect = ToNativeRectFPixels(rectangle);

            var paint = new Paint()
            {
                Color = color.ToAndroidColor(),
                StrokeWidth = NativeConversions.DevicePixelsToPixels(this.NativeContext, strokeWidth),
                AntiAlias = true,
            };
            paint.SetStyle(Paint.Style.Stroke);

            this._canvas.DrawOval(rect, paint);

            paint.Dispose();
        }

        public void FillEllipse(
            Rect rectangle,
            Color color)
        {
            var rect = ToNativeRectFPixels(rectangle);

            var paint = new Paint()
            {
                Color = color.ToAndroidColor(),
                AntiAlias = true,
            };
            paint.SetStyle(Paint.Style.Fill);

            this._canvas.DrawOval(rect, paint);

            paint.Dispose();
        }

        public void DrawImage(
            Bitmap bitmap,
            Rect destinationRectangle,
            Color tint)
        {
            if (null == bitmap)
            {
                throw new ArgumentNullException(nameof(bitmap));
            }

            var nativeBitmap = ((BitmapRenderer)bitmap.Renderer).NativeBitmap;

            this.DrawImage(nativeBitmap, destinationRectangle, tint);
        }

        internal void DrawImage(
            global::Android.Graphics.Bitmap nativeBitmap,
            Rect destinationRectangle,
            Color tint)
        {
            if (null == nativeBitmap)
            {
                throw new ArgumentNullException(nameof(nativeBitmap));
            }

            var destinationRect = this.ToNativeRectFPixels(destinationRectangle);

            var paint = new Paint()
            {
                AntiAlias = true,
            };

            LightingColorFilter colorFilter = null;

            if ((!tint.IsTransparent) && (tint != Colors.White))
            {
                colorFilter = new LightingColorFilter(tint.ToAndroidColor(), 1);
                paint.SetColorFilter(colorFilter);
            }

            this._canvas.DrawBitmap(nativeBitmap, null, destinationRect, paint);

            paint.Dispose();
            colorFilter?.Dispose();
        }

        // TODO: Replace with a Shadow property on View
        // ANDROID: View.Elevation
        public void FillEllipseShadow(
            Rect rectangle,
            Color color,
            float shadowBlur = 8.0f)
        {
            rectangle = rectangle.Inflate(-2 * shadowBlur, -2 * shadowBlur);

            var rect = ToNativeRectFPixels(rectangle);
            var shadowDp = NativeConversions.DevicePixelsToPixels(this.NativeContext, shadowBlur);

            var paint = new Paint()
            {
                Color = color.ToAndroidColor(),
                AntiAlias = true,
            };
            paint.SetShadowLayer(shadowDp, shadowDp, shadowDp, global::Android.Graphics.Color.Black);
            paint.SetStyle(Paint.Style.Fill);

            this._canvas.DrawOval(rect, paint);

            paint.Dispose();
        }

        private Paint CreateStrokePaint(
            Color color,
            float strokeWidth)
        {
            var paint = new Paint()
            {
                Color = color.ToAndroidColor(),
                StrokeWidth = NativeConversions.DevicePixelsToPixels(this.NativeContext, strokeWidth),
                AntiAlias = true,
            };
            paint.SetStyle(Paint.Style.Stroke);
            paint.StrokeCap = Paint.Cap.Round;
            paint.StrokeJoin = Paint.Join.Round;
            return paint;
        }

        public void DrawLine(
            Point start,
            Point end,
            Color color,
            float strokeWidth = 1.0f)
        {
            var nativeStart = this.ToNativePointFPixels(start);
            var nativeEnd = this.ToNativePointFPixels(end);

            var paint = this.CreateStrokePaint(color, strokeWidth);
            this._canvas.DrawLine(nativeStart.X, nativeStart.Y, nativeEnd.X, nativeEnd.Y, paint);
            paint.Dispose();
        }

        public void DrawArc(
            Point center,
            float radius,
            float startDegrees,
            float sweepDegrees,
            Color color,
            float strokeWidth = 1.0f)
        {
            var oval = this.ToNativeRectFPixels(new Rect(center.X - radius, center.Y - radius, 2 * radius, 2 * radius));

            // NOTE: Android measures angles from 3 o'clock; XForms from 12 o'clock
            var paint = this.CreateStrokePaint(color, strokeWidth);
            this._canvas.DrawArc(oval, startDegrees - 90, sweepDegrees, false, paint);
            paint.Dispose();
        }

        /// <summary>
        /// The size of a single line of text, in device-independent pixels, as
        /// <see cref="DrawText"/> draws it.
        /// </summary>
        internal static Size MeasureText(
            Context context,
            string text,
            TextFormat textFormat)
        {
            var paint = new Paint()
            {
                TextSize = NativeConversions.ScaledPixelsToPixels(context, textFormat.FontSize),
                AntiAlias = true,
            };
            paint.SetTypeface(GetTypefaceFromTextFormat(textFormat));

            var fontMetrics = paint.GetFontMetrics();
            float width = string.IsNullOrEmpty(text) ? 0 : paint.MeasureText(text);
            float height = fontMetrics.Descent - fontMetrics.Ascent;
            paint.Dispose();

            return new Size(
                (float)Math.Ceiling(NativeConversions.PixelsToDevicePixels(context, width)),
                (float)Math.Ceiling(NativeConversions.PixelsToDevicePixels(context, height)));
        }

        public void DrawText(
            string text,
            Rect bounds,
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

            var dpBounds = this.ToNativeRectFPixels(bounds);

            var paint = new Paint()
            {
                TextSize = NativeConversions.ScaledPixelsToPixels(this.NativeContext, textFormat.FontSize),
                TextAlign = Paint.Align.Left,
                Color = color.ToAndroidColor(),
                AntiAlias = true,
            };

            var typeface = GetTypefaceFromTextFormat(textFormat);
            paint.SetTypeface(typeface);

            float originX;
            if (textFormat.HorizontalAlignment == HorizontalAlignment.Left)
            {
                originX = 0;
            }
            else if (textFormat.HorizontalAlignment == HorizontalAlignment.Center)
            {
                originX = 0.5f;
            }
            else if (textFormat.HorizontalAlignment == HorizontalAlignment.Right)
            {
                originX = 1.0f;
            }
            else if (textFormat.HorizontalAlignment == HorizontalAlignment.Stretch)
            {
                originX = 0.5f;
            }
            else
            {
                throw new NotSupportedException("Unsupported alignment.");
            }

            float originY;
            if (textFormat.VerticalAlignment == VerticalAlignment.Top)
            {
                originY = 0;
            }
            else if (textFormat.VerticalAlignment == VerticalAlignment.Center)
            {
                originY = 0.5f;
            }
            else if (textFormat.VerticalAlignment == VerticalAlignment.Bottom)
            {
                originY = 1.0f;
            }
            else if (textFormat.VerticalAlignment == VerticalAlignment.Stretch)
            {
                originY = 0.5f;
            }
            else
            {
                throw new NotSupportedException("Unsupported alignment.");
            }

            // NOTE: Canvas.DrawText positions text by its baseline, so align the
            // line box (ascent to descent) and then offset to the baseline
            var fontMetrics = paint.GetFontMetrics();
            float textWidth = paint.MeasureText(text);
            float lineHeight = fontMetrics.Descent - fontMetrics.Ascent;

            float x = ((dpBounds.Width() - textWidth) * originX) + dpBounds.Left;
            float lineTop = ((dpBounds.Height() - lineHeight) * originY) + dpBounds.Top;
            float y = lineTop - fontMetrics.Ascent;

            this._canvas.DrawText(text, 0, text.Length, x, y, paint);
        }

        private static void InitTypefaceWeightRamp()
        {
			// https://android.googlesource.com/platform/frameworks/base/+/master/data/fonts/fonts.xml

			lock (InitSyncObject)
            {
                if (null == TypefaceWeightRamp)
                {
                    Typeface[] typefaceWeightRamp = new Typeface[5];

                    // Weight 100

                    if (global::Android.OS.Build.VERSION.SdkInt >= global::Android.OS.BuildVersionCodes.JellyBeanMr1)
                    {
                        typefaceWeightRamp[0] = Typeface.Create("sans-serif-thin", TypefaceStyle.Normal); // Weight=100
                    }
                    else if (global::Android.OS.Build.VERSION.SdkInt >= global::Android.OS.BuildVersionCodes.JellyBean)
                    {
                        typefaceWeightRamp[0] = Typeface.Create("sans-serif-light", TypefaceStyle.Normal); // Weight=300
                    }
                    else
                    {
                        typefaceWeightRamp[0] = Typeface.Create("sans-serif", TypefaceStyle.Normal); // Weight=400
                    }

                    // Weight 300

                    if (global::Android.OS.Build.VERSION.SdkInt >= global::Android.OS.BuildVersionCodes.JellyBean)
                    {
                        typefaceWeightRamp[1] = Typeface.Create("sans-serif-light", TypefaceStyle.Normal); // Weight=300
                    }
                    else
                    {
                        typefaceWeightRamp[1] = Typeface.Create("sans-serif", TypefaceStyle.Normal); // Weight=400
                    }

                    // Weight 400

                    typefaceWeightRamp[2] = Typeface.Create("sans-serif", TypefaceStyle.Normal); // Weight=400

                    // Weight 500

                    if (global::Android.OS.Build.VERSION.SdkInt >= global::Android.OS.BuildVersionCodes.Lollipop)
                    {
                        typefaceWeightRamp[3] = Typeface.Create("sans-serif-medium", TypefaceStyle.Normal); // Weight=500
                    }
                    else
                    {
                        typefaceWeightRamp[3] = Typeface.Create("sans-serif", TypefaceStyle.Bold); // Weight=700
                    }

                    // Weight 700

                    typefaceWeightRamp[4] = Typeface.Create("sans-serif", TypefaceStyle.Bold); // Weight=700

                    TypefaceWeightRamp = typefaceWeightRamp;
                }
            }
        }

        private static Typeface GetTypefaceFromTextFormat(
            TextFormat textFormat)
        {
            if (null == textFormat)
            {
                throw new ArgumentNullException(nameof(textFormat));
            }

            if (null == TypefaceWeightRamp)
            {
                InitTypefaceWeightRamp();
            }

            Typeface typeface;

            if (textFormat.FontWeight == FontWeight.Light)
            {
                typeface = TypefaceWeightRamp[0];
            }
            else if (textFormat.FontWeight == FontWeight.Normal)
            {
                typeface = TypefaceWeightRamp[1];
            }
            else if (textFormat.FontWeight == FontWeight.Medium)
            {
                typeface = TypefaceWeightRamp[2];
            }
            else if (textFormat.FontWeight == FontWeight.SemiBold)
            {
                typeface = TypefaceWeightRamp[3];
            }
            else if (textFormat.FontWeight == FontWeight.Bold)
            {
                typeface = TypefaceWeightRamp[4];
            }
            else
            {
                throw new NotSupportedException("Unsupported font weight.");
            }

            return typeface;
        }
    }
}
