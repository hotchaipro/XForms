using System;
using System.Collections.Generic;

namespace XForms.Controls
{
    public interface IPopoverDelegate
    {
        void NotifyClosed();
    }

    public interface IPopoverRenderer : IElementRenderer
    {
        View Content { get; set; }

        void ShowAsync();

        void Close();
    }

    public abstract class Popover : Element, IPopoverDelegate
    {
        public Popover()
        {
        }

        protected View Content
        {
            get
            {
                return this.Renderer.Content;
            }

            set
            {
                this.Renderer.Content = value;
            }
        }

        public void ShowAsync()
        {
            this.Renderer.ShowAsync();
            this.Application.SetActivePopover(this);
        }

        public void Close()
        {
            this.Renderer.Close();
        }

        protected virtual void OnClosed()
        {
        }

        void IPopoverDelegate.NotifyClosed()
        {
            this.Application.SetActivePopover(null);
            this.OnClosed();
        }

        public new IPopoverRenderer Renderer
        {
            get
            {
                return (IPopoverRenderer)base.Renderer;
            }
        }

        protected override IElementRenderer CreateRenderer()
        {
            return this.Application.Platform.CreatePopoverRenderer(this);
        }

        internal override IEnumerable<Element> GetChildElements()
        {
            var content = this.Content;
            if (null != content)
            {
                yield return content;
            }
        }
    }
}
