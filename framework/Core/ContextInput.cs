using System;

namespace XForms
{
    /// <summary>
    /// The user asked for a view's context menu: with a right-click, a long
    /// press, or the Menu key or Shift+F10 on the focused view.
    /// </summary>
    public sealed class ContextRequestedEventArgs : EventArgs
    {
        public ContextRequestedEventArgs(
            Point? position,
            PointerDeviceType? deviceType)
        {
            this.Position = position;
            this.DeviceType = deviceType;
        }

        /// <summary>
        /// Where the pointer asked, in <see cref="OriginalSource"/>'s
        /// coordinates; null when the keyboard asked.
        /// </summary>
        public Point? Position
        {
            get;
        }

        /// <summary>
        /// The pointer that asked (a mouse's right-click, a touch's or pen's
        /// long press), or null for the keyboard.
        /// </summary>
        public PointerDeviceType? DeviceType
        {
            get;
        }

        /// <summary>
        /// The view the request started at, before going to its ancestors.
        /// </summary>
        public View OriginalSource
        {
            get;
            internal set;
        }

        /// <summary>
        /// Set when the request was answered, so it goes no further.
        /// </summary>
        public bool Handled
        {
            get;
            set;
        }
    }
}
