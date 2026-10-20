using System;
using System.Collections.Generic;

namespace XForms.Controls
{
    public interface IContentControlRenderer : IControlRenderer
    {
        void SetContent(IViewRenderer content);
    }

    public abstract class ContentControl : Control
    {
        private View _content;

        public ContentControl()
        {
        }

        /// <summary>
        /// Space between the control's edges and its content.
        /// </summary>
        public Thickness Padding
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

        internal new IContentControlRenderer Renderer
        {
            get
            {
                return (IContentControlRenderer)base.Renderer;
            }
        }

        private IContentControlRenderer Native
        {
            get
            {
                return (IContentControlRenderer)this.CreatedRenderer;
            }
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
            return this.Application.Platform.CreateContentControlRenderer(this);
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
    }
}
