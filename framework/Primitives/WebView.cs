using System;
using XForma.Platform;

namespace XForma.Primitives
{
    /// <summary>
    /// What a <see cref="WebView"/> finished loading.
    /// </summary>
    public sealed class WebViewNavigatedEventArgs : EventArgs
    {
        internal WebViewNavigatedEventArgs(
            Uri uri,
            bool isSuccess)
        {
            this.Uri = uri;
            this.IsSuccess = isSuccess;
        }

        /// <summary>
        /// The page's address, or null for HTML given to
        /// <see cref="WebView.NavigateToString"/>.
        /// </summary>
        public Uri Uri
        {
            get;
        }

        /// <summary>
        /// Whether it loaded. The browser doesn't tell a page in a frame what
        /// happened, so there it's always true.
        /// </summary>
        public bool IsSuccess
        {
            get;
        }
    }

    /// <summary>
    /// Shows web pages, in the platform's own web view: WebView2 on Windows,
    /// <c>android.webkit.WebView</c> on Android, <c>WKWebView</c> on iOS and an
    /// <c>iframe</c> in the browser, where a site can refuse to appear in a frame.
    /// An Android app needs the <c>INTERNET</c> permission. Windows needs the
    /// WebView2 runtime, which comes with Windows 11; without it, the view stays
    /// empty and <see cref="FrameworkLog"/> says why.
    /// </summary>
    public class WebView : NativeViewHost
    {
        private Uri? _source;
        private string? _html;

        /// <summary>
        /// Raised when a page has finished loading.
        /// </summary>
        public event EventHandler<WebViewNavigatedEventArgs>? Navigated;

        /// <summary>
        /// The page's address: setting it goes there, and it follows the user's
        /// navigation.
        /// </summary>
        public Uri? Source
        {
            get
            {
                return this._source;
            }

            set
            {
                if (value == this._source)
                {
                    return;
                }

                this._source = value;
                this._html = null;
                if (null != value)
                {
                    this.Native?.Navigate(value);
                }

                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Shows the HTML, with no address of its own.
        /// </summary>
        public void NavigateToString(
            string html)
        {
            ArgumentNullException.ThrowIfNull(html);
            this._html = html;
            this._source = null;
            this.Native?.NavigateToString(html);
            this.OnPropertyChanged(nameof(this.Source));
        }

        /// <summary>
        /// Whether there's a page to go back to.
        /// </summary>
        public bool CanGoBack
        {
            get
            {
                return this.Native?.CanGoBack ?? false;
            }
        }

        /// <summary>
        /// Whether there's a page to go forward to.
        /// </summary>
        public bool CanGoForward
        {
            get
            {
                return this.Native?.CanGoForward ?? false;
            }
        }

        public void GoBack()
        {
            this.Native?.GoBack();
        }

        public void GoForward()
        {
            this.Native?.GoForward();
        }

        public void Reload()
        {
            this.Native?.Reload();
        }

        private IWebViewRenderer? Native
        {
            get
            {
                return (IWebViewRenderer)this.CreatedRenderer;
            }
        }

        /// <summary>
        /// A page finished loading, as the platform reports it.
        /// </summary>
        [PlatformContract]
        internal void OnNativeNavigated(
            Uri uri,
            bool isSuccess)
        {
            if (uri != this._source)
            {
                this._source = uri;
                this.OnPropertyChanged(nameof(this.Source));
            }

            this.Navigated?.Invoke(this, new WebViewNavigatedEventArgs(uri, isSuccess));
        }

        internal override void OnRendererCreated()
        {
            base.OnRendererCreated();

            if (null != this._source)
            {
                this.Native.Navigate(this._source);
            }
            else if (null != this._html)
            {
                this.Native.NavigateToString(this._html);
            }
        }

        private protected override IElementRenderer CreateRenderer()
        {
            return this.Application.Platform.CreateWebViewRenderer(this);
        }
    }
}
