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
| `platform/web/` | `XForms.Framework.Web` | Browser implementation (.NET on WebAssembly, DOM elements and canvases) |
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

**Lightweight views.** Most views have no native object of their own.
Owner-drawn and content controls, panels and text blocks with no animator
are lightweight: they draw into their nearest native ancestor, their layer.
Pages, list rows, scrolled content, popup content, views with animators and
native primitives (text boxes, images, lists, scroll views) are layers.
- On Android and iOS, the framework provides lightweight renderers. A
  layer's native view draws its lightweight descendants beneath its
  subviews, and hosts the native views inside them. The framework hit-tests
  lightweight views and routes their pointer input.
- On Win32, lightweight renderers stay in the renderer tree (so hit testing,
  focus and UI Automation work as before) but have no visual: the layer draws
  them, and holds the visuals of native views inside them.

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
`Render(DrawingContext)` and draws within its bounds. Each platform
implements `DrawingContext`:
- Android: `Canvas`.
- iOS: Core Graphics.
- Win32: Direct2D, with DirectWrite for text.

The control calls `InvalidateVisual()` to redraw. A `UserControl` can also host a
child view through its `Content`, which is how controls combine drawing with
native text.

`DrawingContext` draws:
- rectangles (also rounded), ellipses, lines and arcs, with round ends;
- paths (`PathGeometry`), filled with a solid or gradient brush, or
  stroked;
- images (optionally tinted);
- text.

It can push a clip, a transform or group opacity. Text is laid out with a
`TextLayout`: a `TextFormat` (family, size, weight, style, alignment,
wrapping, trimming, maximum lines) and optional styled spans. A layout is
measured once and drawn, so controls can size themselves to their text.

Font families can be generic (`FontFamilies.Serif`, `SansSerif`,
`Monospace`), which map to matching system fonts on each platform, or
registered from a font file with `Application.RegisterFont`. Controls draw
their own glyphs (chevrons, calendar icons) with lines and shapes rather
than icon fonts, which differ between platforms.

Animation goes through an `Animator`, which holds a view's translation,
scale, rotation, opacity and clip, around a transform origin. These don't
affect layout. A view has no animator unless it's given one when it's
created:
- the app's, through the `init`-only `View.Animator`
  (`new ProgressRing { Animator = new() }`);
- a control's own, private ones, from `Control.CreateOwnAnimator()` in its
  constructor.

The framework gives its own animated views an animator: pages, and the
children of panels with `ChildTransitions`. Each animator has its own
native layer, nested: the app's outside, a control's own inside it, with
the view's content and children innermost. The animators' `To` and `From`
methods (translate, scale, rotate, fade, reveal) run on the platform's
compositor or animation thread:
- Win32: a DirectComposition visual per layer, with its transforms, opacity
  and clip animated by the compositor.
- Android: the layers composed onto the view's own render properties,
  animated by value animators.
- iOS: Core Animation keyframe animations.

Each returns an `AnimationHandle`, which can be awaited for whether the
animation finished or was interrupted. `Application.RequestAnimationFrame`
calls back before the display's next frame, for drawing that changes every
frame.

The toolkit's `ProgressRing` is an arc drawn once and spun by its own
animator.

## Input

Platforms deliver pointer input as `PointerEventArgs` (with a pointer ID,
device type, position and modifiers) to the deepest view under the pointer,
then to its ancestors, until one handles the press. That view gets the rest
of that pointer. Every finger is its own pointer.

Views handle input through overrides and events (`OnPointerPressed`,
`PointerMoved`, `OnPointerWheelChanged` and so on), the `Tapped`,
`DoubleTapped` and `Holding` events, or gesture recognizers
(`TapGestureRecognizer`, `SwipeGestureRecognizer`, `PanGestureRecognizer`,
`PinchGestureRecognizer`). `Application.PreviewPointerInput` sees every event
first. Two rules keep scrolling native:

- **A gesture claims Moved events only once it is sure.** A swipe claims them
  once it is in its Changed state. A tap never claims them.
- **Platforms capture the pointer only after a claimed move.** A move is
  claimed when it's handled, or when the view called `CapturePointer`.
  Until then, a drag can still become a scroll of the enclosing list or
  scroll view.

Keyboard input goes to the focused view through public `View` API:
- `Focusable`. Views with a tap gesture are focusable by default.
- `Focus()`, `IsFocused`, and `GotFocus`/`LostFocus` (with the overrides
  `OnGotFocus` and `OnLostFocus`).
- `KeyDown` and `KeyUp`, with the overrides `OnKeyDown` and `OnKeyUp`. A key
  the focused view doesn't handle (`e.Handled`) bubbles up to its ancestors.
  `Application.PreviewKeyDown` sees each key first.
- `OnActivate`, for Enter, Space or a screen reader's "invoke". By default it
  sends the view a tap.

The platform handles Tab and Shift+Tab (focus order), Escape (closes the top
popup, then goes back) and Alt+Left (back) on Win32. Android and iOS route
hardware keys to the focused view the same way. There, a lightweight view has
no native object to take focus: its layer's native view takes it, and the
layer tracks which lightweight view inside it is focused, routes keys to it
and draws its focus ring. Tab and Shift+Tab follow the view tree, lightweight
and native views alike, on every platform (Android and iOS would otherwise
order native views by position, which puts a layer before the views inside
it).

## Accessibility

