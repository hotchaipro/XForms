using System;
using NativeView = UIKit.UIView;

namespace XForms.iOS
{
    internal class NativeDockLayout : NativeLayout
    {
        private enum LayoutAxis
        {
            None = 0,
            Horizontal = 1,
            Vertical = 2,
        }

        internal NativeDockLayout()
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
            float usedLeft = 0;
            float usedTop = 0;
            float usedRight = 0;
            float usedBottom = 0;

            // Keep track of the starting point for measuring each dimension
            // in support of overlay regions
            float startLeft = 0;
            float startTop = 0;
            float startRight = 0;
            float startBottom = 0;

            float minWidth = 0;
            float minHeight = 0;

            var remainingSize = availableSize;

            var childWidthState = MeasuredStateFlags.None;
            var childHeightState = MeasuredStateFlags.None;

            int childCount = this.ChildCount;

            for (int i = 0; i < childCount; i += 1)
            {
                NativeView child = this.GetChildAt(i);

                if (child.Hidden)
                {
                    continue;
                }

                var layoutParams = (LayoutParams)child.LayoutParameters();

                var dockRegion = layoutParams.DockRegion;

                // Arrange all but the center elements
                if (dockRegion != DockRegion.CenterOverlay)
                {
                    LayoutAxis axis = GetLayoutAxis(dockRegion);

                    // Save the child width and height
                    var childWidth = layoutParams.Width;
                    var childHeight = layoutParams.Height;

                    // Overwrite child MatchParent size on the stacking axis
                    if ((axis == LayoutAxis.Horizontal) && (childWidth == LayoutParams.MatchParent))
                    {
                        childWidth = LayoutParams.WrapContent;
                    }
                    else if ((axis == LayoutAxis.Vertical) && (childHeight == LayoutParams.MatchParent))
                    {
                        childHeight = LayoutParams.WrapContent;
                    }

                    // Measure the child
                    this.MeasureChildWithMarginsOverride(
                        child,
                        widthMeasureSpec,
                        usedLeft + usedRight,
                        childWidth,
                        heightMeasureSpec,
                        usedTop + usedBottom,
                        childHeight);

                    Size childUsedSize = GetChildUsedSizeWithMargins(child, layoutParams);

                    if (dockRegion == DockRegion.Left)
                    {
                        usedLeft = Math.Max(usedLeft, startLeft + childUsedSize.Width);
                        startLeft += childUsedSize.Width;
                        minHeight = Math.Max(minHeight, usedTop + usedBottom + childUsedSize.Height);
                    }
                    else if (dockRegion == DockRegion.LeftOverlay)
                    {
                        usedLeft = Math.Max(usedLeft, startLeft + childUsedSize.Width);
                        minHeight = Math.Max(minHeight, usedTop + usedBottom + childUsedSize.Height);
                    }
                    else if (dockRegion == DockRegion.Top)
                    {
                        usedTop = Math.Max(usedTop, startTop + childUsedSize.Height);
                        startTop += childUsedSize.Height;
                        minWidth = Math.Max(minWidth, usedLeft + usedRight + childUsedSize.Width);
                    }
                    else if (dockRegion == DockRegion.TopOverlay)
                    {
                        usedTop = Math.Max(usedTop, startTop + childUsedSize.Height);
                        minWidth = Math.Max(minWidth, usedLeft + usedRight + childUsedSize.Width);
                    }
                    else if (dockRegion == DockRegion.Right)
                    {
                        usedRight = Math.Max(usedRight, startRight + childUsedSize.Width);
                        startRight += childUsedSize.Width;
                        minHeight = Math.Max(minHeight, usedTop + usedBottom + childUsedSize.Height);
                    }
                    else if (dockRegion == DockRegion.RightOverlay)
                    {
                        usedRight = Math.Max(usedRight, startRight + childUsedSize.Width);
                        minHeight = Math.Max(minHeight, usedTop + usedBottom + childUsedSize.Height);
                    }
                    else if (dockRegion == DockRegion.Bottom)
                    {
                        usedBottom = Math.Max(usedBottom, startBottom + childUsedSize.Height);
                        startBottom += childUsedSize.Height;
                        minWidth = Math.Max(minWidth, usedLeft + usedRight + childUsedSize.Width);
                    }
                    else if (dockRegion == DockRegion.BottomOverlay)
                    {
                        usedBottom = Math.Max(usedBottom, startBottom + childUsedSize.Height);
                        minWidth = Math.Max(minWidth, usedLeft + usedRight + childUsedSize.Width);
                    }
                    else
                    {
                        throw new NotSupportedException("Unsupported dock region.");
                    }

                    // Update the remaining size
                    remainingSize = new Size(
                        (float)Math.Max(0, availableSize.Width - startLeft - startRight),
                        (float)Math.Max(0, availableSize.Height - startTop - startBottom));

                    // Update the child state
                    childWidthState |= child.LayoutProperties().MeasuredSize.WidthState;
                    childHeightState |= child.LayoutProperties().MeasuredSize.HeightState;
                }
            }

