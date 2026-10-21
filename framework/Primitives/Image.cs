using System;
using XForms.Platform;

namespace XForms.Primitives
{
    public class Image : View
    {
        protected override AccessibilityRole DefaultAccessibilityRole
        {
            get
            {
                return AccessibilityRole.Image;
            }
        }

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
                this.Native?.SetTint(value);

                this.OnPropertyChanged();
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
                this.Native?.SetSource(value);
                this.InvalidateMeasure();

                this.OnPropertyChanged();
            }
        }

        [PlatformContract]
        internal new IImageRenderer Renderer
        {
            get
            {
                return (IImageRenderer)base.Renderer;
            }
        }

        private IImageRenderer Native
        {
            get
            {
                return (IImageRenderer)this.CreatedRenderer;
            }
        }

        // The platforms render it natively
        internal override bool IsLightweightType
        {
            get
            {
                return false;
            }
        }

        internal override void OnRendererCreated()
        {
            base.OnRendererCreated();

            var native = this.Native;
            if (this._tint != Colors.Transparent)
            {
                native.SetTint(this._tint);
            }

            if (null != this._source)
            {
                native.SetSource(this._source);
            }
        }

        protected override IElementRenderer CreateRenderer()
        {
            return this.Application.Platform.CreateImageRenderer(this);
        }
    }
}
