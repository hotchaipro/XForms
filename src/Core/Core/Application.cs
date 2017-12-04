using System;
using XForms.Controls;

namespace XForms
{
    public interface IApplicationDelegate
    {
        void NotifySuspend();

        void NotifyResume();
    }

    public interface IApplicationRenderer
    {
        void SetRootFrame(Frame frame);

        void OnNavigationComplete();

        void InvokeOnUIThreadAsync(Action action);

        void Exit();

        void ApplyTheme(AppTheme theme);

        Color? TitleBarAccentColor { get; set; }

        Color? TitleBarForegroundColor { get; set; }

        Version GetVersion();

        Color AccentColor { get; set; }

        Color ForegroundColor { get; }
    }

    public abstract class Application : IApplicationDelegate
    {
        private static Application _CurrentApplication;

        private IPlatform _platform;
        private Frame _rootFrame;
        private Popover _activePopover;
        private AppTheme _appTheme;
        private ThemeColor _accentColor;

        public static float TouchSlop = 15.0f;

        public event EventHandler<AppTheme> ThemeChanged;

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

            this._accentColor = AppTheme.DefaultAccentColor;
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

        public Color ForegroundColor
        {
            get
            {
                return this.Renderer.ForegroundColor;
            }
        }

        public TitleBar TitleBar
        {
            get;
        }

        public void Start()
        {
            this.Renderer.SetRootFrame(this._rootFrame);

            this.OnStart();

            this.OnLoadResources();
        }

        public AppTheme Theme
        {
            get
            {
                return this._appTheme ?? AppTheme.Light;
            }

            set
            {
                this._appTheme = value;

                if (null == value)
                {
                    this.ApplyTheme(AppTheme.Light);
                }
                else
                {
                    this.ApplyTheme(value);
                }
            }
        }

        public ThemeColor AccentColor
        {
            get
            {
                return this._accentColor;
            }

            set
            {
                this._accentColor = value;

                this.ApplyTheme(this._appTheme);
            }
        }

        public bool CanNavigateBack
        {
            get
            {
                return (null != this._activePopover) || (this._rootFrame.CanNavigateBack);
            }
        }

        private void ApplyTheme(
            AppTheme theme)
        {
            if (null == theme)
            {
                throw new ArgumentNullException(nameof(theme));
            }

            this.OnApplyTheme(theme);

            this.Renderer.ApplyTheme(theme);

            this.ThemeChanged?.Invoke(this, theme);
        }

        protected virtual void OnApplyTheme(
            AppTheme theme)
        {
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

            // Close the active popover before navigating
            this.TryClosePopover();

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

            // Close any active popover
            if (this.TryClosePopover())
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

        internal LightDismissControl LightDismissControl
        {
            get;
            set;
        }

        internal bool HandlePreviewInput(
            Page page,
            PointerInputEvent pointerInputEvent)
        {
            bool handled = false;

            if (pointerInputEvent.State == PointerInputState.Began)
            {
                LightDismissControl lightDismissControl = this.LightDismissControl;
                if (null != lightDismissControl)
                {
                    // Invoke light dismiss
                    lightDismissControl.OnLightDismiss();

                    this.LightDismissControl = null;

                    handled = true;
                }
            }

            return handled;
        }

        internal bool HandlePreviewInput(
            View view,
            PointerInputEvent pointerInputEvent)
        {
            return this.HandleLightDismissEvent(view, pointerInputEvent);
        }

        private bool HandleLightDismissEvent(
            View view,
            PointerInputEvent pointerInputEvent)
        {
            bool handled = false;

            if (pointerInputEvent.State == PointerInputState.Began)
            {
                LightDismissControl lightDismissControl = this.LightDismissControl;
                if (null != lightDismissControl)
                {
                    if (!lightDismissControl.HasDescendant(view))
                    {
                        // Invoke light dismiss
                        lightDismissControl.OnLightDismiss();

                        this.LightDismissControl = null;

                        handled = true;
                    }
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

        public virtual void Exit()
        {
            this.Renderer.Exit();
        }

        internal void SetActivePopover(
            Popover popover)
        {
            if ((null != popover) && (this._activePopover != null))
            {
                throw new NotSupportedException("Only one popover at a time is supported.");
            }

            this._activePopover = popover;

            this.OnNavigationComplete();
        }

        private bool TryClosePopover()
        {
            bool didClose = false;

            var popover = this._activePopover;
            if (null != popover)
            {
                popover.Close();
                this._activePopover = null;
                didClose = true;
            }

            return didClose;
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
    }
}
