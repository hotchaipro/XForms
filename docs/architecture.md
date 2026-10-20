# XForms architecture

XForms is built in three layers. The **framework** contains everything
platform-specific: hosting, native primitives, input and accessibility. The
**toolkit** is a library of controls built only on the framework's public API.
**Apps** use both.

```
┌──────────────────────────────────────────────────────────────┐
│ App (for example SoonerOrLater)                              │
├──────────────────────────────────────────────────────────────┤
│ Toolkit: XForms.Toolkit                                      │
│   Buttons, tabs, menus, pickers, calendar, popovers, ...     │
│   Built only on the public framework API                     │
├──────────────────────────────────────────────────────────────┤
│ Framework: XForms.Framework                                  │
│   Application, pages, layout engine, input, accessibility,   │
│   popups, native primitives, owner-drawn surface             │
├───────────────────┬────────────────────┬─────────────────────┤
│ XForms.Framework. │ XForms.Framework.  │ XForms.Framework.   │
│ Android           │ iOS                │ Win32               │
└───────────────────┴────────────────────┴─────────────────────┘
```

The goal is that any control library built on the framework runs on every
supported platform with the same look and the same layout. All of the
per-platform complexity stays inside the framework.

## The rule for what goes where

**Something belongs in the framework only if it needs platform-specific code.
Everything else belongs in a control library such as the toolkit.**

The framework holds two kinds of things:

- **Native primitives.** These use the platform's own implementation because
  that's where the quality comes from:
  - text rendering with the platform's fonts and shaping;
  - text entry with the platform's IME, spell checking, selection and
    accessibility;
  - scrolling with the platform's physics;
  - virtualized lists;
  - images.
- **Building blocks every control needs.** Each of these needs platform code:
  - an owner-drawn surface;
  - pointer and keyboard input;
  - keyboard focus;
  - accessibility;
  - popups;
  - compositor animations.

A control is a toolkit control when it can be written once on top of those
pieces. The toolkit's Button, for example, draws itself through `IDrawContext`
and reacts to a `TapGestureRecognizer`. It contains no platform code and looks
identical everywhere.

The same rule decides which layer owns a new feature. For example, a
repeating animation needs the compositor, so it goes in the framework. A
spinner built from that animation goes in the toolkit.

## Repository layout

| Folder | Project | Contents |
|---|---|---|
| `framework/` | `XForms.Framework` (net10.0) | The platform-neutral framework API |
| `platform/android/` | `XForms.Framework.Android` | Android implementation (Views, ViewGroups, ListView) |
| `platform/ios/` | `XForms.Framework.iOS` | iOS implementation (UIKit) |
| `platform/win32/` | `XForms.Framework.Win32` | Windows implementation (Win32, DirectComposition, Direct2D, DirectWrite, RichEdit) |
| `toolkit/` | `XForms.Toolkit` (net10.0) | Cross-platform controls |
| `test/` | `XForms.Test.*` | Test apps (the pages are shared in `test/XForms.Test`) |

The dependency rules:

- **Platform projects** reference the framework and may use its internals
  (`InternalsVisibleTo`).
- **The toolkit**, and any other control library, references the framework
  only. It uses **public API only**. This is enforced by the compiler: the
  toolkit has no access to framework internals. Needing an internal is a
  signal that the framework is missing a public API for control authors.
- **An app** references the framework, one platform project, and the control
  libraries it uses. Its startup code calls into the platform project, for
  example `Win32Application.Run` or an `AndroidApplicationActivity` subclass.

Namespaces: the framework uses `XForms`, `XForms.Controls` (primitives),
`XForms.Layouts` and `XForms.Input`. The toolkit uses `XForms.Toolkit`.

## Elements and renderers

Every UI object is an `Element`. Elements that appear on screen are `View`s.
An element doesn't talk to the platform directly. It asks the platform
(`IPlatform`, internal) for a renderer, and the renderer wraps the native
object:

```
TextBlock ──CreateRenderer()──► IPlatform.CreateTextBlockRenderer(...)
                                   │
            Android: TextBlockRenderer → android.widget.TextView
            iOS:     TextBlockRenderer → UILabel
            Win32:   TextBlockRenderer → DirectComposition visual + DirectWrite
```

Renderer interfaces (`IViewRenderer`, `ITextBoxRenderer`, `IListViewRenderer`,
and so on) are the contract each platform implements. Control libraries don't
create renderers. They build on the primitives, chiefly `UserControl` for
owner-drawn controls and `ContentControl` for composition.

**Native objects are created when a view is attached.** A view keeps its
property values itself, and its renderer is created the first time a parent
attaches it (a panel adds it, a control sets it as content, a frame shows a
page, a popup opens). By then every constructor and object initializer has
run. The renderer then receives the values the view kept, and containers
attach their children in turn. `View.Attached` and `View.Detached` report
when a view is added to and removed from its parent. The `Renderer`
properties are internal: no public API exposes a native object.

