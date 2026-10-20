using System;
using UIKit;
using XForms.Controls;

namespace XForms.iOS.Renderers
{
    /// <summary>
    /// Hosts a native UIKit editor so text entry keeps the platform's full editing
    /// behavior: selection, loupe, autocorrect, dictation, text replacement and
    /// Dynamic Type. Single-line entry uses UITextField; multiline uses UITextView.
    /// </summary>
    internal class TextBoxRenderer : ControlRenderer<TextBoxRenderer.NativeTextEntryView>, ITextBoxRenderer
    {
        private readonly NativeTextEntryView _container;
        private readonly UILabel _placeholderLabel;
        private UITextField _textField;
        private UITextView _textView;
        private KeyboardOptions _keyboardOptions;
        private UIColor _textColor;
        private UIColor _tintColor;

        public TextBoxRenderer(
            TextBox textEntry)
            : base(textEntry)
        {
            this._container = new NativeTextEntryView();
            this._keyboardOptions = KeyboardOptions.All;
            this._textColor = UIColor.Label;

            this._placeholderLabel = new UILabel()
            {
                Font = CreateFont(),
                AdjustsFontForContentSizeCategory = true,
                TextColor = UIColor.PlaceholderText,
                Lines = 0,
                UserInteractionEnabled = false,
            };

            this.SetNativeElement(this._container);

            this.CreateTextField(String.Empty, null);
        }

        private UIView Editor
        {
            get
            {
                return (UIView)this._textField ?? this._textView;
            }
        }

        public string Text
        {
            get
            {
                return (null != this._textField) ? this._textField.Text : this._textView.Text;
            }

            set
            {
                value = value ?? String.Empty;

                if (null != this._textField)
                {
                    this._textField.Text = value;
                }
                else
                {
                    this._textView.Text = value;
                    this.UpdatePlaceholderVisibility();
                }

                this._container.RequestLayout();
            }
        }

        public string PlaceholderText
        {
            get
            {
                return this._placeholderLabel.Text;
            }

            set
            {
                this._placeholderLabel.Text = value;

                if (null != this._textField)
                {
                    this._textField.Placeholder = value;
                }

                this._container.RequestLayout();
            }
        }

        public bool Multiline
        {
            get
            {
                return (null != this._textView);
            }

            set
            {
                if (value == this.Multiline)
                {
                    return;
                }

                string text = this.Text;
                bool isFocused = this.Editor.IsFirstResponder;

                this.Editor.RemoveFromSuperview();

                if (value)
                {
                    this._textField = null;
                    this.CreateTextView(text);
                }
                else
                {
                    this._textView.Changed -= this.TextView_Changed;
                    this._textView = null;
                    this._container.Placeholder = null;
                    this.CreateTextField(text, this.PlaceholderText);
                }

                if (isFocused)
                {
                    this.Editor.BecomeFirstResponder();
                }
            }
        }

        public KeyboardOptions KeyboardOptions
        {
            get
            {
                return this._keyboardOptions;
            }

            set
            {
                this._keyboardOptions = value;

                this.ApplyKeyboardOptions();
            }
        }

        public void Focus()
        {
            this.Editor.BecomeFirstResponder();
        }

        public override void OnApplyTheme(
            AppTheme theme)
        {
            base.OnApplyTheme(theme);

            this._textColor = theme.ForegroundColor.ToUIColor();
            this._tintColor = theme.AccentColor.ToUIColor();
            this._placeholderLabel.TextColor = theme.SubtleForegroundColor.ToUIColor();

            this.ApplyColors();
        }

        private static UIFont CreateFont()
        {
            return UIFont.GetPreferredFontForTextStyle(UIFontTextStyle.Body);
        }

        private void CreateTextField(
            string text,
            string placeholderText)
        {
            var textField = new UITextField()
            {
                Font = CreateFont(),
                AdjustsFontForContentSizeCategory = true,
                BorderStyle = UITextBorderStyle.None,
                Text = text,
                Placeholder = placeholderText,
                ReturnKeyType = UIReturnKeyType.Done,
            };

            textField.ShouldReturn = (field) =>
            {
                field.ResignFirstResponder();
                return true;
            };

            this._textField = textField;

            this.AddEditor(textField);
        }

