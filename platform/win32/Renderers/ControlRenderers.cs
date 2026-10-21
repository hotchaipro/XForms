using System;
using System.IO;
using Windows.Win32.Graphics.Direct2D;
using Windows.Win32.Graphics.DirectComposition;
using XForms.Primitives;
using XForms.Platform;

namespace XForms.Win32.Renderers
{
    /// <summary>
    /// Draws an image file or bitmap stretched to the view, optionally painted
    /// with a tint through the image's alpha.
    /// </summary>
    internal sealed unsafe class ImageRenderer : ViewRenderer, IImageRenderer
    {
        private ID2D1Bitmap1* _bitmap;
        private Size _imageSize;
        private bool _ownsBitmap;
        private Color _tint = Colors.Transparent;

        public ImageRenderer(
            XForms.Primitives.Image image)
            : base(image, isContainer: false)
        {
        }

        protected override bool HasContent
        {
            get
            {
                return null != this._bitmap;
            }
        }

        public void SetTint(
            Color tint)
        {
            this._tint = tint;
            this.InvalidateRender();
        }

        public void SetSource(
            ImageSource source)
        {
            this.ReleaseBitmap();

            if (source is FileImageSource fileSource)
            {
                this._bitmap = ImageLoader.Load(Path.Combine(AppContext.BaseDirectory, "Content", fileSource.Path), out this._imageSize);
                this._ownsBitmap = true;
            }
            else if (source is BitmapImageSource bitmapSource)
            {
                var bitmapRenderer = (BitmapRenderer)bitmapSource.Bitmap.Renderer;
                this._bitmap = bitmapRenderer.NativeBitmap;
                this._imageSize = bitmapRenderer.Size;
            }
            else if (null != source)
            {
                throw new ArgumentException("Invalid image source.", nameof(source));
            }

            this.InvalidateMeasure();
            this.InvalidateRender();
        }

        private void ReleaseBitmap()
        {
            if (this._ownsBitmap && (null != this._bitmap))
            {
                this._bitmap->Release();
            }

            this._bitmap = null;
            this._ownsBitmap = false;
        }

        public override Size MeasureNative(
            Size availableSize)
        {
            if (null == this._bitmap)
            {
                return Size.Empty;
            }

            // The image's own size, scaled down (keeping its shape) to fit
            float width = this._imageSize.Width;
            float height = this._imageSize.Height;

            float fit = Math.Min(1.0f, Math.Min(availableSize.Width / width, availableSize.Height / height));
            return new Size(width * fit, height * fit);
        }

        protected override void DrawContent(
            DrawContext drawContext)
        {
            drawContext.DrawBitmap(this._bitmap, new Rect(0, 0, this.Frame.Width, this.Frame.Height), this._tint);
        }
    }

    /// <summary>
    /// Shows the popup's content above everything else: centered over the
    /// window, or below (or above) its placement target. Clicking outside it or
    /// pressing Escape closes it. The popup draws nothing itself: its content
    /// provides the look.
    /// </summary>
    internal sealed class PopupRenderer : ElementRenderer, IPopupRenderer, IOverlay
    {
        private const float WindowMargin = 24.0f;
        private const float AnchorGap = 4.0f;
        private const float EdgeMargin = 8.0f;

        private readonly PopupHostRenderer _chrome = new PopupHostRenderer();
        private View _content;
        private bool _isOpen;

        public PopupRenderer(
            Popup popup)
            : base(popup)
        {
        }

        public View Content
        {
            get
            {
                return this._content;
            }

            set
            {
                this._chrome.ClearChildren();
                this._content = value;

                if (value?.Renderer is ViewRenderer child)
                {
                    this._chrome.AddChild(child);
                }
            }
        }

        public View PlacementTarget
        {
            get;
            set;
        }

        // The window the popup opened in
        private Win32Host _openHost;

        ViewRenderer IOverlay.Renderer
        {
            get
            {
                return this._chrome;
            }
        }

        public void Show()
        {
            if (this._isOpen)
            {
                return;
            }

            this._isOpen = true;
            this._openHost = (this.PlacementTarget?.Renderer as ViewRenderer)?.WindowHost ?? Win32Host.Current;
            this._openHost.Overlays.Show(this);
        }

        public void Close()
        {
            if (!this._isOpen)
            {
                return;
            }

            this._isOpen = false;
            (this._openHost ?? Win32Host.Current).Overlays.Close(this);
            ((IPopupDelegate)this.Element).NotifyClosed();
        }

        void IOverlay.Dismiss()
        {
            this.Close();
        }

