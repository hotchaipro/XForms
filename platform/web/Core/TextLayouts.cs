using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.Versioning;
using System.Text;

namespace XForms.Web
{
    /// <summary>
    /// Text measured with the browser's canvas and broken into lines here, so
    /// drawn text and text elements break the same way.
    /// </summary>
    [SupportedOSPlatform("browser")]
    internal static class TextLayouts
    {
        public const string DefaultFontFamily = "'Segoe UI', system-ui, -apple-system, Roboto, 'Helvetica Neue', sans-serif";

        private static readonly Dictionary<(string Font, string Text), float> Widths = new Dictionary<(string, string), float>();
        private static readonly Dictionary<string, (float Ascent, float Descent)> Metrics = new Dictionary<string, (float, float)>();

        public static string Font(
            string family,
            float size,
            FontWeight weight,
            FontStyle style)
        {
            // CSS's generic families (serif, monospace) work unquoted; others are
            // names, with the default after them
            string familyCss = string.IsNullOrEmpty(family) ? DefaultFontFamily
                : IsGenericFamily(family) ? family
                : $"'{family}', {DefaultFontFamily}";
            return string.Format(
                CultureInfo.InvariantCulture,
                "{0} {1} {2:0.##}px {3}",
                (style == FontStyle.Italic) ? "italic" : "normal",
                (int)weight,
                Math.Max(1, size),
                familyCss);
        }

        private static bool IsGenericFamily(
            string family)
        {
            switch (family)
            {
                case "serif":
                case "sans-serif":
                case "monospace":
                case "cursive":
                case "fantasy":
                case "system-ui":
                    return true;

                default:
                    return false;
            }
        }

        public static float Measure(
            string font,
            string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return 0;
            }

            if (!Widths.TryGetValue((font, text), out float width))
            {
                width = (float)Js.MeasureText(font, text);
                if (Widths.Count > 20000)
                {
                    Widths.Clear();
                }

                Widths[(font, text)] = width;
            }

            return width;
        }

