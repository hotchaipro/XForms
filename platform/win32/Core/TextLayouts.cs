using System;
using System.Collections.Generic;
using System.IO;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Direct2D;
using Windows.Win32.Graphics.DirectWrite;
using Windows.Win32.System.Com;
using XForms.Platform;

namespace XForms.Win32
{
    /// <summary>
    /// Builds DirectWrite text layouts for <see cref="TextLayout"/> and text
    /// blocks: the font family (registered or installed), wrapping, alignment,
    /// trimming, line limits and styled spans.
    /// </summary>
    internal static unsafe class TextLayouts
    {
        // Registered families: the name the app uses, the collection holding the
        // font, and the font's own family name in it
        private static readonly Dictionary<string, (nint Collection, string FamilyName)> RegisteredFonts = new(StringComparer.OrdinalIgnoreCase);

        public static void RegisterFont(
            string familyName,
            string fileName)
        {
            string path = Path.IsPathRooted(fileName) ? fileName : Path.Combine(AppContext.BaseDirectory, fileName);
            if (!File.Exists(path))
            {
                throw new FileNotFoundException("The font file wasn't found.", path);
            }

            var factory = Win32Host.Current.DWriteFactory;
            ((IUnknown*)factory)->QueryInterface(IDWriteFactory5.IID_Guid, out var factory5Pointer).ThrowOnFailure();
            var factory5 = (IDWriteFactory5*)factory5Pointer;
            try
            {
                IDWriteFontFile* file;
                factory->CreateFontFileReference(path, null, &file);

                IDWriteFontSetBuilder1* builder;
                factory5->CreateFontSetBuilder(&builder);
                builder->AddFontFile(file);
                file->Release();

                IDWriteFontSet* fontSet;
                builder->CreateFontSet(&fontSet);
                builder->Release();

                IDWriteFontCollection1* collection;
                factory5->CreateFontCollectionFromFontSet(fontSet, &collection);
                fontSet->Release();

                // The family name inside the file, which DirectWrite matches on
                IDWriteFontFamily1* family;
                collection->GetFontFamily(0, &family);
                IDWriteLocalizedStrings* names;
                family->GetFamilyNames(&names);
                names->GetStringLength(0, out uint length);
                var buffer = new char[length + 1];
                fixed (char* bufferPointer = buffer)
                {
                    names->GetString(0, bufferPointer, length + 1);
                }

                names->Release();
                family->Release();

                RegisteredFonts[familyName] = ((nint)collection, new string(buffer, 0, (int)length));
            }
            finally
            {
                factory5->Release();
            }
        }

        public static IDWriteTextFormat* CreateTextFormat(
            TextFormat format,
            FontWeight? weight = null,
            FontStyle? style = null)
        {
            string family = format.FontFamily ?? TextResources.FontFamily;
            IDWriteFontCollection* collection = null;
            if (string.Equals(family, FontFamilies.SansSerif, StringComparison.OrdinalIgnoreCase))
            {
                family = TextResources.FontFamily;
            }
            else if (string.Equals(family, FontFamilies.Serif, StringComparison.OrdinalIgnoreCase))
            {
                family = "Cambria";
            }
            else if (string.Equals(family, FontFamilies.Monospace, StringComparison.OrdinalIgnoreCase))
            {
                family = "Consolas";
            }
            else if (RegisteredFonts.TryGetValue(family, out var registered))
            {
                collection = (IDWriteFontCollection*)registered.Collection;
                family = registered.FamilyName;
            }

            var fontStyle = style ?? format.FontStyle;
            IDWriteTextFormat* textFormat;
            Win32Host.Current.DWriteFactory->CreateTextFormat(
                family,
                collection,
                (DWRITE_FONT_WEIGHT)(int)(weight ?? format.FontWeight),
                (fontStyle == FontStyle.Italic) ? DWRITE_FONT_STYLE.DWRITE_FONT_STYLE_ITALIC : DWRITE_FONT_STYLE.DWRITE_FONT_STYLE_NORMAL,
                DWRITE_FONT_STRETCH.DWRITE_FONT_STRETCH_NORMAL,
                // Font sizes follow Windows' text size setting
                Math.Max(1, format.FontSize * SystemTheme.TextScale),
                "",
                &textFormat);

            return textFormat;
        }

