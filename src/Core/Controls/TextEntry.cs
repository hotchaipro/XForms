using System;

namespace XForms.Controls
{
    [Flags]
    public enum KeyboardOptions
    {
        None = 0,
        SpellCheck = 1,
        Suggestions = 2,
        All = SpellCheck | Suggestions,
    }

    public interface ITextEntryRenderer : IControlRenderer
    {
        string Text { get; set; }
        string PlaceholderText { get; set; }
        bool Multiline { get; set; }
        KeyboardOptions KeyboardOptions { get; set; }
        void Focus();
    }

    public class TextEntry : Control
    {
        public TextEntry()
        {
            this.HorizontalAlignment = LayoutAlignment.Start;
            this.VerticalAlignment = LayoutAlignment.Start;
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

        public string PlaceholderText
        {
            get
            {
                return this.Renderer.PlaceholderText;
            }

            set
            {
                this.Renderer.PlaceholderText = value;
            }
        }

        public bool Multiline
        {
            get
            {
                return this.Renderer.Multiline;
            }

            set
            {
                this.Renderer.Multiline = value;
            }
        }

        public KeyboardOptions KeyboardOptions
        {
            get
            {
                return this.Renderer.KeyboardOptions;
            }

            set
            {
                this.Renderer.KeyboardOptions = value;
            }
        }

        public void Focus()
        {
            this.Renderer.Focus();
        }

        public new ITextEntryRenderer Renderer
        {
            get
            {
                return (ITextEntryRenderer)base.Renderer;
            }
        }

        protected override IElementRenderer CreateRenderer()
        {
            return this.Application.Platform.CreateTextEntryRenderer(this);
        }
    }
}
