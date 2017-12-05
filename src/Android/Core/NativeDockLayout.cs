using System;
using System.Collections.Generic;
using Android.Content;
using Android.Views;
using AndroidView = Android.Views.View;
using XForms.Layouts;

namespace XForms.Android
{
    public sealed class NativeDockLayout : global::Android.Views.ViewGroup
    {
        private enum LayoutAxis
        {
            None = 0,
            Horizontal = 1,
            Vertical = 2,
        }

        internal NativeDockLayout(
            Context context)
            : base(context)
        {
        }

        protected override void OnMeasure(
            int widthMeasureSpec,
            int heightMeasureSpec)
        {
            Size availableSize = new Size(
                MeasureSpec.GetSize(widthMeasureSpec) - this.PaddingLeft - this.PaddingRight,
                MeasureSpec.GetSize(heightMeasureSpec) - this.PaddingTop - this.PaddingBottom);

#if DEBUG
            var widthMode = MeasureSpec.GetMode(widthMeasureSpec);
            var heightMode = MeasureSpec.GetMode(heightMeasureSpec);
#endif

            // Keep track of the space used by children
            double usedLeft = 0;
            double usedTop = 0;
            double usedRight = 0;
            double usedBottom = 0;

            // Keep track of the starting point for measuring each dimension
            // in support of overlay regions
            double startLeft = 0;
            double startTop = 0;
            double startRight = 0;
            double startBottom = 0;

            double minWidth = 0;
            double minHeight = 0;

            var remainingSize = availableSize;

            int childState = 0;
            int childCount = this.ChildCount;

            for (int i = 0; i < childCount; i += 1)
            {
                AndroidView child = this.GetChildAt(i);

                if (child.Visibility == ViewStates.Gone)
                {
                    continue;
                }

                var layoutParams = (LayoutParams)child.LayoutParameters;

                var dockRegion = layoutParams.DockRegion;

                // Arrange all but the center elements
                if (dockRegion != DockRegion.CenterOverlay)
                {
                    LayoutAxis axis = GetLayoutAxis(dockRegion);

                    // Save the child width and height
                    int childWidth = layoutParams.Width;
                    int childHeight = layoutParams.Height;

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
                        (int)(usedLeft + usedRight),
                        childWidth,
                        heightMeasureSpec,
                        (int)(usedTop + usedBottom),
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
                    childState = CombineMeasuredStates(childState, child.MeasuredState);
                }
            }

            // Measure the center elements now that the center size has been completely calculated
            Size usedCenterSize = new Size(0, 0);
            for (int i = 0; i < childCount; i += 1)
            {
                AndroidView child = this.GetChildAt(i);

                if (child.Visibility == ViewStates.Gone)
                {
                    continue;
                }

                var layoutParams = (LayoutParams)child.LayoutParameters;

                var dockRegion = layoutParams.DockRegion;

                // Arrange only the center elements
                if (dockRegion != DockRegion.CenterOverlay)
                {
                    continue;
                }

                this.MeasureChildWithMarginsOverride(
                    child,
                    widthMeasureSpec,
                    (int)(usedLeft + usedRight),
                    layoutParams.Width,
                    heightMeasureSpec,
                    (int)(usedTop + usedBottom),
                    layoutParams.Height);

                Size childUsedSize = GetChildUsedSizeWithMargins(child, layoutParams);

                usedCenterSize = new Size(
                    Math.Max(usedCenterSize.Width, childUsedSize.Width),
                    Math.Max(usedCenterSize.Height, childUsedSize.Height));

                childState = CombineMeasuredStates(childState, child.MeasuredState);
            }

            // Calculate the total size used by children
            Size usedSize = new Size(
                (float)Math.Max(minWidth, usedLeft + usedRight + usedCenterSize.Width),
                (float)Math.Max(minHeight, usedTop + usedBottom + usedCenterSize.Height));

            // Default the final size to the size used by children
            var finalSize = new Size(
                usedSize.Width + this.PaddingLeft + this.PaddingRight,
                usedSize.Height + this.PaddingTop + this.PaddingBottom);

            int dimensionX = ResolveSizeAndState((int)finalSize.Width, widthMeasureSpec, childState);
            int dimensionY = ResolveSizeAndState((int)finalSize.Height, heightMeasureSpec, childState);

            this.SetMeasuredDimension(dimensionX, dimensionY);
        }

