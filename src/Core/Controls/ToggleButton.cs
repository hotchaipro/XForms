using System;
using XForms.Input;

namespace XForms.Controls
{
    public sealed class ToggleButton : UserControl, ITapGestureDelegate
    {
        private const float TouchDownScaleDelta = 0.05f;
        private const float IsUncheckedScale = 0.8f;

        private string _text;
        private Bitmap _icon;
        private TextFormat _textFormat;
        private bool _isChecked;
        private bool _isTouchDown;

        public event EventHandler<ToggledEventArgs> IsCheckedChanged;

        public ToggleButton()
        {
            this._textFormat = new TextFormat()
            {
                FontSize = 10.0f,
                HorizontalAlignment = LayoutAlignment.Center,
                VerticalAlignment = LayoutAlignment.End,
            };

            this.Size = new Size(72, 60);
            this.Scale = IsUncheckedScale;

            this.GestureRecognizer = new TapGestureRecognizer(this);
        }

        public bool IsChecked
        {
            get
            {
                return this._isChecked;
            }

            set
            {
                if (this._isChecked != value)
                {
                    this._isChecked = value;

                    this.Invalidate();
                    this.UpdateScale();

                    this.IsCheckedChanged?.Invoke(this, new ToggledEventArgs(value));
                }
            }
        }

        public Bitmap Icon
        {
            get
            {
                return this._icon;
            }

            set
            {
                this._icon = value;

                this.Invalidate();
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
                this._text = value;

                this.Invalidate();
            }
        }

        private Color ForegroundColor
        {
            get
            {
                return Colors.White;
            }
        }

        protected override void DrawBackground(
            IDrawContext drawContext,
            Rectangle bounds)
        {
            base.DrawBackground(drawContext, bounds);

            // Calculate text bounds

            float textHeight = bounds.Height / 4;
            Rectangle textBounds = new Rectangle(bounds.X, bounds.Bottom - textHeight, bounds.Width, textHeight);

            // Calculate button bounds

            float buttonDimension = Math.Min(bounds.Width, bounds.Height - textHeight);
            float buttonRadius = buttonDimension / 2;
            Rectangle buttonBounds = new Rectangle(
                bounds.Center.X - buttonRadius,
                bounds.Center.Y - (textHeight / 2) - buttonRadius,
                buttonDimension,
                buttonDimension);

            // Calculate image bounds

            float imageDimension = buttonDimension / 2;
            Rectangle imageBounds = new Rectangle(
                buttonBounds.Center.X - (imageDimension / 2),
                buttonBounds.Center.Y - (imageDimension / 2),
                imageDimension,
                imageDimension);

            if (this._isTouchDown)
            {
                drawContext.FillEllipse(buttonBounds, this.Theme.SubtleForegroundColor);
                drawContext.DrawImage(this.Icon, imageBounds, this.ForegroundColor);
            }
            else if (this.IsChecked)
            {
                drawContext.FillEllipse(buttonBounds, this.Theme.AccentColor);
                drawContext.DrawImage(this.Icon, imageBounds, this.ForegroundColor);
            }
            else
            {
                drawContext.DrawEllipse(buttonBounds.Center, buttonRadius - 1.0f, buttonRadius - 1.0f, this.Theme.SubtleForegroundColor, strokeWidth: 1.0f);
                drawContext.DrawImage(this.Icon, imageBounds, this.Theme.SubtleForegroundColor);
            }

            drawContext.DrawText(this.Text, textBounds, this.Theme.SubtleForegroundColor, this._textFormat);
        }

        private void UpdateScale()
        {
            if (this._isTouchDown)
            {
                this.ScaleTo(IsUncheckedScale - TouchDownScaleDelta, TimeSpan.FromMilliseconds(25), new CubicEase(EasingMode.EaseIn));
            }
            else if (this.IsChecked)
            {
                this.ScaleTo(1.0f, TimeSpan.FromMilliseconds(100), new CubicEase(EasingMode.EaseIn));
            }
            else
            {
                this.ScaleTo(IsUncheckedScale, TimeSpan.FromMilliseconds(100), new CubicEase(EasingMode.EaseOut));
            }
        }

        #region ITapGestureDelegate implementation

        void ITapGestureDelegate.OnTapBegan()
        {
            this._isTouchDown = true;
            this.Invalidate();
            this.UpdateScale();
        }

        void ITapGestureDelegate.OnTapped()
        {
            this.IsChecked = !this.IsChecked;
        }

        void ITapGestureDelegate.OnTapEnded()
        {
            this._isTouchDown = false;
            this.Invalidate();
            this.UpdateScale();
        }

        void IGestureRecognizerDelegate.OnTouchBegan()
        {
        }

        void IGestureRecognizerDelegate.OnTouchEnded()
        {
        }

        void IGestureRecognizerDelegate.OnTouchCanceled()
        {
        }

        #endregion
    }
}
