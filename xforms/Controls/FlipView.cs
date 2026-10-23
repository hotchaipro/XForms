using System;
using System.Collections.Generic;
using XForms.Input;

namespace XForms.Controls
{
    /// <summary>
    /// Views shown one at a time, side by side: a horizontal swipe moves to
    /// the next or previous (following the finger, then settling), as do its
    /// arrow buttons and, with keyboard focus, Left and Right (following the
    /// flow direction), Page Up, Page Down, Home and End. Its theme's
    /// templates draw its arrow buttons.
    /// </summary>
    public class FlipView : TemplatedControl, IPanGestureDelegate
    {
        public static readonly ViewProperty<FlipView, int> SelectedIndexProperty = new(nameof(SelectedIndex), v => v.SelectedIndex, (v, x) => v.SelectedIndex = x);

        private const float ButtonWidth = 32.0f;

        private readonly FlipViewStrip _strip;
        private readonly FlipViewButton _previousButton;
        private readonly FlipViewButton _nextButton;
        private int _selectedIndex;
        private bool _isPanning;

        public FlipView()
        {
            this.Focusable = true;
            this.ClipToBounds = true;
            this._strip = new FlipViewStrip(this) { Animator = new Animator() };
            // NOTE: The buttons have layers of their own (an animator), to be
            // above the strip's
            this._previousButton = new FlipViewButton(this, -1) { Animator = new Animator() };
            this._nextButton = new FlipViewButton(this, 1) { Animator = new Animator() };
            this.AddVisualChild(this._strip);
            this.AddVisualChild(this._previousButton);
            this.AddVisualChild(this._nextButton);
            this.GestureRecognizers.Add(new PanGestureRecognizer(this) { Directions = PanDirections.Horizontal });
            this.Items = new FlipViewItemCollection(this);
        }

        /// <summary>
        /// Raised when the shown view changes.
        /// </summary>
        public event EventHandler SelectionChanged;

        /// <summary>
        /// The views, in order.
        /// </summary>
        public FlipViewItemCollection Items
        {
            get;
        }

        /// <summary>
        /// The view shown (its place in <see cref="Items"/>).
        /// </summary>
        public int SelectedIndex
        {
            get
            {
                return this._selectedIndex;
            }

            set
            {
                this.Select(value, animated: false);
            }
        }

        protected override AccessibilityRole DefaultAccessibilityRole
        {
            get
            {
                return AccessibilityRole.Group;
            }
        }

        // Read as "2 of 4"
        public override string AccessibilityValue
        {
            get
            {
                return (this.Items.Count == 0) ? null : $"{this._selectedIndex + 1} of {this.Items.Count}";
            }
        }

        // User state: the view shown, when that changed since it was first
        // attached
        private int _initialSelectedIndex;

        protected override void OnAttaching(
            Theme theme)
        {
            base.OnAttaching(theme);
            this._initialSelectedIndex = this.SelectedIndex;
        }

        protected override object GetUserState()
        {
            return (this.SelectedIndex != this._initialSelectedIndex) ? this.SelectedIndex : null;
        }

        protected override void SetUserState(
            object state)
        {
            if (state is int index)
            {
                this.SelectedIndex = index;
            }
        }

        internal void OnItemAdded(
            View item)
        {
            this._strip.Add(item);
            this.UpdateButtons();
        }

        private bool IsRightToLeft
        {
            get
            {
                return this.FlowDirection == FlowDirection.RightToLeft;
            }
        }

