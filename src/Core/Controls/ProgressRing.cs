using System;

namespace XForms.Controls
{
    public interface IProgressRingRenderer : IControlRenderer
    {
        bool IsActive { get; set; }
    }

    public class ProgressRing : Control
    {
        public ProgressRing()
        {
        }

        public bool IsActive
        {
            get
            {
                return this.Renderer.IsActive;
            }

            set
            {
                this.Renderer.IsActive = value;
            }
        }

        public new IProgressRingRenderer Renderer
        {
            get
            {
                return (IProgressRingRenderer)base.Renderer;
            }
        }

        protected override IElementRenderer CreateRenderer()
        {
            return this.Application.Platform.CreateProgressRingRenderer(this);
        }
    }
}
