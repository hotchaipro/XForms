using System;
using System.IO;
using System.Threading.Tasks;
using UIKit;

using XForma.Primitives;
using XForma.Platform;

namespace XForma.iOS.Renderers
{
    internal class ImageRenderer : ViewRenderer<UIImageView>, IImageRenderer
    {
        private UIImageView _nativeImageView;
        private int _loadNumber;
        private UIImage _image;
        private Color _tint = Colors.Transparent;

        public ImageRenderer(
            Image imageControl)
            : base(imageControl)
        {
            this._nativeImageView = new UIImageView()
            {
            };
            this._nativeImageView.ContentMode = UIViewContentMode.ScaleAspectFit;
            this.SetNativeElement(this._nativeImageView);
        }

        public void SetTint(
            Color tint)
        {
            this._tint = tint;
            this._nativeImageView.TintColor = tint.ToUIColor();
            this.ShowImage();
        }

        // A tinted image is a template: its alpha, in the tint (as on the
        // other platforms); untinted, its own colors
        private void ShowImage()
        {
            var image = this._image;
            if ((null != image) && !this._tint.IsTransparent)
            {
                image = image.ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);
            }

            this._nativeImageView.Image = image;
        }

        // An image that doesn't load shows nothing, and the app carries on. The
        // latest source wins, whichever loads first (a list reuses its images)
        public async void SetSource(
            ImageSource source)
        {
            int loadNumber = ++this._loadNumber;
            try
            {
                await this.LoadSourceAsync(source, loadNumber);
            }
            catch (Exception ex)
            {
                FrameworkLog.Write(LogLevel.Warning, "An image didn't load.", ex);
                if (loadNumber == this._loadNumber)
                {
                    this._image = null;
                    this.ShowImage();
                }
            }
        }

        private async Task LoadSourceAsync(
            ImageSource source,
            int loadNumber)
        {
            var fileImageSource = source as FileImageSource;
            if (null != fileImageSource)
            {
                string resourceName = Path.GetFileName(fileImageSource.Path).ToLowerInvariant();
                // Loaded off the UI thread; shown on it (UIKit's views are only
                // touched there), where the await comes back to
                var image = await Task.Run(() => UIImage.FromBundle(resourceName));
                if (null == image)
                {
                    throw new FileNotFoundException("Resource not found", resourceName);
                }

                if (loadNumber == this._loadNumber)
                {
                    this._image = image;
                    this.ShowImage();
                }
            }
            else
            {
                var bitmapImageSource = source as BitmapImageSource;
                if (null != bitmapImageSource)
                {
                    this._image = ((BitmapRenderer)bitmapImageSource.Bitmap.Renderer).NativeImage;
                    this.ShowImage();
                }
                else
                {
                    throw new ArgumentException("Invalid image source.", nameof(source));
                }
            }
        }
    }
}
