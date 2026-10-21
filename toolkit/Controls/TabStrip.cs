using System;
using XForms.Layouts;
using XForms.Controls;
using XForms.Primitives;

namespace XForms.Toolkit
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

    public class TabStrip : ContentControl
    {
        private UniformGrid _buttonsLayout;
        private TabStripItem _selection;

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

        public override Color BackgroundColor
        {
            get
            {
                if (this.ActualTheme.Mode != ThemeMode.Light)
                {
                    return this.ActualTheme.Colors.Surface;
                }
                else
                {
                    return this.ActualTheme.Colors.Accent;
                }
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
            if (direction == 0)
            {
                return;
            }

            int tabCount = 0;
            int selectedIndex = -1;
            foreach (TabStripItem tabButton in this._buttonsLayout.Children)
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
            TabStripItem checkedButton = null;
            TabStripItem uncheckedButton = null;
            int checkedButtonIndex = -1;
            int uncheckedButtonIndex = -1;
            int i = -1;
            foreach (TabStripItem tabButton in this._buttonsLayout.Children)
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

        private void TabStripItem_IsCheckedChanged(
            object sender,
            ToggledEventArgs e)
        {
            TabStripItem checkedButton = sender as TabStripItem;
            if ((null != checkedButton) && (checkedButton.IsChecked) && (null != this._buttonsLayout.Children))
            {
                // Uncheck the other buttons
                TabStripItem uncheckedButton = null;
                int checkedButtonIndex = -1;
                int uncheckedButtonIndex = -1;
                int i = 0;
                foreach (TabStripItem tabButton in this._buttonsLayout.Children)
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
