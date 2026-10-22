using System;
using XForms.Controls;

namespace XForms.Themes.Simple
{
    // The Simple look of list rows: the page's background, tinted with the
    // accent while selected
    internal sealed class ListViewItemTemplate : ControlTemplate<ListViewItem>
    {
        protected override void Render(
            ListViewItem control,
            DrawingContext drawingContext)
        {
            var colors = control.ActualTheme.Colors;
            var bounds = new Rect(0, 0, control.Bounds.Width, control.Bounds.Height);
            drawingContext.FillRectangle(bounds, colors.Background);
            if (control.IsSelected)
            {
                drawingContext.FillRectangle(bounds, colors.Accent.MultiplyAlpha(0.18f));
            }
        }
    }

    // What shows behind a swiped row: the command's icon and text, in white,
    // on its color (the control's background)
    internal sealed class ListViewSwipeItemTemplate : ControlTemplate<ListViewSwipeItem>
    {
        private const float Padding = 10.0f;

        private static readonly TextFormat PrimaryFormat = new TextFormat()
        {
            FontSize = 12.0f,
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Center,
        };

        private static readonly TextFormat SecondaryFormat = new TextFormat()
        {
            FontSize = 12.0f,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
        };

        protected override void Render(
            ListViewSwipeItem control,
            DrawingContext drawContext)
        {
            var bounds = new Rect(0, 0, control.Bounds.Width, control.Bounds.Height).Deflate(Padding, 0);
            bool isPrimary = control.Direction == ListViewSwipeDirection.Primary;
            var iconSize = control.IconSize;

            float origin = isPrimary ? 1.0f : 0.0f;
            Rect imageBounds = new Rect(
                bounds.X + (origin * (bounds.Width - iconSize.Width)),
                bounds.Center.Y - (iconSize.Height / 2.0f),
                iconSize.Width,
                iconSize.Height);
            Rect textBounds = bounds.Deflate(imageBounds.Width + Padding, 0);

            if (null != control.Icon)
            {
                drawContext.DrawImage(control.Icon, imageBounds, Colors.White);
            }

            if (null != control.Text)
            {
                drawContext.DrawText(control.Text, textBounds, Colors.White, isPrimary ? PrimaryFormat : SecondaryFormat);
            }
        }
    }
}