Views describe themselves with `AccessibilityRole` and `AccessibilityLabel`,
and controls with state override `AccessibilityIsChecked` (toggles),
`AccessibilityIsSelected` (list items, tabs, radio buttons) and
`AccessibilityValue` (a picker's choice). Assistive technology acts on a view
through `PerformAccessibilityAction` (invoke, toggle or select; by default
each does what a tap would). A control calls `NotifyAccessibilityStateChanged`
when its state or value changes. Controls provide defaults: a Button is a
button named by its text; a list item is named by the text inside it. Each
platform maps these to its own accessibility API:
- Win32: UI Automation providers, with the Invoke, Toggle, SelectionItem and
  Value patterns and their property-changed events.
- Android: TalkBack, through `AccessibilityNodeInfo`. Every native view gets
  an accessibility delegate; a layer exposes the lightweight views inside it
  as virtual nodes (as `ExploreByTouchHelper` does), with touch exploration,
  in reading order with its native children.
- iOS: VoiceOver, through `UIAccessibility`: a layer is one element when its
  view is accessible, and otherwise holds a `UIAccessibilityElement` for each
  accessible lightweight view, then its subviews.

Native text entries expose themselves natively.

## Themes

A `Theme` is an immutable record of design tokens: a `ColorScheme` of
semantic colors (accent, background, surface and error, each with the color
of content on it, plus subtle foreground, outline and disabled),
`Typography`, `Shapes` and `Motion`, for a `ThemeMode` (light, dark or high
contrast). It can also carry default styles for controls, such as the
toolkit's `ButtonStyle`.

The application builds its theme with `CreateTheme(mode, accent)` from the
mode (the system's, unless the app sets `RequestedThemeMode`) and the accent
color (the system's, unless the app sets `AccentColor`). Each platform
reports the system's mode, accent and contrast colors, and their changes.

Any view can set `Theme` for itself and its descendants; `ActualTheme` is the
effective one. An element hears its theme in `OnThemeChanged`, once it's
attached and whenever the theme changes. The framework's primitives use the
theme's colors by default (text, page and list backgrounds, text boxes), and
the toolkit's controls draw from its tokens.

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
  Implement `ILightDismissTarget`. "Inside" means the view or any descendant
  in the element tree, wherever it's drawn.
- **Views don't clip their children** unless `ClipToBounds` is set, and
  that holds for input too: a child drawn outside its parent (moved there by
  an animator, like a flyout's buttons) takes clicks there. Scrolling views
  always clip.

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

| | Android | iOS | Win32 | Web |
|---|---|---|---|---|
| Host | Activity with a root `ViewGroup`; edge-to-edge with insets | Scene-based `UIWindow`; safe area | Win32 window with a DirectComposition tree | A page: an element per layer, positioned absolutely |
| Drawing | `Canvas` | Core Graphics | Direct2D, DirectWrite | A `<canvas>` per layer, from a recorded command list |
| Text entry | `EditText` | `UITextField` / `UITextView` | RichEdit (`RichEditD2DPT`) | `<input>` / `<textarea>` |
| List | `ListView`; `RecyclerView` for rows and grids | `UITableView`; `UICollectionView` for rows and grids | Virtualized compositor list | A scrolling element, virtualized by lines |
| Scrolling | Native (`android.widget.ScrollView`) | Native (`UIScrollView`) | Compositor-animated, with touch flick | Native (`overflow: auto`) |
| Popups | `PopupWindow` | View over the window | Topmost composition target | An element over a backdrop |
| Accessibility | `AccessibilityNodeInfo`, virtual nodes for lightweight views | `UIAccessibility` elements | UI Automation | ARIA roles and states; hidden elements for lightweight views |
| Keyboard focus | Native focus, layer-tracked for lightweight views; view-tree Tab order | The same (first responder) | Tab order, focus ring, activation | `tabindex` and the browser's focus; lightweight focus as elsewhere |
| Suspend and resume | Activity pause and resume | Scene resigning and becoming active | Minimized, closed or Windows shutting down; restored | The page hidden or shown |
| Interop | .NET for Android | .NET for iOS | Source-generated (CsWin32, `GeneratedComInterface`): trimmable and Native AOT-compatible | `[JSImport]`, `[JSExport]` |

## Current state

[alignment.md](alignment.md) proposes aligning the framework's API with WPF,
WinUI and Avalonia (and with MAUI for app services), and lists the gaps to
fill.

The split is in place: the toolkit, including its pickers, calendar and
progress ring, builds against public API only, and no platform has
picker-specific code. Still to do:

- **iOS testing.** iOS builds, but it hasn't been run since the restructure
  (it needs a Mac). Its popups, in particular, are new and untested.
- **Theming.** The toolkit's controls other than `Button` have no styles of
  their own.
- **Layering.** The platforms use the framework's internals without a
  contract, and the framework holds control behavior such as tap
  recognition. Phases 19-22 of [alignment.md](alignment.md#layering) add a
  platform contract, slim the framework, rename the toolkit to
  `XForms.Controls` and plan themes, markup and WinUI compatibility.
- **Web.** The browser backend (`platform/web`) has been tested in one
  browser (Chromium) without a screen reader or a touch screen.
- **Known Win32 issue.** RichEdit's built-in spell checker can crash the
  process when a text entry window is shown, hidden or resized while it is
  checking. A fix is in progress.
