using System;
using Windows.Win32.Graphics.Direct2D;
using Windows.Win32.Graphics.Direct2D.Common;
using Windows.Win32.Graphics.DirectWrite;
using XForms.Controls;

namespace XForms.Win32.Renderers
{
    /// <summary>
    /// A text label measured and drawn with DirectWrite.
    /// </summary>
    internal sealed unsafe class TextBlockRenderer : ViewRenderer, ITextBlockRenderer
    {
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

        private void OnTextChanged()
        {
            this.InvalidateMeasure();
            this.InvalidateRender();
        }

        private IDWriteTextLayout* CreateLayout(
            float maxWidth,
            float maxHeight)
        {
            var format = TextResources.CreateTextFormat(this._fontSize, this._fontWeight, this._fontStyle);
            try
            {
                format->SetWordWrapping(this._wordWrap ? DWRITE_WORD_WRAPPING.DWRITE_WORD_WRAPPING_WRAP : DWRITE_WORD_WRAPPING.DWRITE_WORD_WRAPPING_NO_WRAP);
                format->SetTextAlignment(this._horizontalTextAlignment.ToDWriteTextAlignment());

                IDWriteTextLayout* layout;
                Win32Host.Current.DWriteFactory->CreateTextLayout(this._text, (uint)this._text.Length, format, maxWidth, maxHeight, &layout);

                if (this._strikethrough && (this._text.Length > 0))
                {
                    layout->SetStrikethrough(true, new DWRITE_TEXT_RANGE() { startPosition = 0, length = (uint)this._text.Length });
                }

                return layout;
            }
            finally
            {
                format->Release();
            }
        }

        public override Size MeasureNative(
            Size availableSize)
        {
            float maxWidth = float.IsInfinity(availableSize.Width) ? 100000 : availableSize.Width;
            float maxHeight = float.IsInfinity(availableSize.Height) ? 100000 : availableSize.Height;

            var layout = this.CreateLayout(maxWidth, maxHeight);
            try
            {
                layout->GetMetrics(out var metrics);

                return new Size(
                    Math.Min((float)Math.Ceiling(metrics.widthIncludingTrailingWhitespace), availableSize.Width),
                    Math.Min((float)Math.Ceiling(metrics.height), availableSize.Height));
            }
            finally
            {
                layout->Release();
            }
        }

        protected override void DrawContent(
            DrawContext drawContext)
        {
            var frame = this.Frame;
            var layout = this.CreateLayout(frame.Width, frame.Height);
            try
            {
                drawContext.DrawTextLayout(layout, this._foregroundColor);
            }
            finally
            {
                layout->Release();
            }
        }
    }
}
