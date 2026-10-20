using System;
using System.Threading.Tasks;
using Android.Views;
using Android.Views.Animations;
using Android.Animation;
using Android.Graphics.Drawables;
using AndroidView = global::Android.Views.View;

namespace XForms.Android.Renderers
{
    public abstract class ViewRenderer<TNativeElement> : ElementRenderer<TNativeElement>, IViewRenderer
        where TNativeElement : AndroidView
    {
        private AndroidView _nativeView;

        public ViewRenderer(
            global::Android.Content.Context context,
            View view)
            : base(context, view)
        {
        }

        private IViewDelegate ViewDelegate
        {
            get
            {
                return (IViewDelegate)this.Element;
            }
        }

        protected float PixelsToDevicePixels(
            float pixels)
        {
            return NativeConversions.PixelsToDevicePixels(this.NativeContext, pixels);
        }

        protected float DevicePixelsToPixels(
            float dp)
        {
            return NativeConversions.DevicePixelsToPixels(this.NativeContext, dp);
        }

        public virtual bool IsVisible
        {
            get
            {
                bool isVisible;

                var nativeView = this._nativeView;
                if (null != nativeView)
                {
                    isVisible = (nativeView.Visibility == global::Android.Views.ViewStates.Visible);
                }
                else
                {
                    isVisible = true;
                }

                return isVisible;
            }

            set
            {
                var nativeView = this._nativeView;
                if (null != nativeView)
                {
                    nativeView.Visibility = (value ? global::Android.Views.ViewStates.Visible : global::Android.Views.ViewStates.Gone);

                    nativeView.RequestLayout();
                }
            }
        }

        public virtual Size MeasureNative(
            Size availableSize)
        {
            var nativeView = this._nativeView;
            if (null == nativeView)
            {
                return Size.Empty;
            }

            nativeView.Measure(
                this.ToMeasureSpec(availableSize.Width),
                this.ToMeasureSpec(availableSize.Height));

            return new Size(
                this.PixelsToDevicePixels(nativeView.MeasuredWidth),
                this.PixelsToDevicePixels(nativeView.MeasuredHeight));
        }

        public virtual void ArrangeNative(
            Rect frame)
        {
            var nativeView = this._nativeView;
            if (null == nativeView)
            {
                return;
            }

            // Round each edge (not the size) so adjacent views meet without gaps
            int left = (int)Math.Round(this.DevicePixelsToPixels(frame.X));
            int top = (int)Math.Round(this.DevicePixelsToPixels(frame.Y));
            int right = (int)Math.Round(this.DevicePixelsToPixels(frame.X + frame.Width));
            int bottom = (int)Math.Round(this.DevicePixelsToPixels(frame.Y + frame.Height));

            // Never lose a pixel to edge rounding: a view arranged at exactly its
            // measured size (text in particular) would otherwise wrap or clip
            right = Math.Max(right, left + (int)Math.Ceiling(this.DevicePixelsToPixels(frame.Width) - 0.01f));
            bottom = Math.Max(bottom, top + (int)Math.Ceiling(this.DevicePixelsToPixels(frame.Height) - 0.01f));

            // NOTE: Always finish with an exact measure at the final size: views
            // such as ListView lay out their content using the mode of the last
            // measure, not just its size. Android caches measurements per spec,
            // so this costs nothing when the view hasn't changed.
            nativeView.Measure(
                AndroidView.MeasureSpec.MakeMeasureSpec(right - left, MeasureSpecMode.Exactly),
                AndroidView.MeasureSpec.MakeMeasureSpec(bottom - top, MeasureSpecMode.Exactly));

            nativeView.Layout(left, top, right, bottom);
        }

        public virtual void InvalidateMeasure()
        {
            this._nativeView?.RequestLayout();
        }

        public bool ClipToBounds
        {
            set
            {
                // NOTE: ClipChildren clips each child to the child's own bounds, not
                // to this view's. Clipping to the view's outline (its bounds) clips
                // its drawing and all its descendants.
                var nativeView = this._nativeView;
                if (null != nativeView)
                {
                    nativeView.OutlineProvider = global::Android.Views.ViewOutlineProvider.Bounds;
                    nativeView.ClipToOutline = value;
                }
            }
        }

        private int ToMeasureSpec(
            float availableLength)
        {
            if (float.IsInfinity(availableLength) || float.IsNaN(availableLength))
            {
                return AndroidView.MeasureSpec.MakeMeasureSpec(0, MeasureSpecMode.Unspecified);
            }

            int pixels = (int)Math.Ceiling(this.DevicePixelsToPixels(availableLength));
            return AndroidView.MeasureSpec.MakeMeasureSpec(Math.Max(0, pixels), MeasureSpecMode.AtMost);
        }

        public virtual Color BackgroundColor
        {
            get
            {
                Color color = Colors.Transparent;
                var nativeView = this._nativeView;
                if (null != nativeView)
                {
                    var background = nativeView.Background as ColorDrawable;
                    if (null != background)
                    {
                        color = NativeConversions.FromAndroidColor(background.Color);
                    }
                }
                return color;
            }

            set
            {
                var nativeView = this._nativeView;
                if (null == nativeView)
                {
                    return;
                }

                nativeView.SetBackgroundColor(NativeConversions.ToAndroidColor(value));
            }
        }

        public virtual float Opacity
        {
            get
            {
                return (float)this._nativeView?.Alpha;
            }

            set
            {
                var nativeView = this._nativeView;
                if (null != nativeView)
                {
                    nativeView.Alpha = value;
                }
            }
        }

        public virtual Origin Anchor
        {
            get
            {
                var nativeView = this._nativeView;
                if (null == nativeView)
                {
                    return Origin.TopLeft;
                }

                var pivotX = nativeView.PivotX;
                var pivotY = nativeView.PivotY;

                return new Origin(
                    nativeView.Width == 0 ? 0 : pivotX / nativeView.Width,
                    nativeView.Height == 0 ? 0 : pivotY / nativeView.Height);
            }

            set
            {
                var nativeView = this._nativeView;
                if (nativeView != null)
                {
                    nativeView.PivotX = value.X * nativeView.Width;
                    nativeView.PivotY = value.Y * nativeView.Height;
                }
            }
        }

        // TODO: Consider changing the type of Scale to Vector2 so the X and Y axes can be scaled independently
        public virtual float Scale
        {
            get
            {
                return (float)this._nativeView?.ScaleX;
            }

            set
            {
                var nativeView = this._nativeView;
                if (null != nativeView)
                {
                    nativeView.ScaleX = value;
                    nativeView.ScaleY = value;
                }
            }
        }

        public virtual Angle Rotation
        {
            get
            {
                return Angle.FromDegrees((float)this._nativeView?.Rotation);
            }

            set
            {
                var nativeView = this._nativeView;
                if (null != nativeView)
                {
                    nativeView.Rotation = value.Degrees;
                }
            }
        }

        public virtual Point Translation
        {
            get
            {
                var nativeView = this._nativeView;
                if (null == nativeView)
                {
                    return Point.Zero;
                }

                return new Point(
                    this.PixelsToDevicePixels(nativeView.TranslationX),
                    this.PixelsToDevicePixels(nativeView.TranslationY));
            }

            set
            {
                var nativeView = this._nativeView;
                if (null != nativeView)
                {
                    nativeView.TranslationX = this.DevicePixelsToPixels(value.X);
                    nativeView.TranslationY = this.DevicePixelsToPixels(value.Y);
                }
            }
        }

        public virtual Task ScaleTo(
            float scale,
            TimeSpan duration,
            EasingFunction ease)
        {
            var nativeView = this._nativeView;
            if (null == nativeView)
            {
                return null;
            }

            var animation = nativeView.Animate();
            animation.ScaleX(scale);
            animation.ScaleY(scale);
            animation.SetDuration((long)duration.TotalMilliseconds);
            var androidEase = GetAndroidInterpolator(ease);
            animation.SetInterpolator(androidEase);
            return RunAnimationAsync(animation);
        }

        public virtual Task RotateTo(
            Angle angle,
            TimeSpan duration,
            EasingFunction ease)
        {
            var nativeView = this._nativeView;
            if (null == nativeView)
            {
                return null;
            }

            var animation = nativeView.Animate();
            animation.Rotation(angle.Degrees);
            animation.SetDuration((long)duration.TotalMilliseconds);
            var androidEase = GetAndroidInterpolator(ease);
            animation.SetInterpolator(androidEase);
            return RunAnimationAsync(animation);
        }

        public Task TranslateTo(
            Point point,
            TimeSpan duration,
            EasingFunction ease)
        {
            var nativeView = this._nativeView;
            if (null == nativeView)
            {
                return null;
            }

            var animation = nativeView.Animate();
            animation.TranslationX(this.DevicePixelsToPixels(point.X));
            animation.TranslationY(this.DevicePixelsToPixels(point.Y));
            animation.SetDuration((long)duration.TotalMilliseconds);
            var androidEase = GetAndroidInterpolator(ease);
            animation.SetInterpolator(androidEase);
            return RunAnimationAsync(animation);
        }

        public virtual void StopAnimation()
        {
            this._nativeView?.Animation?.Cancel();
        }

        private global::Android.Animation.ObjectAnimator _spinAnimator;

        public void StartContinuousRotation(
            TimeSpan period)
        {
            this.StopContinuousRotation();

            var nativeView = this._nativeView;
            if (null == nativeView)
            {
                return;
            }

            // The render thread runs property animations of a hardware layer's rotation
            var animator = global::Android.Animation.ObjectAnimator.OfFloat(nativeView, "rotation", 0f, 360f);
            animator.SetDuration((long)period.TotalMilliseconds);
            animator.RepeatCount = global::Android.Animation.ValueAnimator.Infinite;
            animator.SetInterpolator(new global::Android.Views.Animations.LinearInterpolator());
            animator.Start();
            this._spinAnimator = animator;
        }

        public void StopContinuousRotation()
        {
            var animator = this._spinAnimator;
            if (null != animator)
            {
                this._spinAnimator = null;
                animator.Cancel();
                animator.Dispose();

                if (null != this._nativeView)
                {
                    this._nativeView.Rotation = 0;
                }
            }
        }

        public bool Focus()
        {
            // NOTE: Keyboard focus for owner-drawn views isn't implemented on Android yet
            return false;
        }

        private ITimeInterpolator GetAndroidInterpolator(
            EasingFunction ease)
        {
            ITimeInterpolator androidEase = null;

            if (null != ease)
            {
                var cubicEase = ease as CubicEase;
                var elasticEase = ease as SpringEase;

                if (null != cubicEase)
                {
                    if (ease.EasingMode == EasingMode.EaseIn)
                    {
                        androidEase = new AccelerateInterpolator(1.5f);
                    }
                    else if (ease.EasingMode == EasingMode.EaseOut)
                    {
                        androidEase = new DecelerateInterpolator(1.5f);
                    }
                    else if (ease.EasingMode == EasingMode.EaseInOut)
                    {
                        // TODO: A custom interpolator. This is actually a circular ease, not a cubic ease-in-out
                        androidEase = new AccelerateDecelerateInterpolator();
                    }
                    else
                    {
                        throw new NotSupportedException("Unsupported easing mode.");
                    }
                }
                else if (null != elasticEase)
                {
                    // TODO: Custom interpolators
                    if (ease.EasingMode == EasingMode.EaseIn)
                    {
                        androidEase = new BounceInterpolator();
                    }
                    else if (ease.EasingMode == EasingMode.EaseOut)
                    {
                        androidEase = new BounceInterpolator();
                    }
                    else if (ease.EasingMode == EasingMode.EaseInOut)
                    {
                        androidEase = new BounceInterpolator();
                    }
                    else
                    {
                        throw new NotSupportedException("Unsupported easing mode.");
                    }
                }
                else
                {
                    throw new NotSupportedException("Unsupported easing function.");
                }
            }

            return androidEase;
        }

        private sealed class AsyncAnimatorListener : Java.Lang.Object, global::Android.Animation.Animator.IAnimatorListener
        {
            private TaskCompletionSource<object> _tcs;

            public AsyncAnimatorListener()
            {
                this._tcs = new TaskCompletionSource<object>();
            }

            public Task Task
            {
                get
                {
                    return this._tcs.Task;
                }
            }

            public void OnAnimationCancel(
                global::Android.Animation.Animator animation)
            {
                this._tcs.SetCanceled();
            }

            public void OnAnimationEnd(
                global::Android.Animation.Animator animation)
            {
                this._tcs.TrySetResult(null);
            }

            public void OnAnimationRepeat(
                global::Android.Animation.Animator animation)
            {
            }

            public void OnAnimationStart(
                global::Android.Animation.Animator animation)
            {
            }
        }

        private static Task RunAnimationAsync(
            ViewPropertyAnimator animator)
        {
            if (null == animator)
            {
                throw new ArgumentNullException(nameof(animator));
            }

            AsyncAnimatorListener listener = new AsyncAnimatorListener();
            animator.SetListener(listener);
            animator.Start();

            return listener.Task;
        }

        protected override void OnNativeElementSet(
            TNativeElement oldElement,
            TNativeElement newElement)
        {
            if (null == newElement)
            {
                throw new ArgumentNullException(nameof(newElement));
            }

            base.OnNativeElementSet(oldElement, newElement);

            this._nativeView = newElement;

            if (null != oldElement)
            {
                oldElement.Touch -= Element_Touch;
            }

            if (null != newElement)
            {
                newElement.Touch += Element_Touch;
            }
        }

        private void Element_Touch(
            object sender,
            AndroidView.TouchEventArgs e)
        {
            bool result = false;

            if (this.IsHitTestVisible)
            {
                var control = this.ViewDelegate;
                if (null != control)
                {
                    var motionEvent = e.Event;
                    bool isInContact;
                    bool isKnownAction;
                    PointerInputState pointerState;
                    if (motionEvent.Action == MotionEventActions.Down)
                    {
                        pointerState = PointerInputState.Began;
                        isKnownAction = true;
                        isInContact = true;
                    }
                    else if (motionEvent.Action == MotionEventActions.Up)
                    {
                        pointerState = PointerInputState.Ended;
                        isKnownAction = true;
                        isInContact = true;
                    }
                    else if (motionEvent.Action == MotionEventActions.Move)
                    {
                        pointerState = PointerInputState.Moved;
                        isKnownAction = true;
                        isInContact = true;
                    }
                    else if (motionEvent.Action == MotionEventActions.Cancel)
                    {
                        pointerState = PointerInputState.Canceled;
                        isKnownAction = true;
                        isInContact = true;
                    }
                    else if (motionEvent.Action == MotionEventActions.HoverMove)
                    {
                        pointerState = PointerInputState.Moved;
                        isKnownAction = true;
                        isInContact = false;
                    }
                    else
                    {
                        pointerState = PointerInputState.Canceled;
                        isKnownAction = false;
                        isInContact = false;
                    }

                    if (isKnownAction)
                    {
                        var contactPoint = new Point(
                            this.PixelsToDevicePixels(motionEvent.GetX()),
                            this.PixelsToDevicePixels(motionEvent.GetY()));

                        var pointerEvent = new PointerEventArgs(contactPoint, pointerState, isInContact);

                        result = control.HandlePointerEvent(pointerEvent);

                        // A handled move means a gesture (such as a swipe) is under
                        // way; stop scrolling parents from taking the pointer over
                        if ((result) && (pointerState == PointerInputState.Moved))
                        {
                            this.CapturePointer();
                        }
                        else if ((pointerState == PointerInputState.Ended)
                            || (pointerState == PointerInputState.Canceled))
                        {
                            this.ReleasePointer();
                        }
                    }
                }
            }

            e.Handled = result;
        }

        private void CapturePointer()
        {
            this.NativeElement?.Parent?.RequestDisallowInterceptTouchEvent(true);
        }

        private void ReleasePointer()
        {
            this.NativeElement?.Parent?.RequestDisallowInterceptTouchEvent(false);
        }

        public virtual bool IsHitTestVisible
        {
            get
            {
                return this._nativeView?.Enabled ?? false;
            }

            set
            {
                var nativeView = this._nativeView;
                if (null != nativeView)
                {
                    nativeView.Enabled = true;
                }
            }
        }
    }
}
