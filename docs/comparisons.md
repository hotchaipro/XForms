# XForms compared with other UI frameworks

This document compares XForms with other cross-platform UI frameworks:
- Uno Platform
- Avalonia
- .NET MAUI
- Electron
- React Native
- Flutter
- Compose Multiplatform

Tauri and Qt are covered briefly at the end. The goal is to make the
trade-offs clear, not to rank the frameworks: most of them are far more
mature than XForms, and each is the right choice for some apps.

The details here describe these frameworks as of late 2025 and early 2026.
They change quickly, so check current documentation before deciding.

## What XForms is optimizing for

XForms makes one bet: **native text, custom look**.

- **Native where quality comes from the platform.** Text rendering (the
  platform's fonts, shaping and emoji), text entry (IME, spell checking,
  selection, accessibility, autofill), scrolling physics and virtualized
  lists all use the platform's own implementation.
- **Owner-drawn everywhere else.** Controls draw themselves, so they look
  identical on every platform and don't change when the platform's design
  language does.
- **Small and layered.** A small framework holds all the platform-specific
  code. Control libraries, such as the toolkit, are written once on its public
  API. There's no XAML, no styling system and no data-binding engine to carry.
- **Lean.** For example, the Win32 test app is about 21 MB self-contained and
  trimmed. The same app on WinUI 3 was about 221 MB. On Windows, XForms uses
  Win32 and DirectComposition directly, with no XAML.

See [architecture.md](architecture.md) for how this works.

## At a glance

| | Language and UI model | Control rendering | Text rendering | Text entry | Platforms |
|---|---|---|---|---|---|
| **XForms** | C#, code (no markup) | Owner-drawn by control libraries | Native | Native | Windows, Android, iOS (web planned) |
| **Uno Platform** | C#, XAML (WinUI API) | Skia (default in new apps) or native controls | Skia, or native | Uno's own (Skia) or native | Windows, macOS, Linux, iOS, Android, web |
| **Avalonia** | C#, XAML (Avalonia's own) | Drawn by Avalonia (Skia) | Avalonia (HarfBuzz, Skia) | Avalonia's own | Windows, macOS, Linux, iOS, Android, web |
| **.NET MAUI** | C#, XAML or code | Native controls | Native | Native | Windows, macOS, iOS, Android |
| **Electron** | JS/TS, HTML/CSS | Chromium | Chromium | Chromium | Windows, macOS, Linux |
| **React Native** | JS/TS, React | Native views | Native | Native | iOS, Android (Windows, macOS and web through extra projects) |
| **Flutter** | Dart, widgets | Drawn by Flutter (Impeller or Skia) | Flutter's own | Flutter's own, bridged to the platform's input methods | iOS, Android, web, Windows, macOS, Linux |
| **Compose Multiplatform** | Kotlin, composables | Drawn by Compose | Platform text on Android; Skia elsewhere | Compose's own | Android, iOS, desktop, web |

## Uno Platform

**What it is.** Uno reimplements Microsoft's WinUI API on other platforms:
XAML, dependency properties, data binding, control templates and the WinUI
control set. Code written for WinUI runs on iOS, Android, macOS, Linux and the
web. On Windows, Uno can run on WinUI itself or on its own Skia-based desktop
host.

**Rendering.** Uno has two renderers:
- **Native.** WinUI controls map onto platform views, so apps look partly
  native and vary by platform.
- **Skia.** Uno draws everything itself, text included. Recent versions make
  this the default for new apps. It gives a consistent look, but text is
  shaped and drawn by Skia, not by the platform's text system.

**Compared with XForms:**
- **Text.** Uno can give you a consistent look (Skia) or native text (native
  renderer), but not both at once. XForms keeps text and text entry native
  and owner-draws everything else.
- **Layering.** In Uno, the framework and controls are one WinUI stack. You
  customize the look with XAML styles and templates, but you always carry the
  whole WinUI model: XAML, dependency properties, binding, visual states and
  templates. In XForms, control libraries sit on a small framework, and a
  control's look is whatever it draws.
- **Size.** Uno has a much larger runtime and tooling surface.

**Uno is ahead on:**
- maturity;
- platform coverage (macOS, Linux and the web);
- the size of its control set;
- Fluent, Material and Cupertino themes;
- hot reload and design tooling;
- accessibility on every platform;
- WinUI compatibility (your WinUI knowledge and code carry over).

