using System;
using XForms.Controls;

namespace XForms.Toolkit
{
    /// <summary>
    /// A field showing the selected item; clicking it opens a list of the items
    /// below it. Up and Down change the selection without opening the list.
    /// </summary>
    public class ListPicker : PickerField
    {
        private int _selectedIndex = -1;
        private float _maximumTextWidth = -1;
        private Popup _popup;
        private ListPickerItemsView _itemsView;

        public ListPicker()
        {
            this.Items = new ListPickerItemCollection(this);
        }

        public event EventHandler<ListPickerItem> SelectionChanged;

        public ListPickerItemCollection Items
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

        public ListPickerItem SelectedValue
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
                return this.SelectedValue?.Text;
            }
        }

        protected override string DisplayText
        {
            get
            {
                return this.SelectedValue?.Text ?? string.Empty;
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
                        width = Math.Max(width, this.Application.MeasureText(item.Text, format).Width);
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
            this.InvalidateLayout();
            this.Invalidate();
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
            this.Invalidate();

            if (index >= 0)
            {
                this.SelectionChanged?.Invoke(this, this.Items[index]);
            }
        }

        protected override void DrawGlyph(
            IDrawContext drawContext,
            Rectangle bounds,
            Color color)
        {
            DrawChevronDown(drawContext, bounds, color);
        }

        protected override bool OnKeyDown(
            KeyInputEvent keyInputEvent)
        {
            if (base.OnKeyDown(keyInputEvent))
            {
                return true;
            }

            switch (keyInputEvent.Key)
            {
                case Key.Up:
                    this.Select(Math.Max(0, this._selectedIndex - 1));
                    return true;

                case Key.Down:
                    this.Select(Math.Min(this.Items.Count - 1, this._selectedIndex + 1));
                    return true;
            }

            return false;
        }

        protected override void OpenPicker()
        {
            if (this.Items.Count == 0)
            {
                return;
            }

            if (null == this._popup)
            {
                this._itemsView = new ListPickerItemsView(this);
                this._popup = new Popup()
                {
                    PlacementTarget = this,
                    Content = new ListPickerDropDown(this, this._itemsView),
                };
            }

            if (this._popup.IsOpen)
            {
                return;
            }

            bool hasKeyboardFocus = this.IsFocused;
            this._itemsView.Reset();
            this._popup.Show();
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
        private sealed class ListPickerDropDown : UserControl
        {
            private const float VerticalPadding = 4.0f;

            private readonly ListPicker _picker;
            private readonly ScrollView _scrollView;

            public ListPickerDropDown(
                ListPicker picker,
                ListPickerItemsView itemsView)
            {
                this._picker = picker;
                this._scrollView = new ScrollView()
                {
                    IsVerticalScrollingEnabled = true,
                    IsHorizontalScrollingEnabled = false,
                    Content = itemsView,
                };
                itemsView.ScrollView = this._scrollView;
                this.Content = this._scrollView;
            }

            private static float Inset
            {
                get
                {
                    return PopupBorderThickness + VerticalPadding;
                }
            }

            protected override Size MeasureOverride(
                Size availableSize)
            {
                float width = Math.Max(
                    this._picker.Frame.Width,
                    this._picker.MaximumTextWidth + (2 * TextInset) + (2 * PopupBorderThickness));
                width = Math.Min(width, availableSize.Width);

                // As tall as the items, up to the space available
                float height = Math.Min(
                    (this._picker.Items.Count * ListPickerItemsView.RowHeight) + (2 * Inset),
                    availableSize.Height);

                this._scrollView.Measure(new Size(
                    Math.Max(0, width - (2 * PopupBorderThickness)),
                    Math.Max(0, height - (2 * Inset))));

                return new Size(width, height);
            }

            protected override void ArrangeOverride(
                Size finalSize)
            {
                this._scrollView.Arrange(new Rectangle(
                    PopupBorderThickness,
                    Inset,
                    Math.Max(0, finalSize.Width - (2 * PopupBorderThickness)),
                    Math.Max(0, finalSize.Height - (2 * Inset))));
            }

            protected override void DrawBackground(
                IDrawContext drawContext,
                Rectangle bounds)
            {
                drawContext.Clear(Colors.Transparent);
                DrawPopupFrame(drawContext, bounds, this.Theme);
            }
        }

        /// <summary>
        /// Draws the rows, and picks the row under a click. Up and Down move the
        /// highlight; Enter or Space chooses it.
        /// </summary>
        private sealed class ListPickerItemsView : UserControl
        {
            public const float RowHeight = 36.0f;

            private readonly ListPicker _picker;
            private int _highlightIndex;
            private int _pressedIndex = -1;

            public ListPickerItemsView(
                ListPicker picker)
            {
                this._picker = picker;
                this.IsKeyboardFocusable = true;
                this.AccessibilityRole = AccessibilityRole.List;
            }

            public ScrollView ScrollView
            {
                get;
                set;
            }

            public void Reset()
            {
                this._highlightIndex = Math.Max(0, this._picker.SelectedIndex);
                this._pressedIndex = -1;
                this.InvalidateLayout();
                this.Invalidate();
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
                return ((point.X >= 0) && (point.X < this.Frame.Width) && (index >= 0) && (index < this._picker.Items.Count)) ? index : -1;
            }

            public override bool HandlePointerEvent(
                PointerInputEvent pointerInputEvent)
            {
                switch (pointerInputEvent.State)
                {
                    case PointerInputState.Began:
                        this._pressedIndex = this.RowAt(pointerInputEvent.Point);
                        this.Invalidate();
                        return this._pressedIndex >= 0;

                    case PointerInputState.Ended:
                        {
                            int index = this.RowAt(pointerInputEvent.Point);
                            bool isChosen = (index >= 0) && (index == this._pressedIndex);
                            this._pressedIndex = -1;
                            this.Invalidate();

                            if (isChosen)
                            {
                                this._picker.Choose(index);
                            }

                            return true;
                        }

                    case PointerInputState.Canceled:
                        this._pressedIndex = -1;
                        this.Invalidate();
                        return true;
                }

                // NOTE: Moves aren't claimed, so a touch drag scrolls the list
                return false;
            }

            protected override bool OnKeyDown(
                KeyInputEvent keyInputEvent)
            {
                int count = this._picker.Items.Count;

                switch (keyInputEvent.Key)
                {
                    case Key.Up:
                        this.Highlight(Math.Max(0, this._highlightIndex - 1));
                        return true;

                    case Key.Down:
                        this.Highlight(Math.Min(count - 1, this._highlightIndex + 1));
                        return true;

                    case Key.Home:
                        this.Highlight(0);
                        return true;

                    case Key.End:
                        this.Highlight(count - 1);
                        return true;

                    case Key.Enter:
                    case Key.Space:
                        this._picker.Choose(this._highlightIndex);
                        return true;

                    case Key.Tab:
                        // Close, and let focus move on
                        this._picker.ClosePopup();
                        return false;
                }

                return false;
            }

            protected override void OnActivate()
            {
                this._picker.Choose(this._highlightIndex);
            }

            private void Highlight(
                int index)
            {
                this._highlightIndex = index;
                this.Invalidate();
                this.BringHighlightIntoView(animated: true);
            }

            public void BringHighlightIntoView(
                bool animated)
            {
                var scrollView = this.ScrollView;
                if (null == scrollView)
                {
                    return;
                }

                float top = this._highlightIndex * RowHeight;
                float viewport = scrollView.Frame.Height;
                float offset = scrollView.VerticalOffset;

                if (!animated)
                {
                    // Opening: center the item in the list
                    scrollView.ScrollTo(Math.Max(0, top - ((viewport - RowHeight) / 2)), animated: false);
                }
                else if (top < offset)
                {
                    scrollView.ScrollTo(top, animated: true);
                }
                else if (top + RowHeight > offset + viewport)
                {
                    scrollView.ScrollTo(top + RowHeight - viewport, animated: true);
                }
            }

            protected override void DrawBackground(
                IDrawContext drawContext,
                Rectangle bounds)
            {
                var theme = this.Theme;
                var items = this._picker.Items;
                int selectedIndex = this._picker.SelectedIndex;

                drawContext.Clear(Colors.Transparent);

                var textFormat = this._picker.CreateTextFormat();

                for (int i = 0; i < items.Count; i += 1)
                {
                    var row = new Rectangle(0, i * RowHeight, bounds.Width, RowHeight);
                    var highlight = new Rectangle(4, row.Y + 2, Math.Max(0, bounds.Width - 8), RowHeight - 4);

                    if (i == this._pressedIndex)
                    {
                        drawContext.FillRectangle(highlight, theme.ForegroundColor.MultiplyAlpha(0.12f));
                    }
                    else if ((i == selectedIndex) || ((i == this._highlightIndex) && this.IsFocused))
                    {
                        drawContext.FillRectangle(highlight, theme.SubtleBackgroundColor);
                    }

                    if (i == selectedIndex)
                    {
                        // The selected item's accent mark
                        drawContext.FillRectangle(new Rectangle(4, row.Y + ((RowHeight - 16) / 2), 3, 16), theme.AccentColor);
                    }

                    drawContext.DrawText(
                        items[i].Text,
                        new Rectangle(TextInset, row.Y, Math.Max(0, bounds.Width - (2 * TextInset)), RowHeight),
                        theme.ForegroundColor,
                        textFormat);
                }
            }

            protected override void OnFocusChanged()
            {
                this.Invalidate();
            }
        }
    }
}