        void IOverlay.Place(
            Size windowSize)
        {
            var content = this._content;
            if (null == content)
            {
                return;
            }

            var frame = (this.PlacementTarget?.Renderer is ViewRenderer target)
                ? this.PlaceNextTo(content, target, windowSize)
                : this.PlaceCentered(content, windowSize);

            this._chrome.ArrangeNative(frame);
            content.Arrange(new Rect(0, 0, frame.Width, frame.Height));
        }

        private Rect PlaceCentered(
            View content,
            Size windowSize)
        {
            content.Measure(new Size(
                Math.Max(0, windowSize.Width - (2 * WindowMargin)),
                Math.Max(0, windowSize.Height - (2 * WindowMargin))));

            var size = content.DesiredSize;
            return new Rect(
                (float)Math.Round((windowSize.Width - size.Width) / 2),
                (float)Math.Round((windowSize.Height - size.Height) / 2),
                size.Width,
                size.Height);
        }

        /// <summary>
        /// Below the target, or above it when there's more room there, starting at
        /// its left edge and kept inside the window.
        /// </summary>
        private Rect PlaceNextTo(
            View content,
            ViewRenderer target,
            Size windowSize)
        {
            var origin = target.AbsoluteOrigin();
            var targetFrame = target.Frame;

            float below = Math.Max(0, windowSize.Height - (origin.Y + targetFrame.Height + AnchorGap) - EdgeMargin);
            float above = Math.Max(0, origin.Y - AnchorGap - EdgeMargin);
            float availableWidth = Math.Max(0, windowSize.Width - (2 * EdgeMargin));

            content.Measure(new Size(availableWidth, below));
            var size = content.DesiredSize;

            float y;
            if ((size.Height <= below) || (below >= above))
            {
                y = origin.Y + targetFrame.Height + AnchorGap;
            }
            else
            {
                content.Measure(new Size(availableWidth, above));
                size = content.DesiredSize;
                y = origin.Y - AnchorGap - size.Height;
            }

            float x = Math.Max(EdgeMargin, Math.Min(origin.X, windowSize.Width - size.Width - EdgeMargin));
            return new Rect((float)Math.Round(x), (float)Math.Round(y), size.Width, size.Height);
        }
    }

    /// <summary>
    /// Positions a popup's content; draws nothing itself.
    /// </summary>
    internal sealed class PopupHostRenderer : ViewRenderer
    {
    }

    /// <summary>
    /// Decodes image files with WIC into Direct2D bitmaps.
    /// </summary>
    internal static unsafe class ImageLoader
    {
        /// <summary>
        /// Loads the image, and gets its size in DIPs (one pixel each).
        /// </summary>
        /// <remarks>
        /// NOTE: The size comes from WIC because calling ID2D1Bitmap::GetSize
        /// (which returns a structure) through the generated interop corrupted
        /// memory.
        /// </remarks>
        public static ID2D1Bitmap1* Load(
            string path,
            out Size size)
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException("Image not found.", path);
            }

            var host = Win32Host.Current;

            var decoder = host.WicFactory->CreateDecoderFromFilename(
                path,
                null,
                Windows.Win32.Foundation.GENERIC_ACCESS_RIGHTS.GENERIC_READ,
                Windows.Win32.Graphics.Imaging.WICDecodeOptions.WICDecodeMetadataCacheOnDemand);

            Windows.Win32.Graphics.Imaging.IWICBitmapFrameDecode* frame;
            decoder->GetFrame(0, &frame);

            uint width, height;
            frame->GetSize(&width, &height);
            size = new Size(width, height);

            Windows.Win32.Graphics.Imaging.IWICFormatConverter* converter;
            host.WicFactory->CreateFormatConverter(&converter);
            converter->Initialize(
                (Windows.Win32.Graphics.Imaging.IWICBitmapSource*)frame,
                Windows.Win32.PInvoke.GUID_WICPixelFormat32bppPBGRA,
                Windows.Win32.Graphics.Imaging.WICBitmapDitherType.WICBitmapDitherTypeNone,
                null,
                0,
                Windows.Win32.Graphics.Imaging.WICBitmapPaletteType.WICBitmapPaletteTypeCustom);

            ID2D1Bitmap1* bitmap;
            host.ResourceContext->CreateBitmapFromWicBitmap((Windows.Win32.Graphics.Imaging.IWICBitmapSource*)converter, (D2D1_BITMAP_PROPERTIES1?)null, &bitmap);

            converter->Release();
            frame->Release();
            decoder->Release();

            return bitmap;
        }
    }
}
