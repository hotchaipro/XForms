using System;
using XForma.Controls;
using XForma.Layouts;
using XForma.Primitives;

namespace XForma.Test
{
    // The platform's web view among drawn views: HTML given directly, and an
    // address, with what it reports
    public class WebViewPage : Page
    {
        protected override View Build()
        {
            var status = new TextBlock() { Text = "Loading", Name = "status" };
            var webView = new WebView()
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
            };
            webView.Navigated += (sender, e) =>
            {
                status.Text = $"Navigated: {e.Uri?.ToString() ?? "(HTML)"}, success: {e.IsSuccess}, back: {webView.CanGoBack}";
            };
            webView.NavigateToString("<html><body><h1 id=\"hello\">Hello from HTML</h1></body></html>");

            var go = new Button() { Text = "Go to example.com", Width = 170, Height = 34 };
            go.Clicked += (sender, e) => webView.Source = new Uri("https://example.com/");

            var back = new Button() { Text = "Back", Width = 70, Height = 34 };
            back.Clicked += (sender, e) => webView.GoBack();

            var bar = new StackPanel() { Orientation = Orientation.Horizontal, Spacing = 8 };
            bar.Children.Add(go);
            bar.Children.Add(back);
            bar.Children.Add(status);

            var layout = new DockPanel();
            layout.Children.Add(bar, Dock.Top);
            layout.Children.Add(webView, Dock.CenterOverlay);
            return layout;
        }
    }
}
