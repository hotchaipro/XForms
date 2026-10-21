using System;
using System.IO;
using System.Threading.Tasks;
using Android.Graphics;
using AndroidBitmap = global::Android.Graphics.Bitmap;

namespace XForms.Android.Renderers
{
    public class BitmapRenderer : ElementRenderer<AndroidBitmap>, IBitmapRenderer
    {
        private AndroidBitmap _nativeBitmap;

        public BitmapRenderer(
            global::Android.Content.Context context,
            Bitmap bitmap)
            : base(context, bitmap)
        {
        }

        public AndroidBitmap NativeBitmap
        {
            get
            {
                return this._nativeBitmap;
            }
        }

        public async Task LoadAsync()
        {
            var bitmap = (Bitmap)this.Element;

            var fileImageSource = (FileImageSource)bitmap.Source;

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

            this._nativeBitmap = nativeBitmap;

            return;
        }
    }
}
