# Getting started

This builds and runs a new XForma app on each platform, then walks through how
an app is put together.

## What you need

- The [.NET 10 SDK](https://dotnet.microsoft.com/download).
- For Android: `dotnet workload install android`, and the Android SDK.
- For iOS: `dotnet workload install ios`, and a Mac with Xcode to build on.
- For the web: `dotnet workload install wasm-tools`.

## Create an app

Install the template, and create an app from it:

```bash
dotnet new install XForma.Templates
```

```bash
dotnet new xforma -n MyApp
```

`MyApp/MyApp` holds the app, shared by every platform. `MyApp.Win32`,
`MyApp.Android`, `MyApp.iOS` and `MyApp.Web` each hold only a small entry point.

## Run it

**Windows:**

```bash
dotnet run --project MyApp/MyApp.Win32
```

**Web:**

```bash
dotnet run --project MyApp/MyApp.Web
```

**Android**, with an emulator running or a device connected:

```bash
dotnet build MyApp/MyApp.Android -t:Run
```

**iOS**, on a Mac:

```bash
dotnet build MyApp/MyApp.iOS -t:Run
```

While you work, `dotnet watch` applies your edits to the running app:

```bash
dotnet watch --project MyApp/MyApp.Win32
```

## How an app is put together

### The app and its pages

An app derives from `Application` and maps paths to pages:

```csharp no-compile
public sealed class App : Application
{
    protected override void OnConfigureRoutes(Routes routes)
    {
        base.OnConfigureRoutes(routes);
        routes.Add("/", _ => new MainPage());
        routes.Add("/settings", _ => new SettingsPage());
    }
}
```

`Application.NavigateAsync("/settings")` opens a page, and the platform's Back
goes back. On the web, the path is the page's address.

### Views

A page's `Build` returns its views, in plain C#. There's no markup:

```csharp
protected override View Build()
{
    var layout = new StackPanel() { Spacing = 12, Margin = new Thickness(24) };
    layout.Children.Add(new TextBlock() { Text = "Hello" });
    return layout;
}
```

Panels (`StackPanel`, `DockPanel`, `Grid`, `UniformGrid`) lay views out. Controls
(`Button`, `CheckBox`, `ListView`, `NavigationView` and the rest) are in
`XForma.Controls`.

### Data binding

Bindings are typed, with compiled lambdas instead of reflection, so they survive
trimming and Native AOT. A view model raises `PropertyChanged`, and a view's
property follows it.

### Themes

Controls have no look of their own: a theme package draws them. Pass one to
`XFormaApp.Run`. Derive from `BasicTheme` to adapt it, or write your own.

```csharp no-compile
XFormaApp.Run("MyApp", new App(), new BasicTheme());
```

### The platform's own views

`WebView` shows web pages. `NativeViewHost` hosts any view of the platform's,
such as a map.

### When something goes wrong

`Application.UnhandledException` hears of exceptions nothing caught, for crash
reporting, and `FrameworkLog.MessageLogged` of what the framework has to say.
Call `Application.ReportException` at the top of an `async void` method.

## Where to go next

- [The guides](README.md), a topic at a time.
- [API reference](api/README.md).
- [What's supported](support.md), and what 1.0 leaves out.
- The test app in `test/XForma.Test`, which uses every control.
