using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Direct2D;
using Windows.Win32.Graphics.Imaging;
using XForms.Primitives;
using XForms.Platform;

namespace XForms.Win32.Renderers
{
    /// <summary>
    /// The renderer of every view that holds children (panels, content and
    /// owner-drawn controls, pages): the view positions its children, and a
    /// view that draws itself draws beneath them.
    /// </summary>
    internal sealed class ContainerRenderer : ViewRenderer, IContainerRenderer
    {
        private readonly bool _drawsContent;

        public ContainerRenderer(
            View view)
            : base(view, isContainer: true)
        {
            this._drawsContent = LayerHost.For(view).DrawsContent;
        }

        protected override bool SupportsLightweight
        {
            get
            {
                return true;
            }
        }

        // It draws itself, or a background (a page's, under only native
        // children, has no lightweight children to draw it with)
        protected override bool HasContent
        {
            get
            {
                return this._drawsContent || base.HasContent;
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

        protected override void DrawContent(
            DrawContext drawContext)
        {
            if (this._drawsContent)
            {
                LayerHost.For((View)this.Element).Render(drawContext, new Rect(0, 0, this.Frame.Width, this.Frame.Height));
            }
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

        public void AddPage(
            Page page)
        {
            var renderer = (ViewRenderer)page.Renderer;
            if (!this.Children.Contains(renderer))
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
            // Children draw in order: move the page to the end
            var renderer = (ViewRenderer)page.Renderer;
            if (this.Children.IndexOf(renderer) != (this.Children.Count - 1))
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
