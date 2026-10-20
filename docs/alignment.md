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
| 3 | **Theming** | `AppTheme` (light and dark) with five colors | Resources, styles, and reporting the system theme, accent color and high contrast |
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
- **Theme.** Keep `AppTheme` (MAUI's name), and report system theme changes.

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

### 7. Be clear about what ports

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
| `ListViewItemContainer` | `ListViewItem` | WinUI, Avalonia | The container, as in those frameworks |
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
| `Task` returned by animations | `AnimationHandle` | HotChai.Game | Can be awaited or cancelled; an interrupted animation never completes |
| `Timer` | `DispatcherTimer` | WPF, WinUI, Avalonia | No priorities |
| `PointerInputEvent` | `PointerEventArgs` | Avalonia | With pointer ID, position, button and modifier state |
| `KeyInputEvent` | `KeyEventArgs` | WPF, Avalonia | `Key` stays |
| `GestureRecognizer`, `TapGestureRecognizer`, `SwipeGestureRecognizer` | Keep | MAUI | Plus `Tapped`, `DoubleTapped` and `Holding` events on views |
| `AccessibilityRole` | Keep | SwiftUI, Flutter, React Native | Plain view property |
| `AppTheme`, `LightTheme`, `DarkTheme` | `AppTheme` (Light, Dark), with resources for colors | MAUI | Theme colors move toward resources |
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
| (new) | `Animator.TranslateFrom(oldOffset, zero)` for layout transitions, used by transition panels | HotChai.Game (`MoveTo`) |
| `View.AccessibilityLabel` | Keep | SwiftUI, Flutter, React Native |
| `UserControl.DrawBackground(IDrawContext, Rectangle)` | `Control.Render(DrawingContext)` | Avalonia |
| `UserControl.Invalidate()` | `InvalidateVisual()` | WPF, Avalonia |
| `Page.Layout` | `Page.Content` | WinUI, Avalonia, MAUI |
| `Application.NavigateTo`, `NavigateBack`, `CanNavigateBack` | `NavigateAsync(page)`, `GoBackAsync()`, `CanGoBack` | MAUI, WinUI |
| `Application.InvokeOnUIThreadAsync` | `Dispatcher.Post` and `Dispatcher.InvokeAsync` | Avalonia, WPF |
| `Application.OpenUri` | A launcher service from the application (`OpenAsync`) | MAUI |
| `Application.MeasureText` | `TextLayout` | Avalonia, DirectWrite |
| `TextView.ForegroundColor` | `Foreground` | All |
| `TextView.HorizontalTextAlignment` | `TextAlignment` | WPF, WinUI, Avalonia |
| `TextView.WordWrap` | `TextWrapping` | WPF, WinUI, Avalonia |
| `TextView.TextTrimming` (`bool`) | `TextTrimming` (enum) | WPF, WinUI, Avalonia |
| `TextEntry.Multiline` | `AcceptsReturn` | WPF, WinUI, Avalonia |
| `TextEntry.KeyboardOptions` | `IsSpellCheckEnabled`, `IsTextPredictionEnabled` | WinUI |
| (new) | `TextBox.TextChanged` event | All |
| `ScrollView.ScrollTo` and `VerticalOffset` | `ScrollToVerticalOffset` and `VerticalOffset` | WPF |
| `ListView.Bind(source)` | `ItemsSource` | All |
| `ListView.ItemClicked` | `ItemClick` | WinUI |

### Toolkit

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
| Storyboards | Partly. Animations of translation, scale, rotation, opacity and clip would map to `Animator` channels, which don't exist yet. Animations of layout properties aren't supported, by design. |
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
| `TextEntry` raises no event when its text changes | None: two-way bindings behave as one-way | 8 |
| No opacity animation, and no frame callback | Opacity is stepped on the UI thread by a timer; translation uses `TranslateTo` | 6 |
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
  [benchmarks.md](benchmarks.md)) covers that risk, and phase 7 answers it.

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
     - `ListView<T>`, `ListViewItem` and `ItemClick` (phase 9).
     - Async navigation and `Dispatcher` (phase 10).
     - `RelayCommand` (phase 3).
   - **Still to do:**
     - Make the renderer interfaces and `Application.Renderer` internal.
     - Theme overrides still run from `Element`'s constructor, before
       derived constructors. A control whose `OnApplyTheme` uses views that
       its constructor builds has to skip that first call (SoonerOrLater's
       `TaskListViewItem` does). Phase 3's property system should replace
       this with theme resources.
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
4. **Input.**
   - Add pointer, wheel, key and focus events and overrides, with multiple
     pointers and explicit pointer capture.
   - Add an application-level input filter.
   - Add `Tapped`, `DoubleTapped` and `Holding`, plus pinch and pan
     recognizers.
   - Add key routing on Android and iOS.
5. **Drawing and text.**
   - Expand `DrawingContext` on Direct2D, Core Graphics and Android `Canvas`:
     paths, rounded rectangles, immutable brushes, push/pop of clip,
     transform and opacity, and `TextLayout`.
   - Add font families and bundled fonts, text spans, and maximum lines.
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
7. **Lightweight controls.** After the Animator, since it decides which views
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
8. **Text entry.** Add `TextChanged`, a submitted event, password mode,
   maximum length, read-only and a selection API.
9. **Lists.** Add `ListView<T>` with typed item sources and item views that
   are created once and bound to many items, plus grouping, horizontal and
   grid layouts, and selection modes.
10. **App layer.**
   - Asynchronous, instance-based navigation, with parameters, results,
     modal pages and transitions.
   - Platform services, as interfaces from the application.
   - Windows on desktop.
   - System theme and text-scale changes.
11. **Ongoing.** Accessibility (states, values, actions; Android and iOS) and
    keyboard focus on every platform, alongside the phases above.
