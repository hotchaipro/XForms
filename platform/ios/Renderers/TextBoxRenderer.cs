using System;
using System.Collections.Generic;
using Foundation;
using UIKit;
using XForms.Primitives;
using XForms.Platform;

namespace XForms.iOS.Renderers
{
    /// <summary>
    /// Hosts a native UIKit editor so text entry keeps the platform's full editing
    /// behavior: selection, loupe, autocorrect, dictation, text replacement and
    /// Dynamic Type. Single-line entry uses UITextField; multiline uses UITextView.
    /// </summary>
    internal class TextBoxRenderer : ViewRenderer<TextBoxRenderer.NativeTextEntryView>, ITextBoxRenderer
    {
        private readonly NativeTextEntryView _container;
        private readonly UILabel _placeholderLabel;
        private UITextField _textField;
        private UITextView _textView;
        private KeyboardOptions _keyboardOptions;
        private UIColor _textColor;
        private UIColor _tintColor;
        private bool _isPassword;
        private int _maxLength;
        private bool _isReadOnly;
        private readonly HashSet<Key> _offeredKeys = new HashSet<Key>();

        public TextBoxRenderer(
            TextBox textEntry)
            : base(textEntry)
        {
            this._container = new NativeTextEntryView() { Owner = textEntry };
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

                this.ApplyPlaceholder();
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

        // The editor takes focus, not its container (Tab moving into the box
        // comes here)
        public override bool Focus()
        {
            return this.Editor.BecomeFirstResponder();
        }

        void ITextBoxRenderer.Focus()
        {
            this.Focus();
        }

        public Color ForegroundColor
        {
            set
            {
                this._textColor = value.ToUIColor();
                this.ApplyColors();
            }
        }

        public Color PlaceholderColor
        {
            set
            {
                this._placeholderLabel.TextColor = value.ToUIColor();
                this.ApplyPlaceholder();
            }
        }

        public Color AccentColor
        {
            set
            {
                this._tintColor = value.ToUIColor();
                this.ApplyColors();
            }
        }

        private static UIFont CreateFont()
        {
            return UIFont.GetPreferredFontForTextStyle(UIFontTextStyle.Body);
        }

        private void CreateTextField(
            string text,
            string placeholderText)
        {
            var textField = new NativeTextField(this)
            {
                Font = CreateFont(),
                AdjustsFontForContentSizeCategory = true,
                BorderStyle = UITextBorderStyle.None,
                Text = text,
                ReturnKeyType = UIReturnKeyType.Done,
            };

            textField.EditingChanged += (sender, e) => ((ITextBoxDelegate)this.Element).OnNativeTextChanged();
            textField.EditingDidBegin += (sender, e) => this.OnNativeFocusChanged(true);
            textField.EditingDidEnd += (sender, e) => this.OnNativeFocusChanged(false);

            // The on-screen keyboard's Done key submits and puts the keyboard
            // away (a hardware keyboard's Return is NativeTextField's)
            textField.ShouldReturn = (field) =>
            {
                ((ITextBoxDelegate)this.Element).OnNativeSubmitted();
                field.ResignFirstResponder();
                return true;
            };

            textField.ShouldChangeCharacters = (field, range, replacement) => this.CanChange(field.Text, range, replacement);
            textField.DidChangeSelection += (sender, e) => ((ITextBoxDelegate)this.Element).OnNativeSelectionChanged();

            this._textField = textField;
            this.ApplyPlaceholder();

            this.AddEditor(textField);
        }

        private void CreateTextView(
            string text)
        {
            var textView = new NativeTextView(this)
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
            textView.Started += (sender, e) => this.OnNativeFocusChanged(true);
            textView.Ended += (sender, e) => this.OnNativeFocusChanged(false);
            textView.ShouldChangeText = (view, range, replacement) => this.CanChange(view.Text, range, replacement);
            textView.SelectionChanged += (sender, e) => ((ITextBoxDelegate)this.Element).OnNativeSelectionChanged();

            this._textView = textView;

            this.AddEditor(textView);

            // UITextView has no placeholder of its own
            this._container.Placeholder = this._placeholderLabel;
            this.UpdatePlaceholderVisibility();
            this.ApplyPlaceholder();
        }

        // Editing starts and ends with keyboard focus: the view's IsFocused and
        // its focus events follow it
        private void OnNativeFocusChanged(
            bool hasFocus)
        {
            if (this.Element is View view)
            {
                LayerHost.For(view).NativeFocusChanged(hasFocus, isBackward: false);
            }

            if (hasFocus)
            {
                FrameRenderer.TextEntryFocused(this.Editor?.Window);
            }
            else
            {
                FrameRenderer.KeyboardFocusLost(this.Editor?.Window);
            }
        }

        // Keys from a hardware keyboard, before the editor sees them: Tab moves
        // focus, and a single-line box offers the keys it has no use for (Up,
        // Down, Page Up, Page Down and Escape) to its ancestors first, for a
        // suggestion list or a number box. Returns whether they were used.
        private bool OnPresses(
            NSSet<UIPress> presses,
            bool isDown)
        {
            if (!(this.Element is TextBox textBox))
            {
                return false;
            }

            bool handled = false;
            foreach (var press in presses)
            {
                var uiKey = press.Key;
                if (null == uiKey)
                {
                    continue;
                }

                var key = NativeLayoutView.ToKey(uiKey.KeyCode);
                var modifiers = NativeLayoutView.ToKeyModifiers(uiKey.ModifierFlags);
                if ((key == Key.Tab) && ((modifiers & ~KeyModifiers.Shift) == KeyModifiers.None))
                {
                    bool isForward = !modifiers.HasFlag(KeyModifiers.Shift);
                    if (isDown && !PopupRenderer.MoveFocusIntoModal(textBox, isForward))
                    {
                        LayerHost.For(textBox).MoveFocus(isForward);
                    }

                    handled = true;
                }
                else if (!this.Multiline && View.IsTextBoxNavigationKey(key))
                {
                    if (isDown)
                    {
                        if (this.OfferToAncestors(textBox, new KeyEventArgs(key, modifiers)))
                        {
                            this._offeredKeys.Add(key);
                            handled = true;
                        }
                    }
                    else
                    {
                        handled = this._offeredKeys.Remove(key);
                    }
                }
            }

            return handled;
        }

        private bool OfferToAncestors(
            TextBox textBox,
            KeyEventArgs keyEvent)
        {
            for (var view = textBox.Parent as View; null != view; view = view.Parent as View)
            {
                if (LayerHost.For(view).DispatchKeyDown(keyEvent))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsReturn(
            NSSet<UIPress> presses)
        {
            foreach (var press in presses)
            {
                var code = press.Key?.KeyCode;
                if ((code == UIKeyboardHidUsage.KeyboardReturnOrEnter) || (code == UIKeyboardHidUsage.KeypadEnter))
                {
                    return true;
                }
            }

            return false;
        }

        private void AddEditor(
            UIView editor)
        {
            this._container.Editor = editor;

            this.ApplyKeyboardOptions();
            this.ApplyColors();
            this.ApplyEditOptions();
        }

        // Whether an edit may happen: not while read-only, nor past the
        // maximum length
        private bool CanChange(
            string text,
            Foundation.NSRange range,
            string replacement)
        {
            if (this._isReadOnly)
            {
                return false;
            }

            if (0 < this._maxLength)
            {
                int length = (text?.Length ?? 0) - (int)range.Length + (replacement?.Length ?? 0);
                return length <= this._maxLength;
            }

            return true;
        }

        private void ApplyEditOptions()
        {
            if (null != this._textField)
            {
                this._textField.SecureTextEntry = this._isPassword;
            }
            else
            {
                this._textView.Editable = !this._isReadOnly;
            }
        }

        public bool IsPassword
        {
            set
            {
                this._isPassword = value;
                this.ApplyEditOptions();
            }
        }

        public int MaxLength
        {
            set
            {
                this._maxLength = value;
            }
        }

        public bool IsReadOnly
        {
            set
            {
                this._isReadOnly = value;
                this.ApplyEditOptions();
            }
        }

        private IUITextInput TextInput
        {
            get
            {
                return (IUITextInput)this._textField ?? this._textView;
            }
        }

        public int SelectionStart
        {
            get
            {
                var input = this.TextInput;
                var range = input.SelectedTextRange;
                return (null == range) ? 0 : (int)input.GetOffsetFromPosition(input.BeginningOfDocument, range.Start);
            }
        }

        public int SelectionLength
        {
            get
            {
                var input = this.TextInput;
                var range = input.SelectedTextRange;
                return (null == range) ? 0 : (int)input.GetOffsetFromPosition(range.Start, range.End);
            }
        }

        public void Select(
            int start,
            int length)
        {
            var input = this.TextInput;
            var from = input.GetPosition(input.BeginningOfDocument, start);
            var to = input.GetPosition(input.BeginningOfDocument, start + length);
            if ((null != from) && (null != to))
            {
                input.SelectedTextRange = input.GetTextRange(from, to);
            }
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

        // A single-line field's placeholder, in the theme's color (as the
        // multiline one's label), not UIKit's
        private void ApplyPlaceholder()
        {
            string text = this._placeholderLabel.Text;
            if (null == this._textField)
            {
                // VoiceOver reads a field's placeholder as its name; a text
                // view's label, drawn over it, is named the same way
                this._placeholderLabel.IsAccessibilityElement = false;
                if (null != this._textView)
                {
                    this._textView.AccessibilityLabel = text;
                }

                return;
            }

            this._textField.AttributedPlaceholder = string.IsNullOrEmpty(text)
                ? null
                : new NSAttributedString(text, foregroundColor: this._placeholderLabel.TextColor);
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

            ((ITextBoxDelegate)this.Element).OnNativeTextChanged();
        }

        private void UpdatePlaceholderVisibility()
        {
            this._placeholderLabel.Hidden = !String.IsNullOrEmpty(this._textView?.Text);
        }

        // The editors, with hardware keys going through the renderer first
        private sealed class NativeTextField : UITextField
        {
            private readonly WeakReference<TextBoxRenderer> _renderer;

            public NativeTextField(
                TextBoxRenderer renderer)
            {
                this._renderer = new WeakReference<TextBoxRenderer>(renderer);
            }

            private bool _isReturnDown;

            public override void PressesBegan(
                NSSet<UIPress> presses,
                UIPressesEvent evt)
            {
                if (!this._renderer.TryGetTarget(out var renderer) || renderer.OnPresses(presses, isDown: true))
                {
                    return;
                }

                // A hardware keyboard's Return submits and keeps focus, as on
                // the desktop (unless it ends an input method's composition)
                if (IsReturn(presses) && (null == this.MarkedTextRange))
                {
                    this._isReturnDown = true;
                    ((ITextBoxDelegate)renderer.Element).OnNativeSubmitted();
                    return;
                }

                base.PressesBegan(presses, evt);
            }

            public override void PressesEnded(
                NSSet<UIPress> presses,
                UIPressesEvent evt)
            {
                if (this._isReturnDown && IsReturn(presses))
                {
                    this._isReturnDown = false;
                    return;
                }

                if (!this._renderer.TryGetTarget(out var renderer) || !renderer.OnPresses(presses, isDown: false))
                {
                    base.PressesEnded(presses, evt);
                }
            }
        }

        private sealed class NativeTextView : UITextView
        {
            private readonly WeakReference<TextBoxRenderer> _renderer;

            public NativeTextView(
                TextBoxRenderer renderer)
            {
                this._renderer = new WeakReference<TextBoxRenderer>(renderer);
            }

            public override void PressesBegan(
                NSSet<UIPress> presses,
                UIPressesEvent evt)
            {
                if (!this._renderer.TryGetTarget(out var renderer) || !renderer.OnPresses(presses, isDown: true))
                {
                    base.PressesBegan(presses, evt);
                }
            }

            public override void PressesEnded(
                NSSet<UIPress> presses,
                UIPressesEvent evt)
            {
                if (!this._renderer.TryGetTarget(out var renderer) || !renderer.OnPresses(presses, isDown: false))
                {
                    base.PressesEnded(presses, evt);
                }
            }
        }

        /// <summary>
        /// Hosts the native editor (and, for multiline text, a placeholder) at
        /// full width, sized to the editor's content.
        /// </summary>
        internal sealed class NativeTextEntryView : UIView
        {
            private UIView _editor;
            private UIView _placeholder;

            public TextBox Owner
            {
                get;
                set;
            }

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
