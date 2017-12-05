using System;
using System.IO;
using System.Threading.Tasks;
using UIKit;

namespace XForms.iOS.Renderers
{
    public class ImageRenderer : ViewRenderer<UIImageView>, IImageRenderer
    {
        private UIImageView _nativeImageView;
        private ImageSource _imageSource;

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
            this._nativeImageView.TintColor = tint.ToUIColor();
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
                await Task.Run(() =>
                {
                    var image = UIImage.FromBundle(resourceName);
                    this._nativeImageView.Image = image;
                });
            }
            else
            {
                var bitmapImageSource = source as BitmapImageSource;
                if (null != bitmapImageSource)
                {
                    this._nativeImageView.Image = (((BitmapRenderer)bitmapImageSource.Bitmap.Renderer).NativeImage);
                }
                else
                {
                    throw new ArgumentException("Invalid image source.", nameof(source));
                }
            }
        }
    }
}
