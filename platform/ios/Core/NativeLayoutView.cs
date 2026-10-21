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
        private readonly NativePointerInput _pointerInput = new NativePointerInput();
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

            // Each finger is its own pointer
            this.MultipleTouchEnabled = true;
        }

        /// <summary>
        /// Draws the lightweight views inside this view, beneath its subviews.
        /// </summary>
        public override void Draw(
            CoreGraphics.CGRect area)
        {
            if (this._owner.HasLightweightChildren)
            {
                var drawContext = new DrawContext(UIGraphics.GetCurrentContext(), this.Bounds);
                LightweightPainter.PaintDescendants(this._owner, drawContext);
            }
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

        #region Keyboard

        public override bool CanBecomeFirstResponder
        {
            get
            {
                return this._owner.Focusable;
            }
        }

        public override bool BecomeFirstResponder()
        {
            bool result = base.BecomeFirstResponder();
            if (result)
            {
                this._owner.IsFocused = true;
            }

            return result;
        }

        public override bool ResignFirstResponder()
        {
            bool result = base.ResignFirstResponder();
            if (result)
            {
                this._owner.IsFocused = false;
            }

            return result;
        }

        public override void PressesBegan(
            NSSet<UIPress> presses,
            UIPressesEvent evt)
        {
            if (!this.RoutePresses(presses, isDown: true))
            {
                base.PressesBegan(presses, evt);
            }
        }

        public override void PressesEnded(
            NSSet<UIPress> presses,
            UIPressesEvent evt)
        {
            if (!this.RoutePresses(presses, isDown: false))
            {
                base.PressesEnded(presses, evt);
            }
        }

        // Hardware keys go to the focused view, then its ancestors
        private bool RoutePresses(
            NSSet<UIPress> presses,
            bool isDown)
        {
            bool handled = false;
            foreach (var press in presses)
            {
                var uiKey = press.Key;
                if (null == uiKey)
                {
                    continue;
                }

                var key = ToKey(uiKey.KeyCode);
                if (key == Key.None)
                {
                    continue;
                }

                var keyEvent = new KeyEventArgs(key, ToKeyModifiers(uiKey.ModifierFlags));
                handled |= isDown ? this._owner.RouteKeyDown(keyEvent) : this._owner.RouteKeyUp(keyEvent);
            }

            return handled;
        }

        private static KeyModifiers ToKeyModifiers(
            UIKeyModifierFlags flags)
        {
            var modifiers = KeyModifiers.None;
            if (flags.HasFlag(UIKeyModifierFlags.Shift)) modifiers |= KeyModifiers.Shift;
            if (flags.HasFlag(UIKeyModifierFlags.Control)) modifiers |= KeyModifiers.Control;
            if (flags.HasFlag(UIKeyModifierFlags.Alternate)) modifiers |= KeyModifiers.Alt;
            if (flags.HasFlag(UIKeyModifierFlags.Command)) modifiers |= KeyModifiers.Meta;
            return modifiers;
        }

        private static Key ToKey(
            UIKeyboardHidUsage usage)
        {
            switch (usage)
            {
                case UIKeyboardHidUsage.KeyboardTab: return Key.Tab;
                case UIKeyboardHidUsage.KeyboardReturnOrEnter:
                case UIKeyboardHidUsage.KeypadEnter: return Key.Enter;
                case UIKeyboardHidUsage.KeyboardSpacebar: return Key.Space;
                case UIKeyboardHidUsage.KeyboardEscape: return Key.Escape;
                case UIKeyboardHidUsage.KeyboardLeftArrow: return Key.Left;
                case UIKeyboardHidUsage.KeyboardUpArrow: return Key.Up;
                case UIKeyboardHidUsage.KeyboardRightArrow: return Key.Right;
                case UIKeyboardHidUsage.KeyboardDownArrow: return Key.Down;
                case UIKeyboardHidUsage.KeyboardHome: return Key.Home;
                case UIKeyboardHidUsage.KeyboardEnd: return Key.End;
                case UIKeyboardHidUsage.KeyboardPageUp: return Key.PageUp;
                case UIKeyboardHidUsage.KeyboardPageDown: return Key.PageDown;
                case UIKeyboardHidUsage.KeyboardDeleteOrBackspace: return Key.Back;
                case UIKeyboardHidUsage.KeyboardDeleteForward: return Key.Delete;
                case UIKeyboardHidUsage.KeyboardInsert: return Key.Insert;
                case UIKeyboardHidUsage.KeyboardF1: return Key.F1;
                case UIKeyboardHidUsage.KeyboardF2: return Key.F2;
                case UIKeyboardHidUsage.KeyboardF3: return Key.F3;
                case UIKeyboardHidUsage.KeyboardF4: return Key.F4;
                case UIKeyboardHidUsage.KeyboardF5: return Key.F5;
                case UIKeyboardHidUsage.KeyboardF6: return Key.F6;
                case UIKeyboardHidUsage.KeyboardF7: return Key.F7;
                case UIKeyboardHidUsage.KeyboardF8: return Key.F8;
                case UIKeyboardHidUsage.KeyboardF9: return Key.F9;
                case UIKeyboardHidUsage.KeyboardF10: return Key.F10;
                case UIKeyboardHidUsage.KeyboardF11: return Key.F11;
                case UIKeyboardHidUsage.KeyboardF12: return Key.F12;
                case UIKeyboardHidUsage.Keyboard0: return Key.D0;
                default:
                    if ((usage >= UIKeyboardHidUsage.KeyboardA) && (usage <= UIKeyboardHidUsage.KeyboardZ))
                    {
                        return Key.A + (int)(usage - UIKeyboardHidUsage.KeyboardA);
                    }

                    if ((usage >= UIKeyboardHidUsage.Keyboard1) && (usage <= UIKeyboardHidUsage.Keyboard9))
                    {
                        return Key.D1 + (int)(usage - UIKeyboardHidUsage.Keyboard1);
                    }

                    return Key.None;
            }
        }

        #endregion Keyboard

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
