using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using XForms.Primitives;
using XForms.Platform;

namespace XForms
{
    public abstract class Application
    {
        private static Application _CurrentApplication;

        private IPlatform _platform;
        private Frame _rootFrame;
        private readonly List<Popup> _openPopups = new List<Popup>();
        private ThemeMode? _requestedThemeMode;
        private Color? _accentColor;
        private Theme _theme;

        public static float TouchSlop = 15.0f;

        /// <summary>
        /// Raised when the application's theme changes: its mode, accent color
        /// or design tokens.
        /// </summary>
        public event EventHandler ThemeChanged;

        public static Application Current
        {
            get
            {
                return _CurrentApplication;
            }
        }

        public Application(
            IPlatform platform)
        {
            if (null == platform)
            {
                throw new ArgumentNullException(nameof(platform));
            }

            this._platform = platform;

            _CurrentApplication = this;

            this.Renderer = this._platform.CreateApplicationRenderer(this);

            this.TitleBar = new TitleBar(this);

            this._rootFrame = new Frame();
            this.Dispatcher = new Dispatcher(this.Renderer);
            this.MainWindow = new Window(this.Renderer);
            this._textScale = this.Renderer.SystemTextScale;

        }

        internal IPlatform Platform
        {
            get
            {
                return this._platform;
            }
        }

        public IApplicationRenderer Renderer
        {
            get;
            private set;
        }



        public TitleBar TitleBar
        {
            get;
        }

        public void Start()
        {
            this.Renderer.SetRootFrame(this._rootFrame);

            // The platform's own parts exist once the root is set
            var theme = this.Theme;
            this.Renderer.ApplyTheme(theme);
            this.OnThemeChanged(theme);

            this.OnStart();

            this.OnLoadResources();
        }

        /// <summary>
        /// The application's theme: the one <see cref="CreateTheme"/> builds for
        /// the actual mode and accent color. Views inherit it unless they set
        /// their own <see cref="View.Theme"/>.
        /// </summary>
        public Theme Theme
        {
            get
            {
                return this._theme ??= this.CreateTheme(this.ActualThemeMode, this.ActualAccentColor);
            }
        }

        /// <summary>
        /// The mode the app asks for, or null (the default) to follow the
        /// system.
        /// </summary>
        public ThemeMode? RequestedThemeMode
        {
            get
            {
                return this._requestedThemeMode;
            }

            set
            {
                this._requestedThemeMode = value;
                this.UpdateTheme();
            }
        }

        /// <summary>
        /// The system's theme mode.
        /// </summary>
        public ThemeMode SystemThemeMode
        {
            get
            {
                return this.Renderer.SystemThemeMode;
            }
        }

        /// <summary>
        /// The mode in use. The system's high contrast always wins, since
        /// people who turn it on need it.
        /// </summary>
        public ThemeMode ActualThemeMode
        {
            get
            {
                var system = this.SystemThemeMode;
                return (system == ThemeMode.HighContrast) ? system : (this._requestedThemeMode ?? system);
            }
        }

        /// <summary>
        /// The app's accent color, or null (the default) for the system's (or,
        /// without one, <see cref="Theme.DefaultAccentColor"/>).
        /// </summary>
        public Color? AccentColor
        {
            get
            {
                return this._accentColor;
            }

            set
            {
                this._accentColor = value;
                this.UpdateTheme();
            }
        }

        /// <summary>
        /// The accent color in use.
        /// </summary>
        public Color ActualAccentColor
        {
            get
            {
                return this._accentColor ?? this.Renderer.SystemAccentColor ?? Theme.DefaultAccentColor;
            }
        }

        /// <summary>
        /// Builds the application's theme for a mode and accent color. Override
        /// it for a brand's colors, type scale, shapes or control styles; call
        /// <see cref="InvalidateTheme"/> when what it depends on changes.
        /// </summary>
        protected virtual Theme CreateTheme(
            ThemeMode mode,
            Color accentColor)
        {
            var theme = Theme.Create(mode, accentColor);
            if (mode == ThemeMode.HighContrast)
            {
                var system = this.Renderer.SystemHighContrastColors;
                if (null != system)
                {
                    theme = theme with { Colors = system };
                }
            }

            return theme;
        }

        /// <summary>
        /// Builds the theme again (see <see cref="CreateTheme"/>), and gives it
        /// to every view if it changed.
        /// </summary>
        public void InvalidateTheme()
        {
            this.UpdateTheme();
        }

