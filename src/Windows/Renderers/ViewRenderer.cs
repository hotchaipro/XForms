using System;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;

namespace XForms.Windows.Renderers
{
    public class ViewRenderer : ElementRenderer, IViewRenderer
    {
        private FrameworkElement _frameworkElement;
        private bool _isPointerCaptured;

        // Animation support
        private CompositeTransform _compositeTransform;
        private Storyboard _scaleStoryboard;
        private DoubleAnimation _scaleXAnimation, _scaleYAnimation;
        private Storyboard _rotationStoryboard;
        private DoubleAnimation _rotationAnimation;
        private Storyboard _translationStoryboard;
        private DoubleAnimation _translateXAnimation, _translateYAnimation;

        public ViewRenderer(
            View view)
            : base(view)
        {
        }

        private FrameworkElement FrameworkElement
        {
            get
            {
                return this._frameworkElement;
            }
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

                var frameworkElement = this.FrameworkElement;
                if (null != frameworkElement)
                {
                    isVisible = (frameworkElement.Visibility == Visibility.Visible);
                }
                else
                {
                    isVisible = true;
                }

                return isVisible;
            }

            set
            {
                var frameworkElement = this.FrameworkElement;
                if (null != frameworkElement)
                {
                    frameworkElement.Visibility = (value ? Visibility.Visible : Visibility.Collapsed);
                }
            }
        }

        public Size Size
        {
            get
            {
                Size size;

                var frameworkElement = this.FrameworkElement;
                if (null != frameworkElement)
                {
                    size = new Size((float)frameworkElement.ActualWidth, (float)frameworkElement.ActualHeight);
                }
                else
                {
                    size = Size.Empty;
                }

                return size;
            }

            set
            {
                var frameworkElement = this.FrameworkElement;
                if (null != frameworkElement)
                {
                    frameworkElement.Width = value.Width;
                    frameworkElement.Height = value.Height;
                }
            }
        }

        public Size MaximumSize
        {
            get
            {
                Size size;

                var frameworkElement = this.FrameworkElement;
                if (null != frameworkElement)
                {
                    float maxWidth = ((frameworkElement.MaxWidth == double.PositiveInfinity) ? float.NaN : (float)frameworkElement.MaxWidth);
                    float maxHeight = ((frameworkElement.MaxHeight == double.PositiveInfinity) ? float.NaN : (float)frameworkElement.MaxHeight);
                    size = new Size(maxWidth, maxHeight);
                }
                else
                {
                    size = new Size(Dimension.Auto, Dimension.Auto);
                }

                return size;
            }

            set
            {
                var frameworkElement = this.FrameworkElement;
                if (null != frameworkElement)
                {
                    if ((float.IsNaN(value.Width)) || (float.IsInfinity(value.Width)))
                    {
                        frameworkElement.MaxWidth = double.PositiveInfinity;
                    }
                    else
                    {
                        frameworkElement.MaxWidth = value.Width;
                    }

                    if ((float.IsNaN(value.Height)) || (float.IsInfinity(value.Height)))
                    {
                        frameworkElement.MaxHeight = double.PositiveInfinity;
                    }
                    else
                    {
                        frameworkElement.MaxHeight = value.Height;
                    }
                }
            }
        }

        public Thickness Margin
        {
            get
            {
                return this.FrameworkElement?.Margin.ToThickness() ?? Thickness.Zero;
            }

            set
            {
                var frameworkElement = this.FrameworkElement;
                if (null != frameworkElement)
                {
                    frameworkElement.Margin = value.ToXamlThickness();
                }
            }
        }

        public LayoutAlignment HorizontalAlignment
        {
            get
            {
                return this.FrameworkElement?.HorizontalAlignment.ToLayoutAlignment() ?? LayoutAlignment.Start;
            }

            set
            {
                var frameworkElement = this.FrameworkElement;
                if (null != frameworkElement)
                {
                    frameworkElement.HorizontalAlignment = value.ToXamlHorizontalAlignment();
                }
            }
        }

