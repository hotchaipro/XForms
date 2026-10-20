using System;
using XForms.Input;
using XForms.Controls;

namespace XForms.Toolkit
{
    public sealed class ToggleButton : UserControl, ITapGestureDelegate
    {
        public static readonly ViewProperty<ToggleButton, bool> IsCheckedProperty = new(nameof(IsChecked), v => v.IsChecked, (v, x) => v.IsChecked = x);

        // Animates the control itself (press feedback, spinning)
        private readonly Animator _ownAnimator;

        protected override AccessibilityRole DefaultAccessibilityRole
        {
            get
            {
                return AccessibilityRole.ToggleButton;
            }
        }

        protected override string DefaultAccessibilityLabel
        {
            get
            {
                return this.Text;
            }
        }

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
            this._ownAnimator = this.CreateOwnAnimator();

            this._textFormat = new TextFormat()
            {
                FontSize = 10.0f,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Bottom,
            };

            this.Width = 72;
            this.Height = 60;
            this._ownAnimator.Scale = IsUncheckedScale;

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

                    this.InvalidateVisual();
                    this.UpdateScale();

                    this.IsCheckedChanged?.Invoke(this, new ToggledEventArgs(value));
                    this.OnPropertyChanged();
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

                this.InvalidateVisual();
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

                this.InvalidateVisual();
            }
        }

        private Color ForegroundColor
        {
            get
            {
                return Colors.White;
            }
        }

        protected override void Render(
            DrawingContext drawContext)
        {
            var bounds = new Rect(0, 0, this.Bounds.Width, this.Bounds.Height);
            base.Render(drawContext);

            // Calculate text bounds

            float textHeight = bounds.Height / 4;
            Rect textBounds = new Rect(bounds.X, bounds.Bottom - textHeight, bounds.Width, textHeight);

            // Calculate button bounds

            float buttonDimension = Math.Min(bounds.Width, bounds.Height - textHeight);
            float buttonRadius = buttonDimension / 2;
            Rect buttonBounds = new Rect(
                bounds.Center.X - buttonRadius,
                bounds.Center.Y - (textHeight / 2) - buttonRadius,
                buttonDimension,
                buttonDimension);

            // Calculate image bounds

            float imageDimension = buttonDimension / 2;
            Rect imageBounds = new Rect(
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
                this._ownAnimator.ScaleTo(IsUncheckedScale - TouchDownScaleDelta, TimeSpan.FromMilliseconds(25), new CubicEase(EasingMode.EaseIn));
            }
            else if (this.IsChecked)
            {
                this._ownAnimator.ScaleTo(1.0f, TimeSpan.FromMilliseconds(100), new CubicEase(EasingMode.EaseIn));
            }
            else
            {
                this._ownAnimator.ScaleTo(IsUncheckedScale, TimeSpan.FromMilliseconds(100), new CubicEase(EasingMode.EaseOut));
            }
        }

        #region ITapGestureDelegate implementation

        void ITapGestureDelegate.OnTapBegan()
        {
            this._isTouchDown = true;
            this.InvalidateVisual();
            this.UpdateScale();
        }

        void ITapGestureDelegate.OnTapped()
        {
            this.IsChecked = !this.IsChecked;
        }

        void ITapGestureDelegate.OnTapEnded()
        {
            this._isTouchDown = false;
            this.InvalidateVisual();
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
