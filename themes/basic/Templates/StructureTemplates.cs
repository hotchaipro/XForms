using System;
using XForms.Controls;

namespace XForms.Themes.Basic
{
    // The Basic look of the structural controls: the navigation pane on the
    // surface color with the selected item marked in the accent color, flip
    // view arrows and pips, the refresh indicator, and tree items

    // On a narrow window, the bar at the top with the pane's title
    internal sealed class NavigationViewTemplate : ControlTemplate<NavigationView>
    {
        protected override void Render(
            NavigationView control,
            DrawingContext drawContext)
        {
            var colors = control.ActualTheme.Colors;
            drawContext.Clear(Colors.Transparent);
            if (!control.IsPaneDrawer)
            {
                return;
            }

            var bar = control.BarBounds;
            drawContext.FillRectangle(bar, colors.Surface);
            var format = control.ActualTheme.Typography.Title.ToTextFormat();
            format.VerticalAlignment = VerticalAlignment.Center;
            bool isRightToLeft = control.FlowDirection == FlowDirection.RightToLeft;
            format.HorizontalAlignment = isRightToLeft ? HorizontalAlignment.Right : HorizontalAlignment.Left;
            float inset = NavigationView.BarHeight + 4;
            drawContext.DrawText(control.PaneTitle, new Rect(inset, 0, Math.Max(0, bar.Width - (2 * inset)), bar.Height), colors.Foreground, format);
        }
    }

    // The surface color, with the title at the top
    internal sealed class NavigationViewPaneTemplate : ControlTemplate<NavigationViewPane>
    {
        protected override void Render(
            NavigationViewPane control,
            DrawingContext drawContext)
        {
            var colors = control.ActualTheme.Colors;
            var bounds = new Rect(0, 0, control.Bounds.Width, control.Bounds.Height);
            bool isRightToLeft = control.FlowDirection == FlowDirection.RightToLeft;

            drawContext.Clear(Colors.Transparent);
            drawContext.FillRectangle(bounds, colors.Surface);

            // An edge beside the content
            float edge = isRightToLeft ? 0.5f : bounds.Width - 0.5f;
            drawContext.DrawLine(new Point(edge, 0), new Point(edge, bounds.Height), colors.SubtleForeground.MultiplyAlpha(0.3f), 1);

            var format = control.ActualTheme.Typography.Title.ToTextFormat();
            format.VerticalAlignment = VerticalAlignment.Center;
            format.HorizontalAlignment = isRightToLeft ? HorizontalAlignment.Right : HorizontalAlignment.Left;
            var title = control.TitleBounds;
            drawContext.DrawText(control.NavigationView.PaneTitle, new Rect(16, title.Y, Math.Max(0, bounds.Width - 32), title.Height), colors.Foreground, format);
        }
    }

    // Its icon and text; selected, on a highlight with an accent pill at
    // the start
    internal sealed class NavigationViewItemTemplate : ControlTemplate<NavigationViewItem>
    {
        protected override void Render(
            NavigationViewItem control,
            DrawingContext drawContext)
        {
            var colors = control.ActualTheme.Colors;
            var bounds = new Rect(0, 0, control.Bounds.Width, control.Bounds.Height);
            bool isRightToLeft = control.FlowDirection == FlowDirection.RightToLeft;

            drawContext.Clear(Colors.Transparent);
            if (control.IsPressed)
            {
                drawContext.FillRoundedRectangle(bounds, 4, colors.Foreground.MultiplyAlpha(0.12f));
            }
            else if (control.IsSelected)
            {
                drawContext.FillRoundedRectangle(bounds, 4, colors.Foreground.MultiplyAlpha(0.07f));
            }

            if (control.IsSelected)
            {
                float x = isRightToLeft ? bounds.Right - 3 : 0;
                drawContext.FillRoundedRectangle(new Rect(x, (bounds.Height - 16) / 2, 3, 16), 1.5f, colors.Accent);
            }

            const float IconSize = 18;
            float textStart = 16;
            if (null != control.Icon)
            {
                float iconX = isRightToLeft ? bounds.Width - 14 - IconSize : 14;
                drawContext.DrawImage(control.Icon, new Rect(iconX, (bounds.Height - IconSize) / 2, IconSize, IconSize), colors.Foreground);
                textStart = 44;
            }

            drawContext.DrawText(control.Text, new Rect(isRightToLeft ? 12 : textStart, 0, Math.Max(0, bounds.Width - textStart - 12), bounds.Height), colors.Foreground, control.CreateTextFormat());
        }
    }

    // Three lines
    internal sealed class NavigationViewMenuButtonTemplate : ControlTemplate<NavigationViewMenuButton>
    {
        protected override void Render(
            NavigationViewMenuButton control,
            DrawingContext drawContext)
        {
            var colors = control.ActualTheme.Colors;
            var c = new Point(control.Bounds.Width / 2, control.Bounds.Height / 2);

            drawContext.Clear(Colors.Transparent);
            if (control.IsPressed)
            {
                drawContext.FillRoundedRectangle(new Rect(c.X - 18, c.Y - 18, 36, 36), 4, colors.Foreground.MultiplyAlpha(0.12f));
            }

            for (int i = -1; i <= 1; i += 1)
            {
                drawContext.DrawLine(new Point(c.X - 8, c.Y + (i * 5)), new Point(c.X + 8, c.Y + (i * 5)), colors.Foreground, 1.5f);
            }
        }
    }

