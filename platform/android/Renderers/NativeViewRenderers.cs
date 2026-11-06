using System;
using Android.Webkit;
using XForma.Primitives;
using XForma.Platform;
using AndroidView = global::Android.Views.View;
using AndroidWebView = global::Android.Webkit.WebView;

namespace XForma.Android.Renderers
{
    /// <summary>
    /// A view of the app's own, which takes the room layout gives it.
    /// </summary>
    internal class NativeViewHostRenderer : ViewRenderer<AndroidView>
    {
        protected NativeViewHostRenderer(
            global::Android.Content.Context context,
            View view)
            : base(context, view)
        {
        }

        public NativeViewHostRenderer(
            global::Android.Content.Context context,
            NativeViewHost host)
            : base(context, host)
        {
            this.SetNativeElement((host.CreateNativeView() as AndroidView)
                ?? throw new InvalidOperationException("On Android, a NativeViewHost's function returns an Android.Views.View."));
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
        private readonly AndroidWebView _webView;

        public WebViewRenderer(
            global::Android.Content.Context context,
            XForma.Primitives.WebView view)
            : base(context, (View)view)
        {
            this._webView = new AndroidWebView(context);

            // As on the other platforms
            this._webView.Settings.JavaScriptEnabled = true;
            this._webView.SetWebViewClient(new Client(view));
            this.SetNativeElement(this._webView);
        }

        public void Navigate(
            Uri uri)
        {
            this._webView.LoadUrl(uri.AbsoluteUri);
        }

        public void NavigateToString(
            string html)
        {
            this._webView.LoadDataWithBaseURL(null, html, "text/html", "utf-8", null);
        }

        public bool CanGoBack => this._webView.CanGoBack();

        public bool CanGoForward => this._webView.CanGoForward();

        public void GoBack() => this._webView.GoBack();

        public void GoForward() => this._webView.GoForward();

        public void Reload() => this._webView.Reload();

        private sealed class Client : WebViewClient
        {
            private readonly XForma.Primitives.WebView _view;
            private bool _hasFailed;

            public Client(
                XForma.Primitives.WebView view)
            {
                this._view = view;
            }

            public override void OnPageStarted(
                AndroidWebView view,
                string url,
                global::Android.Graphics.Bitmap favicon)
            {
                base.OnPageStarted(view, url, favicon);
                this._hasFailed = false;
            }

            public override void OnReceivedError(
                AndroidWebView view,
                IWebResourceRequest request,
                WebResourceError error)
            {
                base.OnReceivedError(view, request, error);
                this._hasFailed |= request?.IsForMainFrame ?? false;
            }

            public override void OnPageFinished(
                AndroidWebView view,
                string url)
            {
                base.OnPageFinished(view, url);

                // HTML given directly has no address (about:blank, or a data URL)
                Uri uri = (Uri.TryCreate(url, UriKind.Absolute, out var parsed) && (parsed.Scheme != "about") && (parsed.Scheme != "data")) ? parsed : null;
                this._view.OnNativeNavigated(uri, !this._hasFailed);
            }
        }
    }
}
