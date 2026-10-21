using System;
using System.Collections.Generic;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;
using XForms.Controls;
using XForms.Platform;

namespace XForms.Web.Renderers
{
    /// <summary>
    /// Text in an element: the lines are broken here (as for drawn text), so
    /// the element shows them as laid out, with explicit line breaks.
    /// </summary>
    [SupportedOSPlatform("browser")]
    internal sealed class TextBlockRenderer : ViewRenderer, ITextBlockRenderer
    {
        private string _text = string.Empty;
        private TextAlignment _horizontalAlignment = TextAlignment.Start;
        private TextAlignment _verticalAlignment = TextAlignment.Start;
        private bool _wordWrap;
        private bool _textTrimming;
        private Color _foregroundColor = Colors.Black;
        private float _fontSize = 14;
        private FontStyle _fontStyle = FontStyle.Normal;
        private FontWeight _fontWeight = FontWeight.Normal;
        private bool _strikethrough;
        private string _fontFamily;
        private int _maxLines;
        private IReadOnlyList<TextSpan> _spans = Array.Empty<TextSpan>();
        private WebTextLayout _layout;
        private float _layoutWidth = -1;

        public TextBlockRenderer(
            TextBlock textBlock)
            : base(textBlock, "div", isContainer: false)
        {
            Js.SetStyle(this.Id, "white-space", "pre");
            Js.SetStyle(this.Id, "overflow", "hidden");
            this.ApplyFont();
            this.ApplyColor();
        }

        public string Text
        {
            get => this._text;
            set { this._text = value ?? string.Empty; this.Relayout(); }
        }

        public TextAlignment HorizontalTextAlignment
        {
            get => this._horizontalAlignment;
            set { this._horizontalAlignment = value; this.Relayout(); }
        }

        public TextAlignment VerticalTextAlignment
        {
            get => this._verticalAlignment;
            set { this._verticalAlignment = value; this.ApplyLines(); }
        }

        public bool WordWrap
        {
            get => this._wordWrap;
            set { this._wordWrap = value; this.Relayout(); }
        }

        public bool TextTrimming
        {
            get => this._textTrimming;
            set { this._textTrimming = value; this.Relayout(); }
        }

        public Color ForegroundColor
        {
            get => this._foregroundColor;
            set { this._foregroundColor = value; this.ApplyColor(); this.ApplyLines(); }
        }

        public float FontSize
        {
            get => this._fontSize;
            set { this._fontSize = value; this.ApplyFont(); this.Relayout(); }
        }

        public FontStyle FontStyle
        {
            get => this._fontStyle;
            set { this._fontStyle = value; this.ApplyFont(); this.Relayout(); }
        }

        public FontWeight FontWeight
        {
            get => this._fontWeight;
            set { this._fontWeight = value; this.ApplyFont(); this.Relayout(); }
        }

        public bool Strikethrough
        {
            get => this._strikethrough;
            set { this._strikethrough = value; Js.SetStyle(this.Id, "text-decoration", value ? "line-through" : null); }
        }

        public string FontFamily
        {
            set { this._fontFamily = value; this.ApplyFont(); this.Relayout(); }
        }

        public int MaxLines
        {
            set { this._maxLines = value; this.Relayout(); }
        }

        public IReadOnlyList<TextSpan> Spans
        {
            set { this._spans = value ?? Array.Empty<TextSpan>(); this.Relayout(); }
        }

        private void ApplyFont()
        {
            Js.SetStyle(this.Id, "font", TextLayouts.Font(this._fontFamily, this._fontSize, this._fontWeight, this._fontStyle));
        }

        private void ApplyColor()
        {
            Js.SetStyle(this.Id, "color", Css(this._foregroundColor));
        }

        private TextFormat Format(
            float maxWidth)
        {
            return new TextFormat()
            {
                FontFamily = this._fontFamily,
                FontSize = this._fontSize,
                FontWeight = this._fontWeight,
                FontStyle = this._fontStyle,
                TextWrapping = this._wordWrap ? TextWrapping.Wrap : TextWrapping.NoWrap,
                TextTrimming = this._textTrimming ? XForms.TextTrimming.CharacterEllipsis : XForms.TextTrimming.None,
                MaxLines = this._maxLines,
                HorizontalAlignment = (this._horizontalAlignment == TextAlignment.Center) ? HorizontalAlignment.Center
                    : (this._horizontalAlignment == TextAlignment.End) ? HorizontalAlignment.Right
                    : HorizontalAlignment.Left,
            };
        }

        private WebTextLayout CreateLayout(
            float maxWidth,
            float maxHeight)
        {
            using (var layout = new TextLayout(this._text, this.Format(maxWidth), maxWidth, maxHeight, this._spans))
            {
                return (WebTextLayout)layout.Native;
            }
        }

        private void Relayout()
        {
            this._layoutWidth = -1;
            this.InvalidateMeasure();
        }

        public override Size MeasureNative(
            Size availableSize)
        {
            var layout = this.CreateLayout(availableSize.Width, availableSize.Height);
            return new Size(layout.Size.Width + 1, layout.Size.Height);
        }

        public override void ArrangeNative(
            Rect frame)
        {
            base.ArrangeNative(frame);

            if (frame.Width != this._layoutWidth)
            {
                this._layoutWidth = frame.Width;
                this._layout = this.CreateLayout(frame.Width, float.PositiveInfinity);
                this.ApplyLines();
            }
            else
            {
                this.ApplyVerticalAlignment();
            }
        }

