using System;
using System.Collections.Generic;
using CoreGraphics;
using UIKit;
using XForms.Primitives;
using XForms.Platform;

namespace XForms.iOS.Renderers
{
    /// <summary>
    /// Shows a popup on top of the window: centered, or below (or, without room,
    /// above) its placement target. A transparent layer behind the popup covers
    /// the rest of the window, and touching it dismisses the popup.
    /// </summary>
    internal sealed class PopupRenderer : ElementRenderer<UIView>, IPopupRenderer
    {
        private const float WindowMargin = 24.0f;
        private const float AnchorGap = 4.0f;
        private const float EdgeMargin = 8.0f;

        private readonly UIControl _dismissLayer;
        private View _content;
        private bool _isOpen;

        public PopupRenderer(
            Popup popup)
            : base(popup)
        {
            this._dismissLayer = new UIControl()
            {
                BackgroundColor = UIColor.Clear,
            };
            this._dismissLayer.TouchDown += this.DismissLayer_TouchDown;
        }

        public View Content
        {
            get
            {
                return this._content;
            }

            set
            {
                this._content = value;
            }
        }

        // The placement target's frame in the window: its native view's, or
        // for a lightweight target, its place in its layer's view
        private bool TryGetAnchorFrame(
            UIWindow window,
            out CoreGraphics.CGRect anchorFrame)
        {
            var target = this.PlacementTarget;
            if (target?.Renderer.NativeElement is UIView anchor)
            {
                anchorFrame = anchor.ConvertRectToView(anchor.Bounds, window);
                return true;
            }

            if ((null != target) && (LayerHost.For(target).Layer?.NativeElement is UIView layer))
            {
                var offset = LayerHost.For(target).OffsetInLayer;
                var local = new CoreGraphics.CGRect(offset.X, offset.Y, target.Bounds.Width, target.Bounds.Height);
                anchorFrame = layer.ConvertRectToView(local, window);
                return true;
            }

            anchorFrame = default;
            return false;
        }

        public View PlacementTarget
        {
            get;
            set;
        }

        public bool IsLightDismissEnabled
        {
            get;
            set;
        } = true;

        private static UIWindow Window
        {
            get
            {
                return IosApplication.Current?.Window;
            }
        }

        public void Show()
        {
            var window = Window;
            var contentView = this._content?.Renderer.NativeElement as UIView;
            if (this._isOpen || (null == window) || (null == contentView))
            {
                return;
            }

            this._isOpen = true;
            this._previousFocus = FrameRenderer.FocusedView(window);

            if (this.IsLightDismissEnabled)
            {
                this._dismissLayer.Frame = window.Bounds;
                window.AddSubview(this._dismissLayer);
            }

            window.AddSubview(contentView);

            this.Place(window);

            // A popup that takes the user's attention (a menu, a picker, a
            // dialog) keeps VoiceOver inside it while it's open
            if (this.IsLightDismissEnabled)
            {
                ModalPopups.Add(this);
                contentView.AccessibilityViewIsModal = true;
                UIAccessibility.PostNotification(UIAccessibilityPostNotification.ScreenChanged, contentView);
            }
        }

