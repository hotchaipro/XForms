# Pages and navigation

An app is a class derived from `Application`, and each screen is a `Page`. Pages are reached by
**path**, as on the web, so links, Back and restoring after the system ends the app all work the
same way.

## Routes

List the pages in `OnConfigureRoutes`. Each route has a function that makes the page, so nothing
depends on reflection and the app stays safe to trim:

```csharp
public sealed class App : Application
{
    protected override void OnConfigureRoutes(Routes routes)
    {
        base.OnConfigureRoutes(routes);
        routes.Add("/", _ => new HomePage());
        routes.Add("/item/{id}", route => new ItemPage(route["id"]));
    }
}

public sealed class HomePage : Page
{
}

public sealed class ItemPage : Page
{
    public ItemPage(string id)
    {
    }
}
```

The app starts on `StartPath`, which is `/` unless it says otherwise. On the web, the path is in
the address.

## Going to a page and back

```csharp
await this.Application.NavigateAsync("/item/42");
await this.Application.GoBackAsync();
```

The platform's Back (Android's button or gesture, the browser's button, Escape or Alt+Left on
Windows) goes back too, and leaves the app when there's nowhere to go back to.

`ShowModalAsync` opens a page over the current one. `OpenWindowAsync` opens another window where
`CanOpenWindows` says the platform can: only on Windows.

## A page's life

```csharp
protected override View Build()
{
    return new XForma.Primitives.TextBlock() { Text = "Hello" };
}

protected override void OnNavigatedTo(NavigationEventArgs navigationEventArgs)
{
    base.OnNavigatedTo(navigationEventArgs);

    // The page is showing: refresh what it shows
}

protected override bool OnNavigatingFrom(NavigationEventArgs navigationEventArgs)
{
    // Return false to stay, such as when there are unsaved changes
    return base.OnNavigatingFrom(navigationEventArgs);
}
```

`Build` runs once as the page is first shown, and again after a hot reload edit.

## The app's life

Override these on your `Application`:

| Method | When |
|---|---|
| `OnStart` | The platform's ready: set up the app. The constructor can't use the platform yet |
| `OnSuspend` | The app went into the background: save its state |
| `OnResume` | It came back |
| `OnBackgroundRefreshAsync` | The system woke it to refresh, if `BackgroundRefreshInterval` is set |
| `OnRouteNotFound` | A path matched nothing: return one to go to instead |

## Running it

Each platform's project holds only an entry point, which calls `XFormaApp.Run` with the app and a
theme. `dotnet new xforma` writes them for you; see [Deploying](deploying.md).
