using System;

namespace XForms.Layouts
{
    /// <summary>
    /// Stacks children horizontally and distributes the spare width evenly
    /// between them.
    /// </summary>
    public class UniformGrid : Panel
    {
        public class UniformGridViewCollection : LayoutViewCollection
        {
            public UniformGridViewCollection(
                UniformGrid layout)
                : base(layout)
            {
            }
        }

        private UniformGridViewCollection _children;

        public UniformGrid()
        {
            this.HorizontalAlignment = HorizontalAlignment.Left;
            this.VerticalAlignment = VerticalAlignment.Top;

            this._children = new UniformGridViewCollection(this);
        }

        public UniformGridViewCollection Children
        {
            get
            {
                return this._children;
            }
        }

        protected override Size MeasureChildren(
            Size availableSize)
        {
            var children = this.LayoutChildren;

            float usedWidth = 0;
            float usedHeight = 0;

            for (int i = 0; i < children.Count; i += 1)
            {
                var child = children[i];

                child.Measure(new Size(Math.Max(0, availableSize.Width - usedWidth), availableSize.Height));

                Size childSize = child.DesiredSize;
                usedWidth += childSize.Width;
                usedHeight = Math.Max(usedHeight, childSize.Height);
            }

            return new Size(usedWidth, usedHeight);
        }

        protected override void ArrangeChildren(
            Size finalSize)
        {
            var children = this.LayoutChildren;
            int childCount = children.Count;
            if (childCount == 0)
            {
                return;
            }

            float childrenTotalWidth = 0;
            for (int i = 0; i < childCount; i += 1)
            {
                childrenTotalWidth += children[i].DesiredSize.Width;
            }

            // Give each child an even share of the spare width
            float childSpacing = Math.Max(0, (finalSize.Width - childrenTotalWidth) / childCount);

            float x = 0;
            for (int i = 0; i < childCount; i += 1)
            {
                var child = children[i];

                float width = child.DesiredSize.Width + childSpacing;
                child.Arrange(new Rect(x, 0, width, finalSize.Height));

                x += width;
            }
        }
    }
}
