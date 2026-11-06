using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using XForma.Platform;

namespace XForma
{
    // Measuring and arranging
    public partial class View
    {
        private Size _requestedSize = new Size(Dimension.Auto, Dimension.Auto);
        private Size _minimumSize = Size.Empty;
        private Size _maximumSize = new Size(Dimension.Auto, Dimension.Auto);
        private Thickness _margin = Thickness.Zero;
        private Thickness _padding = Thickness.Zero;
        private SafeAreaEdges _extendsIntoSafeArea;

        // How far the view reaches into the safe area (an extending view), or
        // how far a page keeps its content from the system's bars: added to
        // its padding
        private Thickness _safeAreaExtension;
        private HorizontalAlignment _horizontalAlignment = HorizontalAlignment.Stretch;
        private VerticalAlignment _verticalAlignment = VerticalAlignment.Stretch;
        private FlowDirection? _flowDirection;
        private int _zIndex;
        private bool _clipToBounds;
        private Shadow? _shadow;
        private Thickness _hitTestOutset;
        private Rect _frame;
        private Size _contentMeasureSize = new Size(float.NaN, float.NaN);
        private Size _contentArrangeSize;

        /// <summary>
        /// The width the view asks for, or <see cref="Dimension.Auto"/> (the default) to size to its content.
        /// </summary>
        public float Width
        {
            get
            {
                return this._requestedSize.Width;
            }

            set
            {
                this._requestedSize = new Size(value, this._requestedSize.Height);
                this.InvalidateMeasure();

                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// The height the view asks for, or <see cref="Dimension.Auto"/> (the default) to size to its content.
        /// </summary>
        public float Height
        {
            get
            {
                return this._requestedSize.Height;
            }

            set
            {
                this._requestedSize = new Size(this._requestedSize.Width, value);
                this.InvalidateMeasure();

                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// The least width the view takes (0 by default).
        /// </summary>
        public float MinWidth
        {
            get
            {
                return this._minimumSize.Width;
            }

            set
            {
                this._minimumSize = new Size(value, this._minimumSize.Height);
                this.InvalidateMeasure();

                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// The least height the view takes (0 by default).
        /// </summary>
        public float MinHeight
        {
            get
            {
                return this._minimumSize.Height;
            }

            set
            {
                this._minimumSize = new Size(this._minimumSize.Width, value);
                this.InvalidateMeasure();

                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// The most width the view takes, or <see cref="Dimension.Auto"/> (the default) for no limit.
        /// </summary>
        public float MaxWidth
        {
            get
            {
                return this._maximumSize.Width;
            }

            set
            {
                this._maximumSize = new Size(value, this._maximumSize.Height);
                this.InvalidateMeasure();

                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// The most height the view takes, or <see cref="Dimension.Auto"/> (the default) for no limit.
        /// </summary>
        public float MaxHeight
        {
            get
            {
                return this._maximumSize.Height;
            }

            set
            {
                this._maximumSize = new Size(this._maximumSize.Width, value);
                this.InvalidateMeasure();

                this.OnPropertyChanged();
            }
        }

        public Thickness Margin
        {
            get
            {
                return this._margin;
            }

            set
            {
                this._margin = value;
                this.InvalidateMeasure();

                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Space between the view's edges and its children. Containers expose
        /// it as <c>Padding</c>.
        /// </summary>
        protected Thickness ContentPadding
        {
            get
            {
                return this._padding;
            }

            set
            {
                this._padding = value;
                this.InvalidateMeasure();
            }
        }

        /// <summary>
        /// The edges on which the view reaches into the safe area: where it
        /// lies along the edge of a page that keeps clear of the system's
        /// bars, it grows through them to the screen's edge, and its content
        /// is padded by as much, staying clear. So a bar docked at the top or
        /// bottom of a page fills the status bar or home indicator area behind
        /// it, and moves as one view when it animates. None by default.
        /// </summary>
        public SafeAreaEdges ExtendsIntoSafeArea
        {
            get
            {
                return this._extendsIntoSafeArea;
            }

            set
            {
                if (value != this._extendsIntoSafeArea)
                {
                    this._extendsIntoSafeArea = value;
                    this.InvalidateMeasure();
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// The view's padding, with the safe area's added (see
        /// <see cref="ExtendsIntoSafeArea"/>, and a page's insets).
        /// </summary>
        private Thickness LayoutPadding
        {
            get
            {
                var safeArea = this.SafeAreaPadding;
                var padding = this._padding;
                return new Thickness(
                    padding.Left + safeArea.Left,
                    padding.Top + safeArea.Top,
                    padding.Right + safeArea.Right,
                    padding.Bottom + safeArea.Bottom);
            }
        }

        /// <summary>
        /// The space the safe area adds to the view's padding: a page's insets
        /// from the system's bars, or how far an extending view reaches.
        /// </summary>
        internal virtual Thickness SafeAreaPadding
        {
            get
            {
                return this._safeAreaExtension;
            }
        }

        /// <summary>
        /// How far the view reaches into the safe area (see
        /// <see cref="ExtendsIntoSafeArea"/>).
        /// </summary>
        internal Thickness SafeAreaExtension
        {
            get
            {
                return this._safeAreaExtension;
            }
        }

        /// <summary>
        /// The view reaches into the safe area by another amount: a scrolling
        /// view insets its content by it instead, so its content scrolls
        /// beneath the system's bars, and its ends can scroll clear of them.
        /// </summary>
        internal virtual void OnSafeAreaExtensionChanged()
        {
        }

        // An extending view along the edge of a page's safe area grows through
        // the page's insets there (the rectangle is in the parent's coordinates)
        private Rect ExtendIntoSafeArea(
            Rect rect)
        {
            var extension = Thickness.Zero;
            Page? page = null;
            float x = rect.X, y = rect.Y;
            // The page that keeps clear of the system's bars: the main frame's
            // (a page in a frame inside it, such as a tab's, passes it on)
            for (var ancestor = this.Parent as View; null != ancestor; ancestor = ancestor.Parent as View)
            {
                if ((ancestor is Page candidate) && (candidate.SafeAreaPadding != Thickness.Zero))
                {
                    page = candidate;
                    break;
                }

                x += ancestor._frame.X;
                y += ancestor._frame.Y;
            }

            var insets = page?.SafeAreaPadding ?? Thickness.Zero;
            var edges = SafeAreaEdges.None;
            if (null != page)
            {
                const float Tolerance = 0.5f;
                // (A page's frame is the platform's to place: its size is the
                // one it last laid out at)
                var size = page._contentArrangeSize;
                var wanted = this._extendsIntoSafeArea;
                if (wanted.HasFlag(SafeAreaEdges.Left) && (0 < insets.Left) && (Math.Abs(x - insets.Left) < Tolerance))
                {
                    extension = new Thickness(insets.Left, extension.Top, extension.Right, extension.Bottom);
                    edges |= SafeAreaEdges.Left;
                }

                if (wanted.HasFlag(SafeAreaEdges.Top) && (0 < insets.Top) && (Math.Abs(y - insets.Top) < Tolerance))
                {
                    extension = new Thickness(extension.Left, insets.Top, extension.Right, extension.Bottom);
                    edges |= SafeAreaEdges.Top;
                }

                if (wanted.HasFlag(SafeAreaEdges.Right) && (0 < insets.Right) && (Math.Abs((x + rect.Width) - (size.Width - insets.Right)) < Tolerance))
                {
                    extension = new Thickness(extension.Left, extension.Top, insets.Right, extension.Bottom);
                    edges |= SafeAreaEdges.Right;
                }

                if (wanted.HasFlag(SafeAreaEdges.Bottom) && (0 < insets.Bottom) && (Math.Abs((y + rect.Height) - (size.Height - insets.Bottom)) < Tolerance))
                {
                    extension = new Thickness(extension.Left, extension.Top, extension.Right, insets.Bottom);
                    edges |= SafeAreaEdges.Bottom;
                }

                page.SetExtendedEdges(this, edges);
            }

            if (extension != this._safeAreaExtension)
            {
                this._safeAreaExtension = extension;
                this.OnSafeAreaExtensionChanged();
            }

            return new Rect(
                rect.X - extension.Left,
                rect.Y - extension.Top,
                rect.Width + extension.Left + extension.Right,
                rect.Height + extension.Top + extension.Bottom);
        }

        public HorizontalAlignment HorizontalAlignment
        {
            get
            {
                return this._horizontalAlignment;
            }

            set
            {
                this._horizontalAlignment = value;
                this.InvalidateMeasure();

                this.OnPropertyChanged();
            }
        }

        public VerticalAlignment VerticalAlignment
        {
            get
            {
                return this._verticalAlignment;
            }

            set
            {
                this._verticalAlignment = value;
                this.InvalidateMeasure();

                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Whether the view's children are laid out left to right or right to
        /// left. Inherited from the parent unless set.
        /// </summary>
        /// <remarks>
        /// Right to left mirrors the positions of the children; text and
        /// drawing aren't mirrored.
        /// </remarks>
        public FlowDirection FlowDirection
        {
            get
            {
                return this._flowDirection ?? (this.Parent as View)?.FlowDirection ?? FlowDirection.LeftToRight;
            }

            set
            {
                this._flowDirection = value;
                this.OnFlowDirectionChanged();

                this.OnPropertyChanged();
            }
        }

        // The view, and the descendants that take its direction, lay out and
        // draw again (each mirrors its children, and may draw by direction)
        private void OnFlowDirectionChanged()
        {
            this.InvalidateMeasure();
            this.InvalidateArrange();
            if (this._drawsContent)
            {
                this.InvalidateVisual();
            }

            foreach (var child in this.VisualChildren)
            {
                if (!child._flowDirection.HasValue)
                {
                    child.OnFlowDirectionChanged();
                }
            }
        }

        /// <summary>
        /// The view's order among its siblings in a panel: views with a higher
        /// index draw above (and are hit before) views with a lower one. Views
        /// with the same index keep the order they were added in. To draw over
        /// a native sibling, such as a frame, set it before the view is shown:
        /// a view with an index other than 0 is then a layer of its own.
        /// </summary>
        public int ZIndex
        {
            get
            {
                return this._zIndex;
            }

            set
            {
                if (value != this._zIndex)
                {
                    this._zIndex = value;
                    (this.Parent as View)?.OnChildZIndexChanged(this);
                }

                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Whether the view clips its children (and drawing) to its bounds.
        /// False by default: children can draw outside their parent.
        /// </summary>
        public bool ClipToBounds
        {
            get
            {
                return this._clipToBounds;
            }

            set
            {
                this._clipToBounds = value;
                this.Native?.ClipToBounds = value;

                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// A drop shadow beneath the view, outside its bounds, or null (the
        /// default) for none. It moves, scales and fades with the view's
        /// animators. A frame gives its pages one while they slide.
        /// </summary>
        public Shadow? Shadow
        {
            get
            {
                return this._shadow;
            }

            set
            {
                if (value != this._shadow)
                {
                    this._shadow = value;
                    this.Native?.Shadow = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// How far past its bounds the view takes presses (taps and gestures
        /// alike), on each side: a bigger touch target than it looks, as
        /// UIKit's <c>point(inside:)</c> and Android's touch delegates give.
        /// Zero by default. A view drawn above it still takes presses on
        /// itself; what's beneath it doesn't, in the outset.
        /// </summary>
        /// <remarks>
        /// The outset reaches only as far as the view's parents take presses:
        /// a lightweight view's stops at its lightweight ancestors' bounds.
        /// </remarks>
        public Thickness HitTestOutset
        {
            get
            {
                return this._hitTestOutset;
            }

            set
            {
                if (value != this._hitTestOutset)
                {
                    this._hitTestOutset = value;
                    this.Native?.HitTestOutset = value;
                    this.OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// The size the view asked for in the last measure, including its margin.
        /// </summary>
        public Size DesiredSize
        {
            get;
            private set;
        }

        /// <summary>
        /// The view's position and size from the last arrange, relative to its parent.
        /// </summary>
        public Rect Bounds
        {
            get
            {
                return this._frame;
            }
        }

        /// <summary>
        /// Lays the view out again because its size may have changed: its parent
        /// measures it again, and arranges it.
        /// </summary>
        public void InvalidateMeasure()
        {
            this._contentMeasureSize = new Size(float.NaN, float.NaN);
            this.Native?.InvalidateMeasure();
        }

        /// <summary>
        /// Arranges the view's children again, without changing the view's size.
        /// </summary>
        public void InvalidateArrange()
        {
            this.Native?.InvalidateMeasure();
        }

        /// <summary>
        /// Measures the view within the available size, which may be infinite, and
        /// updates <see cref="DesiredSize"/>. Called by the parent view.
        /// </summary>
        public void Measure(
            Size availableSize)
        {
            if (!this.IsVisible)
            {
                this.DesiredSize = Size.Empty;
                return;
            }

            var margin = this._margin;

            float availableWidth = ConstrainLength(
                Math.Max(0, availableSize.Width - margin.Left - margin.Right),
                this._requestedSize.Width,
                this._minimumSize.Width,
                this._maximumSize.Width);

            float availableHeight = ConstrainLength(
                Math.Max(0, availableSize.Height - margin.Top - margin.Bottom),
                this._requestedSize.Height,
                this._minimumSize.Height,
                this._maximumSize.Height);

            // An extending view lays its content out within its extension (in
            // its padding), but takes its place without it
            var extension = this._safeAreaExtension;
            Size measuredSize = this.Renderer.MeasureNative(new Size(
                availableWidth + extension.Left + extension.Right,
                availableHeight + extension.Top + extension.Bottom));
            measuredSize = new Size(
                Math.Max(0, measuredSize.Width - extension.Left - extension.Right),
                Math.Max(0, measuredSize.Height - extension.Top - extension.Bottom));

            float width = ConstrainLength(measuredSize.Width, this._requestedSize.Width, this._minimumSize.Width, this._maximumSize.Width);
            float height = ConstrainLength(measuredSize.Height, this._requestedSize.Height, this._minimumSize.Height, this._maximumSize.Height);

            this.DesiredSize = new Size(
                width + margin.Left + margin.Right,
                height + margin.Top + margin.Bottom);
        }

        /// <summary>
        /// Positions the view within the rectangle (relative to the parent view)
        /// according to its margin, alignment and size. Called by the parent view
        /// after <see cref="Measure"/>.
        /// </summary>
        /// <summary>
        /// Whether the view mirrors its children's places when right to left:
        /// all views but those whose platforms place their content themselves
        /// (a scroll viewer's, which fills its width).
        /// </summary>
        internal virtual bool MirrorsChildren
        {
            get
            {
                return true;
            }
        }

        // The direction the view was last drawn in: a view moved (or
        // re-attached) under another direction draws again
        private FlowDirection? _drawnFlowDirection;

        public void Arrange(
            Rect finalRect)
        {
            if (this._drawnFlowDirection.HasValue && (this._drawnFlowDirection != this.FlowDirection))
            {
                this._drawnFlowDirection = null;
                this.InvalidateVisual();
            }

            var margin = this._margin;

            float slotX = finalRect.X + margin.Left;
            float slotY = finalRect.Y + margin.Top;
            float slotWidth = Math.Max(0, finalRect.Width - margin.Left - margin.Right);
            float slotHeight = Math.Max(0, finalRect.Height - margin.Top - margin.Bottom);

            var desiredSize = this.DesiredSize;

            float width = ArrangeLength(
                slotWidth,
                desiredSize.Width - margin.Left - margin.Right,
                (int)this._horizontalAlignment,
                this._requestedSize.Width,
                this._minimumSize.Width,
                this._maximumSize.Width);

            float height = ArrangeLength(
                slotHeight,
                desiredSize.Height - margin.Top - margin.Bottom,
                (int)this._verticalAlignment,
                this._requestedSize.Height,
                this._minimumSize.Height,
                this._maximumSize.Height);

            float x = slotX + AlignmentOffset((int)this._horizontalAlignment, slotWidth, width);
            float y = slotY + AlignmentOffset((int)this._verticalAlignment, slotHeight, height);

            // The parent's padding offsets its children, and a right-to-left
            // parent mirrors them
            if (this.Parent is View parent)
            {
                var padding = parent.LayoutPadding;
                x += padding.Left;
                y += padding.Top;

                if ((parent.FlowDirection == FlowDirection.RightToLeft) && parent.MirrorsChildren)
                {
                    x = parent._contentArrangeSize.Width - x - width;
                }
            }

            this._frame = new Rect(x, y, width, height);
            if ((SafeAreaEdges.None != this._extendsIntoSafeArea) || (Thickness.Zero != this._safeAreaExtension))
            {
                this._frame = this.ExtendIntoSafeArea(this._frame);
            }

            if (!(this.Parent is View))
            {
                this.OnRootSizeChanged(new Size(width, height));
            }

            // A native view inside lightweight ancestors is positioned in their
            // layer
            var nativeFrame = this._frame;
            if ((this.Parent is View host) && host.IsLightweight && !this.IsLightweight)
            {
                var offset = host.OffsetInLayer;
                nativeFrame = new Rect(nativeFrame.X + offset.X, nativeFrame.Y + offset.Y, nativeFrame.Width, nativeFrame.Height);
            }

            this.Renderer.ArrangeNative(nativeFrame);
        }

        /// <summary>
        /// Measures the view's own children within the available size and returns
        /// the size they need, plus the view's padding. Called by native
        /// containers from their measure pass; views without layout children
        /// return their padding.
        /// </summary>
        public Size MeasureContent(
            Size availableSize)
        {
            this._contentMeasureSize = availableSize;

            var padding = this.LayoutPadding;
            var size = this.MeasureOverride(new Size(
                Math.Max(0, availableSize.Width - padding.Left - padding.Right),
                Math.Max(0, availableSize.Height - padding.Top - padding.Bottom)));

            return new Size(
                size.Width + padding.Left + padding.Right,
                size.Height + padding.Top + padding.Bottom);
        }

        /// <summary>
        /// Positions the view's own children within the given size. Called by
        /// native containers from their layout pass; remeasures the children first
        /// if the final size differs from the size they were measured at.
        /// </summary>
        public void LayoutContent(
            Size size)
        {
            // A root view (a popup's content, say) has no parent to arrange it:
            // its size is its native view's
            if (!(this.Parent is View))
            {
                this._frame = new Rect(this._frame.X, this._frame.Y, size.Width, size.Height);
                this.OnRootSizeChanged(size);
            }

            if (size != this._contentMeasureSize)
            {
                this.MeasureContent(size);
            }

            this.ArrangeContent(size);
        }

        /// <summary>
        /// Positions the view's own children within the given size using their
        /// last measurement. For platforms whose layout system guarantees a measure
        /// pass before every arrange (remeasuring during arrange there would make
        /// the platform lay out again).
        /// </summary>
        public void ArrangeContent(
            Size size)
        {
            this._contentArrangeSize = size;

            var padding = this.LayoutPadding;
            var contentSize = new Size(
                Math.Max(0, size.Width - padding.Left - padding.Right),
                Math.Max(0, size.Height - padding.Top - padding.Bottom));
            this.ArrangeOverride(contentSize);
            this.OnContentArranged(contentSize);
        }

        /// <summary>
        /// The size the view last laid its content out at (a page's, where the
        /// platform places the page).
        /// </summary>
        internal Size ContentArrangeSize
        {
            get
            {
                return this._contentArrangeSize;
            }
        }

        /// <summary>
        /// Called once the children are arranged within the padding.
        /// </summary>
        internal virtual void OnContentArranged(
            Size finalSize)
        {
        }

        /// <summary>
        /// Measures the children (by calling their <see cref="Measure"/>) and
        /// returns the size the view needs for them, within its padding.
        /// </summary>
        protected virtual Size MeasureOverride(
            Size availableSize)
        {
            // By default the children overlap, each in the whole area
            float width = 0, height = 0;
            foreach (var child in this.VisualChildren)
            {
                child.Measure(availableSize);
                width = Math.Max(width, child.DesiredSize.Width);
                height = Math.Max(height, child.DesiredSize.Height);
            }

            return new Size(width, height);
        }

        /// <summary>
        /// Arranges the children (by calling their <see cref="Arrange"/>) within
        /// the final size, inside the view's padding: a child arranged at (0, 0)
        /// sits at the padding's top left corner.
        /// </summary>
        protected virtual void ArrangeOverride(
            Size finalSize)
        {
            foreach (var child in this.VisualChildren)
            {
                child.Arrange(new Rect(0, 0, finalSize.Width, finalSize.Height));
            }
        }

        // The alignment enums share these values: start (left or top), center,
        // end (right or bottom) and stretch
        private const int Start = 0;
        private const int End = 2;
        private const int Stretch = 3;

        private static float ConstrainLength(
            float length,
            float requestedLength,
            float minimumLength,
            float maximumLength)
        {
            if (!float.IsNaN(requestedLength))
            {
                length = requestedLength;
            }

            if (!float.IsNaN(maximumLength))
            {
                length = Math.Min(length, maximumLength);
            }

            return Math.Max(length, minimumLength);
        }

        private static float ArrangeLength(
            float slotLength,
            float desiredLength,
            int alignment,
            float requestedLength,
            float minimumLength,
            float maximumLength)
        {
            float length;

            if (!float.IsNaN(requestedLength))
            {
                length = requestedLength;
            }
            else if (alignment == Stretch)
            {
                length = slotLength;
            }
            else
            {
                length = Math.Min(desiredLength, slotLength);
            }

            if (!float.IsNaN(maximumLength))
            {
                length = Math.Min(length, maximumLength);
            }

            return Math.Max(Math.Max(0, length), minimumLength);
        }

        private static float AlignmentOffset(
            int alignment,
            float slotLength,
            float length)
        {
            switch (alignment)
            {
                case Start:
                    return 0;

                case End:
                    return slotLength - length;

                default:
                    // Center, or Stretch when the length is limited
                    return (slotLength - length) / 2;
            }
        }
    }
}