            // Measure the center elements now that the center size has been completely calculated
            Size usedCenterSize = new Size(0, 0);
            for (int i = 0; i < childCount; i += 1)
            {
                NativeView child = this.GetChildAt(i);

                if (child.Hidden)
                {
                    continue;
                }

                var layoutParams = (LayoutParams)child.LayoutParameters();

                var dockRegion = layoutParams.DockRegion;

                // Arrange only the center elements
                if (dockRegion != DockRegion.CenterOverlay)
                {
                    continue;
                }

                this.MeasureChildWithMarginsOverride(
                    child,
                    widthMeasureSpec,
                    usedLeft + usedRight,
                    layoutParams.Width,
                    heightMeasureSpec,
                    usedTop + usedBottom,
                    layoutParams.Height);

                Size childUsedSize = GetChildUsedSizeWithMargins(child, layoutParams);

                usedCenterSize = new Size(
                    Math.Max(usedCenterSize.Width, childUsedSize.Width),
                    Math.Max(usedCenterSize.Height, childUsedSize.Height));

                childWidthState |= child.LayoutProperties().MeasuredSize.WidthState;
                childHeightState |= child.LayoutProperties().MeasuredSize.HeightState;
            }

            // Calculate the total size used by children
            Size usedSize = new Size(
                (float)Math.Max(minWidth, usedLeft + usedRight + usedCenterSize.Width),
                (float)Math.Max(minHeight, usedTop + usedBottom + usedCenterSize.Height));

            // Default the final size to the size used by children
            var finalSize = new Size(
                usedSize.Width + layoutProperties.Padding.Left + layoutProperties.Padding.Right,
                usedSize.Height + layoutProperties.Padding.Top + layoutProperties.Padding.Bottom);

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

            Rectangle remainingRect = new Rectangle(
                layoutProperties.Padding.Left,
                layoutProperties.Padding.Top,
                right - left - layoutProperties.Padding.Left - layoutProperties.Padding.Right,
                bottom - top - layoutProperties.Padding.Top - layoutProperties.Padding.Bottom);

            int childCount = this.ChildCount;

            for (int i = 0; i < childCount; i += 1)
            {
                NativeView child = this.GetChildAt(i);

                if (child.Hidden)
                {
                    continue;
                }

                var layoutParams = (LayoutParams)child.LayoutParameters();

                var dockRegion = layoutParams.DockRegion;

                // Arrange all but the center elements in the first pass
                if (dockRegion != DockRegion.CenterOverlay)
                {
                    Rectangle arrangeRect = remainingRect;

                    Size childUsedSize = GetChildUsedSizeWithMargins(child, layoutParams);

                    if (dockRegion == DockRegion.Left)
                    {
                        arrangeRect.Width = childUsedSize.Width;
                        remainingRect.X += childUsedSize.Width;
                        remainingRect.Width = Math.Max(0, remainingRect.Width - childUsedSize.Width);
                    }
                    else if (dockRegion == DockRegion.LeftOverlay)
                    {
                        arrangeRect.Width = childUsedSize.Width;
                    }
                    else if (dockRegion == DockRegion.Top)
                    {
                        arrangeRect.Height = childUsedSize.Height;
                        remainingRect.Y += childUsedSize.Height;
                        remainingRect.Height = Math.Max(0, remainingRect.Height - childUsedSize.Height);
                    }
                    else if (dockRegion == DockRegion.TopOverlay)
                    {
                        arrangeRect.Height = childUsedSize.Height;
                    }
                    else if (dockRegion == DockRegion.Right)
                    {
                        arrangeRect.X = remainingRect.Right - childUsedSize.Width;
                        arrangeRect.Width = childUsedSize.Width;
                        remainingRect.Width = Math.Max(0, remainingRect.Width - childUsedSize.Width);
                    }
                    else if (dockRegion == DockRegion.RightOverlay)
                    {
                        arrangeRect.X = remainingRect.Right - childUsedSize.Width;
                        arrangeRect.Width = childUsedSize.Width;
                    }
                    else if (dockRegion == DockRegion.Bottom)
                    {
                        arrangeRect.Y = remainingRect.Bottom - childUsedSize.Height;
                        arrangeRect.Height = childUsedSize.Height;
                        remainingRect.Height = Math.Max(0, remainingRect.Height - childUsedSize.Height);
                    }
                    else if (dockRegion == DockRegion.BottomOverlay)
                    {
                        arrangeRect.Y = remainingRect.Bottom - childUsedSize.Height;
                        arrangeRect.Height = childUsedSize.Height;
                    }
                    else
                    {
                        throw new NotSupportedException("Unsupported dock region.");
                    }

                    this.LayoutChild(child, layoutParams, arrangeRect);
                }
            }

