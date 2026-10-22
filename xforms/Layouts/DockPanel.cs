using System;
using XForms.Controls;

namespace XForms.Layouts
{
    public enum Dock
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
    public class DockPanel : Panel
    {
        private readonly ViewCollection<Dock> _children;

        public DockPanel()
        {
            this._children = new ViewCollection<Dock>(this, Dock.CenterOverlay);
        }

        /// <summary>
        /// The children, each docked to an edge or overlaid:
        /// <c>Children.Add(view, Dock.Top)</c>.
        /// </summary>
        public ViewCollection<Dock> Children
        {
            get
            {
                return this._children;
            }
        }

        private Dock GetDock(
            View child)
        {
            return this._children.GetLayout(child);
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
                var dockRegion = GetDock(child);
                if (dockRegion == Dock.CenterOverlay)
                {
                    continue;
                }

                child.Measure(remainingSize);

                Size childSize = child.DesiredSize;

                switch (dockRegion)
                {
                    case Dock.Left:
                        usedLeft = Math.Max(usedLeft, startLeft + childSize.Width);
                        startLeft += childSize.Width;
                        minHeight = Math.Max(minHeight, usedTop + usedBottom + childSize.Height);
                        break;

                    case Dock.LeftOverlay:
                        usedLeft = Math.Max(usedLeft, startLeft + childSize.Width);
                        minHeight = Math.Max(minHeight, usedTop + usedBottom + childSize.Height);
                        break;

                    case Dock.Top:
                        usedTop = Math.Max(usedTop, startTop + childSize.Height);
                        startTop += childSize.Height;
                        minWidth = Math.Max(minWidth, usedLeft + usedRight + childSize.Width);
                        break;

                    case Dock.TopOverlay:
                        usedTop = Math.Max(usedTop, startTop + childSize.Height);
                        minWidth = Math.Max(minWidth, usedLeft + usedRight + childSize.Width);
                        break;

                    case Dock.Right:
                        usedRight = Math.Max(usedRight, startRight + childSize.Width);
                        startRight += childSize.Width;
                        minHeight = Math.Max(minHeight, usedTop + usedBottom + childSize.Height);
                        break;

                    case Dock.RightOverlay:
                        usedRight = Math.Max(usedRight, startRight + childSize.Width);
                        minHeight = Math.Max(minHeight, usedTop + usedBottom + childSize.Height);
                        break;

                    case Dock.Bottom:
                        usedBottom = Math.Max(usedBottom, startBottom + childSize.Height);
                        startBottom += childSize.Height;
                        minWidth = Math.Max(minWidth, usedLeft + usedRight + childSize.Width);
                        break;

                    case Dock.BottomOverlay:
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
                if (GetDock(child) != Dock.CenterOverlay)
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
                var dockRegion = GetDock(child);
                if (dockRegion == Dock.CenterOverlay)
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
                    case Dock.Left:
                        width = childSize.Width;
                        remainingX += childSize.Width;
                        remainingWidth = Math.Max(0, remainingWidth - childSize.Width);
                        break;

                    case Dock.LeftOverlay:
                        width = childSize.Width;
                        break;

                    case Dock.Top:
                        height = childSize.Height;
                        remainingY += childSize.Height;
                        remainingHeight = Math.Max(0, remainingHeight - childSize.Height);
                        break;

                    case Dock.TopOverlay:
                        height = childSize.Height;
                        break;

                    case Dock.Right:
                        x = remainingX + remainingWidth - childSize.Width;
                        width = childSize.Width;
                        remainingWidth = Math.Max(0, remainingWidth - childSize.Width);
                        break;

                    case Dock.RightOverlay:
                        x = remainingX + remainingWidth - childSize.Width;
                        width = childSize.Width;
                        break;

                    case Dock.Bottom:
                        y = remainingY + remainingHeight - childSize.Height;
                        height = childSize.Height;
                        remainingHeight = Math.Max(0, remainingHeight - childSize.Height);
                        break;

                    case Dock.BottomOverlay:
                        y = remainingY + remainingHeight - childSize.Height;
                        height = childSize.Height;
                        break;

                    default:
                        throw new NotSupportedException("Unsupported dock region.");
                }

                child.Arrange(new Rect(x, y, width, height));
            }

            // The center children share whatever is left
            var centerRect = new Rect(remainingX, remainingY, remainingWidth, remainingHeight);
            for (int i = 0; i < children.Count; i += 1)
            {
                var child = children[i];
                if (GetDock(child) == Dock.CenterOverlay)
                {
                    child.Arrange(centerRect);
                }
            }
        }
    }
}
