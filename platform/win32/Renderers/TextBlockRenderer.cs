using System;
using Windows.Win32.Graphics.Direct2D;
using Windows.Win32.Graphics.Direct2D.Common;
using Windows.Win32.Graphics.DirectWrite;
using XForms.Controls;
using XForms.Platform;

namespace XForms.Win32.Renderers
{
    /// <summary>
    /// A text label measured and drawn with DirectWrite.
    /// </summary>
    internal sealed unsafe class TextBlockRenderer : ViewRenderer, ITextBlockRenderer
    {
        protected override bool SupportsLightweight
        {
            get
            {
                return true;
            }
        }

        private string _text = string.Empty;
        private TextAlignment _horizontalTextAlignment = TextAlignment.Start;
        private TextAlignment _verticalTextAlignment = TextAlignment.Start;
        private bool _wordWrap;
        private bool _textTrimming;
        private Color _foregroundColor = Colors.Black;
        private float _fontSize = 15.0f;
        private FontStyle _fontStyle = FontStyle.Normal;
        private FontWeight _fontWeight = FontWeight.Normal;
        private bool _strikethrough;
        private string _fontFamily;
        private int _maxLines;
        private System.Collections.Generic.IReadOnlyList<TextSpan> _spans;

        public TextBlockRenderer(
            TextBlock textView)
            : base(textView, isContainer: false)
        {
        }

        protected override bool HasContent
        {
            get
            {
                return true;
            }
        }

        public string Text
        {
            get { return this._text; }
            set { this._text = value ?? string.Empty; this.OnTextChanged(); }
        }

        public TextAlignment HorizontalTextAlignment
        {
            get { return this._horizontalTextAlignment; }
            set { this._horizontalTextAlignment = value; this.InvalidateRender(); }
        }

        public TextAlignment VerticalTextAlignment
        {
            get { return this._verticalTextAlignment; }
            set { this._verticalTextAlignment = value; this.InvalidateRender(); }
        }

        public bool WordWrap
        {
            get { return this._wordWrap; }
            set { this._wordWrap = value; this.OnTextChanged(); }
        }

        public bool TextTrimming
        {
            get { return this._textTrimming; }
            set { this._textTrimming = value; this.InvalidateRender(); }
        }

        public Color ForegroundColor
        {
            get { return this._foregroundColor; }
            set { this._foregroundColor = value; this.InvalidateRender(); }
        }

        public float FontSize
        {
            get { return this._fontSize; }
            set { this._fontSize = value; this.OnTextChanged(); }
        }

        public FontStyle FontStyle
        {
            get { return this._fontStyle; }
            set { this._fontStyle = value; this.OnTextChanged(); }
        }

        public FontWeight FontWeight
        {
            get { return this._fontWeight; }
            set { this._fontWeight = value; this.OnTextChanged(); }
        }

        public bool Strikethrough
        {
            get { return this._strikethrough; }
            set { this._strikethrough = value; this.InvalidateRender(); }
        }

        public string FontFamily
        {
            set { this._fontFamily = value; this.OnTextChanged(); }
        }

        public int MaxLines
        {
            set { this._maxLines = value; this.OnTextChanged(); }
        }

        public System.Collections.Generic.IReadOnlyList<TextSpan> Spans
        {
            set { this._spans = value; this.OnTextChanged(); }
        }

        private void OnTextChanged()
        {
            this.InvalidateMeasure();
            this.InvalidateRender();
        }

        private IDWriteTextLayout* CreateLayout(
            float maxWidth,
            float maxHeight,
            out Size size)
        {
            var format = new TextFormat()
            {
                FontFamily = this._fontFamily,
                FontSize = this._fontSize,
                FontWeight = this._fontWeight,
                FontStyle = this._fontStyle,
                HorizontalAlignment = (this._horizontalTextAlignment == TextAlignment.Center) ? HorizontalAlignment.Center
                    : (this._horizontalTextAlignment == TextAlignment.End) ? HorizontalAlignment.Right
                    : HorizontalAlignment.Left,
                TextWrapping = this._wordWrap ? TextWrapping.Wrap : TextWrapping.NoWrap,
                TextTrimming = this._textTrimming ? XForms.TextTrimming.CharacterEllipsis : XForms.TextTrimming.None,
                MaxLines = this._maxLines,
            };

            var spans = this._spans;
            if (this._strikethrough && (this._text.Length > 0))
            {
                var all = new System.Collections.Generic.List<TextSpan>(spans ?? Array.Empty<TextSpan>());
                all.Add(new TextSpan(0, this._text.Length, strikethrough: true));
                spans = all;
            }

            return TextLayouts.CreateLayout(this._text, format, maxWidth, maxHeight, spans, out size, out _);
        }

        public override Size MeasureNative(
            Size availableSize)
        {
            var layout = this.CreateLayout(availableSize.Width, availableSize.Height, out var size);
            layout->Release();
            return size;
        }

        protected override void DrawContent(
            DrawContext drawContext)
        {
            var frame = this.Frame;
            var layout = this.CreateLayout(frame.Width, frame.Height, out _);
            try
            {
                drawContext.DrawTextLayout(layout, this._foregroundColor, this._spans);
            }
            finally
            {
                layout->Release();
            }
        }
    }
}
