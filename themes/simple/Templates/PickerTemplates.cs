using System;
using System.Globalization;
using XForms.Controls;

namespace XForms.Themes.Simple
{
    // The Simple look of the pickers: flat fields on the surface color, and
    // popups in a thin frame

    // A picker's field: the value (or a placeholder) and a glyph at the end,
    // darkening while pressed
    internal abstract class PickerFieldTemplate<TControl> : ControlTemplate<TControl>
        where TControl : PickerField
    {
        // The glyph, centered in its area
        protected abstract void RenderGlyph(DrawingContext drawContext, Rect bounds, Color color);

        protected override void Render(
            TControl control,
            DrawingContext drawContext)
        {
            var bounds = new Rect(0, 0, control.Bounds.Width, control.Bounds.Height);
            var colors = control.ActualTheme.Colors;

            drawContext.Clear(Colors.Transparent);
            drawContext.FillRectangle(bounds, colors.Surface);
            if (control.IsPressed)
            {
                drawContext.FillRectangle(bounds, colors.Foreground.MultiplyAlpha(0.08f));
            }

            var textBounds = new Rect(PickerField.TextInset, 0, Math.Max(0, bounds.Width - PickerField.TextInset - PickerField.GlyphWidth), bounds.Height);
            drawContext.DrawText(
                control.DisplayText,
                textBounds,
                control.IsPlaceholder ? colors.SubtleForeground : colors.Foreground,
                control.CreateTextFormat());

            this.RenderGlyph(drawContext, new Rect(bounds.Width - PickerField.GlyphWidth, 0, PickerField.GlyphWidth, bounds.Height), colors.SubtleForeground);
        }

        // A downward chevron
        protected static void RenderChevronDown(
            DrawingContext drawContext,
            Rect bounds,
            Color color)
        {
            var center = bounds.Center;
            drawContext.DrawLine(new Point(center.X - 5, center.Y - 2.5f), new Point(center.X, center.Y + 2.5f), color, 1.25f);
            drawContext.DrawLine(new Point(center.X, center.Y + 2.5f), new Point(center.X + 5, center.Y - 2.5f), color, 1.25f);
        }
    }

    internal sealed class ComboBoxTemplate : PickerFieldTemplate<ComboBox>
    {
        protected override void RenderGlyph(
            DrawingContext drawContext,
            Rect bounds,
            Color color)
        {
            RenderChevronDown(drawContext, bounds, color);
        }
    }

    internal sealed class CalendarDatePickerTemplate : PickerFieldTemplate<CalendarDatePicker>
    {
        // A calendar page: a box with a band across the top and two rings
        protected override void RenderGlyph(
            DrawingContext drawContext,
            Rect bounds,
            Color color)
        {
            var center = bounds.Center;
            var page = new Rect(center.X - 7, center.Y - 5.5f, 14, 12);
            drawContext.DrawRectangle(page, color, 1.0f);
            drawContext.DrawLine(new Point(page.X, page.Y + 3.5f), new Point(page.Right, page.Y + 3.5f), color, 1.0f);
            drawContext.DrawLine(new Point(page.X + 3.5f, page.Y - 2), new Point(page.X + 3.5f, page.Y + 1), color, 1.0f);
            drawContext.DrawLine(new Point(page.Right - 3.5f, page.Y - 2), new Point(page.Right - 3.5f, page.Y + 1), color, 1.0f);
        }
    }

    // A popup's frame: the app's background inside a thin border
    internal sealed class PopupFrameTemplate : ControlTemplate<PopupFrame>
    {
        protected override void Render(
            PopupFrame control,
            DrawingContext drawContext)
        {
            var bounds = new Rect(0, 0, control.Bounds.Width, control.Bounds.Height);
            var colors = control.ActualTheme.Colors;
            const float Thickness = PopupFrame.BorderThickness;

            drawContext.Clear(Colors.Transparent);
            drawContext.FillRectangle(bounds, colors.Background);
            drawContext.DrawRectangle(
                bounds.Deflate(Thickness / 2, Thickness / 2),
                colors.SubtleForeground.MultiplyAlpha(0.4f),
                Thickness);
        }
    }

    // A combo box's rows: pressed, highlighted or selected rows on the
    // surface color, and an accent mark by the selected one
    internal sealed class ComboBoxItemsViewTemplate : ControlTemplate<ComboBoxItemsView>
    {
        protected override void Render(
            ComboBoxItemsView control,
            DrawingContext drawContext)
        {
            var bounds = new Rect(0, 0, control.Bounds.Width, control.Bounds.Height);
            var colors = control.ActualTheme.Colors;
            var comboBox = control.ComboBox;
            var items = comboBox.Items;
            int selectedIndex = comboBox.SelectedIndex;
            const float RowHeight = ComboBoxItemsView.RowHeight;

            drawContext.Clear(Colors.Transparent);

            var textFormat = comboBox.CreateTextFormat();

            for (int i = 0; i < items.Count; i += 1)
            {
                var row = new Rect(0, i * RowHeight, bounds.Width, RowHeight);
                var highlight = new Rect(4, row.Y + 2, Math.Max(0, bounds.Width - 8), RowHeight - 4);

                if (i == control.PressedIndex)
                {
                    drawContext.FillRectangle(highlight, colors.Foreground.MultiplyAlpha(0.12f));
                }
                else if ((i == selectedIndex) || ((i == control.HighlightedIndex) && control.IsFocused))
                {
                    drawContext.FillRectangle(highlight, colors.Surface);
                }

                if (i == selectedIndex)
                {
                    drawContext.FillRectangle(new Rect(4, row.Y + ((RowHeight - 16) / 2), 3, 16), colors.Accent);
                }

                drawContext.DrawText(
                    items[i].Text,
                    new Rect(PickerField.TextInset, row.Y, Math.Max(0, bounds.Width - (2 * PickerField.TextInset)), RowHeight),
                    colors.Foreground,
                    textFormat);
            }
        }
    }

