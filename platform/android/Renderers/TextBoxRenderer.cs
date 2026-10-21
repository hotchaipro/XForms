using System;
using XForms.Primitives;
using XForms.Platform;
using AndroidTextEntry = global::Android.Widget.EditText;

namespace XForms.Android.Renderers
{
    public class TextBoxRenderer : ViewRenderer<AndroidTextEntry>, ITextBoxRenderer
    {
        private NativeEditText _nativeTextEntry;
        private bool _multiline;
        private KeyboardOptions _keyboardOptions = KeyboardOptions.All;
        private bool _isPassword;
        private bool _isReadOnly;
        private global::Android.Text.Method.IKeyListener _keyListener;

        public TextBoxRenderer(
            global::Android.Content.Context context,
            TextBox textEntry)
            : base(context, textEntry)
        {
            var textBoxDelegate = (ITextBoxDelegate)textEntry;

            this._nativeTextEntry = new NativeEditText(context);
            this._keyListener = this._nativeTextEntry.KeyListener;
            this.ApplyInputType();

            //this._nativeTextView.RevealOnFocusHint = true;

            this._nativeTextEntry.AfterTextChanged += (sender, e) => textBoxDelegate.OnNativeTextChanged();
            this._nativeTextEntry.SelectionChanged += (sender, e) => textBoxDelegate.OnNativeSelectionChanged();

            // Enter, or the keyboard's Done key, submits a single-line text box
            this._nativeTextEntry.EditorAction += (sender, e) =>
            {
                if (!this._multiline)
                {
                    textBoxDelegate.OnNativeSubmitted();
                }

                // NOTE: Not handled, so Done also closes the keyboard
                e.Handled = false;
            };

            this.SetNativeElement(this._nativeTextEntry);
        }

        // The input type from the text box's options: the keyboard, suggestions
        // and line breaks it offers
        private void ApplyInputType()
        {
            var edit = this._nativeTextEntry;
            var typeface = edit.Typeface;

            global::Android.Text.InputTypes type = global::Android.Text.InputTypes.ClassText;
            if (this._isPassword)
            {
                type |= global::Android.Text.InputTypes.TextVariationPassword;
            }
            else
            {
                if (this._multiline)
                {
                    type |= global::Android.Text.InputTypes.TextFlagMultiLine;
                }

                if (this._keyboardOptions.HasFlag(KeyboardOptions.SpellCheck))
                {
                    type |= global::Android.Text.InputTypes.TextFlagAutoCorrect;
                }

                if (!this._keyboardOptions.HasFlag(KeyboardOptions.Suggestions))
                {
                    type |= global::Android.Text.InputTypes.TextFlagNoSuggestions;
                }
            }

            edit.InputType = type;
            edit.ImeOptions = this._multiline ? global::Android.Views.InputMethods.ImeAction.None : global::Android.Views.InputMethods.ImeAction.Done;

            // NOTE: A password input type switches to a monospace font
            edit.Typeface = typeface;
        }

        public string Text
        {
            get
            {
                return this._nativeTextEntry.Text;
            }

            set
            {
                this._nativeTextEntry.Text = value;
            }
        }

        public string PlaceholderText
        {
            get
            {
                return this._nativeTextEntry.Hint;
            }

            set
            {
                this._nativeTextEntry.Hint = value;
            }
        }

        public bool Multiline
        {
            get
            {
                return this._multiline;
            }

            set
            {
                this._multiline = value;
                this.ApplyInputType();
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
                this.ApplyInputType();
            }
        }

        public bool IsPassword
        {
            set
            {
                this._isPassword = value;
                this.ApplyInputType();
            }
        }

        public int MaxLength
        {
            set
            {
                this._nativeTextEntry.SetFilters((0 < value)
                    ? new global::Android.Text.IInputFilter[] { new global::Android.Text.InputFilterLengthFilter(value) }
                    : new global::Android.Text.IInputFilter[0]);
            }
        }

        public bool IsReadOnly
        {
            set
            {
                // Selectable, but without a key listener nothing changes the text
                this._isReadOnly = value;
                this._nativeTextEntry.KeyListener = value ? null : this._keyListener;
                this._nativeTextEntry.ShowSoftInputOnFocus = !value;
            }
        }

        public int SelectionStart
        {
            get
            {
                return Math.Max(0, Math.Min(this._nativeTextEntry.SelectionStart, this._nativeTextEntry.SelectionEnd));
            }
        }

        public int SelectionLength
        {
            get
            {
                return Math.Abs(this._nativeTextEntry.SelectionEnd - this._nativeTextEntry.SelectionStart);
            }
        }

        public void Select(
            int start,
            int length)
        {
            int textLength = this._nativeTextEntry.Text?.Length ?? 0;
            start = Math.Min(start, textLength);
            this._nativeTextEntry.SetSelection(start, Math.Min(textLength, start + length));
        }

        public Color ForegroundColor
        {
            set
            {
                this._nativeTextEntry.SetTextColor(value.ToAndroidColor());
            }
        }

        public Color PlaceholderColor
        {
            set
            {
                this._nativeTextEntry.SetHintTextColor(value.ToAndroidColor());
            }
        }

        public Color AccentColor
        {
            set
            {
                // NOTE: Before Android 10, the caret is the app theme's accent color
                if (OperatingSystem.IsAndroidVersionAtLeast(29))
                {
                    var cursor = this._nativeTextEntry.TextCursorDrawable?.Mutate();
                    if (null != cursor)
                    {
                        cursor.SetTint(value.ToAndroidColor());
                        this._nativeTextEntry.TextCursorDrawable = cursor;
                    }
                }
            }
        }

        public void Focus()
        {
            this._nativeTextEntry.RequestFocus();
        }
    }

    /// <summary>
    /// An EditText that reports selection changes, which EditText has no
    /// event for.
    /// </summary>
    internal sealed class NativeEditText : AndroidTextEntry
    {
        public NativeEditText(
            global::Android.Content.Context context)
            : base(context)
        {
        }

        public event EventHandler SelectionChanged;

        protected override void OnSelectionChanged(
            int selStart,
            int selEnd)
        {
            base.OnSelectionChanged(selStart, selEnd);

            // NOTE: Called from EditText's constructor too, before this is set up
            this.SelectionChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