        private void UpdateTheme()
        {
            var theme = this.CreateTheme(this.ActualThemeMode, this.ActualAccentColor)
                ?? throw new InvalidOperationException("CreateTheme returned null.");

            bool isChanged = !theme.Equals(this._theme);
            this._theme = theme;
            if (!isChanged)
            {
                return;
            }

            this.Renderer.ApplyTheme(theme);
            this.OnThemeChanged(theme);

            this._rootFrame.UpdateTheme();
            foreach (var window in this._windows)
            {
                window.Frame.UpdateTheme();
            }
            foreach (var popup in this._openPopups)
            {
                popup.UpdateTheme();
            }

            this.ThemeChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Called with the application's theme when it starts, and whenever
        /// the theme changes.
        /// </summary>
        protected virtual void OnThemeChanged(
            Theme theme)
        {
        }

        /// <summary>
        /// Whether going back has somewhere to go: a popup or modal page to
        /// close, or a page in the back stack.
        /// </summary>
        public bool CanGoBack
        {
            get
            {
                return (this._openPopups.Count > 0) || (this._modals.Count > 0) || this._rootFrame.CanGoBack;
            }
        }

        public Version GetVersion()
        {
            return this.Renderer.GetVersion();
        }

        // The modal pages showing, newest last, with how to complete each
        private readonly List<(Page Page, Action Complete)> _modals = new List<(Page Page, Action Complete)>();

        /// <summary>
        /// Shows a page (a new instance, or one kept for reuse), keeping the
        /// current page in the back stack. Pass what the page needs to its
        /// constructor. Completes when the transition has.
        /// </summary>
        public async Task NavigateAsync(
            Page page,
            PageTransition transition = null)
        {
            if (null == page)
            {
                throw new ArgumentNullException(nameof(page));
            }

            // Close any open popups before navigating
            while (this.TryClosePopup())
            {
            }

            await this._rootFrame.NavigateAsync(page, transition);
            this.OnNavigationComplete();
        }

        /// <summary>
        /// Goes back: closes the top popup, or the top modal page, or returns
        /// to the previous page. Completes with false if there was nowhere to
        /// go.
        /// </summary>
        public async Task<bool> GoBackAsync()
        {
            if (this.TryClosePopup())
            {
                return true;
            }

            if (0 < this._modals.Count)
            {
                var modal = this._modals[this._modals.Count - 1];
                return await this.CloseModalAsync(modal.Page);
            }

            bool didGoBack = await this._rootFrame.GoBackAsync();
            this.OnNavigationComplete();
            return didGoBack;
        }

        /// <summary>
        /// Shows a page over the current one, sliding up. Completes when it's
        /// closed: by Back, or by the page (<see cref="Page.CloseModalAsync"/>).
        /// </summary>
        public async Task ShowModalAsync(
            Page page)
        {
            if (null == page)
            {
                throw new ArgumentNullException(nameof(page));
            }

            var completion = new TaskCompletionSource();
            this._modals.Add((page, () => completion.TrySetResult()));

            while (this.TryClosePopup())
            {
            }

            await this._rootFrame.NavigateAsync(page, PageTransition.SlideUp);
            this.OnNavigationComplete();
            await completion.Task;
        }

        /// <summary>
        /// Closes a modal page (and any pages shown after it), completing its
        /// <see cref="ShowModalAsync(Page)"/>.
        /// </summary>
        internal async Task<bool> CloseModalAsync(
            Page page)
        {
            int index = this._modals.FindIndex((modal) => modal.Page == page);
            if (index < 0)
            {
                return false;
            }

            var modal = this._modals[index];
            this._modals.RemoveAt(index);

            // Back to the page beneath the modal page
            while ((this._rootFrame.CurrentPage != page) && this._rootFrame.CanGoBack)
            {
                await this._rootFrame.GoBackAsync(PageTransition.None);
            }

            await this._rootFrame.GoBackAsync(PageTransition.SlideUp);
            this.OnNavigationComplete();
            modal.Complete();
            return true;
        }

        /// <summary>
        /// The platform's Back key or gesture: goes back, or leaves the app
        /// when there's nowhere to go back to.
        /// </summary>
        internal void OnBackRequested()
        {
            if (this.CanGoBack)
            {
                _ = this.GoBackAsync();
            }
            else
            {
                this.Exit();
            }
        }

        private void OnNavigationComplete()
        {
            this.Renderer.OnNavigationComplete();
        }



        /// <summary>
        /// Raised for every pointer event before the view under the pointer
        /// gets it (the event's <see cref="PointerEventArgs.OriginalSource"/>).
        /// Set <see cref="PointerEventArgs.Handled"/> to keep it from the view.
        /// </summary>
        public event EventHandler<PointerEventArgs> PreviewPointerInput;

        /// <summary>
        /// Raised for every key press before the focused view gets it. Set
        /// <see cref="KeyEventArgs.Handled"/> to keep it from the view.
        /// </summary>
        public event EventHandler<KeyEventArgs> PreviewKeyDown;

        // The views under a hovering pointer, deepest first
        private List<View> _pointerOverPath = new List<View>();
        private int _pointerOverVersion;

        /// <summary>
        /// Counts changes to the views under the hovering pointer, so a
        /// platform can tell whether the pointer moved on to another view after
        /// it left one.
        /// </summary>
        internal int PointerOverVersion
        {
            get
            {
                return this._pointerOverVersion;
            }
        }

        /// <summary>
        /// A hovering pointer moved over a view (its deepest view, or null when
        /// it left the window). Views it left get <see cref="View.PointerExited"/>,
        /// deepest first; views it moved into get
        /// <see cref="View.PointerEntered"/>, outermost first.
        /// </summary>
        internal void UpdatePointerOver(
            View deepest,
            PointerEventArgs e)
        {
            this._pointerOverVersion += 1;

            var path = new List<View>();
            for (var view = deepest; null != view; view = view.Parent as View)
            {
                path.Add(view);
            }

            foreach (var view in this._pointerOverPath)
            {
                if (!path.Contains(view))
                {
                    view.SetPointerOver(false, e);
                }
            }

            for (int i = path.Count - 1; i >= 0; i -= 1)
            {
                if (!this._pointerOverPath.Contains(path[i]))
                {
                    path[i].SetPointerOver(true, e);
                }
            }

            this._pointerOverPath = path;
        }

        internal bool FilterPointerInput(
            View view,
            PointerEventArgs e)
        {


            // The application sees each event once, at the view it was sent to
            if (e.OriginalSource == view)
            {
                this.PreviewPointerInput?.Invoke(this, e);
            }

            return e.Handled;
        }

        internal bool FilterKeyInput(
            View view,
            KeyEventArgs e)
        {
            this.PreviewKeyDown?.Invoke(this, e);
            return e.Handled;
        }

        /// <summary>
        /// Runs code on the UI thread.
        /// </summary>
        public Dispatcher Dispatcher
        {
            get;
        }

        // The windows besides the main one
        private readonly List<Window> _windows = new List<Window>();

        /// <summary>
        /// Whether the app can open windows besides its main one (on the
        /// desktop).
        /// </summary>
        public bool CanOpenWindows
        {
            get
            {
                return this.Renderer.CanOpenWindows;
            }
        }

        /// <summary>
        /// The app's windows: the main one, then those it opened.
        /// </summary>
        public IReadOnlyList<Window> Windows
        {
            get
            {
                var windows = new List<Window>() { this.MainWindow };
                windows.AddRange(this._windows);
                return windows;
            }
        }

        /// <summary>
        /// Opens a window showing the page, in a frame of its own. Throws where
        /// the platform has one window (see <see cref="CanOpenWindows"/>).
        /// </summary>
        public async Task<Window> OpenWindowAsync(
            Page page,
            string title = null,
            Size? size = null)
        {
            if (null == page)
            {
                throw new ArgumentNullException(nameof(page));
            }

            if (!this.CanOpenWindows)
            {
                throw new PlatformNotSupportedException("This platform shows one window.");
            }

            var frame = new Frame();
            var window = new Window(this, frame);
            this._windows.Add(window);
            window.Renderer = this.Renderer.OpenWindow(window, frame, title ?? string.Empty, size ?? new Size(640, 480));
            window.Title = title;
            frame.UpdateTheme();

            await frame.NavigateAsync(page, PageTransition.None);
            return window;
        }

        internal void OnWindowClosed(
            Window window)
        {
            this._windows.Remove(window);
        }

        /// <summary>
        /// The app's main window.
        /// </summary>
        public Window MainWindow
        {
            get;
        }

        private float _textScale;

        /// <summary>
        /// The system's text size, as a scale (1 is the default). Every font
        /// size is scaled by it, on every platform, so text follows the user's
        /// setting; layouts that depend on text size should leave room for it.
        /// </summary>
        public float TextScale
        {
            get
            {
                return this._textScale;
            }
        }

        public event EventHandler TextScaleChanged;

        // Lays text out again at the system's new text size
        private void UpdateTextScale()
        {
            float scale = this.Renderer.SystemTextScale;
            if (scale == this._textScale)
            {
                return;
            }

            this._textScale = scale;
            this._rootFrame.OnTextScaleChanged();
            foreach (var window in this._windows)
            {
                window.Frame.OnTextScaleChanged();
            }
            foreach (var popup in this._openPopups)
            {
                popup.OnTextScaleChanged();
            }

            this.TextScaleChanged?.Invoke(this, EventArgs.Empty);
        }

        private List<Action<TimeSpan>> _frameCallbacks = new List<Action<TimeSpan>>();

        /// <summary>
        /// Calls the callback once, on the UI thread, before the display's next
        /// frame, with the frame's time (as a browser's
        /// <c>requestAnimationFrame</c>). Request again from the callback to run
        /// every frame, for animations the compositor can't play, such as
        /// redrawing an owner-drawn view.
        /// </summary>
        public void RequestAnimationFrame(
            Action<TimeSpan> callback)
        {
            if (null == callback)
            {
                throw new ArgumentNullException(nameof(callback));
            }

            this._frameCallbacks.Add(callback);
            if (1 == this._frameCallbacks.Count)
            {
                this.Renderer.RequestFrame(this.OnFrame);
            }
        }

        private void OnFrame(
            TimeSpan frameTime)
        {
            // Callbacks requested from these run on the next frame
            var callbacks = this._frameCallbacks;
            this._frameCallbacks = new List<Action<TimeSpan>>();
            foreach (var callback in callbacks)
            {
                callback(frameTime);
            }
        }

        /// <summary>
        /// Registers a font file bundled with the app (TrueType or OpenType)
        /// under a family name, for <see cref="TextFormat.FontFamily"/> and
        /// <see cref="Primitives.TextBlock.FontFamily"/>. The file is next to the
        /// app on Windows, in its assets on Android, and in its bundle on iOS.
        /// </summary>
        public void RegisterFont(
            string familyName,
            string fileName)
        {
            if (string.IsNullOrEmpty(familyName))
            {
                throw new ArgumentException("A family name is required.", nameof(familyName));
            }

            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentException("A file name is required.", nameof(fileName));
            }

            this.Platform.RegisterFont(familyName, fileName);
        }

