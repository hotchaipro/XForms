using System;
using XForms.Input;
using XForms.Controls;
using XForms.Primitives;

namespace XForms.Controls
{
    public class TabStripItem : UserControl, ITapGestureDelegate
    {
        // Animates the control itself (press feedback, spinning)
        private readonly DrawnAnimator _ownAnimator;

        protected override AccessibilityRole DefaultAccessibilityRole
        {
            get
            {
                return AccessibilityRole.TabItem;
            }
        }

        protected override string DefaultAccessibilityLabel
        {
            get
            {
                return this.Text;
            }
        }

        public override bool AccessibilityIsSelected
        {
            get
            {
                return this.IsChecked;
            }
        }

        public event EventHandler<ToggledEventArgs> IsCheckedChanged;

        private const float DefaultScale = 0.8f;

        private bool _isChecked;
        private string _text;
        private TextFormat _textFormat;
        private Bitmap _icon;
        //private Color _foregroundColor;
        private bool _isTouchDown;

        public TabStripItem()
        {
            this._ownAnimator = this.CreateDrawnAnimator();

            this._textFormat = new TextFormat()
            {
                FontSize = 12.0f,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Bottom,
            };

            this._ownAnimator.Scale = DefaultScale;

            this.GestureRecognizers.Add(new TapGestureRecognizer(this));
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
                    this.NotifyAccessibilityStateChanged();
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
                    this._ownAnimator.ScaleTo(0.95f, TimeSpan.FromMilliseconds(25), new CubicEase(EasingMode.EaseIn));
                }
                else
                {
                    this._ownAnimator.ScaleTo(DefaultScale - 0.05f, TimeSpan.FromMilliseconds(25), new CubicEase(EasingMode.EaseIn));
                }

                this._ownAnimator.Opacity = 1.0f;
            }
            else if (this.IsChecked)
            {
                this._ownAnimator.ScaleTo(1.0f, this.ActualTheme.Motion.Short, new SpringEase(EasingMode.EaseOut));

                this._ownAnimator.Opacity = 1.0f;
            }
            else
            {
                this._ownAnimator.ScaleTo(DefaultScale, this.ActualTheme.Motion.Short, new CubicEase(EasingMode.EaseIn));

                if (this.ActualTheme.Mode != ThemeMode.Light)
                {
                    this._ownAnimator.Opacity = 1.0f;
                }
                else
                {
                    this._ownAnimator.Opacity = 0.5f;
                }
            }

            this.InvalidateVisual();
        }

        protected override void Render(
            DrawingContext drawContext)
        {
            var bounds = new Rect(0, 0, this.Bounds.Width, this.Bounds.Height);
            base.Render(drawContext);

            //drawContext.DrawRectangle(bounds, Color.FromArgb(0x40, 255, 0, 0));

            float margin = Math.Min(bounds.Width, bounds.Height) * 0.1f;
            bounds = bounds.Inflate(-margin, -margin);

            float imageDimension = Math.Min(bounds.Width, bounds.Height) * 0.65f;
            float textHeight = bounds.Height * 0.35f;

            Rect imageBounds = new Rect(
                bounds.Center.X - (imageDimension / 2f),
                bounds.Center.Y - ((imageDimension + textHeight) / 2f),
                imageDimension,
                imageDimension);

            Rect textBounds = new Rect(bounds.X, imageBounds.Bottom, bounds.Width, bounds.Bottom - imageBounds.Bottom);

            //drawContext.DrawRectangle(bounds, Color.FromArgb(0x40, 255, 0, 0));
            //drawContext.DrawRectangle(imageBounds, Color.FromArgb(0x40, 255, 0, 0));
            //drawContext.DrawRectangle(textBounds, Color.FromArgb(0x40, 255, 0, 0));

            Color foregroundColor;
            if (this.ActualTheme.Mode != ThemeMode.Light)
            {
                if (this.IsChecked)
                {
                    foregroundColor = this.ActualTheme.Colors.Accent;
                }
                else
                {
                    foregroundColor = this.ActualTheme.Colors.SubtleForeground;
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
