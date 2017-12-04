using System;

namespace XForms.Controls
{
    public interface ITextViewRenderer : IViewRenderer
    {
        string Text { get; set; }

        TextAlignment HorizontalTextAlignment { get; set; }

        TextAlignment VerticalTextAlignment { get; set; }

        bool WordWrap { get; set; }

        bool TextTrimming { get; set; }

        Color ForegroundColor { get; set; }

        Color BackgroundColor { get; set; }

        float FontSize { get; set; }

        FontStyle FontStyle { get; set; }

        FontWeight FontWeight { get; set; }

        bool Strikethrough { get; set; }
    }

    public class TextView : View
    {
        public TextView()
        {
        }

        public string Text
        {
            get
            {
                return this.Renderer.Text;
            }

            set
            {
                this.Renderer.Text = value;
            }
        }

        public TextAlignment HorizontalTextAlignment
        {
            get
            {
                return this.Renderer.HorizontalTextAlignment;
            }

            set
            {
                this.Renderer.HorizontalTextAlignment = value;
            }
        }

        public TextAlignment VerticalTextAlignment
        {
            get
            {
                return this.Renderer.VerticalTextAlignment;
            }

            set
            {
                this.Renderer.VerticalTextAlignment = value;
            }
        }

        public bool WordWrap
        {
            get
            {
                return this.Renderer.WordWrap;
            }

            set
            {
                this.Renderer.WordWrap = value;
            }
        }

        public bool TextTrimming
        {
            get
            {
                return this.Renderer.TextTrimming;
            }

            set
            {
                this.Renderer.TextTrimming = value;
            }
        }

        public Color ForegroundColor
        {
            get
            {
                return this.Renderer.ForegroundColor;
            }

            set
            {
                this.Renderer.ForegroundColor = value;
            }
        }

        public Color BackgroundColor
        {
            get
            {
                return this.Renderer.BackgroundColor;
            }

            set
            {
                this.Renderer.BackgroundColor = value;
            }
        }

        public float FontSize
        {
            get
            {
                return this.Renderer.FontSize;
            }

            set
            {
                this.Renderer.FontSize = value;
            }
        }

        public FontStyle FontStyle
        {
            get
            {
                return this.Renderer.FontStyle;
            }

            set
            {
                this.Renderer.FontStyle = value;
            }
        }

        public FontWeight FontWeight
        {
            get
            {
                return this.Renderer.FontWeight;
            }

            set
            {
                this.Renderer.FontWeight = value;
            }
        }

        public new ITextViewRenderer Renderer
        {
            get
            {
                return (ITextViewRenderer)base.Renderer;
            }
        }

        public bool Strikethrough
        {
            get
            {
                return this.Renderer.Strikethrough;
            }

            set
            {
                this.Renderer.Strikethrough = value;
            }
        }

        protected override IElementRenderer CreateRenderer()
        {
            return this.Application.Platform.CreateLabelRenderer(this);
        }
    }
}
