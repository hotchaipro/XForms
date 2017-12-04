using System;
using Android.Content;

namespace XForms.Android
{
    internal static class NativeConversions
    {
        public static float PixelsToDevicePixels(
            Context context,
            float pixels)
        {
            return pixels / context.Resources.DisplayMetrics.Density;
        }

        public static float DevicePixelsToPixels(
            Context context,
            float dp)
        {
            return dp * context.Resources.DisplayMetrics.Density;
        }

        public static float PixelsToScaledPixels(
            Context context,
            float pixels)
        {
            return pixels / context.Resources.DisplayMetrics.ScaledDensity;
        }

        public static float ScaledPixelsToPixels(
            Context context,
            float dp)
        {
            return dp * context.Resources.DisplayMetrics.ScaledDensity;
        }

        public static float DimensionFromAndroidDimension(
            Context context,
            int nativeDimension)
        {
            if (nativeDimension == global::Android.Views.ViewGroup.LayoutParams.MatchParent)
            {
                return Dimension.Auto;
            }
            else if (nativeDimension == global::Android.Views.ViewGroup.LayoutParams.WrapContent)
            {
                return Dimension.Auto;
            }

            return PixelsToDevicePixels(context, nativeDimension);
        }

        public static LayoutAlignment FromAndroidHorizontalLayoutGravityFlags(
            this global::Android.Views.GravityFlags gravityFlags)
        {
            LayoutAlignment alignment;

            var horizontalGravityFlags = gravityFlags & global::Android.Views.GravityFlags.HorizontalGravityMask;

            if (horizontalGravityFlags == global::Android.Views.GravityFlags.Left)
            {
                alignment = LayoutAlignment.Start;
            }
            else if (horizontalGravityFlags == global::Android.Views.GravityFlags.Right)
            {
                alignment = LayoutAlignment.End;
            }
            else if (horizontalGravityFlags == global::Android.Views.GravityFlags.CenterHorizontal)
            {
                alignment = LayoutAlignment.Center;
            }
            else if (horizontalGravityFlags ==  global::Android.Views.GravityFlags.FillHorizontal)
            {
                alignment = LayoutAlignment.Fill;
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(gravityFlags), gravityFlags, "Unsupported GravityFlags.");
            }

            return alignment;
        }

        public static global::Android.Views.GravityFlags ToAndroidHorizontalLayoutGravityFlags(
            this LayoutAlignment layoutAlignment)
        {
            global::Android.Views.GravityFlags gravityFlags;

            if (layoutAlignment == LayoutAlignment.Start)
            {
                gravityFlags = global::Android.Views.GravityFlags.Left;
            }
            else if (layoutAlignment == LayoutAlignment.End)
            {
                gravityFlags = global::Android.Views.GravityFlags.Right;
            }
            else if (layoutAlignment == LayoutAlignment.Center)
            {
                gravityFlags = global::Android.Views.GravityFlags.CenterHorizontal;
            }
            else if (layoutAlignment == LayoutAlignment.Fill)
            {
                gravityFlags = global::Android.Views.GravityFlags.FillHorizontal;
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(layoutAlignment), layoutAlignment, "Unsupported alignment.");
            }

