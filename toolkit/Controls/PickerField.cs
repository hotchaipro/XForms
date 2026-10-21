using System;
using XForms.Controls;

namespace XForms.Toolkit
{
    /// <summary>
    /// The field of a picker: shows the current value (or a placeholder) like a
    /// text entry, with a glyph at the end, and opens the picker's popup when
    /// clicked, or with Enter, Space, F4 or Alt+Down.
    /// </summary>
    public abstract class PickerField : UserControl
    {
        protected const float FontSize = 15.0f;
        protected const float TextInset = 11.0f;
        private const float VerticalPadding = 7.0f;
        private const float GlyphWidth = 34.0f;

        private bool _isPressed;

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
        protected abstract string DisplayText { get; }

        /// <summary>
        /// Whether the text is a placeholder (drawn in the subtle color).
        /// </summary>
        protected virtual bool IsPlaceholder
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
        /// Draws the glyph at the end of the field, centered in the bounds.
        /// </summary>
        protected abstract void DrawGlyph(DrawingContext drawContext, Rect bounds, Color color);

        protected TextFormat CreateTextFormat()
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

        protected override void Render(
            DrawingContext drawContext)
        {
            var bounds = new Rect(0, 0, this.Bounds.Width, this.Bounds.Height);
            var theme = this.ActualTheme;

            drawContext.Clear(Colors.Transparent);
            drawContext.FillRectangle(bounds, theme.Colors.Surface);
            if (this._isPressed)
            {
                drawContext.FillRectangle(bounds, theme.Colors.Foreground.MultiplyAlpha(0.08f));
            }

            var textBounds = new Rect(TextInset, 0, Math.Max(0, bounds.Width - TextInset - GlyphWidth), bounds.Height);
            drawContext.DrawText(
                this.DisplayText,
                textBounds,
                this.IsPlaceholder ? theme.Colors.SubtleForeground : theme.Colors.Foreground,
                this.CreateTextFormat());

            this.DrawGlyph(drawContext, new Rect(bounds.Width - GlyphWidth, 0, GlyphWidth, bounds.Height), theme.Colors.SubtleForeground);
        }

        protected override void OnThemeChanged(
            Theme theme)
        {
            base.OnThemeChanged(theme);

            this.InvalidateVisual();
        }

        // NOTE: Moves aren't claimed, so a drag that starts here can still scroll

        protected override void OnPointerPressed(
            PointerEventArgs e)
        {
            this.SetPressed(true);
            e.Handled = true;
        }

        protected override void OnPointerReleased(
            PointerEventArgs e)
        {
            this.SetPressed(false);
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
            this.SetPressed(false);
            e.Handled = true;
        }

        private void SetPressed(
            bool isPressed)
        {
            if (isPressed != this._isPressed)
            {
                this._isPressed = isPressed;
                this.InvalidateVisual();
            }
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

        protected override void OnGotFocus()
        {
            this.OnFocusChanged();
        }

        protected override void OnLostFocus()
        {
            this.OnFocusChanged();
        }

        private void OnFocusChanged()
        {
            this.InvalidateVisual();
        }

        /// <summary>
        /// Draws a downward chevron centered in the bounds.
        /// </summary>
        protected static void DrawChevronDown(
            DrawingContext drawContext,
            Rect bounds,
            Color color)
        {
            var center = bounds.Center;
            drawContext.DrawLine(new Point(center.X - 5, center.Y - 2.5f), new Point(center.X, center.Y + 2.5f), color, 1.25f);
            drawContext.DrawLine(new Point(center.X, center.Y + 2.5f), new Point(center.X + 5, center.Y - 2.5f), color, 1.25f);
        }

        /// <summary>
        /// A frame for a picker's popup: the app's background inside a thin border.
        /// </summary>
        protected internal static void DrawPopupFrame(
            DrawingContext drawContext,
            Rect bounds,
            Theme theme)
        {
            drawContext.FillRectangle(bounds, theme.Colors.Background);
            drawContext.DrawRectangle(
                bounds.Deflate(PopupBorderThickness / 2, PopupBorderThickness / 2),
                theme.Colors.SubtleForeground.MultiplyAlpha(0.4f),
                PopupBorderThickness);
        }

        protected internal const float PopupBorderThickness = 1.0f;
    }
}
