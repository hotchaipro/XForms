# XForma documentation

New here? Start with [Getting started](getting-started.md), which builds and runs an app on every
platform in a few minutes.

## Guides

| Guide | What it covers |
|---|---|
| [Pages and navigation](guides/pages-and-navigation.md) | Routes, going back and forward, a page's and an app's life |
| [Layout](guides/layout.md) | Panels, sizes, alignment and scrolling |
| [Controls](guides/controls.md) | Which control to use, lists, messages and menus |
| [Data binding](guides/data-binding.md) | Typed bindings to your data |
| [Input and gestures](guides/input-and-gestures.md) | Gestures, the keyboard, drag and drop |
| [Theming](guides/theming.md) | Dark mode, your colors, styles, templates, theme packages |
| [Custom controls](guides/custom-controls.md) | Composing, drawing, layout, themeable controls |
| [Native views and web pages](guides/native-views.md) | `WebView` and `NativeViewHost` |
| [Platform services](guides/services.md) | Preferences, secure storage, files, the UI thread, animation |
| [Accessibility](guides/accessibility.md) | Screen readers, the keyboard, text size, high contrast |
| [Diagnostics](guides/diagnostics.md) | Crash and log hooks, hot reload, common problems |
| [Testing](guides/testing.md) | Testing views with no device |
| [Building and deploying](guides/deploying.md) | Publishing for each platform |

## Reference

- [API reference](api/README.md): every public type and member.
- [What's supported](support.md): platforms, what 1.0 leaves out, known limitations.
- [Comparisons](comparisons.md): XForma next to other frameworks.

## For contributors

- [Architecture](architecture.md): the layers, and how everything works underneath.
- [Alignment](alignment.md): the API's design choices, and the history of each phase.
- [Benchmarks](benchmarks.md): measurements.

The C# in these docs is compiled by `python docs/check-snippets.py`. Run it after changing the docs
or the public API.
