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
        private NavigationMode _navigationMode;

        public NavigationEventArgs(
            NavigationMode navigationMode)
        {
            this._navigationMode = navigationMode;
        }
    }
}