        /// <summary>
        /// Lays out the text. An infinite width sizes the layout to the text, so
        /// alignment is within the text's own width.
        /// </summary>
        public static IDWriteTextLayout* CreateLayout(
            string text,
            TextFormat format,
            float maxWidth,
            float maxHeight,
            IReadOnlyList<TextSpan> spans,
            out Size size,
            out int lineCount)
        {
            var factory = Win32Host.Current.DWriteFactory;
            text ??= string.Empty;

            var textFormat = CreateTextFormat(format);
            IDWriteTextLayout* layout;
            try
            {
                bool wraps = (format.TextWrapping == TextWrapping.Wrap) && !float.IsInfinity(maxWidth);
                textFormat->SetWordWrapping(wraps ? DWRITE_WORD_WRAPPING.DWRITE_WORD_WRAPPING_WRAP : DWRITE_WORD_WRAPPING.DWRITE_WORD_WRAPPING_NO_WRAP);
                textFormat->SetTextAlignment(format.HorizontalAlignment.ToDWriteTextAlignment());

                float width = float.IsInfinity(maxWidth) ? 100000 : maxWidth;
                float height = float.IsInfinity(maxHeight) ? 100000 : maxHeight;
                factory->CreateTextLayout(text, (uint)text.Length, textFormat, width, height, &layout);

                bool trims = (format.TextTrimming == TextTrimming.CharacterEllipsis) || (0 < format.MaxLines);
                if (trims)
                {
                    IDWriteInlineObject* ellipsis;
                    factory->CreateEllipsisTrimmingSign(textFormat, &ellipsis);
                    var trimming = new DWRITE_TRIMMING() { granularity = DWRITE_TRIMMING_GRANULARITY.DWRITE_TRIMMING_GRANULARITY_CHARACTER };
                    layout->SetTrimming(&trimming, ellipsis);
                    ellipsis->Release();
                }
            }
            finally
            {
                textFormat->Release();
            }

            ApplySpans(layout, format, spans);

            // Limit the lines by limiting the height; the trimming adds the ellipsis
            var lines = GetLineMetrics(layout);
            uint actualLines = (uint)lines.Length;
            lineCount = (int)actualLines;
            if ((0 < format.MaxLines) && (actualLines > format.MaxLines))
            {
                float limit = 0;
                for (int i = 0; i < format.MaxLines; i += 1)
                {
                    limit += lines[i].height;
                }

                layout->SetMaxHeight(Math.Min(limit, layout->GetMaxHeight()));
                lineCount = format.MaxLines;
            }

            DWRITE_TEXT_METRICS metrics;
            layout->GetMetrics(&metrics);
            float measuredHeight = Math.Min(metrics.height, layout->GetMaxHeight());

            if (float.IsInfinity(maxWidth))
            {
                // Size the layout to the text, so alignment works within it
                layout->SetMaxWidth((float)Math.Ceiling(metrics.widthIncludingTrailingWhitespace));
            }

            size = new Size(
                Math.Min((float)Math.Ceiling(metrics.widthIncludingTrailingWhitespace), maxWidth),
                Math.Min((float)Math.Ceiling(measuredHeight), maxHeight));
            return layout;
        }

        private static DWRITE_LINE_METRICS[] GetLineMetrics(
            IDWriteTextLayout* layout)
        {
            // NOTE: Asking with too small a buffer fails with the count needed
            var lines = new DWRITE_LINE_METRICS[16];
            while (true)
            {
                uint count = 0;
                try
                {
                    fixed (DWRITE_LINE_METRICS* linesPointer = lines)
                    {
                        layout->GetLineMetrics(linesPointer, (uint)lines.Length, &count);
                    }

                    Array.Resize(ref lines, (int)count);
                    return lines;
                }
                catch (System.Runtime.InteropServices.COMException) when (count > lines.Length)
                {
                    lines = new DWRITE_LINE_METRICS[count];
                }
            }
        }

