using System;

namespace XForms.Controls
{
    public interface IImageRenderer : IViewRenderer
    {
        void SetTint(Color color);

        void SetSource(ImageSource source);
    }

    public class Image : View
    {
        private ImageSource _source;
        private Color _tint = Colors.Transparent;

        public Image()
        {
        }

        public Color Tint
        {
            get
            {
                return this._tint;
            }

            set
            {
                this._tint = value;

                this.Renderer.SetTint(value);
            }
        }

        public ImageSource Source
        {
            get
            {
                return this._source;
            }

            set
            {
                this._source = value;

                this.Renderer.SetSource(value);
            }
        }

        public new IImageRenderer Renderer
        {
            get
            {
                return (IImageRenderer)base.Renderer;
            }
        }

        protected override IElementRenderer CreateRenderer()
        {
            return this.Application.Platform.CreateImageRenderer(this);
        }
    }
}
