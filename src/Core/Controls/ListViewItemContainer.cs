using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using XForms.Input;
using XForms.Layouts;

namespace XForms.Controls
{
    // CONSIDER: A custom list view item container layout to protect the public child collection
    public sealed class ListViewItemContainer : DockLayout, ISwipeGestureDelegate
    {
        private bool _isContentSet;
        private ListViewSwipeItem _swipeItem;
        private CommandBar _commandBar;
        private CommandBarCommandCollection _commandBarCommands;
        private DockLayout _contentLayout;

        internal ListViewItemContainer()
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

        internal ListView ListView
        {
            get;
            private set;
        }

        internal object Item
        {
            get;
            private set;
        }

        internal ListViewItem Content
        {
            get
            {
                return (this._isContentSet) ? (ListViewItem)this._contentLayout.Children[this._contentLayout.Children.Count - 1] : null;
            }

            set
            {
                if (this._isContentSet)
                {
                    if (this._contentLayout.Children[this._contentLayout.Children.Count - 1] != value)
                    {
                        throw new NotImplementedException("ListView content must be recycled.");
                        // TODO: this.Children.Replace(this.Children.Count - 1, value, DockRegion.CenterOverlay);
                    }
                }
                else
                {
                    this._contentLayout.Children.Add(value, DockRegion.CenterOverlay);

                    this._isContentSet = true;
                }

                value.Container = this;
            }
        }

        internal void Bind(
            ListView listView,
            object item)
        {
            if (null == listView)
            {
                throw new ArgumentNullException(nameof(listView));
            }

            this.ListView = listView;
            this.Item = item;

            // Reset command bar
            this.IsCommandBarVisible = false;
            if (this._commandBarCommands != this.ListView.CommandBarCommands)
            {
                this._commandBar.SetCommands(this.ListView.CommandBarCommands);
                this._commandBarCommands = this.ListView.CommandBarCommands;
            }
            this._commandBar.BindingContext = item;

            // Reset swipe defaults
            this._swipeItem.StopAnimation();
            this._swipeItem.Translation = Point.Zero;
            this._contentLayout.StopAnimation();
            this._contentLayout.Translation = Point.Zero;
        }

        public void Recycle()
        {
            // Unset binding context
            var content = this.Content;
            if (null != content)
            {
                content.BindingContext = null;
                content.Container = null;
            }

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

        internal bool IsCommandBarVisible
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

        internal Thickness CommandBarMargin
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
                isLongSwipeRequired = this.ListView?.SwipeCommands.IsLongSwipeRequiredForPrimaryCommands ?? false;
            }
            else
            {
                isLongSwipeRequired = this.ListView?.SwipeCommands.IsLongSwipeRequiredForSecondaryCommands ?? false;
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
                commands = this.ListView?.SwipeCommands.PrimaryCommands;
            }
            else
            {
                commands = this.ListView?.SwipeCommands.SecondaryCommands;
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
            var listView = this.ListView;
            if (null == listView)
            {
                return;
            }

            // Hide the previously selected command bar
            var previousContainer = listView.SelectedItemContainer;
            if ((null != previousContainer) && (previousContainer != this))
            {
                previousContainer.IsCommandBarVisible = false;
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
