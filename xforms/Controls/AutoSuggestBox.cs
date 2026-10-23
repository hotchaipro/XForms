using System;
using System.Collections.Generic;
using XForms.Primitives;

namespace XForms.Controls
{
    /// <summary>
    /// Why an auto-suggest box's text changed.
    /// </summary>
    public enum AutoSuggestionBoxTextChangeReason
    {
        /// <summary>
        /// The user typed.
        /// </summary>
        UserInput,

        /// <summary>
        /// The app set <see cref="AutoSuggestBox.Text"/>.
        /// </summary>
        ProgrammaticChange,

        /// <summary>
        /// The user moved to or chose a suggestion.
        /// </summary>
        SuggestionChosen,
    }

    public sealed class AutoSuggestBoxTextChangedEventArgs : EventArgs
    {
        public AutoSuggestBoxTextChangedEventArgs(
            AutoSuggestionBoxTextChangeReason reason)
        {
            this.Reason = reason;
        }

        public AutoSuggestionBoxTextChangeReason Reason
        {
            get;
        }
    }

    public sealed class AutoSuggestBoxSuggestionChosenEventArgs : EventArgs
    {
        public AutoSuggestBoxSuggestionChosenEventArgs(
            string selectedItem)
        {
            this.SelectedItem = selectedItem;
        }

        public string SelectedItem
        {
            get;
        }
    }

    public sealed class AutoSuggestBoxQuerySubmittedEventArgs : EventArgs
    {
        public AutoSuggestBoxQuerySubmittedEventArgs(
            string queryText,
            string chosenSuggestion)
        {
            this.QueryText = queryText;
            this.ChosenSuggestion = chosenSuggestion;
        }

        /// <summary>
        /// The box's text.
        /// </summary>
        public string QueryText
        {
            get;
        }

        /// <summary>
        /// The suggestion chosen, or null when the text was submitted with
        /// Enter.
        /// </summary>
        public string ChosenSuggestion
        {
            get;
        }
    }

    /// <summary>
    /// A text box offering suggestions as the user types: the app sets
    /// <see cref="ItemsSource"/> when the text changes (from the user's
    /// typing), and they show in a list below the box. Down and Up move
    /// through them, putting each in the box; Enter submits the text (or the
    /// suggestion), Escape goes back to what was typed and closes the list,
    /// and a click on a suggestion chooses and submits it.
    /// </summary>
    public class AutoSuggestBox : TemplatedControl
    {
        public static readonly ViewProperty<AutoSuggestBox, string> TextProperty = new(nameof(Text), v => v.Text, (v, x) => v.Text = x);

        /// <summary>
        /// The most suggestions shown.
        /// </summary>
        public const int MaximumSuggestions = 8;

        private TextBox _textBox;
        private string _text = string.Empty;
        private string _typedText = string.Empty;
        private string _placeholderText;
        private IReadOnlyList<string> _itemsSource = Array.Empty<string>();
        private Popup _popup;
        private AutoSuggestBoxListView _list;
        private AutoSuggestionBoxTextChangeReason? _changeReason;

        public AutoSuggestBox()
        {
        }

        public event EventHandler<AutoSuggestBoxTextChangedEventArgs> TextChanged;

        /// <summary>
        /// Raised as the user moves to a suggestion (it's put in the box).
        /// </summary>
        public event EventHandler<AutoSuggestBoxSuggestionChosenEventArgs> SuggestionChosen;

        /// <summary>
        /// Raised when the user submits the text, or chooses a suggestion.
        /// </summary>
        public event EventHandler<AutoSuggestBoxQuerySubmittedEventArgs> QuerySubmitted;

        public string Text
        {
            get
            {
                return this._text;
            }

            set
            {
                this.SetText(value, AutoSuggestionBoxTextChangeReason.ProgrammaticChange);
            }
        }

        /// <summary>
        /// Shown while the box is empty; it also names the box for assistive
        /// technology.
        /// </summary>
        public string PlaceholderText
        {
            get
            {
                return this._placeholderText;
            }

            set
            {
                this._placeholderText = value;
                this._textBox?.PlaceholderText = value;
            }
        }

        /// <summary>
        /// The suggestions for the text, which the app sets as it changes.
        /// </summary>
        public IReadOnlyList<string> ItemsSource
        {
            get
            {
                return this._itemsSource;
            }

            set
            {
                this._itemsSource = value ?? Array.Empty<string>();
                this._list?.Reset();
                this.UpdateSuggestions();
            }
        }

        /// <summary>
        /// The suggestions shown: the first <see cref="MaximumSuggestions"/>.
        /// </summary>
        public int SuggestionCount
        {
            get
            {
                return Math.Min(this._itemsSource.Count, MaximumSuggestions);
            }
        }

