using System;
using System.IO;
using System.Threading.Tasks;
using Android.Graphics;
using AndroidImageView = global::Android.Widget.ImageView;
using XForms.Controls;

namespace XForms.Android.Renderers
{
    public class ImageRenderer : ViewRenderer<AndroidImageView>, IImageRenderer
    {
        private AndroidImageView _nativeImageView;
        private ImageSource _imageSource;

        public ImageRenderer(
            global::Android.Content.Context context,
            XForms.Controls.Image imageControl)
            : base(context, imageControl)
        {
            this._nativeImageView = new AndroidImageView(context);
            this.SetNativeElement(this._nativeImageView);
        }

        public void SetTint(
            Color tint)
        {
            this._nativeImageView.SetColorFilter(tint.ToAndroidColor());
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
                string resourceName = System.IO.Path.GetFileNameWithoutExtension(fileImageSource.Path).ToLowerInvariant();
                int resourceId = this.NativeContext.Resources.GetIdentifier(resourceName, "drawable", this.NativeContext.PackageName);
                if (0 == resourceId)
                {
                    throw new FileNotFoundException("Resource not found", resourceName);
                }
                var nativeBitmap = await BitmapFactory.DecodeResourceAsync(this.NativeContext.Resources, resourceId);
                if (null == nativeBitmap)
                {
                    throw new FileNotFoundException("Resource not found", resourceName);
                }

                this._nativeImageView.SetImageBitmap(nativeBitmap);
            }
            else
            {
                var bitmapImageSource = source as BitmapImageSource;
                if (null != bitmapImageSource)
                {
                    this._nativeImageView.SetImageBitmap(((BitmapRenderer)bitmapImageSource.Bitmap.Renderer).NativeBitmap);
                }
                else
                {
                    throw new ArgumentException("Invalid image source.", nameof(source));
                }
            }
        }
    }
}
