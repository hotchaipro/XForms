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
        }

        public AndroidApplicationActivity Activity
        {
            get
            {
                return this._androidApplication;
            }
        }

        public Color? TitleBarAccentColor
        {
            get;
            set;
        }

        public Color? TitleBarForegroundColor
        {
            get;
            set;
        }

        public Color AccentColor
        {
            get;
            set;
        }

        public Color ForegroundColor
        {
            get
            {
                return this.Theme.ForegroundColor;
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
            throw new NotImplementedException();
        }

        public AppTheme Theme
        {
            get;
            private set;
        }

        public void ApplyTheme(
            AppTheme theme)
        {
            this.Theme = theme;

            this.AccentColor = theme.AccentColor;
        }

        public void InvokeOnUIThreadAsync(
            Action action)
        {
            if (null != action)
            {
                this._androidApplication.RunOnUiThread(action);
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
            this._androidApplication.SetContentView(nativeFrame);
        }

        public void OnNavigationComplete()
        {
        }
    }
}
