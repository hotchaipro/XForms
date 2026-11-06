# Diagnostics and troubleshooting

## Crashes and logs

Hook these in `OnStart`, and pass them on to your crash reporter:

```csharp
public sealed class App : Application
{
    protected override void OnStart()
    {
        base.OnStart();

        this.UnhandledException += (sender, e) =>
        {
            var failure = e.Exception;

            // Keeps the app running on Windows and Android
            e.Handled = true;
        };

        FrameworkLog.MessageLogged += (sender, e) =>
        {
            var message = $"{e.Level}: {e.Message} {e.Exception}";
        };
    }
}
```

| Platform | After an unhandled exception |
|---|---|
| Windows, Android | Ends the app, unless a handler sets `Handled` |
| Web | Keeps running |
| iOS | Ends the app, after the event |

Exceptions in `async void` methods have nowhere to go. Catch them at the top of the method:

```csharp
private async void Load()
{
    try
    {
        await System.Threading.Tasks.Task.Delay(1);
    }
    catch (Exception exception)
    {
        Application.ReportException(exception);
    }
}
```

## Hot reload

`dotnet watch` applies edits while the app runs. Pages and list rows build again by themselves, and
views keep their user state by `Name` or their place. A view that built something once from code
runs that code again in `OnHotReload`.

## Common problems

| What you see | Why, and what to do |
|---|---|
| "The app hasn't started" | The app's constructor used the platform. Move it to `OnStart` |
| A control throws about a missing template or style | Its theme has none: check the theme passed to `XFormaApp.Run` |
| An exception about a NativeViewHost's function | It returned the wrong type for this platform. See [Native views](native-views.md) |
| A list's rows show old data | Rows are reused: set everything in `ItemViewFactory`'s second function |
| A binding doesn't update | The source raised nothing, or raised it off the UI thread. See [Data binding](data-binding.md) |
| A view takes no room | A hidden view, or an empty one with no `Width` or `Height`. Check its parent's alignment too |
| A `WebView` is empty on Windows | The WebView2 runtime is missing: see `FrameworkLog` |
| An image doesn't show | It didn't load: see `FrameworkLog`. Android lowercases resource names |
| Build error XF0001, XF0002 or XF0003 | A layering rule: see [the architecture](../architecture.md) |
| Build error RS0016 or RS0017 | Inside XForma only: public API changed, so update `PublicAPI.Unshipped.txt` |
