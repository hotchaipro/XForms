using System;

namespace XForma
{
    /// <summary>
    /// Whether navigation goes forward, back or replaces the page.
    /// </summary>
    public enum NavigationMode
    {
        Forward = 0,
        Back = 1,
    }

    /// <summary>
    /// A page being navigated to or from.
    /// </summary>
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
