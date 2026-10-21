using System;

namespace XForms.Android.Renderers
{
    public class ApplicationRenderer : IApplicationRenderer
    {
        private Application _application;
        private IApplicationDelegate _applicationDelegate;
        private AndroidApplicationActivity _androidApplication;

        public ApplicationRenderer(
            XForms.Application application,
            AndroidApplicationActivity androidApplication)
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
            this._applicationDelegate = application;
            this._androidApplication = androidApplication;

            androidApplication.Suspending += Application_Suspending;
            androidApplication.Resuming += Application_Resuming;
            androidApplication.ConfigurationChanged += (sender, e) => this._applicationDelegate.NotifySystemThemeChanged();
        }

        public AndroidApplicationActivity Activity
        {
            get
            {
                return this._androidApplication;
            }
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

            // The status bar takes the title bar colors; the navigation bar follows the theme
            this._androidApplication.SetStatusBarColors(
                this._titleBarAccentColor ?? theme.Colors.Background,
                this._titleBarForegroundColor.HasValue ? !IsLightColor(this._titleBarForegroundColor.Value) : isLightTheme);

            this._androidApplication.SetNavigationBarColors(theme.Colors.Background, isLightTheme);
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
                var activity = this._androidApplication;

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
                    var color = this._androidApplication.Resources.GetColor(global::Android.Resource.Color.SystemAccent1500, null);
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

        private void Application_Suspending(
            object sender,
            EventArgs e)
        {
            this._applicationDelegate.NotifySuspend();
        }

        private void Application_Resuming(
            object sender,
            EventArgs e)
        {
            this._applicationDelegate.NotifyResume();
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
        }
    }
}
