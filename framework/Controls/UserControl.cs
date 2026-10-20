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

        void IDrawDelegate.InvokeRender(
            DrawingContext drawContext,
            Rect bounds)
        {
            this.Render(drawContext);
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

        protected override void OnApplyTheme(
            AppTheme theme)
        {
            base.OnApplyTheme(theme);

            this.InvalidateVisual();
        }
    }
}