        /// <summary>
        /// Whether the list of suggestions is open.
        /// </summary>
        public bool IsSuggestionListOpen
        {
            get
            {
                return this._popup?.IsOpen ?? false;
            }
        }

        public override bool? AccessibilityIsExpanded
        {
            get
            {
                return this.IsSuggestionListOpen;
            }
        }

        public override bool PerformAccessibilityAction(
            AccessibilityAction action)
        {
            if (action == AccessibilityAction.Expand)
            {
                this.OpenSuggestions();
                return true;
            }

            if (action == AccessibilityAction.Collapse)
            {
                this.CloseSuggestions();
                return true;
            }

            return base.PerformAccessibilityAction(action);
        }

        protected override AccessibilityRole DefaultAccessibilityRole
        {
            get
            {
                return AccessibilityRole.ComboBox;
            }
        }

        protected override string DefaultAccessibilityLabel
        {
            get
            {
                return this._placeholderText;
            }
        }

        protected override View Build()
        {
            this._textBox = new TextBox()
            {
                Text = this._text,
                PlaceholderText = this._placeholderText,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
            };
            this._textBox.TextChanged += this.TextBox_TextChanged;
            this._textBox.Submitted += (sender, e) => this.Submit(null);
            this._textBox.LostFocus += this.TextBox_LostFocus;
            return this._textBox;
        }

        // Focus leaving closes the list, unless it left for a press on the
        // list (in the browser, pressing anything takes the focus)
        private void TextBox_LostFocus(
            object sender,
            EventArgs e)
        {
            if ((this._list?.PressedIndex ?? -1) < 0)
            {
                this.CloseSuggestions();
            }
        }

        private void TextBox_TextChanged(
            object sender,
            EventArgs e)
        {
            // A change the box didn't make is the user's
            var reason = this._changeReason ?? AutoSuggestionBoxTextChangeReason.UserInput;
            this.SetText(this._textBox.Text, reason);
        }

        private void SetText(
            string text,
            AutoSuggestionBoxTextChangeReason reason)
        {
            text ??= string.Empty;
            if (reason != AutoSuggestionBoxTextChangeReason.SuggestionChosen)
            {
                this._typedText = text;
            }

            if (text == this._text)
            {
                return;
            }

            this._text = text;
            if ((null != this._textBox) && (this._textBox.Text != text))
            {
                this._changeReason = reason;
                try
                {
                    this._textBox.Text = text;
                    this._textBox.Select(text.Length, 0);
                }
                finally
                {
                    this._changeReason = null;
                }
            }

            this.OnPropertyChanged(nameof(this.Text));
            this.TextChanged?.Invoke(this, new AutoSuggestBoxTextChangedEventArgs(reason));

            if (reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                this._list?.Reset();
                this.UpdateSuggestions();
            }
        }

        // The list shows while the user is typing in the box and there are
        // suggestions
        private void UpdateSuggestions()
        {
            if ((this.SuggestionCount > 0) && (this._textBox?.IsFocused ?? false))
            {
                this.OpenSuggestions();
            }
            else
            {
                this.CloseSuggestions();
            }
        }

        private void OpenSuggestions()
        {
            if (this.SuggestionCount == 0)
            {
                return;
            }

            if (null == this._popup)
            {
                this._list = new AutoSuggestBoxListView(this);
                this._popup = new Popup()
                {
                    PlacementTarget = this,
                    IsLightDismissEnabled = false,
                    Content = new SuggestionFrame(this._list),
                };
                this._popup.Closed += (sender, e) => this.NotifyAccessibilityStateChanged();
            }

            this._list.Width = this.Bounds.Width - (2 * PopupFrame.BorderThickness);
            this._list.InvalidateMeasure();
            this._list.InvalidateVisual();
            if (!this._popup.IsOpen)
            {
                this._popup.Show();
                this.NotifyAccessibilityStateChanged();
            }
        }

        private void CloseSuggestions()
        {
            this._popup?.Close();
        }

        /// <summary>
        /// Moves to a suggestion (or, at -1, back to the typed text): it goes
        /// in the box.
        /// </summary>
        internal void Highlight(
            int index)
        {
            this._list.HighlightedIndex = index;
            if (index < 0)
            {
                this.SetText(this._typedText, AutoSuggestionBoxTextChangeReason.SuggestionChosen);
                return;
            }

            string item = this._itemsSource[index];
            this.SuggestionChosen?.Invoke(this, new AutoSuggestBoxSuggestionChosenEventArgs(item));
            this.SetText(item, AutoSuggestionBoxTextChangeReason.SuggestionChosen);
        }

        /// <summary>
        /// Chooses a suggestion (clicked): it goes in the box and is
        /// submitted.
        /// </summary>
        internal void Choose(
            int index)
        {
            string item = this._itemsSource[index];
            this.SuggestionChosen?.Invoke(this, new AutoSuggestBoxSuggestionChosenEventArgs(item));
            this.SetText(item, AutoSuggestionBoxTextChangeReason.SuggestionChosen);
            this._typedText = item;
            this.Submit(item);
            this._textBox?.Focus();
        }