        public static (float Ascent, float Descent) FontMetrics(
            string font)
        {
            if (!Metrics.TryGetValue(font, out var metrics))
            {
                var values = Js.FontMetrics(font);
                metrics = ((float)values[0], (float)values[1]);
                Metrics[font] = metrics;
            }

            return metrics;
        }
    }

    /// <summary>
    /// A run of text on a line, with its own style.
    /// </summary>
    [SupportedOSPlatform("browser")]
    internal readonly struct TextRun
    {
        public TextRun(string text, float x, float width, string font, FontWeight weight, FontStyle style, Color? foreground, bool underline, bool strikethrough)
        {
            this.Text = text;
            this.X = x;
            this.Width = width;
            this.Font = font;
            this.Weight = weight;
            this.Style = style;
            this.Foreground = foreground;
            this.Underline = underline;
            this.Strikethrough = strikethrough;
        }

        public string Text { get; }

        public float X { get; }

        public float Width { get; }

        public string Font { get; }

        public FontWeight Weight { get; }

        public FontStyle Style { get; }

        public Color? Foreground { get; }

        public bool Underline { get; }

        public bool Strikethrough { get; }
    }

    /// <summary>
    /// A laid-out line: its runs, with x relative to the layout's left edge.
    /// </summary>
    [SupportedOSPlatform("browser")]
    internal sealed class TextLine
    {
        public List<TextRun> Runs { get; } = new List<TextRun>();

        public float Width { get; set; }
    }

    [SupportedOSPlatform("browser")]
    internal sealed class WebTextLayout : ITextLayoutRenderer
    {
        private const string Ellipsis = "…";

        public WebTextLayout(
            TextLayout layout)
        {
            var format = layout.Format;
            this.Font = TextLayouts.Font(format.FontFamily, format.FontSize, format.FontWeight, format.FontStyle);
            var (ascent, descent) = TextLayouts.FontMetrics(this.Font);
            this.Ascent = ascent;
            this.LineHeight = (float)Math.Ceiling(ascent + descent);

            this.Layout(layout);
        }

        public string Font { get; }

        public float Ascent { get; }

        public float LineHeight { get; }

        public List<TextLine> Lines { get; } = new List<TextLine>();

        public Size Size { get; private set; }

        public int LineCount
        {
            get
            {
                return this.Lines.Count;
            }
        }

        public void Dispose()
        {
        }

        // Lines as (start, length) in the text
        private void Layout(
            TextLayout layout)
        {
            var format = layout.Format;
            string text = layout.Text;
            float maxWidth = float.IsInfinity(layout.MaxWidth) ? float.PositiveInfinity : layout.MaxWidth + 0.5f;
            bool isWrapping = (format.TextWrapping == TextWrapping.Wrap) && !float.IsInfinity(maxWidth);

            int maxLines = (format.MaxLines > 0) ? format.MaxLines : int.MaxValue;
            if (!float.IsInfinity(layout.MaxHeight) && (this.LineHeight > 0))
            {
                maxLines = Math.Min(maxLines, Math.Max(1, (int)Math.Floor((layout.MaxHeight + 0.5f) / this.LineHeight)));
            }

            var ranges = new List<(int Start, int Length)>();
            int paragraphStart = 0;
            while (paragraphStart <= text.Length)
            {
                int end = text.IndexOf('\n', paragraphStart);
                if (end < 0)
                {
                    end = text.Length;
                }

                int length = end - paragraphStart;
                if (length > 0 && text[end - 1] == '\r')
                {
                    length -= 1;
                }

                if (isWrapping)
                {
                    this.Wrap(text, paragraphStart, length, maxWidth, ranges);
                }
                else
                {
                    ranges.Add((paragraphStart, length));
                }

                paragraphStart = end + 1;
            }

            // Lines beyond the limit go; the last line shown may end in an ellipsis
            bool isTruncated = ranges.Count > maxLines;
            if (isTruncated)
            {
                ranges.RemoveRange(maxLines, ranges.Count - maxLines);
            }

            float width = 0;
            for (int i = 0; i < ranges.Count; i += 1)
            {
                var (start, length) = ranges[i];
                string lineText = text.Substring(start, length).TrimEnd();
                bool isLast = i == ranges.Count - 1;
                // A line limit trims too, as on the other platforms
                bool trims = (format.TextTrimming != TextTrimming.None) || (format.MaxLines > 0);
                bool needsEllipsis = trims
                    && ((isLast && isTruncated) || (TextLayouts.Measure(this.Font, lineText) > maxWidth));

                var line = needsEllipsis
                    ? this.BuildTrimmedLine(layout, start, lineText, maxWidth)
                    : this.BuildLine(layout, start, lineText.Length);
                this.Lines.Add(line);
                width = Math.Max(width, line.Width);
            }

            // Lines align within the width the text has
            float alignWidth = float.IsInfinity(maxWidth) ? width : layout.MaxWidth;
            float factor = (format.HorizontalAlignment == HorizontalAlignment.Center) ? 0.5f
                : (format.HorizontalAlignment == HorizontalAlignment.Right) ? 1 : 0;
            if (factor > 0)
            {
                foreach (var line in this.Lines)
                {
                    float offset = (alignWidth - line.Width) * factor;
                    for (int r = 0; r < line.Runs.Count; r += 1)
                    {
                        var run = line.Runs[r];
                        line.Runs[r] = new TextRun(run.Text, run.X + offset, run.Width, run.Font, run.Weight, run.Style, run.Foreground, run.Underline, run.Strikethrough);
                    }
                }
            }

            this.Size = new Size((float)Math.Ceiling(width), this.Lines.Count * this.LineHeight);
        }

        // Breaks a paragraph into lines at spaces, and long words anywhere
        private void Wrap(
            string text,
            int start,
            int length,
            float maxWidth,
            List<(int, int)> ranges)
        {
            if (length == 0)
            {
                ranges.Add((start, 0));
                return;
            }

            int end = start + length;
            int lineStart = start;
            int position = start;
            while (position < end)
            {
                // The next word, with the spaces after it
                int wordEnd = position;
                while ((wordEnd < end) && !char.IsWhiteSpace(text[wordEnd]))
                {
                    wordEnd += 1;
                }

                int spaceEnd = wordEnd;
                while ((spaceEnd < end) && char.IsWhiteSpace(text[spaceEnd]))
                {
                    spaceEnd += 1;
                }

                float lineWidth = TextLayouts.Measure(this.Font, text.Substring(lineStart, wordEnd - lineStart));
                if ((lineWidth <= maxWidth) || (position == lineStart))
                {
                    if ((lineWidth > maxWidth) && (position == lineStart))
                    {
                        // A word wider than the line breaks where it must
                        int fit = position + 1;
                        while ((fit < wordEnd) && (TextLayouts.Measure(this.Font, text.Substring(lineStart, fit + 1 - lineStart)) <= maxWidth))
                        {
                            fit += 1;
                        }

                        ranges.Add((lineStart, fit - lineStart));
                        lineStart = position = fit;
                        continue;
                    }

                    position = spaceEnd;
                }
                else
                {
                    ranges.Add((lineStart, position - lineStart));
                    lineStart = position;
                }
            }

            ranges.Add((lineStart, end - lineStart));
        }

        private TextLine BuildLine(
            TextLayout layout,
            int start,
            int length)
        {
            var line = new TextLine();
            var format = layout.Format;
            int end = start + length;
            float x = 0;

            // Runs split where the spans start and end
            int position = start;
            while (position < end)
            {
                int runEnd = end;
                TextSpan? span = null;
                foreach (var candidate in layout.Spans)
                {
                    int spanEnd = candidate.Start + candidate.Length;
                    if ((candidate.Start <= position) && (spanEnd > position))
                    {
                        span = candidate;
                        runEnd = Math.Min(runEnd, spanEnd);
                    }
                    else if ((candidate.Start > position) && (candidate.Start < runEnd))
                    {
                        runEnd = candidate.Start;
                    }
                }

                string runText = layout.Text.Substring(position, runEnd - position);
                var weight = span?.FontWeight ?? format.FontWeight;
                var style = span?.FontStyle ?? format.FontStyle;
                string font = TextLayouts.Font(format.FontFamily, format.FontSize, weight, style);
                float width = TextLayouts.Measure(font, runText);
                line.Runs.Add(new TextRun(runText, x, width, font, weight, style, span?.Foreground, span?.Underline ?? false, span?.Strikethrough ?? false));
                x += width;
                position = runEnd;
            }

            line.Width = x;
            return line;
        }

        // A line cut to fit with an ellipsis, in the text's own style
        private TextLine BuildTrimmedLine(
            TextLayout layout,
            int start,
            string lineText,
            float maxWidth)
        {
            int low = 0, high = lineText.Length;
            while (low < high)
            {
                int middle = (low + high + 1) / 2;
                if (TextLayouts.Measure(this.Font, lineText.Substring(0, middle).TrimEnd() + Ellipsis) <= maxWidth)
                {
                    low = middle;
                }
                else
                {
                    high = middle - 1;
                }
            }

            var line = this.BuildLine(layout, start, low);
            float width = TextLayouts.Measure(this.Font, Ellipsis);
            var format = layout.Format;
            line.Runs.Add(new TextRun(Ellipsis, line.Width, width, this.Font, format.FontWeight, format.FontStyle, null, false, false));
            line.Width += width;
            return line;
        }
    }
}
