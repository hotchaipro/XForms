using System;
using XForms.Input;

namespace XForms.Controls
{
    internal sealed class MenuButton : UserControl, ITapGestureDelegate
    {
        private const float TouchDownScale = 0.9f;

        private TextFormat _textFormat;

        internal MenuButton(
            MenuBar menuBar,
            MenuItem menuItem)
        {
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
                HorizontalAlignment = LayoutAlignment.Center,
                VerticalAlignment = LayoutAlignment.End,
            };

            this.Size = new Size(72, 50);

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
            this.Invalidate();
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
                if (this.Theme == AppTheme.Dark)
                {
                    return this.Theme.ForegroundColor;
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

        protected override void DrawBackground(
            IDrawContext drawContext,
            Rectangle bounds)
        {
            base.DrawBackground(drawContext, bounds);

            float margin = Math.Min(bounds.Width, bounds.Height) * 0.1f;
            bounds = bounds.Inflate(-margin, -margin);

            float imageDimension = Math.Min(bounds.Width, bounds.Height) * 0.75f;
            float textHeight = bounds.Height * 0.25f;

            Rectangle imageBounds = new Rectangle(
                bounds.Center.X - (imageDimension / 2f),
                bounds.Center.Y - ((imageDimension + textHeight) / 2f),
                imageDimension,
                imageDimension);

            Rectangle textBounds = new Rectangle(bounds.X, imageBounds.Bottom, bounds.Width, textHeight);

            drawContext.DrawImage(this.MenuItem.Icon, imageBounds, this.ForegroundColor);
            drawContext.DrawText(this.MenuItem.Text, textBounds, this.ForegroundColor, this._textFormat);
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
            this.MenuItem.Command?.Execute(this.BindingContext);
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
