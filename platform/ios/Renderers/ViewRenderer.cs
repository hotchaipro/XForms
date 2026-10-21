using System;
using CoreGraphics;
using UIKit;
using XForms.Platform;

namespace XForms.iOS.Renderers
{
    public abstract class ViewRenderer<TNativeElement> : ElementRenderer<TNativeElement>, IViewRenderer
        where TNativeElement : UIView
    {
        private UIView _nativeView;
        private AnimationLayer[] _layers;
        private UIView _rootView;

        public ViewRenderer(
            View view)
            : base(view)
        {
        }

        private View ViewDelegate
        {
            get
            {
                return (View)this.Element;
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
                var nativeView = this._rootView ?? this._nativeView;
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

            // A wrapping animation layer is placed at the frame; the views
            // inside it fill it
            Place(this._rootView ?? nativeView, frame);
            if (null != this._rootView)
            {
                var inner = new Rect(0, 0, frame.Width, frame.Height);
                for (int i = 1; i < this._layers.Length; i += 1)
                {
                    Place(this._layers[i].View, inner);
                }
            }
        }

        private static void Place(
            UIView view,
            Rect frame)
        {
            // NOTE: Set the bounds and center rather than the frame, which is
            // undefined while the view has a transform (scale, rotation)
            var size = new CGSize(frame.Width, frame.Height);
            var bounds = view.Bounds;
            if (bounds.Size != size)
            {
                view.Bounds = new CGRect(bounds.Location, size);
            }

            var anchor = view.Layer.AnchorPoint;
            view.Center = new CGPoint(
                frame.X + (anchor.X * frame.Width),
                frame.Y + (anchor.Y * frame.Height));
        }

        public virtual void InvalidateMeasure()
        {
            this._nativeView?.RequestLayout();
        }

        public bool ClipToBounds
        {
            set
            {
                if (null != this._nativeView)
                {
                    this._nativeView.ClipsToBounds = value;
                }
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

        public IAnimationLayer[] CreateAnimationLayers(
            int count)
        {
            var nativeView = this._nativeView;
            if ((null != this._layers) || (null == nativeView))
            {
                throw new InvalidOperationException("The view's animation layers can't be created.");
            }

            // The innermost layer is the view itself; outer ones are views that
            // wrap it, each filling the one outside it
            var layers = new AnimationLayer[count];
            layers[count - 1] = new AnimationLayer(nativeView);

            UIView inner = nativeView;
            for (int i = count - 2; i >= 0; i -= 1)
            {
                var wrapper = new UIView();
                wrapper.AddSubview(inner);
                layers[i] = new AnimationLayer(wrapper);
                inner = wrapper;
            }

            if (inner != nativeView)
            {
                this._rootView = inner;
            }

            this._layers = layers;
            return layers;
        }

        protected override object RootNativeElement
        {
            get
            {
                return (object)this._rootView ?? this.NativeElement;
            }
        }

        // Views hosted for lightweight descendants, after the view's own subviews
        private int _hostedCount;

        public void AddHostedView(
            IViewRenderer child,
            int index)
        {
            var subviews = this._nativeView.Subviews;
            this._nativeView.InsertSubview((UIView)child.NativeElement, subviews.Length - this._hostedCount + index);
            this._hostedCount += 1;
        }

        public void RemoveHostedView(
            IViewRenderer child)
        {
            ((UIView)child.NativeElement).RemoveFromSuperview();
            this._hostedCount -= 1;
        }

        public void InvalidateLayerContent()
        {
            this._nativeView?.SetNeedsDisplay();
        }

        public bool Focus()
        {
            // NOTE: Only native views that can become first responder take focus
            // (owner-drawn and layout views do when their view is focusable)
            return this._nativeView?.BecomeFirstResponder() ?? false;
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
