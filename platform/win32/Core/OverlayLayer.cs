using System;
using System.Collections.Generic;
using Windows.Win32.Graphics.DirectComposition;
using Windows.Win32.UI.Input.KeyboardAndMouse;
using XForma.Win32.Renderers;

namespace XForma.Win32
{
    /// <summary>
    /// A popup shown above the page, such as a popover or a picker's drop-down.
    /// </summary>
    internal interface IOverlay
    {
        /// <summary>
        /// The popup's outermost view, positioned in window coordinates.
        /// </summary>
        ViewRenderer Renderer { get; }

        /// <summary>
        /// Measures and arranges the popup within the window.
        /// </summary>
        void Place(Size windowSize);

        /// <summary>
        /// Closes the popup because the user clicked outside it or pressed Escape.
        /// </summary>
        void Dismiss();

        /// <summary>
        /// Whether clicking outside the popup closes it, and keyboard focus
        /// stays inside it; otherwise clicks outside it go on beneath.
        /// </summary>
        bool IsLightDismissEnabled { get; }

        /// <summary>
        /// The color over the window beneath the popup, or transparent.
        /// </summary>
        Color ScrimColor { get; }
    }

    /// <summary>
    /// Popups, drawn on a composition target above the window's child windows
    /// (so they also cover native text entries). Clicking outside the top popup
    /// dismisses it, and keyboard focus stays inside it while it is open.
    /// </summary>
    internal sealed unsafe class OverlayLayer
    {
        private readonly Win32Host _host;
        private readonly IDCompositionVisual2* _visual;
        private readonly List<IOverlay> _overlays = new List<IOverlay>();
        private readonly List<ViewRenderer> _previousFocus = new List<ViewRenderer>();

        // The scrims beneath the popups that have one
        private readonly Dictionary<IOverlay, Scrim> _scrims = new Dictionary<IOverlay, Scrim>();

        // A visual filled with a color, over the whole window
        private sealed class Scrim
        {
            public IDCompositionVisual2* Visual;
            public IDCompositionSurface* Surface;
            public Color Color;
            public int Width, Height;
        }

        public OverlayLayer(
            Win32Host host,
            IDCompositionVisual2* visual)
        {
            this._host = host;
            this._visual = visual;
        }

        /// <summary>
        /// The top light-dismissed popup, which input outside dismisses and
        /// which holds keyboard focus.
        /// </summary>
        public IOverlay Top
        {
            get
            {
                for (int i = this._overlays.Count - 1; i >= 0; i -= 1)
                {
                    if (this._overlays[i].IsLightDismissEnabled)
                    {
                        return this._overlays[i];
                    }
                }

                return null;
            }
        }

        /// <summary>
        /// Whether any popup is open.
        /// </summary>
        public bool IsAnyOpen
        {
            get
            {
                return this._overlays.Count > 0;
            }
        }

        public IReadOnlyList<IOverlay> Overlays
        {
            get
            {
                return this._overlays;
            }
        }

        public void Show(
            IOverlay overlay)
        {
            if (this._overlays.Contains(overlay))
            {
                return;
            }

            this._previousFocus.Add(this._host.Focus.Focused);
            this._overlays.Add(overlay);
            overlay.Renderer.WindowHost = this._host;

            // NOTE: A null reference with insertAbove false places the visual on top
            if (!overlay.ScrimColor.IsTransparent)
            {
                var scrim = new Scrim() { Color = overlay.ScrimColor };
                IDCompositionVisual2* scrimVisual;
                this._host.CompositionDevice->CreateVisual(&scrimVisual);
                scrim.Visual = scrimVisual;
                this._scrims[overlay] = scrim;
                this._visual->AddVisual((IDCompositionVisual*)scrimVisual, false, null);
                this.SizeScrim(scrim);
            }

            this._visual->AddVisual((IDCompositionVisual*)overlay.Renderer.Visual, false, null);

            // Lay the popup out now so it appears in the same frame
            this.Place(overlay);
            overlay.Renderer.InvalidateRenderRecursive();
            this._host.RequestCommit();
        }

        public void Close(
            IOverlay overlay)
        {
            int index = this._overlays.IndexOf(overlay);
            if (index < 0)
            {
                return;
            }

            var previousFocus = this._previousFocus[index];
            this._overlays.RemoveAt(index);
            this._previousFocus.RemoveAt(index);
            this._visual->RemoveVisual((IDCompositionVisual*)overlay.Renderer.RootVisual);
            if (this._scrims.Remove(overlay, out var scrim))
            {
                this._visual->RemoveVisual((IDCompositionVisual*)scrim.Visual);
                scrim.Visual->SetContent(null);
                scrim.Visual->Release();
                if (null != scrim.Surface)
                {
                    scrim.Surface->Release();
                }
            }

            this._host.RequestCommit();

            // Return keyboard focus to where it was before the popup opened
            this._host.Focus.OnOverlayClosed(overlay, previousFocus);
        }

        public void Layout()
        {
            foreach (var overlay in this._overlays)
            {
                this.Place(overlay);
            }

            foreach (var scrim in this._scrims.Values)
            {
                this.SizeScrim(scrim);
            }
        }

        // Fills the scrim's surface with its color, at the window's size
        private void SizeScrim(
            Scrim scrim)
        {
            var size = this._host.ClientSize;
            float scale = this._host.Scale;
            int width = Math.Max(1, (int)Math.Ceiling(size.Width * scale));
            int height = Math.Max(1, (int)Math.Ceiling(size.Height * scale));
            if ((null != scrim.Surface) && (width == scrim.Width) && (height == scrim.Height))
            {
                return;
            }

            if (null != scrim.Surface)
            {
                scrim.Surface->Release();
            }

            IDCompositionSurface* surface;
            this._host.CompositionDevice->CreateSurface(
                (uint)width,
                (uint)height,
                Windows.Win32.Graphics.Dxgi.Common.DXGI_FORMAT.DXGI_FORMAT_B8G8R8A8_UNORM,
                Windows.Win32.Graphics.Dxgi.Common.DXGI_ALPHA_MODE.DXGI_ALPHA_MODE_PREMULTIPLIED,
                &surface);
            scrim.Surface = surface;
            scrim.Width = width;
            scrim.Height = height;
            scrim.Visual->SetContent((Windows.Win32.System.Com.IUnknown*)surface);

            surface->BeginDraw(null, Windows.Win32.Graphics.Direct2D.ID2D1DeviceContext.IID_Guid, out var contextPointer, out _);
            var context = (Windows.Win32.Graphics.Direct2D.ID2D1DeviceContext*)contextPointer;
            try
            {
                var drawContext = new DrawContext(context, new Rect(0, 0, width, height));
                drawContext.Clear(scrim.Color);
                drawContext.Release();
            }
            finally
            {
                surface->EndDraw();
                context->Release();
            }
        }

        private void Place(
            IOverlay overlay)
        {
            overlay.Place(this._host.ClientSize);
        }

        /// <summary>
        /// Finds the view under the point in the top popup. Returns false when a
        /// popup is open but the point is outside it.
        /// </summary>
        public bool HitTest(
            Point point,
            out ViewRenderer hit)
        {
            hit = null;

            // Down through the popups that aren't light-dismissed, to the top
            // light-dismissed one (outside which a click dismisses it) or the
            // window (null)
            for (int i = this._overlays.Count - 1; i >= 0; i -= 1)
            {
                var overlay = this._overlays[i];
                hit = overlay.Renderer.HitTest(point);
                if (null != hit)
                {
                    return true;
                }

                if (overlay.IsLightDismissEnabled)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
