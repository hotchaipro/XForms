using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.JavaScript;
using System.Runtime.Versioning;
using XForma.Primitives;
using XForma.Platform;

namespace XForma.Web.Renderers
{
    /// <summary>
    /// An element of the app's own, filling a positioned one, which takes the
    /// room layout gives it.
    /// </summary>
    [SupportedOSPlatform("browser")]
    internal sealed class NativeViewHostRenderer : ViewRenderer
    {
        private readonly JSObject _element;

        public NativeViewHostRenderer(
            NativeViewHost host)
            : base(host, "div", isContainer: false)
        {
            this._element = (host.CreateNativeView() as JSObject)
                ?? throw new InvalidOperationException("In the browser, a NativeViewHost's function returns an element (a JSObject).");
            Js.AdoptChild(this.Id, this._element);
        }

        public override object NativeElement
        {
            get
            {
                return this._element;
            }
        }

        public override Size MeasureNative(
            Size availableSize)
        {
            return new Size(
                float.IsInfinity(availableSize.Width) ? 0 : availableSize.Width,
                float.IsInfinity(availableSize.Height) ? 0 : availableSize.Height);
        }
    }

    /// <summary>
    /// A frame. A page in another frame keeps its history to itself, so going
    /// back and forward covers the pages the app asked for.
    /// </summary>
    [SupportedOSPlatform("browser")]
    internal sealed class WebViewRenderer : ViewRenderer, IWebViewRenderer
    {
        private readonly WebView _webView;
        private readonly List<(Uri Uri, string Html)> _history = new List<(Uri, string)>();
        private int _current = -1;

        public WebViewRenderer(
            WebView webView)
            : base(webView, "iframe", isContainer: false)
        {
            this._webView = webView;
            Js.SetStyle(this.Id, "border", "none");
            Js.ListenLoad(this.Id);
        }

        public void Navigate(
            Uri uri)
        {
            this.Push(uri, null);
        }

        public void NavigateToString(
            string html)
        {
            this.Push(null, html);
        }

        private void Push(
            Uri uri,
            string html)
        {
            this._history.RemoveRange(this._current + 1, this._history.Count - (this._current + 1));
            this._history.Add((uri, html));
            this._current = this._history.Count - 1;
            this.Show();
        }

        private void Show()
        {
            var (uri, html) = this._history[this._current];
            Js.SetAttribute(this.Id, "srcdoc", html);
            Js.SetAttribute(this.Id, "src", uri?.AbsoluteUri);
        }

        public bool CanGoBack => this._current > 0;

        public bool CanGoForward => this._current < this._history.Count - 1;

        public void GoBack()
        {
            if (this.CanGoBack)
            {
                this._current -= 1;
                this.Show();
            }
        }

        public void GoForward()
        {
            if (this.CanGoForward)
            {
                this._current += 1;
                this.Show();
            }
        }

        public void Reload()
        {
            if (this._current >= 0)
            {
                this.Show();
            }
        }

        public void OnLoaded()
        {
            if (this._current >= 0)
            {
                this._webView.OnNativeNavigated(this._history[this._current].Uri, isSuccess: true);
            }
        }

        public override Size MeasureNative(
            Size availableSize)
        {
            return new Size(
                float.IsInfinity(availableSize.Width) ? 0 : availableSize.Width,
                float.IsInfinity(availableSize.Height) ? 0 : availableSize.Height);
        }
    }
}
