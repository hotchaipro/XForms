using System;
using XForms.Controls;
using AndroidTextEntry = global::Android.Widget.EditText;

namespace XForms.Android.Renderers
{
    public class TextEntryRenderer : ControlRenderer<AndroidTextEntry>, ITextEntryRenderer
    {
        private AndroidTextEntry _nativeTextEntry;

        public TextEntryRenderer(
            global::Android.Content.Context context,
            TextEntry textEntry)
            : base(context, textEntry)
        {
            this._nativeTextEntry = new AndroidTextEntry(context)
            {
                InputType = global::Android.Text.InputTypes.TextFlagAutoCorrect,
            };

            //this._nativeTextView.RevealOnFocusHint = true;

            this._nativeTextEntry.SetTextColor(global::Android.Graphics.Color.Green);
            this._nativeTextEntry.SetHintTextColor(global::Android.Graphics.Color.LightGray);

            this.SetNativeElement(this._nativeTextEntry);
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
                return this._nativeTextEntry.InputType.HasFlag(global::Android.Text.InputTypes.TextFlagMultiLine);
            }

            set
            {
                if (value)
                {
                    this._nativeTextEntry.InputType |= global::Android.Text.InputTypes.TextFlagMultiLine;
                }
                else
                {
                    this._nativeTextEntry.InputType &= ~global::Android.Text.InputTypes.TextFlagMultiLine;
                }
            }
        }

        public KeyboardOptions KeyboardOptions
        {
            get
            {
                KeyboardOptions options = KeyboardOptions.None;

                if (this._nativeTextEntry.InputType.HasFlag(global::Android.Text.InputTypes.TextFlagAutoCorrect))
                {
                    options |= KeyboardOptions.SpellCheck;
                }

                if (!this._nativeTextEntry.InputType.HasFlag(global::Android.Text.InputTypes.TextFlagNoSuggestions))
                {
                    options |= KeyboardOptions.Suggestions;
                }

                return options;
            }

            set
            {
                if (value.HasFlag(KeyboardOptions.SpellCheck))
                {
                    this._nativeTextEntry.InputType |= global::Android.Text.InputTypes.TextFlagAutoCorrect;
                }
                else
                {
                    this._nativeTextEntry.InputType &= ~global::Android.Text.InputTypes.TextFlagAutoCorrect;
                }

                if (value.HasFlag(KeyboardOptions.Suggestions))
                {
                    this._nativeTextEntry.InputType &= ~global::Android.Text.InputTypes.TextFlagNoSuggestions;
                }
                else
                {
                    this._nativeTextEntry.InputType |= global::Android.Text.InputTypes.TextFlagNoSuggestions;
                }
            }
        }

        public void Focus()
        {
            this._nativeTextEntry.RequestFocus();
        }
    }
}
