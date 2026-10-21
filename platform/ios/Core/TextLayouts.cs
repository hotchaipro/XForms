using System;
using System.Collections.Generic;
using CoreGraphics;
using CoreText;
using Foundation;
using UIKit;

namespace XForms.iOS
{
    /// <summary>
    /// Fonts for font families (registered or installed), weights and styles,
    /// and attributed text for spans.
    /// </summary>
    internal static class Fonts
    {
        // The name the app uses, and the font's own family name
        private static readonly Dictionary<string, string> Registered = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public static void Register(
            string familyName,
            string fileName)
        {
            string path = System.IO.Path.IsPathRooted(fileName)
                ? fileName
                : System.IO.Path.Combine(NSBundle.MainBundle.ResourcePath, fileName);

            using (var url = NSUrl.FromFilename(path))
            {
                CTFontManager.RegisterFontsForUrl(url, CTFontManagerScope.Process);
            }

            using (var provider = new CGDataProvider(path))
            using (var cgFont = CGFont.CreateFromProvider(provider))
            using (var ctFont = new CTFont(cgFont, 12, null))
            {
                Registered[familyName] = ctFont.FamilyName;
            }
        }

        public static UIFont GetFont(
            string family,
            float size,
            FontWeight weight,
            FontStyle style)
        {
            // Font sizes follow the Dynamic Type text size
            size = (float)UIFontMetrics.DefaultMetrics.GetScaledValue(size);

            UIFont font;
            bool isSerif = string.Equals(family, FontFamilies.Serif, StringComparison.OrdinalIgnoreCase);
            bool isMonospace = string.Equals(family, FontFamilies.Monospace, StringComparison.OrdinalIgnoreCase);
            if ((null == family) || isSerif || isMonospace || string.Equals(family, FontFamilies.SansSerif, StringComparison.OrdinalIgnoreCase))
            {
                font = UIFont.SystemFontOfSize(size, weight.ToUIFontWeight());
                if (isSerif || isMonospace)
                {
                    var designed = font.FontDescriptor.CreateWithDesign(isSerif ? UIFontDescriptorSystemDesign.Serif : UIFontDescriptorSystemDesign.Monospaced);
                    if (null != designed)
                    {
                        font = UIFont.FromDescriptor(designed, size);
                    }
                }
            }
            else
            {
                if (Registered.TryGetValue(family, out var registered))
                {
                    family = registered;
                }

                var attributes = new UIFontAttributes()
                {
                    Family = family,
                    Traits = new UIFontTraits() { Weight = (float)weight.ToUIFontWeight().GetWeight() },
                };
                font = UIFont.FromDescriptor(new UIFontDescriptor(attributes), size);
            }

            if (style == FontStyle.Italic)
            {
                var italic = font.FontDescriptor.CreateWithTraits(UIFontDescriptorSymbolicTraits.Italic);
                if (null != italic)
                {
                    font = UIFont.FromDescriptor(italic, size);
                }
            }

            return font;
        }

        /// <summary>
        /// The text with its font and color, and the spans' styles.
        /// </summary>
        public static NSMutableAttributedString Styled(
            string text,
            IReadOnlyList<TextSpan> spans,
            string family,
            float size,
            FontWeight weight,
            FontStyle style,
            UIColor color,
            NSParagraphStyle paragraphStyle = null)
        {
            text ??= string.Empty;
            var attributes = new UIStringAttributes()
            {
                Font = GetFont(family, size, weight, style),
                ForegroundColor = color,
            };

            if (null != paragraphStyle)
            {
                attributes.ParagraphStyle = paragraphStyle;
            }

            var styled = new NSMutableAttributedString(text, attributes);
            if (null == spans)
            {
                return styled;
            }

            foreach (var span in spans)
            {
                int start = Math.Max(0, Math.Min(span.Start, text.Length));
                int end = Math.Max(start, Math.Min(span.Start + span.Length, text.Length));
                if (end <= start)
                {
                    continue;
                }

                var spanAttributes = new UIStringAttributes();
                if (span.FontWeight.HasValue || span.FontStyle.HasValue)
                {
                    spanAttributes.Font = GetFont(family, size, span.FontWeight ?? weight, span.FontStyle ?? style);
                }

                if (span.Foreground.HasValue)
                {
                    spanAttributes.ForegroundColor = span.Foreground.Value.ToUIColor();
                }

                if (span.Underline)
                {
                    spanAttributes.UnderlineStyle = NSUnderlineStyle.Single;
                }

                if (span.Strikethrough)
                {
                    spanAttributes.StrikethroughStyle = NSUnderlineStyle.Single;
                }

                styled.AddAttributes(spanAttributes, new NSRange(start, end - start));
            }

            return styled;
        }
    }

