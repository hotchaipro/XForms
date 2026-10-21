using System;
using Android.Views;
using AndroidTextView = Android.Widget.TextView;
using XForms.Primitives;
using XForms.Platform;

namespace XForms.Android.Renderers
{
    public class TextBlockRenderer : ViewRenderer<AndroidTextView>, ITextBlockRenderer
    {
        private AndroidTextView _nativeTextView;

        public TextBlockRenderer(
            global::Android.Content.Context context,
            TextBlock textView)
            : base(context, textView)
        {
            this._nativeTextView = new AndroidTextView(
                context)
            {
                Gravity = GravityFlags.Top | GravityFlags.Left,
            };
            this._nativeTextView.SetPadding(0, 0, 0, 0);

#if DEBUG_LAYOUT
            this._nativeTextView.SetBackgroundColor(Color.FromArgb(0x80, 0xff, 0, 0).ToAndroidColor());
#endif

            this.SetNativeElement(this._nativeTextView);
        }

        public string Text
        {
            get
            {
                return this._text;
            }

            set
            {
                this._text = value ?? String.Empty;
                this.ApplyText();
            }
        }

        public TextAlignment HorizontalTextAlignment
        {
            get
            {
                return this._nativeTextView.Gravity.FromAndroidHorizontalGravityFlags();
            }

            set
            {
                var gravityFlags = value.ToAndroidHorizontalGravityFlags();
                this._nativeTextView.Gravity = ((this._nativeTextView.Gravity & ~GravityFlags.HorizontalGravityMask) | gravityFlags);

#if DEBUG_LAYOUT
                if (this.HorizontalTextAlignment != value)
                {
                    throw new Exception();
                }
#endif
            }
        }

        public TextAlignment VerticalTextAlignment
        {
            get
            {
                return this._nativeTextView.Gravity.FromAndroidVerticalGravityFlags();
            }

            set
            {
                var gravityFlags = value.ToAndroidVerticalGravityFlags();
                this._nativeTextView.Gravity = ((this._nativeTextView.Gravity & ~GravityFlags.VerticalGravityMask) | gravityFlags);

#if DEBUG_LAYOUT
                if (this.VerticalTextAlignment != value)
                {
                    throw new Exception();
                }
#endif
            }
        }

        public bool WordWrap
        {
            get
            {
                return (this._nativeTextView.MaxLines > 1);
            }

            set
            {
                this._nativeTextView.SetMaxLines(value ? 100 : 1);
            }
        }


        public bool TextTrimming
        {
            get
            {
                return (this._nativeTextView.Ellipsize != null);
            }

            set
            {
                this._nativeTextView.Ellipsize = (value ? global::Android.Text.TextUtils.TruncateAt.End : null);
            }
        }

        public Color ForegroundColor
        {
            get
            {
                var color = new global::Android.Graphics.Color(this._nativeTextView.CurrentTextColor);
                return NativeConversions.FromAndroidColor(color);
            }

            set
            {
                this._nativeTextView.SetTextColor(NativeConversions.ToAndroidColor(value));
            }
        }

        public float FontSize
        {
            get
            {
                return NativeConversions.PixelsToScaledPixels(this.NativeContext, this._nativeTextView.TextSize);
            }

            set
            {
                this._nativeTextView.SetTextSize(global::Android.Util.ComplexUnitType.Sp, value);
            }
        }

        public FontStyle FontStyle
        {
            get
            {
                return this._fontStyle;
            }

            set
            {
                this._fontStyle = value;
                this.ApplyTypeface();
            }
        }

        private string _fontFamily;
        private FontWeight _fontWeight = FontWeight.Normal;
        private FontStyle _fontStyle = FontStyle.Normal;
        private string _text = string.Empty;
        private System.Collections.Generic.IReadOnlyList<TextSpan> _spans;

        public FontWeight FontWeight
        {
            get
            {
                return this._fontWeight;
            }

            set
            {
                this._fontWeight = value;
                this.ApplyTypeface();
            }
        }

        public string FontFamily
        {
            set
            {
                this._fontFamily = value;
                this.ApplyTypeface();
            }
        }

        public int MaxLines
        {
            set
            {
                if (0 < value)
                {
                    this._nativeTextView.SetMaxLines(value);
                    this._nativeTextView.Ellipsize = global::Android.Text.TextUtils.TruncateAt.End;
                }
            }
        }

        public System.Collections.Generic.IReadOnlyList<TextSpan> Spans
        {
            set
            {
                this._spans = value;
                this.ApplyText();
            }
        }

        private void ApplyTypeface()
        {
            this._nativeTextView.Typeface = Fonts.GetTypeface(this._fontFamily, this._fontWeight, this._fontStyle);
            this.ApplyText();
        }

        private void ApplyText()
        {
            this._nativeTextView.TextFormatted = Fonts.Styled(this._text, this._spans, this._fontFamily, this._fontWeight, this._fontStyle);
        }

        public bool Strikethrough
        {
            get
            {
                return this._nativeTextView.PaintFlags.HasFlag(global::Android.Graphics.PaintFlags.StrikeThruText);
            }

            set
            {
                if (value)
                {
                    this._nativeTextView.PaintFlags |= global::Android.Graphics.PaintFlags.StrikeThruText;
                }
                else
                {
                    this._nativeTextView.PaintFlags &= (~global::Android.Graphics.PaintFlags.StrikeThruText);
                }
            }
        }
    }
}
