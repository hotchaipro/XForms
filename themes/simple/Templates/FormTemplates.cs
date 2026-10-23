using System;
using System.Runtime.CompilerServices;
using XForms.Controls;

namespace XForms.Themes.Simple
{
    // The Simple look of the form controls: flat shapes in the accent color
    // for what's on, chosen or done, and the subtle foreground for the rest

    internal static class FormColors
    {
        // The color of what's on or chosen: the accent, or the disabled color
        public static Color Active(
            TemplatedControl control)
        {
            var colors = control.ActualTheme.Colors;
            return control.IsEnabled ? colors.Accent : colors.Disabled;
        }

        // The color of an outline or an empty track
        public static Color Inactive(
            TemplatedControl control)
        {
            var colors = control.ActualTheme.Colors;
            return control.IsEnabled ? colors.SubtleForeground : colors.Disabled;
        }

        public static Color Text(
            TemplatedControl control)
        {
            var colors = control.ActualTheme.Colors;
            return control.IsEnabled ? colors.Foreground : colors.Disabled;
        }

        // A press darkens (or, on a dark background, lightens) a part
        public static Color Pressed(
            TemplatedControl control)
        {
            return control.ActualTheme.Colors.Foreground.MultiplyAlpha(0.12f);
        }
    }

    // A rounded box, filled with a check mark when checked and a dash when
    // indeterminate, and the text after it
    internal sealed class CheckBoxTemplate : ControlTemplate<CheckBox>
    {
        protected override void Render(
            CheckBox control,
            DrawingContext drawContext)
        {
            var colors = control.ActualTheme.Colors;
            var box = control.BoxBounds;
            const float Radius = 4.0f;

            drawContext.Clear(Colors.Transparent);

            if (control.IsChecked != false)
            {
                var fill = FormColors.Active(control);
                drawContext.FillRoundedRectangle(box, Radius, control.IsPressed ? fill.MultiplyAlpha(0.8f) : fill);

                var mark = control.IsEnabled ? colors.OnAccent : colors.Background;
                var center = box.Center;
                if (control.IsChecked == true)
                {
                    drawContext.DrawLine(new Point(center.X - 5, center.Y), new Point(center.X - 1.5f, center.Y + 3.5f), mark, 2);
                    drawContext.DrawLine(new Point(center.X - 1.5f, center.Y + 3.5f), new Point(center.X + 5, center.Y - 4), mark, 2);
                }
                else
                {
                    drawContext.DrawLine(new Point(center.X - 5, center.Y), new Point(center.X + 5, center.Y), mark, 2);
                }
            }
            else
            {
                if (control.IsPressed)
                {
                    drawContext.FillRoundedRectangle(box, Radius, FormColors.Pressed(control));
                }

                drawContext.DrawRoundedRectangle(box.Deflate(0.75f, 0.75f), Radius, FormColors.Inactive(control), 1.5f);
            }

            drawContext.DrawText(control.Text, control.TextBounds, FormColors.Text(control), control.CreateTextFormat());
        }
    }

    // A pill-shaped track with a round knob: filled in the accent color with
    // the knob at the end when on, outlined with it at the start when off
    internal sealed class ToggleSwitchTemplate : ControlTemplate<ToggleSwitch>
    {
        protected override void Render(
            ToggleSwitch control,
            DrawingContext drawContext)
        {
            var colors = control.ActualTheme.Colors;
            var track = control.TrackBounds;
            float radius = track.Height / 2;
            bool isRightToLeft = control.FlowDirection == FlowDirection.RightToLeft;

            drawContext.Clear(Colors.Transparent);

            if (!string.IsNullOrEmpty(control.Header))
            {
                var headerFormat = control.CreateHeaderFormat();
                headerFormat.HorizontalAlignment = isRightToLeft ? HorizontalAlignment.Right : HorizontalAlignment.Left;
                drawContext.DrawText(control.Header, control.HeaderBounds, FormColors.Text(control), headerFormat);
            }

            // The knob sits at the end when on (the end is the left, right to left)
            bool knobAtRight = control.IsOn != isRightToLeft;
            float knobRadius = control.IsPressed ? radius - 2 : radius - 4;
            var knobCenter = new Point(knobAtRight ? track.Right - radius : track.X + radius, track.Center.Y);

            if (control.IsOn)
            {
                drawContext.FillRoundedRectangle(track, radius, FormColors.Active(control));
                drawContext.FillEllipse(knobCenter, knobRadius, knobRadius, control.IsEnabled ? colors.OnAccent : colors.Background);
            }
            else
            {
                var color = FormColors.Inactive(control);
                drawContext.DrawRoundedRectangle(track.Deflate(0.75f, 0.75f), radius - 0.75f, color, 1.5f);
                drawContext.FillEllipse(knobCenter, knobRadius, knobRadius, color);
            }

            drawContext.DrawText(control.StateText, control.TextBounds, FormColors.Text(control), control.CreateTextFormat());
        }
    }

