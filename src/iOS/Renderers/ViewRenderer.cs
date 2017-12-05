using System;
using System.Threading.Tasks;
using CoreGraphics;
using UIKit;

namespace XForms.iOS.Renderers
{
    public abstract class ViewRenderer<TNativeElement> : ElementRenderer<TNativeElement>, IViewRenderer
        where TNativeElement : UIView
    {
        private UIView _nativeView;

        public ViewRenderer(
            View view)
            : base(view)
        {
        }

        private IViewDelegate ViewDelegate
        {
            get
            {
                return (IViewDelegate)this.Element;
            }
        }

        private NativeLayout.LayoutParams EnsureLayoutParameters()
        {
            var nativeView = this._nativeView;

            var extendedProperties = nativeView.LayoutProperties();

            NativeLayout.LayoutParams layoutParams = extendedProperties?.LayoutParameters as NativeLayout.LayoutParams;
            if (null == layoutParams)
            {
                var existingLayoutParams = extendedProperties?.LayoutParameters;
                if (null == existingLayoutParams)
                {
                    layoutParams = new NativeLayout.LayoutParams();
                }
                else
                {
                    layoutParams = new NativeLayout.LayoutParams(existingLayoutParams);
                }

                if (null != nativeView)
                {
                    extendedProperties.LayoutParameters = layoutParams;
                }
            }

            return layoutParams;
        }

        public virtual bool IsVisible
        {
            get
            {
                bool isVisible;

                var nativeView = this._nativeView;
                if (null != nativeView)
                {
                    isVisible = (!nativeView.Hidden);
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
                    nativeView.Hidden = !value;

                    //nativeView.RequestLayout();
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
                    size = NativeConversions.ToSize(nativeView.Frame.Size);
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
                        SizeToDimension(layoutParameters.Width),
                        SizeToDimension(layoutParameters.Height));
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

                    layoutParameters.Width = DimensionToSize(layoutParameters.HorizontalAlignment, value.Width);
                    layoutParameters.Height = DimensionToSize(layoutParameters.VerticalAlignment, value.Height);

                    nativeView.RequestLayout();
                }
            }
        }

        private static Dimension SizeToDimension(
            float size)
        {
            Dimension value;

            if (size == NativeLayout.LayoutParams.MatchParent)
            {
                value = Dimension.Auto;
            }
            else if (size == NativeLayout.LayoutParams.WrapContent)
            {
                value = Dimension.Auto;
            }
            else
            {
                value = new Dimension(size);
            }

            return value;
        }

        private static float DimensionToSize(
            LayoutAlignment alignment,
            Dimension dimension)
        {
            float value;

            if (dimension == Dimension.Auto)
            {
                if (alignment == LayoutAlignment.Fill)
                {
                    value = NativeLayout.LayoutParams.MatchParent;
                }
                else
                {
                    value = NativeLayout.LayoutParams.WrapContent;
                }
            }
            else
            {
                value = dimension;
            }

            return value;
        }

