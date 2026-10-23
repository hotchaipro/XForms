using System;
using System.Windows.Input;
using XForms.Input;
using XForms.Controls;
using XForms.Primitives;

namespace XForms.Controls
{
    /// <summary>
    /// One of a <see cref="Controls.RadioButtons"/> group's buttons, for one of
    /// its items. Its theme's template draws it.
    /// </summary>
    public sealed class RadioButton : TemplatedControl, ITapGestureDelegate
    {
        protected override AccessibilityRole DefaultAccessibilityRole
        {
            get
            {
                return AccessibilityRole.RadioButton;
            }
        }

        protected override string DefaultAccessibilityLabel
        {
            get
            {
                return this.RadioButtonItem.Text;
            }
        }

        public override bool AccessibilityIsSelected
        {
            get
            {
                return this.IsChecked;
            }
        }

        private bool _isChecked;

        public event EventHandler<ToggledEventArgs> IsCheckedChanged;

        internal RadioButton(
            RadioButtons radioButtonGroup,
            RadioButtonItem radioButtonItem)
        {
            if (null == radioButtonGroup)
            {
                throw new ArgumentNullException(nameof(radioButtonGroup));
            }

            if (null == radioButtonItem)
            {
                throw new ArgumentNullException(nameof(radioButtonItem));
            }

            this.Width = 72;
            this.Height = 60;

            this.RadioButtons = radioButtonGroup;
            this.RadioButtonItem = radioButtonItem;

            this.GestureRecognizers.Add(new TapGestureRecognizer(this));

            radioButtonItem.PropertyChanged += MenuItem_PropertyChanged; // TODO: Matching detach event handler
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
                    this.NotifyAccessibilityStateChanged();
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

        #region ITapGestureDelegate implementation

        void ITapGestureDelegate.OnTapBegan()
        {
            this.IsPressed = true;
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
            this.IsPressed = false;
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
