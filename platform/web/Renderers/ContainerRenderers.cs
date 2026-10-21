using System;
using System.Runtime.Versioning;
using XForms.Controls;

namespace XForms.Web.Renderers
{
    [SupportedOSPlatform("browser")]
    internal sealed class PanelRenderer : ViewRenderer, IPanelRenderer
    {
        public PanelRenderer(
            Panel layout)
            : base(layout, "div", isContainer: true)
        {
        }

        void IPanelRenderer.ClearChildren()
        {
            this.ClearChildren();
        }

        void IPanelRenderer.AddChild(
            IViewRenderer childRenderer)
        {
            this.AddChild(childRenderer);
        }

        void IPanelRenderer.InsertChild(
            int index,
            IViewRenderer childRenderer)
        {
            this.InsertChild(index, childRenderer);
        }

        void IPanelRenderer.RemoveChildAt(
            int index)
        {
            this.RemoveChildAt(index);
        }

        void IPanelRenderer.ReplaceChild(
            int index,
            IViewRenderer childRenderer)
        {
            this.RemoveChildAt(index);
            this.InsertChild(index, childRenderer);
        }
    }

    [SupportedOSPlatform("browser")]
    internal sealed class PageRenderer : ViewRenderer, IPageRenderer
    {
        public PageRenderer(
            Page page)
            : base(page, "div", isContainer: true)
        {
        }

        public void SetLayout(
            IPanelRenderer layoutRenderer)
        {
            this.ClearChildren();
            this.AddChild(layoutRenderer);
        }
    }

    [SupportedOSPlatform("browser")]
    internal sealed class ContentControlRenderer : ViewRenderer, IContentControlRenderer
    {
        public ContentControlRenderer(
            ContentControl contentControl)
            : base(contentControl, "div", isContainer: true)
        {
        }

        public void SetContent(
            IViewRenderer content)
        {
            this.ClearChildren();
            if (null != content)
            {
                this.AddChild(content);
            }
        }
    }

    /// <summary>
    /// An owner-drawn control: it draws into its canvas, beneath its content.
    /// </summary>
    [SupportedOSPlatform("browser")]
    internal sealed class UserControlRenderer : ViewRenderer, IUserControlRenderer
    {
        private readonly IDrawDelegate _drawDelegate;

        public UserControlRenderer(
            UserControl userControl)
            : base(userControl, "div", isContainer: true)
        {
            this._drawDelegate = userControl;
        }

        protected override bool HasContent
        {
            get
            {
                return true;
            }
        }

        public override Color BackgroundColor
        {
            get
            {
                return Colors.Transparent;
            }

            set
            {
                // NOTE: The control paints its own background when it draws
            }
        }

        public void Invalidate()
        {
            this.InvalidateRender();
        }

        public void SetContent(
            IViewRenderer content)
        {
            this.ClearChildren();
            if (null != content)
            {
                this.AddChild(content);
            }
        }

        protected override void DrawContent(
            DrawContext drawContext)
        {
            this._drawDelegate.InvokeRender(drawContext, new Rect(0, 0, this.Frame.Width, this.Frame.Height));
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
