using System;
using XForms.Controls;
using XForms.Primitives;

namespace XForms.Controls
{
    /// <summary>
    /// A field showing the selected item; clicking it opens a list of the items
    /// below it. Up and Down change the selection without opening the list.
    /// </summary>
    public class ComboBox : PickerField
    {
        public static readonly ViewProperty<ComboBox, int> SelectedIndexProperty = new(nameof(SelectedIndex), v => v.SelectedIndex, (v, x) => v.SelectedIndex = x);
        public static readonly ViewProperty<ComboBox, ComboBoxItem> SelectedItemProperty = new(nameof(SelectedItem), v => v.SelectedItem, (v, x) => v.SelectedItem = x);

        private int _selectedIndex = -1;
        private float _maximumTextWidth = -1;
        private Popup _popup;
        private ComboBoxItemsView _itemsView;

        public ComboBox()
        {
            this.Items = new ComboBoxItemCollection(this);
        }

        public event EventHandler<ComboBoxItem> SelectionChanged;

        public ComboBoxItemCollection Items
        {
            get;
        }

        public int SelectedIndex
        {
            get
            {
                return this._selectedIndex;
            }

            set
            {
                this.Select(value);
            }
        }

        public ComboBoxItem SelectedItem
        {
            get
            {
                return (this._selectedIndex >= 0) ? this.Items[this._selectedIndex] : null;
            }

            set
            {
                this.Select(this.Items.IndexOf(value));
            }
        }

        protected override string DefaultAccessibilityLabel
        {
            get
            {
                return this.SelectedItem?.Text;
            }
        }

        public override string DisplayText
        {
            get
            {
                return this.SelectedItem?.Text ?? string.Empty;
            }
        }

        protected override float MaximumTextWidth
        {
            get
            {
                if (this._maximumTextWidth < 0)
                {
                    var format = this.CreateTextFormat();
                    float width = 0;
                    foreach (var item in this.Items)
                    {
                        width = Math.Max(width, TextLayout.Measure(item.Text, format).Width);
                    }

                    this._maximumTextWidth = width;
                }

                return this._maximumTextWidth;
            }
        }

        internal void OnItemsChanged()
        {
            // Like the native pickers, the first item starts out selected
            if ((this._selectedIndex < 0) && (this.Items.Count > 0))
            {
                this._selectedIndex = 0;
            }
            else if (this._selectedIndex >= this.Items.Count)
            {
                this._selectedIndex = this.Items.Count - 1;
            }

            this._maximumTextWidth = -1;
            this.InvalidateMeasure();
            this.InvalidateVisual();
        }

        /// <summary>
        /// Selects the item at the index (or nothing, for -1), and raises
        /// <see cref="SelectionChanged"/> when the selection changes.
        /// </summary>
        public void Select(
            int index)
        {
            if ((index < -1) || (index >= this.Items.Count) || (index == this._selectedIndex))
            {
                return;
            }

            this._selectedIndex = index;
            this.InvalidateVisual();
            this.NotifyAccessibilityStateChanged();

            this.OnPropertyChanged(nameof(this.SelectedIndex));
            this.OnPropertyChanged(nameof(this.SelectedItem));

            if (index >= 0)
            {
                this.SelectionChanged?.Invoke(this, this.Items[index]);
            }
        }

        protected override void OnKeyDown(
            KeyEventArgs keyInputEvent)
        {
            base.OnKeyDown(keyInputEvent);
            if (keyInputEvent.Handled)
            {
                return;
            }

            switch (keyInputEvent.Key)
            {
                case Key.Up:
                    this.Select(Math.Max(0, this._selectedIndex - 1));
                    keyInputEvent.Handled = true;
                    return;

                case Key.Down:
                    this.Select(Math.Min(this.Items.Count - 1, this._selectedIndex + 1));
                    keyInputEvent.Handled = true;
                    return;
            }

            return;
        }

        protected override bool IsPickerOpen
        {
            get
            {
                return this._popup?.IsOpen ?? false;
            }
        }

        protected override void ClosePicker()
        {
            this._popup?.Close();
        }

        protected override void OpenPicker()
        {
            if (this.Items.Count == 0)
            {
                return;
            }

            if (null == this._popup)
            {
                this._itemsView = new ComboBoxItemsView(this);
                this._popup = new Popup()
                {
                    PlacementTarget = this,
                    Content = new ComboBoxDropDown(this, this._itemsView),
                };
                this._popup.Closed += (sender, e) => this.NotifyAccessibilityStateChanged();
            }

            if (this._popup.IsOpen)
            {
                return;
            }

            bool hasKeyboardFocus = this.IsFocused;
            this._itemsView.Reset();
            this._popup.Show();
            this.NotifyAccessibilityStateChanged();
            this._itemsView.BringHighlightIntoView(animated: false);

            // From the keyboard, the arrow keys move through the list
            if (hasKeyboardFocus)
            {
                this._itemsView.Focus();
            }
        }

        internal void Choose(
            int index)
        {
            this._popup?.Close();
            this.Select(index);
        }

        internal void ClosePopup()
        {
            this._popup?.Close();
        }

        /// <summary>
        /// The popup's frame: a scrolling list of the items.
        /// </summary>
        private sealed class ComboBoxDropDown : PopupFrame
        {
            private const float VerticalPadding = 4.0f;

            private readonly ComboBox _picker;
            private readonly ScrollViewer _scrollView;

            public ComboBoxDropDown(
                ComboBox picker,
                ComboBoxItemsView itemsView)
            {
                this._picker = picker;
                this._scrollView = new ScrollViewer()
                {
                    IsVerticalScrollingEnabled = true,
                    IsHorizontalScrollingEnabled = false,
                    Content = itemsView,
                };
                itemsView.ScrollViewer = this._scrollView;
                this.Content = this._scrollView;
            }

