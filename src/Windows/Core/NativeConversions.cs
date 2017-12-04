using System;
using Microsoft.Graphics.Canvas.Text;
using Windows.UI.Xaml;

namespace XForms.Windows
{
    internal static class NativeConversions
    {
        public static LayoutAlignment ToLayoutAlignment(
            this HorizontalAlignment horizontalAlignment)
        {
            LayoutAlignment alignment;

            if (horizontalAlignment == HorizontalAlignment.Left)
            {
                alignment = LayoutAlignment.Start;
            }
            else if (horizontalAlignment == HorizontalAlignment.Right)
            {
                alignment = LayoutAlignment.End;
            }
            else if (horizontalAlignment == HorizontalAlignment.Center)
            {
                alignment = LayoutAlignment.Center;
            }
            else if (horizontalAlignment == HorizontalAlignment.Stretch)
            {
                alignment = LayoutAlignment.Fill;
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(horizontalAlignment), horizontalAlignment, "Unsupported alignment.");
            }

            return alignment;
        }

        public static HorizontalAlignment ToXamlHorizontalAlignment(
            this LayoutAlignment layoutAlignment)
        {
            HorizontalAlignment alignment;

            if (layoutAlignment == LayoutAlignment.Start)
            {
                alignment = HorizontalAlignment.Left;
            }
            else if (layoutAlignment == LayoutAlignment.End)
            {
                alignment = HorizontalAlignment.Right;
            }
            else if (layoutAlignment == LayoutAlignment.Center)
            {
                alignment = HorizontalAlignment.Center;
            }
            else if (layoutAlignment == LayoutAlignment.Fill)
            {
                alignment = HorizontalAlignment.Stretch;
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(layoutAlignment), layoutAlignment, "Unsupported alignment.");
            }

