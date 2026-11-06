# What XForma supports

This page says what XForma 1.0 promises, what it leaves out, and how issues
are handled.

## Supported versions

| | Supported |
|---|---|
| .NET | .NET 10. .NET 11 hasn't been tested yet |
| Windows | Windows 10 and 11, x64 |
| Android | Android 7.0 (API 24) and later |
| iOS | iOS 15 and later |
| Web | Current Chrome, Edge, Safari and Firefox |

## What 1.0 leaves out

**Not checked yet, and planned to be looked at again for a later release:**

- What VoiceOver and TalkBack say, and screen readers on the web. The
  accessibility API is built on all four platforms, but a person hasn't
  checked what those screen readers announce. Treat these as unverified if
  your app depends on them.
- A way to drag and drop with a screen reader. A keyboard alternative is
  included.
- Performance compared with MAUI, Avalonia and Uno.
- .NET 11.
- A second real app. Sooner Or Later is the only app built on XForma so far,
  so expect API gaps it never ran into.
- Continuous integration. Tests and builds run before each commit, on one
  machine.

**Not included:**

- macOS and Linux.
- XAML or other markup, and a designer: views are written in C#. These wait
  until a real project needs them.
- A data grid and printing.
- A WinUI compatibility layer.
- Embedding XForma in part of an existing app's UI.
- Maps and media players. Host the platform's own with `NativeViewHost`.

## Known limitations

- Native views, such as a `WebView` or a `NativeViewHost`, draw above XForma's
  owner-drawn views on some platforms, so an XForma popup can't cover one
  there. See [architecture.md](architecture.md).

- A `PanGestureRecognizer`'s translation is measured in its view's coordinates, so
  a view that moves itself as it's panned gets a shrinking distance. Put the
  recognizer on a view that stays put, such as the moving view's parent.
- On Windows, a drag from the app shows the system's cursor, without a picture of
  what's dragged. The other platforms show a plain box the size of the view.

## Versions and breaking changes

XForma follows [semantic versioning](https://semver.org). From 1.0 on,
breaking a public API needs a new major version. The `PublicAPI` files record
the public API, and the build fails on any change that isn't listed in them.

Names that an app's stored data lives under never change: an update can't lose
what users have saved.

## Reporting issues

Report bugs and ask for features in
[GitHub issues](https://github.com/hotchaipro/XForms/issues). XForma has one
maintainer, so fixes go into the latest release and there's no promised
response time. For a security problem, use GitHub's private vulnerability
reporting instead of a public issue.
