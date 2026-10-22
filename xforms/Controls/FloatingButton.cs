using System;
using XForms.Input;
using XForms.Controls;
using XForms.Primitives;

namespace XForms.Controls
{
    /// <summary>
    /// A round button with an icon, floating over the content, which can also
    /// be swiped. Its theme's template draws it.
    /// </summary>
    public class FloatingButton : TemplatedControl, ISwipeGestureDelegate
    {
        // Animates the button within a FlyoutButton
        private readonly Animator _flyoutAnimator;

        /// <summary>
        /// The animator a <see cref="FlyoutButton"/> moves and scales the button
        /// with. The public Animator stays the app's.
        /// </summary>
        internal Animator FlyoutAnimator
        {
            get
            {
                return this._flyoutAnimator;
            }
        }
        private Bitmap _icon;
        private bool _isSubtleStyle;

        public event EventHandler Clicked;
        public event EventHandler<int> Swiped;

        public FloatingButton()
        {
            this._flyoutAnimator = this.CreateOwnAnimator();
            this.GestureRecognizers.Add(new SwipeGestureRecognizer(this));

            this.Focusable = true;
        }

        /// <summary>
        /// The button shows only an icon: give it an
        /// <see cref="View.AccessibilityLabel"/>.
        /// </summary>
        protected override AccessibilityRole DefaultAccessibilityRole
        {
            get
            {
                return AccessibilityRole.Button;
            }
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

                this.InvalidateVisual();
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
            this.IsPressed = true;
        }

        void ITapGestureDelegate.OnTapEnded()
        {
            this.OnTapEnded();
            this.IsPressed = false;
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
