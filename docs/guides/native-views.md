# Native views and web pages

## Web pages

`WebView` shows pages in the platform's own web view:

```csharp
var web = new WebView() { Source = new Uri("https://example.com/") };
web.Navigated += (sender, e) =>
{
    var loaded = e.IsSuccess;
};
```

`NavigateToString` shows HTML you give it. `CanGoBack`, `GoBack`, `GoForward` and `Reload` do what
they say.

| Platform | Uses | Needs |
|---|---|---|
| Windows | WebView2 | Its runtime, which comes with Windows 11. Without it the view stays empty, and `FrameworkLog` says why |
| Android | `android.webkit.WebView` | The `INTERNET` permission |
| iOS | `WKWebView` | |
| Web | `iframe` | A site can refuse to appear in a frame. Back and forward cover only the pages the app asked for |

## Any native view

`NativeViewHost` hosts one of the platform's views, such as a map. It takes a function that creates
the view, so put each platform's code in that platform's project:

```csharp
Func<object> createMap = () => new object();
var map = new NativeViewHost(createMap) { Height = 300 };
```

| Platform | The function returns |
|---|---|
| Windows | A window handle (`nint`) |
| Android | An `Android.Views.View` |
| iOS | A `UIKit.UIView` |
| Web | An element, as a `JSObject` |

`NativeView` gives you the view again once it exists.

On Windows, a native view is above everything XForma draws in that window, popups included.