        /// <summary>
        /// Opens the URI in the system's handler for it, such as the web browser.
        /// </summary>
        public void OpenUri(
            Uri uri)
        {
            if (null == uri)
            {
                throw new ArgumentNullException(nameof(uri));
            }

            this.Platform.NavigateToUri(uri);
        }

        public virtual void Exit()
        {
            this.Renderer.Exit();
        }

        internal void OnPopupShown(
            Popup popup)
        {
            this._openPopups.Add(popup);
            this.OnNavigationComplete();
        }

        internal void OnPopupClosed(
            Popup popup)
        {
            this._openPopups.Remove(popup);
            this.OnNavigationComplete();
        }

        private bool TryClosePopup()
        {
            int count = this._openPopups.Count;
            if (count == 0)
            {
                return false;
            }

            var popup = this._openPopups[count - 1];
            popup.Close();

            // NOTE: The platform normally reports the close right away
            this._openPopups.Remove(popup);
            return true;
        }

        internal protected virtual void OnResume()
        {
        }

        /// <summary>
        /// Save application state and stop any background activity.
        /// </summary>
        internal protected virtual void OnSuspend()
        {
        }

        protected virtual void OnStart()
        {
        }

        protected virtual void OnLoadResources()
        {
        }



        /// <summary>
        /// Keeps the current page clear of the system bars, or not, as it asks.
        /// </summary>
        internal void UpdateSafeAreaUse()
        {
            this.Renderer.ExtendsIntoSafeArea = !(this._rootFrame?.CurrentPage?.UsesSafeArea ?? true);
        }

        /// <summary>
        /// The platform's side of the contract for this app (see
        /// <see cref="ApplicationHost.For"/>).
        /// </summary>
        internal ApplicationHost Host
        {
            get;
            set;
        }

        internal void OnSystemThemeChanged()
        {
            this.UpdateTheme();
            this.UpdateTextScale();
        }
    }
}
