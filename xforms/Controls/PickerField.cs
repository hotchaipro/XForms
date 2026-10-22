using System;
using XForms.Controls;
using XForms.Primitives;

namespace XForms.Controls
{
    /// <summary>
    /// The field of a picker: shows the current value (or a placeholder) like a
    /// text entry, with a glyph at the end, and opens the picker's popup when
    /// clicked, or with Enter, Space, F4 or Alt+Down. Its theme's template
    /// draws it, in the layout the field measures: the text from
    /// <see cref="TextInset"/>, and the glyph in the last
    /// <see cref="GlyphWidth"/>.
    /// </summary>
    public abstract class PickerField : TemplatedControl
    {
        protected const float FontSize = 15.0f;
        private const float VerticalPadding = 7.0f;

        /// <summary>
        /// Where the text starts.
        /// </summary>
        public const float TextInset = 11.0f;

        /// <summary>
        /// The width of the glyph's area, at the end of the field.
        /// </summary>
        public const float GlyphWidth = 34.0f;

        protected PickerField()
        {
            this.Focusable = true;
        }

        protected override AccessibilityRole DefaultAccessibilityRole
        {
            get
            {
                return AccessibilityRole.ComboBox;
            }
        }

        public override bool? AccessibilityIsExpanded
        {
            get
            {
                return this.IsPickerOpen;
            }
        }

        public override bool PerformAccessibilityAction(
            AccessibilityAction action)
        {
            if ((action == AccessibilityAction.Collapse) && this.IsPickerOpen)
            {
                this.ClosePicker();
                return true;
            }

            if ((action == AccessibilityAction.Expand) && !this.IsPickerOpen)
            {
                this.OpenPicker();
                return true;
            }

            return base.PerformAccessibilityAction(action);
        }

        /// <summary>
        /// Whether the picker's popup is open.
        /// </summary>
        protected virtual bool IsPickerOpen
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Closes the picker's popup.
        /// </summary>
        protected virtual void ClosePicker()
        {
        }

        public override string AccessibilityValue
        {
            get
            {
                return this.IsPlaceholder ? null : this.DisplayText;
            }
        }

        /// <summary>
        /// The text shown in the field.
        /// </summary>
        public abstract string DisplayText { get; }

        /// <summary>
        /// Whether the text is a placeholder (drawn subtly).
        /// </summary>
        public virtual bool IsPlaceholder
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// The widest text the field may show, which sets its width.
        /// </summary>
        protected abstract float MaximumTextWidth { get; }

        /// <summary>
        /// Opens the picker's popup.
        /// </summary>
        protected abstract void OpenPicker();

        /// <summary>
        /// The field's text format, which it measures with.
        /// </summary>
        public TextFormat CreateTextFormat()
        {
            return new TextFormat()
            {
                FontSize = FontSize,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
            };
        }

        protected override Size MeasureOverride(
            Size availableSize)
        {
            float lineHeight = TextLayout.Measure("Ag", this.CreateTextFormat()).Height;
            float width = TextInset + this.MaximumTextWidth + GlyphWidth;

            return new Size(
                Math.Min(width, availableSize.Width),
                Math.Min(lineHeight + (2 * VerticalPadding), availableSize.Height));
        }

        // NOTE: Moves aren't claimed, so a drag that starts here can still scroll

        protected override void OnPointerPressed(
            PointerEventArgs e)
        {
            this.IsPressed = true;
            e.Handled = true;
        }

        protected override void OnPointerReleased(
            PointerEventArgs e)
        {
            this.IsPressed = false;
            var point = e.Position;
            if ((point.X >= 0) && (point.Y >= 0) && (point.X < this.Bounds.Width) && (point.Y < this.Bounds.Height))
            {
                this.OpenPicker();
            }

            e.Handled = true;
        }

        protected override void OnPointerCanceled(
            PointerEventArgs e)
        {
            this.IsPressed = false;
            e.Handled = true;
        }

        protected override void OnActivate()
        {
            this.OpenPicker();
        }

        protected override void OnKeyDown(
            KeyEventArgs keyInputEvent)
        {
            if ((keyInputEvent.Key == Key.F4) || (keyInputEvent.IsAltDown && (keyInputEvent.Key == Key.Down)))
            {
                this.OpenPicker();
                keyInputEvent.Handled = true;
                return;
            }

            return;
        }
    }
}
