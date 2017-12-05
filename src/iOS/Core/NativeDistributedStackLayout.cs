using System;

namespace XForms.iOS
{
    /// <summary>
    /// A layout that stacks items linearly and distributes the space between the 
    /// items evently.
    /// </summary>
    internal sealed class NativeDistributedStackLayout : NativeLayout
    {
        private enum LayoutAxis
        {
            None = 0,
            Horizontal = 1,
            Vertical = 2,
        }

        internal NativeDistributedStackLayout()
        {
        }

        protected override void OnMeasure(
            MeasureSpec widthMeasureSpec,
            MeasureSpec heightMeasureSpec)
        {
            var layoutProperties = this.LayoutProperties();

            Size availableSize = new Size(
                widthMeasureSpec.Size - layoutProperties.Padding.Left - layoutProperties.Padding.Right,
                heightMeasureSpec.Size - layoutProperties.Padding.Top - layoutProperties.Padding.Bottom);

            // Keep track of the space used by children
            float usedWidth = 0;
            float usedHeight = 0;

            var remainingSize = availableSize;

            MeasuredStateFlags childWidthState = MeasuredStateFlags.None;
            MeasuredStateFlags childHeightState = MeasuredStateFlags.None;
            int childCount = this.ChildCount;

            for (int i = 0; i < childCount; i += 1)
            {
                var child = this.GetChildAt(i);

                if (child.Hidden)
                {
                    continue;
                }

                var layoutParams = (LayoutParams)child.LayoutParameters();

                this.MeasureChildWithMarginsOverride(
                    child,
                    widthMeasureSpec,
                    usedWidth,
                    (layoutParams.Width == LayoutParams.MatchParent) ? LayoutParams.WrapContent : layoutParams.Width,
                    heightMeasureSpec,
                    usedHeight,
                    layoutParams.Height);

                var childSize = GetChildUsedSizeWithMargins(child, layoutParams);

                usedWidth += childSize.Width;
                usedHeight = Math.Max(usedHeight, childSize.Height);

                // Update the remaining size
                remainingSize = new Size(
                    (float)Math.Max(0, availableSize.Width - usedWidth),
                    (float)Math.Max(0, availableSize.Height));

                // Update the child state
                childWidthState = CombineMeasuredStates(childWidthState, child.LayoutProperties().MeasuredSize.WidthState);
                childHeightState = CombineMeasuredStates(childHeightState, child.LayoutProperties().MeasuredSize.HeightState);
            }

            // Calculate the total size used by children
            Size usedSize = new Size(
                (float)Math.Max(0, usedWidth),
                (float)Math.Max(0, usedHeight));

            // Default the final size to the size used by children
            var finalSize = usedSize;

            var dimensionX = ResolveSizeAndState(finalSize.Width, widthMeasureSpec, childWidthState);
            var dimensionY = ResolveSizeAndState(finalSize.Height, heightMeasureSpec, childHeightState);

            this.SetMeasuredSize(dimensionX, dimensionY);
        }

        protected override void OnLayout(
            bool changed,
            float left,
            float top,
            float right,
            float bottom)
        {
            var layoutProperties = this.LayoutProperties();

            Rectangle layoutRect = new Rectangle(
                layoutProperties.Padding.Left,
                layoutProperties.Padding.Top,
                right - left - layoutProperties.Padding.Left - layoutProperties.Padding.Right,
                bottom - top - layoutProperties.Padding.Top - layoutProperties.Padding.Bottom);

            float childrenTotalWidth = 0;

            int childCount = this.ChildCount;

            for (int i = 0; i < childCount; i += 1)
            {
                var child = this.GetChildAt(i);

                var layoutParams = (LayoutParams)child.LayoutParameters();

                var childSize = GetChildUsedSizeWithMargins(child, layoutParams);

                childrenTotalWidth += childSize.Width;
            }

            // Calculate the spacing between children
            float childSpacing = 0;
            int spaceCount = childCount;
            if (spaceCount > 0)
            {
                childSpacing = (float)Math.Max(0, (layoutRect.Width - childrenTotalWidth) / spaceCount);
            }

            Rectangle remainingRect = layoutRect;

            for (int i = 0; i < childCount; i += 1)
            {
                var child = this.GetChildAt(i);

                var layoutParams = (LayoutParams)child.LayoutParameters();
                
                var childLayoutProperties = child.LayoutProperties();
                
                float arrangeWidth = layoutParams.Margin.Left + childLayoutProperties.MeasuredSize.Width + layoutParams.Margin.Right + childSpacing;

                // Handle gravity flags

                LayoutAxis axis = LayoutAxis.Horizontal;
                
                float childLeft = remainingRect.Left + layoutParams.Margin.Left;
                float childLayoutWidth = childLayoutProperties.MeasuredSize.Width;
                var horizontalAlignment = layoutParams.HorizontalAlignment;
                if (horizontalAlignment == LayoutAlignment.Center)
                {
                    childLeft = remainingRect.Left + (childSpacing / 2) + layoutParams.Margin.Left;
                }
                else if (horizontalAlignment == LayoutAlignment.End)
                {
                    childLeft = remainingRect.Left + childSpacing + layoutParams.Margin.Left;
                }
                else if ((horizontalAlignment == LayoutAlignment.Fill) && (axis != LayoutAxis.Horizontal))
                {
                    // Use the entire available width (minus the margins) to layout the child
                    childLayoutWidth = remainingRect.Width - layoutParams.Margin.Left - layoutParams.Margin.Right;
                    arrangeWidth = remainingRect.Width;
                }

                float childTop = remainingRect.Top + layoutParams.Margin.Top;
                float childLayoutHeight = childLayoutProperties.MeasuredSize.Height;
                var verticalAlignment = layoutParams.VerticalAlignment;
                if (verticalAlignment == LayoutAlignment.Center)
                {
                    childTop = remainingRect.Top + layoutParams.Margin.Top + ((remainingRect.Height - (layoutParams.Margin.Top + childLayoutProperties.MeasuredSize.Height + layoutParams.Margin.Bottom)) / 2);
                }
                else if (verticalAlignment == LayoutAlignment.End)
                {
                    childTop = remainingRect.Bottom - (childLayoutProperties.MeasuredSize.Height + layoutParams.Margin.Bottom);
                }
                else if ((verticalAlignment == LayoutAlignment.Fill) && (axis != LayoutAxis.Vertical))
                {
                    // Use the entire available height (minus the margins) to layout the child
                    childLayoutHeight = remainingRect.Height - layoutParams.Margin.Top - layoutParams.Margin.Bottom;
                }

                // Layout the child with gravity taken into account

                child.Layout(
                    childLeft,
                    childTop,
                    Math.Max(0, childLeft + childLayoutWidth),
                    Math.Max(0, childTop + childLayoutHeight));

                remainingRect.X += arrangeWidth;
                remainingRect.Width = Math.Max(0, remainingRect.Width - arrangeWidth);
            }
        }
    }
}
