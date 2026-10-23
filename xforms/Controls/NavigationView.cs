using System;
using System.Collections.Generic;
using XForms.Input;
using XForms.Layouts;

namespace XForms.Controls
{
    /// <summary>
    /// The selected item of a navigation view.
    /// </summary>
    public sealed class NavigationViewSelectionChangedEventArgs : EventArgs
    {
        public NavigationViewSelectionChangedEventArgs(
            NavigationViewItem selectedItem)
        {
            this.SelectedItem = selectedItem;
        }

        public NavigationViewItem SelectedItem
        {
            get;
        }
    }

    /// <summary>
    /// A pane of navigation items beside the app's content: on a wide window
    /// a side pane that's always there, and on a narrow one a drawer that a
    /// menu button at the top opens over the content (and a tap outside it
    /// or Escape closes). Choosing an item selects it and raises
    /// <see cref="SelectionChanged"/>, for the app to show its content. Its
    /// theme's templates draw its parts.
    /// </summary>
    public class NavigationView : TemplatedControl
    {
        /// <summary>
        /// The width below which the pane becomes a drawer.
        /// </summary>
        public const float CompactThreshold = 640.0f;

        /// <summary>
        /// The pane's width.
        /// </summary>
        public const float PaneWidth = 260.0f;

        /// <summary>
        /// The height of the bar at the top on a narrow window (with the menu
        /// button and the pane's title).
        /// </summary>
        public const float BarHeight = 48.0f;

        private readonly NavigationViewPane _pane;
        private readonly NavigationViewMenuButton _menuButton;
        private readonly NavigationViewScrim _scrim;
        private readonly StackPanel _itemsPanel;
        private readonly Border _contentHost;
        private View _content;
        private string _paneTitle;
        private NavigationViewItem _selectedItem;
        private bool _isPaneOpen;
        private bool _isNarrow;

        public NavigationView()
        {
            this._itemsPanel = new StackPanel() { Spacing = 2 };
            this._pane = new NavigationViewPane(this) { Animator = new Animator(), Content = this._itemsPanel };
            // NOTE: The scrim and the button have layers of their own (an
            // animator), to be above the content's native views
            this._menuButton = new NavigationViewMenuButton(this) { Animator = new Animator() };
            this._scrim = new NavigationViewScrim(this) { Animator = new Animator(), IsVisible = false };
            this.ClipToBounds = true;

            // The children: the content, the scrim over it, the pane, and the
            // menu button. NOTE: The content is in a layer of its own (an
            // animator), so its native views stay beneath the others
            this._contentHost = new Border() { Animator = new Animator() };
            this.AddVisualChild(this._contentHost);
            this.AddVisualChild(this._scrim);
            this.AddVisualChild(this._pane);
            this.AddVisualChild(this._menuButton);

            this.MenuItems = new NavigationViewItemCollection(this);
        }

        /// <summary>
        /// Raised when the user selects an item.
        /// </summary>
        public event EventHandler<NavigationViewSelectionChangedEventArgs> SelectionChanged;

        /// <summary>
        /// The navigation items, in order.
        /// </summary>
        public NavigationViewItemCollection MenuItems
        {
            get;
        }

        /// <summary>
        /// The pane's title, shown above its items (and in the bar on a
        /// narrow window).
        /// </summary>
        public string PaneTitle
        {
            get
            {
                return this._paneTitle;
            }

            set
            {
                this._paneTitle = value;
                this.InvalidateVisual();
                this._pane.InvalidateVisual();
            }
        }

        /// <summary>
        /// The app's content, beside (or, narrow, beneath) the pane.
        /// </summary>
        public new View Content
        {
            get
            {
                return this._content;
            }

            set
            {
                this._content = value;
                this._contentHost.Child = value;
                this.InvalidateMeasure();
            }
        }