**Choose Uno when** you want WinUI code or skills to run everywhere, or need
its breadth today.

## Avalonia

**What it is.** A WPF-inspired .NET UI framework with its own XAML dialect,
styling system and controls. Avalonia draws everything itself with Skia:
controls, text and scrolling. That gives it a pixel-identical look on Windows,
macOS, Linux, iOS, Android and the web.

**Compared with XForms:**
- **Rendering.** Avalonia and XForms share the goal of a consistent,
  self-drawn look, but draw the line differently. Avalonia owns text
  rendering (HarfBuzz and Skia) and text entry, with the platform's input
  methods bridged in. XForms keeps text and text entry native. So under
  Avalonia, text, editing behavior, spell checking and accessibility details
  can differ from what users see in native apps on each platform.
- **Model.** Avalonia brings XAML, styles, templates, a property system and
  binding. Restyling is very flexible but goes through that system. XForms
  controls are plain C# that draws.
- **Scrolling and lists.** Avalonia implements its own (consistent, but not
  native). XForms uses the platform's.

**Avalonia is ahead on:**
- maturity and its desktop track record (especially on Linux);
- its control set and themes;
- tooling (previewer, dev tools);
- web support;
- a large community.

**Choose Avalonia when** you want WPF-style development with a consistent
look everywhere, including Linux, and owned text rendering is acceptable.

## .NET MAUI

**What it is.** The successor to Xamarin.Forms. MAUI has one control API that
maps, through "handlers", to the platform's own controls:
- Windows: WinUI 3.
- Android: Android views.
- iOS and macOS: UIKit (macOS through Mac Catalyst).

Layout is cross-platform. For custom drawing there is `GraphicsView`, using
Microsoft.Maui.Graphics.

**Compared with XForms:**
- **Look.** MAUI deliberately looks native on each platform, so the same app
  looks and behaves differently on each one. A fully custom look means
  per-platform handler customization, or drawing your own controls. That is
  the gap XForms was originally created to fill. XForms owner-draws by
  default and keeps native only where it helps (text, text entry, scrolling,
  lists).
- **Text.** Both use native text and text entry.
- **Weight.** On Windows, MAUI runs on WinUI 3 and the Windows App SDK. That's
  the 221 MB versus 21 MB difference measured for the XForms test app (WinUI
  versus Win32). MAUI also carries XAML and its binding system.
- **Layering.** MAUI's controls are built into the framework. XForms separates
  the framework from control libraries.

**MAUI is ahead on:**
- Microsoft support;
- tooling in Visual Studio;
- the ecosystem (community toolkits, third-party control vendors);
- macOS support;
- maturity.

**Choose MAUI when** you want a native look per platform, Microsoft's
long-term support, or its ecosystem.

## Electron

**What it is.** Desktop apps built with web technology. Each app bundles
Chromium for the UI (HTML and CSS) and Node.js for system access. It runs on
Windows, macOS and Linux.

**Compared with XForms:**
- **Look.** Like XForms, Electron gives you complete control: CSS draws
  whatever you want, identically on every OS. Text and text entry are
  Chromium's: good quality and consistent, but not the platform's own. The
  web's own conventions apply instead.
- **Cost.** Every Electron app ships its own browser engine and Node runtime.
  Apps are typically 100 MB or more, with high memory use and slower
  startup. That is one of the things XForms set out to avoid.
- **Platforms.** Electron is desktop only. Mobile needs a separate stack.
- **Language.** Electron uses JS or TS and web skills. XForms uses C#.

**Electron is ahead on:**
- the web ecosystem (every web library and UI toolkit works);
- developer reach;
- desktop maturity: many widely used desktop apps are built on it.

**Choose Electron when** your team or code is web-based and the size and
memory overhead is acceptable.

## React Native

**What it is.** UIs written in React (JS/TS) that render to native views:
`View` becomes a `UIView` or Android `ViewGroup`, `Text` becomes native text,
and `TextInput` becomes the native text field.

**Compared with XForms.** This is architecturally the closest framework.
React Native:
- lays out with **Yoga**, a single cross-platform flexbox engine shared by
  every platform, much like XForms' shared layout engine driven by native
  containers;
