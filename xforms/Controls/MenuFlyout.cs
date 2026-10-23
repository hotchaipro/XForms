using System;
using System.Collections.Generic;
using XForms.Primitives;

namespace XForms.Controls
{
    /// <summary>
    /// A menu of commands in a popup, beside the view it opens from: a view's
    /// context menu (its <c>ContextFlyout</c>, see
    /// <see cref="ViewAttachments"/>), or opened by the app. Choosing an item
    /// raises its <see cref="MenuItem.Click"/> and runs its command; a click
    /// outside, Escape or going back closes it.
    /// </summary>
    public class MenuFlyout : Popup
    {
        private readonly MenuFlyoutView _view;

        public MenuFlyout()
        {
            this._view = new MenuFlyoutView(this);
            base.Content = new MenuFrame(this._view);
        }

        /// <summary>
        /// The commands, in order (those not <see cref="MenuItem.IsVisible"/>
        /// left out).
        /// </summary>
        public List<MenuItem> Items
        {
            get;
        } = new List<MenuItem>();

        internal List<MenuItem> VisibleItems
        {
            get
            {
                return this.Items.FindAll((item) => item.IsVisible);
            }
        }

        /// <summary>
        /// Gives the menu keyboard focus, on its first item.
        /// </summary>
        internal void FocusMenu()
        {
            this._view.Reset();
            this._view.Focus();
        }

        internal void Choose(
            MenuItem item)
        {
            this.Close();
            item.RaiseClick();
        }

        /// <summary>
        /// The popup's frame around the menu.
        /// </summary>
        private sealed class MenuFrame : PopupFrame
        {
            public MenuFrame(
                MenuFlyoutView view)
            {
                view.Margin = new Thickness(BorderThickness);
                this.Content = view;
            }
        }
    }

    /// <summary>
    /// A menu flyout's items: a click or a tap chooses one; with keyboard
    /// focus, Up and Down (wrapping), Home and End move the highlight, and
    /// Enter or Space chooses it. Its theme's template draws the rows, each
    /// <see cref="RowHeight"/> tall.
    /// </summary>
    public sealed class MenuFlyoutView : TemplatedControl
    {
        public const float RowHeight = 36.0f;

        /// <summary>
        /// The space before an item's text (its icon goes in it).
        /// </summary>
        public const float TextInset = 40.0f;

        private const float MinimumWidth = 160.0f;

        private readonly MenuFlyout _flyout;
        private int _highlightedIndex;
        private int _pressedIndex = -1;

        internal MenuFlyoutView(
            MenuFlyout flyout)
        {
            this._flyout = flyout;
            this.Focusable = true;
            this.AccessibilityRole = AccessibilityRole.Menu;
        }

        /// <summary>
        /// The items shown.
        /// </summary>
        public IReadOnlyList<MenuItem> Items
        {
            get
            {
                return this._flyout.VisibleItems;
            }
        }

        /// <summary>
        /// The item the keyboard (or a hovering pointer) is on.
        /// </summary>
        public int HighlightedIndex
        {
            get
            {
                return this._highlightedIndex;
            }
        }

        /// <summary>
        /// The item being pressed, or -1.
        /// </summary>
        public int PressedIndex
        {
            get
            {
                return this._pressedIndex;
            }
        }

        // The highlighted item, read as the menu's value
        public override string AccessibilityValue
        {
            get
            {
                var items = this.Items;
                return ((this._highlightedIndex >= 0) && (this._highlightedIndex < items.Count)) ? items[this._highlightedIndex].Text : null;
            }
        }

        internal void Reset()
        {
            this._highlightedIndex = 0;
            this._pressedIndex = -1;
            this.InvalidateMeasure();
            this.InvalidateVisual();
        }

        public TextFormat CreateTextFormat()
        {
            var format = this.ActualTheme.Typography.Body.ToTextFormat();
            format.HorizontalAlignment = (this.FlowDirection == FlowDirection.RightToLeft) ? HorizontalAlignment.Right : HorizontalAlignment.Left;
            format.VerticalAlignment = VerticalAlignment.Center;
            return format;
        }

        protected override Size MeasureOverride(
            Size availableSize)
        {
            var format = this.CreateTextFormat();
            float width = MinimumWidth;
            foreach (var item in this.Items)
            {
                width = Math.Max(width, TextInset + TextLayout.Measure(item.Text ?? string.Empty, format).Width + 24);
            }

            return new Size(Math.Min(width, availableSize.Width), Math.Min(this.Items.Count * RowHeight, availableSize.Height));
        }

        private int RowAt(
            Point point)
        {
            int index = (int)Math.Floor(point.Y / RowHeight);
            return ((point.X >= 0) && (point.X < this.Bounds.Width) && (index >= 0) && (index < this.Items.Count)) ? index : -1;
        }

        /// <summary>
        /// Whether the item can be chosen (its command, if any, can run).
        /// </summary>
        public bool IsItemEnabled(
            int index)
        {
            var command = this.Items[index].Command;
            return (null == command) || command.CanExecute(null);
        }

        protected override void OnPointerPressed(
            PointerEventArgs e)
        {
            this._pressedIndex = this.RowAt(e.Position);
            this.InvalidateVisual();
            e.Handled = (this._pressedIndex >= 0);
        }

        // A hovering pointer highlights the item under it
        protected override void OnPointerMoved(
            PointerEventArgs e)
        {
            int index = this.RowAt(e.Position);
            if (!e.IsInContact && (index >= 0) && (index != this._highlightedIndex))
            {
                this.Highlight(index);
            }
        }

        protected override void OnPointerReleased(
            PointerEventArgs e)
        {
            int index = this.RowAt(e.Position);
            bool isChosen = (index >= 0) && (index == this._pressedIndex);
            this._pressedIndex = -1;
            this.InvalidateVisual();
            e.Handled = true;

            if (isChosen && this.IsItemEnabled(index))
            {
                this._flyout.Choose(this.Items[index]);
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
            int count = this.Items.Count;
            if (count == 0)
            {
                return;
            }

            switch (keyInputEvent.Key)
            {
                case Key.Up:
                    this.Highlight((this._highlightedIndex + count - 1) % count);
                    break;

                case Key.Down:
                    this.Highlight((this._highlightedIndex + 1) % count);
                    break;

                case Key.Home:
                    this.Highlight(0);
                    break;

                case Key.End:
                    this.Highlight(count - 1);
                    break;

                case Key.Enter:
                case Key.Space:
                    this.ChooseHighlighted();
                    break;

                case Key.Escape:
                case Key.Tab:
                    this._flyout.Close();
                    break;

                default:
                    return;
            }

            keyInputEvent.Handled = true;
        }

        protected override void OnActivate()
        {
            this.ChooseHighlighted();
        }

        private void ChooseHighlighted()
        {
            int index = this._highlightedIndex;
            if ((index >= 0) && (index < this.Items.Count) && this.IsItemEnabled(index))
            {
                this._flyout.Choose(this.Items[index]);
            }
        }

        private void Highlight(
            int index)
        {
            this._highlightedIndex = index;
            this.InvalidateVisual();
            this.NotifyAccessibilityStateChanged();
        }
    }
}
