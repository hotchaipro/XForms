# XForms architecture

XForms is built in layers. The **framework** contains only what needs a
platform: hosting, the views the platforms render natively, raw input,
drawing, focus and accessibility. **XForms.Toolkit** has what apps use
besides controls (platform services and data binding), for any set of
controls; its platform assemblies (`XForms.Toolkit.Win32` and the others)
implement the services. **XForms.Controls** is our controls, with the panels and gestures,
built only on the framework's public API. **Apps** use all three.

```
┌──────────────────────────────────────────────────────────────┐
│ App (for example SoonerOrLater)                              │
├──────────────────────────────────────────────────────────────┤
│ Theme: XForms.Themes.Simple (chosen by the app)              │
│   How the controls look: a template for each, and tokens     │
├──────────────────────────────────────────────────────────────┤
│ Controls: XForms.Controls                                    │
│   Buttons, tabs, menus, pickers, calendar, lists, panels,    │
│   gestures and their base classes: what they do, and their   │
│   state; built only on the public framework API              │
├──────────────────────────────────────────────────────────────┤
│ Toolkit: XForms.Toolkit                                      │
│   Platform services and data binding, for any set of         │
│   controls; XForms.Toolkit.<Platform> implement the          │
│   services, on the hosting API below                         │
├──────────────────────────────────────────────────────────────┤
│ Framework: XForms.Framework                                  │
│   Application, pages, views and layout, raw input, focus,    │
│   accessibility, drawing, popups, native primitives          │
├──────────────────────────────────────────────────────────────┤
│ Platform contract: XForms.Platform (internal)                │
│   Renderer interfaces (implemented by the platforms) and     │
│   hosts (ApplicationHost, LayerHost) they call               │
├──────────────┬──────────────┬───────────────┬────────────────┤
│ XForms.      │ XForms.      │ XForms.       │ XForms.        │
│ Framework.   │ Framework.   │ Framework.    │ Framework.     │
│ Android      │ iOS          │ Win32         │ Web            │
│ Public: only │ Public: only │ Public: only  │ Public: only   │
│ Android-     │ Ios-         │ Win32-        │ Web-           │
│ Application  │ Application  │ Application   │ Application    │
└──────────────┴──────────────┴───────────────┴────────────────┘
```

The goal is that any control library built on the framework runs on every
supported platform with the same look and the same layout. All of the
per-platform complexity stays inside the framework and the platforms.

