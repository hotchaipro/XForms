using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using XForms.Controls;
using XForms.Input;
using XForms.Layouts;

namespace XForms.Toolkit
{
    public class SwipeCommandCollection
    {
        private List<UICommand> _primaryCommands;
        private List<UICommand> _secondaryCommands;

        public SwipeCommandCollection()
        {
            this._primaryCommands = new List<UICommand>();
            this._secondaryCommands = new List<UICommand>();
        }

        public List<UICommand> PrimaryCommands
        {
            get
            {
                return this._primaryCommands;
            }
        }

        public List<UICommand> SecondaryCommands
        {
            get
            {
                return this._secondaryCommands;
            }
        }

        public bool IsLongSwipeRequiredForPrimaryCommands
        {
            get;
            set;
        }

        public bool IsLongSwipeRequiredForSecondaryCommands
        {
            get;
            set;
        }
    }

    /// <summary>
    /// A list whose items reveal a command bar when tapped (one item at a
    /// time), and run a command when swiped sideways.
    /// </summary>
    public class CommandListView : ListView
    {
        public CommandListView()
        {
            this.CommandBarCommands = new CommandBarCommandCollection();
            this.SwipeCommands = new SwipeCommandCollection();
        }

        /// <summary>
        /// The commands shown below a tapped item.
        /// </summary>
        public CommandBarCommandCollection CommandBarCommands
        {
            get;
        }

        /// <summary>
        /// The commands run by swiping an item right (primary) or left (secondary).
        /// </summary>
        public SwipeCommandCollection SwipeCommands
        {
            get;
        }

        public override ListViewItemContainer CreateItemContainer()
        {
            return new CommandListViewItemContainer();
        }
    }

    /// <summary>
    /// An item view in a <see cref="CommandListView"/>, which can show or hide
    /// its command bar.
    /// </summary>
    public class CommandListViewItem : ListViewItem
    {
        private CommandListViewItemContainer CommandContainer
        {
            get
            {
                return this.Container as CommandListViewItemContainer;
            }
        }

        public bool IsCommandBarVisible
        {
            get
            {
                return this.CommandContainer?.IsCommandBarVisible ?? false;
            }

            set
            {
                var container = this.CommandContainer;
                if (null != container)
                {
                    container.IsCommandBarVisible = value;
                }
            }
        }

        public Thickness CommandBarMargin
        {
            get
            {
                return this.CommandContainer?.CommandBarMargin ?? Thickness.Zero;
            }

            set
            {
                var container = this.CommandContainer;
                if (null != container)
                {
                    container.CommandBarMargin = value;
                }
            }
        }
    }

    /// <summary>
    /// The item container of a <see cref="CommandListView"/>: reveals the
    /// list's command bar below the item when tapped, and runs a swipe command
    /// when the item is swiped sideways.
    /// </summary>
    public class CommandListViewItemContainer : ListViewItemContainer, ISwipeGestureDelegate
    {
        private ListViewSwipeItem _swipeItem;
        private CommandBar _commandBar;
        private CommandBarCommandCollection _commandBarCommands;
        private DockLayout _contentLayout;

        public CommandListViewItemContainer()
        {
            this.VerticalAlignment = LayoutAlignment.Fill;

            this._swipeItem = new ListViewSwipeItem()
            {
                HorizontalAlignment = LayoutAlignment.Fill,
                VerticalAlignment = LayoutAlignment.Fill,
                IconSize = new Size(30, 30),
            };
            this.Children.Add(this._swipeItem, DockRegion.CenterOverlay);

            this._contentLayout = new DockLayout()
            {
                VerticalAlignment = LayoutAlignment.Fill,
                HorizontalAlignment = LayoutAlignment.Fill,
            };
            this.Children.Add(this._contentLayout, DockRegion.CenterOverlay);

            this._commandBar = new CommandBar()
            {
                IsVisible = false,
            };
            this._contentLayout.Children.Add(this._commandBar, DockRegion.Bottom);

            this.GestureRecognizer = new SwipeGestureRecognizer(this);
        }

        private CommandListView CommandListView
        {
            get
            {
                return this.ListView as CommandListView;
            }
        }

        protected override void AddContent(
            ListViewItem content)
        {
            this._contentLayout.Children.Add(content, DockRegion.CenterOverlay);
        }

