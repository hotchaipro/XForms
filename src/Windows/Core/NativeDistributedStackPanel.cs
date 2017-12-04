using System;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using XamlSize = global::Windows.Foundation.Size;

namespace XForms.Windows
{
    /// <summary>
    /// A layout panel that stacks items linearly and distributes the space between the 
    /// items evently.
    /// </summary>
    internal class NativeDistributedStackPanel : Panel
    {
        public NativeDistributedStackPanel()
        {
        }

        protected override XamlSize MeasureOverride(
            XamlSize availableSize)
        {
            // NOTE: Children must be measured in order to set the DesiredSize 
            // required for the ArrangeOverride implementation.

            // Keep track of the space used by children
            double usedWidth = 0;
            double usedHeight = 0;

            var remainingSize = availableSize;

            foreach (UIElement child in this.Children)
            {
                child.Measure(remainingSize);

                XamlSize childSize = child.DesiredSize;

                usedWidth += childSize.Width;
                usedHeight = Math.Max(usedHeight, childSize.Height);

                // Update the remaining size
                remainingSize = new XamlSize(
                    Math.Max(0, availableSize.Width - usedWidth),
                    Math.Max(0, availableSize.Height));
            }

            // Calculate the total size used by children
            XamlSize usedSize = new XamlSize(
                Math.Max(0, usedWidth),
                Math.Max(0, usedHeight));

            // Default the final size to the size used by children
            var finalSize = usedSize;

            // Adjust final size for horizontal stretch taking infinity into account
            if ((this.HorizontalAlignment == HorizontalAlignment.Stretch)
                && (!double.IsInfinity(availableSize.Width))
                && (!double.IsNaN(availableSize.Width)))
            {
                finalSize.Width = availableSize.Width;
            }

            // Adjust final size for vertical stretch taking infinity into account
            if ((this.VerticalAlignment == VerticalAlignment.Stretch)
                && (!double.IsInfinity(availableSize.Height))
                && (!double.IsNaN(availableSize.Height)))
            {
                finalSize.Height = availableSize.Height;
            }

            return finalSize;
        }

        protected override XamlSize ArrangeOverride(
            XamlSize arrangeSize)
        {
            double childrenTotalWidth = 0;

            foreach (UIElement child in this.Children)
            {
                XamlSize childSize = child.DesiredSize;

                childrenTotalWidth += childSize.Width;
            }

            // Calculate the spacing between children
            double childSpacing = 0;
            int spaceCount = this.Children.Count;
            if (spaceCount > 0)
            {
                childSpacing = Math.Max(0, (arrangeSize.Width - childrenTotalWidth) / spaceCount);
            }

            Rect remainingRect = new Rect(0, 0, arrangeSize.Width, arrangeSize.Height);

            foreach (UIElement child in this.Children)
            {
                XamlSize childSize = child.DesiredSize;

                double arrangeWidth = childSize.Width + childSpacing;

                Rect arrangeRect = remainingRect;
                arrangeRect.Width = arrangeWidth;

                child.Arrange(arrangeRect);

                remainingRect.X += arrangeWidth;
                remainingRect.Width = Math.Max(0, remainingRect.Width - arrangeWidth);
            }

            return arrangeSize;
        }
    }
}