        private void Place(
            UIWindow window)
        {
            var content = this._content;
            // The safe area, above the keyboard while it shows
            var safeFrame = window.SafeAreaLayoutGuide.LayoutFrame;
            var keyboardTop = window.Bounds.Height - FrameRenderer.KeyboardOverlap;
            if (safeFrame.Bottom > keyboardTop)
            {
                safeFrame.Height = (nfloat)Math.Max(0, keyboardTop - safeFrame.Top);
            }

            var windowSize = new Size((float)safeFrame.Width, (float)safeFrame.Height);

            Rect frame;
            if (this.TryGetAnchorFrame(window, out var anchorFrame))
            {
                float top = (float)(anchorFrame.Top - safeFrame.Top);
                float bottom = (float)(anchorFrame.Bottom - safeFrame.Top);

                float below = Math.Max(0, windowSize.Height - (bottom + AnchorGap) - EdgeMargin);
                float above = Math.Max(0, top - AnchorGap - EdgeMargin);
                float availableWidth = Math.Max(0, windowSize.Width - (2 * EdgeMargin));

                // Below, if its natural height fits there (or there's more room
                // there)
                content.Measure(new Size(availableWidth, float.PositiveInfinity));
                var size = content.DesiredSize;

                float y;
                if ((size.Height <= below) || (below >= above))
                {
                    content.Measure(new Size(availableWidth, below));
                    size = content.DesiredSize;
                    y = bottom + AnchorGap;
                }
                else
                {
                    content.Measure(new Size(availableWidth, above));
                    size = content.DesiredSize;
                    y = top - AnchorGap - size.Height;
                }

                float left = (float)(anchorFrame.Left - safeFrame.Left);
                float x = Math.Max(EdgeMargin, Math.Min(left, windowSize.Width - size.Width - EdgeMargin));
                frame = new Rect(x, y, size.Width, size.Height);
            }
            else
            {
                content.Measure(new Size(
                    Math.Max(0, windowSize.Width - (2 * WindowMargin)),
                    Math.Max(0, windowSize.Height - (2 * WindowMargin))));

                var size = content.DesiredSize;
                frame = new Rect((windowSize.Width - size.Width) / 2, (windowSize.Height - size.Height) / 2, size.Width, size.Height);
            }

            // NOTE: The content is positioned in window coordinates
            content.Arrange(new Rect(
                (float)Math.Round(frame.X + safeFrame.Left),
                (float)Math.Round(frame.Y + safeFrame.Top),
                frame.Width,
                frame.Height));
        }

        public void Close()
        {
            if (!this._isOpen)
            {
                return;
            }

            this._isOpen = false;
            ModalPopups.Remove(this);
            var window = Window;
            var focused = FrameRenderer.FocusedView(window);
            var contentView = this._content?.Renderer.NativeElement as UIView;
            contentView?.RemoveFromSuperview();
            this._dismissLayer.RemoveFromSuperview();
            if ((null != contentView) && contentView.AccessibilityViewIsModal)
            {
                contentView.AccessibilityViewIsModal = false;
                UIAccessibility.PostNotification(UIAccessibilityPostNotification.ScreenChanged, null);
            }

            // Keyboard focus that was in the popup (or is nowhere) goes back to
            // where it was before it opened, as on the desktop
            var previous = this._previousFocus;
            this._previousFocus = null;
            if ((null != previous) && ((null == focused) || this.Contains(focused)))
            {
                previous.Focus();
            }

            ((IPopupDelegate)this.Element).NotifyClosed();
        }

        private View _previousFocus;

        // The open popups that take the user's attention, the top one last
        private static readonly List<PopupRenderer> ModalPopups = new List<PopupRenderer>();

        /// <summary>
        /// The content of the top open popup that takes the user's attention
        /// (a menu, a picker, a dialog), or null: keyboard focus stays inside
        /// it, as on the desktop.
        /// </summary>
        internal static View TopModalContent
        {
            get
            {
                return (0 < ModalPopups.Count) ? ModalPopups[ModalPopups.Count - 1]._content : null;
            }
        }

        /// <summary>
        /// Tab from a view outside the top open popup (focus left behind on the
        /// page) moves into the popup. Returns whether it did.
        /// </summary>
        internal static bool MoveFocusIntoModal(
            View from,
            bool isForward)
        {
            var top = ModalPopups.Count > 0 ? ModalPopups[ModalPopups.Count - 1] : null;
            if ((null == top) || (null == top._content) || top.Contains(from))
            {
                return false;
            }

            return LayerHost.For(top._content).MoveFocus(isForward);
        }

        private bool Contains(
            View view)
        {
            for (; null != view; view = view.Parent as View)
            {
                if (view == this._content)
                {
                    return true;
                }
            }

            return false;
        }

        private void DismissLayer_TouchDown(
            object sender,
            EventArgs e)
        {
            this.Close();
        }
    }
}