        protected override void OnBind()
        {
            // Reset command bar
            this.IsCommandBarVisible = false;
            var commands = this.CommandListView?.CommandBarCommands;
            if ((null != commands) && (this._commandBarCommands != commands))
            {
                this._commandBar.SetCommands(commands);
                this._commandBarCommands = commands;
            }
            this._commandBar.BindingContext = this.Item;

            // Reset swipe defaults
            this._swipeItem.StopAnimation();
            this._swipeItem.Translation = Point.Zero;
            this._contentLayout.StopAnimation();
            this._contentLayout.Translation = Point.Zero;
        }

        protected override void OnRecycle()
        {
            // Reset command bar
            this.IsCommandBarVisible = false;
            this._commandBar.BindingContext = null;

            // Reset swipe commands
            this._swipeItem.StopAnimation();
            this._swipeItem.Translation = Point.Zero;
            this._swipeItem.Reset();
            this._contentLayout.StopAnimation();
            this._contentLayout.Translation = Point.Zero;
        }

        public bool IsCommandBarVisible
        {
            get
            {
                var commandBar = this._commandBar;
                if (null == commandBar)
                {
                    return false;
                }

                return commandBar.IsVisible;
            }

            set
            {
                var commandBar = this._commandBar;
                if (null == commandBar)
                {
                    return;
                }

                if (commandBar.IsVisible != value)
                {
                    commandBar.IsVisible = value;
                }
            }
        }

        public Thickness CommandBarMargin
        {
            get
            {
                var commandBar = this._commandBar;
                if (null == commandBar)
                {
                    return Thickness.Zero;
                }

                return commandBar.ButtonsLayout.Margin;
            }

            set
            {
                var commandBar = this._commandBar;
                if (null == commandBar)
                {
                    return;
                }

                commandBar.ButtonsLayout.Margin = value;
            }
        }

        private void ToggleCommandBar()
        {
            var commandBar = this._commandBar;
            if (null == commandBar)
            {
                return;
            }

            this.IsCommandBarVisible = !commandBar.IsVisible;

            if (commandBar.IsVisible)
            {
                this.ListView.ScrollToItem(this.Item);
            }
        }

        #region Swipe input

        private float _swipeFencePosition;
        private bool _swipeEnabled;
        private bool _longSwipe;
        private UICommand _swipeCommand;

        bool ISwipeGestureDelegate.OnSwipeBegan(
            int direction)
        {
            this._swipeFencePosition = 0;
            this._swipeEnabled = false;
            this._longSwipe = false;

            // HACK: Prevent flicker
            this._swipeItem.Translation = new XForms.Point(-1000, 0);

            this._contentLayout.Translation = Point.Zero;
            UpdateSwipeCommand(direction);

            return true;
        }

        bool ISwipeGestureDelegate.OnSwipeMoved(
            int direction,
            float dx)
        {
            const float EnabledMargin = 48.0f;
            const float LongSwipeDistancePercentage = 0.7f;

            if (null == this.Content)
            {
                return false;
            }

            // Update fence position
            if (dx > this._swipeFencePosition + Application.TouchSlop)
            {
                this._swipeEnabled = true;
                this._swipeFencePosition = dx;
            }
            else if (dx < this._swipeFencePosition - Application.TouchSlop)
            {
                this._swipeEnabled = false;
                this._swipeFencePosition = dx;
            }

            // Enforce margin
            if (dx < EnabledMargin)
            {
                this._swipeEnabled = false;
            }

            // Enforce command state
            bool commandEnabled = true;
            if ((null == this._swipeCommand) || (!this._swipeCommand.CanExecute(this.Item)))
            {
                commandEnabled = false;
                this._swipeEnabled = false;
            }

            if ((!commandEnabled) && (dx > EnabledMargin))
            {
                dx = EnabledMargin;
            }

            // Limit position
            if (dx > this.Size.Width)
            {
                dx = this.Size.Width;
            }
            else if (dx < 0)
            {
                dx = 0;
            }

            // Check for long swipe
            if ((!this._longSwipe) && (dx >= this.Size.Width * LongSwipeDistancePercentage))
            {
                // Switch to long swipe
                this._longSwipe = true;
                UpdateSwipeCommand(direction);
            }
            else if ((this._longSwipe) && (dx < (this.Size.Width * LongSwipeDistancePercentage) - Application.TouchSlop))
            {
                // Reset back to short swipe
                this._longSwipe = false;
                UpdateSwipeCommand(direction);
            }

            // Enforce long swipe requirement
            bool isLongSwipeRequired;
            if (direction > 0)
            {
                isLongSwipeRequired = this.CommandListView?.SwipeCommands.IsLongSwipeRequiredForPrimaryCommands ?? false;
            }
            else
            {
                isLongSwipeRequired = this.CommandListView?.SwipeCommands.IsLongSwipeRequiredForSecondaryCommands ?? false;
            }
            if ((isLongSwipeRequired) && (!this._longSwipe))
            {
                this._swipeEnabled = false;
            }

            float position = dx * direction;

            this._contentLayout.Translation = new Point(position, 0);
            this._swipeItem.Translation = new Point(position - (direction * this.Size.Width), 0);
            float dimmedOpacity = (Application.Current.Theme == AppTheme.Light ? 0.1f : 0.25f);
            this._swipeItem.Opacity = (this._swipeEnabled ? 1.0f : dimmedOpacity);

            return true;
        }

