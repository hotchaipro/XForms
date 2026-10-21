using System;
using System.Windows.Input;
using XForms.Input;
using XForms.Controls;
using XForms.Primitives;

namespace XForms.Toolkit
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
            this.GestureRecognizers.Add(new TapGestureRecognizer(this));
        }

        /// <summary>
        /// The value passed to the command when it runs (and when asked whether
        /// it can).
        /// </summary>
        public object CommandParameter
        {
            get;
            set;
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

                    this.InvalidateVisual();

                    this.IsCheckedChanged?.Invoke(this, new ToggledEventArgs(value));
                }
                else
                {
                    this.InvalidateVisual();
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

                this.InvalidateVisual();
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

                this.InvalidateVisual();
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

                this.InvalidateVisual();
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

                this.InvalidateVisual();
            }
        }

        protected override void Render(
            DrawingContext drawContext)
        {
            var bounds = new Rect(0, 0, this.Bounds.Width, this.Bounds.Height);
            base.Render(drawContext);

            Rect iconBounds = bounds;
            if (this._iconSize.HasValue)
            {
                Size iconSize = this._iconSize.Value;
                iconBounds = new Rect(
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
            if ((null != command) && (command.CanExecute(this.CommandParameter)))
            {
                command.Execute(this.CommandParameter);
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