        private void Submit(
            string chosenSuggestion)
        {
            if ((null == chosenSuggestion) && (null != this._list) && this.IsSuggestionListOpen && (this._list.HighlightedIndex >= 0))
            {
                chosenSuggestion = this._itemsSource[this._list.HighlightedIndex];
            }

            this.CloseSuggestions();
            this._typedText = this._text;
            this.QuerySubmitted?.Invoke(this, new AutoSuggestBoxQuerySubmittedEventArgs(this._text, chosenSuggestion));
        }

        // Keys the text box offers: Up, Down and Escape
        protected override void OnKeyDown(
            KeyEventArgs keyInputEvent)
        {
            int count = this.SuggestionCount;
            switch (keyInputEvent.Key)
            {
                case Key.Down:
                    if (!this.IsSuggestionListOpen)
                    {
                        this.OpenSuggestions();
                    }
                    else if (count > 0)
                    {
                        // Past the last, back to the typed text
                        int next = this._list.HighlightedIndex + 1;
                        this.Highlight((next >= count) ? -1 : next);
                    }

                    break;

                case Key.Up:
                    if (this.IsSuggestionListOpen && (count > 0))
                    {
                        int previous = this._list.HighlightedIndex - 1;
                        this.Highlight((previous < -1) ? count - 1 : previous);
                    }

                    break;

                case Key.Escape:
                    if (!this.IsSuggestionListOpen)
                    {
                        return;
                    }

                    this.SetText(this._typedText, AutoSuggestionBoxTextChangeReason.SuggestionChosen);
                    this.CloseSuggestions();
                    break;

                default:
                    return;
            }

            keyInputEvent.Handled = true;
        }

        /// <summary>
        /// The popup's frame around the suggestions.
        /// </summary>
        private sealed class SuggestionFrame : PopupFrame
        {
            public SuggestionFrame(
                AutoSuggestBoxListView list)
            {
                list.Margin = new Thickness(BorderThickness);
                this.Content = list;
            }
        }
    }

    /// <summary>
    /// An auto-suggest box's suggestions, in a list below it: a click chooses
    /// one. Its theme's template draws the rows, each <see cref="RowHeight"/>
    /// tall.
    /// </summary>
    public sealed class AutoSuggestBoxListView : TemplatedControl
    {
        public const float RowHeight = 36.0f;

        private readonly AutoSuggestBox _box;
        private int _highlightedIndex = -1;
        private int _pressedIndex = -1;

        internal AutoSuggestBoxListView(
            AutoSuggestBox box)
        {
            this._box = box;
            this.AccessibilityRole = AccessibilityRole.List;
        }

        public AutoSuggestBox AutoSuggestBox
        {
            get
            {
                return this._box;
            }
        }

        /// <summary>
        /// The suggestion the keyboard is on, or -1.
        /// </summary>
        public int HighlightedIndex
        {
            get
            {
                return this._highlightedIndex;
            }

            internal set
            {
                this._highlightedIndex = value;
                this.InvalidateVisual();
            }
        }

        /// <summary>
        /// The suggestion being pressed, or -1.
        /// </summary>
        public int PressedIndex
        {
            get
            {
                return this._pressedIndex;
            }
        }

        internal void Reset()
        {
            this._highlightedIndex = -1;
            this._pressedIndex = -1;
            this.InvalidateMeasure();
            this.InvalidateVisual();
        }

        protected override Size MeasureOverride(
            Size availableSize)
        {
            float width = float.IsInfinity(availableSize.Width) ? 0 : availableSize.Width;
            return new Size(width, Math.Min(this._box.SuggestionCount * RowHeight, availableSize.Height));
        }

        private int RowAt(
            Point point)
        {
            int index = (int)Math.Floor(point.Y / RowHeight);
            return ((point.X >= 0) && (point.X < this.Bounds.Width) && (index >= 0) && (index < this._box.SuggestionCount)) ? index : -1;
        }

        protected override void OnPointerPressed(
            PointerEventArgs e)
        {
            this._pressedIndex = this.RowAt(e.Position);
            this.InvalidateVisual();
            e.Handled = (this._pressedIndex >= 0);
        }

        protected override void OnPointerReleased(
            PointerEventArgs e)
        {
            int index = this.RowAt(e.Position);
            bool isChosen = (index >= 0) && (index == this._pressedIndex);
            this._pressedIndex = -1;
            this.InvalidateVisual();
            e.Handled = true;

            if (isChosen)
            {
                this._box.Choose(index);
            }
        }

        protected override void OnPointerCanceled(
            PointerEventArgs e)
        {
            this._pressedIndex = -1;
            this.InvalidateVisual();
            e.Handled = true;
        }
    }
}