    // A thin track, filled in the accent color up to a round thumb, which
    // grows while it's dragged
    internal sealed class SliderTemplate : ControlTemplate<Slider>
    {
        private const float TrackThickness = 4.0f;

        protected override void Render(
            Slider control,
            DrawingContext drawContext)
        {
            var colors = control.ActualTheme.Colors;
            var track = control.TrackBounds;
            bool isRightToLeft = control.FlowDirection == FlowDirection.RightToLeft;

            drawContext.Clear(Colors.Transparent);

            if (!string.IsNullOrEmpty(control.Header))
            {
                var format = control.ActualTheme.Typography.Label.ToTextFormat();
                format.HorizontalAlignment = isRightToLeft ? HorizontalAlignment.Right : HorizontalAlignment.Left;
                drawContext.DrawText(control.Header, control.HeaderBounds, FormColors.Text(control), format);
            }

            float half = TrackThickness / 2;
            var line = new Rect(track.X - half, track.Y - half, track.Width + TrackThickness, TrackThickness);
            drawContext.FillRoundedRectangle(line, half, FormColors.Inactive(control).MultiplyAlpha(0.45f));

            // Filled from the start (the right, right to left) to the thumb
            float position = control.ValuePosition;
            var filled = isRightToLeft
                ? new Rect(position, line.Y, line.Right - position, TrackThickness)
                : new Rect(line.X, line.Y, position - line.X, TrackThickness);
            drawContext.FillRoundedRectangle(filled, half, FormColors.Active(control));

            // A ring in the background color around an accent dot
            var thumb = control.ThumbCenter;
            const float Radius = Slider.ThumbSize / 2;
            float dot = control.IsDragging ? Radius - 3 : (control.IsPressed ? Radius - 6 : Radius - 5);
            drawContext.FillEllipse(thumb, Radius, Radius, colors.Background);
            drawContext.DrawEllipse(thumb, Radius - 0.5f, Radius - 0.5f, FormColors.Inactive(control).MultiplyAlpha(0.45f), 1);
            drawContext.FillEllipse(thumb, dot, dot, FormColors.Active(control));
        }
    }

    // A thin track, filled in the accent color to the value; while
    // indeterminate, a segment slides along it, on a layer of its own (so it
    // keeps moving while the UI thread is busy)
    internal sealed class ProgressBarTemplate : ControlTemplate<ProgressBar>
    {
        private static readonly TimeSpan SweepPeriod = TimeSpan.FromSeconds(1.5);
        private const float SegmentFraction = 0.4f;

        private sealed class Parts
        {
            public Border Segment;
            public float Width;
            public bool IsSweeping;
        }

        protected override void Apply(
            ProgressBar control,
            TemplateContext context)
        {
            var segment = new Border()
            {
                Animator = new Animator(),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Stretch,
                CornerRadius = ProgressBar.DefaultHeight / 2,
                Background = ColorFor(control),
                IsVisible = false,
                IsHitTestVisible = false,
            };
            control.ClipToBounds = true;
            context.AddPart(segment);
            var parts = new Parts() { Segment = segment };
            context.State = parts;
            PartsOf.AddOrUpdate(control, parts);
        }

