using System;
using XForma.Platform;

namespace XForma.iOS.Renderers
{
    internal class ApplicationRenderer : IApplicationRenderer
    {
        private Application _application;
        private Frame _rootFrame;
        private ApplicationHost _applicationHost;
        private IosApplication _nativeApplicationDelegate;
        private Color? _titleBarAccentColor, _titleBarForegroundColor;
        private Theme _theme;

        public ApplicationRenderer(
            XForma.Application application,
            IosApplication nativeApplication)
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

            // Dark mode and increased contrast change while the app runs
            nativeApplication.SystemStyleChanged += (sender, e) => this._applicationHost.SystemThemeChanged();
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
            get
            {
                return this._titleBarAccentColor;
            }

            set
            {
                this._titleBarAccentColor = value;
                this.UpdateStatusBar();
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
                this.UpdateStatusBar();
            }
        }

        // The status bar takes the title bar colors, as on Android: its band
        // the accent (or the theme's background), its text light or dark
        private void UpdateStatusBar()
        {
            if (this._rootFrame?.Renderer is FrameRenderer frameRenderer)
            {
                var theme = this._theme ?? this._application.Theme;
                frameRenderer.SetStatusBarStyle(
                    this._titleBarForegroundColor.HasValue ? IsLightColor(this._titleBarForegroundColor.Value) : (theme.Mode != ThemeMode.Light));
            }
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
                if (UIKit.UIAccessibility.DarkerSystemColorsEnabled)
                {
                    return ThemeMode.HighContrast;
                }

                // The scene's: the window's and its views' are the mode the app
                // asked for (see ApplyTheme)
                var traits = this._nativeApplicationDelegate.WindowScene?.TraitCollection ?? UIKit.UITraitCollection.CurrentTraitCollection;
                return (traits.UserInterfaceStyle == UIKit.UIUserInterfaceStyle.Dark)
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

        public bool IsTouchPrimary => true;

        // A text box's is the body text style, 17 points before Dynamic Type
        public float TextEntryFontSize => 17;

        // A text field's own height for the body font, at the text size it
        // was measured for
        private float _textEntryLineHeight;
        private nfloat _textEntryLineHeightPointSize;

        public float TextEntryLineHeight
        {
            get
            {
                var font = UIKit.UIFont.GetPreferredFontForTextStyle(UIKit.UIFontTextStyle.Body);
                if (font.PointSize != this._textEntryLineHeightPointSize)
                {
                    using (var textField = new UIKit.UITextField() { Font = font, BorderStyle = UIKit.UITextBorderStyle.None })
                    {
                        this._textEntryLineHeight = (float)Math.Ceiling(textField.SizeThatFits(new CoreGraphics.CGSize(10000, 10000)).Height);
                    }

                    this._textEntryLineHeightPointSize = font.PointSize;
                }

                return this._textEntryLineHeight;
            }
        }

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


        // The first responder resigns if it's a text field (the root then
        // takes keyboard focus), hiding the keyboard
        public void EndTextEntry()
        {
            var window = this._nativeApplicationDelegate.Window;
            if (Renderers.FrameRenderer.FocusedView(window) is XForma.Primitives.TextBox)
            {
                UIKit.UIApplication.SharedApplication.SendAction(new ObjCRuntime.Selector("resignFirstResponder"), null, null, null);
            }
        }

        // The page draws its bottom color; the home indicator adapts itself
        public Color? BottomSafeAreaColor
        {
            set
            {
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

            // Going into the background: the next refresh is scheduled
            BackgroundRefresh.Schedule(this._applicationHost.Application);
        }

        private void Application_Resuming(
            object sender,
            EventArgs e)
        {
            this._applicationHost.SystemThemeChanged();
            this._applicationHost.Resume();
        }

        // iOS doesn't let an app close itself
        public void Exit()
        {
        }

        public void ApplyTheme(
            Theme theme)
        {
            this._theme = theme;
            this.UpdateStatusBar();

            // System views (keyboards, menus) follow the app's mode: the
            // system's, if the app follows it
            var window = this._nativeApplicationDelegate.Window;
            if (null != window)
            {
                bool isFollowing = (null == this._application.RequestedThemeMode) && (theme.Mode != ThemeMode.HighContrast);
                window.OverrideUserInterfaceStyle = isFollowing
                    ? UIKit.UIUserInterfaceStyle.Unspecified
                    : (theme.Mode == ThemeMode.Light) ? UIKit.UIUserInterfaceStyle.Light : UIKit.UIUserInterfaceStyle.Dark;
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
            this.UpdateStatusBar();
        }

        public void OnNavigationComplete()
        {
        }
    }
}
