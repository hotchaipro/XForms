using System;
using Android.Content;
using Android.Graphics;
using XForms.Android.Renderers;

namespace XForms.Android
{
    public sealed class DrawContext : DrawingContext
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

        #region Brushes, paths and pushes

        private int _pushCount;

        private float Px(
            float dp)
        {
            return NativeConversions.DevicePixelsToPixels(this.NativeContext, dp);
        }

        // A fill paint for the brush, with gradients spanning the bounds (in dips)
        private Paint CreatePaint(
            Brush brush,
            Rect bounds)
        {
            var paint = new Paint() { AntiAlias = true };
            paint.SetStyle(Paint.Style.Fill);

            if (brush is LinearGradientBrush gradient)
            {
                gradient.GetAbsolutePoints(bounds, out var start, out var end);
                var colors = new int[gradient.Stops.Count];
                var positions = new float[gradient.Stops.Count];
                for (int i = 0; i < colors.Length; i += 1)
                {
                    colors[i] = gradient.Stops[i].Color.ToAndroidColor().ToArgb();
                    positions[i] = gradient.Stops[i].Offset;
                }

                paint.SetShader(new LinearGradient(this.Px(start.X), this.Px(start.Y), this.Px(end.X), this.Px(end.Y), colors, positions, Shader.TileMode.Clamp));
            }
            else
            {
                paint.Color = ((SolidColorBrush)brush).Color.ToAndroidColor();
            }

            return paint;
        }

        public override void FillRectangle(
            Rect rectangle,
            Brush brush)
        {
            using (var paint = this.CreatePaint(brush, rectangle))
            {
                this._canvas.DrawRect(this.ToNativeRectFPixels(rectangle), paint);
            }
        }

        public override void FillRoundedRectangle(
            Rect rectangle,
            float cornerRadius,
            Brush brush)
        {
            using (var paint = this.CreatePaint(brush, rectangle))
            {
                float radius = this.Px(cornerRadius);
                this._canvas.DrawRoundRect(this.ToNativeRectFPixels(rectangle), radius, radius, paint);
            }
        }

        public override void FillEllipse(
            Rect rectangle,
            Brush brush)
        {
            using (var paint = this.CreatePaint(brush, rectangle))
            {
                this._canvas.DrawOval(this.ToNativeRectFPixels(rectangle), paint);
            }
        }

        private global::Android.Graphics.Path CreatePath(
            PathGeometry geometry)
        {
            var path = new global::Android.Graphics.Path();
            foreach (var segment in geometry.Segments)
            {
                switch (segment.Kind)
                {
                    case PathSegmentKind.MoveTo:
                        path.MoveTo(this.Px(segment.Point.X), this.Px(segment.Point.Y));
                        break;

                    case PathSegmentKind.LineTo:
                        path.LineTo(this.Px(segment.Point.X), this.Px(segment.Point.Y));
                        break;

                    case PathSegmentKind.QuadraticTo:
                        path.QuadTo(this.Px(segment.Control1.X), this.Px(segment.Control1.Y), this.Px(segment.Point.X), this.Px(segment.Point.Y));
                        break;

                    case PathSegmentKind.CubicTo:
                        path.CubicTo(
                            this.Px(segment.Control1.X), this.Px(segment.Control1.Y),
                            this.Px(segment.Control2.X), this.Px(segment.Control2.Y),
                            this.Px(segment.Point.X), this.Px(segment.Point.Y));
                        break;

                    case PathSegmentKind.Close:
                        path.Close();
                        break;
                }
            }

            return path;
        }

        public override void FillGeometry(
            PathGeometry geometry,
            Brush brush)
        {
            using (var path = this.CreatePath(geometry))
            using (var paint = this.CreatePaint(brush, geometry.Bounds))
            {
                path.SetFillType(global::Android.Graphics.Path.FillType.Winding);
                this._canvas.DrawPath(path, paint);
            }
        }

        public override void DrawGeometry(
            PathGeometry geometry,
            Color color,
            float strokeWidth = 1.0f)
        {
            using (var path = this.CreatePath(geometry))
            using (var paint = new Paint()
            {
                Color = color.ToAndroidColor(),
                StrokeWidth = this.Px(strokeWidth),
                AntiAlias = true,
                StrokeCap = Paint.Cap.Round,
                StrokeJoin = Paint.Join.Round,
            })
            {
                paint.SetStyle(Paint.Style.Stroke);
                this._canvas.DrawPath(path, paint);
            }
        }

        protected override void PushClipCore(
            Rect clip)
        {
            this._canvas.Save();
            this._canvas.ClipRect(this.ToNativeRectFPixels(clip));
            this._pushCount += 1;
        }

        protected override void PushTransformCore(
            System.Numerics.Matrix3x2 transform)
        {
            this._canvas.Save();

            // A translation (the common case, such as lightweight views'
            // positions) needs no matrix
            if ((transform.M11 == 1) && (transform.M22 == 1) && (transform.M12 == 0) && (transform.M21 == 0))
            {
                this._canvas.Translate(this.Px(transform.M31), this.Px(transform.M32));
                this._pushCount += 1;
                return;
            }

            // The transform is in dips; the canvas is in pixels, so only the
            // translation scales
            var matrix = new global::Android.Graphics.Matrix();
            matrix.SetValues(new float[]
            {
                transform.M11, transform.M21, this.Px(transform.M31),
                transform.M12, transform.M22, this.Px(transform.M32),
                0, 0, 1,
            });
            this._canvas.Concat(matrix);
            matrix.Dispose();
            this._pushCount += 1;
        }

        protected override void PushOpacityCore(
            float opacity)
        {
            this._canvas.SaveLayerAlpha(null, (int)Math.Round(opacity * 255));
            this._pushCount += 1;
        }

        public override void Pop()
        {
            if (0 == this._pushCount)
            {
                throw new InvalidOperationException("Pop without a matching Push.");
            }

            this._pushCount -= 1;
            this._canvas.Restore();
        }

        #endregion Brushes, paths and pushes

        protected override void ClearCore(
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

        public override void DrawRectangle(
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

        public override void FillRectangle(
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

        public override void DrawRoundedRectangle(
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

        public override void FillRoundedRectangle(
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

        public override void FillEllipse(
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
        public override void FillEllipseShadow(
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

        public override void DrawLine(
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

        public override void DrawArc(
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

        public override void DrawText(
            TextLayout layout,
            Point origin,
            Color color)
        {
            ((TextLayoutRenderer)layout.Native).Draw(this._canvas, origin, color);
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
