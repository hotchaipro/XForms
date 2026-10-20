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

        public virtual Size MeasureNative(
            Size availableSize)
        {
            var nativeView = this._nativeView;
            if (null == nativeView)
            {
                return Size.Empty;
            }

            if (nativeView is NativeLayoutView layoutView)
            {
                return layoutView.MeasureContent(availableSize);
            }

            var fittingSize = nativeView.SizeThatFits(availableSize.ToFittingSize());

            return new Size(
                Math.Min((float)Math.Ceiling(fittingSize.Width), availableSize.Width),
                Math.Min((float)Math.Ceiling(fittingSize.Height), availableSize.Height));
        }

        public virtual void ArrangeNative(
            Rect frame)
        {
            var nativeView = this._nativeView;
            if (null == nativeView)
            {
                return;
            }

            // NOTE: Set the bounds and center rather than the frame, which is
            // undefined while the view has a transform (scale, rotation)
            var size = new CGSize(frame.Width, frame.Height);
            var bounds = nativeView.Bounds;
            if (bounds.Size != size)
            {
                nativeView.Bounds = new CGRect(bounds.Location, size);
            }

            var anchor = nativeView.Layer.AnchorPoint;
            nativeView.Center = new CGPoint(
                frame.X + (anchor.X * frame.Width),
                frame.Y + (anchor.Y * frame.Height));
        }

        public virtual void InvalidateMeasure()
        {
            this._nativeView?.RequestLayout();
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
            EasingFunction ease)
        {
            UIViewAnimationOptions curve;

            if (null == ease)
            {
                curve = UIViewAnimationOptions.CurveLinear;
            }
            else
            {
                var cubicEase = ease as CubicEase;
                var elasticEase = ease as SpringEase;

                if (null != cubicEase)
                {
                    if (ease.EasingMode == EasingMode.EaseIn)
                    {
                        curve = UIViewAnimationOptions.CurveEaseIn;
                    }
                    else if (ease.EasingMode == EasingMode.EaseOut)
                    {
                        curve = UIViewAnimationOptions.CurveEaseOut;
                    }
                    else if (ease.EasingMode == EasingMode.EaseInOut)
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

                    if (ease.EasingMode == EasingMode.EaseIn)
                    {
                        curve = UIViewAnimationOptions.CurveEaseIn;
                    }
                    else if (ease.EasingMode == EasingMode.EaseOut)
                    {
                        curve = UIViewAnimationOptions.CurveEaseOut;
                    }
                    else if (ease.EasingMode == EasingMode.EaseInOut)
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
            EasingFunction ease)
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
            EasingFunction ease)
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
            EasingFunction ease)
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

        private const string SpinAnimationKey = "xforms.spin";

        public void StartContinuousRotation(
            TimeSpan period)
        {
            var layer = this._nativeView?.Layer;
            if (null == layer)
            {
                return;
            }

            // Core Animation runs this in the render server, off the UI thread
            var animation = CoreAnimation.CABasicAnimation.FromKeyPath("transform.rotation.z");
            animation.From = Foundation.NSNumber.FromDouble(0);
            animation.To = Foundation.NSNumber.FromDouble(2 * Math.PI);
            animation.Duration = period.TotalSeconds;
            animation.RepeatCount = float.PositiveInfinity;
            animation.RemovedOnCompletion = false;
            layer.AddAnimation(animation, SpinAnimationKey);
        }

        public void StopContinuousRotation()
        {
            this._nativeView?.Layer.RemoveAnimation(SpinAnimationKey);
        }

        public bool Focus()
        {
            // NOTE: Keyboard focus for owner-drawn views isn't implemented on iOS yet
            return false;
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

            if (oldElement is NativeLayoutView oldLayout)
            {
                oldLayout.PointerInputDelegate = null;
            }

            if (newElement is NativeLayoutView newLayout)
            {
                newLayout.PointerInputDelegate = this.ViewDelegate;
            }
        }

        public virtual bool IsHitTestVisible
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
