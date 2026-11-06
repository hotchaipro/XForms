# XForma

XForma is a lightweight .NET UI framework for apps with a custom look that
should look the same on every platform, while keeping the platform's own
text rendering, text entry and scrolling.

XForma was called XForms until November 2026. It isn't related to
Xamarin.Forms or to the W3C's XForms standard.

## What makes XForma different

- **Native text, custom look.** Text rendering (the platform's fonts, shaping
  and emoji), text entry (input methods, spell checking, selection,
  accessibility, autofill), scrolling physics and virtualized lists all use
  the platform's own implementation. Everything else is owner-drawn, so
  controls look identical everywhere and don't change when a platform's
  design language does.

  Other frameworks usually pick one side:
  - **MAUI and React Native** use native controls, so apps look different on
    each platform.
  - **Flutter, Avalonia, Compose Multiplatform and Uno's Skia renderer** draw
    everything, text and text entry included.

  XForma does both: native where quality comes from the platform, owner-drawn
  everywhere else.
- **A small framework, with controls on top.** All platform-specific code
  lives in a small framework. Control libraries, including XForma's own
  controls, are written once on its public API. The compiler enforces that,
  so any control library you write runs on every supported platform.
- **Your look, not ours.** Controls have no look of their own: each lays out
  and draws from a theme package, and the compiler rejects look that creeps
  back into a control. Use the Basic theme, adapt it, or write your own.
- **No markup or styling machinery.** There's no XAML, styling engine or
  reflection-based binding engine to learn or carry. Views and typed bindings
  are plain C#.
- **Lean.** On Windows, XForma uses Win32, DirectComposition, Direct2D and
  DirectWrite directly, with no XAML or Windows App SDK. Its test app is
  about 21 MB self-contained and trimmed, compared with about 221 MB for the
  same app on WinUI 3. The Windows backend uses source-generated interop, so
  it's trimmable and compatible with Native AOT.
- **Native performance.** Layout is shared across platforms but driven by
  each platform's own layout system, and animations run on the platform's
  compositor, off the UI thread.

## Why choose XForma

XForma is a good fit when:
- you want a distinctive, fully custom design that's identical on every
  platform;
- you also want text and text entry to behave exactly as they do in the
  platform's own apps;
- you work in C# and prefer a small, understandable stack to a large
  framework.

It's not the right choice (yet) when:
- you need macOS or Linux today;
- you need a large ready-made control set, a data grid, markup or a designer;
- you need a mature ecosystem: XForma has one developer and one app;
- you want a native look on each platform (consider MAUI), or need to reuse
  WPF or WinUI XAML (consider Avalonia or Uno Platform).

See [docs/comparisons.md](docs/comparisons.md) for detailed comparisons with
Uno Platform, Avalonia, .NET MAUI, Electron, React Native, Flutter and
Compose Multiplatform.

## Why a custom look?

If you've been shipping apps for more than a few years, you'll know that the
"native" look on every platform periodically changes, and apps that once
looked sleek and modern now look obviously dated. Not necessarily bad, just
dated, because they're using old "native" controls.

I noticed that some old iOS apps in particular still looked great, and the
thing they all had in common was a completely custom user interface that still
looked and worked great for their scenario.

That's when the light clicked on for me.

Despite how much each platform owner (Apple, Google, Microsoft) pushes you to
support their latest native look, so your app looks like every other app and
you'll probably have to rewrite it for every platform as well, it may not be
your best option.

If you agree that makes sense for your app, then XForma may be just what you
need.

## How it's organized

| Folder | Project | What it is |
|---|---|---|
| `framework/` | `XForma.Framework` | The platform-neutral framework: app hosting, layout, input, accessibility, popups, and the native primitives |
| `platform/win32/` | `XForma.Framework.Win32` | Windows: Win32, DirectComposition, Direct2D, DirectWrite, RichEdit |
| `platform/android/` | `XForma.Framework.Android` | Android: native views |
| `platform/ios/` | `XForma.Framework.iOS` | iOS: UIKit |
| `platform/web/` | `XForma.Framework.Web` | Browsers: .NET on WebAssembly, DOM elements and canvases |
| `xforma/` | `XForma` | What apps reference: controls (buttons, pickers, calendar, menus, flyouts, lists, ...), panels, gestures, binding and the app's entry point, built only on the framework's public API |
| `themes/` | `XForma.Themes.Basic` | How the controls look: the Basic theme, and the base for your own |
| `toolkit/` | `XForma.Toolkit` | Services for any .NET app: clipboard, preferences, secure storage, sharing, file pickers and more |
| `analyzers/` | `XForma.Analyzers` | Build rules that keep the layers and the look apart |
| `test/` | `XForma.Test.*`, `XForma.Tests` | Test apps for each platform, with shared pages in `test/XForma.Test`, and the unit tests |

An app references `XForma` and a theme package. See [docs/architecture.md](docs/architecture.md) for how
the layers, layout, drawing, input, popups and lists work.

## Building

Requirements:
- the .NET 10 SDK;
- the .NET Android workload, for Android;
- the .NET iOS workload, and a Mac, to build for and run on iOS.

Build everything with:

```bash
dotnet build XForma.slnx
```

Run the Windows test app with:

```bash
dotnet run --project test/XForma.Test.Win32
```

## Status

XForma is working toward 1.0, and its API can still change. Version 1.0
promises four platforms:

| Platform | Tested on |
|---|---|
| Windows (Win32, no XAML) | Windows 11 |
| Android | Emulators |
| iOS | Simulators |
| Web (.NET on WebAssembly) | Edge |

What 1.0 leaves out is listed in [docs/support.md](docs/support.md).

## Documentation

**[Start here](docs/README.md):** getting started, guides for each topic and the API
reference.

- [Architecture](docs/architecture.md): the layers, and how layout, drawing,
  input, popups, lists and accessibility work.
- [Comparisons](docs/comparisons.md): XForma compared with Uno Platform,
  Avalonia, .NET MAUI, Electron, React Native, Flutter and Compose
  Multiplatform.
- [Alignment](docs/alignment.md): how the API lines up with familiar
  frameworks, and the history of each phase.
- [Support](docs/support.md): what 1.0 includes and leaves out, supported
  versions, and how issues are handled.
