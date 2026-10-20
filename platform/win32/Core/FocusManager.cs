using System;
using System.Collections.Generic;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Direct2D;
using Windows.Win32.Graphics.DirectComposition;
using Windows.Win32.Graphics.Dxgi.Common;
using Windows.Win32.System.Com;
using Windows.Win32.UI.Input.KeyboardAndMouse;
using XForms.Input;
using XForms.Win32.Renderers;

namespace XForms.Win32
{
    /// <summary>
    /// Keyboard focus across owner-drawn controls (anything with a tap gesture)
    /// and native text entries: Tab and Shift+Tab move in view order, Enter and
    /// Space activate, Escape and Alt+Left go back. A focus ring shows only while
    /// the keyboard is being used, as Windows does.
    /// </summary>
    internal sealed unsafe class FocusManager
    {
        private const float RingThickness = 2.0f;
        private const float RingOutset = 3.0f;

        private readonly Win32Host _host;
        private ViewRenderer _focused;
        private IDCompositionVisual3* _ringVisual;
        private IDCompositionSurface* _ringSurface;
        private int _ringWidth, _ringHeight;
        private bool _isRingVisible;
        private IDCompositionVisual2* _ringParent;

        public FocusManager(
            Win32Host host)
        {
            this._host = host;
        }

        public ViewRenderer Focused
        {
            get
            {
                return this._focused;
            }
        }

        /// <summary>
        /// Whether the focus ring is showing (keyboard focus is being used).
        /// </summary>
        public bool IsRingVisible
        {
            get
            {
                return this._isRingVisible;
            }
        }

        public static bool IsFocusable(
            ViewRenderer renderer)
        {
            if (renderer is TextBoxRenderer)
            {
                return true;
            }

            return (renderer.Element is View view) && view.Focusable && renderer.IsHitTestVisible;
        }

        private void CollectFocusable(
            ViewRenderer renderer,
            List<ViewRenderer> focusable)
        {
            if (!renderer.IsVisible)
            {
                return;
            }

            if (IsFocusable(renderer))
            {
                focusable.Add(renderer);
            }

            foreach (var child in renderer.Children)
            {
                this.CollectFocusable(child, focusable);
            }
        }

        private static Key ToKey(
            VIRTUAL_KEY key)
        {
            switch (key)
            {
                case VIRTUAL_KEY.VK_TAB: return Key.Tab;
                case VIRTUAL_KEY.VK_RETURN: return Key.Enter;
                case VIRTUAL_KEY.VK_SPACE: return Key.Space;
                case VIRTUAL_KEY.VK_ESCAPE: return Key.Escape;
                case VIRTUAL_KEY.VK_LEFT: return Key.Left;
                case VIRTUAL_KEY.VK_UP: return Key.Up;
                case VIRTUAL_KEY.VK_RIGHT: return Key.Right;
                case VIRTUAL_KEY.VK_DOWN: return Key.Down;
                case VIRTUAL_KEY.VK_HOME: return Key.Home;
                case VIRTUAL_KEY.VK_END: return Key.End;
                case VIRTUAL_KEY.VK_PRIOR: return Key.PageUp;
                case VIRTUAL_KEY.VK_NEXT: return Key.PageDown;
                case VIRTUAL_KEY.VK_F4: return Key.F4;
                default: return Key.None;
            }
        }