        private static Size GetChildUsedSizeWithMargins(
            AndroidView child,
            LayoutParams layoutParams)
        {
            // NOTE: Child MeasuredWidth/Height includes padding

            float usedWidth = child.MeasuredWidth + layoutParams.LeftMargin + layoutParams.RightMargin;
            float usedHeight = child.MeasuredHeight + layoutParams.TopMargin + layoutParams.BottomMargin;

            Size usedSize = new Size(usedWidth, usedHeight);

            return usedSize;
        }

        private void MeasureChildWithMarginsOverride(
            AndroidView child,
            int parentWidthMeasureSpec,
            int widthUsed,
            int childWidth,
            int parentHeightMeasureSpec,
            int heightUsed,
            int childHeight)
        {
            var layoutParams = (LayoutParams)child.LayoutParameters;

            int childWidthMeasureSpec = GetChildMeasureSpec(
                parentWidthMeasureSpec,
                this.PaddingLeft + this.PaddingRight + layoutParams.LeftMargin + layoutParams.RightMargin + widthUsed,
                childWidth);

            int childHeightMeasureSpec = GetChildMeasureSpec(
                parentHeightMeasureSpec,
                this.PaddingTop + this.PaddingBottom + layoutParams.TopMargin + layoutParams.BottomMargin + heightUsed,
                childHeight);

#if DEBUG
            var widthMode = MeasureSpec.GetMode(childWidthMeasureSpec);
            var heightMode = MeasureSpec.GetMode(childHeightMeasureSpec);
#endif

            child.Measure(childWidthMeasureSpec, childHeightMeasureSpec);
        }

        protected override void OnLayout(
            bool changed,
            int left,
            int top,
            int right,
            int bottom)
        {
            Rectangle remainingRect = new Rectangle(
                this.PaddingLeft,
                this.PaddingTop,
                right - left - this.PaddingLeft - this.PaddingRight,
                bottom - top - this.PaddingTop - this.PaddingBottom);

            int childCount = this.ChildCount;

            for (int i = 0; i < childCount; i += 1)
            {
                AndroidView child = this.GetChildAt(i);

                if (child.Visibility == ViewStates.Gone)
                {
                    continue;
                }

                var layoutParams = (LayoutParams)child.LayoutParameters;

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
                AndroidView child = this.GetChildAt(i);

                if (child.Visibility == ViewStates.Gone)
                {
                    continue;
                }

                var layoutParams = (LayoutParams)child.LayoutParameters;

                // Arrange only the center elements in the second pass
                if (layoutParams.DockRegion != DockRegion.CenterOverlay)
                {
                    continue;
                }

                this.LayoutChild(child, layoutParams, remainingRect);
            }
        }

