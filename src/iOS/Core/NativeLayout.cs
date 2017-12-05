using System;
using UIKit;

namespace XForms.iOS
{
    internal abstract class NativeLayout : UIView
    {
        public NativeLayout()
        {
        }

        public int ChildCount
        {
            get
            {
                return this.Subviews?.Length ?? 0;
            }
        }
        
        public UIView GetChildAt(
            int index)
        {
            return this.Subviews?[index];
        }
        
        public override void AddSubview(
            UIView view)
        {
            EnsureLayoutParams(view);
            base.AddSubview(view);
            this.RequestLayout();
        }

        public override void InsertSubview(
            UIView view,
            nint atIndex)
        {
            EnsureLayoutParams(view);
            base.InsertSubview(view, atIndex);
            this.RequestLayout();
        }

        private void EnsureLayoutParams(
            UIView view)
        {
            var layoutProperties = view.LayoutProperties();

            var layoutParams = layoutProperties.LayoutParameters;

            if (null == layoutParams)
            {
                layoutParams = this.GenerateDefaultLayoutParams();
            }
            else if (!CheckLayoutParams(layoutParams))
            {
                layoutParams = this.GenerateLayoutParams(layoutParams);
            }

            layoutProperties.LayoutParameters = layoutParams;
        }

        public override void LayoutSubviews()
        {
            var layoutProperties = this.LayoutProperties();
            if (layoutProperties.IsLayoutRequested)
            {
                var widthMeasureSpec = MeasureSpec.MakeMeasureSpec((float)this.Frame.Size.Width, MeasureMode.Exactly);
                var heightMeasureSpec = MeasureSpec.MakeMeasureSpec((float)this.Frame.Size.Height, MeasureMode.Exactly);

                this.OnMeasure(widthMeasureSpec, heightMeasureSpec);

                this.OnLayout(
                    true,
                    (float)this.Frame.Left,
                    (float)this.Frame.Top,
                    (float)this.Frame.Right,
                    (float)this.Frame.Bottom);
            }
        }

        public void Measure(
            MeasureSpec widthMeasureSpec,
            MeasureSpec heightMeasureSpec)
        {
            this.OnMeasure(widthMeasureSpec, heightMeasureSpec);
        }

        public void Layout(
            float left,
            float top,
            float right,
            float bottom)
        {
            this.OnLayout(false, left, top, right, bottom);
        }

        protected abstract void OnMeasure(
            MeasureSpec widthMeasureSpec,
            MeasureSpec heightMeasureSpec);

        protected abstract void OnLayout(
            bool changed,
            float left,
            float top,
            float right,
            float bottom);

        protected static MeasuredDimension ResolveSizeAndState(
            float size,
            MeasureSpec measureSpec,
            MeasuredStateFlags childMeasuredState)
        {
            MeasuredDimension result = new MeasuredDimension();

            MeasureMode specMode = measureSpec.Mode;
            float specSize = measureSpec.Size;

            switch (specMode)
            {
                case MeasureSpec.Unspecified:
                    result.Size = size;
                    break;

                case MeasureSpec.AtMost:
                    if (specSize < size)
                    {
                        result.Size = specSize;
                        result.State |= MeasuredStateFlags.TooSmall;
                    }
                    else
                    {
                        result.Size = size;
                    }
                    break;

                case MeasureSpec.Exactly:
                    result.Size = specSize;
                    break;
            }

            result.State |= childMeasuredState;

            return result;
        }

        protected void MeasureChildWithMarginsOverride(
            UIView child,
            MeasureSpec parentWidthMeasureSpec,
            float widthUsed,
            float childWidth,
            MeasureSpec parentHeightMeasureSpec,
            float heightUsed,
            float childHeight)
        {
            var layoutProperties = child.LayoutProperties();

            var layoutParams = (LayoutParams)child.LayoutParameters();

            MeasureSpec childWidthMeasureSpec = GetChildMeasureSpec(
                parentWidthMeasureSpec,
                layoutProperties.Padding.Left + layoutProperties.Padding.Right + layoutParams.Margin.Left + layoutParams.Margin.Right + widthUsed,
                childWidth);

            MeasureSpec childHeightMeasureSpec = GetChildMeasureSpec(
                parentHeightMeasureSpec,
                layoutProperties.Padding.Top + layoutProperties.Padding.Bottom + layoutParams.Margin.Top + layoutParams.Margin.Bottom + heightUsed,
                childHeight);

            child.Measure(childWidthMeasureSpec, childHeightMeasureSpec);
        }

