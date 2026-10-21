using System;
using System.Runtime.Versioning;
using XForms.Primitives;
using XForms.Platform;

namespace XForms.Web.Renderers
{
    /// <summary>
    /// The renderer of every view that holds children (panels, content and
    /// owner-drawn controls, pages): the view positions its children, and a
    /// view that draws itself draws into its canvas, beneath them.
    /// </summary>
    [SupportedOSPlatform("browser")]
    internal sealed class ContainerRenderer : ViewRenderer, IContainerRenderer
    {
        private readonly bool _drawsContent;

        public ContainerRenderer(
            View view)
            : base(view, "div", isContainer: true)
        {
            this._drawsContent = LayerHost.For(view).DrawsContent;
        }

        protected override bool HasContent
        {
            get
            {
                return this._drawsContent;
            }
        }

        public void Invalidate()
        {
            this.InvalidateRender();
        }

        void IContainerRenderer.ClearChildren()
        {
            this.ClearChildren();
        }

        void IContainerRenderer.InsertChild(
            int index,
            IViewRenderer childRenderer)
        {
            this.InsertChild(index, childRenderer);
        }

        void IContainerRenderer.RemoveChildAt(
            int index)
        {
            this.RemoveChildAt(index);
        }

        protected override void DrawContent(
            DrawContext drawContext)
        {
            if (this._drawsContent)
            {
                LayerHost.For(this.View).Render(drawContext, new Rect(0, 0, this.Frame.Width, this.Frame.Height));
            }
        }
    }

    /// <summary>
    /// Pages fill the frame; transitions show two at once.
    /// </summary>
    [SupportedOSPlatform("browser")]
    internal sealed class FrameRenderer : ViewRenderer, IFrameRenderer
    {
        public FrameRenderer(
            Frame frame)
            : base(frame, "div", isContainer: false)
        {
            Js.SetStyle(this.Id, "overflow", "hidden");
        }

        public void AddPage(
            Page page)
        {
            var renderer = (ViewRenderer)page.Renderer;
            if (renderer.ParentRenderer != this)
            {
                this.AddChild(renderer);
            }
        }

        public void RemovePage(
            Page page)
        {
            this.RemoveChild((ViewRenderer)page.Renderer);
        }

        public void BringToFront(
            Page page)
        {
            // Later elements are on top
            var renderer = (ViewRenderer)page.Renderer;
            int index = 0;
            for (; index < this.Children.Count; index += 1)
            {
                if (this.Children[index] == renderer)
                {
                    break;
                }
            }

            if (index != this.Children.Count - 1)
            {
                this.RemoveChild(renderer);
                this.AddChild(renderer);
            }
        }

        public override Size MeasureNative(
            Size availableSize)
        {
            return new Size(
                float.IsInfinity(availableSize.Width) ? 0 : availableSize.Width,
                float.IsInfinity(availableSize.Height) ? 0 : availableSize.Height);
        }

        public override void ArrangeNative(
            Rect frame)
        {
            base.ArrangeNative(frame);

            foreach (var child in this.Children)
            {
                var page = (View)child.Element;
                page.Measure(frame.Size);
                page.Arrange(new Rect(0, 0, frame.Width, frame.Height));
            }
        }
    }
}
