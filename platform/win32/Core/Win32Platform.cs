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

        public IApplicationRenderer CreateApplicationRenderer(Application application) => new ApplicationRenderer(this._host);

        public IPageRenderer CreatePageRenderer(Page page) => new PageRenderer(page);

        public ILayoutRenderer CreateLayoutRenderer(Layout layout) => new LayoutRenderer(layout);

        public ITextViewRenderer CreateLabelRenderer(TextView label) => new TextViewRenderer(label);

        public IContentControlRenderer CreateContentControlRenderer(ContentControl customControl) => new ContentControlRenderer(customControl);

        public IBitmapRenderer CreateBitmapRenderer(Bitmap bitmap) => new BitmapRenderer(bitmap);

        public IFrameRenderer CreateFrameRenderer(Frame frame) => new FrameRenderer(frame);

        public IListViewRenderer CreateListViewRenderer(ListView listView) => new ListViewRenderer(listView);

        public ITextEntryRenderer CreateTextEntryRenderer(TextEntry textEntry) => new TextEntryRenderer(textEntry);

        public IUserControlRenderer CreateUserControlRenderer(UserControl userControl) => new UserControlRenderer(userControl);

        public IImageRenderer CreateImageRenderer(Image image) => new ImageRenderer(image);

        public IScrollViewRenderer CreateScrollViewRenderer(ScrollView scrollView) => new ScrollViewRenderer(scrollView);

        public IPopupRenderer CreatePopupRenderer(Popup popup) => new PopupRenderer(popup);

        public void NavigateToUri(
            Uri uri)
        {
            Process.Start(new ProcessStartInfo(uri.AbsoluteUri) { UseShellExecute = true });
        }

        public Size MeasureText(
            string text,
            TextFormat textFormat)
        {
            return TextResources.MeasureText(text, textFormat.FontSize, textFormat.FontWeight);
        }
    }

    internal sealed class ApplicationRenderer : IApplicationRenderer
    {
        private readonly Win32Host _host;

        public ApplicationRenderer(
            Win32Host host)
        {
            this._host = host;
        }

        public Color? TitleBarAccentColor { get; set; }

        public Color? TitleBarForegroundColor { get; set; }

        public Color AccentColor { get; set; }

        public Color ForegroundColor
        {
            get
            {
                return this.Theme?.ForegroundColor ?? Colors.Black;
            }
        }

        public AppTheme Theme { get; private set; }

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

        public void Exit()
        {
            PInvoke.PostQuitMessage(0);
        }

        public void ApplyTheme(
            AppTheme theme)
        {
            this.Theme = theme;
            this.AccentColor = theme.AccentColor;
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
