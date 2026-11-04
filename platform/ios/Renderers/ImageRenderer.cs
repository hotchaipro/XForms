using System;
using System.IO;
using System.Threading.Tasks;
using UIKit;

using XForms.Primitives;
using XForms.Platform;

namespace XForms.iOS.Renderers
{
    internal class ImageRenderer : ViewRenderer<UIImageView>, IImageRenderer
    {
        private UIImageView _nativeImageView;
        private ImageSource _imageSource;
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

        public async void SetSource(
            ImageSource source)
        {
            await this.LoadSourceAsync(source);
            this._imageSource = source;
        }

        private async Task LoadSourceAsync(
            ImageSource source)
        {
            var fileImageSource = source as FileImageSource;
            if (null != fileImageSource)
            {
                string resourceName = Path.GetFileName(fileImageSource.Path).ToLowerInvariant();
                // Loaded off the UI thread; shown on it (UIKit's views are only
                // touched there), where the await comes back to
                this._image = await Task.Run(() => UIImage.FromBundle(resourceName));
                this.ShowImage();
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
