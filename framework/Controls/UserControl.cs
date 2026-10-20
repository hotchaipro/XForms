using System;
using System.Collections.Generic;

namespace XForms.Controls
{
    public interface IDrawDelegate
    {
        void InvokeDrawBackground(
            IDrawContext drawContext,
            Rectangle bounds);
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

        protected View Content
        {
            get
            {
                return this._content;
            }

            set
            {
                this._content = value;

                this.Renderer.SetContent(value?.Renderer);
                this.InvalidateLayout();
            }
        }

        public new IUserControlRenderer Renderer
        {
            get
            {
                return (IUserControlRenderer)base.Renderer;
            }
        }

        public virtual void Invalidate()
        {
            this.Renderer?.Invalidate();
        }

        protected override IElementRenderer CreateRenderer()
        {
            return this.Application.Platform.CreateUserControlRenderer(this);
        }

        protected virtual void DrawBackground(
            IDrawContext drawContext,
            Rectangle bounds)
        {
            drawContext.Clear(this.BackgroundColor);
        }

        void IDrawDelegate.InvokeDrawBackground(
            IDrawContext drawContext,
            Rectangle bounds)
        {
            this.DrawBackground(drawContext, bounds);
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
            this._content?.Arrange(new Rectangle(0, 0, finalSize.Width, finalSize.Height));
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

            this.Invalidate();
        }
    }
}