        private void LayoutChild(
            AndroidView child,
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

            // Handle gravity flags

            LayoutAxis axis = GetLayoutAxis(layoutParams.DockRegion);

            float childLeft = arrangeRect.Left + layoutParams.LeftMargin;
            float childLayoutWidth = child.MeasuredWidth;
            var horizontalGravityFlags = layoutParams.Gravity & GravityFlags.HorizontalGravityMask;
            if (horizontalGravityFlags == GravityFlags.CenterHorizontal)
            {
                childLeft = arrangeRect.Left + layoutParams.LeftMargin + ((arrangeRect.Width - (layoutParams.LeftMargin + child.MeasuredWidth + layoutParams.RightMargin)) / 2);
            }
            else if (horizontalGravityFlags == GravityFlags.Right)
            {
                childLeft = arrangeRect.Right - (child.MeasuredWidth + layoutParams.RightMargin);
            }
            else if ((horizontalGravityFlags == GravityFlags.FillHorizontal) && (axis != LayoutAxis.Horizontal))
            {
                // Check for auto-dimension
                if ((child.MeasuredWidth == global::Android.Views.ViewGroup.LayoutParams.MatchParent)
                    || (child.MeasuredWidth == global::Android.Views.ViewGroup.LayoutParams.WrapContent))
                {
                    // Use the entire available width (minus the margins) to layout the child
                    childLayoutWidth = arrangeRect.Width - layoutParams.LeftMargin - layoutParams.RightMargin;
                }
                else
                {
                    // Center in the available width
                    childLeft = arrangeRect.Left + layoutParams.LeftMargin + ((arrangeRect.Width - (layoutParams.LeftMargin + child.MeasuredWidth + layoutParams.RightMargin)) / 2);
                }
            }

            float childTop = arrangeRect.Top + layoutParams.TopMargin;
            float childLayoutHeight = child.MeasuredHeight;
            var verticalGravityFlags = layoutParams.Gravity & GravityFlags.VerticalGravityMask;
            if (verticalGravityFlags == GravityFlags.CenterVertical)
            {
                childTop = arrangeRect.Top + layoutParams.TopMargin + ((arrangeRect.Height - (layoutParams.TopMargin + child.MeasuredHeight + layoutParams.BottomMargin)) / 2);
            }
            else if (verticalGravityFlags == GravityFlags.Bottom)
            {
                childTop = arrangeRect.Bottom - (child.MeasuredHeight + layoutParams.BottomMargin);
            }
            else if ((verticalGravityFlags == GravityFlags.FillVertical) && (axis != LayoutAxis.Vertical))
            {
                // Check for auto-dimension
                if ((child.MeasuredHeight == global::Android.Views.ViewGroup.LayoutParams.MatchParent)
                    || (child.MeasuredHeight == global::Android.Views.ViewGroup.LayoutParams.WrapContent))
                {
                    // Use the entire available height (minus the margins) to layout the child
                    childLayoutHeight = arrangeRect.Height - layoutParams.TopMargin - layoutParams.BottomMargin;
                }
                else
                {
                    // Center in the available height
                    childTop = arrangeRect.Top + layoutParams.TopMargin + ((arrangeRect.Height - (layoutParams.TopMargin + child.MeasuredHeight + layoutParams.BottomMargin)) / 2);
                }
            }

            // Layout the child with gravity taken into account

            child.Layout(
                (int)childLeft,
                (int)childTop,
                Math.Max(0, (int)(childLeft + childLayoutWidth)),
                Math.Max(0, (int)(childTop + childLayoutHeight)));
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

        protected override ViewGroup.LayoutParams GenerateDefaultLayoutParams()
        {
            return new LayoutParams(DockRegion.Top);
        }

        protected override ViewGroup.LayoutParams GenerateLayoutParams(
            ViewGroup.LayoutParams p)
        {
            var baseLayoutParams = p as BaseLayoutParams;
            if (null != baseLayoutParams)
            {
                return new LayoutParams(baseLayoutParams);
            }

            return new LayoutParams(p);
        }

        protected override bool CheckLayoutParams(
            ViewGroup.LayoutParams p)
        {
            return p is LayoutParams;
        }

        internal new class LayoutParams : BaseLayoutParams
        {
            public LayoutParams(
                DockRegion region)
            {
                this.DockRegion = region;
            }

            public LayoutParams(
                BaseLayoutParams source)
                : base(source)
            {
                if (this.Gravity != source.Gravity)
                {
                    throw new Exception();
                }
            }

            public LayoutParams(
                ViewGroup.LayoutParams source)
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
