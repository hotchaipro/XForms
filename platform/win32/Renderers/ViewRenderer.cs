using System;
using System.Collections.Generic;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Direct2D;
using Windows.Win32.Graphics.Direct2D.Common;
using Windows.Win32.Graphics.DirectComposition;
using Windows.Win32.Graphics.Dxgi.Common;
using Windows.Win32.System.Com;
using XForms.Controls;
using XForms.Platform;

namespace XForms.Win32.Renderers
{
    internal abstract class ElementRenderer : IElementRenderer
    {
        protected ElementRenderer(
            Element element)
        {
            this.Element = element ?? throw new ArgumentNullException(nameof(element));
        }

        /// <summary>
        /// For platform-only views (such as a picker's drop-down) with no XForms element.
        /// </summary>
        protected ElementRenderer()
        {
        }

        public Element Element
        {
            get;
        }

        public virtual object NativeElement
        {
            get
            {
                return this;
            }
        }


        public virtual void OnThemeChanged(
            Theme theme)
        {
        }
    }

    /// <summary>
    /// A view backed by a DirectComposition visual. Views that draw (labels,
    /// owner-drawn controls, backgrounds) render into a composition surface.
    /// </summary>
    internal unsafe class ViewRenderer : ElementRenderer, IViewRenderer, IControlRenderer
    {
        private readonly bool _isContainer;
        private readonly bool _isLightweight;
        private IDCompositionVisual3* _visual;

        // The visual this view's root visual was added to: its layer's
        private IDCompositionVisual2* _hostedIn;
        private IDCompositionSurface* _surface;

        // A layer taller than this (in device pixels) draws only a band around
        // what's visible, into a virtual surface: one surface for all of a long
        // page would take a lot of memory, and can be larger than the GPU allows
        private const int TiledThreshold = 4096;

        // How far beyond the range it must show a tiled layer draws (in device
        // pixels), so short scrolls need no drawing
        private const int TiledMargin = 1024;

        private IDCompositionVirtualSurface* _virtualSurface;

        // The rows of a tiled layer that are drawn (in device pixels), or none
        private int _drawnTop, _drawnBottom;
        private AnimationLayer[] _layers;
        private int _surfaceWidth, _surfaceHeight;
        private Rect _frame;
        private bool _isVisible = true;
        private bool _clipToBounds;
        private Color _backgroundColor = Colors.Transparent;

        protected ViewRenderer(
            View view,
            bool isContainer)
            : base(view)
        {
            this._isContainer = isContainer;

            // A lightweight view has no visual of its own: it draws into its
            // layer's surface (see IsLightweight)
            this._isLightweight = this.SupportsLightweight && LayerHost.For(view).CanBeLightweight;
            if (!this._isLightweight)
            {
                this.CreateVisual();
            }
        }

        /// <summary>
        /// Whether views of this renderer's kind can be lightweight: drawn into
        /// their layer, the nearest ancestor with a visual.
        /// </summary>
        protected virtual bool SupportsLightweight
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Whether the view is lightweight. It stays in the renderer tree, so
        /// hit testing, focus and UI Automation work as for any view, but it
        /// has no visual or surface: its layer draws it, and hosts the visuals
        /// of native views inside it.
        /// </summary>
        public bool IsLightweight
        {
            get
            {
                return this._isLightweight;
            }
        }

        // The nearest ancestor with a visual
        private ViewRenderer Layer
        {
            get
            {
                for (var parent = this.ParentRenderer; null != parent; parent = parent.ParentRenderer)
                {
                    if (!parent._isLightweight)
                    {
                        return parent;
                    }
                }

                return null;
            }
        }

        // Where the view's parent's origin is in its layer's coordinates (the
        // lightweight ancestors' positions)
        private Point OffsetInLayer
        {
            get
            {
                float x = 0, y = 0;
                for (var parent = this.ParentRenderer; (null != parent) && parent._isLightweight; parent = parent.ParentRenderer)
                {
                    x += parent._frame.X;
                    y += parent._frame.Y;
                }

                return new Point(x, y);
            }
        }

        /// <summary>
        /// For platform-only views with no XForms element; they lay out and draw
        /// themselves.
        /// </summary>
        protected ViewRenderer()
        {
            this.CreateVisual();
        }

        private void CreateVisual()
        {
            IDCompositionVisual2* visual;
            this.Host.CompositionDevice->CreateVisual(&visual);

            ((IUnknown*)visual)->QueryInterface(IDCompositionVisual3.IID_Guid, out var visual3).ThrowOnFailure();
            visual->Release();
            this._visual = (IDCompositionVisual3*)visual3;
        }