        // The lines as text (or styled runs), with a break after each
        private void ApplyLines()
        {
            var layout = this._layout;
            if (null == layout)
            {
                return;
            }

            Js.SetStyle(this.Id, "line-height", Px(layout.LineHeight));
            Js.SetStyle(this.Id, "text-align", (this._horizontalAlignment == TextAlignment.Center) ? "center" : (this._horizontalAlignment == TextAlignment.End) ? "right" : null);

            if (0 == this._spans.Count)
            {
                var text = new StringBuilder();
                for (int i = 0; i < layout.Lines.Count; i += 1)
                {
                    if (i > 0)
                    {
                        text.Append('\n');
                    }

                    foreach (var run in layout.Lines[i].Runs)
                    {
                        text.Append(run.Text);
                    }
                }

                Js.SetText(this.Id, text.ToString());
            }
            else
            {
                var texts = new List<string>();
                var styles = new List<string>();
                for (int i = 0; i < layout.Lines.Count; i += 1)
                {
                    var runs = layout.Lines[i].Runs;
                    for (int r = 0; r < runs.Count; r += 1)
                    {
                        var run = runs[r];
                        bool isLineEnd = (r == runs.Count - 1) && (i < layout.Lines.Count - 1);
                        texts.Add(isLineEnd ? run.Text + "\n" : run.Text);

                        string decoration = (run.Underline ? "underline " : string.Empty) + (run.Strikethrough ? "line-through" : string.Empty);
                        styles.Add($"font-weight:{(int)run.Weight};font-style:{((run.Style == XForms.FontStyle.Italic) ? "italic" : "normal")};"
                            + (run.Foreground.HasValue ? $"color:{Css(run.Foreground.Value)};" : string.Empty)
                            + ((decoration.Length > 0) ? $"text-decoration:{decoration.Trim()};" : string.Empty));
                    }
                }

                Js.SetRuns(this.Id, texts.ToArray(), styles.ToArray());
            }

            this.ApplyVerticalAlignment();
        }

        private void ApplyVerticalAlignment()
        {
            var layout = this._layout;
            if (null == layout)
            {
                return;
            }

            float space = Math.Max(0, this.Frame.Height - layout.Size.Height);
            float top = (this._verticalAlignment == TextAlignment.Center) ? space / 2
                : (this._verticalAlignment == TextAlignment.End) ? space
                : 0;
            Js.SetStyle(this.Id, "padding-top", (top > 0) ? Px(top) : null);
        }

        public override void OnThemeChanged(
            Theme theme)
        {
            base.OnThemeChanged(theme);
            this.ApplyLines();
        }
    }

    /// <summary>
    /// An image: the element's background (or, tinted, a mask over the tint).
    /// </summary>
    [SupportedOSPlatform("browser")]
    internal sealed class ImageRenderer : ViewRenderer, IImageRenderer
    {
        private WebImages.Entry _image;
        private Color _tint = Colors.Transparent;

        public ImageRenderer(
            Image image)
            : base(image, "div", isContainer: false)
        {
        }

        public void SetTint(
            Color color)
        {
            this._tint = color;
            this.Apply();
        }

        public void SetSource(
            ImageSource source)
        {
            string url = WebImages.UrlFor(source);
            this._image = (null == url) ? null : WebImages.Load(url);
            this.Apply();
            this._image?.WhenLoaded(this.InvalidateMeasure);
        }

        private void Apply()
        {
            string url = (null == this._image) ? null : $"url(\"{this._image.Url}\")";
            if ((null != url) && !this._tint.IsTransparent)
            {
                Js.SetStyle(this.Id, "background-image", null);
                Js.SetStyle(this.Id, "background-color", Css(this._tint));
                Js.SetStyle(this.Id, "mask-image", url);
                Js.SetStyle(this.Id, "mask-size", "100% 100%");
                Js.SetStyle(this.Id, "-webkit-mask-image", url);
                Js.SetStyle(this.Id, "-webkit-mask-size", "100% 100%");
            }
            else
            {
                Js.SetStyle(this.Id, "mask-image", null);
                Js.SetStyle(this.Id, "-webkit-mask-image", null);
                Js.SetStyle(this.Id, "background-color", null);
                Js.SetStyle(this.Id, "background-image", url);
                Js.SetStyle(this.Id, "background-size", "100% 100%");
            }
        }

        public override Size MeasureNative(
            Size availableSize)
        {
            return ((null != this._image) && this._image.IsLoaded) ? this._image.Size : Size.Empty;
        }
    }

    /// <summary>
    /// An image to draw with.
    /// </summary>
    [SupportedOSPlatform("browser")]
    internal sealed class BitmapRenderer : ElementRenderer, IBitmapRenderer
    {
        public BitmapRenderer(
            Bitmap bitmap)
            : base(bitmap)
        {
        }

        public WebImages.Entry Image
        {
            get;
            private set;
        }

        public override object NativeElement
        {
            get
            {
                return this.Image?.Id;
            }
        }

        public Size Size
        {
            get
            {
                return this.Image?.Size ?? Size.Empty;
            }
        }

        public async Task LoadAsync()
        {
            var image = this.EnsureImage();
            if (null != image)
            {
                await image.Loaded;
            }
        }

        /// <summary>
        /// The image, loading (or loaded); null when the source has no URL.
        /// </summary>
        public WebImages.Entry EnsureImage()
        {
            if (null == this.Image)
            {
                string url = WebImages.UrlFor(((Bitmap)this.Element).Source);
                if (null != url)
                {
                    this.Image = WebImages.Load(url);
                }
            }

            return this.Image;
        }
    }
}
