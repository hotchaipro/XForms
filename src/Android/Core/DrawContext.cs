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
            Rectangle rectangle)
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
            this._canvas.DrawColor(color.ToAndroidColor());
        }

        public void DrawRectangle(
            Rectangle rectangle,
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
            Rectangle rectangle,
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

        public void DrawEllipse(
            Rectangle rectangle,
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
            Rectangle rectangle,
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
            Rectangle destinationRectangle,
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
            Rectangle destinationRectangle,
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

            if (!tint.IsTransparent)
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
            Rectangle rectangle,
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
                x = dpBounds.Left;
                y = dpBounds.Top;
            }
            else
            {
                Rect textBounds = new Rect();
                paint.GetTextBounds(text, 0, text.Length, textBounds);

                x = ((dpBounds.Width() - textBounds.Width()) * originX) + dpBounds.Left;
                y = ((dpBounds.Height() - textBounds.Height()) * originY) + dpBounds.Top;
            }

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

        private Typeface GetTypefaceFromTextFormat(
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
