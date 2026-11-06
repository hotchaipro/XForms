using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using XForma.Platform;

namespace XForma
{
    /// <summary>
    /// An image to draw or show, loaded from a file or from pixels.
    /// </summary>
    public class Bitmap : Element
    {
        public Bitmap(
            ImageSource source)
        {
            if (null == source)
            {
                throw new ArgumentNullException(nameof(source));
            }

            this.Source = source;
        }

        private bool _wasDrawnBeforeLoaded;
        private Task? _loading;

        /// <summary>
        /// Loads the image. One drawn before it's loaded draws nothing, and the
        /// views draw again once it is.
        /// </summary>
        public Task? LoadAsync()
        {
            return this._loading ??= this.LoadCoreAsync();
        }

        private async Task LoadCoreAsync()
        {
            var loading = this.Renderer?.LoadAsync();
            if (null != loading)
            {
                await loading;
            }

            if (this._wasDrawnBeforeLoaded)
            {
                this._wasDrawnBeforeLoaded = false;
                var application = Application.Current;
                application?.Dispatcher.Post(application.InvalidateAllViews);
            }
        }

        /// <summary>
        /// A view drew the bitmap before its image had loaded, on a platform that
        /// loads it asynchronously: it loads, if nothing's asked yet, and the
        /// views draw again.
        /// </summary>
        [PlatformContract]
        internal void OnDrawnBeforeLoaded()
        {
            this._wasDrawnBeforeLoaded = true;
            _ = this.LoadInBackgroundAsync();
        }

        private async Task LoadInBackgroundAsync()
        {
            try
            {
                await (this.LoadAsync() ?? Task.CompletedTask);
            }
            catch (Exception ex)
            {
                FrameworkLog.Write(LogLevel.Warning, "An image didn't load.", ex);
            }
        }

        public ImageSource Source
        {
            get;
            private set;
        }

        [PlatformContract]
        internal new IBitmapRenderer? Renderer
        {
            get
            {
                return (IBitmapRenderer)base.Renderer;
            }
        }

        private protected override IElementRenderer CreateRenderer()
        {
            return this.Application.Platform.CreateBitmapRenderer(this);
        }

        internal override IEnumerable<Element> GetChildElements()
        {
            yield break;
        }
    }
}