    /// <summary>
    /// A <see cref="TextLayout"/> on TextKit.
    /// </summary>
    internal sealed class TextLayoutRenderer : ITextLayoutRenderer
    {
        private readonly TextLayout _textLayout;
        private readonly NSTextStorage _storage;
        private readonly NSLayoutManager _layoutManager;
        private readonly NSTextContainer _container;

        public TextLayoutRenderer(
            TextLayout textLayout)
        {
            this._textLayout = textLayout;
            var format = textLayout.Format;
            bool wraps = (format.TextWrapping == TextWrapping.Wrap) && !float.IsInfinity(textLayout.MaxWidth);
            bool trims = (format.TextTrimming == TextTrimming.CharacterEllipsis) || (0 < format.MaxLines);

            var paragraph = new NSMutableParagraphStyle()
            {
                Alignment = format.HorizontalAlignment.ToUITextAlignment(),
            };

            this._storage = new NSTextStorage();
            this._storage.SetString(Fonts.Styled(textLayout.Text, textLayout.Spans, format.FontFamily, format.FontSize, format.FontWeight, format.FontStyle, UIColor.Black, paragraph));

            this._layoutManager = new NSLayoutManager();
            this._storage.AddLayoutManager(this._layoutManager);

            const float Unlimited = 100000;
            this._container = new NSTextContainer(new CGSize(
                float.IsInfinity(textLayout.MaxWidth) ? Unlimited : textLayout.MaxWidth,
                float.IsInfinity(textLayout.MaxHeight) ? Unlimited : textLayout.MaxHeight))
            {
                LineFragmentPadding = 0,
                MaximumNumberOfLines = (nuint)((0 < format.MaxLines) ? format.MaxLines : (wraps ? 0 : 1)),
                LineBreakMode = trims ? UILineBreakMode.TailTruncation : (wraps ? UILineBreakMode.WordWrap : UILineBreakMode.Clip),
            };
            this._layoutManager.AddTextContainer(this._container);

            var used = this._layoutManager.GetUsedRect(this._container);
            if (float.IsInfinity(textLayout.MaxWidth))
            {
                // Size the container to the text, so alignment works within it
                this._container.Size = new CGSize(Math.Ceiling(used.Width), this._container.Size.Height);
            }

            int lines = 0;
            var glyphRange = this._layoutManager.GetGlyphRange(this._container);
            this._layoutManager.EnumerateLineFragments(glyphRange, (CGRect rect, CGRect usedRect, NSTextContainer container, NSRange range, out bool stop) =>
            {
                stop = false;
                lines += 1;
            });

            this.LineCount = lines;
            this.Size = new Size(
                Math.Min((float)Math.Ceiling(used.Width), textLayout.MaxWidth),
                Math.Min((float)Math.Ceiling(used.Height), textLayout.MaxHeight));
        }

        public Size Size { get; }

        public int LineCount { get; }

        public void Draw(
            Point origin,
            Color color)
        {
            // The color is given when drawing; spans keep their own
            var format = this._textLayout.Format;
            this._storage.SetString(Fonts.Styled(this._textLayout.Text, this._textLayout.Spans, format.FontFamily, format.FontSize, format.FontWeight, format.FontStyle, color.ToUIColor(),
                new NSMutableParagraphStyle() { Alignment = format.HorizontalAlignment.ToUITextAlignment() }));

            var glyphRange = this._layoutManager.GetGlyphRange(this._container);
            this._layoutManager.DrawGlyphs(glyphRange, new CGPoint(origin.X, origin.Y));
        }

        public void Dispose()
        {
            this._container.Dispose();
            this._layoutManager.Dispose();
            this._storage.Dispose();
        }
    }
}
