using System;
using System.Collections.Generic;

namespace XForms
{
    /// <summary>
    /// Generic font families, as in CSS: each platform uses a matching system
    /// font, so text looks native everywhere without bundling a font.
    /// </summary>
    public static class FontFamilies
    {
        /// <summary>
        /// The platform's UI font (Segoe UI, Roboto, San Francisco). The default.
        /// </summary>
        public const string SansSerif = "sans-serif";

        /// <summary>
        /// A serif font (Cambria on Windows, Noto Serif on Android, New York on iOS).
        /// </summary>
        public const string Serif = "serif";

        /// <summary>
        /// A fixed-width font (Consolas on Windows, Droid Sans Mono on Android,
        /// SF Mono on iOS).
        /// </summary>
        public const string Monospace = "monospace";
    }

    /// <summary>
    /// How text looks and flows: its font and size, alignment, wrapping and
    /// trimming. Used to create a <see cref="TextLayout"/>, or to draw text
    /// directly with <see cref="DrawingContext.DrawText(string, Rect, Color, TextFormat)"/>.
    /// </summary>
    public class TextFormat
    {
        public TextFormat()
        {
            this.FontSize = 12.0f;
            this.FontWeight = FontWeight.Normal;
            this.VerticalAlignment = VerticalAlignment.Top;
            this.HorizontalAlignment = HorizontalAlignment.Left;
        }

        /// <summary>
        /// The font family: a generic family from <see cref="FontFamilies"/>
        /// (serif, sans-serif or monospace), a family registered with
        /// <see cref="Application.RegisterFont"/>, or an installed family's name.
        /// Null (the default) is the platform's UI font.
        /// </summary>
        public string FontFamily
        {
            get;
            set;
        }

        public float FontSize
        {
            get;
            set;
        }

        public FontWeight FontWeight
        {
            get;
            set;
        }

        public FontStyle FontStyle
        {
            get;
            set;
        }

        public VerticalAlignment VerticalAlignment
        {
            get;
            set;
        }

        public HorizontalAlignment HorizontalAlignment
        {
            get;
            set;
        }

        public TextWrapping TextWrapping
        {
            get;
            set;
        }

        public TextTrimming TextTrimming
        {
            get;
            set;
        }

        /// <summary>
        /// The most lines shown (0, the default, for no limit). Text cut off by
        /// the limit ends with an ellipsis.
        /// </summary>
        public int MaxLines
        {
            get;
            set;
        }

        internal TextFormat Clone()
        {
            return (TextFormat)this.MemberwiseClone();
        }
    }

    /// <summary>
    /// A range of text with its own style, for text with mixed formatting.
    /// Unset values keep the text's own.
    /// </summary>
    public readonly struct TextSpan
    {
        public TextSpan(
            int start,
            int length,
            FontWeight? fontWeight = null,
            FontStyle? fontStyle = null,
            Color? foreground = null,
            bool underline = false,
            bool strikethrough = false)
        {
            this.Start = start;
            this.Length = length;
            this.FontWeight = fontWeight;
            this.FontStyle = fontStyle;
            this.Foreground = foreground;
            this.Underline = underline;
            this.Strikethrough = strikethrough;
        }

        public int Start { get; }

        public int Length { get; }

        public FontWeight? FontWeight { get; }

        public FontStyle? FontStyle { get; }

        public Color? Foreground { get; }

        public bool Underline { get; }

        public bool Strikethrough { get; }
    }

    public interface ITextLayoutRenderer : IDisposable
    {
        Size Size { get; }

        int LineCount { get; }
    }

    /// <summary>
    /// Text laid out with a format, within a maximum size: measured once, and
    /// drawn with <see cref="DrawingContext.DrawText(TextLayout, Point, Color)"/>.
    /// The layout is immutable; dispose it to free its native resources early.
    /// </summary>
    public sealed class TextLayout : IDisposable
    {
        private ITextLayoutRenderer _native;

        public TextLayout(
            string text,
            TextFormat format,
            float maxWidth = float.PositiveInfinity,
            float maxHeight = float.PositiveInfinity,
            IReadOnlyList<TextSpan> spans = null)
        {
            if (null == format)
            {
                throw new ArgumentNullException(nameof(format));
            }

            this.Text = text ?? string.Empty;
            this.Format = format.Clone();
            this.MaxWidth = Math.Max(0, maxWidth);
            this.MaxHeight = Math.Max(0, maxHeight);
            this.Spans = spans ?? Array.Empty<TextSpan>();
            this._native = Application.Current.Platform.CreateTextLayout(this);
        }

        /// <summary>
        /// The size of the text on one line (or wrapped and limited as the format
        /// says, within the width), for sizing owner-drawn controls.
        /// </summary>
        public static Size Measure(
            string text,
            TextFormat format,
            float maxWidth = float.PositiveInfinity)
        {
            using (var layout = new TextLayout(text, format, maxWidth))
            {
                return layout.Size;
            }
        }

        public string Text
        {
            get;
        }

        /// <summary>
        /// A copy of the format the layout was created with.
        /// </summary>
        public TextFormat Format
        {
            get;
        }

        public float MaxWidth
        {
            get;
        }

        public float MaxHeight
        {
            get;
        }

        public IReadOnlyList<TextSpan> Spans
        {
            get;
        }

        /// <summary>
        /// The size of the laid-out text.
        /// </summary>
        public Size Size
        {
            get
            {
                return this.Native.Size;
            }
        }

        /// <summary>
        /// The number of lines shown.
        /// </summary>
        public int LineCount
        {
            get
            {
                return this.Native.LineCount;
            }
        }

        /// <summary>
        /// The platform's layout, for drawing.
        /// </summary>
        public ITextLayoutRenderer Native
        {
            get
            {
                return this._native ?? throw new ObjectDisposedException(nameof(TextLayout));
            }
        }

        public void Dispose()
        {
            this._native?.Dispose();
            this._native = null;
        }
    }
}
