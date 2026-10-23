using System;
using XForms.Controls;

namespace XForms.Themes.Simple
{
    // The Simple look of feedback and disclosure: tooltips and menus on the
    // surface color in a thin frame, info bars tinted by their severity, and
    // expanders in an outline

    // The text on the surface color, in a thin rounded outline
    internal sealed class ToolTipTemplate : ControlTemplate<ToolTip>
    {
        protected override void Render(
            ToolTip control,
            DrawingContext drawContext)
        {
            var colors = control.ActualTheme.Colors;
            var bounds = new Rect(0, 0, control.Bounds.Width, control.Bounds.Height);
            var padding = control.Padding;

            drawContext.Clear(Colors.Transparent);
            drawContext.FillRoundedRectangle(bounds, 4, colors.Surface);
            drawContext.DrawRoundedRectangle(bounds.Deflate(0.5f, 0.5f), 4, colors.SubtleForeground.MultiplyAlpha(0.5f), 1);
            drawContext.DrawText(
                control.Text,
                new Rect(padding.Left, padding.Top, Math.Max(0, bounds.Width - padding.Left - padding.Right), Math.Max(0, bounds.Height - padding.Top - padding.Bottom)),
                colors.OnSurface,
                control.CreateTextFormat());
        }
    }

    // The items, the highlighted one on the surface color, with their icons
    internal sealed class MenuFlyoutViewTemplate : ControlTemplate<MenuFlyoutView>
    {
        protected override void Render(
            MenuFlyoutView control,
            DrawingContext drawContext)
        {
            var colors = control.ActualTheme.Colors;
            var items = control.Items;
            const float RowHeight = MenuFlyoutView.RowHeight;
            bool isRightToLeft = control.FlowDirection == FlowDirection.RightToLeft;
            float width = control.Bounds.Width;
            var format = control.CreateTextFormat();

            drawContext.Clear(Colors.Transparent);

            for (int i = 0; i < items.Count; i += 1)
            {
                var row = new Rect(0, i * RowHeight, width, RowHeight);
                var highlight = new Rect(4, row.Y + 2, Math.Max(0, width - 8), RowHeight - 4);
                bool isEnabled = control.IsItemEnabled(i);

                if (i == control.PressedIndex)
                {
                    drawContext.FillRoundedRectangle(highlight, 4, colors.Foreground.MultiplyAlpha(0.12f));
                }
                else if (i == control.HighlightedIndex)
                {
                    drawContext.FillRoundedRectangle(highlight, 4, colors.Surface);
                    if (control.IsFocused)
                    {
                        drawContext.FillRectangle(new Rect(isRightToLeft ? highlight.Right - 3 : 4, row.Y + ((RowHeight - 16) / 2), 3, 16), colors.Accent);
                    }
                }

                var color = isEnabled ? colors.Foreground : colors.Disabled;
                var icon = items[i].Icon;
                if (null != icon)
                {
                    const float IconSize = 16;
                    float x = isRightToLeft ? width - 12 - IconSize : 12;
                    drawContext.DrawImage(icon, new Rect(x, row.Y + ((RowHeight - IconSize) / 2), IconSize, IconSize), color);
                }

                float textX = isRightToLeft ? 12 : MenuFlyoutView.TextInset;
                drawContext.DrawText(items[i].Text, new Rect(textX, row.Y, Math.Max(0, width - MenuFlyoutView.TextInset - 12), RowHeight), color, format);
            }
        }
    }

    // Tinted by its severity, with a round icon in its color
    internal sealed class InfoBarTemplate : ControlTemplate<InfoBar>
    {
        private static Color SeverityColor(
            InfoBar control)
        {
            var colors = control.ActualTheme.Colors;
            switch (control.Severity)
            {
                case InfoBarSeverity.Success: return Color.FromRgb(0x2E, 0x9E, 0x44);
                case InfoBarSeverity.Warning: return Color.FromRgb(0xE8, 0x9E, 0x00);
                case InfoBarSeverity.Error: return colors.Error;
                default: return colors.Accent;
            }
        }

