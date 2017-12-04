using System;
using XForms.Input;

namespace XForms.Controls
{
    public class FloatingButton : UserControl, ISwipeGestureDelegate
    {
        private Bitmap _icon;
        private bool _isTouchDown;
        private bool _isSubtleStyle;

        public event EventHandler Clicked;
        public event EventHandler<int> Swiped;

        public FloatingButton()
        {
            this.GestureRecognizer = new SwipeGestureRecognizer(this);

            this.IsEnabled = true;
        }

        public bool IsEnabled
        {
            get;
            set;
        }

        public bool IsSubtleStyle
        {
            get
            {
                return this._isSubtleStyle;
            }

            set
            {
                this._isSubtleStyle = value;

                this.Invalidate();
            }
        }

        public virtual Color AccentColor
        {
            get
            {
                return this.Theme.AccentColor;
            }
        }

        public override Color BackgroundColor
        {
            get
            {
                return Colors.Transparent;
            }
        }

        private Color ForegroundColor
        {
            get
            {
                if (this.IsSubtleStyle)
                {
                    if (this.Theme == AppTheme.Dark)
                    {
                        return this.Theme.ForegroundColor;
                    }
                    else
                    {
                        return this.Theme.ForegroundColor;
                    }
                }
                else
                {
                    return Colors.White;
                }
            }
        }

        private Color ButtonBackgroundColor
        {
            get
            {
                if (this.IsSubtleStyle)
                {
                    if (this.Theme == AppTheme.Dark)
                    {
                        return Color.FromRgb(0x33, 0x33, 0x33);
                    }
                    else
                    {
                        return this.Theme.BackgroundColor;
                    }
                }
                else
                {
                    return this.Theme.AccentColor;
                }
            }
        }

        private Color ButtonPressedBackgroundColor
        {
            get
            {
                if (this.IsSubtleStyle)
                {
                    return this.ButtonBackgroundColor;
                }
                else
                {
                    return Colors.White;
                }
            }
        }

        private Color ButtonPressedForegroundColor
        {
            get
            {
                if (this.IsSubtleStyle)
                {
                    return this.ForegroundColor;
                }
                else
                {
                    return Colors.Gray;
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
            }
        }

        protected override void DrawBackground(
            IDrawContext drawContext,
            Rectangle bounds)
        {
            base.DrawBackground(drawContext, bounds);

            if ((bounds.Height <= 0) || (bounds.Width <= 0))
            {
                return;
            }

            Size shadowPadding = new Size(bounds.Width / 10, bounds.Height / 10);
            Size shadowThinPadding = new Size(shadowPadding.Width / 2, shadowPadding.Height / 2);
            Rectangle buttonRect = bounds.Deflate(shadowPadding.Width, shadowPadding.Height);
            Rectangle shadowRect = Rectangle.FromLTRB(bounds.X, buttonRect.Top - shadowThinPadding.Height, buttonRect.Right + shadowThinPadding.Width, bounds.Bottom);
            Rectangle imageRect = buttonRect.Deflate(buttonRect.Width / 6, buttonRect.Height / 6);

            float blurAmount = Math.Min(shadowPadding.Width, shadowPadding.Height) / 1.5f;

            Color shadowColor = Colors.Gray;

            if ((this._isTouchDown) || (!this.IsEnabled))
            {
                drawContext.FillEllipseShadow(shadowRect, shadowColor, blurAmount / 2);
                drawContext.FillEllipse(buttonRect, this.ButtonPressedBackgroundColor);
                drawContext.DrawImage(this._icon, imageRect, this.ButtonPressedForegroundColor);
            }
            else
            {
                drawContext.FillEllipseShadow(shadowRect, shadowColor, blurAmount);
                drawContext.FillEllipse(buttonRect, this.ButtonBackgroundColor);
                drawContext.DrawImage(this._icon, imageRect, this.ForegroundColor);
            }
        }

        protected virtual void OnTapBegan()
        {
        }

        protected virtual void OnTapEnded()
        {
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

        void ITapGestureDelegate.OnTapBegan()
        {
            if (!this.IsEnabled)
            {
                return;
            }

            this.OnTapBegan();

            this._isTouchDown = true;
            this.Invalidate();
        }

        void ITapGestureDelegate.OnTapEnded()
        {
            this.OnTapEnded();

            this._isTouchDown = false;
            this.Invalidate();
        }

        void ITapGestureDelegate.OnTapped()
        {
            if (this.IsEnabled)
            {
                this.Clicked?.Invoke(this, EventArgs.Empty);
            }
        }

        bool ISwipeGestureDelegate.OnSwipeBegan(
            int direction)
        {
            return true;
        }

        bool ISwipeGestureDelegate.OnSwipeMoved(
            int direction,
            float dx)
        {
            return true;
        }

        bool ISwipeGestureDelegate.OnSwipeEnded(
            int direction,
            float dx)
        {
            this.Swiped?.Invoke(this, direction);
            return true;
        }

        bool ISwipeGestureDelegate.OnSwipeCanceled(
            int direction)
        {
            return true;
        }

        //protected override void OnPropertyChanged(
        //    string propertyName)
        //{
        //    base.OnPropertyChanged(propertyName);

        //    if (propertyName == FloatingButton.IsEnabledProperty.PropertyName)
        //    {
        //        this.Invalidate();
        //    }
        //}
    }
}
