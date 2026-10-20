using System;
using Android.Views;
using AndroidTextView = Android.Widget.TextView;
using XForms.Controls;

namespace XForms.Android.Renderers
{
    public class TextViewRenderer : ViewRenderer<AndroidTextView>, ITextViewRenderer
    {
        private AndroidTextView _nativeTextView;

        public TextViewRenderer(
            global::Android.Content.Context context,
            TextView textView)
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
                return this._nativeTextView.Typeface.Style.FromAndroidTypefaceStyle();
            }

            set
            {
                // NOTE: Android combines bold and italics into the style
                var typefaceStyle = this._nativeTextView.Typeface.Style;
                typefaceStyle &= (~global::Android.Graphics.TypefaceStyle.Italic) | value.ToAndroidTypefaceStyle();
                this._nativeTextView.SetTypeface(null, typefaceStyle);
            }
        }

        public FontWeight FontWeight
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                //throw new NotImplementedException();
            }
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
