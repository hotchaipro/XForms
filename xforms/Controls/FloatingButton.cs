using System;
using System.Diagnostics;
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

        /// <summary>
        /// The button was swiped sideways and released: 1 to the right, -1 to
        /// the left.
        /// </summary>
        public event EventHandler<int> Swiped;

        /// <summary>
        /// A sideways swipe began, moved, ended or was canceled: for content
        /// that follows the finger, such as <see cref="TabStrip.FollowSwipe"/>.
        /// </summary>
        public event EventHandler<SwipingEventArgs> Swiping;

        // The swipe's recent moves, for its speed as it's released
        private const int VelocitySamples = 4;
        private static readonly long VelocityWindow = Stopwatch.Frequency / 10;
        private readonly long[] _sampleTimes = new long[VelocitySamples];
        private readonly float[] _sampleDistances = new float[VelocitySamples];
        private int _sampleCount;

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
            this._sampleCount = 0;
            this.AddSample(0);
            this.Swiping?.Invoke(this, new SwipingEventArgs(SwipePhase.Began, direction, 0, 0));
            return true;
        }

        bool ISwipeGestureDelegate.OnSwipeMoved(
            int direction,
            float dx)
        {
            this.AddSample(dx);
            this.Swiping?.Invoke(this, new SwipingEventArgs(SwipePhase.Moved, direction, dx, 0));
            return true;
        }

        bool ISwipeGestureDelegate.OnSwipeEnded(
            int direction,
            float dx)
        {
            this.AddSample(dx);
            this.Swiping?.Invoke(this, new SwipingEventArgs(SwipePhase.Ended, direction, dx, this.Velocity()));
            this.Swiped?.Invoke(this, direction);
            return true;
        }

        bool ISwipeGestureDelegate.OnSwipeCanceled(
            int direction)
        {
            this.Swiping?.Invoke(this, new SwipingEventArgs(SwipePhase.Canceled, direction, 0, 0));
            return true;
        }

        private void AddSample(
            float distance)
        {
            if (this._sampleCount == VelocitySamples)
            {
                Array.Copy(this._sampleTimes, 1, this._sampleTimes, 0, VelocitySamples - 1);
                Array.Copy(this._sampleDistances, 1, this._sampleDistances, 0, VelocitySamples - 1);
                this._sampleCount -= 1;
            }

            this._sampleTimes[this._sampleCount] = Stopwatch.GetTimestamp();
            this._sampleDistances[this._sampleCount] = distance;
            this._sampleCount += 1;
        }

        // Points a second, over the recent moves (a finger that stopped before
        // it lifted has none)
        private float Velocity()
        {
            int last = this._sampleCount - 1;
            long now = this._sampleTimes[last];
            int first = last;
            while ((first > 0) && ((now - this._sampleTimes[first - 1]) <= VelocityWindow))
            {
                first -= 1;
            }

            if (first == last)
            {
                return 0;
            }

            double seconds = (double)(now - this._sampleTimes[first]) / Stopwatch.Frequency;
            return (seconds > 0) ? (float)((this._sampleDistances[last] - this._sampleDistances[first]) / seconds) : 0;
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
