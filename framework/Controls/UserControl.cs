using System;
using System.Collections.Generic;

namespace XForms.Controls
{
    internal interface IDrawDelegate
    {
        void InvokeRender(
            DrawingContext drawContext,
            Rect bounds);
    }

    public interface IUserControlRenderer : IControlRenderer
    {
        void Invalidate();

        void SetContent(IViewRenderer content);
    }

    // TODO: Try to move IDrawDelegate to the Control base class or to CanvasControl
    // TODO: Use IsDrawEnabled to optionally create the native Win2D canvas control
    public abstract class UserControl : Control, IDrawDelegate
    {
        internal override bool IsLightweightType
        {
            get
            {
                return true;
            }
        }

        // The content as a list, made again only when the content changes
        private View[] _contentList;

        internal override IReadOnlyList<View> DrawingOrderChildren
        {
            get
            {
                var content = this._content;
                if (null == content)
                {
                    return Array.Empty<View>();
                }

                if ((null == this._contentList) || (this._contentList[0] != content))
                {
                    this._contentList = new[] { content };
                }

                return this._contentList;
            }
        }

        private View _content;

        public UserControl()
        {
        }

        /// <summary>
        /// Space between the control's edges and its content.
        /// </summary>
        public virtual Thickness Padding
        {
            get
            {
                return this.ContentPadding;
            }

            set
            {
                this.ContentPadding = value;
            }
        }

        protected View Content
        {
            get
            {
                return this._content;
            }

            set
            {
                var oldContent = this._content;
                if (value == oldContent)
                {
                    return;
                }

                this._content = value;

                var native = this.Native;
                if (null != native)
                {
                    native.SetContent(value?.AttachTo(this));
                    oldContent?.NotifyDetached();
                    value?.NotifyAttached();
                }

                this.InvalidateMeasure();
            }
        }

        internal new IUserControlRenderer Renderer
        {
            get
            {
                return (IUserControlRenderer)base.Renderer;
            }
        }

        private IUserControlRenderer Native
        {
            get
            {
                return (IUserControlRenderer)this.CreatedRenderer;
            }
        }

        /// <summary>
        /// Draws the control again.
        /// </summary>
        public virtual void InvalidateVisual()
        {
            this.Native?.Invalidate();
        }

        internal override void OnRendererCreated()
        {
            base.OnRendererCreated();

            var content = this._content;
            if (null != content)
            {
                this.Native.SetContent(content.AttachTo(this));
                content.NotifyAttached();
            }
        }

        protected override IElementRenderer CreateRenderer()
        {
            return this.Application.Platform.CreateUserControlRenderer(this);
        }

        /// <summary>
        /// Draws the control, within its bounds (from 0, 0). By default it fills
        /// the control with its background color. Call
        /// <see cref="InvalidateVisual"/> to draw again.
        /// </summary>
        protected virtual void Render(
            DrawingContext drawingContext)
        {
            drawingContext.Clear(this.BackgroundColor);
        }

        private System.Collections.Generic.List<DrawnAnimator> _drawnAnimators;

        /// <summary>
        /// Creates a scale and opacity the control applies to its own drawing,
        /// animated without a native layer (see <see cref="DrawnAnimator"/>).
        /// For small effects, such as press feedback, that shouldn't cost the
        /// control a layer of its own, as an <see cref="Animator"/> would.
        /// </summary>
        protected DrawnAnimator CreateDrawnAnimator()
        {
            var animator = new DrawnAnimator(this.InvalidateVisual);
            this._drawnAnimators ??= new System.Collections.Generic.List<DrawnAnimator>();
            this._drawnAnimators.Add(animator);
            return animator;
        }

        void IDrawDelegate.InvokeRender(
            DrawingContext drawContext,
            Rect bounds)
        {
            var animators = this._drawnAnimators;
            if (null == animators)
            {
                this.Render(drawContext);
                return;
            }

            var scopes = new DrawnAnimator.Scope[animators.Count];
            for (int i = 0; i < animators.Count; i += 1)
            {
                scopes[i] = animators[i].Apply(drawContext, bounds);
            }

            try
            {
                this.Render(drawContext);
            }
            finally
            {
                for (int i = animators.Count - 1; i >= 0; i -= 1)
                {
                    scopes[i].Dispose();
                }
            }
        }

        protected override Size MeasureOverride(
            Size availableSize)
        {
            var content = this._content;
            if (null == content)
            {
                return Size.Empty;
            }

            content.Measure(availableSize);
            return content.DesiredSize;
        }

        protected override void ArrangeOverride(
            Size finalSize)
        {
            this._content?.Arrange(new Rect(0, 0, finalSize.Width, finalSize.Height));
        }

        internal override IEnumerable<Element> GetChildElements()
        {
            var content = this._content;
            if (null != content)
            {
                yield return content;
            }
        }

        protected override void OnThemeChanged(
            Theme theme)
        {
            base.OnThemeChanged(theme);

            this.InvalidateVisual();
        }

        internal override void OnTextScaleChangedCore()
        {
            base.OnTextScaleChangedCore();
            this.InvalidateVisual();
        }
    }
}
