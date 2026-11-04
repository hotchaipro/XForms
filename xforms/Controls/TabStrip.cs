using System;
using System.Collections.Generic;
using XForms.Input;
using XForms.Layouts;
using XForms.Controls;
using XForms.Primitives;

namespace XForms.Controls
{
    public class TabSelectionChangedEventArgs : EventArgs
    {
        public TabSelectionChangedEventArgs(
            Page selection,
            Page previousSelection,
            NavigationMode navigationMode)
        {
            this.Selection = selection;
            this.PreviousSelection = previousSelection;
            this.NavigationMode = navigationMode;
        }

        public Page Selection
        {
            get;
            private set;
        }

        public Page PreviousSelection
        {
            get;
            private set;
        }

        public NavigationMode NavigationMode
        {
            get;
            private set;
        }
    }

    /// <summary>
    /// A row of tabs, one checked: the page it shows. Give it the
    /// <see cref="Frame"/> its pages show in, and it switches them as the
    /// selection changes, sliding from the tab's side; or handle
    /// <see cref="SelectionChanged"/>. A switch can also follow a gesture
    /// (see <see cref="FollowSwipe"/>).
    /// </summary>
    public class TabStrip : TemplatedContentControl
    {
        // How far a swipe has to take the switch (or how fast, in points a
        // second) for it to complete when released
        private const float SwipeCompleteProgress = 1 / 3.0f;
        private const float SwipeFlickVelocity = 300;

        private static readonly TimeSpan DefaultTransitionDuration = TimeSpan.FromMilliseconds(250);

        private UniformGrid _buttonsLayout;
        private TabStripItem _selection;
        private Frame _frame;
        private float _selectionPosition;
        private TabSwitch _switch;
        private TabSwitch _swipeSwitch;
        private bool _isUpdatingChecks;

        public event EventHandler<TabSelectionChangedEventArgs> SelectionChanged;

        public TabStrip()
        {
            this._buttonsLayout = new UniformGrid()
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Center,
                MaxWidth = 600,
            };

            this.HorizontalAlignment = HorizontalAlignment.Stretch;
            this.VerticalAlignment = VerticalAlignment.Top;

            this.Content = this._buttonsLayout;
        }

        public Page SelectedPage
        {
            get
            {
                return this._selection?.TabPage;
            }
        }

        /// <summary>
        /// The checked tab's index, or -1.
        /// </summary>
        public int SelectedIndex
        {
            get
            {
                return this.IndexOf(this._selection);
            }
        }

        /// <summary>
        /// The tabs, in order.
        /// </summary>
        public IReadOnlyList<TabStripItem> Items
        {
            get
            {
                var items = new List<TabStripItem>();
                foreach (var child in this._buttonsLayout.Children)
                {
                    if (child is TabStripItem item)
                    {
                        items.Add(item);
                    }
                }

                return items;
            }
        }

        /// <summary>
        /// Where the selection shows, in tabs: the checked tab's index, or
        /// between it and the next while a switch follows a gesture. A theme
        /// draws its selection indicator here.
        /// </summary>
        public float SelectionPosition
        {
            get
            {
                return this._selectionPosition;
            }
        }

        /// <summary>
        /// Whether a switch is following a gesture (see <see cref="BeginSwitch"/>):
        /// the selection indicator follows <see cref="SelectionPosition"/>
        /// directly, rather than sliding to it.
        /// </summary>
        public bool IsSwitching
        {
            get
            {
                return null != this._switch;
            }
        }

