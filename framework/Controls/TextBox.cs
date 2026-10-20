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

    public interface ITextBoxRenderer : IControlRenderer
    {
        string Text { get; set; }
        string PlaceholderText { get; set; }
        bool Multiline { get; set; }
        KeyboardOptions KeyboardOptions { get; set; }
        void Focus();
    }

    public class TextBox : Control
    {
        protected override AccessibilityRole DefaultAccessibilityRole
        {
            get
            {
                return AccessibilityRole.TextBox;
            }
        }

        protected override string DefaultAccessibilityLabel
        {
            get
            {
                return this.PlaceholderText;
            }
        }

        public TextBox()
        {
            this.HorizontalAlignment = HorizontalAlignment.Left;
            this.VerticalAlignment = VerticalAlignment.Top;
        }

        private string _text;
        private string _placeholderText;
        private bool _acceptsReturn;
        private KeyboardOptions? _keyboardOptions;
        private bool _focusWhenAttached;

        public string Text
        {
            get
            {
                // The user edits the native text, so read it once it exists
                return this.Native?.Text ?? this._text;
            }

            set
            {
                this._text = value;
                this.Native?.Text = value;
            }
        }

        public string PlaceholderText
        {
            get
            {
                return this._placeholderText;
            }

            set
            {
                this._placeholderText = value;
                this.Native?.PlaceholderText = value;
            }
        }

        public bool AcceptsReturn
        {
            get
            {
                return this._acceptsReturn;
            }

            set
            {
                this._acceptsReturn = value;
                this.Native?.Multiline = value;
                this.InvalidateMeasure();
            }
        }

        public KeyboardOptions KeyboardOptions
        {
            get
            {
                return this._keyboardOptions ?? this.Native?.KeyboardOptions ?? KeyboardOptions.All;
            }

            set
            {
                this._keyboardOptions = value;
                this.Native?.KeyboardOptions = value;
            }
        }

        public new void Focus()
        {
            var native = this.Native;
            if (null != native)
            {
                native.Focus();
            }
            else
            {
                this._focusWhenAttached = true;
            }
        }

        internal new ITextBoxRenderer Renderer
        {
            get
            {
                return (ITextBoxRenderer)base.Renderer;
            }
        }

        private ITextBoxRenderer Native
        {
            get
            {
                return (ITextBoxRenderer)this.CreatedRenderer;
            }
        }

        internal override void OnRendererCreated()
        {
            base.OnRendererCreated();

            var native = this.Native;
            if (this._acceptsReturn) native.Multiline = true;
            if (this._keyboardOptions.HasValue) native.KeyboardOptions = this._keyboardOptions.Value;
            if (null != this._placeholderText) native.PlaceholderText = this._placeholderText;
            if (null != this._text) native.Text = this._text;
        }

        protected override void OnAttached()
        {
            base.OnAttached();

            if (this._focusWhenAttached)
            {
                this._focusWhenAttached = false;
                this.Native?.Focus();
            }
        }

        protected override IElementRenderer CreateRenderer()
        {
            return this.Application.Platform.CreateTextBoxRenderer(this);
        }
    }
}