## Properties and binding

Views implement `INotifyPropertyChanged`. Properties that can be bound
have a descriptor (`TextBlock.TextProperty`), and views bind to any source
with a compiled lambda:

```csharp
title.Bind(TextBlock.TextProperty, viewModel, vm => vm.Title);
name.Bind(TextBox.TextProperty, viewModel, vm => vm.Name, (vm, value) => vm.Name = value);
```

There's no inherited data context: a page or control gets its view model
explicitly, and list rows get their item through `ListViewItem.Item`.
Controls run `System.Windows.Input.ICommand`s.

## Layout

All platforms share one layout engine, in `View`, with WPF-style semantics:

- **Measure.** `View.Measure(available)` applies the view's margin,
  requested size and maximum size, then asks the renderer to measure the
  native view (`IViewRenderer.MeasureNative`). The result is `DesiredSize`.
  Measuring reports the content's size. Fill alignment doesn't stretch during
  measure.
- **Arrange.** `View.Arrange(rect)` applies alignment and margin, sets
  `Bounds`, and positions the native view (`IViewRenderer.ArrangeNative`).
- **Containers.** Native containers call back into the shared engine for
  their children (`MeasureContent`, and `LayoutContent` or `ArrangeContent`).
  So the platform's own layout system drives and caches the passes, but every
  platform computes the same positions. `Panel` subclasses (`StackPanel`,
  `Grid`, `DockPanel`, `WrapPanel`, `Canvas`, `UniformGrid`) and owner-drawn
  controls implement `MeasureOverride` and `ArrangeOverride`.
- **Panels.** A panel keeps its children in a `ViewCollection`, or a
  `ViewCollection<TLayout>` when each child has typed layout parameters
  (`grid.Children.Add(view, row: 1, column: 2)`,
  `dock.Children.Add(view, Dock.Top)`). Libraries write their own panels
  the same way.
- **Padding, sizes and direction.** The engine applies `Padding` (inside
  the content), `MinWidth`/`MaxWidth`, `MinHeight`/`MaxHeight`, and
  right-to-left `FlowDirection`, which mirrors the children's positions.

Because the platform drives the passes, layout runs exactly when the platform
lays out:
- Android: `onMeasure` and `onLayout`.
- iOS: `layoutSubviews`, with a measure cache.
- Win32: a batched layout pass per message-loop turn.

That keeps native performance and native animation behavior.

## Owner-drawn controls

`UserControl` is the framework's owner-drawn surface. A control overrides
`DrawBackground(IDrawContext, bounds)`. Each platform implements
`IDrawContext`:
- Android: `Canvas`.
- iOS: Core Graphics.
- Win32: Direct2D, with DirectWrite for text.

The control calls `InvalidateVisual()` to redraw. A `UserControl` can also host a
child view through its `Content`, which is how controls combine drawing with
native text.

`IDrawContext` draws rectangles, ellipses, lines and arcs (with round ends),
images (optionally tinted), and text. `Application.MeasureText` measures a
line of text the way `DrawText` draws it, so controls can size themselves to
their text. Controls draw their own glyphs (chevrons, calendar icons) with
lines and shapes rather than icon fonts, which differ between platforms.

Animation goes through an `Animator`, which holds a view's translation,
scale, rotation and opacity. These don't affect layout. A view has no
animator unless it's given one when it's created:
- the app's, through the `init`-only `View.Animator`
  (`new ProgressRing { Animator = new() }`);
- a control's own, private ones, from `Control.CreateOwnAnimator()` in its
  constructor.

The framework gives its own animated views (pages) an animator. The
animators' `ScaleTo`, `RotateTo` and `TranslateTo`, and
`StartContinuousRotation`, run on the platform's compositor or animation
thread:
- Win32: DirectComposition.
- Android: a property animator.
- iOS: Core Animation.

The toolkit's `ProgressRing` is an arc drawn once and spun by its own
animator. For now every animator of a view drives the view's one native
object. Nested layers per animator are planned (see
[alignment.md](alignment.md)).

## Input

Platforms deliver pointer input as `PointerEventArgs` (Began, Moved, Ended
and Canceled) to the deepest view under the pointer, then to its ancestors,
until one handles it. Views usually handle input through gesture recognizers
(`TapGestureRecognizer`, `SwipeGestureRecognizer`). Two rules keep scrolling
native:

- **A gesture claims Moved events only once it is sure.** A swipe claims them
  once it is in its Changed state. A tap never claims them.
- **Platforms capture the pointer only after a claimed move.** Until then, a
  drag can still become a scroll of the enclosing list or scroll view.

Owner-drawn controls can also handle input directly by overriding
`View.HandlePointerEvent`, as the toolkit's pickers and calendar do.

