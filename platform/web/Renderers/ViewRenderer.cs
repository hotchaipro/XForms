using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.Versioning;
using XForms.Controls;

namespace XForms.Web.Renderers
{
    [SupportedOSPlatform("browser")]
    internal abstract class ElementRenderer : IElementRenderer
    {
        protected ElementRenderer(
            Element element)
        {
            this.Element = element ?? throw new ArgumentNullException(nameof(element));
        }

        public Element Element
        {
            get;
        }

        public abstract object NativeElement
        {
            get;
        }

        public virtual void OnThemeChanged(
            Theme theme)
        {
        }
    }

    /// <summary>
    /// A view with a DOM element of its own, positioned absolutely in its
    /// parent's. A layer draws its own content and its lightweight
    /// descendants into a canvas beneath its child elements.
    /// </summary>
    [SupportedOSPlatform("browser")]
    internal class ViewRenderer : ElementRenderer, IViewRenderer, IControlRenderer
    {
        private readonly bool _isContainer;

        // The view's children, in order (lightweight ones have no element),
        // and the native views of lightweight descendants it hosts, whose
        // elements come after its own children's
        private readonly List<IViewRenderer> _ownChildren = new List<IViewRenderer>();
        private readonly List<ViewRenderer> _hostedViews = new List<ViewRenderer>();

        private int _canvasId;
        private Rect _frame;
        private bool _isVisible = true;
        private bool _isHitTestVisible = true;
        private Color _backgroundColor = Colors.Transparent;
        private AnimationLayerStack _layers;

        protected ViewRenderer(
            View view,
            string tag,
            bool isContainer)
            : base(view)
        {
            this._isContainer = isContainer;
            this.Id = Js.Create(tag);
            WebHost.Current.Register(this);
        }

        /// <summary>
        /// The element's id in the page's table.
        /// </summary>
        public int Id
        {
            get;
        }

        public override object NativeElement
        {
            get
            {
                return this.Id;
            }
        }

        protected WebHost Host
        {
            get
            {
                return WebHost.Current;
            }
        }

        protected View View
        {
            get
            {
                return (View)this.Element;
            }
        }

        public Rect Frame
        {
            get
            {
                return this._frame;
            }
        }

        public ViewRenderer ParentRenderer
        {
            get;
            private set;
        }

        /// <summary>
        /// The children with elements of their own, in order.
        /// </summary>
        protected IReadOnlyList<ViewRenderer> Children
        {
            get
            {
                var children = new List<ViewRenderer>();
                foreach (var child in this._ownChildren)
                {
                    if (child is ViewRenderer native)
                    {
                        children.Add(native);
                    }
                }

                return children;
            }
        }

        protected static string Px(
            float value)
        {
            return value.ToString("0.###", CultureInfo.InvariantCulture) + "px";
        }

        protected static string Css(
            Color color)
        {
            return string.Format(CultureInfo.InvariantCulture, "rgba({0},{1},{2},{3:0.###})", color.R, color.G, color.B, color.A / 255.0);
        }

        #region Children

        // Where the element of a child at the index goes among the element's
        // children (after the canvas, if any)
        private int DomIndex(
            int index)
        {
            int position = (0 != this._canvasId) ? 1 : 0;
            for (int i = 0; i < index; i += 1)
            {
                if (this._ownChildren[i] is ViewRenderer)
                {
                    position += 1;
                }
            }

            return position;
        }

        protected void InsertChild(
            int index,
            IViewRenderer child)
        {
            this._ownChildren.Insert(index, child);
            if (child is ViewRenderer native)
            {
                native.ParentRenderer?.Detach(native);
                native.ParentRenderer = this;
                Js.Insert(this.Id, native.Id, this.DomIndex(index));
            }
            else
            {
                // A lightweight child draws into this view's canvas
                this.InvalidateRender();
            }

            this.Host.RequestLayout();
        }

        protected void AddChild(
            IViewRenderer child)
        {
            this.InsertChild(this._ownChildren.Count, child);
        }

        protected void RemoveChildAt(
            int index)
        {
            var child = this._ownChildren[index];
            this._ownChildren.RemoveAt(index);
            if (child is ViewRenderer native)
            {
                native.ParentRenderer = null;
                Js.Detach(native.Id);
            }
            else
            {
                this.InvalidateRender();
            }

            this.Host.RequestLayout();
        }

        protected void RemoveChild(
            IViewRenderer child)
        {
            int index = this._ownChildren.IndexOf(child);
            if (index >= 0)
            {
                this.RemoveChildAt(index);
            }
        }

        protected void ClearChildren()
        {
            for (int i = this._ownChildren.Count - 1; i >= 0; i -= 1)
            {
                this.RemoveChildAt(i);
            }
        }