            private static float Inset
            {
                get
                {
                    return BorderThickness + VerticalPadding;
                }
            }

            protected override Size MeasureOverride(
                Size availableSize)
            {
                float width = Math.Max(
                    this._picker.Bounds.Width,
                    this._picker.MaximumTextWidth + (2 * PickerField.TextInset) + (2 * BorderThickness));
                width = Math.Min(width, availableSize.Width);

                // As tall as the items, up to the space available
                float height = Math.Min(
                    (this._picker.Items.Count * ComboBoxItemsView.RowHeight) + (2 * Inset),
                    availableSize.Height);

                this._scrollView.Measure(new Size(
                    Math.Max(0, width - (2 * BorderThickness)),
                    Math.Max(0, height - (2 * Inset))));

                return new Size(width, height);
            }

            protected override void ArrangeOverride(
                Size finalSize)
            {
                this._scrollView.Arrange(new Rect(
                    BorderThickness,
                    Inset,
                    Math.Max(0, finalSize.Width - (2 * BorderThickness)),
                    Math.Max(0, finalSize.Height - (2 * Inset))));
            }
        }
    }

    /// <summary>
    /// A combo box's list of items, in its popup: picks the row under a click;
    /// Up and Down move the highlight, and Enter or Space chooses it. Its
    /// theme's template draws the rows, each <see cref="RowHeight"/> tall.
    /// </summary>
    public sealed class ComboBoxItemsView : TemplatedControl
    {
        public const float RowHeight = 36.0f;

        private readonly ComboBox _picker;
        private int _highlightIndex;
        private int _pressedIndex = -1;

        internal ComboBoxItemsView(
            ComboBox picker)
        {
            this._picker = picker;
            this.Focusable = true;
            this.AccessibilityRole = AccessibilityRole.List;
        }

        /// <summary>
        /// The combo box whose items it shows.
        /// </summary>
        public ComboBox ComboBox
        {
            get
            {
                return this._picker;
            }
        }

        /// <summary>
        /// The row the keyboard is on.
        /// </summary>
        public int HighlightedIndex
        {
            get
            {
                return this._highlightIndex;
            }
        }

        /// <summary>
        /// The row being pressed, or -1.
        /// </summary>
        public int PressedIndex
        {
            get
            {
                return this._pressedIndex;
            }
        }

        internal ScrollViewer ScrollViewer
        {
            get;
            set;
        }

        internal void Reset()
        {
            this._highlightIndex = Math.Max(0, this._picker.SelectedIndex);
            this._pressedIndex = -1;
            this.InvalidateMeasure();
            this.InvalidateVisual();
        }

        protected override Size MeasureOverride(
            Size availableSize)
        {
            float width = float.IsInfinity(availableSize.Width) ? 0 : availableSize.Width;
            return new Size(width, this._picker.Items.Count * RowHeight);
        }

        private int RowAt(
            Point point)
        {
            int index = (int)Math.Floor(point.Y / RowHeight);
            return ((point.X >= 0) && (point.X < this.Bounds.Width) && (index >= 0) && (index < this._picker.Items.Count)) ? index : -1;
        }

        // NOTE: Moves aren't claimed, so a touch drag scrolls the list

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
                this._picker.Choose(index);
            }
        }

        protected override void OnPointerCanceled(
            PointerEventArgs e)
        {
            this._pressedIndex = -1;
            this.InvalidateVisual();
            e.Handled = true;
        }

        protected override void OnKeyDown(
            KeyEventArgs keyInputEvent)
        {
            int count = this._picker.Items.Count;

            switch (keyInputEvent.Key)
            {
                case Key.Up:
                    this.Highlight(Math.Max(0, this._highlightIndex - 1));
                    keyInputEvent.Handled = true;
                    return;

                case Key.Down:
                    this.Highlight(Math.Min(count - 1, this._highlightIndex + 1));
                    keyInputEvent.Handled = true;
                    return;

                case Key.Home:
                    this.Highlight(0);
                    keyInputEvent.Handled = true;
                    return;

                case Key.End:
                    this.Highlight(count - 1);
                    keyInputEvent.Handled = true;
                    return;

                case Key.Enter:
                case Key.Space:
                    this._picker.Choose(this._highlightIndex);
                    keyInputEvent.Handled = true;
                    return;

                case Key.Tab:
                    // Close, and let focus move on
                    this._picker.ClosePopup();
                    return;
            }

            return;
        }

        protected override void OnActivate()
        {
            this._picker.Choose(this._highlightIndex);
        }

        private void Highlight(
            int index)
        {
            this._highlightIndex = index;
            this.InvalidateVisual();
            this.BringHighlightIntoView(animated: true);
        }

        internal void BringHighlightIntoView(
            bool animated)
        {
            var scrollView = this.ScrollViewer;
            if (null == scrollView)
            {
                return;
            }

            float top = this._highlightIndex * RowHeight;
            float viewport = scrollView.Bounds.Height;
            float offset = scrollView.VerticalOffset;

            if (!animated)
            {
                // Opening: center the item in the list
                scrollView.ScrollToVerticalOffset(Math.Max(0, top - ((viewport - RowHeight) / 2)), animated: false);
            }
            else if (top < offset)
            {
                scrollView.ScrollToVerticalOffset(top, animated: true);
            }
            else if (top + RowHeight > offset + viewport)
            {
                scrollView.ScrollToVerticalOffset(top + RowHeight - viewport, animated: true);
            }
        }
    }
}
