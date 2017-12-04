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

        private BaseLayoutParams EnsureLayoutParameters()
        {
            var nativeView = this._nativeView;

            BaseLayoutParams layoutParams = nativeView?.LayoutParameters as BaseLayoutParams;
            if (null == layoutParams)
            {
                ViewGroup.LayoutParams existingLayoutParams = nativeView?.LayoutParameters;
                if (null == existingLayoutParams)
                {
                    layoutParams = new BaseLayoutParams();
                }
                else
                {
                    layoutParams = new BaseLayoutParams(existingLayoutParams);
                }

                if (null != nativeView)
                {
                    nativeView.LayoutParameters = layoutParams;
                }
            }

            return layoutParams;
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

        public virtual Size Size
        {
            get
            {
                return this.ActualSize;
            }

            set
            {
                this.RequestedSize = value;
            }
        }

        private Size ActualSize
        {
            get
            {
                Size size;

                var nativeView = this._nativeView;
                if (null != nativeView)
                {
                    size = new Size(
                        NativeConversions.DimensionFromAndroidDimension(this.NativeContext, nativeView.Width),
                        NativeConversions.DimensionFromAndroidDimension(this.NativeContext, nativeView.Height));
                }
                else
                {
                    size = Size.Empty;
                }

                return size;
            }
        }

        private Size RequestedSize
        {
            get
            {
                Size size;

                var nativeView = this._nativeView;
                if (null != nativeView)
                {
                    var layoutParameters = this.EnsureLayoutParameters();

                    size = new Size(
                        NativeConversions.DimensionFromAndroidDimension(this.NativeContext, layoutParameters.Width),
                        NativeConversions.DimensionFromAndroidDimension(this.NativeContext, layoutParameters.Height));
                }
                else
                {
                    size = Size.Empty;
                }

                return size;
            }

            set
            {
                var nativeView = this._nativeView;
                if (null != nativeView)
                {
                    var layoutParameters = this.EnsureLayoutParameters();

                    layoutParameters.Width = DimensionToAndroidDimension(layoutParameters.Gravity.FromAndroidHorizontalLayoutGravityFlags(), value.Width);
                    layoutParameters.Height = DimensionToAndroidDimension(layoutParameters.Gravity.FromAndroidVerticalLayoutGravityFlags(), value.Height);

                    nativeView.RequestLayout();
                }
            }
        }

        private int DimensionToAndroidDimension(
            LayoutAlignment alignment,
            Dimension dimension)
        {
            if (dimension == Dimension.Auto)
            {
                if (alignment == LayoutAlignment.Fill)
                {
                    return global::Android.Views.ViewGroup.LayoutParams.MatchParent;
                }
                else
                {
                    return global::Android.Views.ViewGroup.LayoutParams.WrapContent;
                }
            }

            return (int)DevicePixelsToPixels(dimension);
        }

        public virtual Size MaximumSize
        {
            get
            {
                throw new NotImplementedException();
                //Size size;

                //var nativeView = this._view;
                //if (null != nativeView)
                //{
                //    float maxWidth = ((nativeView.MaxWidth == double.PositiveInfinity) ? float.NaN : (float)nativeView.MaxWidth);
                //    float maxHeight = ((nativeView.MaxHeight == double.PositiveInfinity) ? float.NaN : (float)nativeView.MaxHeight);
                //    size = new Size(maxWidth, maxHeight);
                //}
                //else
                //{
                //    size = new Size(Dimension.Auto, Dimension.Auto);
                //}

                //return size;
            }

            set
            {
                //throw new NotImplementedException();
                //var nativeView = this._view;
                //if (null != nativeView)
                //{
                //    if ((float.IsNaN(value.Width)) || (float.IsInfinity(value.Width)))
                //    {
                //        nativeView.MaxWidth = double.PositiveInfinity;
                //    }
                //    else
                //    {
                //        nativeView.MaxWidth = value.Width;
                //    }

                //    if ((float.IsNaN(value.Height)) || (float.IsInfinity(value.Height)))
                //    {
                //        nativeView.MaxHeight = double.PositiveInfinity;
                //    }
                //    else
                //    {
                //        nativeView.MaxHeight = value.Height;
                //    }
                //}
            }
        }

        public virtual Thickness Margin
        {
            get
            {
                var nativeView = this._nativeView;
                if (null == nativeView)
                {
                    return Thickness.Zero;
                }

                var layoutParameters = this.EnsureLayoutParameters();

                return new Thickness(
                    PixelsToDevicePixels(layoutParameters.LeftMargin),
                    PixelsToDevicePixels(layoutParameters.TopMargin),
                    PixelsToDevicePixels(layoutParameters.RightMargin),
                    PixelsToDevicePixels(layoutParameters.BottomMargin));
            }

            set
            {
                var nativeView = this._nativeView;
                if (null != nativeView)
                {
                    var layoutParameters = this.EnsureLayoutParameters();
                    layoutParameters.SetMargins(
                        (int)DevicePixelsToPixels(value.Left),
                        (int)DevicePixelsToPixels(value.Top),
                        (int)DevicePixelsToPixels(value.Right),
                        (int)DevicePixelsToPixels(value.Bottom));

                    nativeView.RequestLayout();
                }
            }
        }

        public virtual LayoutAlignment HorizontalAlignment
        {
            get
            {
                var nativeView = this._nativeView;
                if (null == nativeView)
                {
                    return LayoutAlignment.Start;
                }

                var layoutParams = this.EnsureLayoutParameters();
                return layoutParams.Gravity.FromAndroidHorizontalLayoutGravityFlags();
            }

            set
            {
                var nativeView = this._nativeView;
                if (null == nativeView)
                {
                    return;
                }

                var layoutParams = this.EnsureLayoutParameters();

                var gravityFlags = value.ToAndroidHorizontalLayoutGravityFlags();
                layoutParams.Gravity = (layoutParams.Gravity & ~GravityFlags.HorizontalGravityMask) | gravityFlags;

                // Handle auto-size
                if (layoutParams.Width == ViewGroup.LayoutParams.MatchParent)
                {
                    if (value != LayoutAlignment.Fill)
                    {
                        layoutParams.Width = ViewGroup.LayoutParams.WrapContent;
                    }
                }
                else if (layoutParams.Width == ViewGroup.LayoutParams.WrapContent)
                {
                    if (value == LayoutAlignment.Fill)
                    {
                        layoutParams.Width = ViewGroup.LayoutParams.MatchParent;
                    }
                }

#if DEBUG_LAYOUT
                if (this.HorizontalAlignment != value)
                {
                    throw new Exception();
                }
#endif

                this._nativeView.RequestLayout();
            }
        }

        public virtual LayoutAlignment VerticalAlignment
        {
            get
            {
                var nativeView = this._nativeView;
                if (null == nativeView)
                {
                    return LayoutAlignment.Start;
                }

                var layoutParams = this.EnsureLayoutParameters();
                return layoutParams.Gravity.FromAndroidVerticalLayoutGravityFlags();
            }

            set
            {
                var nativeView = this._nativeView;
                if (null == nativeView)
                {
                    return;
                }

                var layoutParams = this.EnsureLayoutParameters();

                var gravityFlags = value.ToAndroidVerticalLayoutGravityFlags();
                layoutParams.Gravity = (layoutParams.Gravity & ~GravityFlags.VerticalGravityMask) | gravityFlags;

#if DEBUG_LAYOUT
                if (this.VerticalAlignment != value)
                {
                    throw new Exception();
                }
#endif
                // Handle auto-size
                if (layoutParams.Height == ViewGroup.LayoutParams.MatchParent)
                {
                    if (value != LayoutAlignment.Fill)
                    {
                        layoutParams.Height = ViewGroup.LayoutParams.WrapContent;
                    }
                }
                else if (layoutParams.Height == ViewGroup.LayoutParams.WrapContent)
                {
                    if (value == LayoutAlignment.Fill)
                    {
                        layoutParams.Height = ViewGroup.LayoutParams.MatchParent;
                    }
                }

                this._nativeView.RequestLayout();
            }
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

        private sealed class AsyncAnimatorListener : Java.Lang.Object, Animator.IAnimatorListener
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
                Animator animation)
            {
                this._tcs.SetCanceled();
            }

            public void OnAnimationEnd(
                Animator animation)
            {
                this._tcs.TrySetResult(null);
            }

            public void OnAnimationRepeat(
                Animator animation)
            {
            }

            public void OnAnimationStart(
                Animator animation)
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

            if (this.IsInputVisible)
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

                        var pointerEvent = new PointerInputEvent(contactPoint, pointerState, isInContact);

                        result = control.HandlePointerEvent(pointerEvent);

                        if ((result) && (pointerState == PointerInputState.Began))
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

        public virtual bool IsInputVisible
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
