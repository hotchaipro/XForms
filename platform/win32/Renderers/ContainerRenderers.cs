using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Direct2D;
using Windows.Win32.Graphics.Imaging;
using XForms.Controls;

namespace XForms.Win32.Renderers
{
    internal sealed class PanelRenderer : ViewRenderer, IPanelRenderer
    {
        protected override bool SupportsLightweight
        {
            get
            {
                return true;
            }
        }

        public PanelRenderer(
            Panel layout)
            : base(layout, isContainer: true)
        {
        }

        void IPanelRenderer.ClearChildren()
        {
            this.ClearChildren();
        }

        public void AddChild(
            IViewRenderer childRenderer)
        {
            base.AddChild((ViewRenderer)childRenderer);
        }

        public void InsertChild(
            int index,
            IViewRenderer childRenderer)
        {
            base.InsertChild(index, (ViewRenderer)childRenderer);
        }

        public void RemoveChildAt(
            int index)
        {
            this.RemoveChild(this.Children[index]);
        }

        public void ReplaceChild(
            int index,
            IViewRenderer childRenderer)
        {
            this.RemoveChild(this.Children[index]);
            base.InsertChild(index, (ViewRenderer)childRenderer);
        }
    }

    internal sealed class PageRenderer : ViewRenderer, IPageRenderer
    {
        public PageRenderer(
            Page page)
            : base(page, isContainer: true)
        {
        }

        public void SetLayout(
            IPanelRenderer layoutRenderer)
        {
            this.ClearChildren();
            this.AddChild((ViewRenderer)layoutRenderer);
        }
    }

    internal sealed class ContentControlRenderer : ViewRenderer, IContentControlRenderer
    {
        protected override bool SupportsLightweight
        {
            get
            {
                return true;
            }
        }

        public ContentControlRenderer(
            ContentControl contentControl)
            : base(contentControl, isContainer: true)
        {
        }

        public void SetContent(
            IViewRenderer content)
        {
            this.ClearChildren();

            if (content is ViewRenderer child)
            {
                this.AddChild(child);
            }
        }
    }

    /// <summary>
    /// Owner-drawn control: draws through the control's <see cref="IDrawDelegate"/>.
    /// </summary>
    internal sealed class UserControlRenderer : ViewRenderer, IUserControlRenderer
    {
        protected override bool SupportsLightweight
        {
            get
            {
                return true;
            }
        }

        private readonly IDrawDelegate _drawDelegate;

        public UserControlRenderer(
            UserControl userControl)
            : base(userControl, isContainer: true)
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
                // NOTE: The control paints its own background in DrawBackground
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

            if (content is ViewRenderer child)
            {
                this.AddChild(child);
            }
        }

        protected override void DrawContent(
            DrawContext drawContext)
        {
            this._drawDelegate.InvokeRender(drawContext, new Rect(0, 0, this.Frame.Width, this.Frame.Height));
        }
    }

    /// <summary>
    /// Hosts pages, each filling the frame.
    /// </summary>
    internal sealed class FrameRenderer : ViewRenderer, IFrameRenderer
    {
        public FrameRenderer(
            Frame frame)
            : base(frame, isContainer: false)
        {
        }

        public void Push(
            Page page)
        {
            var renderer = (ViewRenderer)page.Renderer;
            if (!this.Children.Contains(renderer))
            {
                this.AddChild(renderer);
            }
        }

        public void SetContent(
            Page newPage,
            bool hideCurrentPage)
        {
            var renderer = (ViewRenderer)newPage.Renderer;
            if (!this.Children.Contains(renderer))
            {
                this.AddChild(renderer);
            }

            if (hideCurrentPage)
            {
                foreach (var child in this.Children)
                {
                    if (child != renderer)
                    {
                        child.IsVisible = false;
                    }
                }
            }

            renderer.IsVisible = true;
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

    /// <summary>
    /// Decodes an image with WIC into a Direct2D bitmap.
    /// </summary>
    internal sealed unsafe class BitmapRenderer : ElementRenderer, IBitmapRenderer
    {
        public BitmapRenderer(
            Bitmap bitmap)
            : base(bitmap)
        {
        }

        public ID2D1Bitmap1* NativeBitmap
        {
            get;
            private set;
        }

        /// <summary>
        /// The bitmap's size in DIPs.
        /// </summary>
        public Size Size
        {
            get;
            private set;
        }

        public Task LoadAsync()
        {
            // NOTE: Decoding is quick for app images; doing it here keeps all
            // Direct2D work on the UI thread
            var source = (FileImageSource)((Bitmap)this.Element).Source;
            string path = Path.Combine(AppContext.BaseDirectory, "Content", source.Path);

            this.NativeBitmap = ImageLoader.Load(path, out var size);
            this.Size = size;

            return Task.CompletedTask;
        }
    }
}