        private static void ApplySpans(
            IDWriteTextLayout* layout,
            TextFormat format,
            IReadOnlyList<TextSpan> spans)
        {
            if (null == spans)
            {
                return;
            }

            foreach (var span in spans)
            {
                if (span.Length <= 0)
                {
                    continue;
                }

                var range = new DWRITE_TEXT_RANGE() { startPosition = (uint)span.Start, length = (uint)span.Length };
                if (span.FontWeight.HasValue)
                {
                    layout->SetFontWeight((DWRITE_FONT_WEIGHT)(int)span.FontWeight.Value, range);
                }

                if (span.FontStyle.HasValue)
                {
                    layout->SetFontStyle((span.FontStyle.Value == FontStyle.Italic) ? DWRITE_FONT_STYLE.DWRITE_FONT_STYLE_ITALIC : DWRITE_FONT_STYLE.DWRITE_FONT_STYLE_NORMAL, range);
                }

                if (span.Underline)
                {
                    layout->SetUnderline(true, range);
                }

                if (span.Strikethrough)
                {
                    layout->SetStrikethrough(true, range);
                }
            }
        }

        /// <summary>
        /// Draws a layout, with the spans' colors as drawing effects.
        /// </summary>
        public static void Draw(
            ID2D1DeviceContext* context,
            IDWriteTextLayout* layout,
            Point origin,
            ID2D1Brush* defaultBrush,
            IReadOnlyList<TextSpan> spans)
        {
            List<nint> brushes = null;
            if (null != spans)
            {
                foreach (var span in spans)
                {
                    if (!span.Foreground.HasValue || (span.Length <= 0))
                    {
                        continue;
                    }

                    ID2D1SolidColorBrush* brush;
                    context->CreateSolidColorBrush(span.Foreground.Value.ToD2DColor(), null, &brush);
                    layout->SetDrawingEffect((IUnknown*)brush, new DWRITE_TEXT_RANGE() { startPosition = (uint)span.Start, length = (uint)span.Length });
                    (brushes ??= new List<nint>()).Add((nint)brush);
                }
            }

            context->DrawTextLayout(
                new Windows.Win32.Graphics.Direct2D.Common.D2D_POINT_2F() { x = origin.X, y = origin.Y },
                layout,
                defaultBrush,
                D2D1_DRAW_TEXT_OPTIONS.D2D1_DRAW_TEXT_OPTIONS_ENABLE_COLOR_FONT);

            if (null != brushes)
            {
                foreach (var brush in brushes)
                {
                    ((ID2D1SolidColorBrush*)brush)->Release();
                }
            }
        }
    }

    /// <summary>
    /// A <see cref="TextLayout"/> on DirectWrite.
    /// </summary>
    internal sealed unsafe class TextLayoutRenderer : ITextLayoutRenderer
    {
        private IDWriteTextLayout* _layout;

        public TextLayoutRenderer(
            TextLayout textLayout)
        {
            this._layout = TextLayouts.CreateLayout(textLayout.Text, textLayout.Format, textLayout.MaxWidth, textLayout.MaxHeight, textLayout.Spans, out var size, out int lineCount);
            this.Size = size;
            this.LineCount = lineCount;
        }

        ~TextLayoutRenderer()
        {
            this.Release();
        }

        public Size Size { get; }

        public int LineCount { get; }

        public IDWriteTextLayout* Layout
        {
            get
            {
                return this._layout;
            }
        }

        public void Dispose()
        {
            this.Release();
            GC.SuppressFinalize(this);
        }

        private void Release()
        {
            if (null != this._layout)
            {
                this._layout->Release();
                this._layout = null;
            }
        }
    }
}
