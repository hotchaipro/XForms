using System;

namespace XForms.iOS.Renderers
{
    public class ApplicationRenderer : IApplicationRenderer
    {
        private Application _application;
        private IApplicationDelegate _applicationDelegate;
        private NativeApplicationDelegate _nativeApplicationDelegate;
        //private Color? _titleBarAccentColor, _titleBarForegroundColor;

        public ApplicationRenderer(
            XForms.Application application,
            NativeApplicationDelegate nativeApplication)
        {
            if (null == application)
            {
                throw new ArgumentNullException(nameof(application));
            }

            if (null == nativeApplication)
            {
                throw new ArgumentNullException(nameof(nativeApplication));
            }

            this._application = application;
            this._applicationDelegate = application;
            this._nativeApplicationDelegate = nativeApplication;

            nativeApplication.Suspending += Application_Suspending;
            nativeApplication.Resuming += Application_Resuming;
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
                Foundation.NSRunLoop.Main.BeginInvokeOnMainThread(action);
            }
        }

        public void RequestFrame(
            Action<TimeSpan> onFrame)
        {
            // A display link fires once, as the next frame starts
            CoreAnimation.CADisplayLink link = null;
            link = CoreAnimation.CADisplayLink.Create(() =>
            {
                link.Invalidate();
                onFrame(TimeSpan.FromSeconds(link.Timestamp));
            });
            link.AddToRunLoop(Foundation.NSRunLoop.Main, Foundation.NSRunLoopMode.Common);
        }

        public Version GetVersion()
        {
            var versionString = Foundation.NSBundle.MainBundle.ObjectForInfoDictionary("CFBundleShortVersionString")?.ToString();

            Version version;
            if (!Version.TryParse(versionString, out version))
            {
                version = new Version(0, 0);
            }

            return version;
        }

        public void SetRootFrame(
            Frame frame)
        {
            var frameRenderer = (FrameRenderer)frame.Renderer;

            this._nativeApplicationDelegate.SetRootViewController(frameRenderer.ViewController);
        }

        public void OnNavigationComplete()
        {
        }
    }
}