        /// <summary>
        /// Shows the view at the index (kept within the items), sliding to it
        /// when animated.
        /// </summary>
        public void Select(
            int index,
            bool animated)
        {
            int count = this.Items.Count;
            index = (count == 0) ? 0 : Math.Clamp(index, 0, count - 1);
            bool changed = index != this._selectedIndex;
            this._selectedIndex = index;
            this.Settle(animated);
            this.UpdateButtons();

            if (changed)
            {
                this.OnPropertyChanged(nameof(this.SelectedIndex));
                this.NotifyAccessibilityStateChanged();
                this.SelectionChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        // The strip's offset for the selected view (views run leftward, right
        // to left)
        private float OffsetFor(
            int index)
        {
            float width = this.Bounds.Width;
            return this.IsRightToLeft ? index * width : -index * width;
        }

        private void Settle(
            bool animated)
        {
            var animator = this._strip.Animator;
            var target = new Point(this.OffsetFor(this._selectedIndex), 0);
            if (animated)
            {
                animator.TranslateTo(target, this.ActualTheme.Motion.Medium, this.ActualTheme.Motion.Decelerate);
            }
            else
            {
                animator.Stop();
                animator.Translation = target;
            }
        }

        private void UpdateButtons()
        {
            this._previousButton.IsVisible = this._selectedIndex > 0;
            this._nextButton.IsVisible = this._selectedIndex < (this.Items.Count - 1);
        }

        protected override void OnKeyDown(
            KeyEventArgs keyInputEvent)
        {
            int forward = this.IsRightToLeft ? -1 : 1;
            switch (keyInputEvent.Key)
            {
                case Key.Left:
                    this.Select(this._selectedIndex - forward, animated: true);
                    break;

                case Key.Right:
                    this.Select(this._selectedIndex + forward, animated: true);
                    break;

                case Key.PageUp:
                    this.Select(this._selectedIndex - 1, animated: true);
                    break;

                case Key.PageDown:
                    this.Select(this._selectedIndex + 1, animated: true);
                    break;

                case Key.Home:
                    this.Select(0, animated: true);
                    break;

                case Key.End:
                    this.Select(this.Items.Count - 1, animated: true);
                    break;

                default:
                    return;
            }

            keyInputEvent.Handled = true;
        }

        protected override Size MeasureOverride(
            Size availableSize)
        {
            float width = float.IsInfinity(availableSize.Width) ? 320 : availableSize.Width;
            float height = float.IsInfinity(availableSize.Height) ? 240 : availableSize.Height;
            this._strip.Measure(new Size(width, height));
            this._previousButton.Measure(new Size(ButtonWidth, height));
            this._nextButton.Measure(new Size(ButtonWidth, height));
            return new Size(width, height);
        }

        // NOTE: Arranged in left-to-right terms; right to left mirrors them
        protected override void ArrangeOverride(
            Size finalSize)
        {
            this._strip.ItemWidth = finalSize.Width;
            this._strip.Arrange(new Rect(0, 0, finalSize.Width * Math.Max(1, this.Items.Count), finalSize.Height));
            float buttonHeight = Math.Min(64, finalSize.Height);
            float y = (finalSize.Height - buttonHeight) / 2;
            this._previousButton.Arrange(new Rect(0, y, ButtonWidth, buttonHeight));
            this._nextButton.Arrange(new Rect(finalSize.Width - ButtonWidth, y, ButtonWidth, buttonHeight));
            if (!this._isPanning)
            {
                this.Settle(animated: false);
            }
        }

        #region IPanGestureDelegate implementation

        private float _panStart;

        void IPanGestureDelegate.OnPanStarted()
        {
            this._isPanning = true;
            this._panStart = this._strip.Animator.Translation.X;
            this._strip.Animator.Stop();
        }

        void IPanGestureDelegate.OnPanChanged(
            Point translation)
        {
            // Following the finger, held back past the ends
            float width = this.Bounds.Width;
            float x = this._panStart + translation.X;
            float min = Math.Min(this.OffsetFor(0), this.OffsetFor(this.Items.Count - 1));
            float max = Math.Max(this.OffsetFor(0), this.OffsetFor(this.Items.Count - 1));
            if (x > max)
            {
                x = max + ((x - max) / 3);
            }
            else if (x < min)
            {
                x = min + ((x - min) / 3);
            }

            this._strip.Animator.Translation = new Point(x, 0);
        }

        void IPanGestureDelegate.OnPanCompleted(
            Point translation)
        {
            this._isPanning = false;

            // A fifth of the width moves to the neighbor
            float width = this.Bounds.Width;
            int step = 0;
            if (Math.Abs(translation.X) > width / 5)
            {
                step = (translation.X < 0) ? 1 : -1;
                if (this.IsRightToLeft)
                {
                    step = -step;
                }
            }

            this.Select(this._selectedIndex + step, animated: true);
            this.Settle(animated: true);
        }

        void IPanGestureDelegate.OnPanCanceled()
        {
            this._isPanning = false;
            this.Settle(animated: true);
        }

        void IGestureRecognizerDelegate.OnTouchBegan()
        {
        }

        void IGestureRecognizerDelegate.OnTouchEnded()
        {
        }

        void IGestureRecognizerDelegate.OnTouchCanceled()
        {
        }

        #endregion

        internal void Step(
            int direction)
        {
            this.Select(this._selectedIndex + direction, animated: true);
        }
    }

    /// <summary>
    /// A flip view's views: added to it as they're added here.
    /// </summary>
    public sealed class FlipViewItemCollection : IEnumerable<View>
    {
        private readonly FlipView _owner;
        private readonly List<View> _items = new List<View>();

        internal FlipViewItemCollection(
            FlipView owner)
        {
            this._owner = owner;
        }

        public int Count
        {
            get
            {
                return this._items.Count;
            }
        }

        public View this[int index]
        {
            get
            {
                return this._items[index];
            }
        }

        public void Add(
            View item)
        {
            if (null == item)
            {
                throw new ArgumentNullException(nameof(item));
            }

            this._items.Add(item);
            this._owner.OnItemAdded(item);
        }

        public IEnumerator<View> GetEnumerator()
        {
            return this._items.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this._items.GetEnumerator();
        }
    }

    // The views side by side, each a flip view wide, moved by its animator
    internal sealed class FlipViewStrip : View
    {
        private readonly FlipView _owner;

        public FlipViewStrip(
            FlipView owner)
        {
            this._owner = owner;
        }

        public float ItemWidth
        {
            get;
            set;
        }

        public void Add(
            View item)
        {
            this.AddVisualChild(item);
            this.InvalidateMeasure();
        }

        protected override Size MeasureOverride(
            Size availableSize)
        {
            foreach (var child in this.VisualChildren)
            {
                child.Measure(availableSize);
            }

            return availableSize;
        }

        protected override void ArrangeOverride(
            Size finalSize)
        {
            float width = this.ItemWidth;
            int i = 0;
            foreach (var child in this.VisualChildren)
            {
                child.Arrange(new Rect(i * width, 0, width, finalSize.Height));
                i += 1;
            }
        }
    }

    /// <summary>
    /// A flip view's arrow button, to its previous or next view. Its theme's
    /// template draws it.
    /// </summary>
    public sealed class FlipViewButton : TemplatedControl, ITapGestureDelegate
    {
        private readonly FlipView _owner;

        internal FlipViewButton(
            FlipView owner,
            int direction)
        {
            this._owner = owner;
            this.Direction = direction;
            this.GestureRecognizers.Add(new TapGestureRecognizer(this));
        }

        /// <summary>
        /// -1 for the previous view, 1 for the next.
        /// </summary>
        public int Direction
        {
            get;
        }

        protected override AccessibilityRole DefaultAccessibilityRole
        {
            get
            {
                return AccessibilityRole.Button;
            }
        }

        protected override string DefaultAccessibilityLabel
        {
            get
            {
                return (this.Direction < 0) ? "Previous" : "Next";
            }
        }

        public override bool PerformAccessibilityAction(
            AccessibilityAction action)
        {
            if (action == AccessibilityAction.Invoke)
            {
                this._owner.Step(this.Direction);
                return true;
            }

            return base.PerformAccessibilityAction(action);
        }

        public override bool IsAccessibilityActionSupported(
            AccessibilityAction action)
        {
            return (action == AccessibilityAction.Invoke) || base.IsAccessibilityActionSupported(action);
        }

        void ITapGestureDelegate.OnTapBegan()
        {
            this.IsPressed = true;
        }

        void ITapGestureDelegate.OnTapped()
        {
            this._owner.Step(this.Direction);
        }

        void ITapGestureDelegate.OnTapEnded()
        {
            this.IsPressed = false;
        }

        void IGestureRecognizerDelegate.OnTouchBegan()
        {
        }

        void IGestureRecognizerDelegate.OnTouchEnded()
        {
        }

        void IGestureRecognizerDelegate.OnTouchCanceled()
        {
        }
    }

    /// <summary>
    /// Dots, one for each page, showing which is current (as beneath a flip
    /// view): a tap on a dot selects its page; with keyboard focus, Left and
    /// Right (following the flow direction), Home and End move it. Its
    /// theme's template draws it, in the layout it measures: a dot each
    /// <see cref="DotSpacing"/> apart, centered.
    /// </summary>
    public class PipsPager : TemplatedControl
    {
        public static readonly ViewProperty<PipsPager, int> SelectedPageIndexProperty = new(nameof(SelectedPageIndex), v => v.SelectedPageIndex, (v, x) => v.SelectedPageIndex = x);

        public const float DotSpacing = 20.0f;

        private int _numberOfPages;
        private int _selectedPageIndex;

        public PipsPager()
        {
            this.Focusable = true;
            this.Height = 28;
        }

        public event EventHandler SelectedIndexChanged;

        public int NumberOfPages
        {
            get
            {
                return this._numberOfPages;
            }

            set
            {
                this._numberOfPages = Math.Max(0, value);
                this.InvalidateMeasure();
                this.InvalidateVisual();
            }
        }

        public int SelectedPageIndex
        {
            get
            {
                return this._selectedPageIndex;
            }

            set
            {
                int index = (this._numberOfPages == 0) ? 0 : Math.Clamp(value, 0, this._numberOfPages - 1);
                if (index == this._selectedPageIndex)
                {
                    return;
                }

                this._selectedPageIndex = index;
                this.InvalidateVisual();
                this.OnPropertyChanged();
                this.NotifyAccessibilityStateChanged();
                this.SelectedIndexChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        protected override AccessibilityRole DefaultAccessibilityRole
        {
            get
            {
                return AccessibilityRole.Slider;
            }
        }

        protected override string DefaultAccessibilityLabel
        {
            get
            {
                return "Page";
            }
        }

        // The page as a range, from 1
        public override AccessibilityRange? AccessibilityRange
        {
            get
            {
                return (this._numberOfPages == 0) ? null : new AccessibilityRange(1, this._numberOfPages, this._selectedPageIndex + 1);
            }
        }

        public override string AccessibilityValue
        {
            get
            {
                return (this._numberOfPages == 0) ? null : $"{this._selectedPageIndex + 1} of {this._numberOfPages}";
            }
        }

        public override bool IsAccessibilityActionSupported(
            AccessibilityAction action)
        {
            if ((action == AccessibilityAction.Increment) || (action == AccessibilityAction.Decrement))
            {
                return true;
            }

            return (action != AccessibilityAction.Invoke) && base.IsAccessibilityActionSupported(action);
        }

        public override bool PerformAccessibilityAction(
            AccessibilityAction action)
        {
            if (action == AccessibilityAction.Increment)
            {
                this.SelectedPageIndex += 1;
                return true;
            }

            if (action == AccessibilityAction.Decrement)
            {
                this.SelectedPageIndex -= 1;
                return true;
            }

            return base.PerformAccessibilityAction(action);
        }

        public override bool SetAccessibilityRangeValue(
            double value)
        {
            this.SelectedPageIndex = (int)Math.Round(value) - 1;
            return true;
        }

        private bool IsRightToLeft
        {
            get
            {
                return this.FlowDirection == FlowDirection.RightToLeft;
            }
        }

        /// <summary>
        /// The center of a page's dot.
        /// </summary>
        public Point GetDotCenter(
            int index)
        {
            float total = this._numberOfPages * DotSpacing;
            float start = (this.Bounds.Width - total) / 2;
            int place = this.IsRightToLeft ? this._numberOfPages - 1 - index : index;
            return new Point(start + (place * DotSpacing) + (DotSpacing / 2), this.Bounds.Height / 2);
        }

        protected override Size MeasureOverride(
            Size availableSize)
        {
            return new Size(Math.Min(this._numberOfPages * DotSpacing, availableSize.Width), Math.Min(28, availableSize.Height));
        }

        protected override void OnPointerPressed(
            PointerEventArgs e)
        {
            e.Handled = true;
        }

        protected override void OnPointerReleased(
            PointerEventArgs e)
        {
            for (int i = 0; i < this._numberOfPages; i += 1)
            {
                if (Math.Abs(this.GetDotCenter(i).X - e.Position.X) <= DotSpacing / 2)
                {
                    this.SelectedPageIndex = i;
                    break;
                }
            }

            e.Handled = true;
        }

        protected override void OnKeyDown(
            KeyEventArgs keyInputEvent)
        {
            int forward = this.IsRightToLeft ? -1 : 1;
            switch (keyInputEvent.Key)
            {
                case Key.Left:
                    this.SelectedPageIndex -= forward;
                    break;

                case Key.Right:
                    this.SelectedPageIndex += forward;
                    break;

                case Key.Home:
                    this.SelectedPageIndex = 0;
                    break;

                case Key.End:
                    this.SelectedPageIndex = this._numberOfPages - 1;
                    break;

                default:
                    return;
            }

            keyInputEvent.Handled = true;
        }
    }
}
