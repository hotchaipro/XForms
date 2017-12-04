using System;
using System.Threading.Tasks;
using Microsoft.Graphics.Canvas;
using XForms.Controls;

namespace XForms.Windows.Renderers
{
    public class ImageRenderer : ViewRenderer, IImageRenderer, IDrawDelegate
    {
        private GraphicsManager _graphicsManager;
        private OwnerDrawControl _xamlImage;
        private ImageSource _imageSource;
        private CanvasBitmap _bitmap;
        private Color _tintColor;

        public ImageRenderer(
            XForms.Controls.Image imageControl)
            : base(imageControl)
        {
            this._xamlImage = new OwnerDrawControl(this);
            this.SetNativeElement(this._xamlImage);

            this._graphicsManager = GraphicsManager.Shared;
            this._graphicsManager.ReloadResources += GraphicsManager_ReloadResources;
            this._graphicsManager.Redraw += GraphicsManager_Redraw;
        }

        public void InvokeDrawBackground(
            IDrawContext drawContext,
            Rectangle bounds)
        {
            drawContext.Clear(Colors.Transparent);

            if (null != this._bitmap)
            {
                ((DrawContext)drawContext).DrawImage(
                    this._bitmap,
                    new Rectangle(0, 0, (float)this._xamlImage.ActualWidth, (float)this._xamlImage.ActualHeight),
                    this._tintColor);
            }
        }

        public void SetTint(
            Color tint)
        {
            this._tintColor = tint;

            this._xamlImage.Invalidate();
        }

        public async void SetSource(
            ImageSource source)
        {
            await this.LoadSourceAsync(source);
            this._imageSource = source;
            this._xamlImage.Invalidate();
        }

        private async Task LoadSourceAsync(
            ImageSource source)
        {
            var fileImageSource = source as FileImageSource;
            if (null != fileImageSource)
            {
                Uri imageUri = new Uri(new Uri("ms-appx:///Content/"), fileImageSource.Path);

                this._bitmap = await CanvasBitmap.LoadAsync(this._graphicsManager.GetCanvasDevice(), imageUri);
            }
            else
            {
                var bitmapImageSource = source as BitmapImageSource;
                if (null != bitmapImageSource)
                {
                    this._bitmap = (CanvasBitmap)bitmapImageSource.Bitmap.Renderer.NativeElement;
                }
                else
                {
                    throw new ArgumentException("Invalid image source.", nameof(source));
                }
            }
        }

        private void GraphicsManager_ReloadResources(
            object sender,
            EventArgs e)
        {
            if (null != this._imageSource)
            {
                this.LoadSourceAsync(this._imageSource).Wait();
            }
        }

        private void GraphicsManager_Redraw(
            object sender,
            EventArgs e)
        {
            this._xamlImage.Invalidate();
        }
    }
}
