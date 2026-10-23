using System;
using System.Collections.Generic;
using UIKit;
using XForms.Platform;
using NativeView = UIKit.UIView;

namespace XForms.iOS.Renderers
{
    internal class FrameRenderer : ViewRenderer<NativeView>, IFrameRenderer
    {
        private UIViewController _viewController;

        /// <summary>
        /// How much of the window's bottom the keyboard covers (0 while it's
        /// hidden), for popups to stay above it.
        /// </summary>
        internal static nfloat KeyboardOverlap
        {
            get;
            private set;
        }

        public FrameRenderer(
            Frame frame)
            : base(frame)
        {
            var view = new RootView(frame)
            {
                BackgroundColor = UIColor.SystemBackground,
            };

            this._viewController = new RootViewController(frame);
            this._viewController.View = view;

            this.SetNativeElement(view);
        }

        /// <summary>
        /// Goes back a page, if the app can. Returns whether it did.
        /// </summary>
        internal static bool GoBack()
        {
            var application = Application.Current;
            if ((null != application) && application.CanGoBack)
            {
                ApplicationHost.For(application).Back();
                return true;
            }

            return false;
        }

        /// <summary>
        /// The view with keyboard focus in the window, or null: the first
        /// responder's view (a layer's focused lightweight view, or a text box).
        /// </summary>
        internal static View FocusedView(
            UIWindow window)
        {
            for (var view = (null == window) ? null : RootView.FindFirstResponder(window); null != view; view = view.Superview)
            {
                if (view is NativeLayoutView layout)
                {
                    return LayerHost.For(layout.Owner).FocusedLightweight ?? layout.Owner;
                }

                if (view is TextBoxRenderer.NativeTextEntryView textEntry)
                {
                    return textEntry.Owner;
                }
            }

            return null;
        }

        /// <summary>
        /// A view gave up keyboard focus: if nothing took it, the root takes it,
        /// so a hardware keyboard's keys still reach the app.
        /// </summary>
        internal static void KeyboardFocusLost(
            UIWindow window)
        {
            CoreFoundation.DispatchQueue.MainQueue.DispatchAsync(() =>
            {
                if ((window?.RootViewController is RootViewController controller) && (null == RootView.FindFirstResponder(window)))
                {
                    controller.BecomeFirstResponder();
                }
            });
        }

        // Has keyboard focus when no view does, and handles the keys no view
        // used: Tab starts at the first (or last) focusable view, and Escape
        // and Command+[ go back, as on the desktop and in the browser
        private sealed class RootViewController : UIViewController
        {
            private readonly Frame _frame;
            private readonly HashSet<Key> _handledKeys = new HashSet<Key>();

            public RootViewController(
                Frame frame)
            {
                this._frame = frame;
            }

            public override bool CanBecomeFirstResponder
            {
                get
                {
                    return true;
                }
            }

            public override void ViewDidAppear(
                bool animated)
            {
                base.ViewDidAppear(animated);
                KeyboardFocusLost(this.View.Window);
            }

            public override void PressesBegan(
                Foundation.NSSet<UIPress> presses,
                UIPressesEvent evt)
            {
                bool handled = false;
                foreach (var press in presses)
                {
                    var uiKey = press.Key;
                    if (null == uiKey)
                    {
                        continue;
                    }

                    var key = NativeLayoutView.ToKey(uiKey.KeyCode);
                    var modifiers = NativeLayoutView.ToKeyModifiers(uiKey.ModifierFlags);
                    if (this.OnKeyDown(key, modifiers, uiKey.KeyCode))
                    {
                        this._handledKeys.Add(key);
                        handled = true;
                    }
                }

                if (!handled)
                {
                    base.PressesBegan(presses, evt);
                }
            }

            public override void PressesEnded(
                Foundation.NSSet<UIPress> presses,
                UIPressesEvent evt)
            {
                bool handled = false;
                foreach (var press in presses)
                {
                    var uiKey = press.Key;
                    if (null != uiKey)
                    {
                        handled |= this._handledKeys.Remove(NativeLayoutView.ToKey(uiKey.KeyCode));
                    }
                }

                if (!handled)
                {
                    base.PressesEnded(presses, evt);
                }
            }

            public override bool AccessibilityPerformEscape()
            {
                return GoBack();
            }

