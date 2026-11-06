using System;
using System.IO;
using System.Threading.Tasks;
using Android.Graphics;
using AndroidImageView = global::Android.Widget.ImageView;
using XForma.Primitives;
using XForma.Platform;

namespace XForma.Android.Renderers
{
    internal class ImageRenderer : ViewRenderer<AndroidImageView>, IImageRenderer
    {
        private AndroidImageView _nativeImageView;
        private int _loadNumber;

        public ImageRenderer(
            global::Android.Content.Context context,
            XForma.Primitives.Image imageControl)
            : base(context, imageControl)
        {
            this._nativeImageView = new AndroidImageView(context);
            this._nativeImageView.SetScaleType(global::Android.Widget.ImageView.ScaleType.FitXy);
            this.SetNativeElement(this._nativeImageView);
        }

        public void SetTint(
            Color tint)
        {
            // The tint through the image's alpha; none when transparent
            if (tint.IsTransparent)
            {
                this._nativeImageView.ClearColorFilter();
            }
            else
            {
                this._nativeImageView.SetColorFilter(tint.ToAndroidColor(), global::Android.Graphics.PorterDuff.Mode.SrcIn);
            }
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
                    this._nativeImageView.SetImageBitmap(null);
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

                if (loadNumber == this._loadNumber)
                {
                    this._nativeImageView.SetImageBitmap(nativeBitmap);
                }
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