        /// <summary>
        /// The frame the tabs' pages show in, or null (the default) to switch
        /// them yourself, with <see cref="SelectionChanged"/>. As the selection
        /// changes, the strip replaces the frame's page, sliding in from the
        /// side of the tab.
        /// </summary>
        public Frame Frame
        {
            get
            {
                return this._frame;
            }

            set
            {
                this._frame = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// How long the selection takes to move to another tab: its frame's
        /// <see cref="XForms.Frame.TransitionDuration"/>, so the indicator
        /// slides with the page.
        /// </summary>
        public TimeSpan TransitionDuration
        {
            get
            {
                return this._frame?.TransitionDuration ?? DefaultTransitionDuration;
            }
        }

        public void AddButton(
            TabStripItem tabButton,
            Page tabPage)
        {
            if (null == tabButton)
            {
                throw new ArgumentNullException(nameof(tabButton));
            }

            if (null == tabPage)
            {
                throw new ArgumentNullException(nameof(tabPage));
            }

            tabButton.TabPage = tabPage;
            tabButton.IsChecked = false;
            tabButton.IsCheckedChanged += TabStripItem_IsCheckedChanged;
            tabButton.HorizontalAlignment = HorizontalAlignment.Center;
            this._buttonsLayout.Children.Add(tabButton);
        }

        public void SelectRelative(
            int direction)
        {
            if ((direction == 0) || (null == this._selection))
            {
                return;
            }

            var item = this.ItemAt(this.SelectedIndex + Math.Sign(direction));
            if (null != item)
            {
                item.IsChecked = true;
            }
        }

        /// <summary>
        /// Starts switching to the next tab (a positive direction) or the
        /// previous one (a negative direction), following a gesture: set the
        /// switch's <see cref="TabSwitch.Progress"/> as it moves, then complete
        /// or cancel it. The strip's frame slides the tab's page in as far.
        /// Returns null if there's no tab that way, a switch is under way, or
        /// the current page cancels.
        /// </summary>
        public TabSwitch BeginSwitch(
            int direction)
        {
            if ((0 == direction) || (null != this._switch) || (null == this._selection))
            {
                return null;
            }

            int from = this.SelectedIndex;
            int to = from + Math.Sign(direction);
            var target = this.ItemAt(to);
            if (null == target)
            {
                return null;
            }

            PageSwitch pageSwitch = null;
            if ((null != this._frame) && (null != target.TabPage))
            {
                pageSwitch = this._frame.BeginReplace(target.TabPage, (to > from) ? NavigationMode.Forward : NavigationMode.Back);
                if (null == pageSwitch)
                {
                    return null;
                }
            }

            this._switch = new TabSwitch(this, from, to, pageSwitch);
            this.OnStateChanged();
            return this._switch;
        }

        /// <summary>
        /// Switches tabs as a swipe moves (such as a
        /// <see cref="FloatingButton.Swiping"/>): the content follows the
        /// finger, moving at twice its speed, so a swipe from the middle to the
        /// edge goes all the way; released, the switch completes past a third
        /// of the way, or with a flick, and goes back otherwise. A swipe to the
        /// right goes to the previous tab, as it pulls in the page on the left.
        /// </summary>
        public void FollowSwipe(
            SwipingEventArgs e)
        {
            if (null == e)
            {
                throw new ArgumentNullException(nameof(e));
            }

            switch (e.Phase)
            {
                case SwipePhase.Began:
                    this._swipeSwitch?.Cancel();
                    this._swipeSwitch = this.BeginSwitch(-e.Direction);
                    if (null != this._swipeSwitch)
                    {
                        this._swipeSwitch.Progress = this.SwipeProgress(e.Distance);
                    }
                    break;

                case SwipePhase.Moved:
                    if (null != this._swipeSwitch)
                    {
                        this._swipeSwitch.Progress = this.SwipeProgress(e.Distance);
                    }
                    break;

                case SwipePhase.Ended:
                {
                    var swipeSwitch = this._swipeSwitch;
                    this._swipeSwitch = null;
                    if (null != swipeSwitch)
                    {
                        swipeSwitch.Progress = this.SwipeProgress(e.Distance);
                        bool complete = (e.Velocity >= SwipeFlickVelocity)
                            || ((swipeSwitch.Progress >= SwipeCompleteProgress) && (e.Velocity > -SwipeFlickVelocity));
                        if (complete)
                        {
                            swipeSwitch.Complete();
                        }
                        else
                        {
                            swipeSwitch.Cancel();
                        }
                    }
                    break;
                }

                case SwipePhase.Canceled:
                    this._swipeSwitch?.Cancel();
                    this._swipeSwitch = null;
                    break;
            }
        }

        // A swipe's share of the way: the content moves twice the finger's
        // distance, across the frame (or the strip)
        private float SwipeProgress(
            float distance)
        {
            float width = (null != this._frame) ? this._frame.Bounds.Width : this.Bounds.Width;
            return (width > 0) ? (2 * distance / width) : 0;
        }

        // A switch moved
        internal void OnSwitchMoved(
            TabSwitch tabSwitch,
            float position)
        {
            if (tabSwitch == this._switch)
            {
                this._selectionPosition = position;
                this.OnStateChanged();
            }
        }

        // A switch ended: completed, its tab is checked (its page already
        // arriving); canceled, the selection slides back
        internal void OnSwitchEnded(
            TabSwitch tabSwitch,
            bool isCompleted)
        {
            if (tabSwitch != this._switch)
            {
                return;
            }

            this._switch = null;
            if (isCompleted)
            {
                var target = this.ItemAt(tabSwitch.To);
                if (null != target)
                {
                    this.Check(target, isSwitched: null != tabSwitch.PageSwitch);
                }
            }
            else
            {
                this._selectionPosition = tabSwitch.From;
                this.OnStateChanged();
            }
        }

        private TabStripItem ItemAt(
            int index)
        {
            if (index < 0)
            {
                return null;
            }

            int i = 0;
            foreach (var child in this._buttonsLayout.Children)
            {
                if (child is TabStripItem item)
                {
                    if (i == index)
                    {
                        return item;
                    }

                    i += 1;
                }
            }

            return null;
        }

        private int IndexOf(
            TabStripItem target)
        {
            if (null == target)
            {
                return -1;
            }

            int i = 0;
            foreach (var child in this._buttonsLayout.Children)
            {
                if (child is TabStripItem item)
                {
                    if (item == target)
                    {
                        return i;
                    }

                    i += 1;
                }
            }

            return -1;
        }

        private void TabStripItem_IsCheckedChanged(
            object sender,
            ToggledEventArgs e)
        {
            if (!this._isUpdatingChecks && (sender is TabStripItem item) && item.IsChecked)
            {
                // A tab tapped while a gesture switches puts the switch back
                this._switch?.Cancel();
                this.Check(item, isSwitched: false);
            }
        }

        // Checks the tab and unchecks the others, then moves the selection
        // there, switches the frame's page (unless a switch already has), and
        // raises the changed event
        private void Check(
            TabStripItem item,
            bool isSwitched)
        {
            this._isUpdatingChecks = true;
            try
            {
                foreach (var child in this._buttonsLayout.Children)
                {
                    if (child is TabStripItem other)
                    {
                        other.IsChecked = (other == item);
                    }
                }
            }
            finally
            {
                this._isUpdatingChecks = false;
            }

            if (item == this._selection)
            {
                return;
            }

            var previous = this._selection;
            int index = this.IndexOf(item);
            int previousIndex = this.IndexOf(previous);
            this._selection = item;
            this._selectionPosition = index;
            this.OnStateChanged();

            var navigationMode = (index >= previousIndex) ? NavigationMode.Forward : NavigationMode.Back;
            if (!isSwitched && (null != this._frame) && (null != item.TabPage))
            {
                _ = this._frame.ReplaceAsync(item.TabPage, navigationMode);
            }

            this.SelectionChanged?.Invoke(this, new TabSelectionChangedEventArgs(item.TabPage, previous?.TabPage, navigationMode));
        }
    }

    /// <summary>
    /// A switch to the next or previous tab that follows a gesture (see
    /// <see cref="TabStrip.BeginSwitch"/>): the selection and the tab's page
    /// move as far as the <see cref="Progress"/>, then it completes or goes
    /// back.
    /// </summary>
    public sealed class TabSwitch
    {
        private readonly TabStrip _strip;
        private float _progress;
        private bool _isEnded;

        internal TabSwitch(
            TabStrip strip,
            int from,
            int to,
            PageSwitch pageSwitch)
        {
            this._strip = strip;
            this.From = from;
            this.To = to;
            this.PageSwitch = pageSwitch;
        }

        internal int From { get; }

        internal int To { get; }

        internal PageSwitch PageSwitch { get; }

        /// <summary>
        /// How far it's gone: 0 (still on the tab it started from) to 1 (all
        /// the way to the next).
        /// </summary>
        public float Progress
        {
            get
            {
                return this._progress;
            }

            set
            {
                if (this._isEnded)
                {
                    return;
                }

                this._progress = Math.Clamp(value, 0, 1);
                if (null != this.PageSwitch)
                {
                    this.PageSwitch.Progress = this._progress;
                }

                this._strip.OnSwitchMoved(this, this.From + ((this.To - this.From) * this._progress));
            }
        }

        /// <summary>
        /// Goes the rest of the way: the tab is checked, and its page slides
        /// the rest of the way in.
        /// </summary>
        public void Complete()
        {
            if (this._isEnded)
            {
                return;
            }

            this._isEnded = true;
            if (null != this.PageSwitch)
            {
                _ = this.PageSwitch.CompleteAsync();
            }

            this._strip.OnSwitchEnded(this, isCompleted: true);
        }

        /// <summary>
        /// Goes back to the tab it started from.
        /// </summary>
        public void Cancel()
        {
            if (this._isEnded)
            {
                return;
            }

            this._isEnded = true;
            if (null != this.PageSwitch)
            {
                _ = this.PageSwitch.CancelAsync();
            }

            this._strip.OnSwitchEnded(this, isCompleted: false);
        }
    }
}