            // Arrange the center elements now that the center rectangle has been calculated
            for (int i = 0; i < childCount; i += 1)
            {
                NativeView child = this.GetChildAt(i);

                if (child.Hidden)
                {
                    continue;
                }

                var layoutParams = (LayoutParams)child.LayoutParameters();

                // Arrange only the center elements in the second pass
                if (layoutParams.DockRegion != DockRegion.CenterOverlay)
                {
                    continue;
                }

                this.LayoutChild(child, layoutParams, remainingRect);
            }
        }

        private void LayoutChild(
            NativeView child,
            LayoutParams layoutParams,
            Rectangle arrangeRect)
        {
            if (null == child)
            {
                throw new ArgumentNullException(nameof(child));
            }

            if (null == layoutParams)
            {
                throw new ArgumentNullException(nameof(layoutParams));
            }

            var childLayoutProperties = child.LayoutProperties();

            // Handle gravity flags

            LayoutAxis axis = GetLayoutAxis(layoutParams.DockRegion);

            float childLeft = arrangeRect.Left + layoutParams.Margin.Left;
            float childLayoutWidth = childLayoutProperties.MeasuredSize.Width;
            var horizontalAlignment = layoutParams.HorizontalAlignment;
            if (horizontalAlignment == LayoutAlignment.Center)
            {
                childLeft = arrangeRect.Left + layoutParams.Margin.Left + ((arrangeRect.Width - (layoutParams.Margin.Left + childLayoutProperties.MeasuredSize.Width + layoutParams.Margin.Right)) / 2);
            }
            else if (horizontalAlignment == LayoutAlignment.End)
            {
                childLeft = arrangeRect.Right - (childLayoutProperties.MeasuredSize.Width + layoutParams.Margin.Right);
            }
            else if ((horizontalAlignment == LayoutAlignment.Fill) && (axis != LayoutAxis.Horizontal))
            {
                // Use the entire available width (minus the margins) to layout the child
                childLayoutWidth = arrangeRect.Width - layoutParams.Margin.Left - layoutParams.Margin.Right;
            }

            float childTop = arrangeRect.Top + layoutParams.Margin.Top;
            float childLayoutHeight = childLayoutProperties.MeasuredSize.Height;
            var verticalAlignment = layoutParams.VerticalAlignment;
            if (verticalAlignment == LayoutAlignment.Center)
            {
                childTop = arrangeRect.Top + layoutParams.Margin.Top + ((arrangeRect.Height - (layoutParams.Margin.Top + childLayoutProperties.MeasuredSize.Height + layoutParams.Margin.Bottom)) / 2);
            }
            else if (verticalAlignment == LayoutAlignment.End)
            {
                childTop = arrangeRect.Bottom - (childLayoutProperties.MeasuredSize.Height + layoutParams.Margin.Bottom);
            }
            else if ((verticalAlignment == LayoutAlignment.Fill) && (axis != LayoutAxis.Vertical))
            {
                // Use the entire available height (minus the margins) to layout the child
                childLayoutHeight = arrangeRect.Height - layoutParams.Margin.Top - layoutParams.Margin.Bottom;
            }

            // Layout the child with gravity taken into account

            child.Layout(
                childLeft,
                childTop,
                Math.Max(0, childLeft + childLayoutWidth),
                Math.Max(0, childTop + childLayoutHeight));
        }

        private static LayoutAxis GetLayoutAxis(
            DockRegion dockRegion)
        {
            LayoutAxis axis;

            if (dockRegion == DockRegion.CenterOverlay)
            {
                axis = LayoutAxis.None;
            }
            else if ((dockRegion == DockRegion.Left) || (dockRegion == DockRegion.LeftOverlay)
                || (dockRegion == DockRegion.Right) || (dockRegion == DockRegion.RightOverlay))
            {
                axis = LayoutAxis.Horizontal;
            }
            else
            {
                axis = LayoutAxis.Vertical;
            }

            return axis;
        }

        protected override NativeLayout.LayoutParams GenerateDefaultLayoutParams()
        {
            return new LayoutParams(DockRegion.Top);
        }

        protected override NativeLayout.LayoutParams GenerateLayoutParams(
            NativeLayout.LayoutParams p)
        {
            return new LayoutParams(p);
        }

        protected override bool CheckLayoutParams(
            NativeLayout.LayoutParams p)
        {
            return p is LayoutParams;
        }

        internal new class LayoutParams : NativeLayout.LayoutParams
        {
            public LayoutParams(
                DockRegion region)
            {
                this.DockRegion = region;
            }

            public LayoutParams(
                NativeLayout.LayoutParams source)
                : base(source)
            {
            }

            public DockRegion DockRegion
            {
                get;
                set;
            }
        }
    }
}
