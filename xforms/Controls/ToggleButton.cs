using System;
using XForms.Input;
using XForms.Controls;
using XForms.Primitives;

namespace XForms.Controls
{
    /// <summary>
    /// A button with an icon and a label that toggles between checked and
    /// unchecked when tapped. Its theme's template draws it.
    /// </summary>
    public sealed class ToggleButton : TemplatedControl, ITapGestureDelegate
    {
        public static readonly ViewProperty<ToggleButton, bool> IsCheckedProperty = new(nameof(IsChecked), v => v.IsChecked, (v, x) => v.IsChecked = x);

        protected override AccessibilityRole DefaultAccessibilityRole
        {
            get
            {
                return AccessibilityRole.ToggleButton;
            }
        }

        protected override string DefaultAccessibilityLabel
        {
            get
            {
                return this.Text;
            }
        }

        public override bool? AccessibilityIsChecked
        {
            get
            {
                return this.IsChecked;
            }
        }

        private string _text;
        private Bitmap _icon;
        private bool _isChecked;

        public event EventHandler<ToggledEventArgs> IsCheckedChanged;

        public ToggleButton()
        {
            this.Width = 72;
            this.Height = 60;

            this.GestureRecognizers.Add(new TapGestureRecognizer(this));
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
                    this.OnPropertyChanged();
                    this.NotifyAccessibilityStateChanged();
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

        #region ITapGestureDelegate implementation

        void ITapGestureDelegate.OnTapBegan()
        {
            this.IsPressed = true;
        }

        void ITapGestureDelegate.OnTapped()
        {
            this.IsChecked = !this.IsChecked;
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