            return alignment;
        }

        public static CanvasHorizontalAlignment ToCanvasHorizontalAlignment(
            this LayoutAlignment layoutAlignment)
        {
            CanvasHorizontalAlignment alignment;

            if (layoutAlignment == LayoutAlignment.Start)
            {
                alignment = CanvasHorizontalAlignment.Left;
            }
            else if (layoutAlignment == LayoutAlignment.End)
            {
                alignment = CanvasHorizontalAlignment.Right;
            }
            else if (layoutAlignment == LayoutAlignment.Center)
            {
                alignment = CanvasHorizontalAlignment.Center;
            }
            else if (layoutAlignment == LayoutAlignment.Fill)
            {
                alignment = CanvasHorizontalAlignment.Justified;
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(layoutAlignment), layoutAlignment, "Unsupported alignment.");
            }

            return alignment;
        }

        public static LayoutAlignment ToLayoutAlignment(
            this VerticalAlignment verticalAlignment)
        {
            LayoutAlignment alignment;

            if (verticalAlignment == VerticalAlignment.Top)
            {
                alignment = LayoutAlignment.Start;
            }
            else if (verticalAlignment == VerticalAlignment.Bottom)
            {
                alignment = LayoutAlignment.End;
            }
            else if (verticalAlignment == VerticalAlignment.Center)
            {
                alignment = LayoutAlignment.Center;
            }
            else if (verticalAlignment == VerticalAlignment.Stretch)
            {
                alignment = LayoutAlignment.Fill;
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(verticalAlignment), verticalAlignment, "Unsupported alignment.");
            }

            return alignment;
        }

        public static VerticalAlignment ToXamlVerticalAlignment(
            this LayoutAlignment layoutAlignment)
        {
            VerticalAlignment alignment;

            if (layoutAlignment == LayoutAlignment.Start)
            {
                alignment = VerticalAlignment.Top;
            }
            else if (layoutAlignment == LayoutAlignment.End)
            {
                alignment = VerticalAlignment.Bottom;
            }
            else if (layoutAlignment == LayoutAlignment.Center)
            {
                alignment = VerticalAlignment.Center;
            }
            else if (layoutAlignment == LayoutAlignment.Fill)
            {
                alignment = VerticalAlignment.Stretch;
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(layoutAlignment), layoutAlignment, "Unsupported alignment.");
            }

            return alignment;
        }

        public static CanvasVerticalAlignment ToCanvasVerticalAlignment(
            this LayoutAlignment layoutAlignment)
        {
            CanvasVerticalAlignment alignment;

            if (layoutAlignment == LayoutAlignment.Start)
            {
                alignment = CanvasVerticalAlignment.Top;
            }
            else if (layoutAlignment == LayoutAlignment.End)
            {
                alignment = CanvasVerticalAlignment.Bottom;
            }
            else if (layoutAlignment == LayoutAlignment.Center)
            {
                alignment = CanvasVerticalAlignment.Center;
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(layoutAlignment), layoutAlignment, "Unsupported alignment.");
            }

            return alignment;
        }

        public static TextAlignment ToTextAlignment(
            this global::Windows.UI.Xaml.TextAlignment textAlignment)
        {
            TextAlignment alignment;

            if (textAlignment == global::Windows.UI.Xaml.TextAlignment.Left)
            {
                alignment = TextAlignment.Start;
            }
            else if (textAlignment == global::Windows.UI.Xaml.TextAlignment.Right)
            {
                alignment = TextAlignment.End;
            }
            else if (textAlignment == global::Windows.UI.Xaml.TextAlignment.Center)
            {
                alignment = TextAlignment.Center;
            }
            //else if (textAlignment == global::Windows.UI.Xaml.TextAlignment.Justify)
            //{
            //    alignment = TextAlignment.Fill;
            //}
            else
            {
                throw new ArgumentOutOfRangeException(nameof(textAlignment), textAlignment, "Unsupported alignment.");
            }

            return alignment;
        }

        public static global::Windows.UI.Xaml.TextAlignment ToXamlTextAlignment(
            this TextAlignment textAlignment)
        {
            global::Windows.UI.Xaml.TextAlignment alignment;

            if (textAlignment == TextAlignment.Start)
            {
                alignment = global::Windows.UI.Xaml.TextAlignment.Left;
            }
            else if (textAlignment == TextAlignment.End)
            {
                alignment = global::Windows.UI.Xaml.TextAlignment.Right;
            }
            else if (textAlignment == TextAlignment.Center)
            {
                alignment = global::Windows.UI.Xaml.TextAlignment.Center;
            }
            //else if (textAlignment == TextAlignment.Fill)
            //{
            //    alignment = global::Windows.UI.Xaml.TextAlignment.Justify;
            //}
            else
            {
                throw new ArgumentOutOfRangeException(nameof(textAlignment), textAlignment, "Unsupported alignment.");
            }

            return alignment;
        }


        public static TextAlignment ToTextAlignment(
            this global::Windows.UI.Xaml.VerticalAlignment verticalAlignment)
        {
            TextAlignment alignment;

            if (verticalAlignment == global::Windows.UI.Xaml.VerticalAlignment.Top)
            {
                alignment = TextAlignment.Start;
            }
            else if (verticalAlignment == global::Windows.UI.Xaml.VerticalAlignment.Bottom)
            {
                alignment = TextAlignment.End;
            }
            else if (verticalAlignment == global::Windows.UI.Xaml.VerticalAlignment.Center)
            {
                alignment = TextAlignment.Center;
            }
            //else if (verticalAlignment == global::Windows.UI.Xaml.VerticalAlignment.Stretch)
            //{
            //    alignment = TextAlignment.Fill;
            //}
            else
            {
                throw new ArgumentOutOfRangeException(nameof(verticalAlignment), verticalAlignment, "Unsupported alignment.");
            }

            return alignment;
        }

        public static global::Windows.UI.Xaml.VerticalAlignment ToXamlVerticalAlignment(
            this TextAlignment textAlignment)
        {
            global::Windows.UI.Xaml.VerticalAlignment alignment;

            if (textAlignment == TextAlignment.Start)
            {
                alignment = global::Windows.UI.Xaml.VerticalAlignment.Top;
            }
            else if (textAlignment == TextAlignment.End)
            {
                alignment = global::Windows.UI.Xaml.VerticalAlignment.Bottom;
            }
            else if (textAlignment == TextAlignment.Center)
            {
                alignment = global::Windows.UI.Xaml.VerticalAlignment.Center;
            }
            //else if (textAlignment == TextAlignment.Fill)
            //{
            //    alignment = global::Windows.UI.Xaml.VerticalAlignment.Stretch;
            //}
            else
            {
                throw new ArgumentOutOfRangeException(nameof(textAlignment), textAlignment, "Unsupported alignment.");
            }

            return alignment;
        }

        public static Size ToSize(
            this global::Windows.Foundation.Size windowsSize)
        {
            return new Size((float)windowsSize.Width, (float)windowsSize.Height);
        }

        public static Thickness ToThickness(
            this global::Windows.UI.Xaml.Thickness xamlThickness)
        {
            return new Thickness((float)xamlThickness.Left, (float)xamlThickness.Top, (float)xamlThickness.Right, (float)xamlThickness.Bottom);
        }

        public static global::Windows.UI.Xaml.Thickness ToXamlThickness(
            this Thickness thickness)
        {
            return new global::Windows.UI.Xaml.Thickness(thickness.Left, thickness.Top, thickness.Right, thickness.Bottom);
        }

        public static Color ToColor(
            this global::Windows.UI.Color color)
        {
            return Color.FromArgb(color.A, color.R, color.G, color.B);
        }

        public static global::Windows.UI.Color ToXamlColor(
            this Color color)
        {
            return global::Windows.UI.Color.FromArgb(color.A, color.R, color.G, color.B);
        }

        public static global::Windows.Foundation.Rect ToXamlRect(
            this Rectangle rectangle)
        {
            return new global::Windows.Foundation.Rect(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
        }

        public static global::Windows.UI.Xaml.Media.Animation.EasingMode ToXamlEasingMode(
            this EasingMode easingMode)
        {
            global::Windows.UI.Xaml.Media.Animation.EasingMode windowsEasingMode;

            if (easingMode == EasingMode.EaseIn)
            {
                windowsEasingMode = global::Windows.UI.Xaml.Media.Animation.EasingMode.EaseIn;
            }
            else if (easingMode == EasingMode.EaseOut)
            {
                windowsEasingMode = global::Windows.UI.Xaml.Media.Animation.EasingMode.EaseOut;
            }
            else if (easingMode == EasingMode.EaseInOut)
            {
                windowsEasingMode = global::Windows.UI.Xaml.Media.Animation.EasingMode.EaseInOut;
            }
            else
            {
                throw new NotSupportedException("Unsupported easing mode.");
            }

            return windowsEasingMode;
        }

        public static FontStyle ToFontStyle(
            this global::Windows.UI.Text.FontStyle xamlFontStyle)
        {
            FontStyle fontStyle;

            if (xamlFontStyle == global::Windows.UI.Text.FontStyle.Normal)
            {
                fontStyle = FontStyle.Normal;
            }
            else if (xamlFontStyle == global::Windows.UI.Text.FontStyle.Italic)
            {
                fontStyle = FontStyle.Italic;
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(xamlFontStyle), xamlFontStyle, "Unsupported font style.");
            }

            return fontStyle;
        }

        public static global::Windows.UI.Text.FontStyle ToXamlFontStyle(
            this FontStyle fontStyle)
        {
            global::Windows.UI.Text.FontStyle xamlFontStyle;

            if (fontStyle == FontStyle.Normal)
            {
                xamlFontStyle = global::Windows.UI.Text.FontStyle.Normal;
            }
            else if (fontStyle == FontStyle.Italic)
            {
                xamlFontStyle = global::Windows.UI.Text.FontStyle.Italic;
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(fontStyle), fontStyle, "Unsupported font style.");
            }

            return xamlFontStyle;
        }

        public static FontWeight ToFontWeight(
            this global::Windows.UI.Text.FontWeight xamlFontWeight)
        {
            FontWeight fontWeight;

            if (xamlFontWeight.Weight == global::Windows.UI.Text.FontWeights.Normal.Weight)
            {
                fontWeight = FontWeight.Medium;
            }
            else if (xamlFontWeight.Weight == global::Windows.UI.Text.FontWeights.Light.Weight)
            {
                fontWeight = FontWeight.Light;
            }
            else if (xamlFontWeight.Weight == global::Windows.UI.Text.FontWeights.SemiLight.Weight)
            {
                fontWeight = FontWeight.Normal;
            }
            else if (xamlFontWeight.Weight == global::Windows.UI.Text.FontWeights.SemiBold.Weight)
            {
                fontWeight = FontWeight.SemiBold;
            }
            else if (xamlFontWeight.Weight == global::Windows.UI.Text.FontWeights.Bold.Weight)
            {
                fontWeight = FontWeight.Bold;
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(xamlFontWeight), xamlFontWeight, "Unsupported font weight.");
            }

            return fontWeight;
        }

        public static global::Windows.UI.Text.FontWeight ToXamlFontWeight(
            this FontWeight fontWeight)
        {
            global::Windows.UI.Text.FontWeight xamlFontWeight;

            if (fontWeight == FontWeight.Medium)
            {
                xamlFontWeight = global::Windows.UI.Text.FontWeights.Normal;
            }
            else if (fontWeight == FontWeight.Light)
            {
                xamlFontWeight = global::Windows.UI.Text.FontWeights.Light;
            }
            else if (fontWeight == FontWeight.Normal)
            {
                xamlFontWeight = global::Windows.UI.Text.FontWeights.SemiLight;
            }
            else if (fontWeight == FontWeight.SemiBold)
            {
                xamlFontWeight = global::Windows.UI.Text.FontWeights.SemiBold;
            }
            else if (fontWeight == FontWeight.Bold)
            {
                xamlFontWeight = global::Windows.UI.Text.FontWeights.Bold;
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(fontWeight), fontWeight, "Unsupported font weight.");
            }

            return xamlFontWeight;
        }
    }
}
