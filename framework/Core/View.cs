using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace XForms
{
    public interface IViewDelegate
    {
        bool HandlePointerEvent(PointerEventArgs pointerInputEvent);
    }

    public interface IViewRenderer : IElementRenderer
    {
        bool IsVisible { get; set; }

        bool IsHitTestVisible { get; set; }

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
        void ArrangeNative(Rect frame);

        /// <summary>
        /// Asks the platform to lay this view (and its ancestors) out again.
        /// </summary>
        void InvalidateMeasure();

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
        private bool _isVisible = true;
        private bool _isHitTestVisible = true;
        private Animator _animator;
        private List<Animator> _ownAnimators;

        protected View()
        {
        }

        internal new IViewRenderer Renderer
        {
            get
            {
                return (IViewRenderer)base.Renderer;
            }
        }

        private IViewRenderer Native
        {
            get
            {
                return (IViewRenderer)this.CreatedRenderer;
            }
        }

        public bool IsVisible
        {
            get
            {
                return this._isVisible;
            }

            set
            {
                if (value != this._isVisible)
                {
                    this._isVisible = value;
                    this.Native?.IsVisible = value;

                    // A hidden view takes no space, so its parent must lay out again
                    this.InvalidateMeasure();
                }
            }
        }

        public bool IsHitTestVisible
        {
            get
            {
                return this._isHitTestVisible;
            }

            set
            {
                this._isHitTestVisible = value;
                this.Native?.IsHitTestVisible = value;
            }
        }

        internal override void OnRendererCreated()
        {
            base.OnRendererCreated();

            var native = this.Native;
            if (!this._isVisible)
            {
                native.IsVisible = false;
            }

            if (!this._isHitTestVisible)
            {
                native.IsHitTestVisible = false;
            }

            this._animator?.OnRendererCreated();

            if (null != this._ownAnimators)
            {
                foreach (var animator in this._ownAnimators)
                {
                    animator.OnRendererCreated();
                }
            }
        }

        #region Attach

        /// <summary>
        /// The view this view was added to, or null.
        /// </summary>
        public Element Parent
        {
            get;
            private set;
        }

        /// <summary>
        /// Raised when the view is added to a parent (or shown), once its native
        /// object exists.
        /// </summary>
        public event EventHandler Attached;

        /// <summary>
        /// Raised when the view is removed from its parent.
        /// </summary>
        public event EventHandler Detached;

        /// <summary>
        /// Attaches the view to its parent: creates its native object if it
        /// doesn't have one yet, and returns it for the parent to add.
        /// </summary>
        internal IViewRenderer AttachTo(
            Element parent)
        {
            this.Parent = parent;
            return this.Renderer;
        }

        /// <summary>
        /// Called by the parent once it has added the view's native object.
        /// </summary>
        internal void NotifyAttached()
        {
            this.OnAttached();
            this.Attached?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Called by the parent once it has removed the view's native object.
        /// </summary>
        internal void NotifyDetached()
        {
            if (null == this.Parent)
            {
                return;
            }

            this.Parent = null;
            this.OnDetached();
            this.Detached?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnAttached()
        {
        }

        protected virtual void OnDetached()
        {
        }

        #endregion Attach

        #region Animation

        /// <summary>
        /// The app's animator for the view: its translation, scale, rotation and
        /// opacity, set directly or animated. Null (the default) for a view that
        /// never animates. Set it when the view is created, for example
        /// <c>new ProgressRing { Animator = new() }</c>. Controls never set their
        /// own; they use <see cref="Controls.Control.CreateOwnAnimator"/>.
        /// </summary>
        public Animator Animator
        {
            get
            {
                return this._animator;
            }

            init
            {
                if (null != this._animator)
                {
                    throw new InvalidOperationException("The view already has an Animator.");
                }

                if (null != value)
                {
                    value.Attach(this);
                }

                this._animator = value;
            }
        }

        /// <summary>
        /// Creates one of a control's own animators (see
        /// <see cref="Controls.Control.CreateOwnAnimator"/>).
        /// </summary>
        internal Animator CreateOwnAnimatorCore()
        {
            if (this.HasRenderer)
            {
                throw new InvalidOperationException("CreateOwnAnimator must be called before the view is attached, from its constructor.");
            }

            var animator = new Animator();
            animator.Attach(this);

            this._ownAnimators ??= new List<Animator>();
            this._ownAnimators.Add(animator);

            return animator;
        }

        /// <summary>
        /// The view's animator, creating one for the framework's own animated
        /// views (pages, popups, scrolled content and list rows).
        /// </summary>
        internal Animator GetOrCreateAnimator()
        {
            if (null == this._animator)
            {
                var animator = new Animator();
                animator.Attach(this);
                this._animator = animator;
            }

            return this._animator;
        }

        #endregion Animation

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
        private HorizontalAlignment _horizontalAlignment = HorizontalAlignment.Stretch;
        private VerticalAlignment _verticalAlignment = VerticalAlignment.Stretch;
        private Rect _frame;
        private Size _contentMeasureSize = new Size(float.NaN, float.NaN);

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
            }
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
            this.Renderer?.InvalidateMeasure();
        }

        /// <summary>
        /// Arranges the view's children again, without changing the view's size.
        /// </summary>
        public void InvalidateArrange()
        {
            this.Renderer?.InvalidateMeasure();
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
            Rect finalRect)
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
                (int)this._horizontalAlignment,
                this._requestedSize.Width,
                this._maximumSize.Width);

            float height = ArrangeLength(
                slotHeight,
                desiredSize.Height - margin.Top - margin.Bottom,
                (int)this._verticalAlignment,
                this._requestedSize.Height,
                this._maximumSize.Height);

            float x = slotX + AlignmentOffset((int)this._horizontalAlignment, slotWidth, width);
            float y = slotY + AlignmentOffset((int)this._verticalAlignment, slotHeight, height);

            this._frame = new Rect(x, y, width, height);

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

        // The alignment enums share these values: start (left or top), center,
        // end (right or bottom) and stretch
        private const int Start = 0;
        private const int End = 2;
        private const int Stretch = 3;

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
            int alignment,
            float requestedLength,
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

            return Math.Max(0, length);
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

        #endregion Layout

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
        public bool Focusable
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
            return this.Native?.Focus() ?? false;
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
            KeyEventArgs keyInputEvent)
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
            var center = new Point(this.Bounds.Width / 2, this.Bounds.Height / 2);
            this.HandlePointerEvent(new PointerEventArgs(center, PointerInputState.Began, true));
            this.HandlePointerEvent(new PointerEventArgs(center, PointerInputState.Ended, true));
        }

        #endregion Keyboard

        public virtual bool HandlePointerEvent(
            PointerEventArgs pointerInputEvent)
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