        public LayoutAlignment VerticalAlignment
        {
            get
            {
                return this.FrameworkElement?.VerticalAlignment.ToLayoutAlignment() ?? LayoutAlignment.Start;
            }

            set
            {
                var frameworkElement = this.FrameworkElement;
                if (null != frameworkElement)
                {
                    frameworkElement.VerticalAlignment = value.ToXamlVerticalAlignment();
                }
            }
        }

        public float Opacity
        {
            get
            {
                return (float)this.FrameworkElement?.Opacity;
            }

            set
            {
                this.FrameworkElement.Opacity = value;
            }
        }

        public Origin Anchor
        {
            get
            {
                var nativeOrigin = this.FrameworkElement?.RenderTransformOrigin;
                if (!nativeOrigin.HasValue)
                {
                    return Origin.Center;
                }

                return new Origin((float)nativeOrigin.Value.X, (float)nativeOrigin.Value.Y);
            }

            set
            {
                this.FrameworkElement.RenderTransformOrigin = new global::Windows.Foundation.Point(value.X, value.Y);
            }

        }

        // TODO: Consider changing the type of Scale to Size so the X and Y axes can be scaled independently
        public float Scale
        {
            get
            {
                return (float)this._compositeTransform.ScaleX;
            }

            set
            {
                this._compositeTransform.ScaleX = value;
                this._compositeTransform.ScaleY = value;
            }
        }

        public Angle Rotation
        {
            get
            {
                return Angle.FromDegrees((float)this._compositeTransform.Rotation);
            }

            set
            {
                this._compositeTransform.Rotation = value.Degrees;
            }
        }

        public Point Translation
        {
            get
            {
                return new Point((float)this._compositeTransform.TranslateX, (float)this._compositeTransform.TranslateY);
            }

            set
            {
                this._compositeTransform.TranslateX = value.X;
                this._compositeTransform.TranslateY = value.Y;
            }
        }

        public Task ScaleTo(
            float scale,
            TimeSpan duration,
            EasingFunction ease)
        {
            if (null == this._scaleStoryboard)
            {
                this._scaleStoryboard = new Storyboard();

                this._scaleXAnimation = new DoubleAnimation()
                {
                    FillBehavior = FillBehavior.HoldEnd,
                };
                Storyboard.SetTarget(this._scaleXAnimation, this.FrameworkElement.RenderTransform);
                Storyboard.SetTargetProperty(this._scaleXAnimation, "ScaleX");
                this._scaleStoryboard.Children.Add(this._scaleXAnimation);

                this._scaleYAnimation = new DoubleAnimation()
                {
                    FillBehavior = FillBehavior.HoldEnd,
                };
                Storyboard.SetTarget(this._scaleYAnimation, this.FrameworkElement.RenderTransform);
                Storyboard.SetTargetProperty(this._scaleYAnimation, "ScaleY");
                this._scaleStoryboard.Children.Add(this._scaleYAnimation);
            }
            else
            {
                this._scaleStoryboard.Pause();
            }

            EasingFunctionBase xamlEase = GetXamlEasingFunction(ease);

            this._scaleXAnimation.Duration = duration;
            this._scaleXAnimation.EasingFunction = xamlEase;
            this._scaleXAnimation.To = scale;

            this._scaleYAnimation.Duration = duration;
            this._scaleYAnimation.EasingFunction = xamlEase;
            this._scaleYAnimation.To = scale;

            return RunStoryboardAsync(this._scaleStoryboard);
        }

        public Task RotateTo(
            Angle angle,
            TimeSpan duration,
            EasingFunction ease)
        {
            if (null == this._rotationStoryboard)
            {
                this._rotationStoryboard = new Storyboard();

                this._rotationAnimation = new DoubleAnimation()
                {
                    FillBehavior = FillBehavior.HoldEnd,
                };
                Storyboard.SetTarget(this._rotationAnimation, this.FrameworkElement.RenderTransform);
                Storyboard.SetTargetProperty(this._rotationAnimation, "Rotation");
                this._rotationStoryboard.Children.Add(this._rotationAnimation);
            }
            else
            {
                this._rotationStoryboard.Pause();
            }

            EasingFunctionBase xamlEase = GetXamlEasingFunction(ease);

            this._rotationAnimation.Duration = duration;
            this._rotationAnimation.EasingFunction = xamlEase;
            this._rotationAnimation.To = angle.Degrees;

            return RunStoryboardAsync(this._rotationStoryboard);
        }

