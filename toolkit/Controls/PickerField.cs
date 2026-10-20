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
                return AccessibilityRole.Button;
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
        protected abstract void DrawGlyph(IDrawContext drawContext, Rect bounds, Color color);

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
            float lineHeight = this.Application.MeasureText("Ag", this.CreateTextFormat()).Height;
            float width = TextInset + this.MaximumTextWidth + GlyphWidth;

            return new Size(
                Math.Min(width, availableSize.Width),
                Math.Min(lineHeight + (2 * VerticalPadding), availableSize.Height));
        }

        protected override void DrawBackground(
            IDrawContext drawContext,
            Rect bounds)
        {
            var theme = this.Theme;

            drawContext.Clear(Colors.Transparent);
            drawContext.FillRectangle(bounds, theme.SubtleBackgroundColor);
            if (this._isPressed)
            {
                drawContext.FillRectangle(bounds, theme.ForegroundColor.MultiplyAlpha(0.08f));
            }

            var textBounds = new Rect(TextInset, 0, Math.Max(0, bounds.Width - TextInset - GlyphWidth), bounds.Height);
            drawContext.DrawText(
                this.DisplayText,
                textBounds,
                this.IsPlaceholder ? theme.SubtleForegroundColor : theme.ForegroundColor,
                this.CreateTextFormat());

            this.DrawGlyph(drawContext, new Rect(bounds.Width - GlyphWidth, 0, GlyphWidth, bounds.Height), theme.SubtleForegroundColor);
        }

        protected override void OnApplyTheme(
            AppTheme theme)
        {
            base.OnApplyTheme(theme);

            this.InvalidateVisual();
        }

        public override bool HandlePointerEvent(
            PointerEventArgs pointerInputEvent)
        {
            if (base.HandlePointerEvent(pointerInputEvent))
            {
                return true;
            }

            switch (pointerInputEvent.State)
            {
                case PointerInputState.Began:
                    this.SetPressed(true);
                    return true;

                case PointerInputState.Ended:
                    {
                        this.SetPressed(false);
                        var point = pointerInputEvent.Point;
                        if ((point.X >= 0) && (point.Y >= 0) && (point.X < this.Bounds.Width) && (point.Y < this.Bounds.Height))
                        {
                            this.OpenPicker();
                        }

                        return true;
                    }

                case PointerInputState.Canceled:
                    this.SetPressed(false);
                    return true;
            }

            // NOTE: Moves aren't claimed, so a drag that starts here can still scroll
            return false;
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

        protected override bool OnKeyDown(
            KeyEventArgs keyInputEvent)
        {
            if ((keyInputEvent.Key == Key.F4) || (keyInputEvent.IsAltDown && (keyInputEvent.Key == Key.Down)))
            {
                this.OpenPicker();
                return true;
            }

            return false;
        }

        protected override void OnFocusChanged()
        {
            this.InvalidateVisual();
        }

        /// <summary>
        /// Draws a downward chevron centered in the bounds.
        /// </summary>
        protected static void DrawChevronDown(
            IDrawContext drawContext,
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
            IDrawContext drawContext,
            Rect bounds,
            AppTheme theme)
        {
            drawContext.FillRectangle(bounds, theme.BackgroundColor);
            drawContext.DrawRectangle(
                bounds.Deflate(PopupBorderThickness / 2, PopupBorderThickness / 2),
                theme.SubtleForegroundColor.MultiplyAlpha(0.4f),
                PopupBorderThickness);
        }

        protected internal const float PopupBorderThickness = 1.0f;
    }
}
