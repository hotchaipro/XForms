using System;
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

            this._viewController = new UIViewController();
            this._viewController.View = view;

            this.SetNativeElement(view);
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

            private static NativeView FindFirstResponder(
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
