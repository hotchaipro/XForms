using System;
using XForms.Input;

namespace XForms.Controls
{
    public class Button : UserControl, ITapGestureDelegate
    {
        private const float TouchDownScale = 0.9f;

        private TextFormat _textFormat;
        private string _text;
        private bool _isEnabled = true;
        private bool _isAlert;

        public event EventHandler Clicked;

        public Button()
        {
            this._textFormat = new TextFormat()
            {
                FontSize = 14.0f,
                HorizontalAlignment = LayoutAlignment.Center,
                VerticalAlignment = LayoutAlignment.Center,
            };

            this.GestureRecognizer = new TapGestureRecognizer(this);
        }

        public bool IsEnabled
        {
            get
            {
                return this._isEnabled;
            }

            set
            {
                this._isEnabled = value;

                this.Invalidate();
            }
        }

        public bool IsAlertStyle
        {
            get
            {
                return this._isAlert;
            }

            set
            {
                this._isAlert = value;

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

        private Color? ForegroundColor
        {
            get
            {
                return Colors.White;
            }
        }

        public override Color BackgroundColor
        {
            get
            {
                if (this.IsAlertStyle)
                {
                    return Colors.Red;
                }
                else
                {
                    return this.Application.Theme.AccentColor;
                }
            }
        }

        private Color? DisabledColor
        {
            get
            {
                return this.Application.Theme.SubtleForegroundColor;
            }
        }

        protected override void DrawBackground(
            IDrawContext drawContext,
            Rectangle bounds)
        {
            Rectangle textBounds = new Rectangle(bounds.X, bounds.Y, bounds.Width, bounds.Height);

            if (this.IsEnabled)
            {
                base.DrawBackground(drawContext, bounds);

                var foregroundColor = this.ForegroundColor;
                if (foregroundColor.HasValue)
                {
                    drawContext.DrawText(this.Text, textBounds, foregroundColor.Value, this._textFormat);
                }
            }
            else
            {
                drawContext.Clear(Colors.Transparent);

                var foregroundColor = this.DisabledColor;
                if (foregroundColor.HasValue)
                {
                    drawContext.DrawText(this.Text, textBounds, foregroundColor.Value, this._textFormat);
                }
            }
        }

        #region ITapGestureDelegate implementation

        void ITapGestureDelegate.OnTapBegan()
        {
            this.ScaleTo(TouchDownScale, TimeSpan.FromMilliseconds(25), new CubicEase(EasingMode.EaseIn));
        }

        void ITapGestureDelegate.OnTapEnded()
        {
            this.ScaleTo(1.0f, TimeSpan.FromMilliseconds(100), new CubicEase(EasingMode.EaseIn));
        }

        void ITapGestureDelegate.OnTapped()
        {
            if (this.IsEnabled)
            {
                this.Clicked?.Invoke(this, EventArgs.Empty);
            }
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
