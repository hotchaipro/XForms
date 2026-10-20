using System;

namespace XForms.Layouts
{
    public enum DockRegion
    {
        CenterOverlay = 0,
        Left,
        Top,
        Right,
        Bottom,
        LeftOverlay,
        TopOverlay,
        RightOverlay,
        BottomOverlay,
    }

    /// <summary>
    /// Docks children to the edges in the order they were added, then fills the
    /// remaining center. Overlay regions dock without taking the space away from
    /// the children that follow.
    /// </summary>
    public class DockLayout : Layout
    {
        internal static readonly AttachedProperty<DockRegion> DockRegionProperty = new AttachedProperty<DockRegion>();

        public class DockLayoutViewCollection : LayoutViewCollection
        {
            public DockLayoutViewCollection(
                DockLayout layout)
                : base(layout)
            {
            }

            public void Add(
                View child,
                DockRegion position)
            {
                if (null == child)
                {
                    throw new ArgumentNullException(nameof(child));
                }

                DockRegionProperty.SetValue(child, position);

                this.Add(child);
            }

            public void Insert(
                int index,
                View child,
                DockRegion position)
            {
                if (null == child)
                {
                    throw new ArgumentNullException(nameof(child));
                }

                DockRegionProperty.SetValue(child, position);

                this.Insert(index, child);
            }

            public void Replace(
                int index,
                View child,
                DockRegion position)
            {
                if (null == child)
                {
                    throw new ArgumentNullException(nameof(child));
                }

                DockRegionProperty.SetValue(child, position);

                this.Replace(index, child);
            }
        }

        private DockLayoutViewCollection _children;

        public DockLayout()
        {
            this._children = new DockLayoutViewCollection(this);
        }

        public DockLayoutViewCollection Children
        {
            get
            {
                return this._children;
            }
        }

        private static DockRegion GetDockRegion(
            View child)
        {
            return DockRegionProperty.GetValue(child, DockRegion.CenterOverlay);
        }

        protected override Size MeasureChildren(
            Size availableSize)
        {
            var children = this.LayoutChildren;

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

            // Measure the docked children first, in order
            for (int i = 0; i < children.Count; i += 1)
            {
                var child = children[i];
                var dockRegion = GetDockRegion(child);
                if (dockRegion == DockRegion.CenterOverlay)
                {
                    continue;
                }

                child.Measure(remainingSize);

                Size childSize = child.DesiredSize;

                switch (dockRegion)
                {
                    case DockRegion.Left:
                        usedLeft = Math.Max(usedLeft, startLeft + childSize.Width);
                        startLeft += childSize.Width;
                        minHeight = Math.Max(minHeight, usedTop + usedBottom + childSize.Height);
                        break;

                    case DockRegion.LeftOverlay:
                        usedLeft = Math.Max(usedLeft, startLeft + childSize.Width);
                        minHeight = Math.Max(minHeight, usedTop + usedBottom + childSize.Height);
                        break;

                    case DockRegion.Top:
                        usedTop = Math.Max(usedTop, startTop + childSize.Height);
                        startTop += childSize.Height;
                        minWidth = Math.Max(minWidth, usedLeft + usedRight + childSize.Width);
                        break;

                    case DockRegion.TopOverlay:
                        usedTop = Math.Max(usedTop, startTop + childSize.Height);
                        minWidth = Math.Max(minWidth, usedLeft + usedRight + childSize.Width);
                        break;

                    case DockRegion.Right:
                        usedRight = Math.Max(usedRight, startRight + childSize.Width);
                        startRight += childSize.Width;
                        minHeight = Math.Max(minHeight, usedTop + usedBottom + childSize.Height);
                        break;

                    case DockRegion.RightOverlay:
                        usedRight = Math.Max(usedRight, startRight + childSize.Width);
                        minHeight = Math.Max(minHeight, usedTop + usedBottom + childSize.Height);
                        break;

                    case DockRegion.Bottom:
                        usedBottom = Math.Max(usedBottom, startBottom + childSize.Height);
                        startBottom += childSize.Height;
                        minWidth = Math.Max(minWidth, usedLeft + usedRight + childSize.Width);
                        break;

                    case DockRegion.BottomOverlay:
                        usedBottom = Math.Max(usedBottom, startBottom + childSize.Height);
                        minWidth = Math.Max(minWidth, usedLeft + usedRight + childSize.Width);
                        break;

                    default:
                        throw new NotSupportedException("Unsupported dock region.");
                }

                remainingSize = new Size(
                    Math.Max(0, availableSize.Width - startLeft - startRight),
                    Math.Max(0, availableSize.Height - startTop - startBottom));
            }

            // Measure the center children now that the center size is known
            float centerWidth = 0;
            float centerHeight = 0;
            for (int i = 0; i < children.Count; i += 1)
            {
                var child = children[i];
                if (GetDockRegion(child) != DockRegion.CenterOverlay)
                {
                    continue;
                }

                child.Measure(remainingSize);

                Size childSize = child.DesiredSize;
                centerWidth = Math.Max(centerWidth, childSize.Width);
                centerHeight = Math.Max(centerHeight, childSize.Height);
            }

            return new Size(
                Math.Max(minWidth, usedLeft + usedRight + centerWidth),
                Math.Max(minHeight, usedTop + usedBottom + centerHeight));
        }

