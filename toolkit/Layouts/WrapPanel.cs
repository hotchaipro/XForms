using System;
using System.Collections.Generic;

namespace XForms.Layouts
{
    /// <summary>
    /// Places its children in a row (or column) and starts a new line when the
    /// next child doesn't fit.
    /// </summary>
    public class WrapPanel : Panel
    {
        private readonly ViewCollection _children;
        private Orientation _orientation = Orientation.Horizontal;
        private float _itemSpacing;
        private float _lineSpacing;

        public WrapPanel()
        {
            this._children = new ViewCollection(this);
        }

        public ViewCollection Children
        {
            get
            {
                return this._children;
            }
        }

        /// <summary>
        /// The direction of a line: horizontal (the default) fills rows.
        /// </summary>
        public Orientation Orientation
        {
            get
            {
                return this._orientation;
            }

            set
            {
                this._orientation = value;
                this.InvalidateMeasure();
            }
        }

        /// <summary>
        /// The space between neighboring children in a line.
        /// </summary>
        public float ItemSpacing
        {
            get
            {
                return this._itemSpacing;
            }

            set
            {
                this._itemSpacing = value;
                this.InvalidateMeasure();
            }
        }

        /// <summary>
        /// The space between lines.
        /// </summary>
        public float LineSpacing
        {
            get
            {
                return this._lineSpacing;
            }

            set
            {
                this._lineSpacing = value;
                this.InvalidateMeasure();
            }
        }

        private float Main(Size size) => (this._orientation == Orientation.Horizontal) ? size.Width : size.Height;

        private float Cross(Size size) => (this._orientation == Orientation.Horizontal) ? size.Height : size.Width;

        // Calls back with each line: the index of its first child, its child
        // count and its cross size
        private void ForEachLine(
            float mainLimit,
            Action<int, int, float> line)
        {
            var children = this.LayoutChildren;
            int start = 0, count = 0;
            float main = 0, cross = 0;

            for (int i = 0; i < children.Count; i += 1)
            {
                var child = children[i];
                if (!child.IsVisible)
                {
                    count += 1;
                    continue;
                }

                float childMain = this.Main(child.DesiredSize);
                float next = (main > 0) ? main + this._itemSpacing + childMain : childMain;
                if ((main > 0) && (next > mainLimit))
                {
                    line(start, count, cross);
                    start = i;
                    count = 0;
                    main = 0;
                    cross = 0;
                    next = childMain;
                }

                main = next;
                cross = Math.Max(cross, this.Cross(child.DesiredSize));
                count += 1;
            }

            if (count > 0)
            {
                line(start, count, cross);
            }
        }

        protected override Size MeasureChildren(
            Size availableSize)
        {
            bool isHorizontal = (this._orientation == Orientation.Horizontal);
            var childAvailable = isHorizontal
                ? new Size(availableSize.Width, float.PositiveInfinity)
                : new Size(float.PositiveInfinity, availableSize.Height);

            foreach (var child in this.LayoutChildren)
            {
                child.Measure(childAvailable);
            }

            float mainLimit = this.Main(availableSize);
            float widestLine = 0, totalCross = 0;
            int lineCount = 0;
            var children = this.LayoutChildren;

            this.ForEachLine(mainLimit, (start, count, cross) =>
            {
                float main = 0;
                int visible = 0;
                for (int i = start; i < start + count; i += 1)
                {
                    if (children[i].IsVisible)
                    {
                        main += this.Main(children[i].DesiredSize);
                        visible += 1;
                    }
                }

                main += Math.Max(0, visible - 1) * this._itemSpacing;
                widestLine = Math.Max(widestLine, main);
                totalCross += cross;
                lineCount += 1;
            });

            totalCross += Math.Max(0, lineCount - 1) * this._lineSpacing;
            return isHorizontal ? new Size(widestLine, totalCross) : new Size(totalCross, widestLine);
        }

        protected override void ArrangeChildren(
            Size finalSize)
        {
            bool isHorizontal = (this._orientation == Orientation.Horizontal);
            var children = this.LayoutChildren;
            float lineOffset = 0;

            this.ForEachLine(this.Main(finalSize), (start, count, cross) =>
            {
                float offset = 0;
                for (int i = start; i < start + count; i += 1)
                {
                    var child = children[i];
                    if (!child.IsVisible)
                    {
                        continue;
                    }

                    var size = child.DesiredSize;
                    if (isHorizontal)
                    {
                        child.Arrange(new Rect(offset, lineOffset, size.Width, cross));
                    }
                    else
                    {
                        child.Arrange(new Rect(lineOffset, offset, cross, size.Height));
                    }

                    offset += this.Main(size) + this._itemSpacing;
                }

                lineOffset += cross + this._lineSpacing;
            });
        }
    }
}