        public Task TranslateTo(
            Point point,
            TimeSpan duration,
            EasingFunction ease)
        {
            if (null == this._translationStoryboard)
            {
                this._translationStoryboard = new Storyboard()
                {
                    FillBehavior = FillBehavior.HoldEnd,
                };

                this._translateXAnimation = new DoubleAnimation()
                {
                    FillBehavior = FillBehavior.HoldEnd,
                };
                Storyboard.SetTarget(this._translateXAnimation, this.FrameworkElement.RenderTransform);
                Storyboard.SetTargetProperty(this._translateXAnimation, "TranslateX");
                this._translationStoryboard.Children.Add(this._translateXAnimation);

                this._translateYAnimation = new DoubleAnimation()
                {
                    FillBehavior = FillBehavior.HoldEnd,
                };
                Storyboard.SetTarget(this._translateYAnimation, this.FrameworkElement.RenderTransform);
                Storyboard.SetTargetProperty(this._translateYAnimation, "TranslateY");
                this._translationStoryboard.Children.Add(this._translateYAnimation);
            }
            else
            {
                this._translationStoryboard.Pause();
            }

            EasingFunctionBase xamlEase = GetXamlEasingFunction(ease);

            this._translateXAnimation.Duration = duration;
            this._translateXAnimation.EasingFunction = xamlEase;
            this._translateXAnimation.To = point.X;

            this._translateYAnimation.Duration = duration;
            this._translateYAnimation.EasingFunction = xamlEase;
            this._translateYAnimation.To = point.Y;

            return RunStoryboardAsync(this._translationStoryboard);
        }

        public void StopAnimation()
        {
            this._scaleStoryboard?.Stop();
            this._rotationStoryboard?.Stop();
            this._translationStoryboard?.Stop();
        }

        private EasingFunctionBase GetXamlEasingFunction(
            EasingFunction ease)
        {
            EasingFunctionBase xamlEase = null;

            if (null != ease)
            {
                var cubicEase = ease as CubicEase;
                var elasticEase = ease as SpringEase;

                if (null != cubicEase)
                {
                    xamlEase = new global::Windows.UI.Xaml.Media.Animation.CubicEase()
                    {
                        EasingMode = ease.EasingMode.ToXamlEasingMode(),
                    };
                }
                else if (null != elasticEase)
                {
                    xamlEase = new global::Windows.UI.Xaml.Media.Animation.ElasticEase()
                    {
                        EasingMode = ease.EasingMode.ToXamlEasingMode(),
                        Oscillations = elasticEase.Oscillations,
                    };
                }
                else
                {
                    throw new NotSupportedException("Unsupported easing function.");
                }
            }

            return xamlEase;
        }

        private static Task RunStoryboardAsync(
            Storyboard storyboard)
        {
            if (null == storyboard)
            {
                throw new ArgumentNullException(nameof(storyboard));
            }

            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();

            EventHandler<object> onComplete = null;
            onComplete = (s, e) =>
            {
                storyboard.Completed -= onComplete;
                tcs.SetResult(e);
            };
            storyboard.Completed += onComplete;
            storyboard.Begin();

            return tcs.Task;
        }

        protected override void OnNativeElementSet(
            DependencyObject oldElement,
            DependencyObject newElement)
        {
            base.OnNativeElementSet(oldElement, newElement);

            this.OnFrameworkElementSet(oldElement as FrameworkElement, newElement as FrameworkElement);
        }

        protected virtual void OnFrameworkElementSet(
            FrameworkElement oldElement,
            FrameworkElement newElement)
        {
            if (null == newElement)
            {
                throw new ArgumentException("Expected type FrameworkElement.", nameof(newElement));
            }

            this._frameworkElement = newElement;

            this._compositeTransform = new CompositeTransform();
            newElement.RenderTransform = this._compositeTransform;

            this.Anchor = Origin.Center;

            if (null != oldElement)
            {
                oldElement.PointerPressed -= Element_PointerPressed;
                oldElement.PointerMoved -= Element_PointerMoved;
                oldElement.PointerReleased -= Element_PointerReleased;
                oldElement.PointerCanceled -= Element_PointerCanceled;
            }

            if (null != newElement)
            {
                newElement.PointerPressed += Element_PointerPressed;
                newElement.PointerMoved += Element_PointerMoved;
                newElement.PointerReleased += Element_PointerReleased;
                newElement.PointerCanceled += Element_PointerCanceled;
            }
        }

