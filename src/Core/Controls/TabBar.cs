using System;
using XForms.Layouts;

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

    public class TabBar : ContentControl
    {
        private DistributedStackLayout _buttonsLayout;
        private TabButton _selection;

        public event EventHandler<TabSelectionChangedEventArgs> SelectionChanged;

        public TabBar()
        {
            this._buttonsLayout = new DistributedStackLayout()
            {
                HorizontalAlignment = LayoutAlignment.Fill,
                VerticalAlignment = LayoutAlignment.Center,
                MaximumSize = new Size(600, Dimension.Auto),
            };

            this.HorizontalAlignment = LayoutAlignment.Fill;
            this.VerticalAlignment = LayoutAlignment.Start;

            this.Content = this._buttonsLayout;
        }

        public Page SelectedPage
        {
            get
            {
                return this._selection?.TabPage;
            }
        }

        public override Color BackgroundColor
        {
            get
            {
                if (this.Theme == AppTheme.Dark)
                {
                    return this.Theme.SubtleBackgroundColor;
                }
                else
                {
                    return this.Theme.AccentColor;
                }
            }
        }

        public void AddButton(
            TabButton tabButton,
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
            tabButton.IsCheckedChanged += TabButton_IsCheckedChanged;
            tabButton.HorizontalAlignment = LayoutAlignment.Center;
            this._buttonsLayout.Children.Add(tabButton);
        }

        public void SelectRelative(
            int direction)
        {
            if (direction == 0)
            {
                return;
            }

            int tabCount = 0;
            int selectedIndex = -1;
            foreach (TabButton tabButton in this._buttonsLayout.Children)
            {
                if (null != tabButton)
                {
                    if ((selectedIndex == -1) && (tabButton.IsChecked))
                    {
                        selectedIndex = tabCount;
                    }

                    tabCount += 1;
                }
            }

            if (selectedIndex >= 0)
            {
                int newIndex = selectedIndex + Math.Sign(direction);
                if ((newIndex >= 0) && (newIndex < tabCount))
                {
                    this.Select(newIndex);
                }
            }
        }

        private void Select(
            int index)
        {
            // Uncheck the other buttons
            TabButton checkedButton = null;
            TabButton uncheckedButton = null;
            int checkedButtonIndex = -1;
            int uncheckedButtonIndex = -1;
            int i = -1;
            foreach (TabButton tabButton in this._buttonsLayout.Children)
            {
                if (null != tabButton)
                {
                    i += 1;

                    if (i == index)
                    {
                        checkedButton = tabButton;
                        checkedButtonIndex = i;

                        tabButton.IsChecked = true;
                    }
                    else
                    {
                        if (tabButton.IsChecked)
                        {
                            uncheckedButton = tabButton;
                            uncheckedButtonIndex = i;
                        }
                        tabButton.IsChecked = false;
                    }
                }
            }

            // Raise the changed event
            if (checkedButton != this._selection)
            {
                this._selection = checkedButton;

                var selectionChanged = this.SelectionChanged;
                if (null != selectionChanged)
                {
                    NavigationMode navigationMode = ((checkedButtonIndex >= uncheckedButtonIndex) ? NavigationMode.Forward : NavigationMode.Back);
                    //selectionChanged(this, new TabSelectionChangedEventArgs(checkedButtonIndex, uncheckedButtonIndex, navigationMode));
                    //selectionChanged(this, new TabSelectionChangedEventArgs(checkedButton, uncheckedButton, navigationMode));
                    Page checkedTabPage = (checkedButton == null ? null : checkedButton.TabPage);
                    Page uncheckedTabPage = (uncheckedButton == null ? null : uncheckedButton.TabPage);
                    selectionChanged(this, new TabSelectionChangedEventArgs(checkedTabPage, uncheckedTabPage, navigationMode));
                }
            }
        }

        private void TabButton_IsCheckedChanged(
            object sender,
            ToggledEventArgs e)
        {
            TabButton checkedButton = sender as TabButton;
            if ((null != checkedButton) && (checkedButton.IsChecked) && (null != this._buttonsLayout.Children))
            {
                // Uncheck the other buttons
                TabButton uncheckedButton = null;
                int checkedButtonIndex = -1;
                int uncheckedButtonIndex = -1;
                int i = 0;
                foreach (TabButton tabButton in this._buttonsLayout.Children)
                {
                    if (null != tabButton)
                    {
                        if (tabButton == sender)
                        {
                            checkedButtonIndex = i;
                        }
                        else
                        {
                            if (tabButton.IsChecked)
                            {
                                uncheckedButton = tabButton;
                                uncheckedButtonIndex = i;
                            }
                            tabButton.IsChecked = false;
                        }

                        i += 1;
                    }
                }

                // Raise the changed event
                if (checkedButton != this._selection)
                {
                    this._selection = checkedButton;

                    var selectionChanged = this.SelectionChanged;
                    if (null != selectionChanged)
                    {
                        NavigationMode navigationMode = ((checkedButtonIndex >= uncheckedButtonIndex) ? NavigationMode.Forward : NavigationMode.Back);
                        //selectionChanged(this, new TabSelectionChangedEventArgs(checkedButtonIndex, uncheckedButtonIndex, navigationMode));
                        //selectionChanged(this, new TabSelectionChangedEventArgs(checkedButton, uncheckedButton, navigationMode));
                        Page checkedTabPage = (checkedButton == null ? null : checkedButton.TabPage);
                        Page uncheckedTabPage = (uncheckedButton == null ? null : uncheckedButton.TabPage);
                        selectionChanged(this, new TabSelectionChangedEventArgs(checkedTabPage, uncheckedTabPage, navigationMode));
                    }
                }
            }
        }
    }
}
