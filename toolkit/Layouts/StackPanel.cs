using System;

namespace XForms.Layouts
{


    /// <summary>
    /// Stacks its children vertically (the default) or horizontally, with
    /// optional spacing between them. Children stretch across the panel unless
    /// their alignment says otherwise.
    /// </summary>
    public class StackPanel : Panel
    {
        private readonly ViewCollection _children;
        private Orientation _orientation = Orientation.Vertical;
        private float _spacing;

        public StackPanel()
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
        /// The space between neighboring children.
        /// </summary>
        public float Spacing
        {
            get
            {
                return this._spacing;
            }

            set
            {
                this._spacing = value;
                this.InvalidateMeasure();
            }
        }

        protected override Size MeasureChildren(
            Size availableSize)
        {
            bool isHorizontal = (this._orientation == Orientation.Horizontal);
            var childAvailable = isHorizontal
                ? new Size(float.PositiveInfinity, availableSize.Height)
                : new Size(availableSize.Width, float.PositiveInfinity);

            float main = 0, cross = 0;
            int visibleCount = 0;

            foreach (var child in this.LayoutChildren)
            {
                child.Measure(childAvailable);
                if (!child.IsVisible)
                {
                    continue;
                }

                var size = child.DesiredSize;
                main += isHorizontal ? size.Width : size.Height;
                cross = Math.Max(cross, isHorizontal ? size.Height : size.Width);
                visibleCount += 1;
            }

            main += Math.Max(0, visibleCount - 1) * this._spacing;
            return isHorizontal ? new Size(main, cross) : new Size(cross, main);
        }

        protected override void ArrangeChildren(
            Size finalSize)
        {
            bool isHorizontal = (this._orientation == Orientation.Horizontal);
            float offset = 0;

            foreach (var child in this.LayoutChildren)
            {
                if (!child.IsVisible)
                {
                    continue;
                }

                var size = child.DesiredSize;
                if (isHorizontal)
                {
                    child.Arrange(new Rect(offset, 0, size.Width, finalSize.Height));
                    offset += size.Width + this._spacing;
                }
                else
                {
                    child.Arrange(new Rect(0, offset, finalSize.Width, size.Height));
                    offset += size.Height + this._spacing;
                }
            }
        }
    }
}
