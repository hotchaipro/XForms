using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace XForms.Controls
{
    public interface ITextBlockRenderer : IViewRenderer
    {
        string Text { get; set; }

        TextAlignment HorizontalTextAlignment { get; set; }

        TextAlignment VerticalTextAlignment { get; set; }

        bool WordWrap { get; set; }

        bool TextTrimming { get; set; }

        Color ForegroundColor { get; set; }

        Color BackgroundColor { get; set; }

        float FontSize { get; set; }

        FontStyle FontStyle { get; set; }

        FontWeight FontWeight { get; set; }

        bool Strikethrough { get; set; }

        string FontFamily { set; }

        int MaxLines { set; }

        IReadOnlyList<TextSpan> Spans { set; }
    }

    /// <summary>
    /// A piece of a <see cref="TextBlock"/>'s text with its own style. Unset
    /// values keep the text block's own.
    /// </summary>
    public sealed class Run
    {
        public Run()
        {
        }

        public Run(
            string text)
        {
            this.Text = text;
        }

        public string Text { get; init; }

        public FontWeight? FontWeight { get; init; }

        public FontStyle? FontStyle { get; init; }

        public Color? Foreground { get; init; }

        public bool Underline { get; init; }

        public bool Strikethrough { get; init; }
    }

    public class TextBlock : View
    {
        public static readonly ViewProperty<TextBlock, string> TextProperty = new(nameof(Text), v => v.Text, (v, x) => v.Text = x);
        public static readonly ViewProperty<TextBlock, Color> ForegroundProperty = new(nameof(Foreground), v => v.Foreground, (v, x) => v.Foreground = x);
        public static readonly ViewProperty<TextBlock, float> FontSizeProperty = new(nameof(FontSize), v => v.FontSize, (v, x) => v.FontSize = x);
        public static readonly ViewProperty<TextBlock, FontWeight> FontWeightProperty = new(nameof(FontWeight), v => v.FontWeight, (v, x) => v.FontWeight = x);
        public static readonly ViewProperty<TextBlock, bool> StrikethroughProperty = new(nameof(Strikethrough), v => v.Strikethrough, (v, x) => v.Strikethrough = x);
        // Values the app set; the rest keep the platform's defaults
        private string _text;
        private TextAlignment? _textAlignment;
        private TextAlignment? _verticalTextAlignment;
        private bool? _wordWrap;
        private bool? _textTrimming;
        private Color? _foreground;
        private Color? _backgroundColor;
        private float? _fontSize;
        private FontStyle? _fontStyle;
        private FontWeight? _fontWeight;
        private bool? _strikethrough;
        private string _fontFamily;
        private int _maxLines;
        private IReadOnlyList<TextSpan> _spans = Array.Empty<TextSpan>();
        private ObservableCollection<Run> _inlines;

        protected override AccessibilityRole DefaultAccessibilityRole
        {
            get
            {
                return AccessibilityRole.Text;
            }
        }

        protected override string DefaultAccessibilityLabel
        {
            get
            {
                return this.Text;
            }
        }

        public TextBlock()
        {
        }

        /// <summary>
        /// Text made of runs with their own styles, as an alternative to
        /// <see cref="Text"/>: adding runs replaces the text with theirs.
        /// </summary>
        public IList<Run> Inlines
        {
            get
            {
                if (null == this._inlines)
                {
                    this._inlines = new ObservableCollection<Run>();
                    this._inlines.CollectionChanged += (sender, e) => this.OnInlinesChanged();
                }

                return this._inlines;
            }
        }

        private void OnInlinesChanged()
        {
            var text = new System.Text.StringBuilder();
            var spans = new List<TextSpan>();
            foreach (var run in this._inlines)
            {
                string runText = run.Text ?? string.Empty;
                spans.Add(new TextSpan(text.Length, runText.Length, run.FontWeight, run.FontStyle, run.Foreground, run.Underline, run.Strikethrough));
                text.Append(runText);
            }

            this._spans = spans;
            this.Native?.Spans = spans;
            this.SetText(text.ToString());
        }

        /// <summary>
        /// The font family: a generic family from <see cref="FontFamilies"/>, a
        /// family registered with <see cref="Application.RegisterFont"/>, or an
        /// installed family's name. Null is the platform's UI font.
        /// </summary>
        public string FontFamily
        {
            get
            {
                return this._fontFamily;
            }

            set
            {
                this._fontFamily = value;
                this.Native?.FontFamily = value;
                this.InvalidateMeasure();
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// The most lines shown (0, the default, for no limit); text cut off
        /// ends with an ellipsis.
        /// </summary>
        public int MaxLines
        {
            get
            {
                return this._maxLines;
            }

            set
            {
                this._maxLines = Math.Max(0, value);
                this.Native?.MaxLines = this._maxLines;
                this.InvalidateMeasure();
                this.OnPropertyChanged();
            }
        }

        public string Text
        {
            get
            {
                return this._text;
            }

            set
            {
                // Plain text replaces any runs
                if ((null != this._inlines) && (0 < this._inlines.Count))
                {
                    this._inlines.Clear();
                }

                this.SetText(value);
            }
        }

        private void SetText(
            string value)
        {
            this._text = value;
            this.Native?.Text = value;

            // New text can need a different size, on every platform
            this.InvalidateMeasure();

            this.OnPropertyChanged(nameof(this.Text));
        }

        public TextAlignment TextAlignment
        {
            get
            {
                return this._textAlignment ?? this.Native?.HorizontalTextAlignment ?? TextAlignment.Start;
            }

            set
            {
                this._textAlignment = value;
                this.Native?.HorizontalTextAlignment = value;

                this.OnPropertyChanged();
            }
        }

        public TextAlignment VerticalTextAlignment
        {
            get
            {
                return this._verticalTextAlignment ?? this.Native?.VerticalTextAlignment ?? TextAlignment.Start;
            }

            set
            {
                this._verticalTextAlignment = value;
                this.Native?.VerticalTextAlignment = value;

                this.OnPropertyChanged();
            }
        }

        public TextWrapping TextWrapping
        {
            get
            {
                return (this._wordWrap ?? this.Native?.WordWrap ?? false) ? TextWrapping.Wrap : TextWrapping.NoWrap;
            }

            set
            {
                this._wordWrap = (value == TextWrapping.Wrap);
                this.Native?.WordWrap = this._wordWrap.Value;
                this.InvalidateMeasure();

                this.OnPropertyChanged();
            }
        }

        public TextTrimming TextTrimming
        {
            get
            {
                return (this._textTrimming ?? this.Native?.TextTrimming ?? false) ? TextTrimming.CharacterEllipsis : TextTrimming.None;
            }

            set
            {
                this._textTrimming = (value == TextTrimming.CharacterEllipsis);
                this.Native?.TextTrimming = this._textTrimming.Value;

                this.OnPropertyChanged();
            }
        }

        public Color Foreground
        {
            get
            {
                return this._foreground ?? this.Native?.ForegroundColor ?? Colors.Transparent;
            }

            set
            {
                this._foreground = value;
                this.Native?.ForegroundColor = value;

                this.OnPropertyChanged();
            }
        }

        public Color BackgroundColor
        {
            get
            {
                return this._backgroundColor ?? this.Native?.BackgroundColor ?? Colors.Transparent;
            }

            set
            {
                this._backgroundColor = value;
                this.Native?.BackgroundColor = value;

                this.OnPropertyChanged();
            }
        }

        public float FontSize
        {
            get
            {
                return this._fontSize ?? this.Native?.FontSize ?? 0;
            }

            set
            {
                this._fontSize = value;
                this.Native?.FontSize = value;
                this.InvalidateMeasure();

                this.OnPropertyChanged();
            }
        }

        public FontStyle FontStyle
        {
            get
            {
                return this._fontStyle ?? this.Native?.FontStyle ?? FontStyle.Normal;
            }

            set
            {
                this._fontStyle = value;
                this.Native?.FontStyle = value;
                this.InvalidateMeasure();

                this.OnPropertyChanged();
            }
        }

        public FontWeight FontWeight
        {
            get
            {
                return this._fontWeight ?? this.Native?.FontWeight ?? FontWeight.Normal;
            }

            set
            {
                this._fontWeight = value;
                this.Native?.FontWeight = value;
                this.InvalidateMeasure();

                this.OnPropertyChanged();
            }
        }

        public bool Strikethrough
        {
            get
            {
                return this._strikethrough ?? this.Native?.Strikethrough ?? false;
            }

            set
            {
                this._strikethrough = value;
                this.Native?.Strikethrough = value;

                this.OnPropertyChanged();
            }
        }

        internal new ITextBlockRenderer Renderer
        {
            get
            {
                return (ITextBlockRenderer)base.Renderer;
            }
        }

        private ITextBlockRenderer Native
        {
            get
            {
                return (ITextBlockRenderer)this.CreatedRenderer;
            }
        }

        internal override void OnRendererCreated()
        {
            base.OnRendererCreated();

            var native = this.Native;

            // Formatting first, so the text is laid out once
            if (this._fontSize.HasValue) native.FontSize = this._fontSize.Value;
            if (this._fontStyle.HasValue) native.FontStyle = this._fontStyle.Value;
            if (this._fontWeight.HasValue) native.FontWeight = this._fontWeight.Value;
            if (this._textAlignment.HasValue) native.HorizontalTextAlignment = this._textAlignment.Value;
            if (this._verticalTextAlignment.HasValue) native.VerticalTextAlignment = this._verticalTextAlignment.Value;
            if (this._wordWrap.HasValue) native.WordWrap = this._wordWrap.Value;
            if (this._textTrimming.HasValue) native.TextTrimming = this._textTrimming.Value;
            native.ForegroundColor = this._foreground ?? this.Theme.ForegroundColor;
            if (this._backgroundColor.HasValue) native.BackgroundColor = this._backgroundColor.Value;
            if (this._strikethrough.HasValue) native.Strikethrough = this._strikethrough.Value;
            if (null != this._fontFamily) native.FontFamily = this._fontFamily;
            if (0 < this._maxLines) native.MaxLines = this._maxLines;
            if (0 < this._spans.Count) native.Spans = this._spans;
            if (null != this._text) native.Text = this._text;
        }

        protected override void OnApplyTheme(
            AppTheme theme)
        {
            base.OnApplyTheme(theme);

            if (!this._foreground.HasValue)
            {
                this.Native?.ForegroundColor = theme.ForegroundColor;
            }
        }

        protected override IElementRenderer CreateRenderer()
        {
            return this.Application.Platform.CreateTextBlockRenderer(this);
        }
    }
}
