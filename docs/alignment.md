# Aligning XForms with familiar UI frameworks

This document covers five things:
- the main gaps in the XForms framework layer, compared with other UI
  frameworks;
- options for aligning XForms' API with a framework developers already know;
- a recommendation, including how XForms should draw and animate;
- the legacy mechanisms XForms should deliberately leave behind, and a
  proposed mapping from current names to new ones;
- whether another UI framework, such as one with XAML, could be built on
  XForms' public API, and a proof of concept to find out.

The goal is for XForms to feel familiar, so that developers recognize its
concepts and names, and for controls, view models and app logic to port with
modest rework. But familiar names shouldn't bring legacy mechanics with them:
XForms is free to break with designs that other frameworks keep only for
backward compatibility.

It's a proposal. Nothing here is implemented yet. See
[architecture.md](architecture.md) for how XForms works today and
[comparisons.md](comparisons.md) for how it differs from other frameworks.

## Summary

- **Framework layer.** Use the vocabulary shared by WPF, WinUI and Avalonia
  for layout, view properties, input, focus and drawing. XForms' layout
  already follows WPF's model (Measure and Arrange, `MeasureOverride` and
  `ArrangeOverride`, Margin, alignment, desired size), and it's the model the
  most .NET UI developers know.
- **App layer.** Follow .NET MAUI for lifecycle, navigation and platform
  services, where MAUI is strongest and where XForms' Xamarin.Forms heritage
  already points.