        protected static Size GetChildUsedSizeWithMargins(
            UIView child,
            LayoutParams layoutParams)
        {
            // NOTE: Child MeasuredWidth/Height includes padding

            float usedWidth = child.LayoutProperties().MeasuredSize.Width + layoutParams.Margin.Left + layoutParams.Margin.Right;
            float usedHeight = child.LayoutProperties().MeasuredSize.Height + layoutParams.Margin.Top + layoutParams.Margin.Bottom;

            Size usedSize = new Size(usedWidth, usedHeight);

            return usedSize;
        }

        public static MeasureSpec GetChildMeasureSpec(
            MeasureSpec spec,
            float padding,
            float childDimension)
        {
            var specMode = spec.Mode;
            var specSize = spec.Size;
            float size = Math.Max(0, specSize - padding);
            float resultSize = 0;
            MeasureMode resultMode = 0;
            switch (specMode)
            {
                // Parent has imposed an exact size on us
                case MeasureSpec.Exactly:
                    if (childDimension >= 0)
                    {
                        resultSize = childDimension;
                        resultMode = MeasureSpec.Exactly;
                    }
                    else if (childDimension == LayoutParams.MatchParent)
                    {
                        // Child wants to be our size. So be it.
                        resultSize = size;
                        resultMode = MeasureSpec.Exactly;
                    }
                    else if (childDimension == LayoutParams.WrapContent)
                    {
                        // Child wants to determine its own size. It can't be
                        // bigger than us.
                        resultSize = size;
                        resultMode = MeasureSpec.AtMost;
                    }
                    break;
                // Parent has imposed a maximum size on us
                case MeasureSpec.AtMost:
                    if (childDimension >= 0)
                    {
                        // Child wants a specific size... so be it
                        resultSize = childDimension;
                        resultMode = MeasureSpec.Exactly;
                    }
                    else if (childDimension == LayoutParams.MatchParent)
                    {
                        // Child wants to be our size, but our size is not fixed.
                        // Constrain child to not be bigger than us.
                        resultSize = size;
                        resultMode = MeasureSpec.AtMost;
                    }
                    else if (childDimension == LayoutParams.WrapContent)
                    {
                        // Child wants to determine its own size. It can't be
                        // bigger than us.
                        resultSize = size;
                        resultMode = MeasureSpec.AtMost;
                    }
                    break;
                // Parent asked to see how big we want to be
                case MeasureSpec.Unspecified:
                    if (childDimension >= 0)
                    {
                        // Child wants a specific size... let him have it
                        resultSize = childDimension;
                        resultMode = MeasureSpec.Exactly;
                    }
                    else if (childDimension == LayoutParams.MatchParent)
                    {
                        // Child wants to be our size... find out how big it should
                        // be
                        resultSize = 0;
                        resultMode = MeasureSpec.Unspecified;
                    }
                    else if (childDimension == LayoutParams.WrapContent)
                    {
                        // Child wants to determine its own size.... find out how
                        // big it should be
                        resultSize = 0;
                        resultMode = MeasureSpec.Unspecified;
                    }
                    break;
            }

            return MeasureSpec.MakeMeasureSpec(resultSize, resultMode);
        }

        protected static MeasuredStateFlags CombineMeasuredStates(
            MeasuredStateFlags currentState,
            MeasuredStateFlags newState)
        {
            return currentState | newState;
        }

        protected virtual LayoutParams GenerateDefaultLayoutParams()
        {
            return new LayoutParams();
        }

        protected virtual LayoutParams GenerateLayoutParams(
            LayoutParams p)
        {
            return new LayoutParams(p);
        }

        protected virtual bool CheckLayoutParams(
            LayoutParams p)
        {
            return p is LayoutParams;
        }

        internal class LayoutParams
        {
            public static float MatchParent = -1;
            public static float WrapContent = -2;

            private static float DefaultSize = MatchParent;
            private static LayoutAlignment DefaultAlignment = LayoutAlignment.Fill;

            public LayoutParams()
                : this(DefaultSize, DefaultSize, DefaultAlignment, DefaultAlignment)
            {
            }

            private LayoutParams(
                float width,
                float height)
                : this(width, height, DefaultAlignment, DefaultAlignment)
            {
            }

            public LayoutParams(
                float width,
                float height,
                LayoutAlignment horizontalAlignment,
                LayoutAlignment verticalAlignment)
            {
                this.Width = width;
                this.Height = height;
                this.HorizontalAlignment = horizontalAlignment;
                this.VerticalAlignment = verticalAlignment;
            }

            public LayoutParams(
                LayoutParams source)
                : this(source.Width, source.Height, source.HorizontalAlignment, source.VerticalAlignment)
            {
            }

            public float Width
            {
                get;
                set;
            }

            public float Height
            {
                get;
                set;
            }

            public Size MaximumSize { get; set; }

            public LayoutAlignment HorizontalAlignment { get; set; }

            public LayoutAlignment VerticalAlignment { get; set; }

            public Thickness Margin { get; set; }
        }
    }
}
