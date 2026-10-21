using System;
using XForms.Platform;

namespace XForms.iOS.Renderers
{
    public class ApplicationRenderer : IApplicationRenderer
    {
        private Application _application;
        private Frame _rootFrame;
        private ApplicationHost _applicationHost;
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
            this._applicationHost = ApplicationHost.For(application);
            this._nativeApplicationDelegate = nativeApplication;

            nativeApplication.Suspending += Application_Suspending;
            nativeApplication.Resuming += Application_Resuming;

            // Increased contrast changes while the app runs; dark mode is reread
            // when the app becomes active
            Foundation.NSNotificationCenter.DefaultCenter.AddObserver(
                UIKit.UIView.DarkerSystemColorsStatusDidChangeNotification,
                (notification) => this._applicationHost.SystemThemeChanged());

            // The text size (Dynamic Type)
            Foundation.NSNotificationCenter.DefaultCenter.AddObserver(
                UIKit.UIApplication.ContentSizeCategoryChangedNotification,
                (notification) => this._applicationHost.SystemThemeChanged());
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

        public ThemeMode SystemThemeMode
        {
            get
            {
                if (UIKit.UIAccessibility.DarkerSystemColorsEnabled)
                {
                    return ThemeMode.HighContrast;
                }

                return (UIKit.UITraitCollection.CurrentTraitCollection.UserInterfaceStyle == UIKit.UIUserInterfaceStyle.Dark)
                    ? ThemeMode.Dark
                    : ThemeMode.Light;
            }
        }

        public Color? SystemAccentColor
        {
            get
            {
                // iOS has no system-wide accent color; apps choose their own tint
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

        // Dynamic Type: the body text size over its default
        public float SystemTextScale => (float)UIKit.UIFontMetrics.DefaultMetrics.GetScaledValue(1);

        public string WindowTitle
        {
            set
            {
                var scene = this._nativeApplicationDelegate.Window?.WindowScene;
                if (null != scene)
                {
                    scene.Title = value;
                }
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

        public bool ExtendsIntoSafeArea
        {
            set
            {
                // The current page's view keeps its content in the safe area
                if (this._rootFrame?.CurrentPage?.Renderer?.NativeElement is NativeLayoutView page)
                {
                    page.RespectsSafeArea = !value;
                    page.InvalidateContentLayout();
                }
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
            this._applicationHost.SystemThemeChanged();
            this._applicationHost.Resume();
        }

        public void Exit()
        {
            throw new NotImplementedException();
        }

        public void ApplyTheme(
            Theme theme)
        {
            // System views (keyboards, menus) follow the app's mode
            var window = this._nativeApplicationDelegate.Window;
            if (null != window)
            {
                window.OverrideUserInterfaceStyle = (theme.Mode == ThemeMode.Light) ? UIKit.UIUserInterfaceStyle.Light : UIKit.UIUserInterfaceStyle.Dark;
                window.TintColor = theme.Colors.Accent.ToUIColor();
            }
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
            this._rootFrame = frame;
            var frameRenderer = (FrameRenderer)frame.Renderer;

            this._nativeApplicationDelegate.SetRootViewController(frameRenderer.ViewController);
        }

        public void OnNavigationComplete()
        {
        }
    }
}
