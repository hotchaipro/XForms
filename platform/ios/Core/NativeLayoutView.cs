using System;
using CoreGraphics;
using Foundation;
using UIKit;

namespace XForms.iOS
{
    /// <summary>
    /// Native container for any XForms view that lays out children of its own.
    /// UIKit has no measure pass, so this view caches its last measurement until
    /// something in its subtree asks for layout, and lays its children out from
    /// <see cref="LayoutSubviews"/> using the shared layout engine in Core.
    /// </summary>
    internal class NativeLayoutView : UIView
    {
        private readonly View _owner;
        private NativePointerInput _pointerInput;
        private bool _isMeasureValid;
        private Size _measuredAvailableSize;
        private Size _measuredContentSize;
        private bool _isLayoutValid;
        private Size _laidOutSize;

        public NativeLayoutView(
            View owner)
        {
            if (null == owner)
            {
                throw new ArgumentNullException(nameof(owner));
            }

            this._owner = owner;
        }

        /// <summary>
        /// Keeps the content clear of the status bar, notch and home indicator.
        /// </summary>
        public bool RespectsSafeArea
        {
            get;
            set;
        }

        /// <summary>
        /// The view that receives pointer input from touches that land on this
        /// view or on any descendant that does not handle them.
        /// </summary>
        internal IViewDelegate PointerInputDelegate
        {
            get;
            set;
        }

        /// <summary>
        /// Measures the children for the available size, reusing the last result
        /// when nothing in the subtree has changed.
        /// </summary>
        public Size MeasureContent(
            Size availableSize)
        {
            if ((!this._isMeasureValid) || (availableSize != this._measuredAvailableSize))
            {
                this._measuredContentSize = this._owner.MeasureContent(availableSize);
                this._measuredAvailableSize = availableSize;
                this._isMeasureValid = true;
            }

            return this._measuredContentSize;
        }

        /// <summary>
        /// Drops the cached measurement and layout, and schedules a layout pass.
        /// </summary>
        public void InvalidateContentLayout()
        {
            this._isMeasureValid = false;
            this._isLayoutValid = false;
            this.SetNeedsLayout();
        }

        public override void SafeAreaInsetsDidChange()
        {
            base.SafeAreaInsetsDidChange();

            if (this.RespectsSafeArea)
            {
                this.InvalidateContentLayout();
            }
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();

            var insets = this.RespectsSafeArea ? this.SafeAreaInsets : UIEdgeInsets.Zero;
            var bounds = this.Bounds;

            var contentSize = new Size(
                (float)Math.Max(0, bounds.Width - insets.Left - insets.Right),
                (float)Math.Max(0, bounds.Height - insets.Top - insets.Bottom));

            if ((this._isLayoutValid) && (contentSize == this._laidOutSize))
            {
                return;
            }

            // Shift the coordinate system so children laid out at (0, 0) start
            // inside the safe area
            var origin = new CGPoint(-insets.Left, -insets.Top);
            if (bounds.Location != origin)
            {
                this.Bounds = new CGRect(origin, bounds.Size);
            }

            if ((!this._isMeasureValid) || (contentSize != this._measuredAvailableSize))
            {
                this.MeasureContent(contentSize);
            }

            this._owner.LayoutContent(contentSize);

            this._laidOutSize = contentSize;
            this._isLayoutValid = true;
        }

        public override void TouchesBegan(
            NSSet touches,
            UIEvent evt)
        {
            if (!this._pointerInput.TouchesBegan(this, this.PointerInputDelegate, touches))
            {
                base.TouchesBegan(touches, evt);
            }
        }

        public override void TouchesMoved(
            NSSet touches,
            UIEvent evt)
        {
            if (!this._pointerInput.TouchesMoved(this, this.PointerInputDelegate, touches))
            {
                base.TouchesMoved(touches, evt);
            }
        }

        public override void TouchesEnded(
            NSSet touches,
            UIEvent evt)
        {
            if (!this._pointerInput.TouchesEnded(this, this.PointerInputDelegate, touches))
            {
                base.TouchesEnded(touches, evt);
            }
        }

        public override void TouchesCancelled(
            NSSet touches,
            UIEvent evt)
        {
            if (!this._pointerInput.TouchesCancelled(this, this.PointerInputDelegate, touches))
            {
                base.TouchesCancelled(touches, evt);
            }
        }
    }
}
