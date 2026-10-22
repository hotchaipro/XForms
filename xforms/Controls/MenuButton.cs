using System;
using XForms.Input;
using XForms.Controls;
using XForms.Primitives;

namespace XForms.Controls
{
    /// <summary>
    /// A <see cref="Controls.MenuBar"/>'s button, for one of its items. Its
    /// theme's template draws it.
    /// </summary>
    public sealed class MenuButton : TemplatedControl, ITapGestureDelegate
    {
        internal MenuButton(
            MenuBar menuBar,
            MenuItem menuItem)
        {
            if (null == menuBar)
            {
                throw new ArgumentNullException(nameof(menuBar));
            }

            if (null == menuItem)
            {
                throw new ArgumentNullException(nameof(menuItem));
            }

            this.Width = 72;
            this.Height = 50;

            this.MenuBar = menuBar;
            this.MenuItem = menuItem;
            this.IsVisible = menuItem.IsVisible;

            this.GestureRecognizers.Add(new TapGestureRecognizer(this));

            menuItem.PropertyChanged += MenuItem_PropertyChanged; // TODO: Matching detach event handler
        }

        private void MenuItem_PropertyChanged(
            object sender,
            System.ComponentModel.PropertyChangedEventArgs e)
        {
            this.IsVisible = this.MenuItem.IsVisible;
            this.InvalidateVisual();
        }

        public MenuBar MenuBar
        {
            get;
        }

        public MenuItem MenuItem
        {
            get;
        }

        #region ITapGestureDelegate implementation

        /// <summary>
        /// The value passed to the command when it runs (and when asked whether
        /// it can).
        /// </summary>
        public object CommandParameter
        {
            get;
            set;
        }

        void ITapGestureDelegate.OnTapBegan()
        {
            this.IsPressed = true;
        }

        void ITapGestureDelegate.OnTapEnded()
        {
            this.IsPressed = false;
        }

        void ITapGestureDelegate.OnTapped()
        {
            this.MenuItem.Command?.Execute(this.CommandParameter);
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
    }
}
