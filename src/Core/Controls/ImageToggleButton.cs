using System;
using XForms.Input;

namespace XForms.Controls
{
    public class ToggledEventArgs : EventArgs
    {
        public ToggledEventArgs(
            bool value)
        {
            this.Value = value;
        }

        public bool Value
        {
            get;
        }
    }

    public class ImageToggleButton : UserControl, ITapGestureDelegate
    {
        public event EventHandler<ToggledEventArgs> IsCheckedChanged;

        private bool _isChecked;
        private Bitmap _checkedIcon;
        private Bitmap _uncheckedIcon;
        private Color _tintColor;
        private Size? _iconSize;

        public ImageToggleButton()
        {
            this.GestureRecognizer = new TapGestureRecognizer(this);
        }

        public ICommand ToggleCommand
        {
            get;
            set;
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

                    this.IsCheckedChanged?.Invoke(this, new ToggledEventArgs(value));
                }
                else
                {
                    this.Invalidate();
                }
            }
        }

        public Bitmap CheckedIcon
        {
            get
            {
                return this._checkedIcon;
            }

            set
            {
                this._checkedIcon = value;

                this.Invalidate();
            }
        }

        public Bitmap UncheckedIcon
        {
            get
            {
                return this._uncheckedIcon;
            }

            set
            {
                this._uncheckedIcon = value;

                this.Invalidate();
            }
        }

        public Size? IconSize
        {
            get
            {
                return this._iconSize;
            }

            set
            {
                this._iconSize = value;

                this.Invalidate();
            }
        }

        public Color Tint
        {
            get
            {
                return this._tintColor;
            }

            set
            {
                this._tintColor = value;

                this.Invalidate();
            }
        }

        protected override void DrawBackground(
            IDrawContext drawContext,
            Rectangle bounds)
        {
            base.DrawBackground(drawContext, bounds);

            Rectangle iconBounds = bounds;
            if (this._iconSize.HasValue)
            {
                Size iconSize = this._iconSize.Value;
                iconBounds = new Rectangle(
                    bounds.Left + ((bounds.Width - iconSize.Width) / 2),
                    bounds.Top + ((bounds.Height - iconSize.Height) / 2),
                    iconSize.Width,
                    iconSize.Height);
            }

            if (this._isChecked)
            {
                drawContext.DrawImage(this._checkedIcon, iconBounds, this._tintColor);
            }
            else
            {
                drawContext.DrawImage(this._uncheckedIcon, iconBounds, this._tintColor);
            }
        }

        #region ITapGestureDelegate implementation

        void ITapGestureDelegate.OnTapBegan()
        {
            var command = this.ToggleCommand;
            if ((null != command) && (command.CanExecute(this.BindingContext)))
            {
                command.Execute(this.BindingContext);
            }

            this.IsChecked = !this._isChecked;
        }

        void ITapGestureDelegate.OnTapEnded()
        {
        }

        void ITapGestureDelegate.OnTapped()
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

        #endregion
    }
}
