using System;
using System.Threading.Tasks;
using Microsoft.Graphics.Canvas;

namespace XForms.Windows.Renderers
{
    public class BitmapRenderer : ElementRenderer, IBitmapRenderer
    {
        private GraphicsManager _graphicsManager;

        public BitmapRenderer(
            Bitmap bitmap)
            : base(bitmap)
        {
        }

        public new CanvasBitmap NativeElement
        {
            get
            {
                return (CanvasBitmap)base.NativeElement;
            }
        }

        public async Task LoadAsync()
        {
            if (null == this._graphicsManager)
            {
                this._graphicsManager = GraphicsManager.Shared;
                // TODO: Release event reference
                this._graphicsManager.ReloadResources += GraphicsManager_ReloadResources;
            }

            var bitmap = (Bitmap)this.Element;

            var fileImageSource = (FileImageSource)bitmap.Source;

            Uri imageUri = new Uri(new Uri("ms-appx:///Content/"), fileImageSource.Path);

            var xamlBitmap = await CanvasBitmap.LoadAsync(GraphicsManager.Shared.GetCanvasDevice(), imageUri);

            this.SetNativeElement(xamlBitmap);

            return;
        }

        private void GraphicsManager_ReloadResources(
            object sender,
            EventArgs e)
        {
            this.LoadAsync().Wait();
        }
    }
}
