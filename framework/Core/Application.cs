using System;
using System.Collections.Generic;
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
        private readonly List<Popup> _openPopups = new List<Popup>();
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
                return (this._openPopups.Count > 0) || (this._rootFrame.CanNavigateBack);
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

        internal bool HandlePreviewInput(
            Page page,
            PointerEventArgs pointerInputEvent)
        {
            bool handled = false;

            if (pointerInputEvent.State == PointerInputState.Began)
            {
                var target = this.LightDismissTarget;
                if (null != target)
                {
                    this.LightDismissTarget = null;
                    target.OnLightDismiss();
                    handled = true;
                }
            }

            return handled;
        }

        internal bool HandlePreviewInput(
            View view,
            PointerEventArgs pointerInputEvent)
        {
            return this.HandleLightDismissEvent(view, pointerInputEvent);
        }

        private bool HandleLightDismissEvent(
            View view,
            PointerEventArgs pointerInputEvent)
        {
            bool handled = false;

            if (pointerInputEvent.State == PointerInputState.Began)
            {
                var target = this.LightDismissTarget;
                if ((null != target) && !target.Contains(view))
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

        /// <summary>
        /// Measures a single line of text as the platform draws it with
        /// <see cref="IDrawContext.DrawText"/>, for sizing owner-drawn controls.
        /// </summary>
        public Size MeasureText(
            string text,
            TextFormat textFormat)
        {
            if (null == textFormat)
            {
                throw new ArgumentNullException(nameof(textFormat));
            }

            return this.Platform.MeasureText(text ?? string.Empty, textFormat);
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
    }
}
