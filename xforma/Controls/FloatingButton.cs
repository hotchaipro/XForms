using System;
using XForma.Input;
using XForma.Controls;
using XForma.Primitives;

namespace XForma.Controls
{
    /// <summary>
    /// A round button with an icon, floating over the content, which can also
    /// be swiped. Its theme's template draws it.
    /// </summary>
    public class FloatingButton : TemplatedControl, ITapGestureDelegate, ISwipeGestureDelegate
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

        /// <summary>
        /// The button was swiped sideways and released: 1 to the right, -1 to
        /// the left.
        /// </summary>
        public event EventHandler<int> Swiped;

        /// <summary>
        /// The button's swipe: its <see cref="SwipeGestureRecognizer.Swiping"/>
        /// is for content that follows the finger, such as
        /// <see cref="TabStrip.FollowSwipe"/>.
        /// </summary>
        public SwipeGestureRecognizer SwipeRecognizer
        {
            get;
        }

        public FloatingButton()
        {
            this._flyoutAnimator = this.CreateOwnAnimator();
            this.SwipeRecognizer = new SwipeGestureRecognizer(this);
            this.GestureRecognizers.Add(this.SwipeRecognizer);
            this.GestureRecognizers.Add(new TapGestureRecognizer(this));

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

            bool showsPressed = this.ShowsPressedLook;
            this.OnTapBegan();
            if (showsPressed)
            {
                this.IsPressed = true;
            }
        }

        /// <summary>
        /// Whether a press shows the pressed look (a flyout's button closing
        /// its flyout doesn't: it goes straight back to its closed look).
        /// </summary>
        internal virtual bool ShowsPressedLook
        {
            get
            {
                return true;
            }
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
