using System;

namespace XForms
{
    /// <summary>
    /// The app's main window. On the desktop it has a title and a size the app
    /// can set; on phones the window is the screen, and only its size is
    /// meaningful.
    /// </summary>
    public sealed class Window
    {
        private readonly IApplicationRenderer _renderer;
        private string _title;
        private Size _minimumSize;

        internal Window(
            IApplicationRenderer renderer)
        {
            this._renderer = renderer;
        }

        /// <summary>
        /// The title in the window's title bar (and the task bar).
        /// </summary>
        public string Title
        {
            get
            {
                return this._title;
            }

            set
            {
                this._title = value;
                this._renderer.WindowTitle = value;
            }
        }

        /// <summary>
        /// The size of the window's content area, in device-independent pixels.
        /// </summary>
        public Size Size
        {
            get;
            private set;
        }

        /// <summary>
        /// Whether the app can resize the window (on the desktop).
        /// </summary>
        public bool CanResize
        {
            get
            {
                return this._renderer.CanResizeWindow;
            }
        }

        /// <summary>
        /// The smallest size the user can make the window (on the desktop).
        /// </summary>
        public Size MinimumSize
        {
            get
            {
                return this._minimumSize;
            }

            set
            {
                this._minimumSize = value;
                this._renderer.MinimumWindowSize = value;
            }
        }

        public event EventHandler SizeChanged;

        /// <summary>
        /// Resizes the window's content area (on the desktop; elsewhere it does
        /// nothing).
        /// </summary>
        public void Resize(
            Size size)
        {
            this._renderer.ResizeWindow(size);
        }

        internal void OnSizeChanged(
            Size size)
        {
            if (size != this.Size)
            {
                this.Size = size;
                this.SizeChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
