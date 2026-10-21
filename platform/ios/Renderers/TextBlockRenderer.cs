using System;
using CoreText;
using Foundation;
using UIKit;

using XForms.Controls;
using XForms.Platform;

namespace XForms.iOS.Renderers
{
    public class TextBlockRenderer : ViewRenderer<UILabel>, ITextBlockRenderer
    {
        private UILabel _nativeTextView;

        public TextBlockRenderer(
            TextBlock textView)
            : base(textView)
        {
            this._nativeTextView = new UILabel()
            {
                Lines = 1,
            };

            this._fontSize = (float)UIFont.LabelFontSize;
            this.UpdateFont();

            this.SetNativeElement(this._nativeTextView);
        }

        public string Text
        {
            get
            {
                return this._nativeTextView.Text;
            }

            set
            {
                this._nativeTextView.Text = value ?? String.Empty;

                if (this._strikeThrough || (0 < (this._spans?.Count ?? 0)))
                {
                    this.UpdateAttributedText();
                }

                this._nativeTextView.RequestLayout();
            }
        }

        public TextAlignment HorizontalTextAlignment
        {
            get
            {
                return this._nativeTextView.TextAlignment.ToTextAlignment();
            }

            set
            {
                this._nativeTextView.TextAlignment = value.FromTextAlignment();
            }
        }

        public TextAlignment VerticalTextAlignment
        {
            get
            {
                // NOTE: Labels center text vertically in their bounds, and are
                // sized to their text
                return TextAlignment.Start;
            }

            set
            {
            }
        }

        public bool WordWrap
        {
            get
            {
                return (this._nativeTextView.Lines == 0);
            }

            set
            {
                this._nativeTextView.Lines = value ? 0 : 1;
                this._nativeTextView.LineBreakMode = (value ? UILineBreakMode.WordWrap : UILineBreakMode.Clip);
                this._nativeTextView.RequestLayout();
            }
        }


        public bool TextTrimming
        {
            get
            {
                return (this._nativeTextView.LineBreakMode == UILineBreakMode.TailTruncation);
            }

            set
            {
                this._nativeTextView.LineBreakMode = (value ? UILineBreakMode.TailTruncation : UILineBreakMode.Clip); // TODO
            }
        }

        public Color ForegroundColor
        {
            get
            {
                return this._nativeTextView.TextColor.ToColor();
            }

            set
            {
                this._nativeTextView.TextColor = value.ToUIColor();
            }
        }

        private float _fontSize;
        public float FontSize
        {
            get
            {
                return this._fontSize;
            }

            set
            {
                this._fontSize = value;
                this.UpdateFont();
            }
        }

        private FontStyle _fontStyle = FontStyle.Normal;
        public FontStyle FontStyle
        {
            get
            {
                return this._fontStyle;
            }

            set
            {
                this._fontStyle = value;
                this.UpdateFont();
            }
        }

        private FontWeight _fontWeight = FontWeight.Normal;
        public FontWeight FontWeight
        {
            get
            {
                return this._fontWeight;
            }

            set
            {
                this._fontWeight = value;
                this.UpdateFont();
            }
        }

        private bool _strikeThrough;
        public bool Strikethrough
        {
            get
            {
                return this._strikeThrough;
            }

            set
            {
                this._strikeThrough = value;
                this.UpdateAttributedText();
            }
        }

        private string _fontFamily;
        private System.Collections.Generic.IReadOnlyList<TextSpan> _spans;

        public string FontFamily
        {
            set
            {
                this._fontFamily = value;
                this.UpdateFont();
            }
        }

        public int MaxLines
        {
            set
            {
                if (0 < value)
                {
                    this._nativeTextView.Lines = value;
                    this._nativeTextView.LineBreakMode = UILineBreakMode.TailTruncation;
                    this._nativeTextView.RequestLayout();
                }
            }
        }

        public System.Collections.Generic.IReadOnlyList<TextSpan> Spans
        {
            set
            {
                this._spans = value;
                this.UpdateAttributedText();
            }
        }

        private void UpdateFont()
        {
            UIFont font = Fonts.GetFont(this._fontFamily, this.FontSize, this.FontWeight, this.FontStyle);

            this._nativeTextView.Font = font;
            if (0 < (this._spans?.Count ?? 0))
            {
                this.UpdateAttributedText();
            }
            this._nativeTextView.RequestLayout();
        }

        private void UpdateAttributedText()
        {
            string text = this._nativeTextView.Text ?? String.Empty;
            var styled = Fonts.Styled(text, this._spans, this._fontFamily, this.FontSize, this.FontWeight, this.FontStyle, this._nativeTextView.TextColor);
            if (this._strikeThrough && (0 < text.Length))
            {
                styled.AddAttributes(new UIStringAttributes() { StrikethroughStyle = NSUnderlineStyle.Single }, new NSRange(0, text.Length));
            }

            this._nativeTextView.AttributedText = styled;
        }
    }
}
