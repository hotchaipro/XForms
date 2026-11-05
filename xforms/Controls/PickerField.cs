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
    /// the theme's <see cref="PickerFieldStyle.TextInset"/>, and the glyph in the
    /// last <see cref="PickerFieldStyle.GlyphWidth"/>.
    /// </summary>
    public abstract class PickerField : TemplatedControl
    {
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
        /// Whether the picker's popup is open (a theme shows the field as
        /// focused meanwhile).
        /// </summary>
        public virtual bool IsPickerOpen
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Call when the popup opens or closes: the field redraws, and
        /// assistive technology hears it's expanded or collapsed.
        /// </summary>
        private protected void OnPickerOpenChanged()
        {
            this.NotifyAccessibilityStateChanged();
            this.OnStateChanged();
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
        /// The field's text format, which it measures with: a text box's text
        /// size, so a field and a text box beside it match.
        /// </summary>
        public TextFormat CreateTextFormat()
        {
            return new TextFormat()
            {
                FontSize = TextBox.DefaultFontSize,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
            };
        }

        protected override Size MeasureOverride(
            Size availableSize)
        {
            // A text box's line, so the field is a text box's height
            float lineHeight = TextBox.DefaultLineHeight;
            if (lineHeight <= 0)
            {
                lineHeight = TextLayout.Measure("Ag", this.CreateTextFormat()).Height;
            }

            var style = this.ActualTheme.PickerFieldStyle;
            float width = style.TextInset + this.MaximumTextWidth + style.GlyphWidth;

            return new Size(
                Math.Min(width, availableSize.Width),
                Math.Min(lineHeight + TextBox.DefaultPadding.Top + TextBox.DefaultPadding.Bottom, availableSize.Height));
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
