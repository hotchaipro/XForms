using System;
using CoreGraphics;
using UIKit;
using XForms.Controls;

namespace XForms.iOS
{
    internal static class NativeConversions
    {
        public static Size ToSize(
            this CGSize cgSize)
        {
            return new Size(new Dimension((float)cgSize.Width), new Dimension((float)cgSize.Height));
        }

        public static Color ToColor(
            this UIColor uiColor)
        {
            if (null == uiColor)
            {
                return Colors.Transparent;
            }

            nfloat r, g, b, a;
            uiColor.GetRGBA(out r, out g, out b, out a);
            return Color.FromArgb((float)a, (float)r, (float)g, (float)b);
        }

        public static UIColor ToUIColor(
            this Color color)
        {
            float r = (float)color.R / byte.MaxValue;
            float g = (float)color.G / byte.MaxValue;
            float b = (float)color.B / byte.MaxValue;
            float a = (float)color.A / byte.MaxValue;

            return UIColor.FromRGBA(r, g, b, a);
        }

        public static CGColor ToCGColor(
            this Color color)
        {
            float r = (float)color.R / byte.MaxValue;
            float g = (float)color.G / byte.MaxValue;
            float b = (float)color.B / byte.MaxValue;
            float a = (float)color.A / byte.MaxValue;

            return new CGColor(r, g, b, a);
        }

        public static CGRect ToCGRect(
            this Rect rectangle)
        {
            return new CGRect(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
        }

        public static Rect ToRectangle(
            this CGRect rect)
        {
            return new Rect((float)rect.X, (float)rect.Y, (float)rect.Width, (float)rect.Height);
        }

        public static TextAlignment ToTextAlignment(
            this UITextAlignment uiTextAlignment)
        {
            TextAlignment value;

            if (uiTextAlignment == UITextAlignment.Left)
            {
                value = TextAlignment.Start;
            }
            else if (uiTextAlignment == UITextAlignment.Right)
            {
                value = TextAlignment.End;
            }
            else if (uiTextAlignment == UITextAlignment.Center)
            {
                value = TextAlignment.Center;
            }
            else
            {
                throw new NotSupportedException();
            }

            return value;
        }

        public static UITextAlignment ToUITextAlignment(
            this HorizontalAlignment alignment)
        {
            switch (alignment)
            {
                case HorizontalAlignment.Center:
                    return UITextAlignment.Center;

                case HorizontalAlignment.Right:
                    return UITextAlignment.Right;

                case HorizontalAlignment.Stretch:
                    return UITextAlignment.Justified;

                default:
                    return UITextAlignment.Left;
            }
        }

        public static UITextAlignment FromTextAlignment(
            this TextAlignment textAlignment)
        {
            UITextAlignment value;

            if (textAlignment == TextAlignment.Start)
            {
                value = UITextAlignment.Left;
            }
            else if (textAlignment == TextAlignment.End)
            {
                value = UITextAlignment.Right;
            }
            else if (textAlignment == TextAlignment.Center)
            {
                value = UITextAlignment.Center;
            }
            else
            {
                throw new NotSupportedException();
            }

            return value;
        }

        public static UIFontWeight ToUIFontWeight(
            this FontWeight fontWeight)
        {
            UIFontWeight value;

            if (fontWeight == FontWeight.Light)
            {
                value = UIFontWeight.Light;
            }
            else if (fontWeight == FontWeight.Normal)
            {
                value = UIFontWeight.Regular;
            }
            else if (fontWeight == FontWeight.Medium)
            {
                value = UIFontWeight.Medium;
            }
            else if (fontWeight == FontWeight.SemiBold)
            {
                value = UIFontWeight.Semibold;
            }
            else if (fontWeight == FontWeight.Bold)
            {
                value = UIFontWeight.Bold;
            }
            else
            {
                throw new NotSupportedException();
            }

            return value;
        }
    }
}
