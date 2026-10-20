using System;
using System.Collections.Generic;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Direct2D;
using Windows.Win32.Graphics.Direct2D.Common;
using Windows.Win32.Graphics.DirectComposition;
using Windows.Win32.Graphics.Dxgi.Common;
using Windows.Win32.System.Com;
using XForms.Controls;

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

        public bool HasDescendant(
            Element element)
        {
            return false;
        }

        public virtual void OnApplyTheme(
            AppTheme theme)
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
        private IDCompositionVisual3* _visual;
        private IDCompositionSurface* _surface;
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
            this.CreateVisual();
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
                return Win32Host.Current;
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

            var reference = (index > 0) ? (IDCompositionVisual*)this.Children[index - 1].RootVisual : null;
            // NOTE: With a reference visual, insertAbove places the child in front of
            // it; without one, insertAbove places the child behind all its siblings
            this.ChildVisual->AddVisual((IDCompositionVisual*)child.RootVisual, true, reference);

            this.Children.Insert(index, child);
            child.ParentRenderer = this;

            this.Host.RequestLayout();
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
                this.ChildVisual->RemoveVisual((IDCompositionVisual*)child.RootVisual);
                child.ParentRenderer = null;
                this.Host.RequestLayout();
            }
        }

        public void ClearChildren()
        {
            foreach (var child in this.Children)
            {
                child.ParentRenderer = null;
            }

            this.Children.Clear();
            this.ChildVisual->RemoveAllVisuals();
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

            if ((x < 0) || (y < 0) || (x >= this._frame.Width) || (y >= this._frame.Height))
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

            return this;
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
            float scale = this.Host.Scale;

            // NOTE: Snap to device pixels so drawn content stays crisp. A parent's
            // child offset (scrolling) is applied by the parent's own visuals.
            // Animation layers translate on their own transforms.
            var root = this.RootVisual;
            root->SetOffsetX((float)Math.Round(this._frame.X * scale));
            root->SetOffsetY((float)Math.Round(this._frame.Y * scale));
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
            float scale = this.Host.Scale;
            int width = (int)Math.Ceiling(this._frame.Width * scale);
            int height = (int)Math.Ceiling(this._frame.Height * scale);

            if ((!this.HasContent) || (width <= 0) || (height <= 0))
            {
                if (null != this._surface)
                {
                    this._visual->SetContent(null);
                    this._surface->Release();
                    this._surface = null;
                    this._surfaceWidth = 0;
                    this._surfaceHeight = 0;
                }

                return;
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
                drawContext.Release();
            }
            finally
            {
                this._surface->EndDraw();
                context->Release();
            }
        }

        protected virtual void DrawContent(
            DrawContext drawContext)
        {
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
            this._visual->SetOpacity(this._isVisible ? 1 : 0);
            this.Host.RequestCommit();
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

        public bool Focus()
        {
            var focus = this.Host.Focus;
            focus.SetFocus(this, focus.IsRingVisible);
            return true;
        }

        #endregion Animation
    }
}
