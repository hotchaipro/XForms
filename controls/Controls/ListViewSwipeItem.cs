using System;
using XForms.Controls;
using XForms.Primitives;

namespace XForms.Controls
{
    /// <summary>
    /// Which way a list row was swiped: toward its start (primary) or its
    /// end (secondary).
    /// </summary>
    public enum ListViewSwipeDirection
    {
        Primary = 0,
        Secondary,
    }

    /// <summary>
    /// What shows behind a list row as it's swiped: the command the swipe
    /// runs, in its color. Its theme's template draws it.
    /// </summary>
    public class ListViewSwipeItem : TemplatedControl
    {
        private ListViewSwipeDirection _swipeDirection;
        private UICommand _command;
        private Size _iconSize;

        public ListViewSwipeItem()
        {
            this._swipeDirection = ListViewSwipeDirection.Primary;
        }

        public ListViewSwipeDirection Direction
        {
            get
            {
                return this._swipeDirection;
            }

            set
            {
                this._swipeDirection = value;
                this.InvalidateVisual();
            }
        }

        public UICommand Command
        {
            get
            {
                return this._command;
            }

            set
            {
                this._command = value;

                this.InvalidateVisual();
            }
        }

        public Bitmap Icon
        {
            get
            {
                return this._command?.Icon ?? null;
            }
        }

        public Size IconSize
        {
            get
            {
                return this._iconSize;
            }

            set
            {
                this._iconSize = value;

                this.InvalidateVisual();
            }
        }

        public string Text
        {
            get
            {
                return this._command?.Text ?? null;
            }
        }



        // The command's color
        public override Color BackgroundColor
        {
            get
            {
                return this._command?.AccentColor ?? Colors.Transparent;
            }
        }

        public void Reset()
        {
            this.Command = null;
        }
    }
}