Planned ([alignment.md](alignment.md#layering), phases 25-28): a standalone
XForms.Toolkit (services only, for any .NET app; binding moves to
XForms.Controls),
`XForms.Xaml` for markup, `XForms.Compat.WinUI` for WinUI's API, and
embedding XForms in part of an existing app's UI; and more themes beside
`XForms.Themes.Simple`.

## The rule for what goes where

**Something belongs in the framework only if the platforms need it.
Everything else belongs in a control library such as XForms.Controls.**

The framework holds two kinds of things:

- **Native primitives** (`XForms.Primitives`). These use the platform's own
  implementation because that's where the quality comes from:
  - text rendering with the platform's fonts and shaping (`TextBlock`);
  - text entry with the platform's IME, spell checking, selection and
    accessibility (`TextBox`);
  - scrolling with the platform's physics (`ScrollViewer`);
  - virtualized items (`ItemsView`, with `ItemContainer`);
  - images (`Image`) and popups (`Popup`).
- **Building blocks every control needs.** Each of these needs platform code:
  - `View` as a container: children, an owner-drawn surface (`DrawsContent`,
    `Render`), a background;
  - raw pointer input, activation (`Activated`) and an extension point for
    gestures (`GestureRecognizers`);
  - keyboard focus;
  - accessibility;
  - compositor animations.

A control belongs in XForms.Controls when it can be written once on top of
those pieces. Its Button, for example, reacts to a `TapGestureRecognizer`
and says when it's pressed; its theme's template draws it in `Render`. It
contains no platform code and looks identical everywhere. Panels, `Control`, `ContentControl` and `UserControl`, and the
gesture recognizers (tap, double tap and hold included), are built the same
way.

The same rule decides which layer owns a new feature. For example, a
repeating animation needs the compositor, so it goes in the framework. A
spinner built from that animation goes in XForms.Controls.

## Repository layout

| Folder | Project | Contents |
|---|---|---|
| `framework/` | `XForms.Framework` (net10.0) | The platform-neutral framework: `Core`, `Primitives`, and the platform contract in `Platform` |
| `platform/android/` | `XForms.Framework.Android` | Android implementation (Views, ViewGroups, RecyclerView) |
| `platform/ios/` | `XForms.Framework.iOS` | iOS implementation (UIKit) |
| `platform/win32/` | `XForms.Framework.Win32` | Windows implementation (Win32, DirectComposition, Direct2D, DirectWrite, RichEdit) |
| `platform/web/` | `XForms.Framework.Web` | Browser implementation (.NET on WebAssembly, DOM elements and canvases) |
| `toolkit/` | `XForms.Toolkit` (net10.0) | Platform services and data binding |
| `toolkit/platform/<platform>/` | `XForms.Toolkit.Android`, `.iOS`, `.Win32`, `.Web` | Each platform's services |
| `controls/` | `XForms.Controls` (net10.0) | Controls, panels and gestures |
| `analyzers/` | `XForms.Analyzers` | XF0001, which keeps the platforms to the platform contract |
| `test/` | `XForms.Test.*` | Test apps (the pages are shared in `test/XForms.Test`) |

The dependency rules:

- **Platform projects** reference the framework and use only the platform
  contract: the `XForms.Platform` namespace (renderer interfaces,
  `ApplicationHost`, `LayerHost`) and members marked `[PlatformContract]`.
  The framework gives them its internals (`InternalsVisibleTo`), and the
  XF0001 analyzer makes any other use a build error.
- **XForms.Toolkit** references the framework only. Its platform
  assemblies reference it and their framework platform, and implement its
  services using only that platform's public hosting API.
- **XForms.Controls**, and any other control library, references the
  framework (and XForms.Toolkit, for binding). It uses **public API only**. This is enforced by the
  compiler: it has no access to framework internals. Needing an internal is
  a signal that the framework is missing a public API for control authors.
- **An app** references the framework, one platform project (and its
  toolkit platform, for services), and the control libraries it uses. Its
  startup code runs the app on the platform (see Hosting).

The build enforces these rules:
- a project that references a layer it mustn't fails to build (XF0002, in
  `Directory.Build.targets`);
- the framework, its platforms, the toolkit and its platforms list their
  public API in `PublicAPI.Shipped.txt` and `PublicAPI.Unshipped.txt`, and
  public API that isn't listed (RS0016), or listed API that's gone (RS0017),
  fails the build. A new public type or member shows in review as a change
  to those files, which is how the platforms stay down to their hosting
  API;
- the contract (`XForms.Platform`) is internal, visible only to the four
  framework platforms (`InternalsVisibleTo`), and XF0001 keeps them to it;
  nothing else sees the framework's or the toolkit's internals.

Namespaces: the framework uses `XForms`, `XForms.Primitives` (the native
primitives) and `XForms.Platform` (the contract). XForms.Toolkit uses
`XForms`. XForms.Controls uses
`XForms.Controls` (controls, `Panel` and the base classes), `XForms.Layouts`
(the panels) and `XForms.Input` (the gesture recognizers).

## Hosting

An app is an `Application` subclass. Its constructor runs before any
platform exists, so it does nothing that needs one (the dispatcher, the
windows, services); using them there throws. Its work starts in `OnStart`.

Each platform's one public class runs the app. No app derives from an
XForms type to host it; on Android and iOS the activity and the delegates
are the app's own, with whatever base classes they need:

| Platform | Run | Native objects |
|---|---|---|
| Win32 | `Win32Application.Run("Title", new App())` from `Main`: a window and its message loop | `MainWindowHandle` |
| Browser | `await WebApplication.RunAsync("Title", new App())` from `Main`: the page | — |
| iOS | `IosApplication.Run(args, new App())` from `Main`, with XForms' own app and scene delegates; or `IosApplication.Run(windowScene, new App())` from the app's own scene delegate | `WindowScene`, `Window`, `RootViewController` |
| Android | `AndroidApplication.Run(this, new App())` from the app's own activity's `OnCreate` | `Activity` |

`Current` on each gives the running app's host, and its native objects, to
apps calling native APIs and to libraries adding platform features (the
toolkit's platforms among them). Services are registered the same way,
before `Run`: `Win32ToolkitServices.Register()`,
`AndroidToolkitServices.Register()`, `IosToolkitServices.Register()`, or
`await WebToolkitServices.InitializeAsync()`.

**Hosting rules.** XForms follows the host itself:
- **What XForms observes on its own:**
  - suspend and resume (Android's activity lifecycle callbacks, iOS's scene
    activation);
  - in-place configuration changes (dark mode, text size, size), through its
    root view;
  - Back (Android's back callbacks, and before Android 13 a wrapper on the
    window's callback);
  - the system bars and insets.
- **What the app leaves alone:** handling Back, or intercepting keys,
  before XForms sees them (overriding `OnBackPressed`, or the activity's
  `DispatchKeyEvent`). Android's own activity results go through the
  toolkit's `ResultActivity`, so the activity needn't forward them.
- **What the app forwards:** nothing yet. (A deep-link API would take the
  activity's `OnNewIntent`.)
- **Android re-creates the activity** for configuration changes it doesn't
  declare. The new activity's `Run` moves the running app to it, keeping
  its state, and leaves the new app object unused; when the activity
  finishes, the app stops, and the next launch runs a new one. Declare the
  changes XForms handles in place (dark mode, text size, orientation and
  screen size; SoonerOrLater's `MainActivity` is the pattern), so
  re-creation is rare. Use a theme without an action bar.

## Elements and renderers

Every UI object is an `Element`. Elements that appear on screen are `View`s.
An element doesn't talk to the platform directly. It asks the platform
(`IPlatform`) for a renderer, and the renderer wraps the native object:

```
TextBlock ──CreateRenderer()──► IPlatform.CreateTextBlockRenderer(...)
                                   │
            Android: TextBlockRenderer → android.widget.TextView
            iOS:     TextBlockRenderer → UILabel
            Win32:   TextBlockRenderer → DirectComposition visual + DirectWrite
```

Every view the platform doesn't render natively (panels, content and
owner-drawn controls, pages, list containers) is a **container**: it gets an
`IContainerRenderer`, which keeps its children's native views in drawing
order, fills its background, and draws it when it `DrawsContent`.

**Lightweight views.** Most views have no native object of their own.
Containers and text blocks with no animator are lightweight: they draw into
their nearest native ancestor, their layer. Pages, list containers, scrolled
content, popup content, views with animators and native primitives (text
boxes, images, items views, scroll views) are layers.
- On Android, iOS and the web, the framework provides lightweight renderers
  (the framework picks them; `IPlatform.UsesLightweightRenderers`). A
  layer's native view draws its lightweight descendants beneath its
  subviews, through `LayerHost`, and hosts the native views inside them.
  The framework hit-tests lightweight views and routes their pointer input.
- On Win32, lightweight renderers stay in the renderer tree (so hit testing,
  focus and UI Automation work as before) but have no visual: the layer draws
  them, and holds the visuals of native views inside them.

Renderer interfaces (`IViewRenderer`, `IContainerRenderer`,
`ITextBoxRenderer`, `IItemsViewRenderer`, and so on) are the contract each
platform implements; `ApplicationHost` and `LayerHost` are the framework's
side of it. Control libraries don't create renderers. They build on `View`
(children, drawing, background) and the primitives.

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

Any view can draw itself: it sets `DrawsContent` in its constructor,
overrides `Render(DrawingContext)` and draws within its bounds, beneath its
children. XForms.Controls' `UserControl` is that with one child. Each
platform implements `DrawingContext`:
- Android: `Canvas`.
- iOS: Core Graphics.
- Win32: Direct2D, with DirectWrite for text.

The view calls `InvalidateVisual()` to redraw. Its children (a
`UserControl`'s `Content`) draw above its drawing, which is how controls
combine drawing with native text.

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
children of panels with `ChildTransitions`. A `Frame` moves pages in and
out with a `PageTransition`: the built-in `Slide` (the default), `SlideUp`,
`Fade` and `None`, or a subclass that overrides `PlayAsync` and animates
the page through the `PageTransitionContext` it's given. Each animator has its own
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

XForms.Controls' `ProgressRing` is an arc drawn once and spun by its own
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

A `Theme` is an immutable record: a `ThemeMode` (light, dark or high
contrast), a `ColorScheme` of semantic colors (accent, background, surface
and error, each with the color of content on it, plus subtle foreground,
outline and disabled), and **extensions** that control libraries add
(`GetExtension`, `WithExtension`), as Flutter's theme extensions. The
framework knows only the mode and colors, which the platforms also fill
from the system's high-contrast colors. XForms.Controls adds its
`Typography`, `Shapes` and `Motion` (read as `theme.Typography` and so on)
and its controls' templates (below); a theme package adds its own styles,
such as XForms.Themes.Simple's `ButtonStyle`.

The application builds its theme with `CreateTheme(mode, accent)` from the
mode (the system's, unless the app sets `RequestedThemeMode`) and the accent
color (the system's, unless the app sets `AccentColor`). Each platform
reports the system's mode, accent and contrast colors, and their changes.

Any view can set `Theme` for itself and its descendants; `ActualTheme` is the
effective one. An element hears its theme in `OnThemeChanged`, once it's
attached and whenever the theme changes. The framework's primitives use the
theme's colors by default, as properties an app or control library can
set (`TextBox.BackgroundColor`, `ForegroundColor`, `PlaceholderColor` and
`AccentColor`; `ItemsView.BackgroundColor` and `AccentColor`); the renderers
take those colors and never read the theme.

**Templates.** A control says what it does and what state it's in; its
theme says how it looks. Each XForms.Controls control with a look is a
`TemplatedControl` (owner-drawn) or `TemplatedContentControl` (a content
control); `ListViewItem`, an `ItemContainer`, uses the same machinery. As
the control is first attached (`Element.OnAttaching`: its theme is known,
and its native object doesn't exist yet), it finds its template in its
theme (`theme.GetTemplate(type)`, falling back to its base types) and
applies it. The template (`ControlTemplate<TControl>`, as WPF's and
Avalonia's, but in code):
- `Apply`: adds parts (child views beneath the content) and creates
  animators, through a `TemplateContext`;
- `Render`, and optionally `Measure` and `Arrange`, or just a `Background`;
- `OnStateChanged`: feedback when the control is pressed, pointed at,
  focused or enabled, or its own state (checked, selected, active) changes;
- `OnThemeChanged`: restyles parts when the mode or accent color changes.

One template serves every control of its type, so per-control state (an
animator, a part) lives in `TemplateContext.State`. Controls whose input
depends on their geometry (the calendar's cells, a picker's text and glyph
areas, a combo box's rows) keep that geometry and expose it, with their
state, read-only; the template draws everything else.

**Choosing a theme.** An app chooses its theme in `CreateTheme`, and there's
no fallback: a templated control whose theme has no template for it throws
when it's attached, saying so.

```csharp
protected override Theme CreateTheme(ThemeMode mode, Color accentColor)
{
    return SimpleTheme.Apply(base.CreateTheme(mode, accentColor));
}
```

A view's theme can override a template or a style for it and its
descendants: `view.Theme = theme.WithExtension(new ButtonStyle { ... })`, or
`theme.WithTemplate(new MyButtonTemplate())`. The mode and accent color
change at runtime; switching to another theme package doesn't (yet).
`Border` isn't templated: as WPF's and WinUI's, it draws only what its own
properties say.

## Popups and light dismiss

- **`Popup`** shows a view above the page until it is closed. It is either
  centered in the window, or set beside a view with `PlacementTarget`: below
  it, or above it when there's more room there. The content is measured with
  the space available there, so a long list fits the screen and scrolls.
  Clicking outside the popup, pressing Escape or going back closes it, and
  navigating closes every open popup. When focus moved into a popup, closing
  it returns focus to where it was.
- **Popups draw nothing themselves.** XForms.Controls' `Flyout` and pickers draw
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

`ItemsView` is a native, virtualized view of items:
- Android: `ListView`.
- iOS: `UITableView`.
- Win32: a virtualized compositor list.

`ItemsViewSource` provides the items and their views. Each item view lives in
an `ItemContainer`, which the platform recycles as items scroll in and out of
view. When a container's height changes, the platform animates it. That is
how a list item can reveal more content. `Padding` puts space around the
items inside the scrolling area, so the last item can scroll clear of a bar
floating over the list.

XForms.Controls' `ListView` adds selection and clicks, with `ListViewItem`
as its container. Control libraries customize every item by subclassing the
container and overriding `CreateItemContainer`. Its `CommandListView` works
this way: tapping an item reveals a command bar below it (one item at a
time), and swiping an item runs a command.

## Threading

Each app runs on one UI thread with a `SynchronizationContext`, so `await`
resumes on the UI thread. `Application.InvokeOnUIThreadAsync` posts work from
other threads.

## Platform notes

| | Android | iOS | Win32 | Web |
|---|---|---|---|---|
| Host | The app's activity, with a root `ViewGroup`; edge-to-edge with insets | Scene-based `UIWindow`; safe area | Win32 window with a DirectComposition tree | A page: an element per layer, positioned absolutely |
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

The split is in place: XForms.Controls, including its pickers, calendar,
progress ring, panels and gestures, builds against public API only, no
platform has picker-specific code, and the platforms keep to the platform
contract. Still to do:

- **iOS testing.** iOS builds, but it hasn't been run since the restructure
  (it needs a Mac). Its popups, in particular, are new and untested.
- **Themes.** XForms.Themes.Simple is the only theme so far, and only
  `Button` has a style (`ButtonStyle`); the other looks change through
  their templates. Markup and WinUI compatibility come next
  ([alignment.md](alignment.md#layering)).
- **Web.** The browser backend (`platform/web`) has been tested in one
  browser (Chromium) without a screen reader or a touch screen.
- **Known Win32 issue.** RichEdit's built-in spell checker can crash the
  process when a text entry window is shown, hidden or resized while it is
  checking. A fix is in progress.