        protected override void Unapply(
            ProgressBar control,
            TemplateContext context)
        {
            var parts = (Parts)context.State;
            parts.Segment.Animator.Stop();
            context.RemovePart(parts.Segment);
            PartsOf.Remove(control);
        }

        private static Color ColorFor(
            ProgressBar control)
        {
            var colors = control.ActualTheme.Colors;
            if (control.ShowError)
            {
                return colors.Error;
            }

            return control.ShowPaused ? colors.SubtleForeground : colors.Accent;
        }

        protected override void OnThemeChanged(
            ProgressBar control,
            TemplateContext context)
        {
            ((Parts)context.State).Segment.Background = ColorFor(control);
        }

        protected override void OnStateChanged(
            ProgressBar control,
            TemplateContext context)
        {
            var parts = (Parts)context.State;
            parts.Segment.Background = ColorFor(control);
            this.UpdateSweep(control, parts, control.Bounds.Width);
        }

        // Each bar's parts, for Arrange (which has no context)
        private static readonly ConditionalWeakTable<ProgressBar, Parts> PartsOf = new ConditionalWeakTable<ProgressBar, Parts>();

        // The segment's length follows the bar's width
        protected override bool Arrange(
            ProgressBar control,
            Size finalSize)
        {
            if (PartsOf.TryGetValue(control, out var parts))
            {
                this.UpdateSweep(control, parts, finalSize.Width);
            }

            return false;
        }

        // Starts or stops the segment, and restarts it when the bar's width
        // changes
        private void UpdateSweep(
            ProgressBar control,
            Parts parts,
            float width)
        {
            bool sweep = control.IsIndeterminate && (width > 0);
            if ((sweep == parts.IsSweeping) && (!sweep || (width == parts.Width)))
            {
                return;
            }

            var animator = parts.Segment.Animator;
            animator.Stop();
            parts.IsSweeping = sweep;
            parts.Width = width;
            parts.Segment.IsVisible = sweep;
            if (!sweep)
            {
                return;
            }

            // From just before the start to just past the end (leftward, right
            // to left)
            float length = width * SegmentFraction;
            parts.Segment.Width = length;
            float direction = (control.FlowDirection == FlowDirection.RightToLeft) ? -1 : 1;
            animator.Translation = new Point(-direction * length, 0);
            animator.TranslateTo(new Point(direction * width, 0), SweepPeriod, control.ActualTheme.Motion.Standard, loop: true);
        }

        protected override void Render(
            ProgressBar control,
            DrawingContext drawContext)
        {
            var bounds = new Rect(0, 0, control.Bounds.Width, control.Bounds.Height);
            float radius = bounds.Height / 2;

            drawContext.Clear(Colors.Transparent);
            drawContext.FillRoundedRectangle(bounds, radius, control.ActualTheme.Colors.SubtleForeground.MultiplyAlpha(0.3f));

            if (!control.IsIndeterminate)
            {
                float length = (float)(control.Fraction * bounds.Width);
                var filled = (control.FlowDirection == FlowDirection.RightToLeft)
                    ? new Rect(bounds.Right - length, 0, length, bounds.Height)
                    : new Rect(0, 0, length, bounds.Height);
                drawContext.FillRoundedRectangle(filled, radius, ColorFor(control));
            }
        }
    }

    internal sealed class TimePickerTemplate : PickerFieldTemplate<TimePicker>
    {
        // A clock face with its hands
        protected override void RenderGlyph(
            DrawingContext drawContext,
            Rect bounds,
            Color color)
        {
            var center = bounds.Center;
            drawContext.DrawEllipse(center, 7, 7, color, 1.0f);
            drawContext.DrawLine(center, new Point(center.X, center.Y - 4.5f), color, 1.0f);
            drawContext.DrawLine(center, new Point(center.X + 3, center.Y), color, 1.0f);
        }
    }

