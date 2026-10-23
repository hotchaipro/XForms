using System;
using CoreGraphics;
using Foundation;
using UIKit;
using XForms.Platform;

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

            // A pointer (trackpad, mouse or Pencil) hovering over the view
            if (UIDevice.CurrentDevice.CheckSystemVersion(13, 0))
            {
                this.AddGestureRecognizer(new UIHoverGestureRecognizer(this.OnHover));
            }
        }

        private void OnHover(
            UIHoverGestureRecognizer recognizer)
        {
            var application = Application.Current;
            if (null == application)
            {
                return;
            }

            var location = recognizer.LocationInView(this);
            var point = new Point((float)location.X, (float)location.Y);
            switch (recognizer.State)
            {
                case UIGestureRecognizerState.Began:
                case UIGestureRecognizerState.Changed:
                    {
                        var deepest = LayerHost.For(this._owner).HitTestPointerOver(point);
                        var offset = (deepest == this._owner) ? default : LayerHost.For(deepest).OffsetInLayer;
                        ApplicationHost.For(application).PointerOver(deepest, new PointerEventArgs(0, PointerDeviceType.Mouse, new Point(point.X - offset.X, point.Y - offset.Y), PointerInputState.Moved, false, KeyModifiers.None));
                    }

                    break;

                case UIGestureRecognizerState.Ended:
                case UIGestureRecognizerState.Cancelled:
                    {
                        // Leaving this view for a subview or a neighbor enters
                        // that one next; only a pointer that went nowhere left
                        int version = ApplicationHost.For(application).PointerOverVersion;
                        var exit = new PointerEventArgs(0, PointerDeviceType.Mouse, point, PointerInputState.Moved, false, KeyModifiers.None);
                        CoreFoundation.DispatchQueue.MainQueue.DispatchAsync(() =>
                        {
                            if (ApplicationHost.For(application).PointerOverVersion == version)
                            {
                                ApplicationHost.For(application).PointerOver(null, exit);
                            }
                        });
                    }

                    break;
            }
        }

        internal View Owner
        {
            get
            {
                return this._owner;
            }
        }

        /// <summary>
        /// Draws the lightweight views inside this view, beneath its subviews.
        /// </summary>
        public override void Draw(
            CoreGraphics.CGRect area)
        {
            if (LayerHost.For(this._owner).HasLightweightChildren)
            {
                var drawContext = new DrawContext(UIGraphics.GetCurrentContext(), this.Bounds);
                LayerHost.For(this._owner).PaintLightweightViews(drawContext);
                LayerHost.For(this._owner).PaintFocusRing(drawContext);
            }

            // A focused view with a layer of its own has its ring inside it,
            // as a lightweight view's is around it
            if (this._owner.IsFocused)
            {
                var bounds = this.Bounds;
                var ring = UIBezierPath.FromRoundedRect(new CGRect(bounds.X + 1, bounds.Y + 1, bounds.Width - 2, bounds.Height - 2), 4);
                ring.LineWidth = 2;
                this._owner.ActualTheme.Colors.Foreground.ToUIColor().SetStroke();
                ring.Stroke();
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
        internal View PointerInputDelegate
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

            // The window's safe area, for pages that lay out around it
            var window = this.Window;
            if ((null != window) && (ApplicationHost.Current is ApplicationHost application))
            {
                var insets = window.SafeAreaInsets;
                application.SafeAreaInsetsChanged(new Thickness((float)insets.Left, (float)insets.Top, (float)insets.Right, (float)insets.Bottom));
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

        #region Accessibility

        // An accessible view is one element (a list item is named by the text
        // inside it); otherwise the view holds its lightweight views'
        // elements and its subviews
        public override bool IsAccessibilityElement
        {
            get
            {
                var role = this._owner.AccessibilityRole;
                return (role != AccessibilityRole.None) && !NativeAccessibility.IsContainer(role) && !LayerHost.For(this._owner).IsHiddenFromAccessibility;
            }

            set
            {
            }
        }

        // Hidden from assistive technology (content a drawer covers, say), with
        // everything in it
        public override bool AccessibilityElementsHidden
        {
            get
            {
                return this._owner.AccessibilityIsHidden;
            }

            set
            {
            }
        }

        public override string AccessibilityLabel
        {
            get
            {
                return NativeAccessibility.LabelFor(this._owner);
            }

            set
            {
            }
        }

        public override string AccessibilityValue
        {
            get
            {
                return NativeAccessibility.ValueFor(this._owner);
            }

            set
            {
            }
        }

        public override UIAccessibilityTrait AccessibilityTraits
        {
            get
            {
                return NativeAccessibility.TraitsFor(this._owner);
            }

            set
            {
            }
        }

        [Export("accessibilityExpandedStatus")]
        public nint ExpandedStatus
        {
            get
            {
                return NativeAccessibility.ExpandedStatusFor(this._owner);
            }
        }

        // VoiceOver's escape gesture (a two-finger scrub) does what Escape
        // does: closes a menu or a dialog, or (unhandled, at the root) goes back
        public override bool AccessibilityPerformEscape()
        {
            // The view's ancestors get it here, so enclosing views don't again
            return LayerHost.For(this._owner).RouteKeyDown(new KeyEventArgs(Key.Escape, KeyModifiers.None)) || Renderers.FrameRenderer.GoBack();
        }

        public override bool AccessibilityActivate()
        {
            return LayerHost.For(this._owner).IsAccessibilityActionable && LayerHost.For(this._owner).PerformDefaultAccessibilityAction();
        }

        public override void AccessibilityIncrement()
        {
            NativeAccessibility.Adjust(this._owner, up: true);
        }

        public override void AccessibilityDecrement()
        {
            NativeAccessibility.Adjust(this._owner, up: false);
        }

        // NOTE: Null (nil) means the subviews
        [Export("accessibilityElements")]
        public NSObject[] GetAccessibilityElements()
        {
            var role = this._owner.AccessibilityRole;
            if (((role != AccessibilityRole.None) && !NativeAccessibility.IsContainer(role)) || !LayerHost.For(this._owner).HasLightweightChildren)
            {
                return null;
            }

            return NativeAccessibility.ElementsFor(this, this._owner);
        }
        #endregion Accessibility

        #region Keyboard

        public override bool CanBecomeFirstResponder
        {
            get
            {
                return this._owner.Focusable || LayerHost.For(this._owner).HasFocusableLightweight;
            }
        }

        public override bool BecomeFirstResponder()
        {
            bool result = base.BecomeFirstResponder();
            if (result)
            {
                LayerHost.For(this._owner).NativeFocusChanged(true, isBackward: false);
                this.SetNeedsDisplay();
            }

            return result;
        }

        public override bool ResignFirstResponder()
        {
            bool result = base.ResignFirstResponder();
            if (result)
            {
                LayerHost.For(this._owner).NativeFocusChanged(false, isBackward: false);
                this.SetNeedsDisplay();
                Renderers.FrameRenderer.KeyboardFocusLost(this.Window);
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
                if (isDown && (key == Key.Tab) && ((keyEvent.KeyModifiers & ~KeyModifiers.Shift) == KeyModifiers.None)
                    && Renderers.PopupRenderer.MoveFocusIntoModal(LayerHost.For(this._owner).FocusedLightweight ?? this._owner, !keyEvent.IsShiftDown))
                {
                    handled = true;
                    continue;
                }

                handled |= isDown ? LayerHost.For(this._owner).RouteKeyDown(keyEvent) : LayerHost.For(this._owner).RouteKeyUp(keyEvent);
            }

            return handled;
        }

        internal static KeyModifiers ToKeyModifiers(
            UIKeyModifierFlags flags)
        {
            var modifiers = KeyModifiers.None;
            if (flags.HasFlag(UIKeyModifierFlags.Shift)) modifiers |= KeyModifiers.Shift;
            if (flags.HasFlag(UIKeyModifierFlags.Control)) modifiers |= KeyModifiers.Control;
            if (flags.HasFlag(UIKeyModifierFlags.Alternate)) modifiers |= KeyModifiers.Alt;
            if (flags.HasFlag(UIKeyModifierFlags.Command)) modifiers |= KeyModifiers.Meta;
            return modifiers;
        }

        internal static Key ToKey(
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
                case UIKeyboardHidUsage.KeyboardApplication: return Key.Menu;
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