        #region Input handling

        public bool IsInputVisible
        {
            get
            {
                bool isVisible;

                var frameworkElement = this.FrameworkElement;
                if (null != frameworkElement)
                {
                    isVisible = frameworkElement.IsHitTestVisible;
                }
                else
                {
                    isVisible = false;
                }

                return isVisible;
            }

            set
            {
                var frameworkElement = this.FrameworkElement;
                if (null != frameworkElement)
                {
                    frameworkElement.IsHitTestVisible = value;
                }
            }
        }

        private void Element_PointerPressed(
            object sender,
            PointerRoutedEventArgs e)
        {
            bool result = false;
            var control = this.ViewDelegate;
            if (null != control)
            {
                FrameworkElement nativeElement = this.FrameworkElement;
                var position = e.GetCurrentPoint(nativeElement).Position;
                Point point = new Point((float)position.X, (float)position.Y);
                result = control.HandlePointerEvent(new PointerInputEvent(point, PointerInputState.Began, e.Pointer.IsInContact));
                if (result)
                {
                    this.CapturePointer(e.Pointer);
                }
            }
            e.Handled = result;
        }

        private void Element_PointerMoved(
            object sender,
            PointerRoutedEventArgs e)
        {
            bool result = false;
            var control = this.ViewDelegate;
            if (null != control)
            {
                FrameworkElement nativeElement = this.FrameworkElement;
                var position = e.GetCurrentPoint(nativeElement).Position;
                Point point = new Point((float)position.X, (float)position.Y);
                result = control.HandlePointerEvent(new PointerInputEvent(point, PointerInputState.Moved, e.Pointer.IsInContact));
            }
            e.Handled = result;
        }

        private void Element_PointerReleased(
            object sender,
            PointerRoutedEventArgs e)
        {
            bool result = false;
            var control = this.ViewDelegate;
            if (null != control)
            {
                FrameworkElement nativeElement = this.FrameworkElement;
                var position = e.GetCurrentPoint(nativeElement).Position;
                Point point = new Point((float)position.X, (float)position.Y);
                result = control.HandlePointerEvent(new PointerInputEvent(point, PointerInputState.Ended, e.Pointer.IsInContact));

                this.ReleasePointer(e.Pointer);
            }
            e.Handled = result;
        }

        private void Element_PointerCanceled(
            object sender,
            PointerRoutedEventArgs e)
        {
            bool result = false;
            var control = this.ViewDelegate;
            if (null != control)
            {
                FrameworkElement nativeElement = this.FrameworkElement;
                var position = e.GetCurrentPoint(nativeElement).Position;
                Point point = new Point((float)position.X, (float)position.Y);
                result = control.HandlePointerEvent(new PointerInputEvent(point, PointerInputState.Canceled, e.Pointer.IsInContact));

                this.ReleasePointer(e.Pointer);
            }
            e.Handled = result;
        }

        private void CapturePointer(
            Pointer pointer)
        {
            if (null == pointer)
            {
                throw new ArgumentNullException(nameof(pointer));
            }

            FrameworkElement nativeElement = this.FrameworkElement;
            if (null != nativeElement)
            {
                this._isPointerCaptured = nativeElement.CapturePointer(pointer);
            }
        }

        private void ReleasePointer(
            Pointer pointer)
        {
            if (null == pointer)
            {
                throw new ArgumentNullException(nameof(pointer));
            }

            if (this._isPointerCaptured)
            {
                FrameworkElement nativeElement = this.FrameworkElement;
                if (null != nativeElement)
                {
                    nativeElement.ReleasePointerCapture(pointer);
                    this._isPointerCaptured = false;
                }
            }
        }

        #endregion Input handling
    }
}
