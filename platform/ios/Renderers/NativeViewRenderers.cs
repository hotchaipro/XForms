using System;
using Foundation;
using UIKit;
using WebKit;
using XForma.Primitives;
using XForma.Platform;

namespace XForma.iOS.Renderers
{
    /// <summary>
    /// A view of the app's own, which takes the room layout gives it.
    /// </summary>
    internal class NativeViewHostRenderer : ViewRenderer<UIView>
    {
        protected NativeViewHostRenderer(
            View view,
            bool isCreatedBySubclass)
            : base(view)
        {
        }

        public NativeViewHostRenderer(
            NativeViewHost host)
            : base(host)
        {
            this.SetNativeElement((host.CreateNativeView() as UIView)
                ?? throw new InvalidOperationException("On iOS, a NativeViewHost's function returns a UIKit.UIView."));
        }

        public override Size MeasureNative(
            Size availableSize)
        {
            return new Size(
                float.IsInfinity(availableSize.Width) ? 0 : availableSize.Width,
                float.IsInfinity(availableSize.Height) ? 0 : availableSize.Height);
        }
    }

    internal sealed class WebViewRenderer : NativeViewHostRenderer, IWebViewRenderer
    {
        private readonly WKWebView _webView;

        // WKWebView holds its delegate weakly
        private readonly Delegate _delegate;

        public WebViewRenderer(
            XForma.Primitives.WebView view)
            : base(view, isCreatedBySubclass: true)
        {
            this._webView = new WKWebView(CoreGraphics.CGRect.Empty, new WKWebViewConfiguration());
            this._delegate = new Delegate(view);
            this._webView.NavigationDelegate = this._delegate;
            this.SetNativeElement(this._webView);
        }

        public void Navigate(
            Uri uri)
        {
            this._webView.LoadRequest(new NSUrlRequest(new NSUrl(uri.AbsoluteUri)));
        }

        public void NavigateToString(
            string html)
        {
            this._webView.LoadHtmlString(html, (NSUrl)null);
        }

        public bool CanGoBack => this._webView.CanGoBack;

        public bool CanGoForward => this._webView.CanGoForward;

        public void GoBack() => this._webView.GoBack();

        public void GoForward() => this._webView.GoForward();

        public void Reload() => this._webView.Reload();

        private sealed class Delegate : WKNavigationDelegate
        {
            private readonly XForma.Primitives.WebView _view;

            public Delegate(
                XForma.Primitives.WebView view)
            {
                this._view = view;
            }

            private void Report(
                WKWebView webView,
                bool isSuccess)
            {
                string address = webView.Url?.AbsoluteString;
                Uri uri = (Uri.TryCreate(address, UriKind.Absolute, out var parsed) && (parsed.Scheme != "about")) ? parsed : null;
                this._view.OnNativeNavigated(uri, isSuccess);
            }

            public override void DidFinishNavigation(
                WKWebView webView,
                WKNavigation navigation)
            {
                this.Report(webView, isSuccess: true);
            }

            public override void DidFailNavigation(
                WKWebView webView,
                WKNavigation navigation,
                NSError error)
            {
                this.Report(webView, isSuccess: false);
            }

            public override void DidFailProvisionalNavigation(
                WKWebView webView,
                WKNavigation navigation,
                NSError error)
            {
                this.Report(webView, isSuccess: false);
            }
        }
    }
}
