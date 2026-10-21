using System;
using System.Collections.Generic;
using Android.Content;
using Android.Graphics;
using Android.Text;
using Android.Text.Style;
using XForms.Platform;

namespace XForms.Android
{
    /// <summary>
    /// Typefaces for font families (registered or installed), weights and
    /// styles, and styled text for spans.
    /// </summary>
    internal static class Fonts
    {
        private static readonly Dictionary<string, Typeface> Registered = new Dictionary<string, Typeface>(StringComparer.OrdinalIgnoreCase);

        public static void Register(
            Context context,
            string familyName,
            string fileName)
        {
            Registered[familyName] = System.IO.Path.IsPathRooted(fileName)
                ? Typeface.CreateFromFile(fileName)
                : Typeface.CreateFromAsset(context.Assets, fileName);
        }

        public static Typeface GetTypeface(
            string family,
            FontWeight weight,
            FontStyle style)
        {
            Typeface baseTypeface;
            if ((null != family) && Registered.TryGetValue(family, out var registered))
            {
                baseTypeface = registered;
            }
            else
            {
                baseTypeface = Typeface.Create(family ?? "sans-serif", TypefaceStyle.Normal);
            }

            bool isItalic = (style == FontStyle.Italic);
            if (OperatingSystem.IsAndroidVersionAtLeast(28))
            {
                return Typeface.Create(baseTypeface, (int)weight, isItalic);
            }

            var typefaceStyle = (weight >= FontWeight.SemiBold)
                ? (isItalic ? TypefaceStyle.BoldItalic : TypefaceStyle.Bold)
                : (isItalic ? TypefaceStyle.Italic : TypefaceStyle.Normal);
            return Typeface.Create(baseTypeface, typefaceStyle);
        }

        /// <summary>
        /// The text with the spans' styles.
        /// </summary>
        public static Java.Lang.ICharSequence Styled(
            string text,
            IReadOnlyList<TextSpan> spans,
            string family,
            FontWeight weight,
            FontStyle style)
        {
            text ??= string.Empty;
            if ((null == spans) || (0 == spans.Count))
            {
                return new Java.Lang.String(text);
            }

            var styled = new SpannableString(text);
            foreach (var span in spans)
            {
                int start = Math.Max(0, Math.Min(span.Start, text.Length));
                int end = Math.Max(start, Math.Min(span.Start + span.Length, text.Length));
                if (end <= start)
                {
                    continue;
                }

                if (span.FontWeight.HasValue || span.FontStyle.HasValue)
                {
                    var typeface = GetTypeface(family, span.FontWeight ?? weight, span.FontStyle ?? style);
                    if (OperatingSystem.IsAndroidVersionAtLeast(28))
                    {
                        styled.SetSpan(new TypefaceSpan(typeface), start, end, SpanTypes.ExclusiveExclusive);
                    }
                    else
                    {
                        styled.SetSpan(new StyleSpan(typeface.Style), start, end, SpanTypes.ExclusiveExclusive);
                    }
                }

                if (span.Foreground.HasValue)
                {
                    styled.SetSpan(new ForegroundColorSpan(span.Foreground.Value.ToAndroidColor()), start, end, SpanTypes.ExclusiveExclusive);
                }

                if (span.Underline)
                {
                    styled.SetSpan(new UnderlineSpan(), start, end, SpanTypes.ExclusiveExclusive);
                }

                if (span.Strikethrough)
                {
                    styled.SetSpan(new StrikethroughSpan(), start, end, SpanTypes.ExclusiveExclusive);
                }
            }

            return styled;
        }
    }

    /// <summary>
    /// A <see cref="TextLayout"/> on Android's StaticLayout.
    /// </summary>
    internal sealed class TextLayoutRenderer : ITextLayoutRenderer
    {
        private readonly Context _context;

