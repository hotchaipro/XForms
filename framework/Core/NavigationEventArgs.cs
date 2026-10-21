using System;

namespace XForms
{
    public enum NavigationMode
    {
        Forward = 0,
        Back = 1,
    }

    public class NavigationEventArgs : EventArgs
    {
        public NavigationEventArgs(
            NavigationMode navigationMode)
        {
            this.NavigationMode = navigationMode;
        }

        /// <summary>
        /// Whether the navigation went forward to a page or back to it.
        /// </summary>
        public NavigationMode NavigationMode
        {
            get;
        }
    }
}