            private bool OnKeyDown(
                Key key,
                KeyModifiers modifiers,
                UIKeyboardHidUsage usage)
            {
                // Tab reaches the root only when nothing has focus: it starts
                // in the open popup, if there's one, or the page
                if ((key == Key.Tab) && ((modifiers & ~KeyModifiers.Shift) == KeyModifiers.None) && this.IsFirstResponder)
                {
                    var scope = PopupRenderer.TopModalContent ?? this._frame;
                    return LayerHost.For(scope).MoveFocus(isForward: !modifiers.HasFlag(KeyModifiers.Shift));
                }

                bool isBack = ((key == Key.Escape) && (modifiers == KeyModifiers.None))
                    || ((usage == UIKeyboardHidUsage.KeyboardOpenBracket) && (modifiers == KeyModifiers.Meta));
                return isBack && GoBack();
            }
        }

        // Reports its size: the root frame is the window's content, above the
        // keyboard while it shows (as Android resizes for it), with the focused
        // text entry scrolled into view
        private sealed class RootView : NativeView
        {
            private readonly Frame _frame;
            private nfloat _keyboardOverlap;

            public RootView(
                Frame frame)
            {
                this._frame = frame;
                UIKeyboard.Notifications.ObserveWillChangeFrame(this.Keyboard_WillChangeFrame);
                UIKeyboard.Notifications.ObserveWillHide((sender, e) => this.SetKeyboardOverlap(0));
                UIKeyboard.Notifications.ObserveDidShow((sender, e) => this.RevealFirstResponder());
            }

            private void Keyboard_WillChangeFrame(
                object sender,
                UIKeyboardEventArgs e)
            {
                var keyboard = this.ConvertRectFromView(e.FrameEnd, null);
                this.SetKeyboardOverlap((nfloat)Math.Max(0, this.Bounds.Bottom - keyboard.Top));
            }

            private void SetKeyboardOverlap(
                nfloat overlap)
            {
                if (overlap != this._keyboardOverlap)
                {
                    this._keyboardOverlap = overlap;
                    KeyboardOverlap = overlap;
                    this.SetNeedsLayout();
                }
            }

            // The focused text entry, scrolled into view in its scroll view
            private void RevealFirstResponder()
            {
                var responder = FindFirstResponder(this);
                for (var view = responder?.Superview; null != view; view = view.Superview)
                {
                    if (view is UIScrollView scrollView)
                    {
                        var rect = responder.ConvertRectToView(responder.Bounds, scrollView);
                        scrollView.ScrollRectToVisible(new CoreGraphics.CGRect(rect.X, rect.Y - 8, rect.Width, rect.Height + 16), true);
                        break;
                    }
                }
            }

            internal static NativeView FindFirstResponder(
                NativeView view)
            {
                if (view.IsFirstResponder)
                {
                    return view;
                }

                foreach (var subview in view.Subviews)
                {
                    var found = FindFirstResponder(subview);
                    if (null != found)
                    {
                        return found;
                    }
                }

                return null;
            }

            public override void LayoutSubviews()
            {
                base.LayoutSubviews();
                var height = (nfloat)Math.Max(0, this.Bounds.Height - this._keyboardOverlap);

                // The pages lay themselves out in their views: above the keyboard
                foreach (var page in this.Subviews)
                {
                    page.Frame = new CoreGraphics.CGRect(0, 0, this.Bounds.Width, height);
                }

                LayerHost.For(this._frame).NotifyRootSize(new Size((float)this.Bounds.Width, (float)height));
            }
        }

        internal UIViewController ViewController
        {
            get
            {
                return this._viewController;
            }
        }

        // Pages stay in the frame (hidden when not showing), so transitions can
        // show two at once
        public void AddPage(
            Page page)
        {
            var nativePage = (NativeView)page.Renderer.NativeElement;
            if (nativePage.Superview != this.NativeElement)
            {
                this.NativeElement.AddSubview(nativePage);
                nativePage.Frame = this.NativeElement.Bounds;
                nativePage.AutoresizingMask = UIViewAutoresizing.FlexibleDimensions;
            }
        }

        public void RemovePage(
            Page page)
        {
            ((NativeView)page.Renderer.NativeElement).RemoveFromSuperview();
        }

        public void BringToFront(
            Page page)
        {
            this.NativeElement.BringSubviewToFront((NativeView)page.Renderer.NativeElement);
        }
    }
}
