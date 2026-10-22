using System;
using XForms.Input;
using XForms.Controls;
using XForms.Primitives;

namespace XForms.Controls
{
    /// <summary>
    /// One of a <see cref="TabStrip"/>'s tabs: an icon and a label, checked
    /// while its page shows. Its theme's template draws it.
    /// </summary>
    public class TabStripItem : TemplatedControl, ITapGestureDelegate
    {
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

        private bool _isChecked;
        private string _text;
        private Bitmap _icon;
        public TabStripItem()
        {
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
                this.IsPressed = false;
                if (this._isChecked != value)
                {
                    this._isChecked = value;
                    this.OnStateChanged();

                    this.IsCheckedChanged?.Invoke(this, new ToggledEventArgs(value));
                    this.NotifyAccessibilityStateChanged();
                }
                else
                {
                    this.OnStateChanged();
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

        #region ITapGestureDelegate implementation

        void ITapGestureDelegate.OnTapBegan()
        {
            this.IsPressed = true;
        }

        void ITapGestureDelegate.OnTapEnded()
        {
            this.IsPressed = false;
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
