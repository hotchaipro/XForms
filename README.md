# XForms

XForms is a lightweight .NET UI framework for apps with a custom look that
should look the same on every platform, while keeping the platform's own
text rendering, text entry and scrolling.

## What makes XForms different

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

  XForms does both: native where quality comes from the platform, owner-drawn
  everywhere else.
- **A small framework, with controls on top.** All platform-specific code
  lives in a small framework. Control libraries, including XForms' own
  XForms.Controls, are written once on its public API. The compiler enforces that,
  so any control library you write runs on every supported platform.
- **No markup or styling machinery.** There's no XAML, styling engine or
  binding engine to learn or carry. Controls are plain C# that draws and lays
  itself out.
- **Lean.** On Windows, XForms uses Win32, DirectComposition, Direct2D and
  DirectWrite directly, with no XAML or Windows App SDK. Its test app is
  about 21 MB self-contained and trimmed, compared with about 221 MB for the
  same app on WinUI 3. The Windows backend uses source-generated interop, so
  it's trimmable and compatible with Native AOT.
- **Native performance.** Layout is shared across platforms but driven by
  each platform's own layout system, and animations run on the platform's
  compositor, off the UI thread.

## Why choose XForms

XForms is a good fit when:
- you want a distinctive, fully custom design that's identical on every
  platform;
- you also want text and text entry to behave exactly as they do in the
  platform's own apps;
- you work in C# and prefer a small, understandable stack to a large
  framework.

It's not the right choice (yet) when:
- you need macOS, Linux or the web today;
- you need a large ready-made control set, design tooling or hot reload;
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

If you agree that makes sense for your app, then XForms may be just what you
need.

## How it's organized

| Folder | Project | What it is |
|---|---|---|
| `framework/` | `XForms.Framework` | The platform-neutral framework: app hosting, layout, input, accessibility, popups, and the native primitives |
| `platform/win32/` | `XForms.Framework.Win32` | Windows: Win32, DirectComposition, Direct2D, DirectWrite, RichEdit |
| `platform/android/` | `XForms.Framework.Android` | Android: native views |
| `platform/ios/` | `XForms.Framework.iOS` | iOS: UIKit |
| `platform/web/` | `XForms.Framework.Web` | Browsers: .NET on WebAssembly, DOM elements and canvases |
| `controls/` | `XForms.Controls` | Controls (buttons, pickers, calendar, menus, flyouts, lists, ...), panels and gestures, built only on the framework's public API |
| `test/` | `XForms.Test.*` | Test apps for each platform, with shared pages in `test/XForms.Test` |

An app references the framework, one platform project, and the control
libraries it uses. See [docs/architecture.md](docs/architecture.md) for how
the layers, layout, drawing, input, popups and lists work.

## Building

Requirements:
- the .NET 10 SDK;
- the .NET Android workload, for Android;
- the .NET iOS workload, and a Mac, to build for and run on iOS.

Build everything with:

```bash
dotnet build XForms.slnx
```

Run the Windows test app with:

```bash
dotnet run --project test/XForms.Test.Win32
```

## Status

XForms is in active development, and its API is changing.

| Platform | Status |
|---|---|
| Windows (Win32, no XAML) | In development: rendering, input, scrolling, lists, keyboard focus and UI Automation work |
| Android | In development: rendering, input, scrolling and lists work |
| iOS | In development: builds, but not yet tested since the latest restructure |

Planned work includes aligning the API with WPF, WinUI and Avalonia
conventions, lightweight controls, a richer drawing API, an animation system
played by the compositor, and accessibility and keyboard focus on every
platform.

## Documentation

- [Architecture](docs/architecture.md): the layers, and how layout, drawing,
  input, popups, lists and accessibility work.
- [Comparisons](docs/comparisons.md): XForms compared with Uno Platform,
  Avalonia, .NET MAUI, Electron, React Native, Flutter and Compose
  Multiplatform.
- [Alignment](docs/alignment.md): the framework's main gaps, and the plan to
  align its API with familiar frameworks.
