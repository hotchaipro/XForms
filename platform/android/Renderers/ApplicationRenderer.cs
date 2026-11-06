using System;
using XForma.Platform;

namespace XForma.Android.Renderers
{
    internal class ApplicationRenderer : IApplicationRenderer
    {
        private Application _application;
        private ApplicationHost _applicationHost;
        private AndroidApplication _androidApplication;

        public ApplicationRenderer(
            XForma.Application application,
            AndroidApplication androidApplication)
        {
            if (null == application)
            {
                throw new ArgumentNullException(nameof(application));
            }

            if (null == androidApplication)
            {
                throw new ArgumentNullException(nameof(androidApplication));
            }

            this._application = application;
            this._applicationHost = ApplicationHost.For(application);

            // An exception on its way out to Java: the app can keep running
            global::Android.Runtime.AndroidEnvironment.UnhandledExceptionRaiser += (sender, e) =>
            {
                e.Handled = Application.ReportException(e.Exception);
            };
            this._androidApplication = androidApplication;

            androidApplication.Suspending += Application_Suspending;
            androidApplication.Resuming += Application_Resuming;
            androidApplication.ConfigurationChanged += (sender, e) => this._applicationHost.SystemThemeChanged();
            androidApplication.SafeAreaInsetsChanged += (bars, keyboard) => this._applicationHost.SafeAreaInsetsChanged(bars, keyboard);
        }



        private Color? _titleBarAccentColor, _titleBarForegroundColor;

        public Color? TitleBarAccentColor
        {
            get
            {
                return this._titleBarAccentColor;
            }

            set
            {
                this._titleBarAccentColor = value;
                this.UpdateSystemBars();
            }
        }

        public Color? TitleBarForegroundColor
        {
            get
            {
                return this._titleBarForegroundColor;
            }

            set
            {
                this._titleBarForegroundColor = value;
                this.UpdateSystemBars();
            }
        }

        private void UpdateSystemBars()
        {
            var theme = this._theme ?? this._application.Theme;
            bool isLightTheme = (theme.Mode == ThemeMode.Light);

            // The status bar takes the title bar colors; the navigation bar
            // the page's bottom color, or the theme's background
            this._androidApplication.SetStatusBarColors(
                this._titleBarAccentColor ?? theme.Colors.Background,
                this._titleBarForegroundColor.HasValue ? !IsLightColor(this._titleBarForegroundColor.Value) : isLightTheme);

            this._androidApplication.SetNavigationBarColors(
                this._bottomSafeAreaColor ?? theme.Colors.Background,
                this._bottomSafeAreaColor.HasValue ? IsLightColor(this._bottomSafeAreaColor.Value) : isLightTheme);
        }

        private static bool IsLightColor(
            Color color)
        {
            // Perceived brightness
            return ((0.299 * color.R) + (0.587 * color.G) + (0.114 * color.B)) > 160;
        }

        public ThemeMode SystemThemeMode
        {
            get
            {
                var activity = this._androidApplication.ActivityOrContext;

                // Android 14 added a contrast setting; "high" is 1
                if (global::Android.OS.Build.VERSION.SdkInt >= global::Android.OS.BuildVersionCodes.UpsideDownCake)
                {
                    var uiModeManager = (global::Android.App.UiModeManager)activity.GetSystemService(global::Android.Content.Context.UiModeService);
                    if ((null != uiModeManager) && (uiModeManager.Contrast >= 0.99f))
                    {
                        return ThemeMode.HighContrast;
                    }
                }

                var night = activity.Resources.Configuration.UiMode & global::Android.Content.Res.UiMode.NightMask;
                return (night == global::Android.Content.Res.UiMode.NightYes) ? ThemeMode.Dark : ThemeMode.Light;
            }
        }

        public Color? SystemAccentColor
        {
            get
            {
                // Android 12's dynamic color: the wallpaper's accent
                if (global::Android.OS.Build.VERSION.SdkInt >= global::Android.OS.BuildVersionCodes.S)
                {
                    var color = this._androidApplication.ActivityOrContext.Resources.GetColor(global::Android.Resource.Color.SystemAccent1500, null);
                    return NativeConversions.FromAndroidColor(color);
                }

                return null;
            }
        }

        public ColorScheme SystemHighContrastColors
        {
            get
            {
                return null;
            }
        }

        // NOTE: Android applies the font scale itself: text sizes are in
        // scaled pixels
        public float SystemTextScale => this._androidApplication.ActivityOrContext.Resources.Configuration.FontScale;

