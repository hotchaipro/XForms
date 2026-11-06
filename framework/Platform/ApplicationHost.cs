using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace XForma.Platform
{
    /// <summary>
    /// The framework's side of the platform contract for the app as a whole:
    /// what a platform calls when the app is suspended or resumed, the system's
    /// appearance or the safe area changes, the user asks to go back, a
    /// hovering pointer moves, or a window closes. The counterpart of
    /// <see cref="IApplicationRenderer"/>, which the platform implements.
    /// </summary>
    /// <remarks>
    /// A platform drives the framework only through the hosts in this
    /// namespace (this one and <see cref="LayerHost"/>), the renderer
    /// interfaces, and members marked <see cref="PlatformContractAttribute"/>;
    /// the XF0001 analyzer enforces it. Call its members on the UI thread.
    /// </remarks>
    internal sealed class ApplicationHost
    {
        private readonly Application _application;

        private ApplicationHost(
            Application application)
        {
            this._application = application;
        }

        /// <summary>
        /// The host for the app.
        /// </summary>
        public static ApplicationHost For(
            Application application)
        {
            ArgumentNullException.ThrowIfNull(application);
            return application.Host ??= new ApplicationHost(application);
        }

        /// <summary>
        /// The host for the running app, or null before it's created.
        /// </summary>
        public static ApplicationHost Current
        {
            get
            {
                var application = Application.Current;
                return (null != application) ? For(application) : null;
            }
        }

        /// <summary>
        /// The app this host runs.
        /// </summary>
        public Application Application
        {
            get
            {
                return this._application;
            }
        }

        // ── Lifecycle ──────────────────────────────────────────────────────

        /// <summary>
        /// Starts the app on the platform: creates its renderer, dispatcher and
        /// main window, shows its root frame, calls its <c>OnStart</c>, and
        /// shows its first pages: the stack of paths given (a stack restored
        /// after the system ended the app, the browser's address, a link), or
        /// the app's <c>StartPath</c>.
        /// </summary>
        public void Start(
            IPlatform platform,
            IReadOnlyList<string> startStack = null)
        {
            this._application.Start(platform, startStack);
        }

        // ── Navigation ─────────────────────────────────────────────────────

        /// <summary>
        /// The paths of the pages in the back stack and the page showing,
        /// oldest first, for the platform to save or show (the browser's
        /// address).
        /// </summary>
        public IReadOnlyList<string> NavigationStack
        {
            get
            {
                return this._application.NavigationStack;
            }
        }

        /// <summary>
        /// Whether a popup is open: one more step for Back.
        /// </summary>
        public bool HasOpenPopups
        {
            get
            {
                return this._application.HasOpenPopups;
            }
        }

        /// <summary>
        /// Brings the back stack to the given paths (the browser's Back and
        /// Forward): back to the pages both share, then forward to the rest.
        /// </summary>
        public Task NavigateToStackAsync(
            IReadOnlyList<string> paths)
        {
            return this._application.NavigateToStackAsync(paths);
        }

        /// <summary>
        /// Opens a path from outside the app while it runs (a link), over the
        /// current page.
        /// </summary>
        public Task OpenPathAsync(
            string path)
        {
            return this._application.OpenPathAsync(path);
        }

        /// <summary>
        /// The number the app's icon shows (0 for none).
        /// </summary>
        public int BadgeCount
        {
            get
            {
                return this._application.BadgeCount;
            }
        }

        /// <summary>
        /// Raised when <see cref="BadgeCount"/> changes, before the app starts
        /// too (in a background refresh).
        /// </summary>
        public event EventHandler BadgeCountChanged
        {
            add
            {
                this._application.BadgeCountChanged += value;
            }

            remove
            {
                this._application.BadgeCountChanged -= value;
            }
        }

        /// <summary>
        /// How often the app asks to be woken in the background, or null.
        /// </summary>
        public TimeSpan? BackgroundRefreshInterval
        {
            get
            {
                return this._application.BackgroundRefreshInterval;
            }
        }

        /// <summary>
        /// The system woke the app in the background: it refreshes.
        /// </summary>
        public Task RefreshInBackgroundAsync()
        {
            return this._application.RefreshInBackgroundAsync();
        }

        /// <summary>
        /// The app went into the background: it saves its state.
        /// </summary>
        public void Suspend()
        {
            this._application.OnSuspend();
        }

        /// <summary>
        /// The app came back to the foreground.
        /// </summary>
        public void Resume()
        {
            this._application.OnResume();
        }

        /// <summary>
        /// The platform's Back key or gesture: goes back, or leaves the app
        /// when there's nowhere to go back to.
        /// </summary>
        public void Back()
        {
            this._application.OnBackRequested();
        }

        /// <summary>
        /// The platform closed a window the app opened.
        /// </summary>
        public void WindowClosed(
            Window window)
        {
            ArgumentNullException.ThrowIfNull(window);
            window.OnClosed();
        }

        // ── Environment ────────────────────────────────────────────────────

        /// <summary>
        /// The system's appearance settings changed: its theme mode, accent
        /// color, high-contrast colors or text size.
        /// </summary>
        public void SystemThemeChanged()
        {
            this._application.OnSystemThemeChanged();
        }

        /// <summary>
        /// The parts of the main window covered by system bars and a display
        /// cutout, or the soft keyboard's height over its bottom, changed (in
        /// DIPs). The pages in the main frame lay out again around them.
        /// </summary>
        public void SafeAreaInsetsChanged(
            Thickness bars,
            float keyboardHeight)
        {
            if (this._application.MainWindow.OnSafeAreaInsetsChanged(bars, keyboardHeight))
            {
                this._application.OnPageInsetsChanged();
            }
        }

        // ── Drag and drop ──────────────────────────────────────────────────

        /// <summary>
        /// Whether a drag could start in the view.
        /// </summary>
        public bool CanBeginDrag(
            View view)
        {
            return DragDropSession.CanBegin(view);
        }

        /// <summary>
        /// A drag is starting in the view: asks what's dragged, or null for no
        /// drag.
        /// </summary>
        public DataPackage BeginDrag(
            View view)
        {
            return DragDropSession.Begin(view);
        }

        /// <summary>
        /// A drag moved over the deepest view under it (or null), at a point in
        /// its coordinates, with the data from another app, if that's where it
        /// came from. Returns what a drop there would do.
        /// </summary>
        public DragOperation UpdateDrag(
            View deepest,
            Point point,
            DataPackage external)
        {
            return DragDropSession.Update(deepest, point, external);
        }

        /// <summary>
        /// A drag left the window.
        /// </summary>
        public void ExitDrag()
        {
            DragDropSession.Exit();
        }

        /// <summary>
        /// A drag dropped. Returns what the drop did.
        /// </summary>
        public DragOperation DropDrag(
            View deepest,
            Point point,
            DataPackage external)
        {
            return DragDropSession.Drop(deepest, point, external);
        }

        /// <summary>
        /// A key for a drag from the keyboard, on a platform that sends keys to
        /// views itself: returns whether it was used.
        /// </summary>
        public bool HandleDragKey(
            View focused,
            KeyEventArgs e)
        {
            return DragDropSession.HandleKey(focused, e);
        }

        /// <summary>
        /// A drag from this app ended, having done this.
        /// </summary>
        public void EndDrag(
            DragOperation operation)
        {
            DragDropSession.End(operation);
        }

        // ── Hover ──────────────────────────────────────────────────────────

        /// <summary>
        /// A hovering pointer moved over a view: the deepest view under it
        /// (the platform reports native views; a layer's
        /// <see cref="LayerHost.HitTestPointerOver"/> finds lightweight ones),
        /// or null when it left the window.
        /// </summary>
        public void PointerOver(
            View deepest,
            PointerEventArgs e)
        {
            this._application.UpdatePointerOver(deepest, e);
        }

        /// <summary>
        /// Changes each time a hovering pointer moves over a view, so a platform
        /// can tell whether an exit it waited a turn to report is still the
        /// latest news.
        /// </summary>
        public int PointerOverVersion
        {
            get
            {
                return this._application.PointerOverVersion;
            }
        }
    }
}
