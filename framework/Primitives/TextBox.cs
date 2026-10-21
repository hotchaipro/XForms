using System;
using XForms.Platform;

namespace XForms.Primitives
{
    [Flags]
    public enum KeyboardOptions
    {
        None = 0,
        SpellCheck = 1,
        Suggestions = 2,
        All = SpellCheck | Suggestions,
    }

    public class TextBox : View, ITextBoxDelegate
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

        private Color? _backgroundColor;
        private Color? _foregroundColor;
        private Color? _placeholderColor;
        private Color? _accentColor;

        /// <summary>
        /// The text box's background: by default, the theme's surface color.
        /// </summary>
        public Color BackgroundColor
        {
            get
            {
                return this._backgroundColor ?? this.ActualTheme.Colors.Surface;
            }

            set
            {
                this._backgroundColor = value;
                this.ApplyColors();
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// The text's color: by default, the theme's color for text on its
        /// surface.
        /// </summary>
        public Color ForegroundColor
        {
            get
            {
                return this._foregroundColor ?? this.ActualTheme.Colors.OnSurface;
            }

            set
            {
                this._foregroundColor = value;
                this.ApplyColors();
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// The placeholder text's color: by default, the theme's subtle
        /// foreground color.
        /// </summary>
        public Color PlaceholderColor
        {
            get
            {
                return this._placeholderColor ?? this.ActualTheme.Colors.SubtleForeground;
            }

            set
            {
                this._placeholderColor = value;
                this.ApplyColors();
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// The caret's color: by default, the theme's accent color.
        /// </summary>
        public Color AccentColor
        {
            get
            {
                return this._accentColor ?? this.ActualTheme.Colors.Accent;
            }

            set
            {
                this._accentColor = value;
                this.ApplyColors();
                this.OnPropertyChanged();
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
        private bool _isPassword;
        private int _maxLength;
        private bool _isReadOnly;

        // While the native object receives the text box's values, its own
        // change notifications aren't the user's
        private bool _isApplyingValues;

        // The selection to apply once the native object exists
        private int _selectionStart;
        private int _selectionLength;

        /// <summary>
        /// Raised when the text changes, whether the user or code changed it.
        /// </summary>
        public event EventHandler TextChanged;

        /// <summary>
        /// Raised when the user submits a single-line text box: Enter, or the
        /// on-screen keyboard's Done or Go key. A text box that accepts Return
        /// never submits.
        /// </summary>
        public event EventHandler Submitted;

        /// <summary>
        /// Raised when the selection or the caret moves.
        /// </summary>
        public event EventHandler SelectionChanged;

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
            if (this._isApplyingValues)
            {
                return;
            }
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

        void ITextBoxDelegate.OnNativeSubmitted()
        {
            if (!this._acceptsReturn)
            {
                this.Submitted?.Invoke(this, EventArgs.Empty);
            }
        }

        void ITextBoxDelegate.OnNativeSelectionChanged()
        {
            if (this._isApplyingValues)
            {
                return;
            }

            var native = this.Native;
            if (null != native)
            {
                this._selectionStart = native.SelectionStart;
                this._selectionLength = native.SelectionLength;
            }

            this.SelectionChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Shows the text as dots (as WPF's and WinUI's <c>PasswordBox</c> and
        /// MAUI's <c>Entry.IsPassword</c>). A password box is single-line and
        /// offers no suggestions.
        /// </summary>
        public bool IsPassword
        {
            get
            {
                return this._isPassword;
            }

            set
            {
                this._isPassword = value;
                this.Native?.IsPassword = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// The most characters the user can type or paste, or 0 (the default)
        /// for no limit.
        /// </summary>
        public int MaxLength
        {
            get
            {
                return this._maxLength;
            }

            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                this._maxLength = value;
                this.Native?.MaxLength = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Whether the user can select and copy the text but not change it.
        /// </summary>
        public bool IsReadOnly
        {
            get
            {
                return this._isReadOnly;
            }

            set
            {
                this._isReadOnly = value;
                this.Native?.IsReadOnly = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Where the selection starts (the caret, when nothing is selected), in
        /// UTF-16 code units.
        /// </summary>
        public int SelectionStart
        {
            get
            {
                return this.Native?.SelectionStart ?? this._selectionStart;
            }

            set
            {
                this.Select(value, this.SelectionLength);
            }
        }

        public int SelectionLength
        {
            get
            {
                return this.Native?.SelectionLength ?? this._selectionLength;
            }

            set
            {
                this.Select(this.SelectionStart, value);
            }
        }

        /// <summary>
        /// The selected text, or an empty string.
        /// </summary>
        public string SelectedText
        {
            get
            {
                string text = this._text ?? string.Empty;
                int start = Math.Min(this.SelectionStart, text.Length);
                int length = Math.Min(this.SelectionLength, text.Length - start);
                return text.Substring(start, length);
            }
        }

        /// <summary>
        /// Selects a range of the text (clamped to it); a length of 0 moves the
        /// caret.
        /// </summary>
        public void Select(
            int start,
            int length)
        {
            if ((start < 0) || (length < 0))
            {
                throw new ArgumentOutOfRangeException((start < 0) ? nameof(start) : nameof(length));
            }

            int textLength = (this._text ?? string.Empty).Length;
            start = Math.Min(start, textLength);
            length = Math.Min(length, textLength - start);

            this._selectionStart = start;
            this._selectionLength = length;

            var native = this.Native;
            if (null != native)
            {
                native.Select(start, length);
            }
            else
            {
                this.SelectionChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public void SelectAll()
        {
            this.Select(0, (this._text ?? string.Empty).Length);
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

        [PlatformContract]
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

        // The platforms render it natively
        internal override bool IsLightweightType
        {
            get
            {
                return false;
            }
        }

        protected override void OnThemeChanged(
            Theme theme)
        {
            base.OnThemeChanged(theme);
            this.ApplyColors();
        }

        private void ApplyColors()
        {
            var native = this.Native;
            if (null != native)
            {
                native.BackgroundColor = this.BackgroundColor;
                native.ForegroundColor = this.ForegroundColor;
                native.PlaceholderColor = this.PlaceholderColor;
                native.AccentColor = this.AccentColor;
            }
        }

        internal override void OnRendererCreated()
        {
            base.OnRendererCreated();

            var native = this.Native;
            this.ApplyColors();
            this._isApplyingValues = true;
            try
            {
                if (this._acceptsReturn) native.Multiline = true;
                if (this._keyboardOptions.HasValue) native.KeyboardOptions = this._keyboardOptions.Value;
                if (null != this._placeholderText) native.PlaceholderText = this._placeholderText;
                if (this._isPassword) native.IsPassword = true;
                if (0 < this._maxLength) native.MaxLength = this._maxLength;
                if (this._isReadOnly) native.IsReadOnly = true;
                if (null != this._text) native.Text = this._text;
                if ((0 < this._selectionStart) || (0 < this._selectionLength)) native.Select(this._selectionStart, this._selectionLength);
            }
            finally
            {
                this._isApplyingValues = false;
            }
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
