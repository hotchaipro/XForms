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

    public interface ITextBoxDelegate
    {
        /// <summary>
        /// Called by the platform after the user changes the text.
        /// </summary>
        void OnNativeTextChanged();
    }

    public interface ITextBoxRenderer : IControlRenderer
    {
        string Text { get; set; }
        string PlaceholderText { get; set; }
        bool Multiline { get; set; }
        KeyboardOptions KeyboardOptions { get; set; }
        new void Focus();
    }

    public class TextBox : Control, ITextBoxDelegate
    {
        public static readonly ViewProperty<TextBox, string> TextProperty = new(nameof(Text), v => v.Text, (v, x) => v.Text = x);
        public static readonly ViewProperty<TextBox, string> PlaceholderTextProperty = new(nameof(PlaceholderText), v => v.PlaceholderText, (v, x) => v.PlaceholderText = x);
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

        public override Color BackgroundColor
        {
            get
            {
                return this.Theme.SubtleBackgroundColor;
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

        /// <summary>
        /// Raised when the text changes, whether the user or code changed it.
        /// </summary>
        public event EventHandler TextChanged;

        public string Text
        {
            get
            {
                return this._text;
            }

            set
            {
                if (value == this._text)
                {
                    return;
                }

                this._text = value;
                this.Native?.Text = value;
                this.OnTextChanged();
            }
        }

        private void OnTextChanged()
        {
            this.OnPropertyChanged(nameof(this.Text));
            this.TextChanged?.Invoke(this, EventArgs.Empty);
        }

        void ITextBoxDelegate.OnNativeTextChanged()
        {
            // NOTE: Setting the text from code also changes the native text;
            // only a different value is a change
            string text = this.Native?.Text ?? string.Empty;
            if (text == (this._text ?? string.Empty))
            {
                return;
            }

            this._text = text;
            this.OnTextChanged();
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

                this.OnPropertyChanged();
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

                this.OnPropertyChanged();
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

                this.OnPropertyChanged();
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