        public TextLayoutRenderer(
            Context context,
            TextLayout textLayout)
        {
            this._context = context;
            var format = textLayout.Format;
            string text = textLayout.Text;

            this.Paint = new TextPaint(PaintFlags.AntiAlias)
            {
                TextSize = NativeConversions.ScaledPixelsToPixels(context, format.FontSize),
            };
            this.Paint.SetTypeface(Fonts.GetTypeface(format.FontFamily, format.FontWeight, format.FontStyle));

            var styled = Fonts.Styled(text, textLayout.Spans, format.FontFamily, format.FontWeight, format.FontStyle);

            float desired = global::Android.Text.Layout.GetDesiredWidth(styled, this.Paint);
            bool hasMaxWidth = !float.IsInfinity(textLayout.MaxWidth);
            float maxWidthPixels = hasMaxWidth ? NativeConversions.DevicePixelsToPixels(context, textLayout.MaxWidth) : float.PositiveInfinity;
            bool wraps = (format.TextWrapping == TextWrapping.Wrap) && hasMaxWidth;
            bool trims = (format.TextTrimming == TextTrimming.CharacterEllipsis) || (0 < format.MaxLines);

            // Lay out across the maximum width (for alignment), or the text's own
            // when it's wider and may overflow
            int width;
            if (!hasMaxWidth)
            {
                width = (int)Math.Ceiling(desired);
            }
            else if (wraps || trims)
            {
                width = (int)Math.Floor(maxWidthPixels);
            }
            else
            {
                width = (int)Math.Ceiling(Math.Max(desired, maxWidthPixels));
            }

            width = Math.Max(1, width);

            int maxLines = (0 < format.MaxLines) ? format.MaxLines : (wraps ? int.MaxValue : 1);
            this.Layout = this.Build(styled, width, format, maxLines, trims);

            // Show only the lines that fit in the maximum height
            if (!float.IsInfinity(textLayout.MaxHeight) && (this.Layout.LineCount > 1))
            {
                float maxHeightPixels = NativeConversions.DevicePixelsToPixels(context, textLayout.MaxHeight);
                int fitting = 0;
                while ((fitting < this.Layout.LineCount) && (this.Layout.GetLineBottom(fitting) <= maxHeightPixels + 0.5f))
                {
                    fitting += 1;
                }

                fitting = Math.Max(1, fitting);
                if (fitting < this.Layout.LineCount)
                {
                    this.Layout = this.Build(styled, width, format, fitting, true);
                }
            }

            float textWidth = 0;
            for (int line = 0; line < this.Layout.LineCount; line += 1)
            {
                textWidth = Math.Max(textWidth, this.Layout.GetLineWidth(line));
            }

            this.LineCount = this.Layout.LineCount;
            this.Size = new Size(
                Math.Min((float)Math.Ceiling(NativeConversions.PixelsToDevicePixels(context, textWidth)), textLayout.MaxWidth),
                Math.Min((float)Math.Ceiling(NativeConversions.PixelsToDevicePixels(context, this.Layout.Height)), textLayout.MaxHeight));
        }

        private StaticLayout Build(
            Java.Lang.ICharSequence text,
            int width,
            TextFormat format,
            int maxLines,
            bool trims)
        {
            var alignment = (format.HorizontalAlignment == HorizontalAlignment.Center) ? global::Android.Text.Layout.Alignment.AlignCenter
                : (format.HorizontalAlignment == HorizontalAlignment.Right) ? global::Android.Text.Layout.Alignment.AlignOpposite
                : global::Android.Text.Layout.Alignment.AlignNormal;

            var builder = StaticLayout.Builder.Obtain(text, 0, text.Length(), this.Paint, width)
                .SetAlignment(alignment)
                .SetIncludePad(false)
                .SetMaxLines(maxLines);

            if (trims)
            {
                builder.SetEllipsize(TextUtils.TruncateAt.End);
            }

            return builder.Build();
        }

        public TextPaint Paint { get; }

        public StaticLayout Layout { get; private set; }

        public Size Size { get; }

        public int LineCount { get; }

        public void Draw(
            Canvas canvas,
            Point origin,
            Color color)
        {
            this.Paint.Color = color.ToAndroidColor();
            canvas.Save();
            canvas.Translate(
                NativeConversions.DevicePixelsToPixels(this._context, origin.X),
                NativeConversions.DevicePixelsToPixels(this._context, origin.Y));
            this.Layout.Draw(canvas);
            canvas.Restore();
        }

        public void Dispose()
        {
            this.Layout?.Dispose();
            this.Paint.Dispose();
        }
    }
}