- **Familiar names, not legacy mechanics.** Adopt the names developers know,
  but not the mechanisms those frameworks keep only for compatibility. That
  rules out value-precedence property engines, inherited untyped data
  contexts, string-path bindings, tunneling events, attached properties and
  duplicate transform systems (see [What to leave behind](#what-to-leave-behind)).
- **Data binding.** Use plain observable properties and typed bindings with
  an explicit source, so MVVM view models port without a DependencyProperty
  system, XAML or reflection.
- **Drawing.** Keep XForms' own drawing API, implemented on each platform's
  native 2D library. Don't build on Skia.
- **Animation.** A view that animates is given an animator at construction,
  which holds its translation, scale, rotation, opacity and clip. The public
  `Animator` belongs to the app (`new ProgressRing { Animator = new() }`); a
  control animates its own content with private animators from
  `CreateOwnAnimator()`. The two nest as compositor layers, so they never
  conflict. Other views have no render properties. The
  compositor plays the animations, and the API follows Hot Chai's game
  engine. Layout changes animate as transitions after a single layout pass;
  layout properties are never animated.
- **Lightweight controls.** Owner-drawn controls draw into their host's
  surface instead of each having a native object. Only views given an
  animator, the framework's own pages, popups, scrolled content and list
  rows, and native primitives get native layers. Nothing is promoted at run
  time, and the public API never mentions layers, so this can be built later
  without breaking changes.
- **Code first.** XAML stays optional; a markup layer could come later.
- **Rename now.** SoonerOrLater is the only app built on XForms, so breaking
  changes are cheap today and expensive later.

## Gap analysis

The comparison is against WPF, WinUI and Avalonia, MAUI, Flutter and React
Native. Priority reflects how soon apps, and controls ported from elsewhere,
run into the gap.

| Priority | Area | XForms today | What developers expect |
|---|---|---|---|
| 1 | **Properties and binding** | `BindingContext` (a plain object) and attached properties. No change notification, no bindings | Observable properties, bindings to view models, `INotifyPropertyChanged`, commands (MVVM) |
| 1 | **Layout panels** | `DockLayout`, `DistributedStackLayout` | Grid, a stack panel with spacing, wrap, canvas, Border |
| 1 | **View basics** | Margin, alignment, requested and maximum size, opacity, scale, translation, rotation | Padding, minimum size, corner radius, clipping, z-order, shadow, collapsed vs hidden visibility, hit-test visibility, right-to-left flow |
| 2 | **Native object count** | Every view has its own native object, and every owner-drawn view has its own surface or backing store | Most elements drawn into shared layers, with native objects only where needed (as in browsers, Flutter and WPF) |
| 2 | **Input on views** | `HandlePointerEvent` (one pointer), tap and swipe recognizers, keys on Win32 | Pointer pressed, moved, released and canceled events, multi-touch, wheel, hover and cursor, long press, double tap, pinch and pan, context menus, drag and drop, focus events |
| 2 | **Drawing** | Rectangles, ellipses, lines, arcs, images, single-line text, an approximate shadow | Paths and geometry, rounded rectangles, brushes (solid, gradient, image), push/pop of clip, transform and opacity, multi-line text layouts |
| 2 | **Text** | Size, weight, style, alignment, wrapping, trimming. The font family is fixed per platform (for example Segoe UI on Win32) | Font families and bundled fonts, rich text spans, maximum lines, line height, letter spacing, selectable text |
| 2 | **Text entry** | Text, placeholder, multiline, spell-check and suggestion options, `Focus()` | Text-changed and submitted events, password mode, maximum length, read-only, selection API, input scope, return-key type |
| 3 | **Lists** | A virtualized vertical list with a custom source class, item clicked and selection changed | Typed item sources over observable collections, grouping and headers, horizontal and grid layouts, selection modes, pull-to-refresh, incremental loading |
| 3 | **Animation** | `ScaleTo`, `TranslateTo` and `RotateTo` on views, returning a `Task` that completes even when the animation is interrupted, and continuous rotation, on the compositor | Compositor animation of translation, scale, rotation, opacity and clip, with easing and springs; handles that can be awaited and cancelled; animated layout transitions; a per-frame callback for custom drawing |
| 3 | **Navigation and windows** | A frame stack with `NavigateTo` and `NavigateBack` | Navigation parameters and results, modal pages, transitions, tabs or shell, deep links; on desktop, multiple windows, window title and state |
| 3 | **Platform services** | `OpenUri` | Clipboard, file pickers, sharing, preferences and secure storage, haptics, connectivity, safe-area insets, system theme and text-scale changes |
| 3 | **Assets** | Images loaded by name, with different naming rules per platform | One asset model, density variants, vector images (SVG) |
| 3 | **Theming** | `AppTheme` (light and dark) with five colors | A theme mode reported by the system (light, dark, high contrast); typed design tokens (colors, type, shapes, motion) that any view can scope; per-control styles |
| Ongoing | **Accessibility** | A role and a label, on Win32 only | States (checked, expanded, selected), values, custom actions, live regions, on every platform |
| Ongoing | **Keyboard focus** | Win32 only | Every platform with a keyboard, including Android and iOS hardware keyboards |
| Later | **Platforms and tooling** | Windows, Android, iOS | macOS, Linux and web; hot reload, previewer, headless testing |

## Options

### A. WPF, WinUI and Avalonia vocabulary, code-first

Adopt the names and shapes these three frameworks share: `Grid`,
`StackPanel`, `Border`, `TextBlock`, `TextBox`, `ScrollViewer`, pointer and
key events on views, focus events, `Padding`, and `Render(DrawingContext)`.

- **For:**
  - XForms already follows this layout model, so the change is mostly names
    plus filling gaps.
  - It's the largest pool of .NET UI developers: WPF, UWP, WinUI, Avalonia
    and Uno.
  - Custom-drawn controls and layout panels from WPF or Avalonia port with
    little change.
  - Uno and Avalonia show that the same vocabulary works across platforms.
- **Against:**
  - Developers may expect XAML, styles, templates and the binding engine.
    That needs clear documentation, and maybe an optional markup layer later.

### B. MAUI and Xamarin.Forms vocabulary

Adopt MAUI's names: `Label`, `Entry`, `Editor`, `VerticalStackLayout`,
`CollectionView`, `BindableProperty`, `ContentPage`, Shell.

- **For:**
  - XForms descends from Xamarin.Forms (`Page`, `BindingContext`, one
    renderer per element), so the app model is already close.
  - Many teams have Xamarin.Forms or MAUI apps and want a custom look.
- **Against:**
  - MAUI's controls are native controls with a native look, the opposite of
    XForms' toolkit. Only view models, the app model and services port well.
  - MAUI's layout and drawing vocabulary (`GraphicsView`, `ICanvas`) is less
    widely known than WPF's.

### C. A declarative layer (Flutter, SwiftUI or Compose style)

A model-view-update or reactive library on top of the framework, as
MauiReactor and Comet sit on top of MAUI.

- **For:** attractive to developers from React, Flutter or SwiftUI.
- **Against:** it builds on the framework rather than shaping it. It fits as
  an optional library later.

### D. Compatibility shims

Libraries that imitate a subset of the WPF or MAUI API on top of XForms.

- **Against:** a shim is always partial, and porting fails at whatever falls
  outside the subset. Not recommended.

## Recommendation

### 1. Framework layer: WPF, WinUI and Avalonia vocabulary

Where the three frameworks agree, use their shared name. Where they differ,
prefer Avalonia's, since it's the cross-platform one and closest to XForms in
spirit (for example, `IsVisible` and `Render`). Keep names that are specific
to XForms where none of them has an equivalent, such as the swipe gesture.
Also keep them where XForms deliberately differs (see
[What to leave behind](#what-to-leave-behind)), and take the animation API
from Hot Chai's game engine (see [Animation](#5-animation-an-animator-given-to-a-view-at-construction)).

Two areas need concrete shapes:
- **Owner-drawn controls.** Controls draw in
  `Render(DrawingContext context)`, as in Avalonia (WPF's equivalent is
  `OnRender(DrawingContext)`), and call `InvalidateVisual()` to redraw.
  Composition (a control hosting other views) stays on `ContentControl` and
  `UserControl`, as in those frameworks. A control may or may not have its
  own native object, and the public API doesn't say which (see
  [Lightweight controls and layers](#6-lightweight-controls-and-layers)).
- **Input.** Views get overridable methods for control authors, and matching
  events for app code:
  - `OnPointerPressed`, `OnPointerMoved`, `OnPointerReleased`,
    `OnPointerCanceled` and `OnPointerWheelChanged`;
  - `OnKeyDown` and `OnKeyUp`;
  - `OnGotFocus` and `OnLostFocus`.

  Input bubbles from the view under the pointer (or with focus) up to its
  ancestors, with explicit pointer capture. Gesture recognizers remain, as a
  higher-level option, like MAUI's.

### 2. App layer: MAUI-style lifecycle, navigation and services

- **Lifecycle.** Keep the current lifecycle methods (start, suspend, resume).
- **Navigation.** Navigate to page instances (or factories), asynchronously:
  `NavigateAsync(page)`, `GoBackAsync()` and `CanGoBack`, with parameters,
  results and modal pages.
- **Platform services.** Add an Essentials-style set, using MAUI's API shapes
  where practical: launching URIs, clipboard, file pickers, preferences and
  secure storage, sharing and haptics. Expose them as interfaces available
  from the application rather than as static classes, so apps can substitute
  them in tests.
- **Theme.** Report the system's theme mode, accent color and high contrast,
  and their changes (see [Theming](#7-theming-a-mode-design-tokens-and-styles)).

### 3. Properties and binding

Don't adopt WPF's DependencyProperty engine (value precedence, inheritance,
metadata, coercion). Instead:
- **Observable properties.** View properties raise change notifications, the
  same way a view model's do (`INotifyPropertyChanged`).
- **Typed bindings with an explicit source.** For example,
  `title.Bind(TextBlock.TextProperty, viewModel, vm => vm.Title)`. Any
  conversion is ordinary code in the lambda. Bindings are compiled, so
  they're fast and safe for trimming and Native AOT, and they work with
  CommunityToolkit.Mvvm's `[ObservableProperty]` and `[RelayCommand]`.
- **No inherited data context.** There's no untyped `DataContext` or
  `BindingContext` that flows down the tree. A page or control gets its view
  model in its constructor, and a list passes each typed item to the code
  that binds its row.
- **Commands.** Controls accept `System.Windows.Input.ICommand`, the
  interface view models already expose, and also raise plain events.

This lets MVVM view models from WPF, Avalonia or MAUI apps move over
unchanged. Only the binding code in the views is rewritten.

### 4. Drawing: XForms' own API on native 2D libraries

Keep `DrawingContext` as XForms' own thin API, shaped like WPF and Avalonia's
`DrawingContext` so drawing code ports. Implement it on each platform's
native 2D library:
- Win32: Direct2D, drawing into DirectComposition surfaces.
- Android: `Canvas`, which is backed by Skia, recorded into Android's
  hardware-accelerated display lists.
- iOS: Core Graphics.
- Web (later): HTML canvas.

All of these support paths, gradients, clips, transforms and layers, so
filling the drawing gaps means thin wrappers rather than new engines.

Don't build on Skia (SkiaSharp), as a foundation or as an optional view:
- **Text.** Text drawn by owner-drawn controls would be shaped and rasterized
  by Skia, while `TextBlock` and `TextBox` next to it use DirectWrite, Core
  Text or Android's text stack. Native text is a core XForms principle.
- **Composition.** On Win32, Skia has no path into Direct2D and
  DirectComposition surfaces without extra copies. On Android, a separate
  Skia surface loses the integration with the system's own rendering.
- **Size.** Skia's native library adds several megabytes per platform, to
  apps that are small today.

Microsoft.Maui.Graphics follows the same design: a small canvas abstraction
over each platform's native drawing.

### 5. Animation: an Animator given to a view at construction

Most views don't animate, and they shouldn't pay for animation. A view that
will move, scale, rotate, fade or reveal is given an `Animator`, explicitly,
when it's created. There are two kinds, for two different owners:
- **The app's animator.** An app animates a view as a whole from outside, for
  example fading a progress ring in, through the view's public `Animator`:

  ```csharp
  new ProgressRing { Animator = new() }
  ```

- **A control's own animators.** A control animates its own content inside
  its bounds, for example the ring's spin, through animators it creates for
  itself and keeps private:

  ```csharp
  public class ProgressRing : Control
  {
      private readonly Animator _spin;

      public ProgressRing()
      {
          _spin = CreateOwnAnimator();
      }
  }
  ```

The API shape of the `Animator` comes from Hot Chai's game engine
(HotChai.Game). Animations change only properties that don't affect layout.

**The Animator holds a view's render properties:**
- translation;
- scale;
- rotation;
- opacity;
- clip, also used as a reveal;
- the transform origin.

They're set directly (for a view drawn at half opacity, or rotated 90°) or
animated. A view without any animator has none of them: `View` itself has
only layout and content properties, plus the optional `Animator`.

**Rules:**
- **Both are fixed at construction.** `View.Animator` is an `init`-only
  property, null by default. `Control.CreateOwnAnimator()` is a protected
  method for the control's constructor, and it throws if called after the
  view has been attached (added to a parent or shown). A control can call it
  more than once during construction, for example once in a base class and
  once in a subclass. A view either animates for its whole life or never
  does. The check is deterministic: a control that calls it too late fails
  the first time it's used, in the control library's own testing.
- **The public `Animator` always belongs to the app.** Controls never set it;
  they use their own animators instead. So setting it always works,
  whatever a control does internally, and a control can start animating
  itself in a later version without breaking apps.
- **A control's own animators are private to it.** The framework keeps track
  of them, but only the control holds a reference, in its own private field.
  A subclass that wants its own effect calls `CreateOwnAnimator()` again and
  gets another; it can't reach its base class's.
- **The one misuse that throws** is assigning the same `Animator` instance
  to two views. A control that breaks the guideline and sets its own public
  `Animator` also throws, as soon as an app sets it too: a bug in the
  control library, found the first time anyone uses that control with an
  Animator. The toolkit follows the guideline, and an analyzer could enforce
  it.
- **The framework's own animated views have an `Animator` by design:**
  pages, popups, scrolled content and list rows. That's what drives page
  transitions, popup entrances, scrolling and the list-row reveal.

**Alternatives considered for a control's own animators:**
- **A base constructor parameter** (`: base(new Animator())`). C# doesn't
  allow instance fields in a constructor initializer, so keeping a private
  reference needs a chained private constructor (or a protected property,
  which subclasses could reach). It also allows only one animator per
  class hierarchy, and every intermediate base class would need a forwarding
  constructor overload.
- **An interface** (`class ProgressRing : Control, IAnimatedView`). Interface
  members are public, so outside code could reach the control's animator
  with a cast. An interface is inherited, so subclasses can't add their own
  animator or opt out. It decides per type rather than per instance. And the
  .NET design guidelines advise against marker interfaces for signaling
  framework behavior.

**Nested, not combined.** Each animator gets its own compositor layer:
- The app's animator applies to the view's outer layer.
- Each own animator applies to a layer inside it, holding the control's
  drawing and children.

The compositor composes nested layers itself: transforms concatenate,
opacities multiply and clips intersect. There's no value math, and each
animator tracks its own values independently. Native layers exist only for
what's used:

| App's `Animator` | Own animators | Native layers |
|---|---|---|
| No | None | None (lightweight) |
| Yes | None | One |
| No | One | One |
| Yes | One | Two, nested |

Each further own animator adds one more nested layer.

**Setting and animating.** Setting a value, for example
`view.Animator.Scale = 0.5f`, applies it immediately and stops that channel's
animation. The Animator's methods animate a channel from its current value,
or from a given one, with an optional delay, easing and looping:
- `TranslateTo` and `TranslateFrom`;
- `ScaleTo` and `ScaleFrom`;
- `RotateTo` and `RotateFrom`;
- `FadeTo` and `FadeFrom`;
- `RevealTo` and `RevealFrom`, which animate the clip.

Each method returns an `AnimationHandle`, which can be awaited or cancelled.
Each channel runs one animation at a time. Starting another interrupts it,
and an interrupted animation never reports completion, so code that awaits it
doesn't run as if it had finished.

**Played by the compositor.** A view with an Animator gets its own compositor
layer, so each platform animates native properties directly, and the
animations keep running smoothly while the UI thread is busy:
- **Win32:** DirectComposition properties of the view's visual.
- **iOS:** Core Animation animations of the view's layer.
- **Android:** the `translationX`, `scaleX`, `rotation` and `alpha` of the
  view's native object, with animators that Android's render thread runs.

The layer is an implementation detail. The public API only says that the
view animates (see
[Lightweight controls and layers](#6-lightweight-controls-and-layers)).

The compositor's values can't be read back cheaply, so the Animator computes
a channel's current value by evaluating the same easing curve with the same
timing. The Win32 scroll controller already works this way. That's what lets
an animation start from wherever a view currently appears.

**Two kinds of animation, both explicit:**
- **Animator animations** move, fade, scale or reveal a whole view, on the
  compositor.
- **Drawn animations** are small effects inside one control, such as a press
  highlight or a ripple. The control redraws itself from a per-frame callback
  tied to the display's refresh. They run on the UI thread, but redraw only
  that control and never affect layout. Jetpack Compose's press indications
  work this way. This keeps simple controls, such as buttons, free of any
  animator.

**Layout properties are never animated.** XForms won't offer per-frame
animation of width, height, margin or any other property that affects
layout, not even as an opt-in. That's the expensive pattern: a measure and
arrange pass (and often a redraw) on the UI thread every frame, which is
where jank comes from.

**Layout changes animate as transitions.** After a layout pass moves a view,
the view's Animator animates its translation from the old on-screen offset
back to zero (`Animator.TranslateFrom(oldOffset, zero)`). A move that
interrupts another continues from wherever the view appears. This is the
FLIP technique ("first, last, invert, play"). Size changes use the clip
channel to reveal or hide, because scaling distorts text. Native lists
animate row changes this way, and so does XForms' list-item reveal on all
three platforms.

A panel that animates its children's layout changes needs an `Animator` for
each child. Such a panel is a distinct panel type, documented as giving its
children animators. It sets the public `Animator` of each child it creates,
or wraps a child that was created without one in a container that has one,
since an `Animator` can't be added after construction. Ordinary panels never
do this.

**Continuous animations** are looping channel animations. For example, a
progress ring spins its own content with
`_spin.RotateFrom(0°, 360°, period, loop: true)`.
`StartContinuousRotation` goes away.

**No transform objects.** There's no WPF-style `RenderTransform` object
graph (`TransformGroup`, arbitrary matrices). The fixed channels cover real
UI needs with less API (see [What to leave behind](#what-to-leave-behind)).

**Differences from HotChai.Game.** The engine gives every control its own
transform values as well as an `Animator`, and adds the two together when it
draws. That's nearly free in a game engine, which combines transforms on the
CPU every frame anyway. XForms reaches the same result, a control's own
effects and an app's animations that never interrupt each other, by nesting
compositor layers instead of adding values. And only views given an animator
have render properties at all, because every animated view needs a layer.

**Precedent.**
- **Jetpack Compose** is the closest match. Elements are lightweight, and
  `Modifier.graphicsLayer` gives an element its own render node with
  translation, scale, rotation, alpha and clip. Elements without one have no
  such properties.
- **Core Animation (iOS and macOS)** makes the layer the animatable unit,
  and nests layers freely: a view's layer can hold sublayers that animate
  independently. It stores animations by key, so a new animation for a key
  replaces the old one, like a channel. Its "presentation" layer reports where a layer appears
  right now, and an animation removed early reports that it didn't finish.
- **Android** has transform properties that never affect layout, and a
  per-view animator (`view.animate()`) that the render thread can run.
- **SwiftUI and Flutter** compose effects by nesting: an inner modifier or
  widget for a control's own effect, and an outer one for the app's.
- **The Web Animations API** rejects an animation's `finished` promise,
  rather than resolving it, when the animation is cancelled. That's the same
  rule as "an interrupted animation never completes".

### 6. Lightweight controls and layers

**The problem.** Today every view in XForms has its own native object: an
Android `View`, a UIKit view, or a DirectComposition visual with its own
surface. A command bar with five buttons is six native objects, and every row
of a command list adds more. Native objects cost time to create and memory to
keep. On iOS and Win32 the larger cost is memory: each owner-drawn view has
its own backing store or surface, of roughly width × height × 4 bytes,
multiplied by the square of the display scale. Android is cheaper, because it
records display lists instead.

It isn't urgent: virtualized lists keep counts bounded, and SoonerOrLater's
list scrolled well against Android's own Settings app in testing. But the
cost grows with richer interfaces, and any template-based control library,
such as a XAML layer, would multiply it.

**Lightweight views.** A lightweight view has no native object of its own.
It draws into the surface of its nearest layer, an ancestor that does have
one. The framework still gives it everything a view needs: layout,
hit-testing, input, focus, invalidation and accessibility. This is how
Jetpack Compose draws its elements into one Android view, how browsers paint
most elements into a few compositing layers, and how WPF draws its Visuals
into one surface.

**Which views have layers.** The rule is explicit and fixed when a view is
created. Nothing is promoted at run time.
- **Views given an animator** have a layer for each one: one for the app's
  `Animator`, and one for each of the control's own animators, nested. The
  compositor needs a layer to animate.
- **Views the framework makes layers by design:** pages, popups, scrolled
  content and list rows. These all have Animators, as described in
  [Animation](#5-animation-an-animator-given-to-a-view-at-construction).
- **Native primitives:** `TextBox`, `ScrollViewer`, `ListView` and `Image`
  are native objects.
- **Everything else is lightweight:** owner-drawn controls (everything the
  toolkit draws), `Border`, panels and probably `TextBlock`. Labels can be
  drawn with DirectWrite, Core Text or Android's text layout, so they stay
  native-quality text, while selectable text remains a native primitive.

There is no automatic promotion. Browsers create compositing layers
implicitly whenever content animates, which leads to unpredictable memory use
and to hints like `will-change` added to steer the heuristics. In XForms, a
layer's cost is visible in the code that creates the view.

A layer is the lightest native container that can also host native
primitives (a layer may contain a `TextBox`):
- **Android:** a `View`, whose render node the render thread animates.
- **iOS:** a `UIView` and its `CALayer`.
- **Win32:** a DirectComposition visual.

**Limits.**
- **Order.** A layer draws its lightweight content beneath its own native
  children, so a lightweight view can't draw on top of a native sibling in
  the same layer. A view that needs to must have its own layer.
- **Native text boxes in animated views.** On Win32, a RichEdit window can't
  scale, rotate or fade with its layer; it can only move. While such a view
  animates, the text box is shown as a snapshot, the technique the list-row
  close animation already uses.
- **Accessibility.** Lightweight views are exposed through virtual
  accessibility nodes: Android's `AccessibilityNodeProvider`, iOS's
  `UIAccessibilityElement`, and the UI Automation providers XForms already
  has on Win32, which are virtual already.
- **Hit-testing and input** are done by the framework for lightweight views,
  the way the Win32 backend already dispatches input itself.

**Native objects are created when a view is attached.** Today `Element`'s
constructor creates a view's renderer, by calling a virtual method before
any derived constructor has run, a known anti-pattern. Instead, the framework
creates a view's renderer, and decides whether it's lightweight or which
layers it has, when the view is first attached. By then every constructor and
object initializer has run, so the app's `init`-only `Animator` and the
control's own animators are both known before any native object is built.

**The contract.** The public API never mentions layers. `Render(DrawingContext)`,
`InvalidateVisual`, the input overrides and the `Animator` work the same
whether or not a view has a native object. The framework never promises a
native object or a surface per view, and no public API exposes one. Because
the only public signs of a layer are the animators, and those are decided
at construction, lightweight drawing can be built later without changing any
control library.

**Measure first.** Before building lightweight views, benchmark a page of
about 1,000 owner-drawn controls on each platform, with the current
one-native-object-per-view design. Measure:
- time to create and show the page;
- memory;
- frame time while scrolling and while animating.

The results size the gain on each platform, and set the order of the work.

**Results so far** (see [benchmarks.md](benchmarks.md)):
- **Android gains the most.** With one native view per tile, the page took
  about three times as long to show, used about 18 MB more memory, and
  dropped more frames while scrolling. That's the opposite of what was
  expected: the cost of each `View` dominates, not drawing.
- **Win32 gains in time, less in memory.** Drawing in one view halved the
  time to show the page and saved about 6 MB of process memory, but GPU
  memory didn't improve.
- **Large layers need tiled surfaces.** Drawing a whole tall layer into one
  surface used more GPU memory than 1,000 small ones. A scrolled layer needs
  a virtual or tiled surface that allocates only what's on screen.
- **iOS** hasn't been measured yet.

So lightweight views are worth doing, and Android is the strongest reason.

### 7. Theming: a mode, design tokens and styles

"Theme" means three different things in other frameworks, and XForms keeps
them apart:

| Concept | What it is | Precedent |
|---|---|---|
| **Mode** | Light, dark or high contrast | MAUI's `AppTheme` enum, Avalonia's `ThemeVariant`, WinUI's `ElementTheme`, SwiftUI's `ColorScheme` |
| **Design tokens** | Semantic colors, a type scale, shapes and motion, resolved for a mode | Flutter's `ThemeData`, Compose's `MaterialTheme`, WinUI's theme resources |
| **Skin** | How each control looks | Templates in WPF, WinUI and Avalonia; the control's own drawing, with component themes, in Flutter and Compose |

XForms' controls draw themselves and its properties are typed, so it follows
Flutter and Compose:
- **`ThemeMode`.** An enum: `Light`, `Dark` and `HighContrast`. The platform
  reports the system's mode and raises a change event. An app can follow it
  or override it.
- **`Theme`: typed design tokens.**
  - A `ColorScheme` with semantic roles in pairs (a color, and the color of
    content on it): accent, background, surface and subtle surface, error,
    plus outline and disabled. The names follow Material 3's roles and map
    to WinUI's theme resources.
  - `Typography`: a type scale (display, title, body, label, caption) as
    text formats, using the generic font families.
  - `Shapes`: corner radii (small, medium, large).
  - `Motion`: durations and easings (short, medium, emphasized).
  - XForms provides a theme for each mode, built from the system's accent
    color. An app can build its own (a brand's colors, its type scale).
- **Scoped.** Any view can set a `Theme` for itself and its descendants (a
  dark section in a light app, or a brand-colored banner). A view's
  effective theme is inherited down the tree, as `FlowDirection` is.
- **Styles, not templates.** A control can take an optional typed style
  object (`ButtonStyle { Background, CornerRadius, Padding }`) whose unset
  values come from the theme. A theme can carry default styles per control
  type, as Flutter's component themes do.
- **Delivered after attach.** A control's `OnThemeChanged(Theme)` runs once
  it's attached and whenever its effective theme changes. It never runs
  from a constructor, so a control can use views its constructor built. It
  replaces `OnApplyTheme`, which ran from `Element`'s constructor.

**Alternatives considered:**
- **Keep `AppTheme` as a palette class.** MAUI's `AppTheme` is the mode
  enum, so keeping the name with another meaning would mislead MAUI
  developers, and five colors leave controls to hard-code the rest.
- **String-keyed resource dictionaries** (WPF, WinUI, MAUI). They lose type
  checking and refactoring, and need the dependency property engine XForms
  doesn't adopt (see [Properties and binding](#3-properties-and-binding)).
- **Control templates.** XForms' controls draw themselves (see
  [Drawing](#4-drawing-xforms-own-api-on-native-2d-libraries)), so there's
  no visual tree to template.

### 8. Be clear about what ports

| Ports with little change | Needs rework | Doesn't port |
|---|---|---|
| View models, commands, services, app logic | Views written in code: new type names, typed bindings, layout parameters | XAML markup (until a markup layer exists) |
| Custom-drawn controls (`OnRender` or `Render`, with `DrawingContext`) | Layout written against WPF or Avalonia panels | Control templates, styles, themes and converters |
| Custom layout panels (`MeasureOverride` and `ArrangeOverride`) | Navigation code | Native-look controls from MAUI; animations of layout properties |

## What to leave behind

These are mechanisms other frameworks keep mainly for backward
compatibility. XForms should use familiar names without inheriting them.

| Legacy mechanism | Where it comes from | Why leave it | XForms instead |
|---|---|---|---|
| Animating layout properties (dependent animations, `LayoutTransform`) | WPF, WinUI | Runs layout every frame on the UI thread | Render-property animations and layout transitions |
| A dependency property engine with value precedence, inheritance and coercion | WPF, WinUI, Avalonia | Complex, slow to reason about, needed mostly by XAML styling | Plain observable properties |
| An inherited, untyped data context (`DataContext`, `BindingContext`) | WPF, WinUI, Avalonia, MAUI | Untyped, implicit, a source of binding errors that only show up at run time | Typed bindings with an explicit source |
| String-path bindings and value converters | WPF, WinUI, Avalonia, MAUI | Reflection, run-time failures, trimming problems | Compiled lambda bindings; conversion in code |
| Tunneling (`Preview*`) routed events | WPF, WinUI, Avalonia | A second routing phase that most code never needs | Bubbling with explicit pointer capture, plus one application-level input filter (which a XAML layer can use to implement its own preview events) |
| Attached properties for layout parameters (`Grid.SetRow`, `DockPanel.SetDock`) | WPF, WinUI, Avalonia | Static, untyped, discoverable only by convention | Typed parameters when adding a child, as `DockLayout` does today: `grid.Children.Add(view, row: 1, column: 2)` |
| Attached properties for accessibility (`AutomationProperties.Name`) | WPF, WinUI, Avalonia | The same | Plain view properties: `AccessibilityLabel` and `AccessibilityRole`, the naming used by SwiftUI, Flutter and React Native |
| Two transform systems (`RenderTransform` objects as well as `Translation`, `Scale` and `Rotation`), and a general animatable-property system | WPF, WinUI | Overlapping ways to do the same thing; any property can be animated, including expensive ones | One set of render properties in `View.Animator` (translation, scale, rotation, opacity, clip, transform origin), with no general matrix unless a real need appears |
| Implicit layer promotion (compositing layers created when content animates) | Browsers | Unpredictable memory and performance; needs hints like `will-change` | Views that animate are given an animator at construction; nothing is promoted at run time |
| An untyped `ItemsSource` with a template that creates a view per item | WPF, WinUI, Avalonia | Untyped, and the template hides the create and bind steps that recycling needs | A typed `ListView<T>` whose item views are created once and bound to many items |
| Navigating by page type (`Frame.Navigate(typeof(Page))`) | WinUI, UWP | Reflection; parameters passed as `object` | Navigating to page instances or factories, asynchronously |
| A mutable `FormattedText` | WPF, Avalonia | A legacy, mutable text object | An immutable `TextLayout`, used for measuring and drawing |
| Mutable brush objects (`Freezable`) | WPF | Change tracking that drawing doesn't need | Immutable brush values |
| Dispatcher priorities | WPF, WinUI, Avalonia | Rarely understood, often misused | One UI-thread queue: `Post`, `InvokeAsync`, and `await` |
| Static global services (`Clipboard.Default`) | MAUI | Hard to substitute in tests | Services available from the application, as interfaces |
| Windows 8-era names (`AppBarButton`, `XamlUICommand`) | WinUI | Names from retired designs | Descriptive names: `CommandBarButton`, `UICommand` |
| `double` coordinates, only to match WPF | WPF, WinUI, Avalonia, MAUI | With the changes above, ported code won't compile as-is anyway; Direct2D, DirectComposition and Android drawing use `float` | Keep `float` |

`ICommand` stays, despite its age: view models everywhere expose it, and
keeping it is what lets them port unchanged.

## Proposed name mapping

"Familiar from" names the frameworks a developer would recognize the new name
from. Where XForms keeps its own name, the reason is in
[What to leave behind](#what-to-leave-behind).

### Framework: types

| Current | Proposed | Familiar from | Notes |
|---|---|---|---|
| `View` | `View` | MAUI | Keep. Avalonia and WinUI developers know it as `Control` or `UIElement`; `View` is clearer for a base that includes primitives |
| `Layout` | `Panel` | WPF, WinUI, Avalonia | Base for layout containers; children are added with typed layout parameters |
| `DockLayout` | `DockPanel` | WPF, Avalonia | `DockRegion` becomes `Dock` (Left, Top, Right, Bottom). The overlay regions remain an XForms extension |
| `DistributedStackLayout` | `UniformGrid` | WPF, Avalonia | With one row (or column), it spaces children evenly |
| (new) | `StackPanel` | WPF, WinUI, Avalonia | With `Spacing` |
| (new) | `Grid` | WPF, WinUI, Avalonia | Rows and columns: Auto, star, fixed |
| (new) | `WrapPanel`, `Canvas` | WPF, Avalonia | |
| (new) | `Border` | WPF, WinUI, Avalonia | Background, border, `CornerRadius`, `Padding` |
| `TextView` | `TextBlock` | WPF, WinUI, Avalonia | |
| `TextEntry` | `TextBox` | WPF, WinUI, Avalonia | |
| `ScrollView` | `ScrollViewer` | WPF, WinUI, Avalonia | |
| `ListView` | `ListView<T>` | WinUI, Avalonia | Typed items |
| `ListViewSource` | `ItemsSource` (`IReadOnlyList<T>`), with item views created once and bound to each item | WinUI, Avalonia (name) | Observable collections update the list |
| `ListViewItemContainer` | `ListViewItem` | WinUI, Avalonia | The container, as in those frameworks. **Done** |
| `ListViewItem` (the item's view) | (any `View`) | | Created by the list's item-view factory |
| `Image`, `ImageSource`, `Bitmap` | `Image`, `ImageSource`, `Bitmap` | Avalonia, MAUI | Keep |
| `UserControl` (owner-drawn) | Any `Control`, overriding `Render` | Avalonia, WPF | Owner-drawing moves to the base `Control` |
| `UserControl` (composition) | `UserControl` | WPF, WinUI, Avalonia | Hosts `Content` |
| `ContentControl`, `Control` | `ContentControl`, `Control` | All | Keep |
| `Page`, `Frame` | `Page`, `Frame` | WinUI, MAUI | Keep |
| `Popup` | `Popup` | WinUI, Avalonia | Keep; `PlacementTarget` already matches Avalonia and WPF |
| `ILightDismissTarget` | `Popup.IsLightDismissEnabled`, and a light-dismiss layer for flyouts | WinUI | |
| `IDrawContext` | `DrawingContext` | WPF, Avalonia | XForms' own API on native 2D libraries |
| `TextFormat` | `TextLayout` | Avalonia, DirectWrite | Immutable; for measuring and drawing, single- or multi-line |
| `Rectangle` | `Rect` | WPF, WinUI, Avalonia | |
| `Point`, `Size`, `Thickness`, `Color`, `Colors` | Keep | All | `float` coordinates |
| `Dimension.Auto` | Keep | | |
| `LayoutAlignment` | `HorizontalAlignment` and `VerticalAlignment` | WPF, WinUI, Avalonia | Left, Center, Right, Stretch; Top, Center, Bottom, Stretch |
| `Origin` | `Animator.TransformOrigin` | | One transform system |
| (new) | `Animator` | HotChai.Game, Jetpack Compose (`graphicsLayer`) | Render properties (translation, scale, rotation, opacity, clip, transform origin), set directly or animated |
| (new) | `View.Animator` | | The app's animator for a view: `init`-only, null by default |
| (new) | `Control.CreateOwnAnimator()` | | Protected: a control's own animator for its content, kept in a private field; nested inside the app's |
| `Task` returned by animations | `AnimationHandle` | HotChai.Game | Can be awaited or cancelled; completes with `true` when the animation finishes and `false` when it's stopped or replaced |
| `Timer` | `DispatcherTimer` | WPF, WinUI, Avalonia | No priorities |
| `PointerInputEvent` | `PointerEventArgs` | Avalonia | With pointer ID, position, button and modifier state |
| `KeyInputEvent` | `KeyEventArgs` | WPF, Avalonia | `Key` stays |
| `GestureRecognizer`, `TapGestureRecognizer`, `SwipeGestureRecognizer` | Keep | MAUI | Plus `Tapped`, `DoubleTapped` and `Holding` events on views |
| `AccessibilityRole` | Keep | SwiftUI, Flutter, React Native | Plain view property |
| `AppTheme`, `LightTheme`, `DarkTheme`, `ThemeColor` | `Theme` (design tokens: `ColorScheme`, `Typography`, `Shapes`, `Motion`), and `ThemeMode` (`Light`, `Dark`, `HighContrast`) | Flutter (`ThemeData`), Compose (`MaterialTheme`); MAUI's `AppTheme` is the mode | Typed tokens, scoped to any view, not resource dictionaries. **Done** |
| (new) | `UserControl.CreateDrawnAnimator()`, `DrawnAnimator` (scale and opacity of a control's own drawing) | WPF (`RenderTransform`, drawn) | For small effects that shouldn't cost a layer |
| `OnApplyTheme(AppTheme)` | `OnThemeChanged(Theme)` | | Runs after attach and on changes, never from a constructor. **Done** |
| `Application.Theme` (set), `AccentColor` (`ThemeColor`) | `Application.RequestedThemeMode` (null follows the system), `AccentColor` (null for the system's), `Theme` (read), `CreateTheme` | MAUI (`UserAppTheme`), WinUI (`RequestedTheme`) | **Done** |
| `ICommand` | `System.Windows.Input.ICommand` | All | Use the standard interface |

### Framework: members

| Current | Proposed | Familiar from |
|---|---|---|
| `Element.BindingContext` | (removed) | Typed bindings take an explicit source |
| `View.Size` (requested size) | `Width`, `Height` | All |
| `View.MaximumSize` | `MaxWidth`, `MaxHeight` (and new `MinWidth`, `MinHeight`) | All |
| `View.Frame` | `Bounds` | Avalonia |
| `View.IsVisible` | `IsVisible` (collapsed when false) | Avalonia, MAUI |
| `View.IsInputVisible` | `IsHitTestVisible` | WPF, WinUI, Avalonia |
| `View.InvalidateLayout()` | `InvalidateMeasure()`, `InvalidateArrange()` | All |
| `View.HandlePointerEvent(...)` | `OnPointerPressed`, `OnPointerMoved`, `OnPointerReleased`, `OnPointerCanceled`, and events | WinUI, Avalonia |
| (new) | `CapturePointer`, `ReleasePointerCapture` | WinUI, Avalonia |
| `View.IsKeyboardFocusable` | `Focusable` (and `IsTabStop` for tab order) | WPF, Avalonia; WinUI |
| `View.OnFocusChanged()` | `OnGotFocus`, `OnLostFocus`, and events | All |
| `View.OnActivate()` | Keep, or `OnInvoke` for the screen reader's invoke command | |
| `View.Scale`, `Rotation`, `Translation`, `Opacity` | `Animator.Scale`, `Rotation`, `Translation` and `Opacity`, plus `Clip` and `TransformOrigin`, on the app's `View.Animator` or a control's own animator (setting a value stops that channel's animation) | HotChai.Game, Jetpack Compose |
| `View.ScaleTo`, `RotateTo`, `TranslateTo` | `Animator.ScaleTo`, `RotateTo` and `TranslateTo` (and their `From` forms), plus `FadeTo` and `RevealTo` | HotChai.Game; MAUI (method names) |
| `View.StartContinuousRotation`, `StopContinuousRotation` | A looping `RotateFrom(0°, 360°, period, loop: true)` on the control's own animator, and cancelling its handle | HotChai.Game |
| `View.StopAnimation()` | `Animator.Stop()`, or cancelling a handle | HotChai.Game |
| (new) | `Panel.ChildTransitions` (a `LayoutTransition`): moved children slide from their old positions and added ones fade in | WinUI (`ChildrenTransitions`), Android (`LayoutTransition`) |
| (new) | `Application.RequestAnimationFrame` | Browsers (`requestAnimationFrame`) | A one-time callback before the display's next frame, for redrawing every frame |
| `View.AccessibilityLabel` | Keep | SwiftUI, Flutter, React Native |
| `UserControl.DrawBackground(IDrawContext, Rectangle)` | `Control.Render(DrawingContext)` | Avalonia |
| `UserControl.Invalidate()` | `InvalidateVisual()` | WPF, Avalonia |
| `Page.Layout` | `Page.Content` | WinUI, Avalonia, MAUI |
| `Application.NavigateTo`, `NavigateBack`, `CanNavigateBack` | `NavigateAsync(page)`, `GoBackAsync()`, `CanGoBack` | MAUI, WinUI | **Done**, with `ShowModalAsync` and `PageTransition` |
| `Application.InvokeOnUIThreadAsync` | `Dispatcher.Post` and `Dispatcher.InvokeAsync` | Avalonia, WPF | **Done** |
| `Application.OpenUri` | `Application.Services` (`Clipboard`, `Launcher`, `Preferences`, `SecureStorage`, `Share`, `HapticFeedback`) | MAUI Essentials (as instances) | **Done** |
| `Application.OpenUri` | A launcher service from the application (`OpenAsync`) | MAUI |
| `Application.MeasureText` | `TextLayout` | Avalonia, DirectWrite |
| `TextView.ForegroundColor` | `Foreground` | All |
| `TextView.HorizontalTextAlignment` | `TextAlignment` | WPF, WinUI, Avalonia |
| `TextView.WordWrap` | `TextWrapping` | WPF, WinUI, Avalonia |
| `TextView.TextTrimming` (`bool`) | `TextTrimming` (enum) | WPF, WinUI, Avalonia |
| `TextEntry.Multiline` | `AcceptsReturn` | WPF, WinUI, Avalonia |
| `TextEntry.KeyboardOptions` | `IsSpellCheckEnabled`, `IsTextPredictionEnabled` | WinUI |
| (new) | `TextBox.TextChanged` event | All |
| (new) | `TextBox.Submitted`, `IsPassword`, `MaxLength`, `IsReadOnly`, `SelectionStart`, `SelectionLength`, `SelectedText`, `Select`, `SelectAll`, `SelectionChanged` | WPF, WinUI (selection, `MaxLength`, `IsReadOnly`); MAUI (`Completed`, `IsPassword`) | `Submitted` is MAUI's `Completed` |
| `ScrollView.ScrollTo` and `VerticalOffset` | `ScrollToVerticalOffset` and `VerticalOffset` | WPF |
| `ListView.Bind(source)` | `ItemsSource` | All |
| `ListView.ItemClicked` | `ItemClick` | WinUI | **Done** |
| (new) | `ListView.SelectionMode`, `SelectedItems`, `ContainerFromItem` | WinUI | **Done** |

### Toolkit

The toolkit became `XForms.Controls` in phase 21, after taking the panels,
gestures and controls' base classes from the framework in phase 20 (see
[Layering](#layering)).

| Current | Proposed | Familiar from | Notes |
|---|---|---|---|
| `Button`, `ToggleButton`, `RadioButton` | Keep | All | |
| `RadioButtonPicker` | `RadioButtons` | WinUI | |
| `ListPicker` | `ComboBox` | All | `SelectedValue` becomes `SelectedItem` |
| `CalendarControl` | `CalendarView` | WinUI | `Calendar` in WPF and Avalonia |
| `CalendarDatePicker` | `CalendarDatePicker` | WinUI, Avalonia | Keep |
| `ProgressRing` | `ProgressRing` | WinUI | Keep |
| `Popover` | `Flyout` | WinUI, Avalonia | Centered, or attached to a target |
| `MessageBox` | `ContentDialog` | WinUI | Or keep `MessageBox` for simple alerts |
| `CommandBar`, `CommandBarButton` | Keep | WinUI (`CommandBar`) | Not `AppBarButton` |
| `MenuBar`, `MenuItem` | `MenuBar`, `MenuBarItem`, `MenuItem` | WinUI | |
| `MenuButton`, `FlyoutButton` | `DropDownButton`, or `Button` with a `Flyout` | WinUI | |
| `HyperlinkView` | `HyperlinkButton` | WinUI, Avalonia | |
| `TabBar`, `TabButton` | `TabStrip`, `TabStripItem` | Avalonia | Or `TabView` (WinUI) for closable tabs |
| `CommandListView` | Keep, as a toolkit composite | | WinUI's equivalent is `ListView` with `SwipeControl` items |
| `UICommand`, `Command` | `UICommand`, and `RelayCommand` | CommunityToolkit.Mvvm | Not `XamlUICommand` |
| `FloatingButton`, `ImageToggleButton`, `DaysOfWeekPicker` | Keep | | XForms-specific; no shared equivalent |

## Open decisions

1. **Events, overrides or both for input.** WPF, WinUI and Avalonia offer
   both (override `OnPointerPressed`, or subscribe to `PointerPressed`).
   **Recommended: both**, with overrides for control authors and events for
   app code.
2. **Markup.** Code-first stays the primary model. A later option is a
   XAML-like markup compiled by a source generator into typed code (as
   Avalonia compiles its XAML), or fluent C# markup helpers like
   CommunityToolkit.Maui.Markup. **Recommended: defer**, and keep the API
   pleasant for code-first use (object and collection initializers, `Bind`
   helpers).
3. **Toolkit names.** WinUI's names (`ComboBox`, `CalendarView`, `Flyout`)
   are the most recognizable. Some, like `CommandListView`, describe the
   toolkit's own design better than any shared name. **Recommended: use the
   WinUI names where the behavior matches, except Windows 8-era names, and
   keep XForms names where the behavior differs.**
4. **A general transform matrix.** Translation, scale and rotation cover
   almost every UI need, as the Animator's channels. **Recommended: leave out
   skew and arbitrary matrices** until a real control needs them.

## Could other UI frameworks be built on XForms?

A good test of the framework is whether another UI framework, such as one
with XAML, could be built on it using only the public framework API: no
native code, and no framework API added just for it. If it can, the
framework is complete enough for any control library.

### Feasibility

A XAML framework (WPF, WinUI or Avalonia style) is mostly managed code:
markup, the property system, styles, templates, resources, bindings, visual
states and routed events. It needs relatively little from the platform.
Avalonia shows this: its platform layer is small (windowing, input, a drawing
backend, clipboard and similar services), and everything above it is portable
C#. The XForms framework sits at about that level. A XAML layer on XForms
would also keep native text, because `TextBlock` and `TextBox` would map to
XForms' native text primitives.

A XAML layer could also bring back the mechanisms XForms leaves out, such as
dependency properties, inherited data contexts, string-path bindings and
tunneling events, implemented in the layer itself for apps that want them.
XForms just doesn't build them into the framework.

What a XAML layer needs, and whether the public framework API provides it
today:

| A XAML layer needs | Public framework API today |
|---|---|
| Markup, property system, styles, templates, resources, bindings | Not needed: these are managed code in the XAML layer |
| Layout: Measure and Arrange, custom panels | Yes: `MeasureOverride` and `ArrangeOverride`, with the same semantics |
| Popups and flyouts | Yes: `Popup`, with placement |
| Dispatcher and timers | Yes |
| Keyboard focus and key input | Yes, but only on Win32 so far |
| Shapes, `Border` and brushes: paths, rounded corners, gradients, clips | No: `DrawingContext` is too limited |
| Render transforms (translate, scale, rotate) | Partly: views have translation, scale and rotation today, but no transform origin or clip. They move to the `Animator`, so a XAML element with a render transform is created with one |
| Routed events: bubbling, pointer capture, and an input filter to build tunneling on | No: input bubbles, but capture and the filter aren't public |
| Hit-testing non-rectangular shapes | No: no hit-test override |
| Storyboards | Partly. Animations of translation, scale, rotation, opacity and clip map to `Animator` channels. Animations of layout properties aren't supported, by design. |
| Rich text: inlines and font families | No |
| Automation peers: states, values and patterns | No: only a role and a label |

Every missing item is already in this document's gap analysis and plan,
because control libraries need them too. So the answer is:
- **Today:** no.
- **After the framework phases of the plan:** yes, without native code and
  without API added just for XAML. The one deliberate limit is that
  storyboards can't animate layout properties.

### The risk: element counts

The bigger risk is performance, not capability. XAML templates create many
elements; even a simple templated `Button` is a `Border`, a
`ContentPresenter` and a `TextBlock`. In XForms today, every view has a
native object: an Android `View`, a UIKit view or a DirectComposition
visual. Thousands of them would be expensive, especially on Android.

There are two ways to address this:
1. **The XAML layer draws template subtrees itself.** It renders the
   pure-drawn parts of a template (borders, shapes, static text) into one
   owner-drawn view, with its own invalidation and hit-testing. Real
   framework views are used only for native primitives: text boxes, scroll
   viewers and lists. This needs no extra framework API, just more work in
   the layer.
2. **The framework adds lightweight, drawn-only elements.** These are
   children with no native object, drawn by their parent, like Flutter's
   render objects or WPF's visuals. This would help every control library,
   not only XAML.

The second is now part of the plan, as lightweight controls (see
[Lightweight controls and layers](#6-lightweight-controls-and-layers)).
Until they exist, a XAML layer can take the first approach, and switch to
lightweight controls once they're available.

### Proof of concept

Build a small XAML layer, **XForms.Xaml**, as a throwaway spike.

- **Project:** a separate project that references only `XForms.Framework`. The
  compiler then blocks framework internals and platform code, so whatever
  the spike needs is visible as missing public API.
- **XAML subset:**
  - `Grid`, `StackPanel`, `Border`, `TextBlock` and `TextBox`;
  - `Button` with a `ControlTemplate`;
  - `Style` with setters, and `{Binding}` to a view model;
  - a storyboard animating opacity and translation.
- **Markup:** parsed at run time with `System.Xml` to start. A source
  generator can replace the parser later. (Run-time parsing and string-path
  bindings are the spike's own choice; they don't affect the framework.)
- **Rendering:** borders and other pure-drawn template parts are drawn into
  one owner-drawn view per control (the first approach above), since
  lightweight controls don't exist yet.
- **Test:** a page built from that subset, running unchanged on Win32 and
  Android.

**Success criteria:**
- No platform-specific code in XForms.Xaml.
- No access to framework internals.
- Every framework API the spike needed either already exists or is an item
  in this document's plan. Record each one, with how the spike worked around
  it until it exists.

**When:** do it before the renames in phase 1. Its main value is finding
gaps and showing which phases of the plan matter most. Because it's
throwaway, the renames won't make it obsolete in any way that matters.

### Proof of concept results

**Verdict: yes.** The spike is in `spikes/XForms.Xaml`: a run-time parser of
about 1,000 lines with a handful of elements. It references only
`XForms.Framework`. The test app's XAML page (`XFORMS_TEST_PAGE=xaml` on
Win32, `--es page xaml` on Android) runs unchanged on Win32 and Android. It
covers:
- implicit and keyed styles, `{StaticResource}` and color resources;
- a `Grid` with Auto and star sizes, plus a `StackPanel`;
- a `Border`;
- a `Button` built from a `ControlTemplate` with `{TemplateBinding}`, running
  an `ICommand`;
- `{Binding}` to an `INotifyPropertyChanged` view model;
- a storyboard that fades and slides the card in.

All three success criteria hold:
- XForms.Xaml contains no platform code.
- It uses no framework internals.
- Every missing API it hit is already in the plan below, apart from the
  four marked **new**, which are now added to it.

| Gap the spike hit | Workaround in the spike | Plan phase |
|---|---|---|
| A custom layout can't manage its own children: `LayoutViewCollection`'s constructor and `Layout`'s child methods are internal | The panels derive from `DistributedStackLayout`, whose child collection is public, and replace its layout logic | 2, **done**: `Panel` with a public `ViewCollection` |
| No `Padding` | The `Border`'s padding becomes its child's margin | 2, **done** |
| No rounded rectangles | `CornerRadius` is accepted but drawn square | 2, **done**: `FillRoundedRectangle` and `DrawRoundedRectangle` |
| No visual states (pressed, pointer over) | The button dims its template root while pressed | 4 (pointer events), toolkit |
| `TextEntry` raises no event when its text changes | None: two-way bindings behave as one-way | 3, **done**: `TextBox.TextChanged`, and views raise `PropertyChanged`; the spike's `TwoWay` bindings work |
| No opacity animation, and no frame callback | Opacity is stepped on the UI thread by a timer; translation uses `TranslateTo` | 6, **done**: `FadeTo` and `Application.RequestAnimationFrame` |
| No loaded or attached event | The storyboard starts in a UI-thread callback queued from the page's constructor | 1, **new**: `Attached` and `Detached` events, which come with creating renderers at attach |
| `XForms.Timer` hides `System.Threading.Timer` in any file that uses `XForms` | Fully qualified name | 1, **new**: rename it to `DispatcherTimer` |
| The primitives look different on each platform: `Control.BackgroundColor` is painted behind owner-drawn controls on Android but not on Win32, the default text color is black on Win32 and Android's gray, and `TextEntry`'s background is white on Win32 and gray on Android | Owner-drawn spike elements report a transparent background | 2, **done**: the spike's workarounds are gone |

**What worked as is:**
- `Element.AttachedProperty` carried `Grid.Row` and `Grid.Column`. Phase 2
  replaces it with typed layout parameters, which a XAML layer maps from the
  same attribute syntax.
- Owner-drawn `UserControl`s were enough for `Border`, `ContentPresenter` and
  the templated `Button`.
- The compositor's `TranslateTo` played the slide-in.

**Framework bugs it found, now fixed:**
- Setting `TextView.Text` didn't remeasure the view, so new, longer text
  kept the old size. `Text` now invalidates layout.
- On Android, arranging a view at exactly its measured size could lose a
  pixel when its edges were rounded separately, so text wrapped and was
  clipped. The arranged size is now never smaller than the frame's size
  rounded up.

**What it means for the plan:**
- The order holds:
  - Phases 2 (panels, padding), 5 (rounded rectangles) and 6 (opacity) are
    what a XAML layer misses most.
  - Phase 3's typed bindings don't block it: a XAML layer can keep
    reflection bindings, or generate typed ones.
- Element counts weren't tested: the page is small. The benchmark (see
  [benchmarks.md](benchmarks.md)) covers that risk, and phase 8 answers it.

## Plan

Each phase leaves the framework and toolkit building and SoonerOrLater
working. Do the renames first, while SoonerOrLater is the only app to update.

Before phase 1 (both done):
- Run the XForms.Xaml proof of concept described above. See
  [Proof of concept results](#proof-of-concept-results).
- Run the native-object benchmark described in
  [Lightweight controls and layers](#6-lightweight-controls-and-layers). See
  [benchmarks.md](benchmarks.md).

Neither changed the order of the phases. Both added items, which are below.

1. **Renames and the control contract.**
   - Rename types and members to the proposed vocabulary, and update the
     toolkit, test apps and SoonerOrLater.
   - Document that a control may or may not have its own native object, and
     make sure no public API exposes or assumes one.
   - Move translation, scale, rotation and opacity off `View`. Add the
     app's `init`-only `View.Animator` and the protected
     `Control.CreateOwnAnimator()`, which throws after the view is attached.
     At first a view with an animator works the same as any other view (one
     native object each); the animation phase fills in the animators and
     their nested layers.
   - Create renderers when a view is first attached, instead of in
     `Element`'s constructor, and raise `Attached` and `Detached` events.
   - Rename `XForms.Timer` to `DispatcherTimer`, so it no longer hides
     `System.Threading.Timer`.

   **Status: done**, except the renames that belong with a later phase's
   change of behavior.
   - **Renamed:**
     - Framework types: `Panel`, `DockPanel`, `Dock`, `UniformGrid`,
       `TextBlock`, `TextBox`, `ScrollViewer`, `Rect`, the
       `HorizontalAlignment` and `VerticalAlignment` enums,
       `PointerEventArgs`, `KeyEventArgs` and `DispatcherTimer`.
     - Framework members: `Width`, `Height`, `MaxWidth`, `MaxHeight`,
       `Bounds`, `IsHitTestVisible`, `Focusable`, `InvalidateMeasure` (plus
       `InvalidateArrange`), `InvalidateVisual`, `Page.Content`,
       `Foreground`, `TextAlignment`, `TextWrapping`, `TextTrimming`,
       `AcceptsReturn` and `ScrollToVerticalOffset`.
     - Toolkit: `ComboBox`, `CalendarView`, `Flyout`, `HyperlinkButton`,
       `TabStrip`, `TabStripItem` and `RadioButtons`, with `SelectedItem`
       and `SelectionChanged`.
   - **The Animator:**
     - `View.Animator` (`init`-only) and `Control.CreateOwnAnimator()`
       (throws once attached) replace the transform properties on `View`.
     - Pages get an animator from the framework.
     - Toolkit controls that animate themselves use own animators. A
       toolkit control that animates views passed in by the app uses its
       own animator on them (`FloatingButton.FlyoutAnimator`), so the
       app's `Animator` stays the app's.
     - Every animator of a view still drives the view's one native object.
   - **Renderers created at attach:**
     - Views keep their values and push them when the renderer is created.
     - Containers attach their children, raising `Attached` and `Detached`.
     - `Renderer` is internal.
     - `Element.OnLoaded`, which ran before any derived constructor, is
       gone.
   - **Moved to the phase that changes the behavior:**
     - `DrawingContext` and `Render` (phase 5).
     - `TextLayout` (phase 5).
     - `OnGotFocus` and `OnLostFocus` (phase 4).
     - `ListView<T>`, `ListViewItem` and `ItemClick` (phase 10).
     - Async navigation and `Dispatcher` (phase 11).
     - `RelayCommand` (phase 3).
   - **Still to do:**
     - Make the renderer interfaces and `Application.Renderer` internal.
     - Theme overrides ran from `Element`'s constructor, before derived
       constructors. **Done in phase 7:** `OnThemeChanged` runs after
       attach.
2. **Panels and view basics.**
   - Add `Grid`, `StackPanel`, `Border`, `WrapPanel` and `Canvas`, each with
     typed layout parameters for its children.
   - Add a public `Panel` base class, so that libraries can write their own
     panels and manage their children.
   - Add `Padding`, minimum sizes, `CornerRadius`, `ClipToBounds`, z-order,
     `IsHitTestVisible` and right-to-left `FlowDirection`.
   - Give the primitives one default look on every platform: transparent
     backgrounds unless set, with the same text color and the same
     text-entry background everywhere.

   **Status: done.**
   - **Panels:**
     - `StackPanel` (`Orientation`, `Spacing`).
     - `Grid` (`GridLength` Auto, fixed and star rows and columns,
       `RowSpacing` and `ColumnSpacing`,
       `Children.Add(view, row, column, rowSpan, columnSpan)`).
     - `WrapPanel` (`ItemSpacing`, `LineSpacing`).
     - `Canvas` (`Children.Add(view, left, top)`).
     - `Border` (`Background`, `BorderBrush`, `BorderThickness`,
       `CornerRadius`, `Padding`, `Child`).
   - **Custom panels:** a library derives from `Panel`, creates a
     `ViewCollection` or `ViewCollection<TLayout>` for its children's typed
     layout parameters, and overrides `MeasureChildren` and
     `ArrangeChildren`. `DockPanel` now works this way, without an attached
     property.
   - **View basics:**
     - `Padding` on panels, content controls and `Border`, applied by the
       shared layout engine.
     - `MinWidth` and `MinHeight`.
     - `ZIndex`: the native children are kept in z-order.
     - `ClipToBounds`: a visual clip on Win32, an outline clip on Android,
       `ClipsToBounds` on iOS.
     - `FlowDirection`: inherited, and right to left mirrors the children's
       positions (text and drawing aren't mirrored yet).
     - The drawing API gained rounded rectangles.
   - **One default look:**
     - Controls are transparent unless they give themselves a background;
       list rows use the theme's background.
     - Owner-drawn controls get their background from their drawing only,
       on every platform.
     - Android's `Clear` no longer blends over a native background.
     - Text defaults to the theme's foreground color, and text boxes to its
       subtle background.
   - **The XAML spike** now uses the framework's `Grid`, `StackPanel` and
     `Border`, maps `Grid.Row` and `Grid.Column` to the typed cell, and
     needs none of its workarounds for layout or backgrounds.
   - **Fixed on the way:**
     - Phase 1 still created renderers early: `InvalidateMeasure` went
       through the creating `Renderer` property. It now uses the
       created-only one, so renderers are created at attach.
3. **Properties and binding.**
   - Add observable properties on views.
   - Add typed, compiled bindings with an explicit source.
   - Add command support on controls.
   - Remove `BindingContext`.

   **Status: done.**
   - **Observable views:**
     - `View` implements `INotifyPropertyChanged`, with `OnPropertyChanged`
       and `SetProperty` helpers for control authors.
     - The framework's and toolkit's properties raise it, including values
       the user changes: `TextBox.Text`, `ToggleButton.IsChecked`,
       `ComboBox.SelectedItem`, `CalendarDatePicker.SelectedDate` and
       `RadioButtons.SelectedItem`.
   - **Typed bindings:**
     - Bindable properties have a descriptor, such as `TextBlock.TextProperty`
       (a `ViewProperty<TView, TValue>`: a name and compiled accessors, with
       no property engine behind it).
     - `view.Bind(TextBlock.TextProperty, vm, vm => vm.Title)` binds one
       way, re-reading the lambda when the source raises `PropertyChanged`.
       Adding a setter lambda binds both ways.
     - The view keeps its bindings, and the source refers to them weakly,
       so view models don't keep views alive. `Bind` returns a `Binding`
       to dispose early.
   - **Commands:**
     - Controls take `System.Windows.Input.ICommand`; XForms' own
       `ICommand` is gone.
     - `Button` gained `Command` and `CommandParameter`, and is enabled
       while the command can run.
     - The toolkit's `Command` became `RelayCommand`, with `CanExecute` and
       `NotifyCanExecuteChanged`.
   - **No `BindingContext`:**
     - Command bars and buttons pass a `CommandParameter`.
     - List rows get their item through `ListViewItem.Item` and
       `OnItemChanged`, until phase 10's typed `ListView<T>`.
     - SoonerOrLater's task page takes its view model as a property.
   - **Pulled forward from phase 9:** `TextBox.TextChanged`, reported by each
     platform's native editor, which two-way binding needs.
   - **Fixed on the way:** `DrawImage` with no bitmap now draws nothing on
     every platform; Android and iOS threw.
   - **Not done:** nested paths (`vm.Address.City`) update only when the
     source itself raises a change. Bindings expect changes on the UI
     thread.
4. **Input.**
   - Add pointer, wheel, key and focus events and overrides, with multiple
     pointers and explicit pointer capture.
   - Add an application-level input filter.
   - Add `Tapped`, `DoubleTapped` and `Holding`, plus pinch and pan
     recognizers.
   - Add key routing on Android and iOS.

   **Status: done.**
   - **Pointers:**
     - `PointerEventArgs` has a pointer ID, device type (touch, mouse,
       pen), position relative to the receiving view, key modifiers, wheel
       delta, `OriginalSource` and `Handled`.
     - Views have `OnPointerPressed`, `OnPointerMoved`, `OnPointerReleased`,
       `OnPointerCanceled` and `OnPointerWheelChanged`, with matching
       events. These replace the public `HandlePointerEvent`.
     - `CapturePointer` keeps a pointer's moves from scrolling parents.
     - Win32, Android and iOS deliver every finger, not just the first.
       Win32 sends the wheel to the views under the pointer before
       scrolling, and Android sends it as well.
   - **Gestures:**
     - `Tapped`, `DoubleTapped` and `Holding` events.
     - `PanGestureRecognizer` (with allowed directions) and
       `PinchGestureRecognizer`.
     - Single-pointer recognizers follow only their first pointer.
   - **Focus and keys:**
     - `GotFocus`/`LostFocus` and `KeyDown`/`KeyUp`, with the overrides
       `OnGotFocus`, `OnLostFocus`, `OnKeyDown` and `OnKeyUp`.
       `KeyEventArgs` has `Handled`, modifiers and more keys (letters,
       digits, function keys).
     - Keys go to the focused view, then its ancestors.
     - Android and iOS now route keys and keyboard focus too.
   - **Application filters:** `Application.PreviewPointerInput` and
     `PreviewKeyDown` see every event before the views do.
   - **Fixed on the way:** `DispatcherTimer` ran its action off the UI
     thread, and one shared counter meant starting any timer cancelled the
     others. It now works as in WPF (`Interval`, `Tick`, `Start`, `Stop`),
     ticking on the UI thread.
   - **Verified:**
     - On Win32 and Android: taps, double taps, holds, pans, focus and keys
       (input test page).
     - On Win32: the wheel, and a pinch with injected two-finger touch.
     - Not verified: pinch and wheel on Android, and all of iOS, which is
       only built.
   - **Not done:** pointer enter and exit (hover tracking).
5. **Drawing and text.**
   - Expand `DrawingContext` on Direct2D, Core Graphics and Android `Canvas`:
     paths, rounded rectangles, immutable brushes, push/pop of clip,
     transform and opacity, and `TextLayout`.
   - Add font families and bundled fonts, text spans, and maximum lines.

   **Status: done.**
   - **`DrawingContext`:**
     - It's an abstract class (it was the `IDrawContext` interface).
       Owner-drawn controls override `Render(DrawingContext)`, which was
       `DrawBackground(context, bounds)`.
     - It gained immutable brushes (`SolidColorBrush`,
       `LinearGradientBrush`) and `PathGeometry` (lines, quadratic and cubic
       curves, and SVG-style arcs, which become curves).
     - It gained `PushClip`, `PushTransform` (a `Matrix3x2`) and
       `PushOpacity` (group opacity), each undone by `Pop` or by disposing
       what the push returns.
     - Implemented on Direct2D, Android's Canvas and Core Graphics.
   - **`TextLayout`:**
     - Text laid out once with a `TextFormat`, within a maximum size, and
       drawn with `DrawText(layout, origin, color)`. It replaces
       `Application.MeasureText` (`TextLayout.Measure` measures in one
       call).
     - `TextFormat` gained a font family, style, wrapping, trimming and
       `MaxLines`.
     - Every text drawing call goes through it, on DirectWrite,
       StaticLayout and TextKit.
   - **`TextBlock`:**
     - `FontFamily` and `MaxLines`.
     - `Inlines`: `Run`s with their own weight, style, color, underline and
       strikethrough.
   - **Fonts:**
     - Generic families as in CSS (`FontFamilies.Serif`, `SansSerif`,
       `Monospace`) map to a matching system font on each platform: Cambria,
       Segoe UI and Consolas on Windows; serif, sans-serif and monospace on
       Android; New York, San Francisco and SF Mono on iOS. They're the
       portable choice, with nothing to bundle.
     - `Application.RegisterFont` loads a font file (from the app's folder,
       Android assets or the iOS bundle) under a family name, for brand and
       icon fonts. Windows uses a DirectWrite custom collection.
   - **Fixed on the way:**
     - Win32 text blocks ignored `TextTrimming`.
     - Android's text block `FontWeight` threw when read, and did nothing
       when set.
     - iOS's `VerticalTextAlignment` threw when read.
   - **Verified** on Win32 and Android (drawing test page), including a
     registered font on Win32. On Android, a font registered from assets
     isn't verified, and iOS is only built.
6. **Animation.**
   - Fill in the `Animator` (added in phase 1): translation, scale,
     rotation, opacity, clip and transform origin. Setting a value stops that
     channel's animation.
   - Give each animator its own nested compositor layer: the app's outside,
     a control's own inside, holding the control's drawing and children.
   - Give the framework's pages, popups, scrolled content and list rows an
     Animator.
   - Its animations return an `AnimationHandle`, with easing, springs, delays
     and looping.
   - Each platform's compositor plays the animations, on the view's own
     native properties: DirectComposition, Core Animation, and Android's
     render-thread animators.
   - Toolkit: controls that animate their own content (such as
     `ProgressRing`) use `CreateOwnAnimator()`, and small effects (such as
     button press feedback) become drawn animations. No toolkit control sets
     its public `Animator`.
   - Move the current `ScaleTo`, `TranslateTo` and `RotateTo` onto the
     Animator, and replace `StartContinuousRotation` with a looping rotation.
   - Add a transition panel that gives each child an `Animator` (or wraps
     a child created without one) and animates layout changes with
     `TranslateFrom` and `RevealFrom`.
   - Add a per-frame callback for controls that redraw every frame.

   **Status: done.**
   - **`Animator`:**
     - It holds ten channels: translation, scale and rotation, opacity, and
       a clip's four edges, around a transform origin. Reading a value
       during an animation gives where it is now.
     - `To` and `From` methods (`TranslateTo`, `ScaleFrom`, `RotateFrom`,
       `FadeTo`, `RevealTo`, `RevealFrom` and so on) take an easing, a delay,
       looping and auto-reverse.
     - They return an `AnimationHandle`. It can be awaited (`true` when the
       animation finishes, `false` when it's stopped or replaced) or
       cancelled, and it completes on the UI thread.
     - Setting a value stops that channel's animation.
     - `EasingFunction` gained `Ease(t)`, so platforms can sample any curve.
       `SpringEase` is a real spring, and there's a linear easing.
   - **Layers:**
     - Each animator gets its own native layer when the view is attached,
       through `IViewRenderer.CreateAnimationLayers`. The app's (or the
       framework's) is outermost, and a control's own are inside it.
     - The animation members are gone from `IViewRenderer`.
     - Win32: a composition visual per layer, with a translate, scale and
       rotate transform group, opacity and a rectangle clip. Easing curves
       become cubic segments that the compositor plays.
     - Android: the layers are composed onto the view's own render
       properties (a layer's translation moves with the scale and rotation
       outside it). Animations are value animators with the easing sampled
       into a table. The clip is the view's outline (`ClipBounds` didn't
       clip). Phase 7 replaced the first version's wrapper views: a parent
       clips each child to the child's bounds, which cut off inner layers
       that moved out of their wrapper.
     - iOS: the view's layer, with wrapper views for outer layers, and
       keyframe animations of the layer's transform, opacity and a mask.
   - **Pages** get a framework animator for frame transitions.
   - **Layout transitions:** `Panel.ChildTransitions` (a `LayoutTransition`)
     slides children that move from their old positions and fades added
     ones in. Its children get framework animators when they're attached.
     This replaces the planned transition panel: any panel can animate its
     layout.
   - **Frame callback:** `Application.RequestAnimationFrame` calls back
     once before the next frame. It uses `DwmFlush` on Win32, `Choreographer`
     on Android and `CADisplayLink` on iOS.
   - **Toolkit:** `ProgressRing` spins with a looping `RotateFrom`, and
     continuations of animations use the handle's `Completion`.
   - **Verified** on Win32 and Android (animation test page): looping,
     nested animators, a spring, a reveal, an interrupted move, layout
     transitions and the frame callback. SoonerOrLater runs unchanged on
     Android, including swipes and its undo button. iOS is only built.
   - **Not done:**
     - The framework's popups, scrolled content and list rows don't get
       animators yet; list rows come with phase 10.
     - Win32's frame callback waits for each frame on a pool thread, which
       sometimes misses one (about 48 frames per second on a 60 Hz
       display).
     - Hit testing on Win32 follows translation, but not scale or rotation.
7. **Theming.** (See [Theming](#7-theming-a-mode-design-tokens-and-styles).)
   - Add `ThemeMode`, reported by each platform with its changes: the
     system's dark mode, accent color and high contrast.
   - Replace `AppTheme` with `Theme`: a `ColorScheme` with semantic roles,
     `Typography`, `Shapes` and `Motion`, with a theme for each mode.
   - Let any view set a `Theme` for itself and its descendants, and give
     each view its effective theme.
   - Replace `OnApplyTheme` with `OnThemeChanged`, after attach.
   - Move the toolkit's colors, corner radii, text sizes and durations onto
     the theme's tokens, and add typed styles to the controls that need
     them (buttons first).
   - Update SoonerOrLater, and test both modes and high contrast.

   **Status: done.**
   - **`ThemeMode`** (`Light`, `Dark`, `HighContrast`). Each platform reports
     the system's mode and its changes:
     - Win32: the "apps use light theme" setting, and high contrast, on
       `WM_SETTINGCHANGE` and `WM_SYSCOLORCHANGE`. The title bar follows the
       app's mode.
     - Android: the night mode and, on Android 14 and later, high contrast,
       when the configuration changes. Activities declare
       `ConfigChanges.UiMode`, so dark mode restyles the app in place
       instead of recreating it.
     - iOS: the trait collection's style and "increase contrast", when the
       app becomes active or the contrast setting changes.
   - **The application's theme:**
     - `RequestedThemeMode` is null to follow the system, or the mode the app
       chooses. The system's high contrast always wins.
     - `AccentColor` is null for the system's accent (Windows' accent, or
       Android 12's wallpaper color) or the default blue.
     - `CreateTheme(mode, accent)` builds the theme, and an app overrides it
       for a brand. `InvalidateTheme()` rebuilds it.
     - In high contrast, Win32 uses the system's contrast colors.
   - **`Theme`**, an immutable record: a `ColorScheme` (accent, background,
     surface and error, each with its "on" color, plus subtle foreground,
     outline and disabled), `Typography` (display, title, body, label,
     caption), `Shapes` (corner radii) and `Motion` (durations and easings).
     `Theme.Create(mode, accent)` builds XForms' themes, and `WithStyle`
     adds default control styles.
   - **Scoped:** `View.Theme` sets a theme for a view and its descendants,
     and `ActualTheme` is the effective one (a popup inherits from the view
     it opens from).
   - **`OnThemeChanged(Theme)`** replaces `OnApplyTheme`. It runs once the
     element is attached (after every constructor, with its native object)
     and whenever its effective theme changes, never from a constructor.
     Elements no longer subscribe to a static event (which leaked them).
   - **Toolkit:**
     - Every color comes from `ActualTheme.Colors`, and animation durations
       from `ActualTheme.Motion`.
     - `Button` takes a `ButtonStyle` (background, foreground, corner radius,
       text style), over the theme's default `ButtonStyle`, over the
       tokens. Buttons are rounded, and alert buttons use the error colors.
   - **SoonerOrLater** maps its light and dark setting to
     `RequestedThemeMode`, keeps one accent color (the theme lightens it in
     dark mode), and its list items no longer skip their first theme call.
   - **Fixed on the way:**
     - Android clipped every child to its bounds, unlike the other
       platforms. Layout views no longer clip unless `ClipToBounds` is set.
       This shows SoonerOrLater's flyout menu and its focus toggles, which
       sit outside their parents' bounds.
     - Android's animation layers are composed rather than wrapped (see
       phase 6).
   - **Verified:**
     - Win32 (theme test page): light, dark, high contrast, switching
       modes while running, the system accent color, a scoped dark section
       with its own button style, and button styles.
     - Android: switching the system's dark mode while running, the
       wallpaper accent color, and SoonerOrLater in light mode.
     - iOS is only built.
   - **Not done:**
     - Android can't tap a view moved outside its parent's bounds, such as
       SoonerOrLater's flyout menu items (they were hidden before), so its
       settings can't be opened on Android. Framework hit-testing (phase 8)
       fixes this.
     - SoonerOrLater in dark mode isn't verified on a device (its setting
       is behind that menu).
     - Text boxes have no outline, so in high contrast (where the surface
       and background are both black) a text box's edge doesn't show.
     - The toolkit's other controls take colors from the theme, but not yet
       styles of their own; their text sizes are still fixed.
     - A Win32 text box crashed once in RichEdit while the theme changed
       (an access violation in `SendMessage`). It didn't recur in 21
       automated switches; it looks like the known RichEdit crash.
8. **Lightweight controls.** After the Animator, since it decides which views
   have layers.
   - Draw owner-drawn controls, `Border`, panels and `TextBlock` into their
     nearest layer's surface, with framework layout, hit-testing, input,
     focus and invalidation.
   - Give layers only to views given an animator (one per animator, nested),
     the framework's own layered views, and native primitives. Nothing is
     promoted at run time.
   - Expose lightweight controls through virtual accessibility nodes on every
     platform.
   - Rerun the benchmark to confirm the gain.

   **Status: done.** (Accessibility on Android and iOS came in phase 12.)
   - **Which views are lightweight** is decided when a view's renderer is
     created: owner-drawn and content controls, panels and text blocks, with
     no animator, whose parent allows it. Pages, list rows, scroll views'
     content and popups' content are layers; so are views with animators,
     and native primitives (text boxes, images, lists, scroll views). An
     `AppContext` switch (`XForms.DisableLightweightViews`) turns lightweight
     views off, for comparison.
   - **Android and iOS** use the framework's lightweight renderers:
     - The layer's native view draws its lightweight descendants beneath its
       subviews, each translated to its place (and clipped, with
       `ClipToBounds`).
     - Native views inside lightweight ones are hosted by the layer, in
       drawing order. Their frames are offset by the lightweight ancestors'
       positions.
     - The framework hit-tests lightweight views and routes pointers: the
       deepest view under a press first, then its ancestors. The view that
       takes a press gets the pointer's moves and release.
     - Panels map their children to native indices, skipping lightweight
       ones.
   - **Win32** keeps every view in its renderer tree, so its hit testing,
     focus, scrolling and UI Automation are unchanged. A lightweight renderer
     has no visual or surface: its layer draws it, and the visuals of native
     views inside it go in the layer's visual.
   - **`DrawnAnimator`**, from `UserControl.CreateDrawnAnimator()`: a scale
     and opacity a control applies to its own drawing, animated frame by
     frame. The toolkit's buttons, tabs, toggles and radio buttons use it for
     press feedback, so they're lightweight too. Animations that move a view
     (the flyout, swipes) or run on the compositor (the progress ring) keep
     their animators.
   - **Benchmark** (1,000 tiles; see [benchmarks.md](benchmarks.md)):
     - Win32: about 24 MB of private memory instead of 29.5 MB, and the first
       frame in about 0.35 s instead of 0.6 to 1 s.
     - Android: 63 native views instead of 1,164, less managed memory
       (5.2 MB instead of 6.9 MB) and a faster first frame.
     - SoonerOrLater's main page has 32 native views on Android.
   - **Fixed on the way:**
     - On Android, views moved outside their parent's bounds couldn't be
       tapped. A lightweight container's native children are hosted by the
       layer, so SoonerOrLater's flyout menu (and its Settings page) works.
     - Theme changes didn't reach a scroll view's content, list rows, or
       pages in a frame's back stack.
     - A popup's content never had its own `Bounds`, so drop-downs drew no
       frame.
     - `RadioButtons` compared values with `==` on `object`, which failed for
       value types such as enums.
     - Android popups anchor to a lightweight target's place in its layer.
   - **Verified** on Win32 and Android: every test page, input (taps, holds,
     pans, keys and focus on Win32), drop-downs and the date picker, theme
     switching, and SoonerOrLater (tabs, swipes, the flyout menu, Settings,
     dark mode). iOS is only built.
   - **Not done:**
     - On Android and iOS, lightweight views had no keyboard focus or
       accessibility nodes. Phase 12 added both.
     - Lightweight views draw within their layer's surface: what they draw
       outside it is clipped, and a native view inside a hidden or clipping
       lightweight view is hidden but not clipped.
     - Large layers still need tiled or virtual surfaces on Win32 and iOS
       (the benchmark's scrolled content is one tall surface).
9. **Text entry.** Add `TextChanged`, a submitted event, password mode,
   maximum length, read-only and a selection API.

   **Status: done.**
   - **`TextBox`** gained:
     - `Submitted`: Enter, or the keyboard's Done key, in a single-line box.
       A box that accepts Return never submits.
     - `IsPassword` (dots, no suggestions or spell checking).
     - `MaxLength` (what the user types or pastes; 0 for no limit).
     - `IsReadOnly` (the text can be selected, not changed).
     - `SelectionStart`, `SelectionLength`, `SelectedText`, `Select`,
       `SelectAll` and `SelectionChanged`.
   - **Platforms:**
     - Win32: RichEdit's password character, text limit and read-only mode,
       its selection messages and selection-change notifications. Return in
       a single-line box submits instead of beeping.
     - Android: the input type (password variation), a length filter,
       removing the key listener for read-only, the editor action for Done,
       and an `EditText` subclass that reports selection changes.
     - iOS: `SecureTextEntry`, change checks for the length and read-only,
       the Return key, and `UITextInput` selection.
   - **Fixed on the way:** the native object's own change notifications,
     while it was receiving the text box's values, overwrote the text (a
     password box set from code came up empty on Android).
   - **Verified** on Win32 (a text entry test page): submitting, a
     password, the length limit, read-only text, selecting, and Return in a
     multiline box. On Android: the password and selecting. Typing on the
     Android emulator wasn't verified: the keyboard shows a stylus tutorial
     whenever a text box takes focus. iOS is only built.
10. **Lists.** Add `ListView<T>` with typed item sources and item views that
   are created once and bound to many items, plus grouping, horizontal and
   grid layouts, and selection modes.

   **Status: done**, except horizontal and grid layouts.
   - **`ListView<T>`:**
     - `ItemsSource` is an `IReadOnlyList<T>`. An observable collection
       updates the list as it changes (without groups, the change passes
       through, so the platform list animates it).
     - `ItemViewFactory` (`ItemViewFactory<T>.Create(create, bind)`) makes
       item views and binds them; a view is made once and rebound as the
       list scrolls, and a factory reuses only views it made.
     - `GroupKeySelector` groups consecutive items with the same key under a
       header (`GroupHeaderFactory`, by default the key as text).
     - Typed `SelectedItem` and `SelectedItems`, and `ScrollIntoView`.
   - **`ListView`:**
     - `SelectionMode` (`None`, `Single`, `Multiple`), `SelectedItem`,
       `SelectedItems`, and `SelectionChanged` with added and removed items.
       Tapping a selected item again (single) unselects it.
     - `ItemClick` (`ItemClickEventArgs.ClickedItem`) replaces `ItemClicked`.
     - `ContainerFromItem` replaces `SelectedItemContainer`.
   - **Renames:** the container is `ListViewItem` (it was
     `ListViewItemContainer`), as in WinUI and Avalonia. It draws the row's
     background and selection, and `IsSelected` tells it. Item views are any
     view (the old item-view base class is gone), and
     `ListViewItem.ContainerFromContent` finds a view's container.
   - **Toolkit:** `CommandListView`'s items select with their command bar.
     `CommandListViewItem` (a content control) keeps an `Item` and
     `OnItemChanged` for the views it hosts.
   - **SoonerOrLater:** its item sources return any view and set the item.
   - **Verified** on Win32 and Android (a lists test page: groups,
     selection modes, adding and removing), the text and list page on
     `ListView<T>`, and SoonerOrLater's command list. iOS is only built.
   - **Not done:**
     - Horizontal and grid layouts. Each platform's list is vertical
       (Android's `ListView`, `UITableView`, and Win32's own); horizontal and
       grid layouts need a framework virtualizing layout (as WinUI's
       `ItemsRepeater`), which lightweight views now make practical.
     - Pull-to-refresh and incremental loading (from the gap analysis).
11. **App layer.**
   - Asynchronous, instance-based navigation, with parameters, results,
     modal pages and transitions.
   - Platform services, as interfaces from the application.
   - Windows on desktop.
   - System text-scale changes.

   **Status: done**, except file pickers and secondary windows.
   - **Navigation:**
     - `Application.NavigateAsync(page, transition)`, `GoBackAsync()` and
       `CanGoBack` replace `NavigateTo`, `NavigateBack` and
       `CanNavigateBack`. Parameters go to the page's constructor, and each
       method completes when its transition has.
     - Modal pages: `ShowModalAsync(page)` completes when the page closes
       (by Back, or `Page.CloseModalAsync`), and
       `ShowModalAsync<TResult>(ModalPage<TResult>)` completes with the result
       the page closes with (`ModalPage<TResult>.CloseAsync(result)`).
     - `PageTransition`: a slide (the default), sliding up (modal pages), a
       fade, or none, played by the pages' animators.
     - `Frame` gained `NavigateAsync`, `GoBackAsync`, `ReplaceAsync` (for
       tabs, which was `NavigateInPlace`) and `CurrentPage`. Its renderers
       keep pages in the frame and bring the arriving one to the front, so
       two pages show during a transition (Android and iOS used to replace
       the frame's only child). Pages leaving the back stack are removed.
     - Android's Back key and gesture go back in the app
       (`OnBackInvokedCallback`, registered only while there's somewhere to
       go back to, so the system's predictive back still leaves the app).
   - **`Dispatcher`** (`Post`, `InvokeAsync`, `CheckAccess`) replaces
     `Application.InvokeOnUIThreadAsync`.
   - **Services**, from `Application.Services`, each an interface the app can
     replace: `IClipboard`, `ILauncher`, `IPreferences` (with typed `Get` and
     `Set`), `ISecureStorage` (DPAPI, the Android Keystore, the iOS
     Keychain), `IShare` and `IHapticFeedback`. On Windows, sharing copies to
     the clipboard (the share sheet needs a packaged app), and there are no
     haptics.
   - **`Application.MainWindow`**: `Title`, `Size` and `SizeChanged`, and on
     Windows `Resize` and `MinimumSize`.
   - **Text size:** font sizes follow the system's text size on every
     platform (Android's scaled pixels already did; Windows' "make text
     bigger" and iOS's Dynamic Type now do too). `Application.TextScale` and
     `TextScaleChanged` report it, and views lay out again when it changes.
   - **Verified** on Win32 (an app layer test page: forward, back, a fade,
     a modal page's result, preferences, secure storage, the window title
     and size) and Android (the same, plus the clipboard and the Back key),
     and SoonerOrLater's tabs, Settings and Back on Android. iOS is only
     built. The clipboard and share on Windows weren't run (they'd replace
     the user's clipboard), nor sharing anywhere (it opens a system sheet).
   - **Not done:**
     - File pickers, connectivity and safe-area insets (from the gap
       analysis).
     - Secondary windows on the desktop: the Win32 host is one window.
     - Changing the text size wasn't tested (it's a system setting).
12. **Ongoing.** Accessibility (states, values, actions; Android and iOS) and
    keyboard focus on every platform, alongside the phases above.

    **Status: done** for this plan; accessibility stays ongoing as controls
    are added.
    - **Framework:** `AccessibilityIsChecked`, `AccessibilityIsSelected` and
      `AccessibilityValue`; `AccessibilityAction` (invoke, toggle, select)
      with `IsAccessibilityActionSupported` and `PerformAccessibilityAction`;
      `NotifyAccessibilityStateChanged`. New roles: `RadioButton` and
      `ComboBox`. The toolkit's toggle buttons, tabs, radio buttons (which
      weren't exposed before), pickers and floating buttons, and list items,
      report their states and values.
    - **Win32:** the Toggle, SelectionItem and Value patterns (Value is
      read-only: pickers choose with their own popup), with property-changed
      and element-selected events.
    - **Android:** an accessibility delegate on every native view (class,
      text, checked and selected states, click); each layer's lightweight
      views are virtual nodes, with accessibility focus, clicks, touch
      exploration (hover) and change events. A view group with a node
      provider leaves its native children out, so the layer adds them, in
      reading order with the virtual nodes.
    - **iOS:** labels, values and traits on layers, and a
      `UIAccessibilityElement` for each accessible lightweight view, with
      activation.
    - **Keyboard focus:** on Android and iOS a layer's native view takes
      focus for the lightweight views inside it; the layer tracks the focused
      one, routes keys to it and draws its ring. Tab follows the view tree on
      every platform: Android's own Tab order sorts views by position, which
      put a page's layer before everything inside it, so Tab never reached
      its lightweight views. A layer that holds focus only for its
      lightweight views turns off Android's default highlight, which greyed
      the whole page.
    - **Test page:** `a11y` (a button, toggle, picker, radio buttons and a
      selectable list).
    - **Verified:**
      - Win32, with a UI Automation client: the tree, patterns and states,
        and Invoke, Toggle and Select changing the app.
      - Android, with a UI Automator dump (the nodes TalkBack reads): roles,
        text, checked and selected states and reading order, on the test page
        and in SoonerOrLater (tabs and the labeled floating buttons). Tab,
        Shift+Tab and Enter across lightweight and native views, with the
        ring.
      - iOS is only built.
      - Not run: TalkBack itself (turning it on is a system setting), so
        virtual-node clicks, accessibility focus and touch exploration on
        Android weren't exercised; they use the same framework actions as
        Win32.
    - **Not done:**
      - Text boxes on Android leave Tab to Android's own order.
      - Collapsed flyout buttons and a hidden (scaled to zero) button are
        still exposed, since they're visible to the framework.
      - Expand and collapse (for pickers), ranges (sliders) and headings.

### After the plan

These phases fill the gaps the phases above left, then add a web platform.
Each is committed when it's built for every platform and verified on Win32,
Android and, from phase 17, the browser.

13. **Hover and accessibility gaps.**
    - `PointerEntered` and `PointerExited` (hover tracking) on Win32, Android
      (hover events from a mouse or stylus) and iOS (`UIHoverGestureRecognizer`).
    - Accessibility: a `Heading` role, expand and collapse for pickers (UI
      Automation's ExpandCollapse pattern, Android's expand and collapse
      actions), and not exposing views that are scaled to nothing.

    **Status: done.**
    - **Hover:** `PointerEntered`, `PointerExited` and `IsPointerOver`. The
      framework keeps the path of views under a hovering pointer and raises
      exits deepest first and entries outermost first, as WPF does; the
      platforms only report the deepest view under the pointer (lightweight
      views are found by the framework), or none when it leaves. Win32 uses
      hover pointer messages and `WM_POINTERLEAVE`; Android the native view's
      hover events; iOS `UIHoverGestureRecognizer`. Android and iOS wait a
      turn before treating an exit as leaving the window, since moving into
      a child or neighbor exits the view first.
    - **Accessibility:** the `Heading` role (UI Automation's heading level,
      Android's heading flag, iOS's header trait); `AccessibilityIsExpanded`
      with expand and collapse actions, which pickers implement (the
      ExpandCollapse pattern on Win32, expand and collapse actions on
      Android); and views hidden, or scaled or faded to nothing by an
      animator, are left out on every platform.
    - **Flyout buttons** are hidden once collapsed, so they take no input and
      aren't exposed.
    - **Verified:**
      - Win32: hover entering and leaving a button (with a position local to
        it), and with a UI Automation client, the heading level and a
        picker's expand and collapse.
      - Android and Win32: SoonerOrLater exposes only Add and Menu until the
        flyout opens, then Settings and About, and Settings opens.
      - Not verified: hover on Android and iOS (the emulator's input tool
        can't send hover events), and Android's heading flag and expand
        actions (UI Automator's dump doesn't show them). iOS is only built.
14. **Platform services and the window.**
    - Safe-area insets as a framework value (`Page.SafeAreaInsets`), so apps
      can lay out around notches and system bars themselves.
    - Connectivity (`IConnectivity`: whether there's a network, and a change
      event).
    - File pickers (`IFilePicker`: open one or more files, and save a file).

    **Status: done.**
    - **Safe area:** `Window.SafeAreaInsets` (and `SafeAreaInsetsChanged`)
      reports what system bars, a display cutout or the soft keyboard cover.
      Pages are still kept clear of them by the platform (Android pads the
      root view; iOS pages lay out in the safe area), unless the current page
      sets `UsesSafeArea = false` to draw under them. The soft keyboard still
      pushes such a page up on Android.
    - **Connectivity:** `IConnectivity.NetworkAccess` (none, local or
      internet) and `ConnectivityChanged`, on the UI thread. Win32 reads the
      network interfaces (one that's up with a gateway counts as the
      internet); Android uses the default network's callback (a network the
      system validated reaches the internet), declaring the network-state
      permission from the platform assembly; iOS uses `NWPathMonitor`. Each
      listens to the system only while the app listens.
    - **File pickers:** `IFilePicker.PickAsync` (one or more files, filtered
      by extension) returns `PickedFile`s read with `OpenReadAsync`, since
      mobile platforms grant access to a file rather than a path;
      `SaveAsync` lets the user choose where, then writes with a callback.
      Win32 uses the common file dialogs, Android the Storage Access
      Framework (with activity results on the activity), iOS the document
      picker.
    - **Fixed on the way:**
      - UI Automation actions ran synchronously on the UI thread, so an
        action that opened a modal dialog blocked the screen reader (and
        every later call) until it closed. They now return at once, as UI
        Automation requires.
      - `Thickness` prints its values.
    - **Verified:**
      - Win32: the network status; opening a scratch text file through the
        Open dialog (its contents shown) and cancelling it.
      - Android: the network status, and going offline and back with
        airplane mode; a page drawing under the status bar and back, with
        the insets reported.
      - Not verified: saving through the Save dialog on Win32 (its file-name
        field couldn't be reached from automation, so the dialog was
        cancelled), the Android document picker (system UI), and all of iOS,
        which is only built.
15. **Lists.**
    - Pull-to-refresh (`ListView.IsRefreshEnabled`, `RefreshRequested`,
      `IsRefreshing`).
    - Horizontal lists and grids (`ListView.Orientation`, and a grid layout
      with a column count or item width).

    **Status: done.**
    - **Layouts:** `ListView.Layout` is `ListLayout.Vertical` (the default),
      `ListLayout.Horizontal`, or `ListLayout.Grid(columns)` (columns of
      equal width, scrolling vertically). A horizontal list's items are as
      tall as the list. Groups only show in a vertical list. Each platform
      uses its own list:
      - Win32: the virtualized list now realizes lines (an item, or a row of
        a grid) on either axis; the compositor scrolls sideways for a
        horizontal list, and drags and the wheel follow the axis.
      - Android: `ListView` for a vertical list, and AndroidX `RecyclerView`
        (with a linear or grid layout manager) otherwise, with the same
        native item containers.
      - iOS: `UITableView` for a vertical list, and `UICollectionView` with a
        flow layout otherwise, with cells sized by measuring their content.
    - **Pull-to-refresh:** `IsRefreshEnabled`, `RefreshRequested`,
      `IsRefreshing` and `RequestRefresh()`. Win32 draws its own indicator
      (it follows a touch pull past the top, then spins while refreshing);
      Android uses AndroidX `SwipeRefreshLayout`, iOS `UIRefreshControl`.
    - **Fixed on the way:** Android disposed a list's old adapter before
      replacing it, while the list could still call it, which crashed when
      a list was bound again.
    - **Verified:**
      - Win32: horizontal and grid layouts, and a refresh (the indicator and
        the new item) through `RequestRefresh`.
      - Android: pulling to refresh, horizontal (and scrolling it) and grid
        layouts, and SoonerOrLater's list (a row swiped away and undone).
      - Not verified: the touch pull and a horizontal wheel on Win32 (no
        touch or mouse input was sent), and iOS, which is only built.
16. **Win32 surfaces and windows.**
    - Tiled surfaces for tall layers, so a long scrolled page doesn't need one
      surface the size of all its content.
    - Secondary windows (`Window` objects an app can open and close).

    **Status: done.**
    - **Tiled surfaces:** a layer taller than 4,096 device pixels draws into
      a DirectComposition virtual surface: only a band around what's visible
      (with a margin of 1,024 pixels, in 256-pixel rows) is drawn, and the
      rest is trimmed. A scroll viewer has its content draw the range a
      scroll is about to cover before it starts. On the views benchmark page
      the app's GPU memory fell from 35.6 MB to 16.8 MB.
    - **Windows:** `Application.OpenWindowAsync(page, title, size)` opens a
      window with a frame of its own, `Application.Windows` lists the
      windows, `CanOpenWindows` says whether the platform can (Win32 only),
      and a `Window` the app opened has `Close()` and `Closed`. Themes and
      the text size reach every window.
      - Win32: each window has its own host (window, composition targets,
        root, popups, focus, pointers, text entries and UI Automation root)
        over shared graphics devices and one UI thread. Renderers find their
        window through their root; a popup opens in its placement target's
        window; a text entry's window moves into the window its view is in.
        Opened windows are owned by the main one, and closing them doesn't
        suspend the app.
    - **Verified** on Win32: the views benchmark scrolled to its end, drawn
      correctly, with the memory above; a second window opened, used (its
      own UI Automation tree, focus ring and services) and closed, with the
      main window told; every test page. Android's test pages still run.
    - **Not done:** tiling on iOS (it's only built), and wide (rather than
      tall) layers.
17. **Web platform: host, layers and drawing.** .NET 10 on WebAssembly with
    `[JSImport]` and `[JSExport]`, in `platform/web`:
    - A layer is a DOM element positioned absolutely; lightweight views draw
      into a `<canvas>` per layer, as on the other platforms.
    - Text is drawn and measured with the canvas (`fillText`,
      `measureText`).
    - Images, theming (`prefers-color-scheme`, `prefers-contrast`), the
      dispatcher (the browser's event loop) and pointer input (Pointer
      Events).
    - A test host page and a web build of the test app.

    **Status: done.**
    - **`platform/web`** (`XForms.Framework.Web`): .NET 10 on WebAssembly,
      with `[JSImport]` and `[JSExport]` and one script, `xforms.js`
      (served to apps as `_content/XForms.Framework.Web/`). Apps call
      `WebApplication.RunAsync(title, createApp)`.
    - **Layers and drawing:** each layer is an absolutely positioned element.
      Lightweight views and owner-drawn controls draw into a `<canvas>` under
      the layer's child elements, as on Android. Drawing is recorded in .NET
      as one command list per layer and replayed by the script: one call
      into JavaScript per layer, not per shape. Shapes, paths, gradients,
      shadows, images (tinted through an offscreen canvas), clips,
      transforms and opacity are supported.
    - **Layout:** the framework lays the root out to the viewport on
      animation frames, as Win32's host does; elements get their bounds in
      CSS pixels (XForms' DIPs), and canvases draw at the device pixel
      ratio.
    - **Text:** measured with the canvas and broken into lines in .NET, so
      drawn text and text elements break alike; a text block is an element
      showing those lines (styled runs as spans). Generic families (serif,
      monospace) and fonts from files (`FontFace`) work.
    - **Input:** Pointer Events go to the element under the pointer, then its
      ancestors, until one takes the press, which then gets the pointer's
      moves and release (with pointer capture); hover, leaving the page and
      the wheel are routed too. Touch pans only scroll views (the page
      itself doesn't pan or zoom).
    - **Also:** theming from `prefers-color-scheme` (live) and forced colors;
      a dispatcher on the browser's event loop; animation layers composed
      into CSS transforms and stepped on animation frames; images from the
      app's `Content` folder; the services (clipboard, launcher,
      preferences and secure storage in local storage, `navigator.share`,
      vibration, `navigator.onLine`, file inputs and downloads); suspend
      when the page is hidden.
    - **Basic versions, finished in phase 18:** text boxes (`<input>` and
      `<textarea>`), scroll views (native overflow), lists (every item, not
      yet virtualized; no pull-to-refresh), popups (over a backdrop that
      dismisses them).
    - **Test app:** `test/XForms.Test.Web`, with `?page=` picking the page.
      `dotnet publish` it and serve `wwwroot` (the SDK's development server
      didn't serve the app's files here).
    - **Verified** in the browser: the default, accessibility, drawing,
      animation, app layer and lists pages; clicks on buttons, toggles,
      radio buttons and list rows; a drop-down; navigating forward and
      back; preferences and the network status; dark mode switching live;
      list grids; native scrolling.
    - **Fixed on the way:** none in the framework; the web platform's own
      first-run bugs (a native panel given lightweight children, generic
      font names quoted in CSS, line limits without ellipses) are fixed.
18. **Web platform: controls, animation and app layer.**
    - Native primitives: `<input>` and `<textarea>` for text boxes, a
      scrolling `<div>` for scroll views, virtualized lists, and popups.
    - Animation layers with the Web Animations API.
    - Keyboard focus, and accessibility through ARIA: DOM elements carry
      roles and states, and lightweight views get hidden accessible elements
      (as Flutter's semantics tree does).
    - Navigation with the browser's history and Back button, and services
      (clipboard, local storage for preferences, `navigator.share`).
    - SoonerOrLater in the browser.

    **Status: done.**
    - **Keyboard:** focusable views' elements take focus with Tab
      (`tabindex`); keys go to the focused element's view, or move focus
      from the root when nothing is focused. Lightweight focus and its ring
      work as on the other platforms, and Escape goes back.
    - **Accessibility:** elements carry ARIA roles and states (button,
      checkbox, radio, tab, listbox and option, combobox with
      `aria-expanded`, heading, image; checked, selected, disabled, hidden),
      updated as views' states change. A layer's accessible lightweight
      views get invisible elements over them, placed among the native
      elements in the views' order (only those elements move, so native
      scroll positions and focus aren't disturbed); a click on one, as a
      screen reader sends, performs the view's default action.
    - **History:** a history entry stands for the app's back stack, so the
      browser's Back goes back in the app; it's taken away when the app goes
      back to its first page on its own.
    - **Lists:** virtualized by lines (an item, or a grid's row or column):
      only those near the viewport have containers, recycled as it scrolls,
      with unmeasured lines estimated and a sizer giving the scroll extent.
      Pull-to-refresh follows touches (which go on while the list scrolls)
      and mouse drags from the top, with a spinning indicator while the list
      refreshes.
    - **Images** start loading when first drawn, and their layer draws again
      once they have, so apps needn't wait for bitmaps (the browser has one
      thread to wait on).
    - **Files:** .NET's file system in the browser is in memory, and the app
      data folders are empty paths there. The files the app writes (outside
      the runtime's folders) are restored from local storage before the app
      starts and saved there after it suspends.
    - **Animation** stays as phase 17 left it: layers composed into CSS
      transforms on animation frames, rather than the Web Animations API.
    - **SoonerOrLater:** `SoonerOrLater.XForms.Web`. Its model uses an
      `AppFileStore` (isolated storage, or a folder in the browser, which
      has no isolated storage), and it loads bitmaps without waiting in the
      browser.
    - **Verified** in the browser: Tab focus with the ring, Space toggling a
      focused toggle; the accessibility tree's roles, names and order;
      navigating and the browser's Back; list virtualization in list, row
      and grid layouts; pull-to-refresh (with synthetic pointer events).
      SoonerOrLater: adding a task, swiping it done, the menu, Settings
      (dark theme, accent color), and its tasks and settings kept across
      reloads.
    - **Not verified:** a screen reader; touch pulls on a real touch screen.

### Layering

Phases 1-18 aligned the API and filled the platforms' gaps. These phases
make the layers themselves clean, so that each one holds only what belongs
to it, and later layers (themes, markup, compatibility) have a clear place
to go:

| Assembly | Layer | Builds on |
|---|---|---|
| `XForms.Framework` (+ `.Win32`, `.Android`, `.iOS`, `.Web`) | The platform contract: elements, layout, drawing, raw input, and the views the platforms render | — |
| `XForms.Toolkit` (+ `.Win32`, `.Android`, `.iOS`, `.Web`) | Cross-platform services for any .NET app, with nothing about a UI (phases 22, 23 and 25) | — |
| `XForms` (was XForms.Controls; multi-targeted) | Functional controls (what they do, not how they look) and their base classes; panels, gestures and binding; the app's entry point (`XFormsApp.Run`, phase 25) | Framework, Toolkit |
| `XForms.Themes.<Name>` | How the controls look: `Basic` first | Controls |
| `XForms.Xaml` | Markup | Framework, Controls |
| `XForms.Compat.WinUI` | WinUI's API over XForms' controls | Controls, Xaml |

Two findings shaped the order:

- **The framework holds control behavior.** Tap recognition (`View`'s
  `Tapped`, `DoubleTapped` and `Holding`, and `TapGestureRecognizer`) is
  gesture recognition, like pan and pinch, yet it's in the framework because
  the framework leans on it: keyboard and screen reader activation fake a
  pointer press (`OnActivate`), focusability and "actionable" are inferred
  from tap handlers, and `ListViewItem` selects on a tap. The framework's
  primitives should be raw pointer input and an extension point for
  gestures.
- **The platforms aren't held to a contract.** The framework gives all four
  platforms its internals (`InternalsVisibleTo`), and they use about 35
  internal members of `View` and `Application`: input dispatch, lightweight
  views, focus, accessibility and lifecycle. `IApplicationDelegate` is a
  partial gate, but nothing keeps the platforms to it. HotChai.Game solved
  the same problem with `GameHost` and an analyzer; phase 19 does the same
  here.

Each phase is committed when it's built for every platform and verified on
Win32, Android and the browser, as before.

19. **Platform contract.** The platforms' side of the framework, made
    explicit and enforced. No change to apps' API.
    - **A contract namespace, `XForms.Platform`:** `IPlatform`, the renderer
      interfaces (`IViewRenderer`, `IListViewRenderer` and the rest) and the
      hosts below. The renderer interfaces become internal, visible only to
      the platforms, since apps never use them.
    - **`ApplicationHost`:** what a platform calls for the app as a whole:
      start, suspend and resume, closing, Back, the system theme, safe area
      insets, and windows. It replaces `IApplicationDelegate`, and calls
      such as `OnBackRequested`, `OnResume` and `OnClosed`.
    - **`LayerHost`:** one for each layer (a renderer with lightweight views
      drawn into it). Most of what the platforms reach into `View` for is
      work on a layer's lightweight views, and this exposes just that:
      painting them (with the focus ring), hit testing, routing pointer, key
      and wheel input, hover, moving focus, and the accessible elements with
      their states and actions. The framework can then change how
      lightweight views work without changing a platform.
    - **Activation, focus and accessibility as declared facts:**
      - Activation becomes its own primitive. A platform asks a layer to
        activate a view (Enter or Space, a screen reader's activate command,
        UI Automation's Invoke), and the view handles it in `OnActivate`,
        which no longer fakes a pointer press.
      - `Focusable` and the accessibility actions a view supports are set by
        the view, not inferred from tap handlers.
    - **An analyzer** (from HotChai.Game's HCG0002): in a platform assembly,
      a framework internal outside `XForms.Platform`, and not marked
      `[PlatformContract]`, is a build error.
    - **Decide:** whether the renderer interfaces should stay public, so a
      platform could live outside this repository. Making them internal is
      simpler and can be undone.

    **Status: done.**
    - **The analyzer** (`analyzers/XForms.Analyzers`, rule XF0001) runs on
      every `XForms.Framework.*` project. On the first build it found 50
      framework internals the platforms used; none are left.
    - **`XForms.Platform`** holds `IPlatform`, every renderer interface (and
      the two callbacks the platforms make into views, `ITextBoxDelegate` and
      `IPopupDelegate`), `PlatformContractAttribute` and the hosts, in
      `framework/Platform`. The interfaces stay public: apps pass `IPlatform`
      to their `Application`, which can't take an internal type. The hosts
      are internal, since only the platforms use them.
    - **`ApplicationHost`** replaces `IApplicationDelegate`: suspend and
      resume, the system theme, the safe area, Back, hover and closing
      windows. A platform gets it with `ApplicationHost.For(application)` or
      `ApplicationHost.Current`.
    - **`LayerHost`** (a struct over one view, from `LayerHost.For(view)`):
      lightweight views (painting, hit testing, the focus ring), pointer,
      wheel and key routing, focus, owner drawing, root sizes and
      accessibility. It absorbed the internal `IViewDelegate`. Its pointer
      and wheel dispatch go to the lightweight view under the pointer, as
      the delegate's did, and fall through to the view itself on Win32.
    - **The framework picks lightweight renderers:** `IPlatform` says
      whether it uses them (`UsesLightweightRenderers`: true on Android,
      iOS and the web; false on Win32, which draws lightweight views in its
      own composition tree), and `TextBlock`, `Panel`, `ContentControl` and
      `UserControl` create them. The platforms no longer each repeat the
      check.
    - **The element-to-renderer links** (`Renderer` and `CreatedRenderer`)
      and `ListView.NotifyRefreshRequested` are marked `[PlatformContract]`.
    - **Activation:** `View.Activated` is raised by the default
      `OnActivate`, which no longer fakes a pointer press. Enter and Space,
      a screen reader's activate command and UI Automation's patterns all
      reach it. A view is focusable by default when something handles
      `Activated`: `TapGestureRecognizer` and `SwipeGestureRecognizer`
      handle it once attached to a view (recognizers now know their view),
      and so does `View.Tapped`, as a tap at the view's center. Controls
      that override `OnActivate` (the pickers, the calendar) are unchanged.
    - **Verified:**
      - Win32: clicks, Tab and Space, UI Automation's select, toggle and
        list selection, Escape going back, and closing a second window.
      - Android: taps, Tab and Space on a lightweight radio button (with its
        focus ring), and the accessibility tree. In SoonerOrLater: opening a
        task's commands with a tap, swiping a task done, and Undo.
      - The browser: clicks, Tab and Space.
    - **Fixed on the way:** Win32's focus manager called `OnActivate`
      directly; it's `protected internal`, which the analyzer treats as
      visible, so it goes through `LayerHost.Activate` now.

20. **Slim the framework.** With the contract explicit, move out what isn't
    needed to serve it.
    - **One container primitive.** Today the platforms render five kinds
      of container that differ only in API shape:

      | Type | Adds | Platform renderer |
      |---|---|---|
      | `Control` | a background, its own animator | `IControlRenderer` |
      | `ContentControl` | one child, padding | `IContentControlRenderer` |
      | `UserControl` | one child, padding, and drawing | `IUserControlRenderer` |
      | `Page` | a layout panel, a background | `IPageRenderer` |
      | `Panel` | any number of children | `IPanelRenderer` |

      They're all a view that has children and may draw, so `View` becomes
      that primitive, as `FrameworkElement` is in WPF and `Visual` in
      Avalonia:
      - `View` gets protected visual children, opt-in drawing (`Render`,
        `InvalidateVisual` and drawn animations, from `UserControl`), a
        background, and `CreateOwnAnimator`.
      - The platforms implement one container renderer instead of five.
        (The web platform's panel, content and page renderers are already
        thin wrappers over one.)
      - `Control`, `ContentControl`, `UserControl` and `Border` move to the
        toolkit, and so does `Panel` as the public base class for panels,
        built on `View`'s children. `ContentControl` and `UserControl`
        share their content and padding code, which is duplicated today.
      - `Page` stays, since navigation, windows and the safe area are built
        on it, but it's a plain container with no renderer of its own.
        `Frame` stays a native view, since its renderer runs page
        transitions.
    - **Namespaces:** `XForms.Controls` in the framework becomes
      `XForms.Primitives`, holding only the views the platforms implement
      natively: `TextBlock`, `TextBox`, `Image`, `ScrollViewer`, the items
      view below and `Popup`. That frees `XForms.Controls` for the controls
      assembly.
    - **Gestures** move to the toolkit (renamed in phase 21):
      - Tap, double tap and hold become recognizers, replacing `View`'s
        `Tapped`, `DoubleTapped` and `Holding` and `TapGestureRecognizer`.
        Controls get a pressed state from them, alongside the framework's
        `IsPointerOver`.
      - `PanGestureRecognizer`, `PinchGestureRecognizer` and
        `SwipeGestureRecognizer` move as they are.
      - The framework keeps the `GestureRecognizer` base and the pointer
        pipeline (pressed, moved, released, canceled, entered, exited, wheel
        and capture).
    - **Lists split in two:**
      - The framework keeps a virtualizing items view: realizing and
        recycling containers, layouts (a line, a row, a grid), scrolling and
        pull-to-refresh. WinUI's `ItemsRepeater` is the model; settle the
        name here.
      - `ListView` in the toolkit adds selection and taps.
      - `ListViewItem` no longer derives from `DockPanel`.
    - **Panels** move to the toolkit with their base class: `Grid`,
      `StackPanel`, `DockPanel`, `WrapPanel`, `UniformGrid` and `Canvas`.
    - The toolkit's controls, the test apps and SoonerOrLater move to the
      new gestures and pressed state.

    **Status: done.** The framework is now `framework/Core`,
    `framework/Platform` and `framework/Primitives`.
    - **Containers:** `View` holds children (`AddVisualChild` and the rest,
      in z-order), draws itself when it sets `DrawsContent` (`Render`,
      `InvalidateVisual`, `CreateDrawnAnimator`), has a background
      (`GetBackgroundColor`, `InvalidateBackground`) and `CreateOwnAnimator`,
      and by default lays its children out over its whole area. The
      platforms implement one `IContainerRenderer` (and the framework one
      lightweight container) instead of five renderer kinds. `Page` is a
      container; its `Content` is any view.
    - **Moved to the toolkit:** `Control`, `ContentControl`, `UserControl`,
      `Border`, `Panel`, the six panels, `ViewCollection` and
      `LayoutTransition`, built only on public and protected API. Two
      members became public for them: `Animator.TranslateByOffset` (layout
      transitions) and `View.ContentPadding` (protected). `Orientation`
      moved to the `XForms` namespace, since lists use it too.
    - **`XForms.Primitives`:** `TextBlock`, `TextBox`, `Image`,
      `ScrollViewer`, `Popup`, and the items view. `TextBox` derives from
      `View` and sends its own background.
    - **Lists:** the items view is `ItemsView`, with `ItemContainer` and
      `ItemsViewSource` (was `ListViewSource`), as WinUI names its
      `ItemsView`; the platforms render `IItemsViewRenderer`. The toolkit's
      `ListView` adds selection, `ItemClick` and `SelectionChanged` (it's
      told when a container is bound, through `OnContainerBound`), with
      `ListView<T>` and `ListViewItem` (row background, selection, taps).
    - **Gestures:** `View` keeps raw pointer input, activation and a
      `GestureRecognizers` collection (as MAUI's), whose recognizers learn
      their view. Its `Tapped`, `DoubleTapped` and `Holding` events and tap
      tracking are gone: `TapGestureRecognizer`, in the toolkit with the
      pan, pinch and swipe recognizers, now raises those events, reports
      `IsPressed`, and takes an optional delegate.
    - **Verified:**
      - Win32: the panels, drawing and pickers pages; list selection;
        taps, double taps, holds and pans; Tab and Space; UI Automation's
        select, toggle and Invoke.
      - Android: the panels page; SoonerOrLater's task commands, swipe and
        Undo.
      - The browser: the pickers page; clicks, Tab and Space.
    - **Fixed on the way:** `CommandListViewItemContainer` added its swipe
      recognizer beside `ListViewItem`'s tap recognizer once views had a
      collection, so a tap was handled twice; it replaces it, as it did
      when a view had one recognizer.
    - **Not done:** pressed feedback still comes from each control's
      delegate (`OnTapBegan`, `OnTapEnded`); themes will draw from
      `IsPressed`.

21. **Rename the toolkit to `XForms.Controls`.** The functional controls,
    which say what a control does and leave how it looks to a theme.
    - `toolkit/XForms.Toolkit` becomes `controls/XForms.Controls`, with the
      namespace `XForms.Controls` (and `XForms.Layouts` and `XForms.Input`
      for the panels and gestures it now holds).
    - Update both solutions, the test apps, SoonerOrLater and the docs
      (`architecture.md`'s layout and rules, this document's name mapping).

    **Status: done.**
    - `controls/XForms.Controls` (assembly and root namespace
      `XForms.Controls`). The former `XForms.Toolkit` types are in
      `XForms.Controls`, with `Control`, `ContentControl`, `UserControl`,
      `Border` and now `Panel`, `ViewCollection` and `LayoutTransition`;
      the panels stay in `XForms.Layouts` and the gesture recognizers in
      `XForms.Input`.
    - Both solutions, the test apps, the XAML spike and SoonerOrLater
      reference it; `architecture.md` and the README describe the layers
      as they are now.
    - **Verified:** every project builds (the four platforms, the controls,
      the four test apps and SoonerOrLater on Win32, Android and the web);
      the rename changes no behavior that phases 19 and 20 verified.

22. **Trim the framework.** The framework keeps only what a different set
    of controls would also need; what's specific to ours, or isn't UI at
    all, moves out.

    **Status: done.**
    - **`XForms.Toolkit`** (new, `toolkit/`), what apps use besides
      controls, for any control set:
      - **Services** (clipboard, launcher, preferences, secure storage,
        sharing, connectivity, file pickers, haptics). The platforms still
        implement them, and now register them as they start
        (`PlatformServices.Current`); `IPlatform.CreateServices` is gone.
        `Application.Services` remains, as an extension property, so apps
        are unchanged.
      - **Data binding** (`Binding`, `Bind`, `ViewProperty`). Bindings stay
        alive with their views through a `ConditionalWeakTable` instead of a
        hook on `View`. The framework views' descriptors
        (`TextBlock.TextProperty` and the rest) are C# 14 static extension
        properties here, so apps' code is unchanged.
    - **`Theme`** keeps the mode and `ColorScheme`. Every color in the scheme
      is one the platforms use (they fill it from the system's high-contrast
      colors), so it's the system's vocabulary rather than our controls'.
      Everything else is a theme **extension** a control library adds,
      after Flutter's `ThemeExtension`: `GetExtension` and `WithExtension`
      replace `GetStyle` and `WithStyle`. XForms.Controls' `Typography`,
      `TextStyle`, `Shapes` and `Motion` are extensions, read through
      extension properties (`theme.Typography`), with defaults when a theme
      has none. `Frame` times its own transitions (`TransitionDuration`).
    - **Modal pages:** `ModalPage<TResult>` and the generic
      `ShowModalAsync<TResult>` are gone. `ShowModalAsync(Page)` completes
      when the page closes (`Page.CloseModalAsync`); a page keeps its own
      result.
    - **Light dismiss:** `ILightDismissTarget` and
      `Application.LightDismissTarget` are gone. XForms.Controls'
      `LightDismissControl` handles the general
      `Application.PreviewPointerInput` while it's open.
    - **Verified:**
      - Win32: a modal page's result, the network service, two-way binding
        (and a bound command's enabled state), and the pickers and panels
        pages pixel for pixel against phase 20 (apart from the spinning
        progress ring).
      - Android: SoonerOrLater's menu closes on a press on a task (which
        then does nothing else).
      - The browser: the network service and a modal page's result.
    - **Not done:** a press on a list's blank space (below its last row)
      doesn't reach the framework on Android, so it doesn't close an open
      flyout. That was already so before this phase.
    - **Then, the last control-specific pieces:**
      - **`ItemsView.Padding`** (a `Thickness`, inside the scrolling area)
        replaces `FooterHeight`, which only SoonerOrLater's floating menu
        bar needed.
      - **Colors as properties:** `TextBox` has `BackgroundColor`,
        `ForegroundColor`, `PlaceholderColor` and `AccentColor` (the
        caret), and `ItemsView` a settable `BackgroundColor` and an
        `AccentColor` (the refresh indicator), each defaulting from the
        theme. The platforms' renderers take them and no longer read the
        theme's colors.
      - **Pluggable page transitions:** `PageTransition` is an abstract
        class rather than an enum. The built-in `Slide` (the default),
        `SlideUp`, `Fade` and `None` are instances; a control library or
        app subclasses it and animates the page in `PlayAsync` through a
        `PageTransitionContext` (the page, whether it arrives or leaves,
        the side, the frame's size and `TransitionDuration`).
      - **Verified:** Win32 lists, text boxes, pickers and panels pixel for
        pixel against before (apart from a status label and the spinning
        ring); a padded list with its own background on Win32, Android and
        the browser; slide, fade, back and the modal slide-up with its
        result on Win32 and the browser; the browser's text box colors,
        including the placeholder's. iOS builds only.

23. **Platform layers.** Each layer's platform code lives in its own
    platform assemblies, apps host XForms without deriving from XForms
    types, and the layers are enforced, so they stay clean.

    **Status: done.** As planned below, in four commits:
    - **Hosting:** `Application()`, and `Run` on every platform, with no
      native base classes; Android moves the running app to a re-created
      activity.
    - **The toolkit's platforms:** `XForms.Toolkit.Win32`, `.Android`,
      `.iOS` and `.Web` (in `toolkit/platform/`), registered with
      `Win32ToolkitServices.Register()` and the like; the framework's
      platforms no longer reference the toolkit.
    - **Internal:** the contract, and every platform type but its hosting
      class.
    - **Enforcement:** XF0002 for references, and the public API analyzer
      with today's API as the shipped baseline. Each platform's public API
      is its hosting class; each toolkit platform's, its registration.
    - **Verified:**
      - Win32: the test pages pixel for pixel against before (apart from
        the spinning progress ring); preferences, secure storage, the
        network and haptics; SoonerOrLater pixel for pixel.
      - Android: re-creating the activity (`Activity.Recreate`, from a test
        app button) twice, keeping the page and Back; Back by the key;
        suspend and resume reaching the app, including across re-creation;
        preferences, secure storage, the network and haptics; the document
        picker starting through the toolkit's `ResultActivity`. (The
        SoonerOrLater captures compared here turned out to show its splash
        screen; phase 24 compared its main page properly, against phases 20
        and 22: pixel for pixel.)
      - The browser: navigation and a modal page; preferences, secure
        storage, the network and haptics through `xforms-toolkit.js`;
        SoonerOrLater.
      - The build: a framework platform referencing the toolkit fails with
        XF0002; a new public type in a platform fails with RS0016.
    - **Not done:**
      - iOS builds only: its new hosting (`Run(args, ...)` with XForms'
        delegates, or `Run(windowScene, ...)`) hasn't run.
      - Not exercised: Back before Android 13 (the emulator runs Android
        16), dark mode and text size changing in place on Android (that
        needs the emulator's system settings), and a document picked on
        Android coming back (that needs the system picker's UI). The file
        dialogs and Copy and Share weren't run on Win32.

    **Where things stood.**
    - **The framework's platforms implement the toolkit.** The services
      (clipboard, launcher, preferences, secure storage, sharing, haptics,
      connectivity, file pickers: about 1,850 lines across
      `Core/Services.cs`, `NetworkAndFiles.cs` and Win32's
      `FileDialogs.cs`) live in `XForms.Framework.<Platform>`, so all four
      reference `XForms.Toolkit` and register its services as they start.
      The layer below depends on the layer above.
    - **What the services need from the platforms is small:**
      - Win32: the main window's handle, as the file dialogs' owner.
      - Android: the activity (as context, for haptics on its view, and to
        start an activity for a result, for the document picker).
      - iOS: the scene's view controller, to present the share sheet and
        the document picker.
      - The browser: about 16 JavaScript functions (clipboard, storage,
        sharing, vibration, online status, file picking) in the framework's
        `xforms.js` and `Js`.
    - **The platform assemblies are open.** On Android and iOS the
      renderers, `DrawContext`, `NativeLayoutView`, `OwnerDrawControl`
      and the platform classes are public (about 30 types), so anything
      referencing a platform assembly can reach into it. Win32 and the
      browser already keep nearly everything internal but their entry
      points.
    - **Apps derive from XForms' native classes.** An Android app's
      activity must derive from `AndroidApplicationActivity`, and an iOS
      app's delegate from `NativeApplicationDelegate`. An app that needs
      another base class (AndroidX's `ComponentActivity`, an SDK's own
      activity or delegate) can't have it, and XForms can't run inside an
      existing app's activity.
    - **The contract is public.** `IPlatform` and the renderer interfaces
      (`ITextBoxRenderer` and the rest, 17 types in `XForms.Platform`) are
      public, because an app's `Application` constructor takes an
      `IPlatform`. No control library, toolkit or app uses them otherwise.

    **Strategies, as in HotChai.Game?** HotChai.Game's platform interfaces
    (`IGraphicsServiceStrategy`, `ITextureStrategy` and so on) are
    internal, its platforms see them through `InternalsVisibleTo`, and its
    analyzer limits the platforms to them. XForms has the same shape since
    phase 19: `IPlatform` and the renderer interfaces are the strategies,
    and XF0001 is the same analyzer. What XForms lacks is that the
    interfaces are internal; this phase makes them so. HotChai.Game's
    `Strategy<T>` wrapper (disposal, and a checked cast to the platform's
    type on a hot path) solves problems XForms doesn't have, since an
    element owns its renderer and releases it on detach, and renaming
    renderers to strategies would be churn. So: the principle, not the
    class. HotChai.Game's hosting is the other model this phase follows:
    `AndroidGameEngine.Run(activity, hostView, game)` takes any activity,
    with no base class to derive from.

    **The model:**

    | Assembly | Holds | References |
    |---|---|---|
    | `XForms.Framework` | Core, primitives, and the platform contract (internal) | — |
    | `XForms.Framework.<Platform>` | Renderers and hosting; public: only its hosting API | Framework |
    | `XForms.Toolkit` | Service interfaces, binding | Framework |
    | `XForms.Toolkit.<Platform>` (new: `.Win32`, `.Android`, `.iOS`, `.Web`) | The services' implementations | Toolkit, Framework.<Platform> |
    | `XForms.Controls` | Controls | Framework, Toolkit |

    - **A platform assembly's public API is its hosting API,** and
      nothing else: one class per platform, with `Run` methods and the
      running host's native objects. Every renderer and helper is
      internal.
    - **Hosting by composition.** No app derives from an XForms type to
      host it. Every platform has `Run`, which shows the app in a whole
      host (a window, the page, an activity, a scene):

      | Platform | XForms creates the host | The app's own host |
      |---|---|---|
      | Win32 | `Win32Application.Run("Title", new App())`: its window and message loop | — (embedding in a caller's window is phase 31) |
      | Browser | `await WebApplication.RunAsync("Title", new App())`: the page | — (embedding in an element is phase 31) |
      | iOS | `IosApplication.Run(args, new App())` from `Main`: XForms' internal app and scene delegates | `IosApplication.Run(windowScene, new App())` from the app's own scene delegate, with any base class |
      | Android | — | `AndroidApplication.Run(this, new App())` from the app's own activity's `OnCreate`, with any base class |

      Android has no form where XForms creates the activity: the system
      starts the launcher activity itself, and a launcher activity in
      XForms' library would land in every app's manifest (two launcher
      icons for an app with its own) and fix its label, theme and intent
      filters. A five-line activity, as HotChai.Game asks for, is where
      Android developers expect that code. The project templates include
      it.
    - **What the native base classes did, done by composition** (every
      override, checked):

      | Today (an override in the base class) | Instead |
      |---|---|
      | Android `OnCreate`: sets the root view | `Run` calls `SetContentView` |
      | Android `OnPause`, `OnResume` (suspend and resume) | `Application.IActivityLifecycleCallbacks`, for the host activity only |
      | Android `OnConfigurationChanged` (dark mode, text size) | The root view's `OnConfigurationChanged` |
      | Android `OnBackPressed` | `OnBackInvokedDispatcher` from Android 13, and a back-key listener on the root view before it (as HotChai.Game) |
      | Android `OnActivityResult` (the document picker) | A small internal activity in the toolkit's Android assembly that starts the picker and reports back (as Xamarin.Essentials' `IntermediateActivity`), so apps forward nothing |
      | Android status and navigation bar colors, insets | Already through `activity.Window`; unchanged |
      | iOS delegate and scene delegate overrides (launch, scenes, activation) | `Run(windowScene, ...)` creates the window; activation from the scene's notifications |

      The base classes' public `Suspending`, `Resuming` and
      `ConfigurationChanged` events go: apps have `Application`'s
      `OnSuspend` and `OnResume` already.
    - **The app object, without a factory.** `Run` takes the app itself,
      as HotChai.Game takes the game. `Application()` gets a parameterless
      constructor that builds only what needs no platform (the root frame,
      the title bar); `Run` attaches the platform, then creates the
      application renderer, dispatcher and main window, then calls
      `OnStart`. Apps do their work in `OnStart`, as both apps already do;
      touching a platform-dependent member (`Dispatcher`, `MainWindow`,
      services) in the constructor throws, saying to use `OnStart`.
      `IPlatform` leaves apps entirely.
    - **When Android re-creates the activity** (a configuration change the
      activity doesn't declare, such as rotation for the test app), the
      new activity's `OnCreate` calls `Run` again with a new app object.
      `Run` re-attaches the running app to the new activity and leaves the
      new object unused (harmless: by the rule above, it holds nothing).
      When the activity really finishes, the app stops, so the next launch
      starts a new one. The templates declare the usual configuration
      changes so re-creation is rare.
    - **The running host's native objects,** for apps calling native APIs
      and for libraries adding platform features (the toolkit's platforms
      among them), on each platform's class: `Current`, and its `Activity`
      (Android), `WindowScene` and root view controller (iOS) or
      `MainWindowHandle` (Win32). They're the objects the app handed over,
      or the window it asked for, so nothing internal is exposed.
    - **The contract becomes internal.** `IPlatform`, the renderer and
      delegate interfaces become internal, visible to the four framework
      platform assemblies only (which already have `InternalsVisibleTo`).
    - **Registering the services.** The framework platforms stop setting
      `PlatformServices.Current`. Each toolkit platform has one call, made
      by the app's platform project before `Run`
      (`Win32ToolkitServices.Register()`, and `await
      WebToolkitServices.InitializeAsync()` on the browser, which loads
      the toolkit's own JavaScript module, `xforms-toolkit.js`). The
      services find the window, activity or view controller when they're
      used, through `Current`, so registering needs nothing from a running
      app. An app that uses no services needn't reference the toolkit's
      platform assemblies at all.

    **Keeping it clean:**
    - **Allowed references,** checked by the build: a target in
      `Directory.Build.targets` fails when a project references a layer
      it mustn't (a framework platform referencing the toolkit or the
      controls, the toolkit referencing a platform, the controls
      referencing any platform, a toolkit platform referencing another
      platform's assemblies).
    - **Tracked public API:** Microsoft's public API analyzer
      (`Microsoft.CodeAnalysis.PublicApiAnalyzers`, RS0016 and RS0017) on
      the framework, the framework platforms, the toolkit and the toolkit
      platforms, with each assembly's API in `PublicAPI.Shipped.txt` and
      `PublicAPI.Unshipped.txt`. Making something public then shows up in
      review as a change to those files, which is how the platform
      assemblies stay down to their hosting API.
    - **`InternalsVisibleTo` stays narrow:** the framework's internals go
      to the four framework platforms only (XF0001 still limits them to
      the contract); the toolkit's go to no one.
    - `check.py` gains the reference check, so it runs with every build.

    **Steps,** each built for every platform and verified on Win32,
    Android and the browser:
    1. **Hosting:** `Application()` and the constructor split; `Run` on
       every platform, by composition; the native base classes and
       `AndroidPlatform` and `NativePlatform`'s public constructors go;
       Android re-creation; the test apps and SoonerOrLater updated.
    2. **The toolkit's platforms:** create `XForms.Toolkit.Win32`,
       `.Android`, `.iOS` and `.Web`; move the services and their
       JavaScript into them, with the Android result activity; the
       framework platforms drop their toolkit reference; the apps
       reference the toolkit platforms and register the services.
    3. **Internal:** the contract interfaces and every platform type other
       than the hosting API.
    4. **Enforcement:** the reference check and the public API analyzer,
       with the result as the baseline.
    5. **Docs:** `architecture.md` (the layer diagram gains the toolkit's
       platforms; hosting), this document.

    **Verifying:**
    - Every service on every platform the way phase 22 did (network, file
      open and save, preferences, secure storage; Copy and Share only in
      the browser and on Android, never in the Win32 test app).
    - The test pages pixel for pixel against before on Win32.
    - Android: suspend and resume, back (on the emulator's API level, and
      by the back key), dark mode and text size changing in place, and
      rotating the test app (which re-creates its activity) keeping the
      app's state.
    - SoonerOrLater on Win32, Android and the browser.
    - A deliberate bad reference and a deliberate new public type each
      fail the build.

    **Decided** (2026-10-21):
    1. **Registering services:** an explicit call in the app's platform
       project, not automatic registration when the assembly loads.
    2. **The public API analyzer:** adopted.
    3. **Names:** `Win32Application`, `WebApplication`,
       `AndroidApplication` and `IosApplication`; `XForms.Toolkit.Win32`
       and so on.
    4. **A constructor that touches the platform** throws.

    **Also in this phase,** to soften what composition costs app
    developers:
    - **Hosting rules** in `architecture.md`: what XForms observes on its
      own (lifecycle, configuration, back, insets), what an app must leave
      alone (handling back or intercepting keys before XForms sees them),
      and what it must forward (nothing yet; a later deep-link API would
      take the activity's `OnNewIntent`).
    - **Templates:** the test apps and SoonerOrLater are the pattern
      until there are project templates: the Android activity declares
      the configuration changes XForms handles in place (dark mode, text
      size, orientation and screen size), so re-creation is rare.

24. **Themes.** A control says what it does and what state it's in; a
    theme says how it looks. The controls' current look becomes the first
    theme, `XForms.Themes.Basic`, and an app chooses its theme explicitly.

    **Status: done,** as planned below, with these differences:
    - **The mechanism** (step 1): `ControlTemplate<TControl>` (for any
      view, not only `TemplatedControl`), `TemplateContext`, the
      `ControlTemplates` theme extension (`theme.GetTemplate`,
      `theme.WithTemplate`), and `TemplatedControl`. The framework gained
      one hook, `Element.OnAttaching(theme)`: called as an element is first
      attached, before its native object exists, so a template can create
      the control's own animators. Templates can also give a background
      (`Background`), skip drawing (`Render` is optional), and restyle
      their parts when the mode or accent color changes
      (`OnThemeChanged`).
    - **Two more hosts for templates:** `TemplatedContentControl`, for
      content controls (`MenuBar`, `TabStrip`, `CommandListViewItem`,
      `HyperlinkButton`, `MessageBoxView`), and `ListViewItem`, which is an
      `ItemContainer`; both use the same `TemplateHost`.
    - **The package** (step 2): `themes/basic/XForms.Themes.Basic`, with
      `BasicTheme.Apply(base.CreateTheme(mode, accent))` rather than
      `Create(mode, accent)`, so the framework's theme (with the system's
      high-contrast colors) stays the base. It's in the layer check and
      tracks its public API.
    - **The looks** (step 3), each group verified before its commit:
      - buttons: `Button`, `ToggleButton`, `RadioButton`,
        `ImageToggleButton`, `MenuButton`, `TabStripItem`,
        `FloatingButton`, `CommandBarButton`, `DayOfWeekToggleButton`;
      - list rows: `ListViewItem`, `ListViewSwipeItem` (and its dimming
        during a swipe);
      - fields and pickers: `PickerField` (so `ComboBox` and
        `CalendarDatePicker`), `CalendarView`, the popups' frames (the new
        `PopupFrame`) and `ComboBoxItemsView`;
      - `ProgressRing` and `Flyout`'s frame;
      - the composites: `MenuBar` and `TabStrip`'s backgrounds,
        `CommandListViewItem`'s, `HyperlinkButton`'s feedback, and
        `MessageBox`'s layout (a `MessageBoxView` whose template adds the
        message and buttons as parts).
    - **Geometry stays with input.** Controls whose input depends on their
      geometry (the calendar's cells and arrows, a picker's text and glyph
      areas, a combo box's rows) keep it and expose it read-only, with
      their state; their templates draw everything else. A theme can
      restyle them but not move their cells.
    - **`Border` isn't templated:** it draws only what its own properties
      say, as WPF's and WinUI's `Border` (not a templated control there).
    - **Public now, for themes to template:** `RadioButton`, `MenuButton`,
      `CommandBarButton`, `DayOfWeekToggleButton`, `ListViewSwipeItem`
      (and `ListViewSwipeDirection`), `ComboBoxItemsView`, `PopupFrame`,
      `MessageBoxView` and `CalendarViewPart`. `ButtonStyle` moved to
      XForms.Themes.Basic, and `Button.Style` is gone.
    - **Verified, for each group:** every Win32 test page in light and
      dark pixel for pixel against the baseline (apart from the spinning
      progress ring), and the group's own states against the previous
      commit (built from a stash): a SoonerOrLater walk (a tab mid-press,
      a task's command bar, a task mid-swipe, the new task page and its
      schedule), a selected row, a pressed picker field, the combo box's
      list (and the keyboard in it), the date picker's calendar, the
      Schedule flyout, and the message box, in light and dark. Android
      and the browser: each group's pages, popups and SoonerOrLater
      render, and SoonerOrLater's main page on Android is pixel for pixel
      as in phases 20 and 22.
    - **Not done:** `ComboBoxItem` rows, popups and the calendar are drawn
      by templates, but nothing yet changes them per view other than a
      whole template. Only `Button` has a style. iOS builds only.

    **Where things stand.** Eighteen controls draw themselves in `Render`
    (`Button`, `ToggleButton`, `RadioButton`, `ImageToggleButton`,
    `MenuButton`, `TabStripItem`, `FloatingButton`, `CommandBarButton`,
    `DaysOfWeekPicker`'s day buttons, `ProgressRing`, `ListViewItem`,
    `ListViewSwipeItem`, `PickerField`, `ComboBox`, `CalendarDatePicker`,
    `CalendarView`, `Border`, `Flyout`). Most also animate press feedback
    from their tap delegate (`OnTapBegan` scales the control down), and some
    build child views for their look (a picker's chevron, a calendar's day
    labels). The rest (`CommandBar`, `MessageBox`, `TabStrip`,
    `RadioButtons`, `MenuBar`, `FlyoutButton`, `CommandListView`,
    `ListView<T>`) only compose those. `Theme` holds the mode and colors,
    and XForms.Controls' extensions (typography, shapes, motion, per-control
    styles such as `ButtonStyle`); `Application.CreateTheme` builds it for
    the mode and accent color.

    **The model:**

    | Concept | Where | What it is |
    |---|---|---|
    | `Theme` | framework | The data: the mode and colors, and extensions (XForms.Controls' typography, shapes, motion, styles, and now templates). Immutable; inherited down the view tree and overridable per view, as now |
    | `TemplatedControl` | XForms.Controls | Base for controls with a look: it resolves its template from its theme, hands it its state, and draws through it |
    | `ControlTemplate<TControl>` | XForms.Controls | One control's look: its drawing, the child views (parts) it adds, its layout of them, and its response to state changes |
    | `BasicTheme` | XForms.Themes.Basic | Builds a `Theme` with the Basic tokens, styles and a template for every control |

    WPF and Avalonia call these `ControlTemplate` and (in Avalonia)
    `TemplatedControl`, and a markup layer will want to write templates, so
    the names follow them. Unlike theirs, a template here is code first:
    small enough (draw, add parts, react to state) that phase 29 can
    express one in markup.

    - **Templates in the theme.** The templates are a theme extension of
      XForms.Controls' (phase 22): a map from control type to template,
      read and changed through extension methods (`theme.GetTemplate`,
      `theme.WithTemplate`). The framework knows nothing of them. A control
      without its own entry uses its base type's (a subclass of
      `ToggleButton` looks like one), so per-view overrides
      (`view.Theme = theme.WithTemplate(...)`) work as styles do.
    - **`TemplatedControl`** (a `UserControl`, so it draws and has content):
      - Resolves its template when it's attached (when its theme is known,
        before its renderer exists, so a template can ask for its own
        animation layers), and again when its theme changes.
      - `Render` and, if the template takes them, measure and arrange go to
        the template.
      - Exposes the state a look needs as properties, with one
        `StateChanged`: `IsPressed` (from its tap recognizer), `IsPointerOver`
        and `IsFocused` (from `View`), `IsEnabled`, and per control
        `IsChecked`, `IsSelected` and `IsExpanded`.
      - Gives its template a `TemplateContext` for what's protected on the
        control: adding and removing parts, drawn animators, its own
        animators, and invalidating.
    - **`ControlTemplate<TControl>`:** `Apply(control, context)` and
      `Unapply`, `Render(control, drawingContext)`, optional `Measure` and
      `Arrange`, and `OnStateChanged(control)`. Press, hover and focus
      feedback (today's `OnTapBegan` scaling) moves into it, driven by
      state; the controls' tap delegates keep only behavior (click, toggle,
      select).
    - **Styles are the theme's.** `ButtonStyle` describes the Basic look,
      so it moves to XForms.Themes.Basic. Per-view styling stays the
      theme's: `button.Theme = theme.WithExtension(new ButtonStyle { ... })`.
      That removes `Button.Style` (decision 3).
    - **Choosing a theme.** An app chooses it in `CreateTheme`:
      `protected override Theme CreateTheme(ThemeMode mode, Color accent)
      => BasicTheme.Create(mode, accent);`. `Theme.Create` stays the
      framework's tokens-only theme, with no templates. Switching the mode
      or accent color at runtime keeps working (the template stays, its
      tokens change); switching to another theme package isn't supported
      at runtime (decision 4).
    - **No fallback.** A templated control whose theme has no template for
      it throws when attached, naming the control and saying to choose a
      theme in `CreateTheme` (decision 2).
    - **What stays out:** the native primitives (`TextBlock`, `TextBox`,
      `Image`, `ScrollViewer`, `ItemsView`) keep taking only tokens (their
      colors and fonts), since the platforms draw them; the focus ring stays
      the framework's.

    **Steps,** each built for every platform and verified on Win32, Android
    and the browser:

    1. **The mechanism:** the templates extension; `TemplatedControl`,
       `ControlTemplate<TControl>` and `TemplateContext`; the state
       properties and `StateChanged`; the missing-template error. A
       framework hook if it's needed to resolve the template after the view
       has a parent but before its renderer exists. No control uses it yet.
    2. **The package:** `themes/basic/XForms.Themes.Basic` (referencing
       XForms.Controls), with `BasicTheme.Create`. The test apps and
       SoonerOrLater choose it. Nothing looks different yet.
    3. **The looks, a group at a time,** each control's drawing, parts and
       feedback moving into a Basic template and out of the control:
       - buttons: `Button`, `ToggleButton`, `RadioButton`,
         `ImageToggleButton`, `MenuButton`, `TabStripItem`,
         `FloatingButton`, `CommandBarButton`, the day buttons;
       - list rows: `ListViewItem`, `ListViewSwipeItem`;
       - fields and pickers: `PickerField`, `ComboBox`,
         `CalendarDatePicker`, `CalendarView`;
       - the rest: `ProgressRing`, `Border`, `Flyout`;
       - then the composites, checked for anything look-specific they build
         (a message box's layout, say), which moves into templates too.
    4. **Docs:** `architecture.md` (themes and templates), this document.

    **Verifying that nothing changes:** before step 3, capture every test
    page on Win32 (and the main pages on Android and the browser); after
    each group, capture them again and compare pixel by pixel, in light and
    dark mode. Also check pressed feedback, keyboard focus, and UI
    Automation and Android accessibility for each group's controls.
    SoonerOrLater should look and behave the same.

    **Decided** (2026-10-22):
    1. **Names:** `ControlTemplate<TControl>` and `TemplatedControl`, as
       WPF and Avalonia name them.
    2. **A missing template** throws when the control is attached.
    3. **Per-control style properties** (`Button.Style`) are removed, in
       favor of per-view theme overrides.
    4. **Switching theme packages at runtime** isn't supported for now;
       the mode and accent color still change at runtime.

    **Also:** `XForms.Themes.Basic` joins the build's layer check
    (phase 23): it may reference XForms.Controls, XForms.Toolkit and the
    framework, and tracks its public API like the other libraries.

    **Later, not in this phase:** more themes (Fluent, Material); templates
    in markup (phase 29).

25. **One package for apps, and a standalone toolkit.** An app
    references two things: the `XForms` package and a theme package. It
    starts with one call that takes its theme, and never references,
    starts or registers the framework or the toolkit, which are libraries
    XForms uses. The toolkit becomes a cross-platform services library with
    nothing about a UI in it, usable by any .NET app.

    **Status: done,** as planned below, in four commits (and one that
    completes the second: 0e2d70f has its description but only the move of
    `controls/` to `xforms/`; 41ff83e has the rest):
    - **A standalone toolkit:** services only, in `XForms.Toolkit`, with a
      static entry point each (`Clipboard.Default` and the others) and
      registration taking the host's window, activity or view controller as
      functions. Binding moved into XForms. The toolkit references nothing
      of XForms.
    - **The `XForms` package** (`xforms/`): multi-targeted (`net10.0` for
      the browser and anything neutral, `net10.0-windows`,
      `net10.0-android`, `net10.0-ios`), with `XFormsApp.Run` per platform
      in `Platforms/`. `AppTheme` is a theme package's entry
      (`BasicTheme` derives from it); the framework's
      `Application.ThemeExtender` is the general hook `XFormsApp.Run` uses
      to add it to every theme the app builds, before an app's own
      `CreateTheme` adjusts that.
    - **The apps** (the test apps and SoonerOrLater) reference `XForms` and
      `XForms.Themes.Basic` only (and the test app the XAML spike), and
      start with `XFormsApp.Run(..., new BasicTheme())`.
    - **Packages:** the twelve libraries pack on Windows (iOS included),
      version 0.1.0-preview.1; a sample app referencing only the `XForms`
      and `XForms.Themes.Basic` packages, from a local feed, builds and
      runs on Win32, Android and in the browser.
    - **Enforced:** the layer check allows the toolkit nothing of XForms,
      its platforms only the toolkit, XForms the framework and toolkit
      (and their platforms), and themes XForms; it now looks only at a
      project's own references (not those that come through them). XForms
      tracks its public API: what every platform shares in
      `PublicAPI.Shipped.txt`, each platform's entry points in
      `PublicAPI/<target>/`.
    - **Verified:** every Win32 test page, light and dark, pixel for pixel;
      the SoonerOrLater walk on Win32; preferences, secure storage, the
      network and haptics on Win32, Android and in the browser; the Win32
      file dialog owned by the app's window, and Android's document picker
      started from the app's activity, through the providers; binding (the
      binding page, typed into); Android's activity re-created;
      SoonerOrLater's Android main page against phase 20's; the packaged
      sample on all three; a deliberate XForms reference from the toolkit
      failing the build.
    - **Not done:** iOS builds and packs, but hasn't run. The sample isn't
      in the repository (it needs a local feed).

    **Where things stand.**
    - **Apps reference five or six projects:** the framework, its
      platform, the toolkit's platform, XForms.Controls, the theme (and
      the XAML spike, in the test app).
    - **Apps start through the framework's platforms**
      (`Win32Application.Run`, `AndroidApplication.Run` and the others),
      register the toolkit's services themselves
      (`Win32ToolkitServices.Register()` and the others), and choose their
      theme by overriding `CreateTheme` (`BasicTheme.Apply(...)`).
    - **XForms.Controls is platform-neutral** (`net10.0`), so it can't
      start apps or register services: only a platform's build can.
    - **The toolkit holds binding** (`Binding`, `ViewProperty`, the
      framework views' descriptors) and `Application.Services`, and so
      references the framework. Its platforms find the window, activity or
      view controller through XForms' hosting classes.

    **How services are usually organized,** which this follows: those tied
    to the window system (file pickers need an owner window, the share
    sheet a view controller, the clipboard on Win32 a window) are wired up
    by the UI framework, since only it knows the window; Avalonia puts its
    clipboard and storage provider on its top-level window, and WinUI's
    pickers need the window's handle. Device and OS services
    (preferences, secure storage, connectivity, the launcher, haptics) have
    nothing to do with UI. Either way, the services library stays
    independent: XForms wires it up, and an app may use it, or another.

    **The model:**

    | Package (project) | What it is | Referenced by |
    |---|---|---|
    | **`XForms`** (today's XForms.Controls, multi-targeted) | The controls and binding; on each platform, the app's entry point (`XFormsApp.Run`), which starts the framework's host with the app and its theme, and registers the toolkit's services with the host's window, activity or view controller | **apps** |
    | **`XForms.Themes.Basic`** | The first theme | **apps** |
    | `XForms.Framework` (+ `.Win32`, `.Android`, `.iOS`, `.Web`) | The platform layer, as now | `XForms` |
    | `XForms.Toolkit` (+ `.Win32`, `.Android`, `.iOS`, `.Web`) | Cross-platform services, standalone, with nothing about UI | `XForms`; any other .NET app, directly |

    - **`XForms` builds per platform:** `net10.0-windows` (Win32),
      `net10.0-android`, `net10.0-ios`, and `net10.0` (the browser, whose
      apps and XForms assemblies target plain `net10.0`, and anything
      platform-neutral, such as tests). Each platform's build references
      that platform's framework and toolkit assemblies. Almost all of its
      code (the controls, binding) is shared; only the entry points are
      per platform. Everything builds on Windows: `net10.0-ios` libraries
      (and the iOS test app) already do here, with the iOS workload; a Mac
      is needed only to sign and run an iOS app.
    - **One reference on every platform.** NuGet (or a project reference)
      picks the app's platform's build, so an app references `XForms` and
      a theme, whatever its platform.
    - **Starting an app takes its theme:**

      ```csharp
      // Win32, from Main
      XFormsApp.Run("Sooner or Later", new App(), new BasicTheme());

      // Android, from the app's own activity (any base class)
      XFormsApp.Run(this, new App(), new BasicTheme());

      // iOS, from Main (XForms' delegates), or the app's own scene delegate
      XFormsApp.Run(args, new App(), new BasicTheme());
      XFormsApp.Run(windowScene, new App(), new BasicTheme());

      // The browser, from Main
      await XFormsApp.RunAsync("Sooner or Later", new App(), new BasicTheme());
      ```

      `XFormsApp.Run` starts the framework's host (`Win32Application.Run`
      and the others, which stay public but become what XForms uses, not
      what apps call), registers the toolkit's services with what the host
      knows (its window, activity or view controller), and gives the app
      its theme. Android's re-created activity and iOS's scenes work as in
      phase 23.
    - **Themes are objects.** A theme package's entry is a class (Basic's
      `BasicTheme`) deriving from XForms' `AppTheme`, which adds its tokens, styles and templates to the
      theme the framework builds for the mode and accent color (the
      system's high-contrast colors included). `BasicTheme.Apply` becomes
      its method. An app overrides `Application.CreateTheme` only to adjust
      a brand (its colors, a type scale, a style), and no longer to choose
      the theme package. The framework gets one general hook for this: a
      host's way to add to every theme the app builds.
    - **The toolkit stands alone:**
      - Namespace `XForms.Toolkit` (decided), so its `Clipboard` doesn't
        meet `System.Windows.Forms.Clipboard` in a WinForms app's usings.
      - A static entry point per service, as MAUI Essentials:
        `Clipboard.Default`, `Launcher.Default`, `Preferences.Default`,
        `SecureStorage.Default`, `Share.Default`, `HapticFeedback.Default`,
        `Connectivity.Default`, `FilePicker.Default`; one that isn't
        registered throws, naming the registration. `PlatformServices` and
        `Application.Services` go.
      - Registration takes what the services need of the host, read when a
        service is used: Win32 the owner window (`Func<nint>`), Android the
        activity (`Func<Activity>`), iOS the view controller to present on
        (`Func<UIViewController>`); the browser nothing. XForms passes its
        hosting classes' objects; any other app (WinForms, MAUI, Avalonia)
        passes its own.
      - Binding moves into `XForms` (decided), with its namespace
        unchanged, so app code stays the same.
      - The toolkit references nothing of XForms; its platforms reference
        only the toolkit.
    - **Apps may use the services** (`Clipboard.Default.SetTextAsync(...)`,
      as SoonerOrLater does for backups and settings) without referencing
      or registering anything, or use another library instead.

    **Keeping it clean:** the layer check (XF0002) allows the toolkit no
    references, each toolkit platform only the toolkit, `XForms` the
    framework and toolkit (and on each platform, their platform
    assemblies), and themes `XForms`. `XForms` tracks its public API, like
    the framework and toolkit, since it's what apps use.

    **Steps,** each built for every platform and verified on Win32,
    Android and the browser:
    1. **The standalone toolkit:** binding moves into XForms.Controls;
       namespace `XForms.Toolkit`; the static entry points; registration
       with host providers; no XForms references. The apps register with
       their hosting classes' objects for now.
    2. **The `XForms` package:** XForms.Controls becomes `XForms`
       (project and assembly, in `xforms/`), multi-targeted, with `XFormsApp.Run` per
       platform; the theme base class and the framework's theme hook;
       `BasicTheme` derives from it.
    3. **The apps:** the test apps and SoonerOrLater reference only
       `XForms` and `XForms.Themes.Basic` (and the test app the XAML
       spike), start with `XFormsApp.Run`, and drop their registrations
       and `CreateTheme` overrides.
    4. **Packages:** `dotnet pack` for every library, on Windows; a local
       feed; a small app that references only the `XForms` and
       `XForms.Themes.Basic` packages builds for Win32, Android and the
       browser.
    5. **Enforcement and docs:** the layer check's rules, the public API
       files (`XForms` added), `architecture.md` (the packages and the
       app's view of them), this document.

    **Verifying:** every Win32 test page, light and dark, pixel for pixel;
    a SoonerOrLater walk on Win32; every service on every platform, as
    phase 23 did (the file dialogs' owner on Win32; the document picker
    starting on Android; Copy and Share only in the browser and on
    Android); binding (the test app's binding page); Android's activity
    re-created; the packed app from step 4; and a deliberate XForms
    reference from the toolkit failing the build.

    **Decided** (2026-10-22):
    - The toolkit's namespace is `XForms.Toolkit`; services are reached
      through static entry points; binding goes into `XForms`.
    - Apps reference one `XForms` package (multi-targeted) and a theme
      package, and pass the theme to `XFormsApp.Run`.
    - The theme base class is `AppTheme`.
    - The `XForms` project moves to `xforms/` (from `controls/`).
    - The framework and the toolkit are packages of their own (each with
      its platforms' packages), which `XForms` depends on.

26. **Navigation by path.** Every page an app navigates to has a path,
    such as `/item/42`, registered with a function that makes the page
    from it. The app navigates by path, and the back stack is a list of
    paths, so XForms can save it and restore it, the browser's address
    bar and Back and Forward buttons follow it, and a link can open any
    page. Nothing is found by reflection or made from a `Type`, so it's
    safe for trimming and AOT.

    **Status: done,** in four commits (step 2 came with step 1, since
    taking `NavigateAsync(Page)` away needed the apps' routes), with a
    "Navigation" section in `architecture.md` (step 5):
    - **Routes:** `Routes` and `Route` in the framework,
      `Application.OnConfigureRoutes`, `NavigateAsync(path)` and
      `NavigateAsync<TResult>(path)`, `ShowModalAsync(path)` and
      `ShowModalAsync<TResult>(path)`, `OpenWindowAsync(path)`,
      `NavigationStack`, and a page's `NavigationPath` and `SetResult`.
      The `Page` forms of `NavigateAsync`, `ShowModalAsync` and
      `OpenWindowAsync` are gone; `Frame.NavigateAsync(Page)` stays, for
      frames inside a page. A route's function may return null (the item
      is gone), which counts as no match.
    - **Changed from the plan:** a page's path is `NavigationPath`, not
      `Path`, which would hide `System.IO.Path` in every page's code.
    - **Tests:** a unit test project, `test/XForms.Tests` (xUnit), with 29
      tests of matching: fixed text and parameters, case, trailing
      slashes, decoding, which route wins, query values, typed values,
      routes that clash and bad patterns.
    - **The apps:** the test app's pages have routes, and its start page
      (`XFORMS_TEST_PAGE`, the `page` extra, `?page=`) is a path;
      SoonerOrLater's task page is made by its route, `/item/{id}` or
      `/item/new?category={category}`, instead of being kept and reused
      with a view model set on it.
    - **The start and the browser (step 3):** XForms shows the first
      pages after `OnStart` (and `OnLoadResources`): the stack the platform
      gives, or the app's `StartPath` (`/` unless set); apps no longer
      navigate to their first page. `OnRouteNotFound` chooses what a path
      from outside that matches nothing shows (`/` unless overridden). In
      the browser each page in the stack is a history entry whose state is
      the stack's paths and whose address is the top page's path after a
      `#` (an open popup is one more entry); Back and Forward bring the app
      to an entry's stack, making pages again by their routes; the app's
      own going back moves back through the entries, so Forward returns; a
      reload restores the whole stack, and a typed address shows its page
      over the current one. The address wins over the test app's `?page=`.
    - **Restoring and links (step 4):** on Android, `Run` takes the
      activity's `savedInstanceState` (`XFormsApp.Run(this,
      savedInstanceState, new App(), theme)`; a library can't read it
      later), the stack is saved with the activity's state, and a link the
      activity is started with (its intent's data) opens the page for its
      path and query; a new activity for a link while the app runs opens it
      over the current page. On iOS, `Run` takes the scene's connection
      options; XForms' scene delegate saves the stack as the session's
      state-restoration activity and opens URLs while the app runs, and an
      app's own scene delegate calls `IosApplication.Current`'s
      `CreateStateRestorationActivity` and `OpenUrl`. Win32 never restores.
      A modal page comes back as an ordinary page.
    - **Verified:** every Win32 test page, light and dark, pixel for pixel
      (apart from the app page's two new buttons); the SoonerOrLater walk
      on Win32, pixel for pixel; forward, the stack, Back, and results
      from a modal page and a navigated page (chosen, and by Back) on
      Win32, Android and in the browser (headless Edge, since a hidden
      pane doesn't run animation frames); a window opened by path on
      Win32; SoonerOrLater's new and edited tasks on Win32, Android and in
      the browser; the trimming analyzers report nothing new. Step 3, in
      headless Edge: the address and entries after each navigation, Back
      and Forward through three pages, the app's Back and then Forward, a
      reload restoring four pages, a typed address, an unknown one, a fresh
      load at `#/app/7` and at `#/nope`, and a popup closed by Back; Win32
      pages and the SoonerOrLater walk unchanged by the new start. Step 4,
      on Android: the test app three pages deep, sent to the background by
      starting SoonerOrLater, its process ended (`am kill`), and resumed
      with its task's intent, back on Page 3 with the same stack and Back
      going to Page 2; started with a link (`-d xformstest:///app/5`), on
      that page. iOS builds, but restoring and URLs there haven't run.
    - **Found, not changed:** in the browser, Escape goes back only while
      a view has keyboard focus (as it did before this phase).

    **How the others do it.** Flutter's go_router, Jetpack Compose
    Navigation and SwiftUI register each route with a function (or
    closure) that makes the page, in ordinary code the compiler and
    trimmer see; SwiftUI's stack is saved through code the compiler
    generates. MAUI Shell (`Routing.RegisterRoute("item",
    typeof(ItemPage))` and `[QueryProperty]`), Blazor (`[Route]`
    attributes found by scanning assemblies) and WinUI
    (`Frame.Navigate(typeof(Page))`) make pages from types, which is where
    trimming and AOT break. XForms takes the first way.

    **The plan:**
    - **Routes, in the app's code.** An app lists its pages by path, each
      with a function that makes the page:

      ```csharp
      protected override void OnConfigureRoutes(Routes routes)
      {
          routes.Add("/", _ => new MainPage());
          routes.Add("/about", _ => new AboutPage());
          routes.Add("/item/{id}", route => new TaskItemPage(route["id"]));
          routes.Add("/item/new?category={category}", route => new TaskItemPage(category: route["category"]));
      }
      ```

      A path has fixed segments, `{name}` segments and query values;
      `route["id"]` is the text, and helpers parse it without reflection
      (`route.GetInt32("id")`). A page keeps an ordinary constructor with
      arguments; the route's function passes them.
    - **Navigating:** `await NavigateAsync("/item/42")`, `GoBackAsync()`,
      and modal pages by path (`ShowModalAsync("/pick-color")`). A page is
      made each time it's navigated to, by its route; apps no longer make
      or reuse page objects for navigation. `NavigateAsync(Page)` goes, so
      there's one way.
    - **Results:** `Color color = await NavigateAsync<Color>("/pick-color")`
      (and the same for a modal page): generic only over the result, which
      types the `Task` and makes nothing. The page sets its result before
      it goes back (`this.SetResult(color)`); going back without one gives
      the type's default.
    - **The back stack is paths,** with the page objects held while
      they're in it, as now. Saved as text, it says everything needed to
      make the pages again.
    - **The start:** the app's first page is a path, `/` unless the
      platform gives another: the browser's address, a link that opened
      the app, or a restored stack. The test app's start page (now
      `XFORMS_TEST_PAGE`, `?page=`, and an Android extra) becomes a path,
      such as `/lists`.
    - **The browser:** the address follows the current page
      (`https://host/#/item/42`), each navigation is an entry in the
      browser's history, Back and Forward move through the stack, a
      reload comes back on the same page, and an address typed or
      bookmarked opens its page.
    - **Restoring:** the stack is saved when the app suspends, and made
      again when the platform starts the app to restore it: on Android
      when the activity is created again after its process was ended (its
      saved state), and on iOS through scene state restoration. Each page
      in the stack is made from its path; only the top one is shown.
    - **Links:** a path the platform gives when it starts or resumes the
      app opens that page: the browser's address, an Android intent's
      data, an iOS URL. Registering a scheme or a domain with the platform
      stays the app's own (manifest, entitlements).
    - **A path that doesn't match** throws from `NavigateAsync`, since
      it's the app's own mistake. From outside (an address, a link, a
      restored stack from an older version), the app starts at `/`
      instead, and `Application.OnRouteNotFound` lets it choose
      otherwise.
    - **Checked for trimming and AOT:** navigation uses no reflection; the
      trimming and AOT analyzers, already on for every library, stay
      clean, and the published web app (trimmed) navigates every route.
    - **Not routes:** what a page shows inside itself, such as
      SoonerOrLater's tabs in a `Frame` inside its main page, and popups
      and flyouts (decided below).

    **Steps,** each built for every platform and verified on Win32,
    Android and the browser:
    1. **Routes:** `Routes`, matching paths (with tests of the matching,
       in a new unit test project), `NavigateAsync(path)` and its result
       form, modal pages by path, the stack of paths; `NavigateAsync(Page)`
       goes.
    2. **The apps:** the test app's pages and SoonerOrLater's get routes
       (SoonerOrLater's item page takes the task's ID and looks it up,
       instead of being reused with a view model set on it); the test
       app's start page becomes a path on every platform.
    3. **The browser:** the address, history, Back and Forward, reload,
       and opening an address.
    4. **Restoring and links:** saving the stack, and making it again on
       Android and iOS; a path from an Android intent and an iOS URL.
    5. **Docs:** a "Navigation" section in `architecture.md` (routes,
       paths and their syntax, results, restoring, links, the browser),
       and this document.

    **Verifying:**
    - Every Win32 test page, light and dark, pixel for pixel against the
      captures before it (navigation looks the same); a SoonerOrLater walk
      on Win32, Android and in the browser, adding and editing tasks
      through the item page.
    - A result: a test page that returns a value, by `SetResult` and by
      going back without one, normal and modal.
    - The browser: the address after each navigation, Back and Forward
      through three pages, a reload on `/item/{id}` coming back on that
      task, a typed address opening its page, and an unknown address
      starting at `/`.
    - Android: the app two pages deep, its process ended in the
      background, and opened again, on the same page with the same back
      stack; an intent with a path opening its page. (How to end the
      process without touching the system's UI is worked out in step 4.)
    - The trimming and AOT analyzers report nothing new, and the
      published web app navigates every route.

    **Decided** (2026-10-22):
    - Routes are registered in an override on `Application`,
      `OnConfigureRoutes(Routes routes)`, with the app's other startup
      code.
    - Frames inside a page (such as SoonerOrLater's tabs) stay what they
      are: a view showing one page of the page's own choosing, outside the
      back stack and not restored. Nested routes (`/main/soon`, as
      go_router's shell routes) can come later without changing apps'
      other routes.
    - The browser's address carries the path after a `#`
      (`https://host/#/item/42`), which any static host serves.
    - Win32 never restores the stack: a desktop app starts fresh.

27. **Hot reload.** An app running under the debugger (or `dotnet watch`)
    shows an edit to its C# without restarting: a page's layout, a
    handler, a theme's colors, a template's drawing. It keeps its place
    (the page it's on, the back stack, what the page holds) and what the
    user did (text typed, where a list is scrolled, what's selected and
    focused). Release builds are unchanged. The aim is the experience
    Flutter sets: edit, save, see it, nothing lost.

    **Status: done,** with a "Hot reload" section in `architecture.md`
    (step 5: writing a page and a control, what runs again, what carries
    over, each platform's tools, what needs a restart).

    Step 4, user state: `View.Name`, and `GetUserState` and
    `SetUserState`, which a control overrides to say what has changed in
    it since it was first attached (mostly through input). A page or
    `UserControl` building again records its old views' states and focus
    (`SaveUserState`, before they're removed, since removing a focused
    view takes its focus in the browser) and gives them to the new views
    once attached (`RestoreUserState`): named views by name, wherever they
    moved; unnamed ones as Flutter matches them, from the start while
    their types agree, then from the end, then the rest by type, so a view
    added or removed doesn't shift the others. Focus moves a frame later.
    States: a text box's text and selection, a scroll viewer's offset and
    a list's first item in view (a new `IItemsViewRenderer.
    FirstVisibleIndex` and `ScrollToIndex` on every platform; the browser's
    list scrolls once it's laid out), a list's selection, the toggles'
    `IsChecked` (toggle and radio buttons, tab strip items, image toggles,
    days of the week), the combo box's index, the date picker's date and
    the radio buttons' choice. Nine unit tests of the matching. Checked
    with the test app's simulated hot reload (F9, every so many seconds
    with `XFORMS_SIMULATE_EVERY`, an Android intent extra, or a function
    the browser's script calls): on Win32 a typed password and its focus
    carried over a real edit under `dotnet watch` that added a view above
    it; on Android a list's place and selection; in the browser a typed
    box with its focus, and a list's place and selection. Not checked: a
    list's place and selection on Win32 (input can't be sent there while
    another window holds the foreground, and its list's items aren't in UI
    Automation's tree, an accessibility gap of its own), and text on
    Android (the keyboard's stylus introduction covers the app). Fixed on
    the way: Win32 hid none of a removed view's native windows (a rebuilt
    page's old text boxes stayed on screen); `IsShownOnScreen` now
    requires the view to be in a window. Found, not fixed: under `dotnet
    watch` on Win32, RichEdit text boxes crash the app now and then (an
    access violation inside RichEdit, as the known issue in
    `architecture.md`); running the app directly they don't.

    Step 3, rebuilding: the framework's `MetadataUpdateHandler` posts to the
    UI thread, where the app builds its theme again, and the views showing
    run `View.OnHotReload()` (protected, for control libraries), parents
    first, then the children they kept: a page builds again, a
    `UserControl` builds again, a templated control's template is removed
    and applied again (its context hands back the animators it made, which
    can't be made again once the native object exists), a list makes its
    item views again; pages in a back stack build again when shown; then
    `Application.HotReloaded`. In the browser, XForms provides Blazor's
    two hooks (`WebHotReload`), applying each delta with
    `MetadataUpdater.ApplyUpdate` and calling the handlers; only when
    `MetadataUpdater.IsSupported`, so a published app trims it away.
    Verified on Win32 under `dotnet watch`, three pages deep: a view added
    to a page's `Build()` (shown on the same page, the stack kept, and on
    the page below when going back), a template's code (the button's
    background) and a theme token (`BasicTheme`'s corner radius), and
    their reversal; in the browser under `dotnet watch`, the added view,
    without the page reloading; on Android, the rebuild through the test
    app's "Simulate hot reload" (every handler called, as the agent does),
    with the page rebuilt and working, and the page below rebuilt when
    going back. Win32 pages and the SoonerOrLater walk are unchanged.
    Fixed on the way: Win32's focus ring drew against a focused view that
    had been removed (a rebuilt page's old button), and crashed; a view
    now knows whether it's still in a window (`IsInVisualTree`). The web
    libraries' endpoint manifests are kept out of apps too (the
    development server read a library's, and served no `.pdb` or `.dat`,
    so a Debug app didn't start).

    Step 2, `Build()`: `Page.Build()` and `UserControl.Build()` (with
    `IsBuilt`), called once before the native object exists; `Page.Content`
    is set from it (its setter is now protected). Every test app page and
    every SoonerOrLater page builds in `Build()`: constructors keep state
    (a page's depth, a view model) and subscriptions to objects outside the
    page, which a rebuild mustn't repeat; SoonerOrLater's task list pages
    add their parts to what their base's `Build()` returns, and its task
    page applies a task given before it's built at the end of `Build()`.
    Every Win32 test page, light and dark, the navigation walk and the
    SoonerOrLater walk are pixel for pixel the same; both apps work on
    Android and in the browser. Changed from the plan: templates keep
    `Apply` and `Unapply`, which already build their parts and run again
    when a template is applied again, so they need no build method of
    their own; and `UserControl.Content` stays settable by the control,
    for content given to it (a border's child, the XAML spike's presenter)
    rather than built. XForms' own composite controls (the command bar,
    the menu bar) still build in their constructors: they're library code,
    not edited while an app runs.

    Step 1, how updates reach a running XForms app, with a handler in the
    test app that only logs (`HotReloadLog`):
    - **Win32:** `dotnet watch` applies an edit in about two seconds after
      the build, and the handler runs with the changed types (Visual
      Studio's Hot Reload uses the same mechanism).
    - **Android:** `dotnet watch` (SDK 10.0.402, Android 36.1) builds the
      app without the hot reload agent and launches it with
      `dotnet run --no-build`, so the agent never connects: an SDK
      limitation. Visual Studio's Hot Reload is the way there; it can't be
      driven here, so steps 3 and 4 check Android by calling the rebuild
      directly.
    - **The browser:** `dotnet watch` serves the app (`dotnet run`) and
      sends updates to the page through its browser script, which applies
      them only through Blazor's hooks (`Blazor._internal.
      applyHotReloadDeltas` and `getApplyUpdateCapabilities`); without
      them it reloads the page. XForms' web platform provides those hooks
      (step 3), applying the deltas with `MetadataUpdater` and calling the
      handlers, only when hot reload is on.
    - **Fixed on the way:** the web libraries no longer copy their
      development manifest into apps (an app's development server read a
      library's and served nothing, so `dotnet run` had never worked for
      the test app), and the test app's web project creates the folder its
      linked images appear in, which `dotnet watch` watches.

    **What .NET gives, and what it doesn't.** .NET's hot reload patches
    the running app's methods and then calls every assembly's
    `[MetadataUpdateHandler]` with the types it changed. Code that runs
    again picks up the edit by itself: a click handler, a template's
    `Render` the next time it draws. Code that already ran doesn't: a
    constructor that built a page's views, a theme's `Apply` that built
    the theme, a template's `Apply` that added its parts. So XForms has to
    run that code again, and keep what the user did when it does.

    **How the others do it.** Flutter re-runs every widget's `build()`
    and keeps state, which lives apart from the widgets; React's Fast
    Refresh does the same. MAUI's guidance for UI in C# is a `Build()`
    method that sets a page's content, called again by a hot reload
    handler the app writes and registers. Uno and WinUI create a new
    version of a changed class and swap a new page in, which needs pages
    made by type without arguments. XForms takes Flutter's shape (a build
    method run again, state kept) within its retained views: no handler
    for the app to write, and nothing lost.

    **The plan:**
    - **`Build()` is the one way to build views.** A page, a
      `UserControl` and a template build what they show in a build
      method, which XForms calls when they're first shown and again after
      every edit:
      - A page overrides `protected override View Build()` and returns its
        content; a `UserControl` the same. Their constructors set up only
        fields and state (a view model, the item shown), never views.
        `Page.Content` and `UserControl.Content` are set by XForms from
        `Build()`, not by the app.
      - A field that refers to a view (`this._list = new ListView()`) is
        assigned in `Build()`, so after a rebuild it refers to the new
        view. Event handlers are methods, so they pick up edits anyway.
      - A template builds its parts in its own build method, apart from
        what `Apply` and `Unapply` subscribe to; its drawing (`Render`)
        and layout already run again by themselves.
      - Compiled XAML (phase 29) will produce a `Build()`, so markup and
        C# reload the same way.
    - **Controls declare their user state:** what changed through input
      since the control was created. A text box declares its text and
      selection, a scroll viewer its offset, a list its selection and
      scroll position, a toggle or picker its value, and any view whether
      it has focus. An app's own controls take part the same way. When a
      build runs again, each new view takes the user state of the old view
      in the same place with the same type; a view given a key (as
      Flutter's `Key`) is matched by its key instead, for views that move.
      Where the code and the user disagree, the user wins: editing a text
      box's initial text keeps what was typed, as in Flutter.
    - **A handler in the framework** (`[assembly: MetadataUpdateHandler]`,
      internal), since the framework knows every view, the theme and the
      renderers; XForms adds what it owns (templates, `UserControl`). It
      does nothing unless .NET calls it (only when the app is running with
      hot reload). On the UI thread, after an update:
      - **The theme is rebuilt** (`CreateTheme` again, which runs the
        theme package's `Apply` and the app's own adjustments), and given
        to every view even when it compares equal, so that templates build
        their parts again.
      - **Pages and `UserControl`s build again,** carrying user state
        over: the current page and what's on screen (other windows, open
        popups) at once; pages in the back stack when they're shown again.
      - **Everything is measured and drawn again.**
      - **`Application.HotReloaded`** (an event) tells the app, for
        anything else it built once (a cache).
    - **Writable as an initializer** (a rule for the API, kept from now
      on): everything a view can be given (its properties, its children,
      its place in a panel, such as a grid's cell) can be written in C#'s
      object and collection initializers, so a page's `Build()` can be one
      expression: a tree of `new` with values and children. Event
      handlers, which C# doesn't allow in an initializer, follow the tree
      in one fixed form (`button.Click += this.OnSave;`). A grid's cell is
      already written this way, `Children = { { button, 0, 1 } }`, through
      its `Add(view, row, column)`; a setting that has only a method form
      gets a property or an `Add` overload. This keeps a future designer
      possible that reads and writes C# instead of XAML, as the WinForms
      designer does: it previews any `Build()` by running it, edits the
      code with Roslyn and shows the edit through hot reload, and can
      write back a `Build()` in this form (pages written freely are
      previewed but not edited). Where the code a designer owns lives (a
      file of its own, or a marked `Build()`) is decided with the designer.
    - **No declarative model.** Flutter and React keep state for free
      because their views are descriptions that the framework compares
      and patches. XForms keeps WinUI's retained views, which code holds
      references to and which XAML (phase 29) and WinUI compatibility
      (phase 30) are built on. Patching old views in place instead of
      replacing them was rejected for the same reason: fields assigned in
      `Build()` would refer to discarded views, and handlers and bindings
      would have to be moved across one by one. Replacing the views and
      carrying the user's state over is the sound way in a retained model.
    - **How each platform gets updates** (step 1 checks each, since this
      depends on the tools, not XForms):
      - Win32: Visual Studio's Hot Reload (F5), and `dotnet watch`.
      - Android: Visual Studio's Hot Reload on the emulator or a device;
        `dotnet watch` if the SDK supports it for Android apps.
      - The browser: `dotnet watch` on a Debug build, served by its own
        development server (which delivers the updates), rather than a
        published build on a static server, which can't take them.
      - iOS: Visual Studio Code with a Mac, using the interpreter in Debug
        builds; it builds on Windows here but can't be checked.
    - **What it can't do:** an edit .NET can't apply while running (a
      "rude edit", such as changing a class's base type) still needs a
      restart, as in any .NET app. A change to a page's constructor (its
      fields and state) shows only for pages created after it, as a
      change to Flutter's `initState` does.

    **Steps,** each built for every platform and verified on Win32,
    Android and the browser:
    1. **How updates arrive:** a handler that only logs, in the test app,
       under Visual Studio and `dotnet watch` on Win32, Android and in the
       browser; what each needs (Debug settings, the web development
       server, launch profiles) written down.
    2. **`Build()`:** pages and `UserControl` build in `Build()`, called
       by XForms, and `Content` is no longer the app's to set; templates
       build their parts in their build method. Every page and control in
       the test app, the Basic theme and SoonerOrLater moves to it. No
       hot reload yet: the apps look and work exactly as before.
    3. **Rebuilding:** the framework's handler rebuilds the theme, builds
       templates, pages and `UserControl`s again, measures and draws
       everything again, and raises `HotReloaded`.
    4. **User state:** the way controls declare it, the controls that
       have it (text box, scroll viewer, list, toggles, pickers, focus),
       keys, and carrying it over by place, type and key.
    5. **Docs:** a "Hot reload" section in `architecture.md` (`Build()`,
       user state and keys, each platform's tools, what needs a restart),
       how to write a page and a control, and this document.

    **Verifying:**
    - Step 2: every Win32 test page, light and dark, pixel for pixel
      against the captures before it; a SoonerOrLater walk on Win32; the
      test app and SoonerOrLater on Android and in the browser.
    - Steps 3 and 4, on Win32, Android and in the browser, with the app
      running under hot reload on a page deep in the back stack, with text
      typed into a box, a list scrolled and an item selected, and focus in
      a field: a changed color in `BasicTheme`, a changed template's
      drawing and parts (a button's corner and a combo box's arrow), a
      view added to a page's layout, a changed `UserControl` (one of
      SoonerOrLater's rows), and a changed click handler each show without
      a restart, on the same page, with the back stack, the page's view
      model, the typed text, the scroll position, the selection and the
      focus kept; views reordered with keys keep theirs; the other pages
      show their edits when navigated back to.
    - A Release build never calls the handler, and the web download
      doesn't grow.

    **Decided** (2026-10-22), with no constraint from the current design,
    since nothing has been released:
    - `Build()` is the one way pages, `UserControl`s and templates build
      their views; XForms calls it, and an app registers nothing.
    - Controls declare their user state, carried over a rebuild by place
      and type, or by a key; the user's state wins over the code's.
    - XForms keeps retained views: no declarative model, and no patching
      of old views in place.
    - Everything a view can be given is writable in a C# initializer,
      with event handlers in one fixed form after the tree, so that a
      designer writing C# stays possible.
    - Navigation by path comes first (phase 26), so pages move once, and
      after an edit that needs a restart the app can come back on the same
      page by its stack of paths.

28. **Common controls.** The controls apps expect from MAUI, Avalonia,
    Uno or WinUI and XForms doesn't have yet, so an app doesn't have to
    build them itself. It comes after hot reload, so each is written once
    in the shape phase 27 sets (`Build()`, declared user state), and
    before XAML, so markup has them to use.

    **The controls,** with WinUI's names (decided in phase 24), in three
    groups:
    - **Forms:** `CheckBox` (with a third, indeterminate state),
      `ToggleSwitch`, `Slider`, `ProgressBar` (a value, or indeterminate),
      `TimePicker` (beside `CalendarDatePicker`), `NumberBox` (typed,
      with a range, a step and arrows), `PasswordBox` (over the text box's
      password mode, with a reveal button) and `AutoSuggestBox` (a text
      box with a list of suggestions the app gives as the user types).
    - **Feedback and disclosure:** `ToolTip` (on hover, on keyboard focus,
      and on a long press where there's no hover), `InfoBar` (a message
      in the page, with a severity and an action), a context menu
      (`ContextFlyout` on any view: a right-click, a long press or the
      Menu key, showing `MenuItem`s), and `Expander` (a header that shows
      and hides its content, animated).
    - **Structure:** `NavigationView` (a pane of navigation items, a
      drawer on narrow windows and a side pane on wide ones, as Android's
      and iOS's apps and Windows' Settings do), `RefreshContainer` (pull
      to refresh, around a list), `FlipView` with `PipsPager` (pages swiped
      one at a time, and dots showing where), and `TreeView` (nested items
      that expand, with keyboard and selection as `ListView`'s).

    **For each control:**
    - A functional control in XForms, and its template in
      `XForms.Themes.Basic`; no look in the control (phase 24).
    - Its accessibility role, name and value on every platform (UI
      Automation, TalkBack, VoiceOver, the browser's ARIA), and its
      keyboard behavior as WinUI's.
    - Right to left, the system's text scale, light, dark and high
      contrast.
    - Everything it takes writable in an initializer (phase 27's rule),
      its user state declared (phase 27), and a section on a test app page
      that shows every state.

    **Not in this phase,** each a phase of its own when it's wanted:
    `DataGrid` (columns, sorting, editing; large on its own), and the
    controls that host a platform's native component (`WebView`, a media
    player, maps), which need the platforms to embed native views, as
    phase 31 does in the other direction.

    **Steps,** each built for every platform and verified on Win32,
    Android and the browser:
    1. **Forms:** the eight form controls.
    2. **Feedback and disclosure:** tooltips, the info bar, context menus
       and the expander.
    3. **Structure:** the navigation view, pull to refresh, the flip view
       and pips, and the tree view.
    4. **Docs:** `architecture.md`'s list of controls, and this document.

    **Verifying,** for each control on Win32, Android and in the browser:
    every state on its test page, light, dark and high contrast, against
    WinUI's behavior for the same input (pointer, touch, keyboard); the
    screen reader's announcement (Narrator through UI Automation,
    TalkBack, and the browser's accessibility tree); right to left; and a
    SoonerOrLater walk unchanged (it uses none of them yet).

    **Decided** (2026-10-22, taking the recommendations when asked to
    carry on through this phase):
    - All three groups, in this phase.
    - The time picker is drawn, as XForms' date picker is, so it looks the
      same everywhere.

    **Status: done,** with the controls in `architecture.md` (step 4: a
    Controls section, and the input, accessibility and popup sections
    brought up to date).

    Step 1, forms: `CheckBox` (`IsChecked` a `bool?`, `IsThreeState`),
    `ToggleSwitch` (`Header`, `IsOn`, `OnContent` and `OffContent`), a
    `RangeBase` under `Slider` (`Header`, `StepFrequency`, `SmallChange`,
    `LargeChange`; a mouse press drags at once, a touch once it moves
    sideways, so a vertical drag still scrolls) and `ProgressBar`
    (`IsIndeterminate`, `ShowError`, `ShowPaused`; Basic slides the
    indeterminate segment on a layer of its own), `TimePicker` (a
    `PickerField` whose popup has hour, minute and AM or PM columns and
    Accept and Dismiss, `ClockIdentifier`, `MinuteIncrement`),
    `NumberBox` (typed and committed on Enter or leaving it, a range,
    `SpinButtonPlacementMode`), `PasswordBox` (`PasswordRevealMode`, Peek
    by default) and `AutoSuggestBox` (`ItemsSource` set by the app as the
    text changes, `TextChanged` with its reason, `SuggestionChosen`,
    `QuerySubmitted`), each with a Basic template and its user state, on
    a new forms page in the test app (`/forms`). What they needed:
    - Accessibility: roles for a check box, a switch, a slider, a progress
      bar, a spin button (and, for steps 2 and 3, a tooltip, an alert, a
      menu and its items, and a tree and its items); a mixed checked state;
      a range value (`View.AccessibilityRange`, and
      `SetAccessibilityRangeValue`) and the increment and decrement
      actions. Mapped to UI Automation (the control types, the toggle
      state's indeterminate, the RangeValue pattern), TalkBack (the
      widget classes, range info, scroll forward and back, set progress),
      VoiceOver (the adjustable trait, increment and decrement) and ARIA
      (the roles, `aria-checked="mixed"`, `aria-valuenow`, `-min`,
      `-max`).
    - A single-line text box offers the keys it has no use for (Up, Down,
      Page Up, Page Down, Escape) to its ancestors first, on Win32, in the
      browser and on Android, for the number box's steps and the
      suggestion list.
    - `Popup.IsLightDismissEnabled` (WinUI's name): false for a popup that
      leaves focus where it was and lets presses outside it through (the
      suggestion list, which mustn't take the keyboard from its box), on
      every platform.

    Checked on Win32 through UI Automation (each control's type, name,
    patterns and states; toggling, a range value set, the number box typed
    and stepped, the pickers expanded; real input was blocked, as another
    process held the foreground), in the browser with the mouse and the
    keyboard (each control's ARIA, Tab order, Space, Enter, the slider's
    keys, the time picker opened, moved and accepted from the keyboard, the
    number box's keys and buttons, the password peek, the suggestions'
    Down, Up, Enter, Escape and click), and on Android with taps and drags
    and TalkBack's tree (each control's widget class, label, checked and
    enabled state). Light, dark, high contrast and right to left on the
    page. Not checked: typing on Android (the keyboard's stylus
    introduction covers the app), and so the password peek and the
    suggestions there.

    Fixed on the way:
    - A disabled templated control took keyboard focus and read as enabled
      (`IsEnabled` now makes it not hit-test visible, as the platforms
      read it).
    - `NotifyAccessibilityStateChanged` created a view's native object
      when called before it was attached (from a property set in an
      initializer), so properties set after it were missed (a progress
      bar's label in the browser).
    - In the browser, a focused lightweight view gave assistive technology
      nothing (its layer has the focus): the layer now points at the
      view's element (`aria-activedescendant`).
    - A popup with more room above its target still opened below it,
      squeezed, when its content fits itself to the space it's given (a
      list, a calendar): the side is chosen by its natural height (Win32,
      Android, iOS).
    - On Win32, a page whose only child is native (a scroll viewer) drew
      no background.
    - On Android, a lightweight view that clips (`ClipToBounds`) couldn't
      clip a native child: a view that clips has a native object.
    - On Win32, RichEdit's Direct2D window class crashed the process now
      and then (an access violation inside RichEdit: about one start in
      nine with a few text boxes, with or without the spell checker, which
      the known issue blamed); text boxes use the classic class, which
      draws the same and didn't crash in 35 starts.

    Step 2, feedback and disclosure: `ToolTip` and `ContextFlyout` on any
    view, as extension properties (`ViewAttachments`, so an initializer
    can set them: `new Button { ToolTip = "...", ContextFlyout = new
    MenuFlyout { Items = { ... } } }`); the tooltip shows after the pointer
    rests half a second, on keyboard focus, and for a few seconds after a
    long press on a view without a context menu, beside the view in a popup
    that doesn't take focus. `MenuFlyout` (the existing `MenuItem`s, now
    with `Click`; an item whose command can't run is shown unavailable;
    Up and Down wrap, Enter chooses, Escape closes). `InfoBar` (`Title`,
    `Message`, `Severity`, `IsOpen`, `IsClosable`, `ActionButton`,
    `Closed`; read as it opens: UI Automation's live region, ARIA's
    status, Android's live region) and `Expander` (`Header`, `Content`,
    `IsExpanded`, `Expanding`, `Collapsed`; the content fades and slides
    in), on a new feedback page (`/feedback`). What they needed:
    - Context requests in the framework: `View.ContextRequested` (and
      `OnContextRequested`), raised for the view under a right-click, a
      long press of a touch or pen (half a second, held still; answered,
      the press ends as canceled, so it isn't also a tap), and the focused
      view on the Menu key (a new `Key.Menu`) or Shift+F10, and then for
      its ancestors until one answers. On Win32 a right-click had pressed
      like a left one (it now asks for the context menu), and so had the
      browser's; Narrator's context menu command reaches it through UI
      Automation's `ShowContextMenu`.
    - In the browser, keyboard focus goes back to where it was when a
      popup closes, as on Win32; and the controls inside an accessible
      container (an info bar's buttons) have elements of their own.

    Checked in the browser with the mouse, the keyboard and touch (a
    tooltip after hovering and gone when the pointer leaves; the menu by a
    right-click, the Menu key, Shift+F10 and a touch's long press, chosen
    from the keyboard and by a click, with focus back on its button; a
    long press showing a tooltip; an info bar closed and its action
    button; an expander opened), on Android with taps and long presses
    (the menu, the tooltip, the info bars, the expander, and each one's
    widget class and label), and on Win32 through UI Automation (each
    element's type, name and patterns; an info bar closed, an expander
    expanded and collapsed). Not checked: the menu and the tooltip with
    Win32's mouse and keyboard (another process held the foreground).

    Step 3, structure: `NavigationView` (`MenuItems` of
    `NavigationViewItem`s, each with `Text`, `Icon` and `Tag`;
    `PaneTitle`, `Content`, `SelectedItem`, `SelectionChanged`; a side
    pane on a wide window and, below 640 wide, a drawer a menu button
    opens over a shade, which a tap on the shade or Escape closes; focus
    moves into the drawer from the keyboard and back to its button),
    `FlipView` (`Items`, `SelectedIndex`; a horizontal swipe follows the
    finger and settles, a fifth of the width moving on; arrow buttons;
    Left, Right, Page Up, Page Down, Home and End) with `PipsPager`
    (`NumberOfPages`, `SelectedPageIndex`, read as a range: "2 of 4"),
    `TreeView` (`RootNodes` of `TreeViewNode`s, `SelectedNode`,
    `ItemInvoked`; WinUI's keys: Up and Down, Right to expand or go to the
    first child, Left to collapse or go to the parent, Home and End, Enter
    and Space) and `RefreshContainer` (`RefreshRequested` with a deferral,
    `RequestRefresh`), on a new structure page (`/structure`). Each item
    of a navigation view and a tree is a view of its own, so assistive
    technology reads each (a list item, a tree item with its expanded and
    selected states). `RefreshContainer` drives the pull to refresh the
    platforms already give a list (`ItemsView.IsRefreshEnabled`: Android's
    SwipeRefreshLayout, iOS's UIRefreshControl, and Win32's and the
    browser's own indicators); around other content it refreshes only when
    asked (`RequestRefresh`), since a scroll viewer's pull is the
    platform's (the browser and Android scroll it natively).

    Fixed on the way:
    - Setting `FlowDirection` laid out and drew again only the view
      itself, not the descendants that take its direction; and a view
      drawn under one direction and attached again under another kept its
      drawing.
    - A scroll viewer's content was placed off screen right to left (it
      was mirrored against a width the scroll viewer never had; the
      platforms place it, filling the viewport).
    - Parts that must be above a native sibling (the flip view's buttons
      over its strip, the drawer's shade over the content) have layers of
      their own: a lightweight view is drawn into its layer, beneath its
      native siblings.

    Checked in the browser with the mouse, the keyboard and touch (each
    item's ARIA; swipes, a short swipe staying, the pips, the flip view's
    keys; the tree's keys and expanded states; pull to refresh; the
    drawer from the keyboard and closed by its shade; right to left), on
    Android with taps and swipes (swipes, the drawer, its items, pull to
    refresh, each one's widget class and label), and on Win32 through UI
    Automation (each element's type, name and patterns; the navigation
    items selected, the flip buttons and pips, the tree expanded and
    selected, a refresh, the drawer on a narrow window, right to left).
    Found, not fixed: a text block's text keeps to the left right to left
    (its alignment doesn't follow the direction); a drawer opened with the
    mouse doesn't close on Escape (focus is still outside it).

    **iOS** (2026-10-23), on the simulator (an iPhone, through a Mac),
    driven with Facebook's idb (taps, drags, typing, and the accessibility
    tree): the forms page. Check boxes (three states, read as "Mixed"),
    the disabled one ignoring taps, the switch, the slider tapped and
    dragged (read as adjustable, with its percentage), the time picker
    accepted, the number box's spin buttons and a typed value clamped to
    its range, the password box, and the suggestion list typed into and
    chosen from. Fixed on the way:
    - Drawn text was offset and cut short: `NSLayoutManager` lays out
      lazily, so the used rectangle was read before layout.
    - The pickers page crashed: an image view's image was set off the UI
      thread.
    - Lightweight views' accessibility frames were about 70 points off:
      the frame is now converted from the view's place when asked.
    - The keyboard covered the page: pages now lay out in the space above
      it, the focused text entry is scrolled into view, and popups are
      placed above it.
    - Text boxes never reported keyboard focus, so the suggestion list
      never opened.

    Then a hardware keyboard (an iPad's Magic Keyboard), and the rest of
    the pass, on an iPad simulator and the iPhone, with idb's key presses:
    - Tab and Shift+Tab move through the page from anywhere: from nothing
      focused (the root view controller has focus then, and starts at the
      first view, as the browser does), out of text boxes (a single-line
      box also offers Up, Down, Page Up, Page Down and Escape to its
      ancestors, as on the other platforms), and within an open menu,
      picker or dialog, as on Win32.
    - Return in a single-line box submits and keeps focus, as on the
      desktop (the on-screen keyboard's Done key still puts the keyboard
      away); an input method's composition keeps its Return.
    - Escape, and Command+[, go back when nothing uses them; the Menu key
      opens a context menu.
    - Focus goes back to where it was when a popup closes, and a focused
      view with a layer of its own (the navigation view's menu button)
      draws its ring.
    - Checked: the forms page from the keyboard (check boxes, the slider's
      arrows, the time picker opened, changed and accepted, the number box
      committed on Tab and Return, the suggestions' Down, Return and
      Escape), the feedback page (the menu from a long press and the Menu
      key, tooltips on a long press and on focus, an info bar closed, an
      expander opened), the structure page (the side pane on the iPad, the
      drawer on the iPhone from a tap and the keyboard, the tree's and the
      flip view's keys, pull to refresh), the app page (Escape back, a
      message box answered from the keyboard), the text, input, binding,
      lists and pickers pages, every page started and shown, dark mode and
      right to left.

    Fixed on the way:
    - A text box's focus went to its container, not its editor, so Tab
      never reached a text box.
    - VoiceOver: the controls inside an accessible view (an info bar's
      buttons) are elements of their own, after it; a list, a menu, a tree
      or a group isn't an element itself, so its items are reached (the
      navigation pane's, a popup's, the flip view's); a switch reads as a
      toggle (UIKit's toggle button trait, iOS 17) with its state; expanded
      and collapsed views report it (iOS 18); a menu, picker or dialog
      keeps VoiceOver inside it while it's open; VoiceOver's escape gesture
      does what Escape does; a multiline box is named by its placeholder.
    - A single-line box's placeholder had UIKit's color, not the theme's.

    Not checked on iOS: VoiceOver's speech itself (only its tree, through
    idb), Shift+Tab and Command+[ (idb sends no modifiers), and a real
    device.

    Then what that pass found, on every platform:
    - **Lists from the keyboard.** A list is one Tab stop: the item focus
      was last on, or the first in view (`View.IsTabStop`, internal, which
      Tab follows on every platform; Win32's own traversal too). The arrow
      keys move between the items (all four in a grid, mirrored right to
      left), Home and End go to the ends, Page Up and Page Down ten lines,
      scrolling the item into view and passing over rows that aren't items
      (a group header); Enter and Space click it, as a tap. An item's keys
      reach its list (an item's container isn't attached to it), and so
      does a context request. The browser's list scrolls only as far as
      it must, as the other platforms' do, and names a reused element for
      its new item; Android's native lists no longer take focus from their
      items.
    - **A tree is one Tab stop,** as WinUI's: the item focus was last on,
      or the selected one, or the first.
    - **`View.AccessibilityIsHidden`** (new): assistive technology skips a
      view and everything in it, though it shows. The navigation view
      hides its content while its drawer is open (Win32's tree, the
      browser's `aria-hidden` on every element inside, Android's
      `importantForAccessibility`, iOS's `accessibilityElementsHidden`).
    - A lightweight view's focus ring stays inside its layer, which clipped
      it (a list's or a tree's item that fills the width).
    - iOS scrolls the focused text entry into view again once the page has
      laid out above the keyboard, and when focus moves to another entry
      while the keyboard shows (Tab, or the next field).

    Checked: lists and the tree from the keyboard on the iPad simulator,
    in the browser (headless Edge) and on the Android emulator; the drawer
    hiding its content on the iPhone simulator, in the browser and on
    Android; moving between text boxes with the keyboard up on the iPhone.
    Not checked: Win32 from the keyboard (real input was blocked: another
    process held the foreground), and VoiceOver's and TalkBack's speech.
    Found, not fixed: Tab reaches a navigation view's content before its
    pane (its children are in drawing order, which Tab follows).

29. **Later: `XForms.Xaml`.** The proof of concept in `spikes/XForms.Xaml`
    becomes a layer over the framework and controls, including templates
    written in markup. To be planned in detail when it's next.

30. **Later: `XForms.Compat.WinUI`.** Adapters giving WinUI's names and a
    `DependencyProperty`-style API over XForms' controls, with XAML
    namespace mappings so existing markup loads. It stays separate from any
    Fluent theme, so neither requires the other, and keeps its types in
    `XForms.Compat.WinUI` rather than Microsoft's namespaces. To be planned
    in detail when it's next.

31. **Later: embedding.** XForms inside part of an existing app's UI,
    beside native content, with `Attach` beside each platform's `Run`
    (phase 23): `Run` shows the app in a whole host; `Attach` puts it in
    a container the caller owns and keeps running its own UI.
    - Win32: `Win32Application.Attach(parentHwnd, new App())` creates a
      child window in the caller's window and uses the caller's message
      loop, leaving the process's DPI awareness alone. That's how
      WebView2's controller, XAML Islands' `DesktopWindowXamlSource` and
      CEF embed; taking over the caller's top-level window instead would
      mean competing with its window procedure for every message XForms
      needs.
    - The browser: `WebApplication.AttachAsync(element, new App())`, in
      one element of an existing page.
    - Android: `AndroidApplication.Attach(activity, hostView, new App())`,
      in a `ViewGroup` of the activity, as HotChai.Game's
      `AndroidGameEngine.Run(activity, hostView, game)`.
    - iOS: `IosApplication.Attach(viewController, new App())`, as a child
      view controller.

    The hard part is shared: keyboard focus and Tab moving across the
    boundary between the host's UI and XForms, and the host and XForms
    each knowing which has focus. To be planned in detail when it's next.

**Candidates from SwiftUI** (recorded 2026-10-22, not yet in order or
planned). SwiftUI's views are values described again whenever the state
they read changes; XForms keeps retained views (phase 27), so that model,
chained modifiers (a second way to set what an initializer sets, phase
27's rule) and property wrappers that hide who owns state aren't adopted.
What developers praise in SwiftUI is mostly apart from that, and these
carry over. Observation and environment values would come first:

32. **Candidate: observation.** A view model marked with an attribute
    notifies of its changes without hand-written `INotifyPropertyChanged`,
    as SwiftUI's `@Observable`, so binding updates whatever reads it. A
    source generator writes the notifications at compile time, so it's
    safe for trimming and AOT. The largest day-to-day gain.

33. **Candidate: environment values.** Typed values that flow down the
    tree and that any subtree can override, as SwiftUI's `@Environment`:
    one mechanism for what XForms passes down case by case today (the
    theme, flow direction), and for more, such as navigation reachable
    from any view, a subtree disabled, or a subtree styled differently.
    Keys are typed objects, not strings or reflection.

34. **Candidate: roles.** Say what a control means, not only how it
    looks, as SwiftUI's `ButtonRole` and text styles: a button that is
    primary, destructive or cancel, which themes and platforms present
    (a destructive button in red, Escape for cancel, Enter for the
    default), beside the theme's text styles, which already follow the
    system's text scale.

35. **Candidate: previews.** A preview is a few lines of code beside a
    page or control that show it with sample data, as SwiftUI's
    `#Preview`, run by the future designer (phase 27's rule) and shown
    live through hot reload. Cheap once `Build()` and hot reload exist.

36. **Candidate: implicit animation.** "Animate whatever this change
    causes", as SwiftUI's `withAnimation { ... }`: property and layout
    changes made inside it animate with the theme's motion, without an
    animation written for each. XForms' animator and layout transitions
    stay for animations written out.