            return gravityFlags;
        }

        public static LayoutAlignment FromAndroidVerticalLayoutGravityFlags(
            this global::Android.Views.GravityFlags gravityFlags)
        {
            LayoutAlignment alignment;

            var verticalGravityFlags = gravityFlags & global::Android.Views.GravityFlags.VerticalGravityMask;

            if (verticalGravityFlags == global::Android.Views.GravityFlags.Top)
            {
                alignment = LayoutAlignment.Start;
            }
            else if (verticalGravityFlags == global::Android.Views.GravityFlags.Bottom)
            {
                alignment = LayoutAlignment.End;
            }
            else if (verticalGravityFlags == global::Android.Views.GravityFlags.CenterVertical)
            {
                alignment = LayoutAlignment.Center;
            }
            else if (verticalGravityFlags == global::Android.Views.GravityFlags.FillVertical)
            {
                alignment = LayoutAlignment.Fill;
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(gravityFlags), gravityFlags, "Unsupported GravityFlags.");
            }

            return alignment;
        }

        public static global::Android.Views.GravityFlags ToAndroidVerticalLayoutGravityFlags(
            this LayoutAlignment layoutAlignment)
        {
            global::Android.Views.GravityFlags gravityFlags;

            if (layoutAlignment == LayoutAlignment.Start)
            {
                gravityFlags = global::Android.Views.GravityFlags.Top;
            }
            else if (layoutAlignment == LayoutAlignment.End)
            {
                gravityFlags = global::Android.Views.GravityFlags.Bottom;
            }
            else if (layoutAlignment == LayoutAlignment.Center)
            {
                gravityFlags = global::Android.Views.GravityFlags.CenterVertical;
            }
            else if (layoutAlignment == LayoutAlignment.Fill)
            {
                gravityFlags = global::Android.Views.GravityFlags.FillVertical;
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(layoutAlignment), layoutAlignment, "Unsupported alignment.");
            }

            return gravityFlags;
        }

        public static TextAlignment FromAndroidHorizontalGravityFlags(
            this global::Android.Views.GravityFlags gravityFlags)
        {
            TextAlignment alignment;

            var horizontalGravityFlags = gravityFlags & global::Android.Views.GravityFlags.HorizontalGravityMask;

            if (horizontalGravityFlags == global::Android.Views.GravityFlags.Left)
            {
                alignment = TextAlignment.Start;
            }
            else if (horizontalGravityFlags == global::Android.Views.GravityFlags.Right)
            {
                alignment = TextAlignment.End;
            }
            else if (horizontalGravityFlags == global::Android.Views.GravityFlags.CenterHorizontal)
            {
                alignment = TextAlignment.Center;
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(gravityFlags), gravityFlags, "Unsupported alignment.");
            }

            return alignment;
        }

        public static global::Android.Views.GravityFlags ToAndroidHorizontalGravityFlags(
            this TextAlignment textAlignment)
        {
            global::Android.Views.GravityFlags gravityFlags;

            if (textAlignment == TextAlignment.Start)
            {
                gravityFlags = global::Android.Views.GravityFlags.Left;
            }
            else if (textAlignment == TextAlignment.End)
            {
                gravityFlags = global::Android.Views.GravityFlags.Right;
            }
            else if (textAlignment == TextAlignment.Center)
            {
                gravityFlags = global::Android.Views.GravityFlags.CenterHorizontal;
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(textAlignment), textAlignment, "Unsupported alignment.");
            }

            return gravityFlags;
        }

        public static TextAlignment FromAndroidVerticalGravityFlags(
            this global::Android.Views.GravityFlags gravityFlags)
        {
            TextAlignment alignment;

            var verticalGravityFlags = gravityFlags & global::Android.Views.GravityFlags.VerticalGravityMask;

            if (verticalGravityFlags == global::Android.Views.GravityFlags.Top)
            {
                alignment = TextAlignment.Start;
            }
            else if (verticalGravityFlags == global::Android.Views.GravityFlags.Bottom)
            {
                alignment = TextAlignment.End;
            }
            else if (verticalGravityFlags == global::Android.Views.GravityFlags.CenterVertical)
            {
                alignment = TextAlignment.Center;
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(gravityFlags), gravityFlags, "Unsupported alignment.");
            }

            return alignment;
        }

        public static global::Android.Views.GravityFlags ToAndroidVerticalGravityFlags(
            this TextAlignment textAlignment)
        {
            global::Android.Views.GravityFlags gravityFlags;

            if (textAlignment == TextAlignment.Start)
            {
                gravityFlags = global::Android.Views.GravityFlags.Top;
            }
            else if (textAlignment == TextAlignment.End)
            {
                gravityFlags = global::Android.Views.GravityFlags.Bottom;
            }
            else if (textAlignment == TextAlignment.Center)
            {
                gravityFlags = global::Android.Views.GravityFlags.CenterVertical;
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(textAlignment), textAlignment, "Unsupported alignment.");
            }

            return gravityFlags;
        }

        public static FontStyle FromAndroidTypefaceStyle(
            this global::Android.Graphics.TypefaceStyle typefaceStyle)
        {
            FontStyle fontStyle;

            if (typefaceStyle.HasFlag(global::Android.Graphics.TypefaceStyle.Italic))
            {
                fontStyle = FontStyle.Italic;
            }
            else
            {
                fontStyle = FontStyle.Normal;
            }

            return fontStyle;
        }

        public static global::Android.Graphics.TypefaceStyle ToAndroidTypefaceStyle(
            this FontStyle fontStyle)
        {
            global::Android.Graphics.TypefaceStyle typeFaceStyle;

            if (fontStyle == FontStyle.Italic)
            {
                typeFaceStyle = global::Android.Graphics.TypefaceStyle.Italic;
            }
            else if (fontStyle == FontStyle.Normal)
            {
                typeFaceStyle = global::Android.Graphics.TypefaceStyle.Normal;
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(fontStyle), fontStyle, "Unsupported font style.");
            }

            return typeFaceStyle;
        }

        public static Color FromAndroidColor(
            this global::Android.Graphics.Color androidColor)
        {
            return Color.FromArgb(androidColor.A, androidColor.R, androidColor.G, androidColor.B);
        }

        public static global::Android.Graphics.Color ToAndroidColor(
            this Color color)
        {
            return new global::Android.Graphics.Color(color.R, color.G, color.B, color.A);
        }

        private static readonly DateTime AndroidBaseDateUtc = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static DateTime FromAndroidDateUtc(
            long androidDate)
        {
            return AndroidBaseDateUtc.AddMilliseconds(androidDate);
        }

        public static long ToAndroidDateUtc(
            DateTime date)
        {
            return (long)date.Subtract(AndroidBaseDateUtc).TotalMilliseconds;
        }
    }
}