        protected override void ArrangeChildren(
            Size finalSize)
        {
            var children = this.LayoutChildren;

            float remainingX = 0;
            float remainingY = 0;
            float remainingWidth = finalSize.Width;
            float remainingHeight = finalSize.Height;

            // Arrange the docked children first, in order
            for (int i = 0; i < children.Count; i += 1)
            {
                var child = children[i];
                var dockRegion = GetDockRegion(child);
                if (dockRegion == DockRegion.CenterOverlay)
                {
                    continue;
                }

                Size childSize = child.DesiredSize;

                float x = remainingX;
                float y = remainingY;
                float width = remainingWidth;
                float height = remainingHeight;

                switch (dockRegion)
                {
                    case DockRegion.Left:
                        width = childSize.Width;
                        remainingX += childSize.Width;
                        remainingWidth = Math.Max(0, remainingWidth - childSize.Width);
                        break;

                    case DockRegion.LeftOverlay:
                        width = childSize.Width;
                        break;

                    case DockRegion.Top:
                        height = childSize.Height;
                        remainingY += childSize.Height;
                        remainingHeight = Math.Max(0, remainingHeight - childSize.Height);
                        break;

                    case DockRegion.TopOverlay:
                        height = childSize.Height;
                        break;

                    case DockRegion.Right:
                        x = remainingX + remainingWidth - childSize.Width;
                        width = childSize.Width;
                        remainingWidth = Math.Max(0, remainingWidth - childSize.Width);
                        break;

                    case DockRegion.RightOverlay:
                        x = remainingX + remainingWidth - childSize.Width;
                        width = childSize.Width;
                        break;

                    case DockRegion.Bottom:
                        y = remainingY + remainingHeight - childSize.Height;
                        height = childSize.Height;
                        remainingHeight = Math.Max(0, remainingHeight - childSize.Height);
                        break;

                    case DockRegion.BottomOverlay:
                        y = remainingY + remainingHeight - childSize.Height;
                        height = childSize.Height;
                        break;

                    default:
                        throw new NotSupportedException("Unsupported dock region.");
                }

                child.Arrange(new Rectangle(x, y, width, height));
            }

            // The center children share whatever is left
            var centerRect = new Rectangle(remainingX, remainingY, remainingWidth, remainingHeight);
            for (int i = 0; i < children.Count; i += 1)
            {
                var child = children[i];
                if (GetDockRegion(child) == DockRegion.CenterOverlay)
                {
                    child.Arrange(centerRect);
                }
            }
        }
    }
}
