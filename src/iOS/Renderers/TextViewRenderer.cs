using System;
using CoreText;
using Foundation;
using UIKit;

namespace XForms.iOS.Renderers
{
    public class TextViewRenderer : ViewRenderer<UILabel>, ITextViewRenderer
    {
        private UILabel _nativeTextView;

        public TextViewRenderer(
            TextView textView)
            : base(textView)
        {
            this._nativeTextView = new UILabel()
            {
            };

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
                throw new NotImplementedException();
            }

            set
            {
                // TODO: throw new NotImplementedException();
            }
        }

        public bool WordWrap
        {
            get
            {
                return (this._nativeTextView.LineBreakMode == UILineBreakMode.WordWrap);
            }

            set
            {
                this._nativeTextView.LineBreakMode = (value ? UILineBreakMode.WordWrap : UILineBreakMode.Clip); // TODO
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

        private void UpdateFont()
        {
            UIFont font = UIFont.SystemFontOfSize(this.FontSize, this.FontWeight.ToUIFontWeight());
            if (this.FontStyle == FontStyle.Italic)
            {
                var descriptor = font.FontDescriptor;
                descriptor.Traits.SymbolicTrait = UIFontDescriptorSymbolicTraits.Italic;
                font = UIFont.FromDescriptor(descriptor, this.FontSize);
            }

            this._nativeTextView.Font = font;
        }

        private void UpdateAttributedText()
        {
            var attributedString = new NSAttributedString(
                this.Text,
                underlineStyle: NSUnderlineStyle.Single,
                strikethroughStyle: NSUnderlineStyle.Single);

            this._nativeTextView.AttributedText = attributedString;
        }
    }
}