    // A month: its name and arrows, the days of the week, and the days, with
    // the selected day filled and today ringed
    internal sealed class CalendarViewTemplate : ControlTemplate<CalendarView>
    {
        private const float DayFontSize = 14.0f;

        private static readonly TextFormat WeekdayFormat = new TextFormat()
        {
            FontSize = 12,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
        };

        private static readonly TextFormat DayFormat = new TextFormat()
        {
            FontSize = DayFontSize,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
        };

        protected override void Render(
            CalendarView control,
            DrawingContext drawContext)
        {
            var colors = control.ActualTheme.Colors;
            var culture = CultureInfo.CurrentCulture;

            drawContext.Clear(Colors.Transparent);

            // Header: the month, and the arrows to change it
            drawContext.DrawText(
                control.DisplayedMonth.ToString("MMMM yyyy", culture),
                control.HeaderBounds,
                colors.Foreground,
                new TextFormat()
                {
                    FontSize = 16,
                    FontWeight = FontWeight.SemiBold,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Center,
                });

            RenderArrowButton(control, drawContext, control.PreviousButtonBounds, -1, control.CanShowPreviousMonth, CalendarViewPart.PreviousMonth);
            RenderArrowButton(control, drawContext, control.NextButtonBounds, 1, true, CalendarViewPart.NextMonth);

            // Days of the week, starting on Monday
            for (int column = 0; column < 7; column += 1)
            {
                var dayOfWeek = (DayOfWeek)((column + 1) % 7);
                drawContext.DrawText(
                    culture.DateTimeFormat.GetShortestDayName(dayOfWeek),
                    control.GetWeekdayBounds(column),
                    colors.SubtleForeground,
                    WeekdayFormat);
            }

            // Days
            var today = DateTime.Today;
            var date = control.FirstShownDay;
            var month = control.DisplayedMonth.Month;

            for (int row = 0; row < 6; row += 1)
            {
                for (int column = 0; column < 7; column += 1, date = date.AddDays(1))
                {
                    var cell = control.GetDayBounds(row, column);
                    var center = cell.Center;
                    const float Radius = (CalendarView.CellSize / 2) - 3;

                    bool isEnabled = control.IsDateEnabled(date);
                    bool isInMonth = date.Month == month;
                    bool isSelected = control.SelectedDate == date;

                    var textColor = isInMonth ? colors.Foreground : colors.SubtleForeground;
                    if (!isEnabled)
                    {
                        textColor = colors.SubtleForeground.MultiplyAlpha(0.45f);
                    }

                    if (isSelected)
                    {
                        drawContext.FillEllipse(center, Radius, Radius, colors.Accent);
                        textColor = colors.Background;
                    }
                    else if ((control.PressedPart == CalendarViewPart.Day) && (control.PressedDate == date) && isEnabled)
                    {
                        drawContext.FillEllipse(center, Radius, Radius, colors.Foreground.MultiplyAlpha(0.12f));
                    }

                    if (date == today)
                    {
                        drawContext.DrawEllipse(center, Radius, Radius, isSelected ? colors.Background : colors.Accent, 1.5f);
                    }

                    if (control.IsFocused && (date == control.FocusedDate))
                    {
                        drawContext.DrawEllipse(center, Radius + 1.5f, Radius + 1.5f, colors.Foreground, 2);
                    }

                    drawContext.DrawText(date.Day.ToString(culture), cell, textColor, DayFormat);
                }
            }
        }

        // The month arrows, which point up and down as the months scroll in
        // Windows' calendar
        private static void RenderArrowButton(
            CalendarView control,
            DrawingContext drawContext,
            Rect bounds,
            int direction,
            bool isEnabled,
            CalendarViewPart part)
        {
            var colors = control.ActualTheme.Colors;
            var center = bounds.Center;

            if (isEnabled && (control.PressedPart == part))
            {
                drawContext.FillEllipse(center, 16, 16, colors.Foreground.MultiplyAlpha(0.12f));
            }

            var color = isEnabled ? colors.Foreground : colors.SubtleForeground.MultiplyAlpha(0.45f);
            float tip = center.Y + (direction * 2.5f);
            float tail = center.Y - (direction * 2.5f);
            drawContext.DrawLine(new Point(center.X - 5, tail), new Point(center.X, tip), color, 1.25f);
            drawContext.DrawLine(new Point(center.X, tip), new Point(center.X + 5, tail), color, 1.25f);
        }
    }
}