        protected override void Render(
            InfoBar control,
            DrawingContext drawContext)
        {
            var colors = control.ActualTheme.Colors;
            var bounds = new Rect(0, 0, control.Bounds.Width, control.Bounds.Height);
            var severity = SeverityColor(control);

            drawContext.Clear(Colors.Transparent);
            drawContext.FillRoundedRectangle(bounds, 6, colors.Surface);
            drawContext.FillRoundedRectangle(bounds, 6, severity.MultiplyAlpha(0.14f));
            drawContext.DrawRoundedRectangle(bounds.Deflate(0.5f, 0.5f), 6, severity.MultiplyAlpha(0.5f), 1);

            // The icon: a circle with a mark for the severity
            var icon = control.IconBounds;
            var c = icon.Center;
            float r = icon.Width / 2;
            drawContext.FillEllipse(c, r, r, severity);
            var mark = Colors.White;
            switch (control.Severity)
            {
                case InfoBarSeverity.Success:
                    drawContext.DrawLine(new Point(c.X - 4.5f, c.Y), new Point(c.X - 1.5f, c.Y + 3.5f), mark, 2);
                    drawContext.DrawLine(new Point(c.X - 1.5f, c.Y + 3.5f), new Point(c.X + 4.5f, c.Y - 3.5f), mark, 2);
                    break;

                case InfoBarSeverity.Error:
                    drawContext.DrawLine(new Point(c.X - 4, c.Y - 4), new Point(c.X + 4, c.Y + 4), mark, 2);
                    drawContext.DrawLine(new Point(c.X - 4, c.Y + 4), new Point(c.X + 4, c.Y - 4), mark, 2);
                    break;

                case InfoBarSeverity.Warning:
                    drawContext.DrawLine(new Point(c.X, c.Y - 5), new Point(c.X, c.Y + 1.5f), Colors.Black, 2);
                    drawContext.FillEllipse(new Point(c.X, c.Y + 4.5f), 1.2f, 1.2f, Colors.Black);
                    break;

                default:
                    drawContext.DrawLine(new Point(c.X, c.Y - 1.5f), new Point(c.X, c.Y + 5), mark, 2);
                    drawContext.FillEllipse(new Point(c.X, c.Y - 4.5f), 1.2f, 1.2f, mark);
                    break;
            }

            drawContext.DrawText(control.Title, control.TitleBounds, colors.Foreground, control.CreateTitleFormat());
            drawContext.DrawText(control.Message, control.MessageBounds, colors.Foreground, control.CreateMessageFormat());
        }
    }

    // A cross, in a circle while pressed
    internal sealed class InfoBarCloseButtonTemplate : ControlTemplate<InfoBarCloseButton>
    {
        protected override void Render(
            InfoBarCloseButton control,
            DrawingContext drawContext)
        {
            var colors = control.ActualTheme.Colors;
            var c = new Point(control.Bounds.Width / 2, control.Bounds.Height / 2);

            drawContext.Clear(Colors.Transparent);
            if (control.IsPressed)
            {
                drawContext.FillEllipse(c, 14, 14, colors.Foreground.MultiplyAlpha(0.12f));
            }

            drawContext.DrawLine(new Point(c.X - 5, c.Y - 5), new Point(c.X + 5, c.Y + 5), colors.Foreground, 1.25f);
            drawContext.DrawLine(new Point(c.X - 5, c.Y + 5), new Point(c.X + 5, c.Y - 5), colors.Foreground, 1.25f);
        }
    }

    // A rounded outline around the header and the content
    internal sealed class ExpanderTemplate : ControlTemplate<Expander>
    {
        protected override void Render(
            Expander control,
            DrawingContext drawContext)
        {
            var colors = control.ActualTheme.Colors;
            var bounds = new Rect(0, 0, control.Bounds.Width, control.Bounds.Height);

            drawContext.Clear(Colors.Transparent);
            drawContext.DrawRoundedRectangle(bounds.Deflate(0.5f, 0.5f), 6, colors.SubtleForeground.MultiplyAlpha(0.45f), 1);
        }
    }

    // The header's text, on the surface color, and a chevron pointing down
    // (up while expanded)
    internal sealed class ExpanderHeaderTemplate : ControlTemplate<ExpanderHeader>
    {
        protected override void Render(
            ExpanderHeader control,
            DrawingContext drawContext)
        {
            var colors = control.ActualTheme.Colors;
            var bounds = new Rect(0, 0, control.Bounds.Width, control.Bounds.Height);
            bool isRightToLeft = control.FlowDirection == FlowDirection.RightToLeft;
            const float Inset = 16;

            drawContext.Clear(Colors.Transparent);
            drawContext.FillRoundedRectangle(bounds.Deflate(1, 1), 5, colors.Surface);
            if (control.IsPressed)
            {
                drawContext.FillRoundedRectangle(bounds.Deflate(1, 1), 5, colors.Foreground.MultiplyAlpha(0.08f));
            }

            var color = control.Expander.IsEnabled ? colors.Foreground : colors.Disabled;
            drawContext.DrawText(control.Expander.Header, new Rect(Inset, 0, Math.Max(0, bounds.Width - (2 * Inset) - 24), bounds.Height), color, control.CreateTextFormat());

            float cx = isRightToLeft ? Inset + 6 : bounds.Width - Inset - 6;
            float cy = bounds.Height / 2;
            float direction = control.Expander.IsExpanded ? -1 : 1;
            drawContext.DrawLine(new Point(cx - 5, cy - (direction * 2.5f)), new Point(cx, cy + (direction * 2.5f)), color, 1.25f);
            drawContext.DrawLine(new Point(cx, cy + (direction * 2.5f)), new Point(cx + 5, cy - (direction * 2.5f)), color, 1.25f);
        }
    }
}