        /// <summary>
        /// Offers the key to the focused view, then to its ancestors.
        /// </summary>
        private bool DispatchKey(
            KeyEventArgs keyInputEvent)
        {
            for (var renderer = this._focused; null != renderer; renderer = renderer.ParentRenderer)
            {
                if ((renderer.Element is View view) && view.OnKeyDown(keyInputEvent))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Handles a key before it reaches any window. Returns true when the key
        /// was used by a view, or for focus, activation or navigation.
        /// </summary>
        public bool PreviewKey(
            uint message,
            VIRTUAL_KEY virtualKey)
        {
            bool isShiftDown = (PInvoke.GetKeyState((int)VIRTUAL_KEY.VK_SHIFT) & 0x8000) != 0;
            bool isAltDown = (message == PInvoke.WM_SYSKEYDOWN);

            if ((message != PInvoke.WM_KEYDOWN) && (message != PInvoke.WM_SYSKEYDOWN))
            {
                return false;
            }

            // Alt+Left goes back
            if (isAltDown && (virtualKey == VIRTUAL_KEY.VK_LEFT))
            {
                return this.NavigateBack();
            }

            var key = ToKey(virtualKey);
            if (key == Key.None)
            {
                return false;
            }

            // A native text entry handles its own keys (except Tab and Escape)
            bool isTextBox = this._focused is TextBoxRenderer;

            if (!isTextBox && this.DispatchKey(new KeyEventArgs(key, isShiftDown, isAltDown)))
            {
                return true;
            }

            if (isAltDown)
            {
                return false;
            }

            switch (key)
            {
                case Key.Tab:
                    this.MoveFocus(isShiftDown ? -1 : 1);
                    return true;

                case Key.Enter:
                case Key.Space:
                    if ((null != this._focused) && !isTextBox)
                    {
                        this.Activate(this._focused);
                        return true;
                    }
                    break;

                case Key.Escape:
                    var top = this._host.Overlays.Top;
                    if (null != top)
                    {
                        top.Dismiss();
                        return true;
                    }

                    return this.NavigateBack();
            }

            return false;
        }

        private bool NavigateBack()
        {
            var application = Application.Current;
            if ((null != application) && application.CanNavigateBack)
            {
                application.NavigateBack();
                return true;
            }

            return false;
        }

        public void MoveFocus(
            int direction)
        {
            // Focus stays inside an open popup
            var root = this._host.Overlays.Top?.Renderer ?? this._host.Root;
            if (null == root)
            {
                return;
            }

            var focusable = new List<ViewRenderer>();
            this.CollectFocusable(root, focusable);
            if (focusable.Count == 0)
            {
                return;
            }

            int index = focusable.IndexOf(this._focused);
            if (index < 0)
            {
                index = (direction > 0) ? 0 : focusable.Count - 1;
            }
            else
            {
                index = (index + direction + focusable.Count) % focusable.Count;
            }

            this.SetFocus(focusable[index], showRing: true);
        }

        /// <summary>
        /// Records the focused view and keeps the views' IsFocused current.
        /// </summary>
        private void SetFocused(
            ViewRenderer renderer)
        {
            var previous = this._focused;
            if (previous == renderer)
            {
                return;
            }

            this._focused = renderer;

            if (previous?.Element is View previousView)
            {
                previousView.IsFocused = false;
            }

            if (renderer?.Element is View view)
            {
                view.IsFocused = true;
            }
        }

        public void SetFocus(
            ViewRenderer renderer,
            bool showRing)
        {
            this.SetFocused(renderer);

            if (renderer is TextBoxRenderer textEntry)
            {
                // The native control draws its own caret and focus state
                textEntry.Focus();
                this.HideRing();
                return;
            }

            // Owner-drawn controls take keyboard input through the main window
            PInvoke.SetFocus(this._host.Hwnd);

            if (showRing && (null != renderer))
            {
                this._isRingVisible = true;
                this.UpdateRing();
            }
            else
            {
                this.HideRing();
            }

            Automation.AutomationTree.RaiseFocusChanged(renderer);
        }

        /// <summary>
        /// Puts focus back where it was before a popup opened, if it was inside the popup.
        /// </summary>
        public void OnOverlayClosed(
            IOverlay overlay,
            ViewRenderer previousFocus)
        {
            bool isFocusInOverlay = false;
            for (var renderer = this._focused; null != renderer; renderer = renderer.ParentRenderer)
            {
                if (renderer == overlay.Renderer)
                {
                    isFocusInOverlay = true;
                    break;
                }
            }

            if (isFocusInOverlay || (null == this._focused))
            {
                if ((null != previousFocus) && previousFocus.IsShownOnScreen)
                {
                    this.SetFocus(previousFocus, this._isRingVisible);
                }
                else
                {
                    this.SetFocused(null);
                    this.HideRing();
                }
            }
        }

        /// <summary>
        /// Called when a native text entry takes focus by itself, such as on a click.
        /// </summary>
        public void OnTextBoxFocused(
            TextBoxRenderer textEntry)
        {
            this.SetFocused(textEntry);
            this.HideRing();
        }

        /// <summary>
        /// Pointer input hides the keyboard focus ring.
        /// </summary>
        public void OnPointerInput()
        {
            this.HideRing();
        }

        public void Activate(
            ViewRenderer renderer)
        {
            (renderer.Element as View)?.OnActivate();
        }

        private void HideRing()
        {
            if (this._isRingVisible)
            {
                this._isRingVisible = false;
                this._ringVisual->SetOpacity(0);
                this._host.RequestCommit();
            }
        }

        /// <summary>
        /// Keeps the ring around the focused control as it moves or scrolls.
        /// </summary>
        public void UpdateRing()
        {
            var focused = this._focused;
            if ((!this._isRingVisible) || (null == focused))
            {
                return;
            }

            if (!focused.IsShownOnScreen)
            {
                this.SetFocused(null);
                this.HideRing();
                return;
            }

            this.EnsureRingVisual();

            float scale = this._host.Scale;

            // Ring the part of the view that shows, such as a list inside a scroll view
            var visible = VisibleBounds(focused);
            var origin = new Point(visible.X, visible.Y);
            var frame = visible;

            int width = (int)Math.Ceiling((frame.Width + (2 * RingOutset)) * scale);
            int height = (int)Math.Ceiling((frame.Height + (2 * RingOutset)) * scale);

            if ((width != this._ringWidth) || (height != this._ringHeight))
            {
                this.DrawRing(width, height);
            }

            // The ring goes in the focused view's outermost view (the page, or a
            // popup), relative to that view
            var outermost = focused;
            while (null != outermost.ParentRenderer)
            {
                outermost = outermost.ParentRenderer;
            }

            var outermostFrame = outermost.Frame;

            this._ringVisual->SetOffsetX((float)Math.Round((origin.X - outermostFrame.X - RingOutset) * scale));
            this._ringVisual->SetOffsetY((float)Math.Round((origin.Y - outermostFrame.Y - RingOutset) * scale));
            this._ringVisual->SetOpacity(1);

            // Keep the ring above everything else
            if (null != this._ringParent)
            {
                this._ringParent->RemoveVisual((IDCompositionVisual*)this._ringVisual);
            }

            this._ringParent = outermost.Visual;
            this._ringParent->AddVisual((IDCompositionVisual*)this._ringVisual, false, null);

            this._host.RequestCommit();
        }

        /// <summary>
        /// The view's bounds (in window DIPs) within its ancestors' bounds.
        /// </summary>
        private static Rect VisibleBounds(
            ViewRenderer renderer)
        {
            var origin = renderer.AbsoluteOrigin();
            float left = origin.X;
            float top = origin.Y;
            float right = left + renderer.Frame.Width;
            float bottom = top + renderer.Frame.Height;

            for (var ancestor = renderer.ParentRenderer; null != ancestor; ancestor = ancestor.ParentRenderer)
            {
                var ancestorOrigin = ancestor.AbsoluteOrigin();
                left = Math.Max(left, ancestorOrigin.X);
                top = Math.Max(top, ancestorOrigin.Y);
                right = Math.Min(right, ancestorOrigin.X + ancestor.Frame.Width);
                bottom = Math.Min(bottom, ancestorOrigin.Y + ancestor.Frame.Height);
            }

            return new Rect(left, top, Math.Max(0, right - left), Math.Max(0, bottom - top));
        }

        private void EnsureRingVisual()
        {
            if (null == this._ringVisual)
            {
                IDCompositionVisual2* visual;
                this._host.CompositionDevice->CreateVisual(&visual);

                ((IUnknown*)visual)->QueryInterface(IDCompositionVisual3.IID_Guid, out var visual3).ThrowOnFailure();
                visual->Release();
                this._ringVisual = (IDCompositionVisual3*)visual3;
            }
        }

        private void DrawRing(
            int width,
            int height)
        {
            if (null != this._ringSurface)
            {
                this._ringSurface->Release();
            }

            IDCompositionSurface* surface;
            this._host.CompositionDevice->CreateSurface((uint)width, (uint)height, DXGI_FORMAT.DXGI_FORMAT_B8G8R8A8_UNORM, DXGI_ALPHA_MODE.DXGI_ALPHA_MODE_PREMULTIPLIED, &surface);
            this._ringSurface = surface;
            this._ringWidth = width;
            this._ringHeight = height;
            this._ringVisual->SetContent((IUnknown*)surface);

            surface->BeginDraw(null, ID2D1DeviceContext.IID_Guid, out var contextPointer, out var offset);
            var context = (ID2D1DeviceContext*)contextPointer;
            try
            {
                float scale = this._host.Scale;
                context->SetDpi(96, 96);
                context->SetTransform(new Windows.Win32.Graphics.Direct2D.Common.D2D_MATRIX_3X2_F()
                {
                    Anonymous = new Windows.Win32.Graphics.Direct2D.Common.D2D_MATRIX_3X2_F._Anonymous_e__Union()
                    {
                        Anonymous1 = new Windows.Win32.Graphics.Direct2D.Common.D2D_MATRIX_3X2_F._Anonymous_e__Union._Anonymous1_e__Struct()
                        {
                            m11 = scale,
                            m22 = scale,
                            dx = offset.X,
                            dy = offset.Y,
                        },
                    },
                });

                var drawContext = new DrawContext(context, new Rect(0, 0, 0, 0));
                drawContext.Clear(Colors.Transparent);

                // Windows' focus visual: a dark ring with a light inner edge
                float w = width / scale;
                float h = height / scale;
                drawContext.DrawRectangle(new Rect(RingThickness / 2, RingThickness / 2, w - RingThickness, h - RingThickness), Color.FromArgb(0xFF, 0x00, 0x00, 0x00), RingThickness);
                drawContext.DrawRectangle(new Rect(RingThickness + 0.5f, RingThickness + 0.5f, w - (2 * RingThickness) - 1, h - (2 * RingThickness) - 1), Colors.White, 1);
                drawContext.Release();
            }
            finally
            {
                surface->EndDraw();
                context->Release();
            }
        }
    }
}