        protected Win32Host Host
        {
            get
            {
                return this.WindowHost;
            }
        }

        private Win32Host _windowHost;

        /// <summary>
        /// The window the view is in: its root's (a window's root view, or a
        /// popup's), or the main window before it's in one.
        /// </summary>
        public Win32Host WindowHost
        {
            get
            {
                var root = this;
                while (null != root.ParentRenderer)
                {
                    root = root.ParentRenderer;
                }

                return root._windowHost ?? Win32Host.Current;
            }

            set
            {
                this._windowHost = value;
            }
        }

        /// <summary>
        /// The view's own visual, with its content and children.
        /// </summary>
        public IDCompositionVisual2* Visual
        {
            get
            {
                return (IDCompositionVisual2*)this._visual;
            }
        }

        /// <summary>
        /// The visual the parent adds: the outermost animation layer, or the
        /// view's own visual.
        /// </summary>
        public IDCompositionVisual2* RootVisual
        {
            get
            {
                return (null != this._layers) ? this._layers[0].Visual : this.Visual;
            }
        }

        private bool HasLightweightChildren
        {
            get
            {
                foreach (var child in this.Children)
                {
                    if (child._isLightweight)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        // The animation layers' static translation, in DIPs
        private Point Translation
        {
            get
            {
                float x = 0, y = 0;
                if (null != this._layers)
                {
                    foreach (var layer in this._layers)
                    {
                        var translation = layer.Translation;
                        x += translation.X;
                        y += translation.Y;
                    }
                }

                return new Point(x, y);
            }
        }

        public ViewRenderer ParentRenderer
        {
            get;
            private set;
        }

        public List<ViewRenderer> Children
        {
            get;
        } = new List<ViewRenderer>();

        public Rect Frame
        {
            get
            {
                return this._frame;
            }
        }

        /// <summary>
        /// Offset applied to the children, such as a scroll position.
        /// </summary>
        public virtual Point ChildOffset
        {
            get
            {
                return Point.Zero;
            }
        }

        /// <summary>
        /// The visual the children are added to.
        /// </summary>
        protected virtual IDCompositionVisual2* ChildVisual
        {
            get
            {
                return this.Visual;
            }
        }

        #region Children

        public void InsertChild(
            int index,
            ViewRenderer child)
        {
            child.ParentRenderer?.RemoveChild(child);

            this.Children.Insert(index, child);
            child.ParentRenderer = this;

            // The child's visual (or, for a lightweight child, the visuals of
            // the native views inside it) goes in the layer
            var layer = this._isLightweight ? this.Layer : this;
            if (null != layer)
            {
                layer.HostSubtree(child);
                if (child._isLightweight)
                {
                    layer.InvalidateRender();
                }
            }

            this.Host.RequestLayout();
        }

        // Adds the visuals of the native views in the subtree (stopping at
        // each, since its descendants are in its own visual)
        private void HostSubtree(
            ViewRenderer node)
        {
            if (!node._isLightweight)
            {
                this.HostVisual(node);
                return;
            }

            foreach (var child in node.Children)
            {
                this.HostSubtree(child);
            }
        }

        // Adds a native view's visual to this layer, after the native view
        // before it in drawing order
        private void HostVisual(
            ViewRenderer native)
        {
            if (null != native._hostedIn)
            {
                return;
            }

            ViewRenderer previous = null;
            this.FindPreviousHosted(native, ref previous);

            // NOTE: With a reference visual, insertAbove places the child in
            // front of it; without one, insertAbove places the child behind all
            // its siblings
            var reference = (null != previous) ? (IDCompositionVisual*)previous.RootVisual : null;
            this.ChildVisual->AddVisual((IDCompositionVisual*)native.RootVisual, true, reference);
            native._hostedIn = this.ChildVisual;

            native.UpdateOffset();
            native.UpdateOpacity();
        }

        // Finds the last hosted native view before the target in drawing order
        // (a depth-first walk through lightweight views). Returns whether it
        // found the target.
        private bool FindPreviousHosted(
            ViewRenderer target,
            ref ViewRenderer previous)
        {
            foreach (var child in this.Children)
            {
                if (child == target)
                {
                    return true;
                }

                if (!child._isLightweight)
                {
                    if (null != child._hostedIn)
                    {
                        previous = child;
                    }
                }
                else if (child.FindPreviousHosted(target, ref previous))
                {
                    return true;
                }
            }

            return false;
        }

        // Removes the visuals of the native views in the subtree from their
        // layer
        private static void UnhostSubtree(
            ViewRenderer node)
        {
            if (!node._isLightweight)
            {
                if (null != node._hostedIn)
                {
                    node._hostedIn->RemoveVisual((IDCompositionVisual*)node.RootVisual);
                    node._hostedIn = null;
                }

                return;
            }

            foreach (var child in node.Children)
            {
                UnhostSubtree(child);
            }
        }

        public void AddChild(
            ViewRenderer child)
        {
            this.InsertChild(this.Children.Count, child);
        }

        public void RemoveChild(
            ViewRenderer child)
        {
            if (this.Children.Remove(child))
            {
                UnhostSubtree(child);
                if (child._isLightweight)
                {
                    this.InvalidateRender();
                }

                child.ParentRenderer = null;
                this.Host.RequestLayout();
            }
        }

        public void ClearChildren()
        {
            foreach (var child in this.Children)
            {
                UnhostSubtree(child);
                child.ParentRenderer = null;
            }

            this.Children.Clear();
            this.InvalidateRender();
            this.Host.RequestLayout();
        }

        #endregion Children

        #region Layout

        public virtual Size MeasureNative(
            Size availableSize)
        {
            if (this._isContainer)
            {
                return ((View)this.Element).MeasureContent(availableSize);
            }

            return Size.Empty;
        }

        public virtual void ArrangeNative(
            Rect frame)
        {
            bool isResized = (frame.Width != this._frame.Width) || (frame.Height != this._frame.Height);

            this._frame = frame;
            this.UpdateOffset();

            if (isResized)
            {
                if (null != this._layers)
                {
                    foreach (var layer in this._layers)
                    {
                        layer.SetSize(frame.Size);
                    }
                }

                this.UpdateClip();
                this.OnResized();
            }

            if (this._isContainer)
            {
                ((View)this.Element).LayoutContent(frame.Size);
            }
        }

        public virtual void InvalidateMeasure()
        {
            this.Host.RequestLayout();
        }

        public bool ClipToBounds
        {
            set
            {
                this._clipToBounds = value;
                this.UpdateClip();
            }
        }

        private void UpdateClip()
        {
            if (this._isLightweight)
            {
                // Its layer clips it while drawing it
                this.InvalidateRender();
                return;
            }

            if (!this._clipToBounds)
            {
                // Scrolling views set their own clip
                return;
            }

            float scale = this.Host.Scale;
            this._visual->SetClip(new D2D_RECT_F() { left = 0, top = 0, right = this._frame.Width * scale, bottom = this._frame.Height * scale });
        }

        protected virtual void OnResized()
        {


            if (this.HasContent)
            {
                this.InvalidateRender();
            }
        }

        public Point AbsoluteOrigin()
        {
            float x = 0, y = 0;
            for (var renderer = this; null != renderer; renderer = renderer.ParentRenderer)
            {
                var translation = renderer.Translation;
                x += renderer._frame.X + translation.X;
                y += renderer._frame.Y + translation.Y;

                var parent = renderer.ParentRenderer;
                if (null != parent)
                {
                    var offset = parent.ChildOffset;
                    x += offset.X;
                    y += offset.Y;
                }
            }

            return new Point(x, y);
        }

        /// <summary>
        /// Finds the deepest visible view under the point (in the parent's coordinates).
        /// </summary>
        public ViewRenderer HitTest(
            Point point)
        {
            if (!this.IsEffectivelyVisible)
            {
                return null;
            }

            var translation = this.Translation;
            float x = point.X - this._frame.X - translation.X;
            float y = point.Y - this._frame.Y - translation.Y;

            // Children drawn outside an unclipped view (moved there by an
            // animator, say) still take input there
            bool isInside = (x >= 0) && (y >= 0) && (x < this._frame.Width) && (y < this._frame.Height);
            if (!isInside && this.ClipsHitTesting)
            {
                return null;
            }

            var offset = this.ChildOffset;
            var childPoint = new Point(x - offset.X, y - offset.Y);

            for (int i = this.Children.Count - 1; i >= 0; i -= 1)
            {
                var hit = this.Children[i].HitTest(childPoint);
                if (null != hit)
                {
                    return hit;
                }
            }

            return isInside ? this : null;
        }

        /// <summary>
        /// Whether input outside the view's bounds misses its children too: when
        /// it clips them (scrolling views always do).
        /// </summary>
        protected virtual bool ClipsHitTesting
        {
            get
            {
                return this._clipToBounds;
            }
        }

        private bool IsEffectivelyVisible
        {
            get
            {
                return this._isVisible;
            }
        }

        public bool IsShownOnScreen
        {
            get
            {
                for (var renderer = this; null != renderer; renderer = renderer.ParentRenderer)
                {
                    if (!renderer._isVisible)
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        private void UpdateOffset()
        {
            if (this._isLightweight)
            {
                // The native views inside it are positioned in its layer
                foreach (var child in this.Children)
                {
                    child.UpdateOffset();
                }

                return;
            }

            float scale = this.Host.Scale;

            // NOTE: Snap to device pixels so drawn content stays crisp. A parent's
            // child offset (scrolling) is applied by the parent's own visuals.
            // Animation layers translate on their own transforms. Lightweight
            // ancestors have no visuals, so their positions add up here.
            var offset = this.OffsetInLayer;
            var root = this.RootVisual;
            root->SetOffsetX((float)Math.Round((this._frame.X + offset.X) * scale));
            root->SetOffsetY((float)Math.Round((this._frame.Y + offset.Y) * scale));
        }

        #endregion Layout

        #region Drawing

        protected virtual bool HasContent
        {
            get
            {
                return !this._backgroundColor.IsTransparent;
            }
        }

        public void InvalidateRender()
        {
            if (this._isLightweight)
            {
                // Its layer draws it
                this.Layer?.InvalidateRender();
                return;
            }

            this.Host.RequestRender(this);
        }

        public void InvalidateRenderRecursive()
        {
            this._surfaceWidth = 0;
            this._surfaceHeight = 0;
            this.InvalidateRender();
            this.UpdateOffset();

            foreach (var child in this.Children)
            {
                child.InvalidateRenderRecursive();
            }
        }

        /// <summary>
        /// Draws the content into the visual's surface.
        /// </summary>
        public void Render()
        {
            if (this._isLightweight)
            {
                return;
            }

            float scale = this.Host.Scale;
            int width = (int)Math.Ceiling(this._frame.Width * scale);
            int height = (int)Math.Ceiling(this._frame.Height * scale);

            if ((!(this.HasContent || this.HasLightweightChildren)) || (width <= 0) || (height <= 0))
            {
                this.ReleaseSurfaces();
                return;
            }

            if (height > TiledThreshold)
            {
                // Its content changed: draw the band it needs again
                this._drawnTop = this._drawnBottom = 0;
                this.RenderTiled(width, height, this.VisibleRows(height));
                return;
            }

            if (null != this._virtualSurface)
            {
                this.ReleaseSurfaces();
            }

            if ((null == this._surface) || (width != this._surfaceWidth) || (height != this._surfaceHeight))
            {
                if (null != this._surface)
                {
                    this._surface->Release();
                }

                IDCompositionSurface* surface;
                this.Host.CompositionDevice->CreateSurface(
                    (uint)width,
                    (uint)height,
                    DXGI_FORMAT.DXGI_FORMAT_B8G8R8A8_UNORM,
                    DXGI_ALPHA_MODE.DXGI_ALPHA_MODE_PREMULTIPLIED,
                    &surface);

                this._surface = surface;
                this._surfaceWidth = width;
                this._surfaceHeight = height;
                this._visual->SetContent((IUnknown*)surface);
            }

            this._surface->BeginDraw(null, ID2D1DeviceContext.IID_Guid, out var contextPointer, out var surfaceOffset);
            var context = (ID2D1DeviceContext*)contextPointer;
            try
            {
                // Work in device-independent pixels; the surface offset is in device pixels
                context->SetDpi(96, 96);
                context->SetTransform(new D2D_MATRIX_3X2_F()
                {
                    Anonymous = new D2D_MATRIX_3X2_F._Anonymous_e__Union()
                    {
                        Anonymous1 = new D2D_MATRIX_3X2_F._Anonymous_e__Union._Anonymous1_e__Struct()
                        {
                            m11 = scale,
                            m22 = scale,
                            dx = surfaceOffset.X,
                            dy = surfaceOffset.Y,
                        },
                    },
                });

                var drawContext = new DrawContext(context, new Rect(0, 0, this._frame.Width, this._frame.Height));
                drawContext.Clear(this._backgroundColor);
                this.DrawContent(drawContext);
                this.DrawLightweightChildren(drawContext);
                drawContext.Release();
            }
            finally
            {
                this._surface->EndDraw();
                context->Release();
            }
        }

        private void ReleaseSurfaces()
        {
            if ((null == this._surface) && (null == this._virtualSurface))
            {
                return;
            }

            this._visual->SetContent(null);
            if (null != this._surface)
            {
                this._surface->Release();
                this._surface = null;
            }

            if (null != this._virtualSurface)
            {
                this._virtualSurface->Release();
                this._virtualSurface = null;
            }

            this._surfaceWidth = 0;
            this._surfaceHeight = 0;
            this._drawnTop = this._drawnBottom = 0;
        }

        // The rows of the layer (in device pixels) inside the window
        private (int Top, int Bottom) VisibleRows(
            int height)
        {
            float scale = this.Host.Scale;
            float top = -this.AbsoluteOrigin().Y;
            float bottom = top + this.Host.ClientSize.Height;
            return (
                Math.Clamp((int)Math.Floor(top * scale), 0, height),
                Math.Clamp((int)Math.Ceiling(bottom * scale), 0, height));
        }

        /// <summary>
        /// Makes sure a tiled layer has drawn the rows (in DIPs, in its own
        /// coordinates) it's about to show, such as when its scroll viewer
        /// starts to scroll across them.
        /// </summary>
        public void EnsureDrawn(
            float top,
            float bottom)
        {
            if ((null == this._virtualSurface) || this._isLightweight)
            {
                return;
            }

            float scale = this.Host.Scale;
            int height = this._surfaceHeight;
            int needTop = Math.Clamp((int)Math.Floor(top * scale), 0, height);
            int needBottom = Math.Clamp((int)Math.Ceiling(bottom * scale), 0, height);
            if ((needTop >= this._drawnTop) && (needBottom <= this._drawnBottom))
            {
                return;
            }

            this.RenderTiled(this._surfaceWidth, height, (needTop, needBottom));
            this.Host.RequestCommit();
        }

        // Draws the band around the rows into the virtual surface, and lets the
        // rest go
        private void RenderTiled(
            int width,
            int height,
            (int Top, int Bottom) rows)
        {
            if (null != this._surface)
            {
                this._surface->Release();
                this._surface = null;
            }

            if (null == this._virtualSurface)
            {
                IDCompositionVirtualSurface* virtualSurface;
                this.Host.CompositionDevice->CreateVirtualSurface(
                    (uint)width,
                    (uint)height,
                    DXGI_FORMAT.DXGI_FORMAT_B8G8R8A8_UNORM,
                    DXGI_ALPHA_MODE.DXGI_ALPHA_MODE_PREMULTIPLIED,
                    &virtualSurface);
                this._virtualSurface = virtualSurface;
                this._visual->SetContent((IUnknown*)virtualSurface);
            }
            else if ((width != this._surfaceWidth) || (height != this._surfaceHeight))
            {
                this._virtualSurface->Resize((uint)width, (uint)height);
            }

            this._surfaceWidth = width;
            this._surfaceHeight = height;

            // Round the band out to whole tiles' worth of rows
            const int Rows = 256;
            int top = Math.Max(0, ((rows.Top - TiledMargin) / Rows) * Rows);
            int bottom = Math.Min(height, (((rows.Bottom + TiledMargin) + Rows - 1) / Rows) * Rows);
            if (bottom <= top)
            {
                return;
            }

            var band = new RECT() { left = 0, top = top, right = width, bottom = bottom };
            this._virtualSurface->BeginDraw(band, ID2D1DeviceContext.IID_Guid, out var contextPointer, out var surfaceOffset);
            var context = (ID2D1DeviceContext*)contextPointer;
            try
            {
                float scale = this.Host.Scale;
                context->SetDpi(96, 96);

                // The band's top left is at the surface offset
                context->SetTransform(new D2D_MATRIX_3X2_F()
                {
                    Anonymous = new D2D_MATRIX_3X2_F._Anonymous_e__Union()
                    {
                        Anonymous1 = new D2D_MATRIX_3X2_F._Anonymous_e__Union._Anonymous1_e__Struct()
                        {
                            m11 = scale,
                            m22 = scale,
                            dx = surfaceOffset.X,
                            dy = surfaceOffset.Y - top,
                        },
                    },
                });

                var drawContext = new DrawContext(context, new Rect(0, 0, this._frame.Width, this._frame.Height));
                drawContext.Clear(this._backgroundColor);
                this.DrawContent(drawContext);
                this.DrawLightweightChildren(drawContext);
                drawContext.Release();
            }
            finally
            {
                this._virtualSurface->EndDraw();
                context->Release();
            }

            // Keep only the band
            this._virtualSurface->Trim(&band, 1);
            this._drawnTop = top;
            this._drawnBottom = bottom;
        }

        protected virtual void DrawContent(
            DrawContext drawContext)
        {
        }

        // Draws the lightweight children (and theirs) beneath the native ones,
        // each at its position
        private void DrawLightweightChildren(
            DrawContext drawContext)
        {
            foreach (var child in this.Children)
            {
                if (!child._isLightweight || !child._isVisible)
                {
                    continue;
                }

                var frame = child._frame;
                var bounds = new Rect(0, 0, frame.Width, frame.Height);
                var translation = drawContext.PushTransform(System.Numerics.Matrix3x2.CreateTranslation(frame.X, frame.Y));
                var clip = child._clipToBounds ? drawContext.PushClip(bounds) : default;

                var previous = LayerHost.SetLightweightBounds(drawContext, bounds);
                try
                {
                    if (!child._backgroundColor.IsTransparent)
                    {
                        drawContext.FillRectangle(bounds, child._backgroundColor);
                    }

                    child.DrawContent(drawContext);
                }
                finally
                {
                    LayerHost.SetLightweightBounds(drawContext, previous);
                }

                child.DrawLightweightChildren(drawContext);

                clip.Dispose();
                translation.Dispose();
            }
        }

        #endregion Drawing

        #region Properties

        public virtual bool IsVisible
        {
            get
            {
                return this._isVisible;
            }

            set
            {
                this._isVisible = value;
                this.UpdateOpacity();
                this.OnVisibilityChanged();
            }
        }

        protected virtual void OnVisibilityChanged()
        {
            // Text entry windows follow their views' visibility on the next update
            this.Host.RequestTextBoxPositionUpdate();
        }

        public bool IsHitTestVisible
        {
            get;
            set;
        } = true;

        public virtual Color BackgroundColor
        {
            get
            {
                return this._backgroundColor;
            }

            set
            {
                this._backgroundColor = value;
                this.InvalidateRender();
            }
        }

        private void UpdateOpacity()
        {
            if (this._isLightweight)
            {
                // Its layer stops drawing it, and the native views inside it hide
                foreach (var child in this.Children)
                {
                    child.UpdateOpacity();
                }

                this.InvalidateRender();
                return;
            }

            this._visual->SetOpacity(this.IsShownInLayer ? 1 : 0);
            this.Host.RequestCommit();
        }

        // Whether the view and its lightweight ancestors (which have no visuals
        // to hide it with) are visible
        private bool IsShownInLayer
        {
            get
            {
                if (!this._isVisible)
                {
                    return false;
                }

                for (var parent = this.ParentRenderer; (null != parent) && parent._isLightweight; parent = parent.ParentRenderer)
                {
                    if (!parent._isVisible)
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        #endregion Properties

        #region Animation

        public IAnimationLayer[] CreateAnimationLayers(
            int count)
        {
            if (null != this._layers)
            {
                throw new InvalidOperationException("The view's animation layers already exist.");
            }

            // Each layer wraps the next; the innermost wraps the view's visual
            var layers = new AnimationLayer[count];
            for (int i = 0; i < count; i += 1)
            {
                layers[i] = new AnimationLayer(this.Host);
                layers[i].SetSize(this._frame.Size);
                if (i > 0)
                {
                    layers[i - 1].Visual->AddVisual((IDCompositionVisual*)layers[i].Visual, false, null);
                }
            }

            layers[count - 1].Visual->AddVisual((IDCompositionVisual*)this._visual, false, null);
            this._layers = layers;
            this.UpdateOffset();

            return layers;
        }

        // Win32's lightweight views are in its own renderer tree (see
        // IsLightweight), so the framework never asks it to host
        public void AddHostedView(IViewRenderer child, int index) => throw new NotSupportedException();

        public void RemoveHostedView(IViewRenderer child) => throw new NotSupportedException();

        public void InvalidateLayerContent() => this.InvalidateRender();

        public bool Focus()
        {
            var focus = this.Host.Focus;
            focus.SetFocus(this, focus.IsRingVisible);
            return true;
        }

        #endregion Animation
    }
}