    // A shade over the content
    internal sealed class NavigationViewScrimTemplate : ControlTemplate<NavigationViewScrim>
    {
        protected override void Render(
            NavigationViewScrim control,
            DrawingContext drawContext)
        {
            drawContext.Clear(Colors.Transparent);
            drawContext.FillRectangle(new Rect(0, 0, control.Bounds.Width, control.Bounds.Height), Colors.Black.MultiplyAlpha(0.35f));
        }
    }

    // The flip view itself draws nothing: its views and buttons do
    internal sealed class FlipViewTemplate : ControlTemplate<FlipView>
    {
        protected override void Render(
            FlipView control,
            DrawingContext drawContext)
        {
            drawContext.Clear(Colors.Transparent);
        }
    }

    // A chevron on a translucent rounded background
    internal sealed class FlipViewButtonTemplate : ControlTemplate<FlipViewButton>
    {
        protected override void Render(
            FlipViewButton control,
            DrawingContext drawContext)
        {
            var colors = control.ActualTheme.Colors;
            var bounds = new Rect(0, 0, control.Bounds.Width, control.Bounds.Height);
            var c = bounds.Center;
            bool isRightToLeft = control.FlowDirection == FlowDirection.RightToLeft;

            drawContext.Clear(Colors.Transparent);
            drawContext.FillRoundedRectangle(bounds.Deflate(2, 0), 4, colors.Surface.MultiplyAlpha(control.IsPressed ? 0.95f : 0.75f));

            // Points away from the content's middle: left for previous (right,
            // right to left)
            float direction = ((control.Direction < 0) != isRightToLeft) ? -1 : 1;
            drawContext.DrawLine(new Point(c.X - (direction * 2.5f), c.Y - 6), new Point(c.X + (direction * 2.5f), c.Y), colors.Foreground, 1.5f);
            drawContext.DrawLine(new Point(c.X + (direction * 2.5f), c.Y), new Point(c.X - (direction * 2.5f), c.Y + 6), colors.Foreground, 1.5f);
        }
    }

    // A dot for each page, the selected one larger, in the accent color
    internal sealed class PipsPagerTemplate : ControlTemplate<PipsPager>
    {
        protected override void Render(
            PipsPager control,
            DrawingContext drawContext)
        {
            var colors = control.ActualTheme.Colors;
            drawContext.Clear(Colors.Transparent);
            for (int i = 0; i < control.NumberOfPages; i += 1)
            {
                var center = control.GetDotCenter(i);
                if (i == control.SelectedPageIndex)
                {
                    drawContext.FillEllipse(center, 5, 5, colors.Accent);
                }
                else
                {
                    drawContext.FillEllipse(center, 3, 3, colors.SubtleForeground);
                }
            }
        }
    }

    // The refresh container itself draws nothing
    internal sealed class RefreshContainerTemplate : ControlTemplate<RefreshContainer>
    {
        protected override void Render(
            RefreshContainer control,
            DrawingContext drawContext)
        {
            drawContext.Clear(Colors.Transparent);
        }
    }

    // The tree draws nothing: its items do
    internal sealed class TreeViewTemplate : ControlTemplate<TreeView>
    {
        protected override void Render(
            TreeView control,
            DrawingContext drawContext)
        {
            drawContext.Clear(Colors.Transparent);
        }
    }

    // The node's text, indented, after a chevron (pointing to the text, or
    // down while expanded); selected, on a highlight with an accent pill
    internal sealed class TreeViewItemTemplate : ControlTemplate<TreeViewItem>
    {
        protected override void Render(
            TreeViewItem control,
            DrawingContext drawContext)
        {
            var colors = control.ActualTheme.Colors;
            var bounds = new Rect(0, 0, control.Bounds.Width, control.Bounds.Height);
            bool isRightToLeft = control.FlowDirection == FlowDirection.RightToLeft;

            drawContext.Clear(Colors.Transparent);
            var highlight = bounds.Deflate(2, 2);
            if (control.IsPressed)
            {
                drawContext.FillRoundedRectangle(highlight, 4, colors.Foreground.MultiplyAlpha(0.12f));
            }
            else if (control.IsSelected)
            {
                drawContext.FillRoundedRectangle(highlight, 4, colors.Foreground.MultiplyAlpha(0.07f));
                float x = isRightToLeft ? highlight.Right - 3 : highlight.X;
                drawContext.FillRoundedRectangle(new Rect(x, (bounds.Height - 16) / 2, 3, 16), 1.5f, colors.Accent);
            }

            if (control.Node.HasChildren)
            {
                var c = control.ChevronBounds.Center;
                if (control.Node.IsExpanded)
                {
                    drawContext.DrawLine(new Point(c.X - 4.5f, c.Y - 2), new Point(c.X, c.Y + 2.5f), colors.Foreground, 1.25f);
                    drawContext.DrawLine(new Point(c.X, c.Y + 2.5f), new Point(c.X + 4.5f, c.Y - 2), colors.Foreground, 1.25f);
                }
                else
                {
                    float d = isRightToLeft ? -1 : 1;
                    drawContext.DrawLine(new Point(c.X - (d * 2), c.Y - 4.5f), new Point(c.X + (d * 2.5f), c.Y), colors.Foreground, 1.25f);
                    drawContext.DrawLine(new Point(c.X + (d * 2.5f), c.Y), new Point(c.X - (d * 2), c.Y + 4.5f), colors.Foreground, 1.25f);
                }
            }

            drawContext.DrawText(control.Node.Content, control.TextBounds, colors.Foreground, control.CreateTextFormat());
        }
    }
}
