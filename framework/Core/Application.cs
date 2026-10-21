using System;
using System.Collections.Generic;
using XForms.Controls;

namespace XForms
{
    public interface IApplicationDelegate
    {
        void NotifySuspend();

        void NotifyResume();

        /// <summary>
        /// The system's theme mode, accent color or high-contrast colors
        /// changed.
        /// </summary>
        void NotifySystemThemeChanged();
    }

    public interface IApplicationRenderer
    {
        void SetRootFrame(Frame frame);

        void OnNavigationComplete();

        void InvokeOnUIThreadAsync(Action action);

        /// <summary>
        /// Calls back once, on the UI thread, when the display is about to show
        /// its next frame, with the frame's time.
        /// </summary>
        void RequestFrame(Action<TimeSpan> onFrame);

        void Exit();

        /// <summary>
        /// Styles the platform's own parts (title bar, system bars) for the
        /// application's theme.
        /// </summary>
        void ApplyTheme(Theme theme);

        /// <summary>
        /// The system's theme mode: dark mode, or a high-contrast setting.
        /// </summary>
        ThemeMode SystemThemeMode { get; }

        /// <summary>
        /// The system's accent color, or null where there isn't one.
        /// </summary>
        Color? SystemAccentColor { get; }

        /// <summary>
        /// The system's high-contrast colors, or null to use XForms' own.
        /// </summary>
        ColorScheme SystemHighContrastColors { get; }

        Color? TitleBarAccentColor { get; set; }

        Color? TitleBarForegroundColor { get; set; }

        Version GetVersion();
    }

    public abstract class Application : IApplicationDelegate
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

        public bool CanNavigateBack
        {
            get
            {
                return (this._openPopups.Count > 0) || (this._rootFrame.CanNavigateBack);
            }
        }

        public Version GetVersion()
        {
            return this.Renderer.GetVersion();
        }

        public void NavigateTo(
            Page newPage)
        {
            if (null == newPage)
            {
                throw new ArgumentNullException(nameof(newPage));
            }

            // Close any open popups before navigating
            while (this.TryClosePopup())
            {
            }

            this._rootFrame.NavigateTo(newPage);

            this.OnNavigationComplete();
        }

        public void NavigateBack()
        {
            if (!this.CanNavigateBack)
            {
                this.Exit();
                return;
            }

            // Going back closes the top popup first
            if (this.TryClosePopup())
            {
                return;
            }

            this._rootFrame.NavigateBack();

            this.OnNavigationComplete();
        }

        private void OnNavigationComplete()
        {
            this.Renderer.OnNavigationComplete();
        }

        /// <summary>
        /// A view that closes (such as a flyout) when the user touches anywhere
        /// outside it. The touch that dismisses it does nothing else.
        /// </summary>
        public ILightDismissTarget LightDismissTarget
        {
            get;
            set;
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

        internal bool FilterPointerInput(
            View view,
            PointerEventArgs e)
        {
            // An open light-dismiss surface takes a press outside it
            if (this.HandleLightDismissEvent(view, e))
            {
                return true;
            }

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

        private bool HandleLightDismissEvent(
            View view,
            PointerEventArgs pointerInputEvent)
        {
            bool handled = false;

            if (pointerInputEvent.State == PointerInputState.Began)
            {
                var target = this.LightDismissTarget;
                if ((null != target) && ((view is Page) || !target.Contains(view)))
                {
                    this.LightDismissTarget = null;
                    target.OnLightDismiss();
                    handled = true;
                }
            }

            return handled;
        }

        public void InvokeOnUIThreadAsync(
            Action action)
        {
            if (null == action)
            {
                throw new ArgumentNullException(nameof(action));
            }

            this.Renderer.InvokeOnUIThreadAsync(action);
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
        /// <see cref="Controls.TextBlock.FontFamily"/>. The file is next to the
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

        void IApplicationDelegate.NotifySuspend()
        {
            this.OnSuspend();
        }

        void IApplicationDelegate.NotifyResume()
        {
            this.OnResume();
        }

        void IApplicationDelegate.NotifySystemThemeChanged()
        {
            this.UpdateTheme();
        }
    }
}
