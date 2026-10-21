using System;

namespace XForms.Layouts
{
    /// <summary>
    /// Places each child at a position, at its desired size. The canvas asks
    /// for no space of its own; size it explicitly or let it stretch.
    /// </summary>
    public class Canvas : Panel
    {
        /// <summary>
        /// A canvas's children, each at a position.
        /// </summary>
        public sealed class CanvasChildren : ViewCollection<Point>
        {
            internal CanvasChildren(
                Canvas owner)
                : base(owner, Point.Zero)
            {
            }

            /// <summary>
            /// Adds a child with its top left corner at the position.
            /// </summary>
            public void Add(
                View child,
                float left,
                float top)
            {
                this.Add(child, new Point(left, top));
            }
        }

        private readonly CanvasChildren _children;

        public Canvas()
        {
            this._children = new CanvasChildren(this);
        }

        public CanvasChildren Children
        {
            get
            {
                return this._children;
            }
        }

        protected override Size MeasureChildren(
            Size availableSize)
        {
            var unlimited = new Size(float.PositiveInfinity, float.PositiveInfinity);
            foreach (var child in this.LayoutChildren)
            {
                child.Measure(unlimited);
            }

            return Size.Empty;
        }

        protected override void ArrangeChildren(
            Size finalSize)
        {
            foreach (var child in this.LayoutChildren)
            {
                var position = this._children.GetLayout(child);
                var size = child.DesiredSize;
                child.Arrange(new Rect(position.X, position.Y, size.Width, size.Height));
            }
        }
    }
}