Keyboard input goes to the focused view through public `View` API:
- `Focusable`. Views with a tap gesture are focusable by default.
- `Focus()`, `IsFocused` and `OnFocusChanged`.
- `OnKeyDown(KeyEventArgs)`. A key the focused view doesn't handle bubbles
  up to its ancestors.
- `OnActivate`, for Enter, Space or a screen reader's "invoke". By default it
  sends the view a tap.

The platform handles Tab and Shift+Tab (focus order), Escape (closes the top
popup, then goes back) and Alt+Left (back). This is implemented on Win32;
Android and iOS don't route keys yet.

## Accessibility

Views describe themselves with `AccessibilityRole` and `AccessibilityLabel`.
Controls provide defaults: a Button is a button named by its text; a list item
is named by the text inside it. Each platform maps these to its own
accessibility API:
- Win32: UI Automation providers (implemented).
- Android: TalkBack, through `AccessibilityNodeInfo` (not yet implemented).
- iOS: VoiceOver, through `UIAccessibility` (not yet implemented).

Native text entries expose themselves natively.

## Popups and light dismiss

- **`Popup`** shows a view above the page until it is closed. It is either
  centered in the window, or set beside a view with `PlacementTarget`: below
  it, or above it when there's more room there. The content is measured with
  the space available there, so a long list fits the screen and scrolls.
  Clicking outside the popup, pressing Escape or going back closes it, and
  navigating closes every open popup. When focus moved into a popup, closing
  it returns focus to where it was.
- **Popups draw nothing themselves.** The toolkit's `Popover` and pickers draw
  their own frames, so popups look the same everywhere. The platforms differ
  in how they show popups:
  - Win32: a second, topmost composition target, so popups also cover native
    text entry windows.
  - Android: a `PopupWindow`.
  - iOS: a view on top of the window.
- **`Application.LightDismissTarget`** is the in-page equivalent, for views
  such as flyouts that close when the user touches anywhere outside them.
  Implement `ILightDismissTarget`.

## Lists

`ListView` is a native, virtualized list:
- Android: `ListView`.
- iOS: `UITableView`.
- Win32: a virtualized compositor list.

`ListViewSource` provides the items and their views. Each item view lives in a
`ListViewItemContainer`, which the platform recycles as items scroll in and
out of view. When a container's height changes, the platform animates it.
That is how a list item can reveal more content.

Control libraries customize every item by subclassing the container and
overriding `ListView.CreateItemContainer`. The toolkit's `CommandListView`
works this way: tapping an item reveals a command bar below it (one item at a
time), and swiping an item runs a command.

## Threading

Each app runs on one UI thread with a `SynchronizationContext`, so `await`
resumes on the UI thread. `Application.InvokeOnUIThreadAsync` posts work from
other threads.

## Platform notes

| | Android | iOS | Win32 |
|---|---|---|---|
| Host | Activity with a root `ViewGroup`; edge-to-edge with insets | Scene-based `UIWindow`; safe area | Win32 window with a DirectComposition tree |
| Drawing | `Canvas` | Core Graphics | Direct2D, DirectWrite |
| Text entry | `EditText` | `UITextField` / `UITextView` | RichEdit (`RichEditD2DPT`) |
| List | `ListView` | `UITableView` | Virtualized compositor list |
| Scrolling | Native (`android.widget.ScrollView`) | Native (`UIScrollView`) | Compositor-animated, with touch flick |
| Popups | `PopupWindow` | View over the window | Topmost composition target |
| Accessibility | Not yet | Not yet | UI Automation |
| Keyboard focus | Not yet | Not yet | Tab order, focus ring, activation |
| Interop | .NET for Android | .NET for iOS | Source-generated (CsWin32, `GeneratedComInterface`): trimmable and Native AOT-compatible |

## Current state

[alignment.md](alignment.md) proposes aligning the framework's API with WPF,
WinUI and Avalonia (and with MAUI for app services), and lists the gaps to
fill.

The split is in place: the toolkit, including its pickers, calendar and
progress ring, builds against public API only, and no platform has
picker-specific code. Still to do:

- **iOS testing.** iOS builds, but it hasn't been run since the restructure
  (it needs a Mac). Its popups, in particular, are new and untested.
- **Theming.** `AppTheme` lives in the framework, and some primitives read
  colors from it. Over time, primitives should expose plain color properties,
  leaving themes to control libraries, while the framework reports the system
  settings (dark mode, accent color, text scale).
- **Accessibility and keyboard focus on Android and iOS.** The framework's
  accessibility roles and labels, and keyboard focus, are implemented only on
  Win32 so far.
- **Web.** A browser backend (DOM, native inputs, canvas and CSS transforms)
  fits the same model as the other platforms.
- **Known Win32 issue.** RichEdit's built-in spell checker can crash the
  process when a text entry window is shown, hidden or resized while it is
  checking. A fix is in progress.