        public bool IsTouchPrimary => true;

        // An edit text's own size (the theme's), in scaled pixels
        private float _textEntryFontSize;

        // As drawn text measures it
        public float TextEntryLineHeight => 0;

        public float TextEntryFontSize
        {
            get
            {
                if (this._textEntryFontSize <= 0)
                {
                    var context = this._androidApplication.ActivityOrContext;
                    var metrics = context.Resources.DisplayMetrics;
                    using (var edit = new global::Android.Widget.EditText(context))
                    {
                        this._textEntryFontSize = edit.TextSize / (metrics.Density * context.Resources.Configuration.FontScale);
                    }
                }

                return this._textEntryFontSize;
            }
        }

        public string WindowTitle
        {
            set
            {
                // Shown in the recent apps list
                this._androidApplication.Title = value;
            }
        }

        public bool CanResizeWindow => false;

        // A phone shows one window
        public bool CanOpenWindows => false;

        public IWindowRenderer OpenWindow(
            Window window,
            Frame frame,
            string title,
            Size size)
        {
            throw new PlatformNotSupportedException("This platform shows one window.");
        }


        public void EndTextEntry()
        {
            this._androidApplication.EndTextEntry();
        }

        private Color? _bottomSafeAreaColor;

        public Color? BottomSafeAreaColor
        {
            set
            {
                this._bottomSafeAreaColor = value;
                this.UpdateSystemBars();
            }
        }

        public Size MinimumWindowSize
        {
            set
            {
            }
        }

        public void ResizeWindow(
            Size size)
        {
        }

        private void Application_Suspending(
            object sender,
            EventArgs e)
        {
            this._applicationHost.Suspend();
        }

        private void Application_Resuming(
            object sender,
            EventArgs e)
        {
            this._applicationHost.Resume();
        }

        public bool UsesSystemDragGesture => false;

        public void StartDrag(
            View source,
            DataPackage data,
            Point position)
        {
            float density = FrameRenderer.RootNativeFrame.Resources.DisplayMetrics.Density;
            AndroidDragDrop.Start(FrameRenderer.RootNativeFrame, source, data, position, density);
        }

        public void Exit()
        {
            this._androidApplication.Finish();
        }

        private Theme _theme;

        public void ApplyTheme(
            Theme theme)
        {
            this._theme = theme;
            this.UpdateSystemBars();
        }

        public void InvokeOnUIThreadAsync(
            Action action)
        {
            if (null != action)
            {
                this._androidApplication.RunOnUiThread(action);
            }
        }

        public void RequestFrame(
            Action<TimeSpan> onFrame)
        {
            // The UI thread's choreographer calls back as the next frame starts
            global::Android.Views.Choreographer.Instance.PostFrameCallback(new FrameCallback(onFrame));
        }

        private sealed class FrameCallback : Java.Lang.Object, global::Android.Views.Choreographer.IFrameCallback
        {
            private readonly Action<TimeSpan> _onFrame;

            public FrameCallback(
                Action<TimeSpan> onFrame)
            {
                this._onFrame = onFrame;
            }

            public void DoFrame(
                long frameTimeNanos)
            {
                this._onFrame(TimeSpan.FromTicks(frameTimeNanos / 100));
                this.Dispose();
            }
        }

        public Version GetVersion()
        {
            Version version = new Version(0, 0);

            var packageInfo = global::Android.App.Application.Context.ApplicationContext.PackageManager.GetPackageInfo(
                global::Android.App.Application.Context.ApplicationContext.PackageName, (global::Android.Content.PM.PackageInfoFlags)0);
            if (null != packageInfo)
            {
                version = new Version(packageInfo.VersionName);
            }

            if ((version.Build < 0) || (version.Revision < 0))
            {
                version = new Version(version.Major, version.Minor, 0, 0);
            }

            return version;
        }

        public void SetRootFrame(
            Frame frame)
        {
            var nativeFrame = frame.Renderer.NativeElement as global::Android.Views.View;
            this._androidApplication.SetRootView(nativeFrame);

            this.UpdateSystemBars();
        }

        public void OnNavigationComplete()
        {
            // Back goes back in the app while it can; otherwise the system
            // takes it (and leaves the app)
            this._androidApplication.SetBackHandler(this._application.CanGoBack ? ApplicationHost.For(this._application).Back : null);
        }
    }
}
