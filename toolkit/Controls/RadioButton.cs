using System;
using System.Windows.Input;
using XForms.Input;
using XForms.Controls;

namespace XForms.Toolkit
{
    internal sealed class RadioButton : UserControl, ITapGestureDelegate
    {
        // Animates the control itself (press feedback, spinning)
        private readonly Animator _ownAnimator;

        private const float TouchDownScaleDelta = 0.05f;
        private const float IsUncheckedScale = 0.8f;

        private TextFormat _textFormat;
        private bool _isChecked;
        private bool _isTouchDown;

        public event EventHandler<ToggledEventArgs> IsCheckedChanged;

        internal RadioButton(
            RadioButtons radioButtonGroup,
            RadioButtonItem radioButtonItem)
        {
            this._ownAnimator = this.CreateOwnAnimator();

            if (null == radioButtonGroup)
            {
                throw new ArgumentNullException(nameof(radioButtonGroup));
            }

            if (null == radioButtonItem)
            {
                throw new ArgumentNullException(nameof(radioButtonItem));
            }

            this._textFormat = new TextFormat()
            {
                FontSize = 10.0f,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Bottom,
            };

            this.Width = 72;
            this.Height = 60;
            this._ownAnimator.Scale = IsUncheckedScale;

            this.RadioButtons = radioButtonGroup;
            this.RadioButtonItem = radioButtonItem;

            this.GestureRecognizer = new TapGestureRecognizer(this);

            radioButtonItem.PropertyChanged += MenuItem_PropertyChanged; // TODO: Matching detach event handler
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
                }
            }
        }

        public ICommand Command
        {
            get;
            set;
        }

        private void MenuItem_PropertyChanged(
            object sender,
            System.ComponentModel.PropertyChangedEventArgs e)
        {
            this.InvalidateVisual();
        }

        public RadioButtons RadioButtons
        {
            get;
        }

        public RadioButtonItem RadioButtonItem
        {
            get;
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

            Bitmap icon = this.RadioButtonItem.Icon;

            if (this._isTouchDown)
            {
                drawContext.FillEllipse(buttonBounds, this.ActualTheme.Colors.SubtleForeground);
                if (null != icon)
                {
                    drawContext.DrawImage(icon, imageBounds, Colors.White);
                }
            }
            else if (this.IsChecked)
            {
                drawContext.FillEllipse(buttonBounds, this.ActualTheme.Colors.Accent);
                if (null != icon)
                {
                    drawContext.DrawImage(icon, imageBounds, Colors.White);
                }
            }
            else
            {
                drawContext.DrawEllipse(buttonBounds.Center, buttonRadius - 1.0f, buttonRadius - 1.0f, this.ActualTheme.Colors.SubtleForeground, strokeWidth: 1.0f);
                if (null != icon)
                {
                    drawContext.DrawImage(icon, imageBounds, this.ActualTheme.Colors.SubtleForeground);
                }
            }

            drawContext.DrawText(this.RadioButtonItem.Text, textBounds, this.ActualTheme.Colors.SubtleForeground, this._textFormat);
        }

        private void UpdateScale()
        {
            if (this._isTouchDown)
            {
                this._ownAnimator.ScaleTo((this.IsChecked ? 1.0f : IsUncheckedScale) - TouchDownScaleDelta, TimeSpan.FromMilliseconds(25), new CubicEase(EasingMode.EaseIn));
            }
            else if (this.IsChecked)
            {
                this._ownAnimator.ScaleTo(1.0f, this.ActualTheme.Motion.Short, new CubicEase(EasingMode.EaseIn));
            }
            else
            {
                this._ownAnimator.ScaleTo(IsUncheckedScale, this.ActualTheme.Motion.Short, new CubicEase(EasingMode.EaseOut));
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
            if (!this.IsChecked)
            {
                this.IsChecked = true;
            }

            this.Command?.Execute(this);
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