        private void CreateTextView(
            string text)
        {
            var textView = new UITextView()
            {
                Font = CreateFont(),
                AdjustsFontForContentSizeCategory = true,
                BackgroundColor = UIColor.Clear,
                Text = text,

                // Grow with the content instead of scrolling internally; the
                // surrounding XForms layout decides how much room the entry gets
                ScrollEnabled = false,
                TextContainerInset = UIEdgeInsets.Zero,
            };
            textView.TextContainer.LineFragmentPadding = 0;

            textView.Changed += this.TextView_Changed;

            this._textView = textView;

            this.AddEditor(textView);

            // UITextView has no placeholder of its own
            this._container.Placeholder = this._placeholderLabel;
            this.UpdatePlaceholderVisibility();
        }

        private void AddEditor(
            UIView editor)
        {
            this._container.Editor = editor;

            this.ApplyKeyboardOptions();
            this.ApplyColors();
        }

        private void ApplyKeyboardOptions()
        {
            var options = this._keyboardOptions;

            var spellChecking = options.HasFlag(KeyboardOptions.SpellCheck)
                ? UITextSpellCheckingType.Default
                : UITextSpellCheckingType.No;

            var autocorrection = options.HasFlag(KeyboardOptions.Suggestions)
                ? UITextAutocorrectionType.Default
                : UITextAutocorrectionType.No;

            if (null != this._textField)
            {
                this._textField.SpellCheckingType = spellChecking;
                this._textField.AutocorrectionType = autocorrection;
            }
            else
            {
                this._textView.SpellCheckingType = spellChecking;
                this._textView.AutocorrectionType = autocorrection;
            }
        }

        private void ApplyColors()
        {
            if (null != this._textField)
            {
                this._textField.TextColor = this._textColor;
                this._textField.TintColor = this._tintColor;
            }
            else
            {
                this._textView.TextColor = this._textColor;
                this._textView.TintColor = this._tintColor;
            }
        }

        private void TextView_Changed(
            object sender,
            EventArgs e)
        {
            this.UpdatePlaceholderVisibility();

            // The text view sizes to its content, so a new line may change its height
            this._container.RequestLayout();
        }

        private void UpdatePlaceholderVisibility()
        {
            this._placeholderLabel.Hidden = !String.IsNullOrEmpty(this._textView?.Text);
        }

        /// <summary>
        /// Hosts the native editor (and, for multiline text, a placeholder) at
        /// full width, sized to the editor's content.
        /// </summary>
        internal sealed class NativeTextEntryView : UIView
        {
            private UIView _editor;
            private UIView _placeholder;

            public UIView Editor
            {
                get
                {
                    return this._editor;
                }

                set
                {
                    this._editor?.RemoveFromSuperview();
                    this._editor = value;
                    if (null != value)
                    {
                        this.AddSubview(value);
                    }

                    this.SetNeedsLayout();
                }
            }

            public UIView Placeholder
            {
                get
                {
                    return this._placeholder;
                }

                set
                {
                    this._placeholder?.RemoveFromSuperview();
                    this._placeholder = value;
                    if (null != value)
                    {
                        this.AddSubview(value);
                    }

                    this.SetNeedsLayout();
                }
            }

            public override CoreGraphics.CGSize SizeThatFits(
                CoreGraphics.CGSize size)
            {
                var editorSize = this._editor?.SizeThatFits(size) ?? CoreGraphics.CGSize.Empty;
                var placeholderSize = this._placeholder?.SizeThatFits(size) ?? CoreGraphics.CGSize.Empty;

                return new CoreGraphics.CGSize(
                    Math.Max(editorSize.Width, placeholderSize.Width),
                    Math.Max(editorSize.Height, placeholderSize.Height));
            }

            public override void LayoutSubviews()
            {
                base.LayoutSubviews();

                var bounds = this.Bounds;
                var fittingSize = new CoreGraphics.CGSize(bounds.Width, nfloat.MaxValue);

                var editor = this._editor;
                if (null != editor)
                {
                    // A single-line field centers vertically; a text view starts at the top
                    var height = (nfloat)Math.Min(editor.SizeThatFits(fittingSize).Height, bounds.Height);
                    var top = (editor is UITextField) ? (bounds.Height - height) / 2 : 0;
                    editor.Frame = new CoreGraphics.CGRect(0, top, bounds.Width, height);
                }

                var placeholder = this._placeholder;
                if (null != placeholder)
                {
                    var height = (nfloat)Math.Min(placeholder.SizeThatFits(fittingSize).Height, bounds.Height);
                    placeholder.Frame = new CoreGraphics.CGRect(0, 0, bounds.Width, height);
                }
            }
        }
    }
}