        public virtual Size MaximumSize
        {
            get
            {
                var nativeView = this._nativeView;
                if (null == nativeView)
                {
                    return Size.Empty;
                }

                var layoutParameters = this.EnsureLayoutParameters();

                return layoutParameters.MaximumSize;
            }

            set
            {
                var nativeView = this._nativeView;
                if (null != nativeView)
                {
                    var layoutParameters = this.EnsureLayoutParameters();

                    layoutParameters.MaximumSize = value;

                    nativeView.RequestLayout();
                }
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

                return layoutParameters.Margin;
            }

            set
            {
                var nativeView = this._nativeView;
                if (null != nativeView)
                {
                    var layoutParameters = this.EnsureLayoutParameters();

                    layoutParameters.Margin = value;

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
                return layoutParams.HorizontalAlignment;
            }

            set
            {
                var nativeView = this._nativeView;
                if (null == nativeView)
                {
                    return;
                }

                var layoutParams = this.EnsureLayoutParameters();

                layoutParams.HorizontalAlignment = value;

                // Handle auto-size
                if (layoutParams.Width == NativeLayout.LayoutParams.MatchParent)
                {
                    if (value != LayoutAlignment.Fill)
                    {
                        layoutParams.Width = NativeLayout.LayoutParams.WrapContent;
                    }
                }
                else if (layoutParams.Width == NativeLayout.LayoutParams.WrapContent)
                {
                    if (value == LayoutAlignment.Fill)
                    {
                        layoutParams.Width = NativeLayout.LayoutParams.MatchParent;
                    }
                }

                nativeView.RequestLayout();
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
                return layoutParams.VerticalAlignment;
            }

            set
            {
                var nativeView = this._nativeView;
                if (null == nativeView)
                {
                    return;
                }

                var layoutParams = this.EnsureLayoutParameters();

                layoutParams.VerticalAlignment = value;

                // Handle auto-size
                if (layoutParams.Height == NativeLayout.LayoutParams.MatchParent)
                {
                    if (value != LayoutAlignment.Fill)
                    {
                        layoutParams.Height = NativeLayout.LayoutParams.WrapContent;
                    }
                }
                else if (layoutParams.Height == NativeLayout.LayoutParams.WrapContent)
                {
                    if (value == LayoutAlignment.Fill)
                    {
                        layoutParams.Height = NativeLayout.LayoutParams.MatchParent;
                    }
                }

                nativeView.RequestLayout();
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
                    color = nativeView.BackgroundColor.ToColor();
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

                nativeView.BackgroundColor = NativeConversions.ToUIColor(value);
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

                return new Origin((float)nativeView.Layer.AnchorPoint.X, (float)nativeView.Layer.AnchorPoint.Y);
            }

            set
            {
                var nativeView = this._nativeView;
                if (nativeView != null)
                {
                    nativeView.Layer.AnchorPoint = new CGPoint(value.X, value.Y);
                }
            }
        }

        private void UpdateTransform()
        {
            var transform = CGAffineTransform.MakeIdentity();
            transform.Scale(this._scale, this._scale);
            transform.Rotate(this._rotation.Radians);
            transform.Translate(this._translation.X, this._translation.Y);
            this._nativeView.Transform = transform;
        }

        // TODO: Consider changing the type of Scale to Vector2 so the X and Y axes can be scaled independently
        private float _scale = 1.0f;
        public virtual float Scale
        {
            get
            {
                return this._scale;
            }

            set
            {
                this._scale = value;
                UpdateTransform();
            }
        }

        private Angle _rotation = Angle.Zero;
        public virtual Angle Rotation
        {
            get
            {
                return this._rotation;
            }

            set
            {
                this._rotation = value;
                UpdateTransform();
            }
        }

        private Point _translation = Point.Zero;
        public virtual Point Translation
        {
            get
            {
                return this._translation;
            }

            set
            {
                this._translation = value;
                UpdateTransform();
            }
        }

        private UIViewAnimationOptions GetAnimationCurve(
            Animation.EasingFunction ease)
        {
            UIViewAnimationOptions curve;

            if (null == ease)
            {
                curve = UIViewAnimationOptions.CurveLinear;
            }
            else
            {
                var cubicEase = ease as Animation.CubicEase;
                var elasticEase = ease as Animation.SpringEase;

                if (null != cubicEase)
                {
                    if (ease.EasingMode == Animation.EasingMode.EaseIn)
                    {
                        curve = UIViewAnimationOptions.CurveEaseIn;
                    }
                    else if (ease.EasingMode == Animation.EasingMode.EaseOut)
                    {
                        curve = UIViewAnimationOptions.CurveEaseOut;
                    }
                    else if (ease.EasingMode == Animation.EasingMode.EaseInOut)
                    {
                        curve = UIViewAnimationOptions.CurveEaseInOut;
                    }
                    else
                    {
                        throw new NotSupportedException("Unsupported easing mode.");
                    }
                }
                else if (null != elasticEase)
                {
                    // TODO: Custom interpolators

                    if (ease.EasingMode == Animation.EasingMode.EaseIn)
                    {
                        curve = UIViewAnimationOptions.CurveEaseIn;
                    }
                    else if (ease.EasingMode == Animation.EasingMode.EaseOut)
                    {
                        curve = UIViewAnimationOptions.CurveEaseOut;
                    }
                    else if (ease.EasingMode == Animation.EasingMode.EaseInOut)
                    {
                        curve = UIViewAnimationOptions.CurveEaseInOut;
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

            return curve;
        }

        public virtual Task ScaleTo(
            float scale,
            TimeSpan duration,
            XForms.Animation.EasingFunction ease)
        {
            var tcs = new TaskCompletionSource<object>();

            UIView.Animate(
                duration.TotalSeconds,
                0,
                GetAnimationCurve(ease),
                () => { this.Scale = scale; },
                () => { tcs.TrySetResult(null); });

            return tcs.Task;
        }

        public virtual Task RotateTo(
            Angle angle,
            TimeSpan duration,
            XForms.Animation.EasingFunction ease)
        {
            var tcs = new TaskCompletionSource<object>();

            UIView.Animate(
                duration.TotalSeconds,
                0,
                GetAnimationCurve(ease),
                () => { this.Rotation = angle; },
                () => { tcs.TrySetResult(null); });

            return tcs.Task;
        }

        public Task TranslateTo(
            Point point,
            TimeSpan duration,
            XForms.Animation.EasingFunction ease)
        {
            var tcs = new TaskCompletionSource<object>();

            UIView.Animate(
                duration.TotalSeconds,
                0,
                GetAnimationCurve(ease),
                () => { this.Translation = point; },
                () => { tcs.TrySetResult(null); });

            return tcs.Task;
        }

        public virtual void StopAnimation()
        {
            this._nativeView.Layer.RemoveAllAnimations();
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
                //oldElement.Touch -= Element_Touch;
            }

            if (null != newElement)
            {
                //newElement.Touch += Element_Touch;
            }
        }

        //private void Element_Touch(
        //    object sender,
        //    AndroidView.TouchEventArgs e)
        //{
        //    bool result = false;

        //    if (this.IsInputVisible)
        //    {
        //        var control = this.ViewDelegate;
        //        if (null != control)
        //        {
        //            var motionEvent = e.Event;
        //            bool isInContact;
        //            bool isKnownAction;
        //            PointerInputState pointerState;
        //            if (motionEvent.Action == MotionEventActions.Down)
        //            {
        //                pointerState = PointerInputState.Began;
        //                isKnownAction = true;
        //                isInContact = true;
        //            }
        //            else if (motionEvent.Action == MotionEventActions.Up)
        //            {
        //                pointerState = PointerInputState.Ended;
        //                isKnownAction = true;
        //                isInContact = true;
        //            }
        //            else if (motionEvent.Action == MotionEventActions.Move)
        //            {
        //                pointerState = PointerInputState.Moved;
        //                isKnownAction = true;
        //                isInContact = true;
        //            }
        //            else if (motionEvent.Action == MotionEventActions.Cancel)
        //            {
        //                pointerState = PointerInputState.Canceled;
        //                isKnownAction = true;
        //                isInContact = true;
        //            }
        //            else if (motionEvent.Action == MotionEventActions.HoverMove)
        //            {
        //                pointerState = PointerInputState.Moved;
        //                isKnownAction = true;
        //                isInContact = false;
        //            }
        //            else
        //            {
        //                pointerState = PointerInputState.Canceled;
        //                isKnownAction = false;
        //                isInContact = false;
        //            }

        //            if (isKnownAction)
        //            {
        //                var contactPoint = new Point(
        //                    this.PixelsToDevicePixels(motionEvent.GetX()),
        //                    this.PixelsToDevicePixels(motionEvent.GetY()));

        //                var pointerEvent = new PointerInputEvent(contactPoint, pointerState, isInContact);

        //                result = control.HandlePointerEvent(pointerEvent);

        //                // TODO: Equivalent of CapturePointer
        //                // TODO: this._nativeView.Parent.RequestDisallowInterceptTouchEvent(true);
        //            }
        //        }
        //    }

        //    e.Handled = result;
        //}

        public virtual bool IsInputVisible
        {
            get
            {
                return this._nativeView.UserInteractionEnabled;
            }

            set
            {
                this._nativeView.UserInteractionEnabled = value;
            }
        }
    }
}