- keeps text and text entry native, as XForms does;
- leaves the look to you.

The differences:
- **Language and model.** React Native uses JS or TS with React's declarative
  model. XForms uses C# with plain objects.
- **Custom drawing.** In React Native this usually comes from libraries (for
  example React Native Skia). XForms has an owner-drawn surface built in.
- **Ready-made controls.** Pickers and date pickers often come from
  community libraries that wrap native controls, so they look different per
  platform. XForms' toolkit draws them, so they look the same.
- **Windows and macOS** are separate projects maintained by Microsoft. The
  web is supported through React Native for Web.

**React Native is ahead on:**
- maturity on mobile (many large production apps);
- its ecosystem;
- hot reload;
- the React developer pool.

**Choose React Native when** your team works in React and mobile is the main
target.

## Flutter

**What it is.** Google's UI toolkit in Dart. Flutter draws every pixel with
its own engine (Impeller, or Skia on some platforms), including text, which
Flutter shapes and lays out itself using the system's fonts. It runs on iOS,
Android, the web, Windows, macOS and Linux.

**Compared with XForms.** Flutter is the framework closest to XForms' goals:
a custom, identical look everywhere, with good performance. The difference is
where the line is drawn.
- **Flutter owns everything.** That includes text layout, text editing
  (`EditableText`, connected to the platform's input methods), selection and
  scrolling physics (Flutter mimics each platform's). It gets complete
  consistency and control. But text editing and selection behavior are
  Flutter's re-creation of the platform's, and they can lag behind it.
- **XForms keeps text, text entry, scrolling and lists native.** So these
  behave exactly as the platform does, at the cost of more platform-specific
  code inside the framework.
- **Language.** Flutter uses Dart. XForms uses C# and .NET.

**Flutter is ahead on:**
- maturity;
- performance tooling;
- hot reload;
- a very large widget ecosystem (Material and Cupertino);
- platform coverage;
- its accessibility semantics tree.

**Choose Flutter when** a fully self-drawn UI suits you, Dart is fine, and
you want the most mature "custom look everywhere" framework.

## Compose Multiplatform

**What it is.** JetBrains' extension of Android's Jetpack Compose (declarative
UI in Kotlin) to iOS, desktop and the web. Compose draws its own components:
- On Android, it draws with Android's rendering and text stack.
- On iOS, desktop and the web, it draws with Skia (through Skiko).

**Compared with XForms.** Like Flutter, Compose owns the drawing, so its look
is consistent. Text is native on Android but Skia-rendered elsewhere, and
text editing is Compose's own, connected to the platform's input methods. As
with Uno and Flutter, that differs from XForms' rule that text and text entry
are always native. Compose uses Kotlin and its declarative model. Its iOS
support is newer than its Android and desktop support.

**Choose Compose Multiplatform when** your team works in Kotlin and Android
is the primary platform.

## Also considered

- **Tauri.** A Rust backend with the system's own web view (WebView2 on
  Windows, WKWebView on Apple platforms, WebKitGTK on Linux), and mobile
  support since version 2. It's like Electron, but much smaller, because it
  doesn't bundle a browser. The trade-off is that rendering varies with each
  system's web view.
- **Qt (Qt Quick and QML).** A mature C++ framework that draws its own
  controls on desktop, mobile and embedded devices. It has extensive tooling
  and a long track record, especially in embedded and industrial UIs. The
  trade-offs are C++ and QML, commercial licensing for some uses, and
  Qt-owned text and input.

## Where XForms stands

**XForms' advantages:**
- Native text rendering and text entry, combined with a custom, identical
  look built from owner-drawn controls.
- A small framework with all the platform code, and control libraries that
  depend only on its public API (enforced by the compiler).
- No markup, styling engine or binding engine to carry.
- A small footprint on Windows, with no XAML or Windows App SDK.

**XForms' gaps today:**
- Platforms: Windows, Android and iOS (iOS only partly tested since the
  restructure), with no macOS, Linux or web yet.
- Accessibility and keyboard focus are implemented only on Windows.
- A small control set.
- No hot reload, designer or tooling.
- One developer and one app, rather than an ecosystem.

**When XForms is a good fit:** you want both native text and a fully custom
look, you work in C#, and you value a small, understandable stack over
breadth.
