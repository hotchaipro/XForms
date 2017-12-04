using System;
using Android.Views;
using AndroidView = Android.Views.View;

namespace XForms.Android
{
    /// <summary>
    /// A layout that stacks items linearly and distributes the space between the 
    /// items evently.
    /// </summary>
    public sealed class NativeDistributedStackLayout : global::Android.Views.ViewGroup
    {
        private enum LayoutAxis
        {
            None = 0,
            Horizontal = 1,
            Vertical = 2,
        }

        internal NativeDistributedStackLayout(
            global::Android.Content.Context context)
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
            double usedWidth = 0;
            double usedHeight = 0;

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

                var layoutParams = (BaseLayoutParams)child.LayoutParameters;

                this.MeasureChildWithMarginsOverride(
                    child,
                    widthMeasureSpec,
                    (int)usedWidth,
                    (layoutParams.Width == LayoutParams.MatchParent) ? LayoutParams.WrapContent : layoutParams.Width,
                    heightMeasureSpec,
                    (int)usedHeight,
                    layoutParams.Height);

                Size childSize = new Size(
                    child.MeasuredWidth + layoutParams.LeftMargin + layoutParams.RightMargin,
                    child.MeasuredHeight + layoutParams.TopMargin + layoutParams.BottomMargin);

                usedWidth += childSize.Width;
                usedHeight = Math.Max(usedHeight, childSize.Height);

                // Update the remaining size
                remainingSize = new Size(
                    (float)Math.Max(0, availableSize.Width - usedWidth),
                    (float)Math.Max(0, availableSize.Height));

                // Update the child state
                childState = CombineMeasuredStates(childState, child.MeasuredState);
            }

            // Calculate the total size used by children
            Size usedSize = new Size(
                (float)Math.Max(0, usedWidth),
                (float)Math.Max(0, usedHeight));

            // Default the final size to the size used by children
            var finalSize = usedSize;

            this.SetMeasuredDimension(
                ResolveSizeAndState((int)finalSize.Width, widthMeasureSpec, childState),
                ResolveSizeAndState((int)finalSize.Height, heightMeasureSpec, childState));
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
            var layoutParams = (MarginLayoutParams)child.LayoutParameters;

            int childWidthMeasureSpec = GetChildMeasureSpec(
                parentWidthMeasureSpec,
                this.PaddingLeft + this.PaddingRight + layoutParams.LeftMargin + layoutParams.RightMargin + widthUsed,
                childWidth);

            int childHeightMeasureSpec = GetChildMeasureSpec(
                parentHeightMeasureSpec,
                this.PaddingTop + this.PaddingBottom + layoutParams.TopMargin + layoutParams.BottomMargin + heightUsed,
                childHeight);

            child.Measure(childWidthMeasureSpec, childHeightMeasureSpec);
        }

        protected override void OnLayout(
            bool changed,
            int left,
            int top,
            int right,
            int bottom)
        {
            Rectangle layoutRect = new Rectangle(
                this.PaddingLeft,
                this.PaddingTop,
                right - left - this.PaddingLeft - this.PaddingRight,
                bottom - top - this.PaddingTop - this.PaddingBottom);

            double childrenTotalWidth = 0;

            int childCount = this.ChildCount;

            for (int i = 0; i < childCount; i += 1)
            {
                AndroidView child = this.GetChildAt(i);

                var layoutParams = (BaseLayoutParams)child.LayoutParameters;

                Size childSize = new Size(
                    child.MeasuredWidth + layoutParams.LeftMargin + layoutParams.RightMargin,
                    child.MeasuredHeight + layoutParams.TopMargin + layoutParams.BottomMargin);

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
                AndroidView child = this.GetChildAt(i);

                var layoutParams = (BaseLayoutParams)child.LayoutParameters;

                float arrangeWidth = layoutParams.LeftMargin + child.MeasuredWidth + layoutParams.RightMargin + childSpacing;

                // Handle gravity flags

                LayoutAxis axis = LayoutAxis.Horizontal;

                float childLeft = remainingRect.Left + layoutParams.LeftMargin;
                float childLayoutWidth = child.MeasuredWidth;
                var horizontalGravityFlags = layoutParams.Gravity & GravityFlags.HorizontalGravityMask;
                if (horizontalGravityFlags == GravityFlags.CenterHorizontal)
                {
                    childLeft = remainingRect.Left + (childSpacing / 2) + layoutParams.LeftMargin;
                }
                else if (horizontalGravityFlags == GravityFlags.Right)
                {
                    childLeft = remainingRect.Left + childSpacing + layoutParams.LeftMargin;
                }
                else if ((horizontalGravityFlags == GravityFlags.FillHorizontal) && (axis != LayoutAxis.Horizontal))
                {
                    // Use the entire available width (minus the margins) to layout the child
                    childLayoutWidth = remainingRect.Width - layoutParams.LeftMargin - layoutParams.RightMargin;
                    arrangeWidth = remainingRect.Width;
                }

                float childTop = remainingRect.Top + layoutParams.TopMargin;
                float childLayoutHeight = child.MeasuredHeight;
                var verticalGravityFlags = layoutParams.Gravity & GravityFlags.VerticalGravityMask;
                if (verticalGravityFlags == GravityFlags.CenterVertical)
                {
                    childTop = remainingRect.Top + layoutParams.TopMargin + ((remainingRect.Height - (layoutParams.TopMargin + child.MeasuredHeight + layoutParams.BottomMargin)) / 2);
                }
                else if (verticalGravityFlags == GravityFlags.Bottom)
                {
                    childTop = remainingRect.Bottom - (child.MeasuredHeight + layoutParams.BottomMargin);
                }
                else if ((verticalGravityFlags == GravityFlags.FillVertical) && (axis != LayoutAxis.Vertical))
                {
                    // Use the entire available height (minus the margins) to layout the child
                    childLayoutHeight = remainingRect.Height - layoutParams.TopMargin - layoutParams.BottomMargin;
                }

                // Layout the child with gravity taken into account

                child.Layout(
                    (int)childLeft,
                    (int)childTop,
                    Math.Max(0, (int)(childLeft + childLayoutWidth)),
                    Math.Max(0, (int)(childTop + childLayoutHeight)));

                remainingRect.X += arrangeWidth;
                remainingRect.Width = Math.Max(0, remainingRect.Width - arrangeWidth);
            }
        }

        protected override LayoutParams GenerateDefaultLayoutParams()
        {
            return new BaseLayoutParams();
        }

        protected override LayoutParams GenerateLayoutParams(
            LayoutParams p)
        {
            var baseLayoutParams = p as BaseLayoutParams;
            if (null != baseLayoutParams)
            {
                return new BaseLayoutParams(baseLayoutParams);
            }

            return new BaseLayoutParams(p);
        }

        protected override bool CheckLayoutParams(
            LayoutParams p)
        {
            return p is BaseLayoutParams;
        }
    }
}
