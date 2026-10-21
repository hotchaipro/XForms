using System;
using XForms.Platform;

namespace XForms
{
    /// <summary>
    /// A window: the app's main one, or one it opened with
    /// <see cref="Application.OpenWindowAsync"/> (on the desktop). On the
    /// desktop it has a title and a size the app can set; on phones the
    /// window is the screen, and only its size is meaningful.
    /// </summary>
    public sealed class Window
    {
        private readonly IApplicationRenderer _renderer;
        private readonly Application _application;
        private string _title;
        private Size _minimumSize;
        private bool _isClosed;

        internal Window(
            IApplicationRenderer renderer)
        {
            this._renderer = renderer;
        }

        // A window the app opened, showing a frame of its own
        internal Window(
            Application application,
            Frame frame)
        {
            this._application = application;
            this._renderer = application.Renderer;
            this.Frame = frame;
            frame.Window = this;
        }

        /// <summary>
        /// The frame a window the app opened shows (null for the main window).
        /// </summary>
        internal Frame Frame
        {
            get;
        }

        /// <summary>
        /// The renderer of a window the app opened.
        /// </summary>
        [PlatformContract]
        internal IWindowRenderer Renderer
        {
            get;
            set;
        }

        /// <summary>
        /// Whether this is the app's main window, which closes the app.
        /// </summary>
        public bool IsMainWindow
        {
            get
            {
                return null == this.Frame;
            }
        }

        /// <summary>
        /// Raised when a window the app opened is closed (by the user, or with
        /// <see cref="Close"/>).
        /// </summary>
        public event EventHandler Closed;

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
                if (null != this.Renderer)
                {
                    this.Renderer.Title = value ?? string.Empty;
                }
                else if (this.IsMainWindow)
                {
                    this._renderer.WindowTitle = value;
                }
            }
        }

        /// <summary>
        /// Closes a window the app opened. The main window closes with the app.
        /// </summary>
        public void Close()
        {
            if (this.IsMainWindow)
            {
                throw new InvalidOperationException("The main window closes with the app.");
            }

            if (!this._isClosed)
            {
                this.Renderer?.Close();
            }
        }

        /// <summary>
        /// The platform closed a window the app opened.
        /// </summary>
        internal void OnClosed()
        {
            if (this._isClosed)
            {
                return;
            }

            this._isClosed = true;
            this._application?.OnWindowClosed(this);
            this.Closed?.Invoke(this, EventArgs.Empty);
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
                if (null != this.Renderer)
                {
                    this.Renderer.MinimumSize = value;
                }
                else
                {
                    this._renderer.MinimumWindowSize = value;
                }
            }
        }

        public event EventHandler SizeChanged;

        /// <summary>
        /// The parts of the window covered by system bars, a display cutout or
        /// the soft keyboard, in DIPs. Pages are kept clear of them unless a
        /// page sets <see cref="Page.UsesSafeArea"/> to false, to draw under the
        /// bars and lay its content out around them itself. Zero on the desktop.
        /// </summary>
        public Thickness SafeAreaInsets
        {
            get;
            private set;
        }

        public event EventHandler SafeAreaInsetsChanged;

        internal void OnSafeAreaInsetsChanged(
            Thickness insets)
        {
            if (insets != this.SafeAreaInsets)
            {
                this.SafeAreaInsets = insets;
                this.SafeAreaInsetsChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Resizes the window's content area (on the desktop; elsewhere it does
        /// nothing).
        /// </summary>
        public void Resize(
            Size size)
        {
            if (null != this.Renderer)
            {
                this.Renderer.Resize(size);
            }
            else
            {
                this._renderer.ResizeWindow(size);
            }
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