        bool ISwipeGestureDelegate.OnSwipeEnded(
            int direction,
            float dx)
        {
            if (this._swipeEnabled)
            {
                CompleteSwipe(direction);
            }
            else
            {
                CancelSwipe(direction);
            }

            return true;
        }

        bool ISwipeGestureDelegate.OnSwipeCanceled(
            int direction)
        {
            CancelSwipe(direction);

            return true;
        }

        private void UpdateSwipeCommand(
            int direction)
        {
            IReadOnlyList<UICommand> commands;
            if (direction > 0)
            {
                commands = this.CommandListView?.SwipeCommands.PrimaryCommands;
            }
            else
            {
                commands = this.CommandListView?.SwipeCommands.SecondaryCommands;
            }

            UICommand command = null;

            if (null != commands)
            {
                if ((this._longSwipe) && (commands.Count > 1))
                {
                    command = commands[1];
                }
                else if (commands.Count > 0)
                {
                    command = commands[0];
                }
            }

            if (direction > 0)
            {
                this._swipeItem.Direction = ListViewSwipeDirection.Primary;
            }
            else
            {
                this._swipeItem.Direction = ListViewSwipeDirection.Secondary;
            }

            this._swipeItem.Command = command;
            this._swipeCommand = command;
        }

        private void CompleteSwipe(
            int direction)
        {
            if (null == this.Content)
            {
                return;
            }

            this._contentLayout.TranslateTo(new Point(direction * this.Size.Width, 0), TimeSpan.FromMilliseconds(100), new CubicEase(EasingMode.EaseInOut));
            this._swipeItem.TranslateTo(new Point(0, 0), TimeSpan.FromMilliseconds(100), new CubicEase(EasingMode.EaseInOut)).ContinueWith((ignore) =>
            {
                Application.InvokeOnUIThreadAsync(() =>
                {
                    this._swipeCommand?.Execute(this.Item);

                    this._contentLayout.Translation = Point.Zero;
                    this._swipeItem.Translation = new Point(-1000, 0);
                });
            });
        }

        private void CancelSwipe(
            int direction)
        {
            if (null == this.Content)
            {
                return;
            }

            this._contentLayout.TranslateTo(Point.Zero, TimeSpan.FromMilliseconds(100), new CubicEase(EasingMode.EaseInOut));
            this._swipeItem.TranslateTo(
                new Point(-direction * this.Size.Width, 0), TimeSpan.FromMilliseconds(100), new CubicEase(EasingMode.EaseInOut)).ContinueWith(
                (ignore) => { this._swipeItem.Translation = new XForms.Point(-1000, 0); }, TaskContinuationOptions.ExecuteSynchronously);
        }

        #endregion Swipe input

        #region Tap input

        void ITapGestureDelegate.OnTapBegan()
        {
        }

        void ITapGestureDelegate.OnTapEnded()
        {
        }

        void ITapGestureDelegate.OnTapped()
        {
            this.OnTapped();
        }

        protected override void OnTapped()
        {
            var listView = this.ListView;
            if (null == listView)
            {
                return;
            }

            // Hide the previously selected command bar
            var previousContainer = listView.SelectedItemContainer;
            if ((null != previousContainer) && (previousContainer != this) && (previousContainer is CommandListViewItemContainer previous))
            {
                previous.IsCommandBarVisible = false;
            }

            this.ToggleCommandBar();

            bool isSelected = this._commandBar.IsVisible;

            // Update the selected item container
            if (previousContainer != this)
            {
                listView.SelectedItemContainer = this;
            }

            listView.NotifyItemClicked(this.Item, this.Content, isSelected);
        }

        #endregion Tap input

        #region Touch input

        void IGestureRecognizerDelegate.OnTouchBegan()
        {
        }

        void IGestureRecognizerDelegate.OnTouchEnded()
        {
        }

        void IGestureRecognizerDelegate.OnTouchCanceled()
        {
        }

        #endregion Touch input
    }
}
