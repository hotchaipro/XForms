using System;
using System.Collections.Generic;
using Windows.Win32.Graphics.DirectComposition;
using Windows.Win32.UI.Input.KeyboardAndMouse;
using XForms.Win32.Renderers;

namespace XForms.Win32
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

        public OverlayLayer(
            Win32Host host,
            IDCompositionVisual2* visual)
        {
            this._host = host;
            this._visual = visual;
        }

        public IOverlay Top
        {
            get
            {
                return (this._overlays.Count > 0) ? this._overlays[this._overlays.Count - 1] : null;
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

            var top = this.Top;
            if (null == top)
            {
                return true;
            }

            hit = top.Renderer.HitTest(point);
            return null != hit;
        }
    }
}
