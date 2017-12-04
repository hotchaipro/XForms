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
            }
        }

        public new IContentControlRenderer Renderer
        {
            get
            {
                return (IContentControlRenderer)base.Renderer;
            }
        }

        protected override IElementRenderer CreateRenderer()
        {
            return this.Application.Platform.CreateContentControlRenderer(this);
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