    // The time's columns, with the chosen values in an accent band across the
    // middle, and Accept and Dismiss beneath
    internal sealed class TimePickerViewTemplate : ControlTemplate<TimePickerView>
    {
        private static readonly TextFormat RowFormat = new TextFormat()
        {
            FontSize = 15,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
        };

        protected override void Render(
            TimePickerView control,
            DrawingContext drawContext)
        {
            var colors = control.ActualTheme.Colors;
            const int Middle = TimePickerView.VisibleRows / 2;

            drawContext.Clear(Colors.Transparent);

            var band = new Rect(4, Middle * TimePickerView.RowHeight, Math.Max(0, control.Bounds.Width - 8), TimePickerView.RowHeight);
            drawContext.FillRoundedRectangle(band, 4, colors.Accent);

            for (int column = 0; column < control.ColumnCount; column += 1)
            {
                for (int row = 0; row < TimePickerView.VisibleRows; row += 1)
                {
                    var bounds = control.GetRowBounds(column, row);
                    bool isPressed = (control.PressedPart == TimePickerViewPart.Column) && (control.PressedColumn == column) && (control.PressedRow == row);
                    if (isPressed && (row != Middle))
                    {
                        drawContext.FillRectangle(bounds.Deflate(4, 2), FormPressed(colors));
                    }

                    var color = (row == Middle) ? colors.OnAccent : colors.SubtleForeground;
                    drawContext.DrawText(control.GetText(column, row - Middle), bounds, color, RowFormat);
                }

                if (control.IsFocused && (column == control.FocusedColumn))
                {
                    var bounds = control.GetRowBounds(column, Middle);
                    drawContext.DrawRoundedRectangle(bounds.Deflate(6, 4), 4, colors.OnAccent, 2);
                }
            }

            // Accept and Dismiss, beneath a line
            var accept = control.AcceptBounds;
            var dismiss = control.DismissBounds;
            drawContext.DrawLine(new Point(0, accept.Y), new Point(control.Bounds.Width, accept.Y), colors.SubtleForeground.MultiplyAlpha(0.4f), 1);
            if (control.PressedPart == TimePickerViewPart.Accept)
            {
                drawContext.FillRectangle(accept, FormPressed(colors));
            }

            if (control.PressedPart == TimePickerViewPart.Dismiss)
            {
                drawContext.FillRectangle(dismiss, FormPressed(colors));
            }

            var a = accept.Center;
            drawContext.DrawLine(new Point(a.X - 7, a.Y), new Point(a.X - 2, a.Y + 5), colors.Foreground, 1.5f);
            drawContext.DrawLine(new Point(a.X - 2, a.Y + 5), new Point(a.X + 7, a.Y - 5), colors.Foreground, 1.5f);

            var d = dismiss.Center;
            drawContext.DrawLine(new Point(d.X - 6, d.Y - 6), new Point(d.X + 6, d.Y + 6), colors.Foreground, 1.5f);
            drawContext.DrawLine(new Point(d.X - 6, d.Y + 6), new Point(d.X + 6, d.Y - 6), colors.Foreground, 1.5f);
        }

        private static Color FormPressed(
            ColorScheme colors)
        {
            return colors.Foreground.MultiplyAlpha(0.12f);
        }
    }

    // Up and down chevrons at the end of the box, on the surface color the
    // text box shares
    internal sealed class NumberBoxTemplate : ControlTemplate<NumberBox>
    {
        protected override void Render(
            NumberBox control,
            DrawingContext drawContext)
        {
            drawContext.Clear(Colors.Transparent);
            if (control.SpinButtonPlacementMode != NumberBoxSpinButtonPlacementMode.Inline)
            {
                return;
            }

            var colors = control.ActualTheme.Colors;
            var color = FormColors.Inactive(control);
            RenderButton(control, drawContext, control.UpButtonBounds, -1, control.PressedPart == NumberBoxPart.UpButton, color);
            RenderButton(control, drawContext, control.DownButtonBounds, 1, control.PressedPart == NumberBoxPart.DownButton, color);
        }

