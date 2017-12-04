using System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using XForms.Controls;

namespace XForms.Windows.Renderers
{
    public class TextEntryRenderer : ControlRenderer, ITextEntryRenderer
    {
        private TextBox _textBlock;
        private bool _multiline;

        public TextEntryRenderer(
            TextEntry textEntry)
            : base(textEntry)
        {
            this._textBlock = new TextBox()
            {
                FontWeight = global::Windows.UI.Text.FontWeights.SemiLight,
                IsSpellCheckEnabled = true,
                IsTextPredictionEnabled = true,
            };

            InputScope inputScope = new InputScope()
            {
                Names = { new InputScopeName(InputScopeNameValue.Default) },
            };
            this._textBlock.InputScope = inputScope;

            this.SetNativeElement(this._textBlock);
        }

        public string Text
        {
            get
            {
                return this._textBlock.Text;
            }

            set
            {
                this._textBlock.Text = value ?? String.Empty;
            }
        }

        public string PlaceholderText
        {
            get
            {
                return this._textBlock.PlaceholderText;
            }

            set
            {
                this._textBlock.PlaceholderText = value;
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
                if (value)
                {
                    this._textBlock.AcceptsReturn = true;
                    this._textBlock.TextWrapping = global::Windows.UI.Xaml.TextWrapping.Wrap;
                }
                else
                {
                    this._textBlock.AcceptsReturn = false;
                    this._textBlock.TextWrapping = global::Windows.UI.Xaml.TextWrapping.NoWrap;
                }

                this._multiline = value;
            }
        }

        public KeyboardOptions KeyboardOptions
        {
            get
            {
                KeyboardOptions options = KeyboardOptions.None;

                if (this._textBlock.IsSpellCheckEnabled)
                {
                    options |= KeyboardOptions.SpellCheck;
                }

                if (this._textBlock.IsTextPredictionEnabled)
                {
                    options |= KeyboardOptions.Suggestions;
                }

                return options;
            }

            set
            {
                this._textBlock.IsSpellCheckEnabled = value.HasFlag(KeyboardOptions.SpellCheck);
                this._textBlock.IsTextPredictionEnabled = value.HasFlag(KeyboardOptions.Suggestions);
            }
        }

        public void Focus()
        {
            var noWait = this._textBlock.Dispatcher.RunAsync(global::Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                this._textBlock.Focus(global::Windows.UI.Xaml.FocusState.Pointer);
            });
        }
    }
}
