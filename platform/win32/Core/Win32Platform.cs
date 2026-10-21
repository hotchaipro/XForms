using System;
using System.Diagnostics;
using System.Reflection;
using Windows.Win32;
using XForms.Controls;
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

        public IApplicationRenderer CreateApplicationRenderer(Application application) => new ApplicationRenderer(this._host, application);

        public IPageRenderer CreatePageRenderer(Page page) => new PageRenderer(page);

        public IPanelRenderer CreatePanelRenderer(Panel layout) => new PanelRenderer(layout);

        public ITextBlockRenderer CreateTextBlockRenderer(TextBlock label) => new TextBlockRenderer(label);

        public IContentControlRenderer CreateContentControlRenderer(ContentControl customControl) => new ContentControlRenderer(customControl);

        public IBitmapRenderer CreateBitmapRenderer(Bitmap bitmap) => new BitmapRenderer(bitmap);

        public IFrameRenderer CreateFrameRenderer(Frame frame) => new FrameRenderer(frame);

        public IListViewRenderer CreateListViewRenderer(ListView listView) => new ListViewRenderer(listView);

        public ITextBoxRenderer CreateTextBoxRenderer(TextBox textEntry) => new TextBoxRenderer(textEntry);

        public IUserControlRenderer CreateUserControlRenderer(UserControl userControl) => new UserControlRenderer(userControl);

        public IImageRenderer CreateImageRenderer(Image image) => new ImageRenderer(image);

        public IScrollViewerRenderer CreateScrollViewerRenderer(ScrollViewer scrollView) => new ScrollViewerRenderer(scrollView);

        public IPopupRenderer CreatePopupRenderer(Popup popup) => new PopupRenderer(popup);

        public void NavigateToUri(
            Uri uri)
        {
            Process.Start(new ProcessStartInfo(uri.AbsoluteUri) { UseShellExecute = true });
        }

        public PlatformServices CreateServices()
        {
            return Win32Services.Create();
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
            IApplicationDelegate application)
        {
            this._host = host;
            host.SystemThemeChanged += application.NotifySystemThemeChanged;
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
            SystemTheme.ApplyToWindow(this._host.Hwnd, theme.Mode);
        }

        public Version GetVersion()
        {
            return Assembly.GetEntryAssembly()?.GetName().Version ?? new Version(0, 0);
        }
    }

    /// <summary>
    /// Runs an XForms app in a Win32 window.
    /// </summary>
    public static class Win32Application
    {
        public static void Run(
            string title,
            Func<IPlatform, Application> createApplication)
        {
            Win32Host.Run(title, createApplication);
        }
    }
}
