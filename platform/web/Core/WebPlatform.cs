using System;
using System.Reflection;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;
using XForms.Primitives;
using XForms.Platform;
using XForms.Web.Renderers;

namespace XForms.Web
{
    [SupportedOSPlatform("browser")]
    internal sealed class WebPlatform : IPlatform
    {
        private readonly WebHost _host;

        public WebPlatform(
            WebHost host)
        {
            this._host = host;
        }

        public bool UsesLightweightRenderers => true;

        public IApplicationRenderer CreateApplicationRenderer(Application application) => new ApplicationRenderer(this._host, application);

        public IContainerRenderer CreateContainerRenderer(View view) => new ContainerRenderer(view);

        public ITextBlockRenderer CreateTextBlockRenderer(TextBlock label)
        {
            return new TextBlockRenderer(label);
        }

        public IImageRenderer CreateImageRenderer(Image image) => new ImageRenderer(image);

        public IBitmapRenderer CreateBitmapRenderer(Bitmap bitmap) => new BitmapRenderer(bitmap);

        public IFrameRenderer CreateFrameRenderer(Frame frame) => new FrameRenderer(frame);

        public IItemsViewRenderer CreateItemsViewRenderer(ItemsView listView) => new ItemsViewRenderer(listView);

        public ITextBoxRenderer CreateTextBoxRenderer(TextBox textEntry) => new TextBoxRenderer(textEntry);

        public IScrollViewerRenderer CreateScrollViewerRenderer(ScrollViewer scrollView) => new ScrollViewerRenderer(scrollView);

        public IPopupRenderer CreatePopupRenderer(Popup popup) => new PopupRenderer(popup);

        public void NavigateToUri(
            Uri uri)
        {
            if (null != uri)
            {
                Js.OpenUrl(uri.AbsoluteUri);
            }
        }


        public ITextLayoutRenderer CreateTextLayout(TextLayout layout) => new WebTextLayout(layout);

        public void RegisterFont(
            string familyName,
            string fileName)
        {
            _ = Js.RegisterFontAsync(familyName, "Content/" + fileName.Replace('\\', '/'));
        }
    }

    [SupportedOSPlatform("browser")]
    internal sealed class ApplicationRenderer : IApplicationRenderer
    {
        private readonly WebHost _host;

        public ApplicationRenderer(
            WebHost host,
            Application app)
        {
            this._host = host;
            var application = ApplicationHost.For(app);
            host.ThemeChanged += application.SystemThemeChanged;
            host.Suspending += application.Suspend;
            host.Resuming += application.Resume;
        }

        public void SetRootFrame(
            Frame frame)
        {
            this._host.SetRoot((ViewRenderer)frame.Renderer);
        }

        public void OnNavigationComplete()
        {
            this._host.SyncHistory();
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
            this._host.RequestFrame(onFrame);
        }

        // A page can't close itself
        public void Exit()
        {
        }

        public void ApplyTheme(
            Theme theme)
        {
            Js.SetPageBackground(string.Format(
                System.Globalization.CultureInfo.InvariantCulture,
                "rgb({0},{1},{2})",
                theme.Colors.Background.R,
                theme.Colors.Background.G,
                theme.Colors.Background.B));
        }

        public ThemeMode SystemThemeMode
        {
            get
            {
                int state = Js.ThemeState();
                return ((state & 4) != 0) ? ThemeMode.HighContrast : ((state & 1) != 0) ? ThemeMode.Dark : ThemeMode.Light;
            }
        }

        public Color? SystemAccentColor => null;

        public ColorScheme SystemHighContrastColors => null;

        // NOTE: The browser zooms the page itself
        public float SystemTextScale => 1;

        public string WindowTitle
        {
            set
            {
                Js.SetTitle(value ?? string.Empty);
            }
        }

        public bool CanResizeWindow => false;

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

        public bool ExtendsIntoSafeArea
        {
            set
            {
            }
        }

        public bool CanOpenWindows => false;

        public IWindowRenderer OpenWindow(
            Window window,
            Frame frame,
            string title,
            Size size)
        {
            throw new PlatformNotSupportedException("A page shows one window.");
        }

        public Color? TitleBarAccentColor { get; set; }

        public Color? TitleBarForegroundColor { get; set; }

        public Version GetVersion()
        {
            return Assembly.GetEntryAssembly()?.GetName().Version ?? new Version(0, 0);
        }
    }

    /// <summary>
    /// Runs an XForms app in the page.
    /// </summary>
    [SupportedOSPlatform("browser")]
    public sealed class WebApplication
    {
        private WebApplication()
        {
        }

        /// <summary>
        /// The running app's host, or null before <see cref="RunAsync"/>.
        /// </summary>
        public static WebApplication Current
        {
            get;
            private set;
        }
        private static Task _initialization;

        /// <summary>
        /// Loads the platform's script into the page (once).
        /// </summary>
        internal static Task InitializeAsync()
        {
            return _initialization ??= Js.InitializeAsync();
        }

        /// <summary>
        /// Runs the app in the page. Call it once, from <c>Main</c>.
        /// </summary>
        public static async Task RunAsync(
            string title,
            Application application)
        {
            ArgumentNullException.ThrowIfNull(application);
            await InitializeAsync();

            var host = new WebHost();
            SynchronizationContext.SetSynchronizationContext(new WebHost.WebSynchronizationContext(host));
            Js.SetTitle(title ?? string.Empty);

            // The app's files, kept from earlier visits
            WebFiles.Restore();

            try
            {
                Current = new WebApplication();

                // Saved once the app has suspended (and saved its state)
                host.Suspending += WebFiles.Save;
                ApplicationHost.For(application).Start(new WebPlatform(host), WebHost.GetStartStack());
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"XForms: the app didn't start: {ex}");
                throw;
            }
        }
    }
}
