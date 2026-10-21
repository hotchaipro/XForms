using System;
using XForms.Input;
using XForms.Controls;

namespace XForms.Toolkit
{
    internal sealed class MenuButton : UserControl, ITapGestureDelegate
    {
        // Animates the control itself (press feedback, spinning)
        private readonly Animator _ownAnimator;

        private const float TouchDownScale = 0.9f;

        private TextFormat _textFormat;

        internal MenuButton(
            MenuBar menuBar,
            MenuItem menuItem)
        {
            this._ownAnimator = this.CreateOwnAnimator();

            if (null == menuBar)
            {
                throw new ArgumentNullException(nameof(menuBar));
            }

            if (null == menuItem)
            {
                throw new ArgumentNullException(nameof(menuItem));
            }

            this._textFormat = new TextFormat()
            {
                FontSize = 10.0f,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Bottom,
            };

            this.Width = 72;
            this.Height = 50;

            this.MenuBar = menuBar;
            this.MenuItem = menuItem;
            this.IsVisible = menuItem.IsVisible;

            this.GestureRecognizer = new TapGestureRecognizer(this);

            menuItem.PropertyChanged += MenuItem_PropertyChanged; // TODO: Matching detach event handler
        }

        private void MenuItem_PropertyChanged(
            object sender,
            System.ComponentModel.PropertyChangedEventArgs e)
        {
            this.IsVisible = this.MenuItem.IsVisible;
            this.InvalidateVisual();
        }

        public MenuBar MenuBar
        {
            get;
        }

        public MenuItem MenuItem
        {
            get;
        }

        private Color ForegroundColor
        {
            get
            {
                if (this.ActualTheme.Mode != ThemeMode.Light)
                {
                    return this.ActualTheme.Colors.Foreground;
                }
                else
                {
                    return Colors.White;
                }
            }
        }

        public override Color BackgroundColor
        {
            get
            {
                return Colors.Transparent;
            }
        }

        protected override void Render(
            DrawingContext drawContext)
        {
            var bounds = new Rect(0, 0, this.Bounds.Width, this.Bounds.Height);
            base.Render(drawContext);

            float margin = Math.Min(bounds.Width, bounds.Height) * 0.1f;
            bounds = bounds.Inflate(-margin, -margin);

            float imageDimension = Math.Min(bounds.Width, bounds.Height) * 0.75f;
            float textHeight = bounds.Height * 0.25f;

            Rect imageBounds = new Rect(
                bounds.Center.X - (imageDimension / 2f),
                bounds.Center.Y - ((imageDimension + textHeight) / 2f),
                imageDimension,
                imageDimension);

            Rect textBounds = new Rect(bounds.X, imageBounds.Bottom, bounds.Width, textHeight);

            drawContext.DrawImage(this.MenuItem.Icon, imageBounds, this.ForegroundColor);
            drawContext.DrawText(this.MenuItem.Text, textBounds, this.ForegroundColor, this._textFormat);
        }

        #region ITapGestureDelegate implementation

        /// <summary>
        /// The value passed to the command when it runs (and when asked whether
        /// it can).
        /// </summary>
        public object CommandParameter
        {
            get;
            set;
        }

        void ITapGestureDelegate.OnTapBegan()
        {
            this._ownAnimator.ScaleTo(TouchDownScale, TimeSpan.FromMilliseconds(25), new CubicEase(EasingMode.EaseIn));
        }

        void ITapGestureDelegate.OnTapEnded()
        {
            this._ownAnimator.ScaleTo(1.0f, this.ActualTheme.Motion.Short, new CubicEase(EasingMode.EaseIn));
        }

        void ITapGestureDelegate.OnTapped()
        {
            this.MenuItem.Command?.Execute(this.CommandParameter);
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
