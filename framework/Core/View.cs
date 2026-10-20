using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace XForms
{
    public interface IViewDelegate
    {
        bool HandlePointerEvent(PointerInputEvent pointerInputEvent);
    }

    public interface IViewRenderer : IElementRenderer
    {
        bool IsVisible { get; set; }

        bool IsInputVisible { get; set; }

        /// <summary>
        /// Measures the native view within the available size (which excludes the
        /// view's margin and may be infinite). A native container measures its
        /// children by calling back into <see cref="View.MeasureContent"/>.
        /// </summary>
        Size MeasureNative(Size availableSize);

        /// <summary>
        /// Positions the native view at the frame, relative to its parent view. A
        /// native container arranges its children by calling back into
        /// <see cref="View.LayoutContent"/>.
        /// </summary>
        void ArrangeNative(Rectangle frame);

        /// <summary>
        /// Asks the platform to lay this view (and its ancestors) out again.
        /// </summary>
        void InvalidateLayout();

        float Opacity { get; set; }

        float Scale { get; set; }

        Task ScaleTo(float scale, TimeSpan duration, EasingFunction ease);

        Angle Rotation { get; set; }

        Task RotateTo(Angle angle, TimeSpan duration, EasingFunction ease);

        Point Translation { get; set; }

        Task TranslateTo(Point point, TimeSpan duration, EasingFunction ease);

        void StopAnimation();

        /// <summary>
        /// Spins the view continuously, one turn per period, on the platform's
        /// compositor or animation thread (so it keeps turning while the UI
        /// thread is busy).
        /// </summary>
        void StartContinuousRotation(TimeSpan period);

        void StopContinuousRotation();

        /// <summary>
        /// Moves keyboard focus to the view. Returns false where the platform
        /// has no keyboard focus for owner-drawn views.
        /// </summary>
        bool Focus();
    }

    public abstract class View : Element, IViewDelegate
    {
        protected View()
        {
        }

        public new IViewRenderer Renderer
        {
            get
            {
                return (IViewRenderer)base.Renderer;
            }
        }

        public bool IsVisible
        {
            get
            {
                return this.Renderer.IsVisible;
            }

            set
            {
                if (value != this.Renderer.IsVisible)
                {
                    this.Renderer.IsVisible = value;

                    // A hidden view takes no space, so its parent must lay out again
                    this.Renderer.InvalidateLayout();
                }
            }
        }

        public bool IsInputVisible
        {
            get
            {
                return this.Renderer.IsInputVisible;
            }

            set
            {
                this.Renderer.IsInputVisible = value;
            }
        }

        #region Accessibility

        private AccessibilityRole? _accessibilityRole;
        private string _accessibilityLabel;

        /// <summary>
        /// What the view is to assistive technology. Controls provide a default.
        /// </summary>
        public AccessibilityRole AccessibilityRole
        {
            get
            {
                return this._accessibilityRole ?? this.DefaultAccessibilityRole;
            }

            set
            {
                this._accessibilityRole = value;
            }
        }

        /// <summary>
        /// What assistive technology reads for the view. Controls with text
        /// provide a default.
        /// </summary>
        public string AccessibilityLabel
        {
            get
            {
                return this._accessibilityLabel ?? this.DefaultAccessibilityLabel;
            }

            set
            {
                this._accessibilityLabel = value;
            }
        }

        protected virtual AccessibilityRole DefaultAccessibilityRole
        {
            get
            {
                return AccessibilityRole.None;
            }
        }

        protected virtual string DefaultAccessibilityLabel
        {
            get
            {
                return null;
            }
        }

        #endregion Accessibility

        #region Layout

        private Size _requestedSize = new Size(Dimension.Auto, Dimension.Auto);
        private Size _maximumSize = new Size(Dimension.Auto, Dimension.Auto);
        private Thickness _margin = Thickness.Zero;
        private LayoutAlignment _horizontalAlignment = LayoutAlignment.Fill;
        private LayoutAlignment _verticalAlignment = LayoutAlignment.Fill;
        private Rectangle _frame;
        private Size _contentMeasureSize = new Size(float.NaN, float.NaN);

        /// <summary>
        /// Gets the size the view was last laid out at, or sets the size the view
        /// asks for (use <see cref="Dimension.Auto"/> to size to the content).
        /// </summary>
        public Size Size
        {
            get
            {
                return this._frame.Size;
            }

            set
            {
                this._requestedSize = value;
                this.InvalidateLayout();
            }
        }

        public Size MaximumSize
        {
            get
            {
                return this._maximumSize;
            }

            set
            {
                this._maximumSize = value;
                this.InvalidateLayout();
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
                this.InvalidateLayout();
            }
        }

        public LayoutAlignment HorizontalAlignment
        {
            get
            {
                return this._horizontalAlignment;
            }

            set
            {
                this._horizontalAlignment = value;
                this.InvalidateLayout();
            }
        }

        public LayoutAlignment VerticalAlignment
        {
            get
            {
                return this._verticalAlignment;
            }

            set
            {
                this._verticalAlignment = value;
                this.InvalidateLayout();
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
        public Rectangle Frame
        {
            get
            {
                return this._frame;
            }
        }

        public void InvalidateLayout()
        {
            this._contentMeasureSize = new Size(float.NaN, float.NaN);
            this.Renderer?.InvalidateLayout();
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
                this._maximumSize.Width);

            float availableHeight = ConstrainLength(
                Math.Max(0, availableSize.Height - margin.Top - margin.Bottom),
                this._requestedSize.Height,
                this._maximumSize.Height);

            Size measuredSize = this.Renderer.MeasureNative(new Size(availableWidth, availableHeight));

            float width = ConstrainLength(measuredSize.Width, this._requestedSize.Width, this._maximumSize.Width);
            float height = ConstrainLength(measuredSize.Height, this._requestedSize.Height, this._maximumSize.Height);

            this.DesiredSize = new Size(
                width + margin.Left + margin.Right,
                height + margin.Top + margin.Bottom);
        }

        /// <summary>
        /// Positions the view within the rectangle (relative to the parent view)
        /// according to its margin, alignment and size. Called by the parent view
        /// after <see cref="Measure"/>.
        /// </summary>
        public void Arrange(
            Rectangle finalRect)
        {
            var margin = this._margin;

            float slotX = finalRect.X + margin.Left;
            float slotY = finalRect.Y + margin.Top;
            float slotWidth = Math.Max(0, finalRect.Width - margin.Left - margin.Right);
            float slotHeight = Math.Max(0, finalRect.Height - margin.Top - margin.Bottom);

            var desiredSize = this.DesiredSize;

            float width = ArrangeLength(
                slotWidth,
                desiredSize.Width - margin.Left - margin.Right,
                this._horizontalAlignment,
                this._requestedSize.Width,
                this._maximumSize.Width);

            float height = ArrangeLength(
                slotHeight,
                desiredSize.Height - margin.Top - margin.Bottom,
                this._verticalAlignment,
                this._requestedSize.Height,
                this._maximumSize.Height);

            float x = slotX + AlignmentOffset(this._horizontalAlignment, slotWidth, width);
            float y = slotY + AlignmentOffset(this._verticalAlignment, slotHeight, height);

            this._frame = new Rectangle(x, y, width, height);

            this.Renderer.ArrangeNative(this._frame);
        }

        /// <summary>
        /// Measures the view's own children within the available size and returns
        /// the size they need. Called by native containers from their measure pass;
        /// views without layout children return an empty size.
        /// </summary>
        public Size MeasureContent(
            Size availableSize)
        {
            this._contentMeasureSize = availableSize;

            return this.MeasureOverride(availableSize);
        }

        /// <summary>
        /// Positions the view's own children within the given size. Called by
        /// native containers from their layout pass; remeasures the children first
        /// if the final size differs from the size they were measured at.
        /// </summary>
        public void LayoutContent(
            Size size)
        {
            if (size != this._contentMeasureSize)
            {
                this.MeasureContent(size);
            }

            this.ArrangeOverride(size);
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
            this.ArrangeOverride(size);
        }

        /// <summary>
        /// Measures the children (by calling their <see cref="Measure"/>) and
        /// returns the size the view needs for them.
        /// </summary>
        protected virtual Size MeasureOverride(
            Size availableSize)
        {
            return Size.Empty;
        }

        /// <summary>
        /// Arranges the children (by calling their <see cref="Arrange"/>) within
        /// the final size.
        /// </summary>
        protected virtual void ArrangeOverride(
            Size finalSize)
        {
        }

        private static float ConstrainLength(
            float length,
            float requestedLength,
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

            return length;
        }

        private static float ArrangeLength(
            float slotLength,
            float desiredLength,
            LayoutAlignment alignment,
            float requestedLength,
            float maximumLength)
        {
            float length;

            if (!float.IsNaN(requestedLength))
            {
                length = requestedLength;
            }
            else if (alignment == LayoutAlignment.Fill)
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

            return Math.Max(0, length);
        }

        private static float AlignmentOffset(
            LayoutAlignment alignment,
            float slotLength,
            float length)
        {
            switch (alignment)
            {
                case LayoutAlignment.Start:
                    return 0;

                case LayoutAlignment.End:
                    return slotLength - length;

                default:
                    // Center, or Fill when the length is limited
                    return (slotLength - length) / 2;
            }
        }

        #endregion Layout

        public float Opacity
        {
            get
            {
                return this.Renderer.Opacity;
            }

            set
            {
                this.Renderer.Opacity = value;
            }
        }

        public float Scale
        {
            get
            {
                return this.Renderer.Scale;
            }

            set
            {
                this.Renderer.Scale = value;
            }
        }

        public Task ScaleTo(
            float scale,
            TimeSpan duration,
            EasingFunction ease)
        {
            return this.Renderer.ScaleTo(scale, duration, ease);
        }

        public Angle Rotation
        {
            get
            {
                return this.Renderer.Rotation;
            }

            set
            {
                this.Renderer.Rotation = value;
            }
        }

        public Task RotateTo(
            Angle angle,
            TimeSpan duration,
            EasingFunction ease)
        {
            return this.Renderer.RotateTo(angle, duration, ease);
        }

        public Point Translation
        {
            get
            {
                return this.Renderer.Translation;
            }

            set
            {
                this.Renderer.Translation = value;
            }
        }

        public Task TranslateTo(
            Point point,
            TimeSpan duration,
            EasingFunction ease)
        {
            return this.Renderer.TranslateTo(point, duration, ease);
        }

        public void StopAnimation()
        {
            this.Renderer.StopAnimation();
        }

        /// <summary>
        /// Spins the view continuously, one turn per period (clockwise, around
        /// its center), until <see cref="StopContinuousRotation"/>. The platform
        /// runs the animation off the UI thread where it can.
        /// </summary>
        public void StartContinuousRotation(
            TimeSpan period)
        {
            if (period <= TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(period));
            }

            this.Renderer.StartContinuousRotation(period);
        }

        public void StopContinuousRotation()
        {
            this.Renderer.StopContinuousRotation();
        }

        public GestureRecognizer GestureRecognizer
        {
            get;
            set;
        }

        #region Keyboard

        private bool? _isKeyboardFocusable;
        private bool _isFocused;

        /// <summary>
        /// Whether keyboard focus can move to the view (with Tab). By default,
        /// views that respond to taps are focusable.
        /// </summary>
        public bool IsKeyboardFocusable
        {
            get
            {
                return this._isKeyboardFocusable ?? (this.GestureRecognizer is Input.TapGestureRecognizer);
            }

            set
            {
                this._isKeyboardFocusable = value;
            }
        }

        /// <summary>
        /// Whether the view has keyboard focus.
        /// </summary>
        public bool IsFocused
        {
            get
            {
                return this._isFocused;
            }

            internal set
            {
                if (value != this._isFocused)
                {
                    this._isFocused = value;
                    this.OnFocusChanged();
                }
            }
        }

        /// <summary>
        /// Moves keyboard focus to the view. Returns false where the platform
        /// doesn't support keyboard focus for the view.
        /// </summary>
        public bool Focus()
        {
            return this.Renderer?.Focus() ?? false;
        }

        /// <summary>
        /// Called when the view gains or loses keyboard focus.
        /// </summary>
        protected virtual void OnFocusChanged()
        {
        }

        /// <summary>
        /// Called when a key is pressed while the view (or one of its
        /// descendants, which didn't handle it) has keyboard focus. Returns
        /// whether the key was handled.
        /// </summary>
        protected internal virtual bool OnKeyDown(
            KeyInputEvent keyInputEvent)
        {
            return false;
        }

        /// <summary>
        /// Does what a tap would: called for Enter or Space on a focused view,
        /// and for a screen reader's activate command. By default this sends a
        /// tap to the center of the view.
        /// </summary>
        protected internal virtual void OnActivate()
        {
            var center = new Point(this.Frame.Width / 2, this.Frame.Height / 2);
            this.HandlePointerEvent(new PointerInputEvent(center, PointerInputState.Began, true));
            this.HandlePointerEvent(new PointerInputEvent(center, PointerInputState.Ended, true));
        }

        #endregion Keyboard

        public virtual bool HandlePointerEvent(
            PointerInputEvent pointerInputEvent)
        {
            if (this.Application.HandlePreviewInput(this, pointerInputEvent))
            {
                return true;
            }

            GestureEventResult result = new GestureEventResult();
            this.GestureRecognizer?.HandlePointerInput(pointerInputEvent, ref result);
            return result.DidHandleEvent;
        }

        internal override IEnumerable<Element> GetChildElements()
        {
            yield break;
        }
    }
}