        private static void RenderButton(
            NumberBox control,
            DrawingContext drawContext,
            Rect bounds,
            int direction,
            bool isPressed,
            Color color)
        {
            drawContext.FillRectangle(bounds, control.ActualTheme.Colors.Surface);
            if (isPressed)
            {
                drawContext.FillRectangle(bounds, FormColors.Pressed(control));
            }

            var center = bounds.Center;
            float tip = center.Y + (direction * 2.5f);
            float tail = center.Y - (direction * 2.5f);
            drawContext.DrawLine(new Point(center.X - 5, tail), new Point(center.X, tip), color, 1.25f);
            drawContext.DrawLine(new Point(center.X, tip), new Point(center.X + 5, tail), color, 1.25f);
        }
    }

    // An eye at the end of the box, on the surface color the text box shares
    internal sealed class PasswordBoxTemplate : ControlTemplate<PasswordBox>
    {
        protected override void Render(
            PasswordBox control,
            DrawingContext drawContext)
        {
            drawContext.Clear(Colors.Transparent);
            if (control.PasswordRevealMode != PasswordRevealMode.Peek)
            {
                return;
            }

            var bounds = control.RevealButtonBounds;
            drawContext.FillRectangle(bounds, control.ActualTheme.Colors.Surface);
            if (!control.IsRevealButtonVisible)
            {
                return;
            }

            if (control.IsPasswordRevealed)
            {
                drawContext.FillRectangle(bounds, FormColors.Pressed(control));
            }

            var color = FormColors.Inactive(control);
            var center = bounds.Center;
            drawContext.DrawEllipse(center, 7.5f, 4.5f, color, 1.25f);
            drawContext.FillEllipse(center, 2, 2, color);
        }
    }

    // The box is its text box alone
    internal sealed class AutoSuggestBoxTemplate : ControlTemplate<AutoSuggestBox>
    {
        protected override void Render(
            AutoSuggestBox control,
            DrawingContext drawContext)
        {
            drawContext.Clear(Colors.Transparent);
        }
    }

    // The suggestions, the one the keyboard is on on the surface color
    internal sealed class AutoSuggestBoxListViewTemplate : ControlTemplate<AutoSuggestBoxListView>
    {
        protected override void Render(
            AutoSuggestBoxListView control,
            DrawingContext drawContext)
        {
            var colors = control.ActualTheme.Colors;
            var box = control.AutoSuggestBox;
            var items = box.ItemsSource;
            const float RowHeight = AutoSuggestBoxListView.RowHeight;
            bool isRightToLeft = control.FlowDirection == FlowDirection.RightToLeft;

            drawContext.Clear(Colors.Transparent);

            var format = control.ActualTheme.Typography.Body.ToTextFormat();
            format.HorizontalAlignment = isRightToLeft ? HorizontalAlignment.Right : HorizontalAlignment.Left;
            format.VerticalAlignment = VerticalAlignment.Center;
            const float Inset = 11.0f;

            for (int i = 0; i < box.SuggestionCount; i += 1)
            {
                var row = new Rect(0, i * RowHeight, control.Bounds.Width, RowHeight);
                var highlight = new Rect(4, row.Y + 2, Math.Max(0, control.Bounds.Width - 8), RowHeight - 4);
                if (i == control.PressedIndex)
                {
                    drawContext.FillRectangle(highlight, colors.Foreground.MultiplyAlpha(0.12f));
                }
                else if (i == control.HighlightedIndex)
                {
                    drawContext.FillRectangle(highlight, colors.Surface);
                    drawContext.FillRectangle(new Rect(isRightToLeft ? highlight.Right - 3 : 4, row.Y + ((RowHeight - 16) / 2), 3, 16), colors.Accent);
                }

                drawContext.DrawText(items[i], new Rect(Inset, row.Y, Math.Max(0, control.Bounds.Width - (2 * Inset)), RowHeight), colors.Foreground, format);
            }
        }
    }
}
