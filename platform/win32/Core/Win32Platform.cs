using System;
using System.Diagnostics;
using System.Reflection;
using Windows.Win32;
using XForms.Primitives;
using XForms.Platform;
using XForms.Win32.Renderers;

namespace XForms.Win32
{
    /// <summary>
    /// PROTOTYPE platform: implements the renderers the XForms test page needs.
    /// </summary>
    internal sealed class Win32Platform : IPlatform
    {
        private readonly Win32Host _host;

        public Win32Platform(
            Win32Host host)
        {
            this._host = host ?? throw new ArgumentNullException(nameof(host));
        }

        // NOTE: Win32 draws lightweight views itself, in its composition tree
        public bool UsesLightweightRenderers => false;

        public IApplicationRenderer CreateApplicationRenderer(Application application) => new ApplicationRenderer(this._host, application);

        public IContainerRenderer CreateContainerRenderer(View view) => new ContainerRenderer(view);

        public ITextBlockRenderer CreateTextBlockRenderer(TextBlock label) => new TextBlockRenderer(label);

        public IBitmapRenderer CreateBitmapRenderer(Bitmap bitmap) => new BitmapRenderer(bitmap);

        public IFrameRenderer CreateFrameRenderer(Frame frame) => new FrameRenderer(frame);

        public IItemsViewRenderer CreateItemsViewRenderer(ItemsView listView) => new ItemsViewRenderer(listView);

        public ITextBoxRenderer CreateTextBoxRenderer(TextBox textEntry) => new TextBoxRenderer(textEntry);

        public IImageRenderer CreateImageRenderer(Image image) => new ImageRenderer(image);

        public IScrollViewerRenderer CreateScrollViewerRenderer(ScrollViewer scrollView) => new ScrollViewerRenderer(scrollView);

        public IPopupRenderer CreatePopupRenderer(Popup popup) => new PopupRenderer(popup);

        public void NavigateToUri(
            Uri uri)
        {
            Process.Start(new ProcessStartInfo(uri.AbsoluteUri) { UseShellExecute = true });
        }

        public ITextLayoutRenderer CreateTextLayout(
            TextLayout layout)
        {
            return new TextLayoutRenderer(layout);
        }

        public void RegisterFont(
            string familyName,
            string fileName)
        {
            TextLayouts.RegisterFont(familyName, fileName);
        }
    }

    internal sealed class ApplicationRenderer : IApplicationRenderer
    {
        private readonly Win32Host _host;

        public ApplicationRenderer(
            Win32Host host,
            Application app)
        {
            this._host = host;
            var application = ApplicationHost.For(app);
            host.SystemThemeChanged += application.SystemThemeChanged;
            host.Suspending += application.Suspend;
            host.Resuming += application.Resume;
        }

        public Color? TitleBarAccentColor { get; set; }

        public Color? TitleBarForegroundColor { get; set; }

        public ThemeMode SystemThemeMode => SystemTheme.Mode;

        public Color? SystemAccentColor => SystemTheme.AccentColor;

        public ColorScheme SystemHighContrastColors => SystemTheme.HighContrastColors;

        public float SystemTextScale => SystemTheme.TextScale;

        public string WindowTitle
        {
            set
            {
                PInvoke.SetWindowText(this._host.Hwnd, value ?? string.Empty);
            }
        }

        public bool CanResizeWindow => true;

        public bool CanOpenWindows => true;

        public IWindowRenderer OpenWindow(
            Window window,
            Frame frame,
            string title,
            Size size)
        {
            var host = Win32Host.OpenWindow(title, size);
            host.Closed = () => ApplicationHost.Current.WindowClosed(window);
            host.SetRoot((ViewRenderer)frame.Renderer);
            SystemTheme.ApplyToWindow(host.Hwnd, Application.Current.Theme.Mode);
            return new WindowRenderer(host);
        }

        /// <summary>
        /// A window the app opened.
        /// </summary>
        private sealed class WindowRenderer : IWindowRenderer
        {
            private readonly Win32Host _host;

            public WindowRenderer(
                Win32Host host)
            {
                this._host = host;
            }

            public string Title
            {
                set
                {
                    PInvoke.SetWindowText(this._host.Hwnd, value ?? string.Empty);
                }
            }

            public Size MinimumSize
            {
                set
                {
                    this._host.MinimumClientSize = value;
                }
            }

            public void Resize(
                Size size)
            {
                this._host.ResizeClient(size);
            }

            public void Close()
            {
                PInvoke.DestroyWindow(this._host.Hwnd);
            }
        }

        // A desktop window has no system bars over it
        public bool ExtendsIntoSafeArea
        {
            set
            {
            }
        }

        public Size MinimumWindowSize
        {
            set
            {
                this._host.MinimumClientSize = value;
            }
        }

        public void ResizeWindow(
            Size size)
        {
            this._host.ResizeClient(size);
        }

        public void SetRootFrame(
            Frame frame)
        {
            this._host.SetRoot((ViewRenderer)frame.Renderer);
        }

        public void OnNavigationComplete()
        {
        }

        public void InvokeOnUIThreadAsync(
            Action action)
        {
            if (null != action)
            {
                this._host.Post(action);
            }
        }

        public void RequestFrame(
            Action<TimeSpan> onFrame)
        {
            // Wait for the compositor's next frame off the UI thread
            System.Threading.Tasks.Task.Run(() =>
            {
                PInvoke.DwmFlush();
                var now = TimeSpan.FromSeconds(System.Diagnostics.Stopwatch.GetTimestamp() / (double)System.Diagnostics.Stopwatch.Frequency);
                this._host.Post(() => onFrame(now));
            });
        }

        public void Exit()
        {
            PInvoke.PostQuitMessage(0);
        }

        public void ApplyTheme(
            Theme theme)
        {
            // The main window, and those the app opened
            SystemTheme.ApplyToWindow(this._host.Hwnd, theme.Mode);
            Win32Host.ApplyToOpenedWindows((hwnd) => SystemTheme.ApplyToWindow(hwnd, theme.Mode));
        }

        public Version GetVersion()
        {
            return Assembly.GetEntryAssembly()?.GetName().Version ?? new Version(0, 0);
        }
    }

    /// <summary>
    /// Runs an XForms app in a Win32 window, and gives native code the
    /// running app's window.
    /// </summary>
    public sealed class Win32Application
    {
        private readonly Win32Host _host;

        private Win32Application(
            Win32Host host)
        {
            this._host = host;
        }

        /// <summary>
        /// The running app's host, or null before <see cref="Run"/>.
        /// </summary>
        public static Win32Application Current
        {
            get;
            private set;
        }

        /// <summary>
        /// The app's main window (an <c>HWND</c>), such as to own a native
        /// dialog.
        /// </summary>
        public unsafe nint MainWindowHandle
        {
            get
            {
                return (nint)this._host.Hwnd.Value;
            }
        }

        /// <summary>
        /// Runs the app in a new window, and returns when the app exits. Call
        /// it from <c>Main</c>, on an STA thread.
        /// </summary>
        public static void Run(
            string title,
            Application application)
        {
            ArgumentNullException.ThrowIfNull(application);

            Win32Host.Run(title, application, (host) => Current = new Win32Application(host));
        }
    }
}
