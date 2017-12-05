using System;
using XForms.Input;

namespace XForms.Controls
{
    public class TabButton : UserControl, ITapGestureDelegate
    {
        public event EventHandler<ToggledEventArgs> IsCheckedChanged;

        private const float DefaultScale = 0.8f;

        private bool _isChecked;
        private string _text;
        private TextFormat _textFormat;
        private Bitmap _icon;
        //private Color _foregroundColor;
        private bool _isTouchDown;

        public TabButton()
        {
            this._textFormat = new TextFormat()
            {
                FontSize = 12.0f,
                HorizontalAlignment = LayoutAlignment.Center,
                VerticalAlignment = LayoutAlignment.End,
            };

            this.Scale = DefaultScale;

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
                this._isTouchDown = false;

                if (this._isChecked != value)
                {
                    this._isChecked = value;

                    this.UpdateState();

                    this.IsCheckedChanged?.Invoke(this, new ToggledEventArgs(value));
                }
                else
                {
                    this.UpdateState();
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

        public override Color BackgroundColor
        {
            get
            {
                return Colors.Transparent;
            }
        }

        public Page TabPage
        {
            get;
            internal set;
        }

        private void UpdateState()
        {
            // TODO: Move animations out of the DrawBackground method

            if (this._isTouchDown)
            {
                if (this.IsChecked)
                {
                    this.ScaleTo(0.95f, TimeSpan.FromMilliseconds(25), new CubicEase(EasingMode.EaseIn));
                }
                else
                {
                    this.ScaleTo(DefaultScale - 0.05f, TimeSpan.FromMilliseconds(25), new CubicEase(EasingMode.EaseIn));
                }

                this.Opacity = 1.0f;
            }
            else if (this.IsChecked)
            {
                this.ScaleTo(1.0f, TimeSpan.FromMilliseconds(100), new SpringEase(EasingMode.EaseOut));

                this.Opacity = 1.0f;
            }
            else
            {
                this.ScaleTo(DefaultScale, TimeSpan.FromMilliseconds(100), new CubicEase(EasingMode.EaseIn));

                if (this.Theme == AppTheme.Dark)
                {
                    this.Opacity = 1.0f;
                }
                else
                {
                    this.Opacity = 0.5f;
                }
            }

            this.Invalidate();
        }

        protected override void DrawBackground(
            IDrawContext drawContext,
            Rectangle bounds)
        {
            base.DrawBackground(drawContext, bounds);

            //drawContext.DrawRectangle(bounds, Color.FromArgb(0x40, 255, 0, 0));

            float margin = Math.Min(bounds.Width, bounds.Height) * 0.1f;
            bounds = bounds.Inflate(-margin, -margin);

            float imageDimension = Math.Min(bounds.Width, bounds.Height) * 0.65f;
            float textHeight = bounds.Height * 0.35f;

            Rectangle imageBounds = new Rectangle(
                bounds.Center.X - (imageDimension / 2f),
                bounds.Center.Y - ((imageDimension + textHeight) / 2f),
                imageDimension,
                imageDimension);

            Rectangle textBounds = new Rectangle(bounds.X, imageBounds.Bottom, bounds.Width, bounds.Bottom - imageBounds.Bottom);

            //drawContext.DrawRectangle(bounds, Color.FromArgb(0x40, 255, 0, 0));
            //drawContext.DrawRectangle(imageBounds, Color.FromArgb(0x40, 255, 0, 0));
            //drawContext.DrawRectangle(textBounds, Color.FromArgb(0x40, 255, 0, 0));

            Color foregroundColor;
            if (this.Theme == AppTheme.Dark)
            {
                if (this.IsChecked)
                {
                    foregroundColor = this.Application.Theme.AccentColor;
                }
                else
                {
                    foregroundColor = this.Application.Theme.SubtleForegroundColor;
                }
            }
            else
            {
                foregroundColor = Colors.White;
            }

            drawContext.DrawImage(this._icon, imageBounds, foregroundColor);
            drawContext.DrawText(this.Text, textBounds, foregroundColor, this._textFormat);
        }

        #region ITapGestureDelegate implementation

        void ITapGestureDelegate.OnTapBegan()
        {
            this._isTouchDown = true;

            this.UpdateState();
        }

        void ITapGestureDelegate.OnTapEnded()
        {
            this._isTouchDown = false;

            this.UpdateState();
        }

        void ITapGestureDelegate.OnTapped()
        {
            this.IsChecked = true;
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
