using System;
using System.Windows.Input;
using XForms.Input;
using XForms.Controls;
using XForms.Primitives;

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

    /// <summary>
    /// An icon that toggles between a checked and an unchecked image when
    /// pressed. Its theme's template draws it.
    /// </summary>
    public class ImageToggleButton : TemplatedControl, ITapGestureDelegate
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

        // User state: whether it's checked, when that changed since it was
        // first attached
        private bool _initialIsChecked;

        protected override void OnAttaching(
            Theme theme)
        {
            base.OnAttaching(theme);
            this._initialIsChecked = this.IsChecked;
        }

        protected override object GetUserState()
        {
            return (this.IsChecked != this._initialIsChecked) ? this.IsChecked : null;
        }

        protected override void SetUserState(
            object state)
        {
            if (state is bool isChecked)
            {
                this.IsChecked = isChecked;
            }
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

                    this.OnStateChanged();
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
