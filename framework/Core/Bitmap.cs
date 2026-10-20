using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace XForms
{
    public interface IBitmapRenderer : IElementRenderer
    {
        Task LoadAsync();
    }

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

        public Task LoadAsync()
        {
            return this.Renderer?.LoadAsync();
        }

        public ImageSource Source
        {
            get;
            private set;
        }

        public new IBitmapRenderer Renderer
        {
            get
            {
                return (IBitmapRenderer)base.Renderer;
            }
        }

        protected override IElementRenderer CreateRenderer()
        {
            return this.Application.Platform.CreateBitmapRenderer(this);
        }

        internal override IEnumerable<Element> GetChildElements()
        {
            yield break;
        }
    }
}