        // Takes a native view out of this one (when it moves elsewhere)
        private void Detach(
            ViewRenderer child)
        {
            if (this._hostedViews.Remove(child))
            {
                child.ParentRenderer = null;
                return;
            }

            int index = this._ownChildren.IndexOf(child);
            if (index >= 0)
            {
                this._ownChildren.RemoveAt(index);
                child.ParentRenderer = null;
            }
        }

        // The native views of lightweight descendants, after the view's own
        // children, in drawing order among themselves
        public void AddHostedView(
            IViewRenderer child,
            int index)
        {
            var renderer = (ViewRenderer)child;
            renderer.ParentRenderer?.Detach(renderer);

            this._hostedViews.Insert(index, renderer);
            renderer.ParentRenderer = this;
            Js.Insert(this.Id, renderer.Id, this.DomIndex(this._ownChildren.Count) + index);
        }

        public void RemoveHostedView(
            IViewRenderer child)
        {
            var renderer = (ViewRenderer)child;
            if (this._hostedViews.Remove(renderer))
            {
                renderer.ParentRenderer = null;
                Js.Detach(renderer.Id);
            }
        }

        #endregion Children

        #region Layout

        public virtual Size MeasureNative(
            Size availableSize)
        {
            if (this._isContainer)
            {
                return this.View.MeasureContent(availableSize);
            }

            return Size.Empty;
        }

        public virtual void ArrangeNative(
            Rect frame)
        {
            bool isResized = (frame.Width != this._frame.Width) || (frame.Height != this._frame.Height);
            this._frame = frame;
            Js.SetBounds(this.Id, frame.X, frame.Y, frame.Width, frame.Height);

            if (this._isContainer)
            {
                this.View.LayoutContent(frame.Size);
            }

            if (isResized)
            {
                this._layers?.Update();
                if (this.HasContent || (0 != this._canvasId))
                {
                    this.InvalidateRender();
                }
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
                Js.SetStyle(this.Id, "overflow", value ? "hidden" : null);
            }
        }

        #endregion Layout

        #region Properties

        public virtual bool IsVisible
        {
            get
            {
                return this._isVisible;
            }

            set
            {
                if (value != this._isVisible)
                {
                    this._isVisible = value;
                    Js.SetStyle(this.Id, "display", value ? null : "none");
                    this.Host.RequestLayout();
                }
            }
        }

        public bool IsHitTestVisible
        {
            get
            {
                return this._isHitTestVisible;
            }

            set
            {
                this._isHitTestVisible = value;
                Js.SetStyle(this.Id, "pointer-events", value ? null : "none");
            }
        }

        public virtual Color BackgroundColor
        {
            get
            {
                return this._backgroundColor;
            }

            set
            {
                this._backgroundColor = value;
                Js.SetStyle(this.Id, "background-color", value.IsTransparent ? null : Css(value));
            }
        }

        #endregion Properties

        #region Drawing

        /// <summary>
        /// Whether the view draws content of its own (in addition to its
        /// lightweight descendants).
        /// </summary>
        protected virtual bool HasContent
        {
            get
            {
                return false;
            }
        }

        public void InvalidateRender()
        {
            this.Host.RequestRender(this);
        }

        public void InvalidateLayerContent()
        {
            this.InvalidateRender();
        }

        /// <summary>
        /// Draws the content and the lightweight descendants into the canvas.
        /// </summary>
        public void Render()
        {
            var view = this.View;
            bool hasDrawing = this.HasContent || view.HasLightweightChildren;
            if (!hasDrawing)
            {
                if (0 != this._canvasId)
                {
                    Js.SetStyle(this._canvasId, "display", "none");
                }

                return;
            }

            if (0 == this._canvasId)
            {
                this._canvasId = Js.Create("canvas");
                Js.SetStyle(this._canvasId, "pointer-events", "none");
                Js.Insert(this.Id, this._canvasId, 0);
            }

            Js.SetStyle(this._canvasId, "display", null);

            var bounds = new Rect(0, 0, this._frame.Width, this._frame.Height);
            var drawContext = new DrawContext(bounds);
            this.DrawContent(drawContext);
            LightweightPainter.PaintDescendants(view, drawContext);
            drawContext.Flush(this._canvasId);
        }

        protected virtual void DrawContent(
            DrawContext drawContext)
        {
        }

        #endregion Drawing

        #region Animation and focus

        public IAnimationLayer[] CreateAnimationLayers(
            int count)
        {
            if (null != this._layers)
            {
                throw new InvalidOperationException("The view's animation layers already exist.");
            }

            this._layers = new AnimationLayerStack(this, count);
            return this._layers.Layers;
        }

        public virtual bool Focus()
        {
            return false;
        }

        #endregion Animation and focus

        public override void OnThemeChanged(
            Theme theme)
        {
            base.OnThemeChanged(theme);
            if (this.HasContent)
            {
                this.InvalidateRender();
            }
        }
    }
}
