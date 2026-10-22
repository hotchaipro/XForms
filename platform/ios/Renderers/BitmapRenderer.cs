using System;
using System.IO;
using System.Threading.Tasks;
using UIKit;
using XForms.Platform;

namespace XForms.iOS.Renderers
{
    internal class BitmapRenderer : ElementRenderer<UIImage>, IBitmapRenderer
    {
        private UIImage _nativeImage;

        public BitmapRenderer(
            Bitmap bitmap)
            : base(bitmap)
        {
        }

        public UIImage NativeImage
        {
            get
            {
                return this._nativeImage;
            }
        }

        public async Task LoadAsync()
        {
            var bitmap = (Bitmap)this.Element;

            var fileImageSource = (FileImageSource)bitmap.Source;

            await Task.Run(() =>
            {
                string resourceName = Path.GetFileName(fileImageSource.Path).ToLowerInvariant();
                this._nativeImage = UIImage.FromBundle(resourceName);
            });

            return;
        }
    }
}