        public NavigationViewItem SelectedItem
        {
            get
            {
                return this._selectedItem;
            }

            set
            {
                if (value == this._selectedItem)
                {
                    return;
                }

                var previous = this._selectedItem;
                this._selectedItem = value;
                previous?.OnSelectionChanged();
                value?.OnSelectionChanged();
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Whether the pane is a drawer (the window is narrow).
        /// </summary>
        public bool IsPaneDrawer
        {
            get
            {
                return this._isNarrow;
            }
        }

        /// <summary>
        /// Whether the drawer is open (on a wide window, the pane is always
        /// there).
        /// </summary>
        public bool IsPaneOpen
        {
            get
            {
                return this._isPaneOpen;
            }

            set
            {
                if (value == this._isPaneOpen)
                {
                    return;
                }

                this._isPaneOpen = value;
                this.UpdatePane(animated: true);
                this._menuButton.OnPaneOpenChanged();
                this.OnPropertyChanged();

                // From the keyboard, focus goes into the drawer, and back
                if (value && this._menuButton.IsFocused)
                {
                    (this._selectedItem ?? this.FirstItem)?.Focus();
                }
                else if (!value && this._isNarrow)
                {
                    this._menuButton.Focus();
                }
            }
        }

        private NavigationViewItem FirstItem
        {
            get
            {
                foreach (var item in this.MenuItems)
                {
                    return item;
                }

                return null;
            }
        }

        // User state: the selected item's place, when it changed since it was
        // first attached
        private int _initialSelectedIndex = -1;

        protected override void OnAttaching(
            Theme theme)
        {
            base.OnAttaching(theme);
            this._initialSelectedIndex = this.MenuItems.IndexOf(this._selectedItem);
        }

        protected override object GetUserState()
        {
            int index = this.MenuItems.IndexOf(this._selectedItem);
            return (index != this._initialSelectedIndex) ? index : null;
        }

        protected override void SetUserState(
            object state)
        {
            if ((state is int index) && (index >= 0) && (index < this.MenuItems.Count))
            {
                this.SelectedItem = this.MenuItems[index];
            }
        }

        internal void OnItemAdded(
            NavigationViewItem item)
        {
            this._itemsPanel.Children.Add(item);
        }

        internal void Choose(
            NavigationViewItem item)
        {
            this.SelectedItem = item;
            if (this._isNarrow)
            {
                this.IsPaneOpen = false;
            }

            this.SelectionChanged?.Invoke(this, new NavigationViewSelectionChangedEventArgs(item));
        }

        // Up and Down move between the items, wrapping
        internal void MoveFocus(
            NavigationViewItem from,
            int direction)
        {
            int count = this.MenuItems.Count;
            int index = this.MenuItems.IndexOf(from);
            if ((count == 0) || (index < 0))
            {
                return;
            }

            this.MenuItems[(index + direction + count) % count].Focus();
        }

        protected override void OnKeyDown(
            KeyEventArgs keyInputEvent)
        {
            if ((keyInputEvent.Key == Key.Escape) && this._isNarrow && this._isPaneOpen)
            {
                this.IsPaneOpen = false;
                keyInputEvent.Handled = true;
            }
        }

        private void UpdatePane(
            bool animated)
        {
            bool isShown = !this._isNarrow || this._isPaneOpen;
            this._scrim.IsVisible = this._isNarrow && this._isPaneOpen;
            this._menuButton.IsVisible = this._isNarrow;

            // While the drawer is open, assistive technology stays in it (and
            // the button that closes it), not the content it covers
            this._contentHost.AccessibilityIsHidden = this._isNarrow && this._isPaneOpen;

            var animator = this._pane.Animator;
            float hidden = (this.FlowDirection == FlowDirection.RightToLeft) ? PaneWidth : -PaneWidth;
            if (isShown)
            {
                this._pane.IsVisible = true;
                if (animated && this._isNarrow)
                {
                    animator.Translation = new Point(hidden, 0);
                    animator.TranslateTo(Point.Zero, this.ActualTheme.Motion.Medium, this.ActualTheme.Motion.Decelerate);
                }
                else
                {
                    animator.Stop();
                    animator.Translation = Point.Zero;
                }
            }
            else
            {
                animator.Stop();
                animator.Translation = Point.Zero;
                this._pane.IsVisible = false;
            }
        }

        protected override Size MeasureOverride(
            Size availableSize)
        {
            float width = float.IsInfinity(availableSize.Width) ? 800 : availableSize.Width;
            float height = float.IsInfinity(availableSize.Height) ? 600 : availableSize.Height;

            bool isNarrow = width < CompactThreshold;
            if (isNarrow != this._isNarrow)
            {
                this._isNarrow = isNarrow;
                this._isPaneOpen = false;
                this.UpdatePane(animated: false);
                this._menuButton.OnPaneOpenChanged();
                this._pane.InvalidateMeasure();
                this._pane.InvalidateVisual();
                this.InvalidateVisual();
            }

            this._pane.Measure(new Size(PaneWidth, height));
            this._menuButton.Measure(new Size(BarHeight, BarHeight));
            this._scrim.Measure(new Size(width, height));
            this._contentHost.Measure(this.ContentBounds(width, height).Size);
            return new Size(width, height);
        }

        // Where the content goes: beside the pane, or below the bar
        private Rect ContentBounds(
            float width,
            float height)
        {
            return this._isNarrow
                ? new Rect(0, BarHeight, width, Math.Max(0, height - BarHeight))
                : new Rect(PaneWidth, 0, Math.Max(0, width - PaneWidth), height);
        }

        /// <summary>
        /// Where the bar is, on a narrow window (empty on a wide one).
        /// </summary>
        public Rect BarBounds
        {
            get
            {
                return this._isNarrow ? new Rect(0, 0, this.Bounds.Width, BarHeight) : default;
            }
        }

        // NOTE: Arranged in left-to-right terms; right to left mirrors them
        protected override void ArrangeOverride(
            Size finalSize)
        {
            this._contentHost.Arrange(this.ContentBounds(finalSize.Width, finalSize.Height));
            this._scrim.Arrange(new Rect(0, 0, finalSize.Width, finalSize.Height));
            this._pane.Arrange(new Rect(0, 0, PaneWidth, finalSize.Height));
            this._menuButton.Arrange(new Rect(0, 0, BarHeight, BarHeight));
        }
    }

    /// <summary>
    /// A navigation view's items: added to its pane as they're added here.
    /// </summary>
    public sealed class NavigationViewItemCollection : IEnumerable<NavigationViewItem>
    {
        private readonly NavigationView _owner;
        private readonly List<NavigationViewItem> _items = new List<NavigationViewItem>();

        internal NavigationViewItemCollection(
            NavigationView owner)
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

        public NavigationViewItem this[int index]
        {
            get
            {
                return this._items[index];
            }
        }

        public void Add(
            NavigationViewItem item)
        {
            if (null == item)
            {
                throw new ArgumentNullException(nameof(item));
            }

            item.NavigationView = this._owner;
            this._items.Add(item);
            this._owner.OnItemAdded(item);
        }

        public int IndexOf(
            NavigationViewItem item)
        {
            return (null == item) ? -1 : this._items.IndexOf(item);
        }

        public IEnumerator<NavigationViewItem> GetEnumerator()
        {
            return this._items.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this._items.GetEnumerator();
        }
    }

    /// <summary>
    /// An item in a navigation view's pane: an icon and text, selected when
    /// chosen (a tap, Enter or Space); Up and Down move between the items.
    /// Its theme's template draws it.
    /// </summary>
    public class NavigationViewItem : TemplatedControl, ITapGestureDelegate
    {
        public const float ItemHeight = 40.0f;

        private string _text;
        private Bitmap _icon;

        public NavigationViewItem()
        {
            this.Focusable = true;
            this.Height = ItemHeight;
            this.Margin = new Thickness(4, 0, 4, 0);
            this.GestureRecognizers.Add(new TapGestureRecognizer(this));
        }

        public string Text
        {
            get
            {
                return this._text;
            }

            set
            {
                this._text = value;
                this.InvalidateVisual();
            }
        }

        public Bitmap Icon
        {
            get
            {
                return this._icon;
            }

            set
            {
                this._icon = value;
                this.InvalidateVisual();
            }
        }

        /// <summary>
        /// Anything the app keeps with the item (such as the path it
        /// navigates to).
        /// </summary>
        public object Tag
        {
            get;
            set;
        }

        /// <summary>
        /// The navigation view it's in.
        /// </summary>
        public NavigationView NavigationView
        {
            get;
            internal set;
        }

        public bool IsSelected
        {
            get
            {
                return (null != this.NavigationView) && (this.NavigationView.SelectedItem == this);
            }
        }

        protected override AccessibilityRole DefaultAccessibilityRole
        {
            get
            {
                return AccessibilityRole.ListItem;
            }
        }

        protected override string DefaultAccessibilityLabel
        {
            get
            {
                return this._text;
            }
        }

        public override bool AccessibilityIsSelected
        {
            get
            {
                return this.IsSelected;
            }
        }

        public override bool PerformAccessibilityAction(
            AccessibilityAction action)
        {
            if (((action == AccessibilityAction.Select) || (action == AccessibilityAction.Invoke)) && (null != this.NavigationView))
            {
                this.NavigationView.Choose(this);
                return true;
            }

            return base.PerformAccessibilityAction(action);
        }

        internal void OnSelectionChanged()
        {
            this.OnStateChanged();
            this.NotifyAccessibilityStateChanged();
        }

        public TextFormat CreateTextFormat()
        {
            var format = this.ActualTheme.Typography.Body.ToTextFormat();
            format.HorizontalAlignment = (this.FlowDirection == FlowDirection.RightToLeft) ? HorizontalAlignment.Right : HorizontalAlignment.Left;
            format.VerticalAlignment = VerticalAlignment.Center;
            return format;
        }

        protected override void OnActivate()
        {
            this.NavigationView?.Choose(this);
        }

        protected override void OnKeyDown(
            KeyEventArgs keyInputEvent)
        {
            if ((keyInputEvent.Key == Key.Up) || (keyInputEvent.Key == Key.Down))
            {
                this.NavigationView?.MoveFocus(this, (keyInputEvent.Key == Key.Up) ? -1 : 1);
                keyInputEvent.Handled = true;
            }
        }

        void ITapGestureDelegate.OnTapBegan()
        {
            this.IsPressed = true;
        }

        void ITapGestureDelegate.OnTapped()
        {
            this.NavigationView?.Choose(this);
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
    /// A navigation view's pane: its title above its items. Its theme's
    /// template draws it.
    /// </summary>
    public sealed class NavigationViewPane : TemplatedControl
    {
        /// <summary>
        /// The height of the title area at the top of the pane.
        /// </summary>
        public const float TitleHeight = 56.0f;

        private readonly NavigationView _owner;
        private View _content;

        internal NavigationViewPane(
            NavigationView owner)
        {
            this._owner = owner;
            this.AccessibilityRole = AccessibilityRole.List;
        }

        public NavigationView NavigationView
        {
            get
            {
                return this._owner;
            }
        }

        protected override string DefaultAccessibilityLabel
        {
            get
            {
                return this._owner.PaneTitle;
            }
        }

        internal new View Content
        {
            get
            {
                return this._content;
            }

            set
            {
                this._content = value;
                base.Content = value;
            }
        }

        /// <summary>
        /// Where the title is: at the top, or, in a drawer, beneath the menu
        /// button's bar.
        /// </summary>
        public Rect TitleBounds
        {
            get
            {
                float top = this._owner.IsPaneDrawer ? NavigationView.BarHeight : 0;
                return new Rect(0, top, this.Bounds.Width, TitleHeight);
            }
        }

        protected override Size MeasureOverride(
            Size availableSize)
        {
            float top = (this._owner.IsPaneDrawer ? NavigationView.BarHeight : 0) + TitleHeight;
            this._content?.Measure(new Size(availableSize.Width, Math.Max(0, availableSize.Height - top)));
            return availableSize;
        }

        protected override void ArrangeOverride(
            Size finalSize)
        {
            float top = (this._owner.IsPaneDrawer ? NavigationView.BarHeight : 0) + TitleHeight;
            this._content?.Arrange(new Rect(0, top, finalSize.Width, Math.Max(0, finalSize.Height - top)));
        }
    }

    /// <summary>
    /// The button that opens and closes a navigation view's drawer. Its
    /// theme's template draws it.
    /// </summary>
    public sealed class NavigationViewMenuButton : TemplatedControl, ITapGestureDelegate
    {
        private readonly NavigationView _owner;

        internal NavigationViewMenuButton(
            NavigationView owner)
        {
            this._owner = owner;
            this.Focusable = true;
            this.IsVisible = false;
            this.GestureRecognizers.Add(new TapGestureRecognizer(this));
        }

        public NavigationView NavigationView
        {
            get
            {
                return this._owner;
            }
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
                return "Navigation";
            }
        }

        public override bool? AccessibilityIsExpanded
        {
            get
            {
                return this._owner.IsPaneOpen;
            }
        }

        public override bool PerformAccessibilityAction(
            AccessibilityAction action)
        {
            if ((action == AccessibilityAction.Expand) || (action == AccessibilityAction.Collapse))
            {
                this._owner.IsPaneOpen = (action == AccessibilityAction.Expand);
                return true;
            }

            return base.PerformAccessibilityAction(action);
        }

        internal void OnPaneOpenChanged()
        {
            this.OnStateChanged();
            this.NotifyAccessibilityStateChanged();
        }

        protected override void OnActivate()
        {
            this._owner.IsPaneOpen = !this._owner.IsPaneOpen;
        }

        void ITapGestureDelegate.OnTapBegan()
        {
            this.IsPressed = true;
        }

        void ITapGestureDelegate.OnTapped()
        {
            this._owner.IsPaneOpen = !this._owner.IsPaneOpen;
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
    /// The shade over a navigation view's content while its drawer is open:
    /// a tap on it closes the drawer. Its theme's template draws it.
    /// </summary>
    public sealed class NavigationViewScrim : TemplatedControl
    {
        private readonly NavigationView _owner;

        internal NavigationViewScrim(
            NavigationView owner)
        {
            this._owner = owner;
        }

        protected override void OnPointerPressed(
            PointerEventArgs e)
        {
            e.Handled = true;
        }

        protected override void OnPointerReleased(
            PointerEventArgs e)
        {
            this._owner.IsPaneOpen = false;
            e.Handled = true;
        }
    }
}
