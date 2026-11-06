# XForma

## AccessibilityAction

What assistive technology can ask a view to do.

| Member | |
|---|---|
| `Collapse = 4 -> XForma.AccessibilityAction` | Closes a view with an expanded state. |
| `Decrement = 6 -> XForma.AccessibilityAction` | Steps a range's value down. |
| `Expand = 3 -> XForma.AccessibilityAction` | Opens a view with an expanded state, such as a picker's popup. |
| `Increment = 5 -> XForma.AccessibilityAction` | Steps a range's value up. |
| `Invoke = 0 -> XForma.AccessibilityAction` | Does what a tap would. |
| `Select = 2 -> XForma.AccessibilityAction` | Selects a list item, tab or radio button. |
| `Toggle = 1 -> XForma.AccessibilityAction` | Checks or unchecks a view with a checked state. |

## AccessibilityRange

A view's value within a range, for assistive technology: a slider's, a progress bar's or a number box's.

| Member | |
|---|---|
| `AccessibilityRange() -> void` |  |
| `AccessibilityRange(double Minimum, double Maximum, double Value) -> void` |  |
| `Deconstruct(out double Minimum, out double Maximum, out double Value) -> void` |  |
| `Equals(XForma.AccessibilityRange other) -> bool` |  |
| `Equals(object obj) -> bool` |  |
| `GetHashCode() -> int` |  |
| `Maximum.get -> double` |  |
| `Maximum.init -> void` |  |
| `Minimum.get -> double` |  |
| `Minimum.init -> void` |  |
| `ToString() -> string` |  |
| `Value.get -> double` |  |
| `Value.init -> void` |  |
| `operator !=(XForma.AccessibilityRange left, XForma.AccessibilityRange right) -> bool` |  |
| `operator ==(XForma.AccessibilityRange left, XForma.AccessibilityRange right) -> bool` |  |

## AccessibilityRole

What a view is to assistive technology such as a screen reader. Each platform maps these onto its own accessibility roles.

| Member | |
|---|---|
| `Alert = 19 -> XForma.AccessibilityRole` | A message about the app's state, read as it appears. |
| `Button = 2 -> XForma.AccessibilityRole` |  |
| `CheckBox = 13 -> XForma.AccessibilityRole` | A box that's checked, unchecked or (when `AccessibilityIsMixed`) neither. |
| `ComboBox = 11 -> XForma.AccessibilityRole` | A field that shows a value and opens a picker for it. |
| `Group = 9 -> XForma.AccessibilityRole` |  |
| `Heading = 12 -> XForma.AccessibilityRole` | Text that heads a section, which screen readers can jump between. |
| `Image = 7 -> XForma.AccessibilityRole` |  |
| `List = 5 -> XForma.AccessibilityRole` |  |
| `ListItem = 6 -> XForma.AccessibilityRole` |  |
| `Menu = 20 -> XForma.AccessibilityRole` | A list of commands. |
| `MenuItem = 21 -> XForma.AccessibilityRole` | A command in a menu. |
| `None = 0 -> XForma.AccessibilityRole` | Not exposed itself; its accessible descendants are. |
| `ProgressBar = 16 -> XForma.AccessibilityRole` | How far a task has got: a range, or none while it's indeterminate. |
| `RadioButton = 10 -> XForma.AccessibilityRole` |  |
| `Slider = 15 -> XForma.AccessibilityRole` | A value the user picks from a range (`AccessibilityRange`). |
| `SpinButton = 17 -> XForma.AccessibilityRole` | A number the user types, or steps up and down. |
| `Switch = 14 -> XForma.AccessibilityRole` | An on and off switch. |
| `TabItem = 4 -> XForma.AccessibilityRole` |  |
| `Text = 1 -> XForma.AccessibilityRole` |  |
| `TextBox = 8 -> XForma.AccessibilityRole` |  |
| `ToggleButton = 3 -> XForma.AccessibilityRole` |  |
| `ToolTip = 18 -> XForma.AccessibilityRole` | A short description shown beside a view. |
| `Tree = 22 -> XForma.AccessibilityRole` | A hierarchy of items that expand and collapse. |
| `TreeItem = 23 -> XForma.AccessibilityRole` | An item in a tree. |

## Angle

An angle, in degrees or radians.

| Member | |
|---|---|
| `Angle() -> void` |  |
| `Degrees.get -> float` |  |
| `Equals(object obj) -> bool` |  |
| `FromDegrees(float degrees) -> XForma.Angle` |  |
| `FromRadians(float radians) -> XForma.Angle` |  |
| `GetHashCode() -> int` |  |
| `Radians.get -> float` |  |
| `Zero -> XForma.Angle` |  |
| `operator !=(XForma.Angle a, XForma.Angle b) -> bool` |  |
| `operator ==(XForma.Angle a, XForma.Angle b) -> bool` |  |

## AnimationChannel

A render property of an animator's layer, as one number.

| Member | |
|---|---|
| `ClipBottom = 9 -> XForma.AnimationChannel` |  |
| `ClipLeft = 6 -> XForma.AnimationChannel` |  |
| `ClipRight = 8 -> XForma.AnimationChannel` |  |
| `ClipTop = 7 -> XForma.AnimationChannel` |  |
| `Opacity = 5 -> XForma.AnimationChannel` |  |
| `Rotation = 4 -> XForma.AnimationChannel` | Degrees, clockwise. |
| `ScaleX = 2 -> XForma.AnimationChannel` |  |
| `ScaleY = 3 -> XForma.AnimationChannel` |  |
| `TranslationX = 0 -> XForma.AnimationChannel` |  |
| `TranslationY = 1 -> XForma.AnimationChannel` |  |

## AnimationHandle

The result of starting an animation: await it for whether it finished (true) or was interrupted (false), or cancel it.

| Member | |
|---|---|
| `Cancel() -> void` | Stops the animation where it is. |
| `Completion.get -> System.Threading.Tasks.Task<bool>` | Completes with true when the animation finishes, or false when it's stopped or replaced first. A looping animation completes only when stopped. |
| `GetAwaiter() -> System.Runtime.CompilerServices.TaskAwaiter<bool>` |  |
| `IsRunning.get -> bool` |  |

## AnimationTiming

When and how an animation plays.

| Member | |
|---|---|
| `AnimationTiming() -> void` |  |
| `AutoReverse.get -> bool` | Each play runs forward, then backward. |
| `AutoReverse.init -> void` | Each play runs forward, then backward. |
| `Delay.get -> System.TimeSpan` |  |
| `Delay.init -> void` |  |
| `Duration.get -> System.TimeSpan` |  |
| `Duration.init -> void` |  |
| `Easing.get -> XForma.EasingFunction` |  |
| `Easing.init -> void` |  |
| `Loop.get -> bool` | Plays again and again until stopped. |
| `Loop.init -> void` | Plays again and again until stopped. |

## Animator

A view's render properties: translation, scale, rotation, opacity and clip, around a transform origin. They don't affect layout. Set them directly, or animate them with the `To` and `From` methods; the platform's compositor plays the animations.

| Member | |
|---|---|
| `Animator() -> void` |  |
| `Clip.get -> XForma.Rect?` | The part of the view shown (in its own coordinates), or null (the default) for all of it. |
| `Clip.set -> void` | The part of the view shown (in its own coordinates), or null (the default) for all of it. |
| `FadeFrom(float opacity, System.TimeSpan duration, XForma.EasingFunction easing = null, System.TimeSpan delay = default(System.TimeSpan)) -> XForma.AnimationHandle` |  |
| `FadeTo(float opacity, System.TimeSpan duration, XForma.EasingFunction easing = null, System.TimeSpan delay = default(System.TimeSpan), bool loop = false, bool autoReverse = false) -> XForma.AnimationHandle` |  |
| `Opacity.get -> float` |  |
| `Opacity.set -> void` |  |
| `RevealFrom(XForma.Rect clip, System.TimeSpan duration, XForma.EasingFunction easing = null, System.TimeSpan delay = default(System.TimeSpan)) -> XForma.AnimationHandle` | Reveals the whole view, growing the clip from a rectangle (such as an empty one at an edge); the clip is removed when it completes. |
| `RevealTo(XForma.Rect clip, System.TimeSpan duration, XForma.EasingFunction easing = null, System.TimeSpan delay = default(System.TimeSpan)) -> XForma.AnimationHandle` | Animates the clip to a rectangle (in the view's coordinates). |
| `RotateFrom(XForma.Angle from, XForma.Angle to, System.TimeSpan duration, XForma.EasingFunction easing = null, System.TimeSpan delay = default(System.TimeSpan), bool loop = false, bool autoReverse = false) -> XForma.AnimationHandle` | Animates the rotation between two angles, for example a looping spin: `RotateFrom(Angle.Zero, Angle.FromDegrees(360), period, loop: true)`. |
| `RotateTo(XForma.Angle angle, System.TimeSpan duration, XForma.EasingFunction easing = null, System.TimeSpan delay = default(System.TimeSpan), bool loop = false, bool autoReverse = false) -> XForma.AnimationHandle` |  |
| `Rotation.get -> XForma.Angle` |  |
| `Rotation.set -> void` |  |
| `Scale.get -> float` | A uniform scale (both axes). |
| `Scale.set -> void` | A uniform scale (both axes). |
| `ScaleFrom(float scale, System.TimeSpan duration, XForma.EasingFunction easing = null, System.TimeSpan delay = default(System.TimeSpan)) -> XForma.AnimationHandle` |  |
| `ScaleTo(float scale, System.TimeSpan duration, XForma.EasingFunction easing = null, System.TimeSpan delay = default(System.TimeSpan), bool loop = false, bool autoReverse = false) -> XForma.AnimationHandle` |  |
| `ScaleX.get -> float` |  |
| `ScaleX.set -> void` |  |
| `ScaleY.get -> float` |  |
| `ScaleY.set -> void` |  |
| `Stop() -> void` | Stops every animation, leaving the values where they are. |
| `TransformOrigin.get -> XForma.Point` | The point scale and rotation are around, relative to the view's size: (0.5, 0.5), the center, by default. |
| `TransformOrigin.set -> void` | The point scale and rotation are around, relative to the view's size: (0.5, 0.5), the center, by default. |
| `TranslateByOffset(XForma.Point offset, System.TimeSpan duration, XForma.EasingFunction easing = null) -> XForma.AnimationHandle` | Animates the translation from an offset back to where it's heading, continuing from where it is now, for a layout change. |
| `TranslateFrom(XForma.Point point, System.TimeSpan duration, XForma.EasingFunction easing = null, System.TimeSpan delay = default(System.TimeSpan)) -> XForma.AnimationHandle` | Animates the translation from the point to its current value, as for a layout change: `TranslateFrom(oldPosition - newPosition)`. |
| `TranslateTo(XForma.Point point, System.TimeSpan duration, XForma.EasingFunction easing = null, System.TimeSpan delay = default(System.TimeSpan), bool loop = false, bool autoReverse = false) -> XForma.AnimationHandle` |  |
| `Translation.get -> XForma.Point` |  |
| `Translation.set -> void` |  |

## Application

The app: its pages, its theme and its life. Derive from it, list the pages in `OnConfigureRoutes`, and start it with `XFormaApp.Run`.

| Member | |
|---|---|
| `AccentColor.get -> XForma.Color?` | The app's accent color, or null (the default) for the system's (or, without one, `DefaultAccentColor`). |
| `AccentColor.set -> void` | The app's accent color, or null (the default) for the system's (or, without one, `DefaultAccentColor`). |
| `ActualAccentColor.get -> XForma.Color` | The accent color in use. |
| `ActualThemeMode.get -> XForma.ThemeMode` | The mode in use. The system's high contrast always wins, since people who turn it on need it. |
| `Application() -> void` |  |
| `BackgroundRefreshInterval.get -> System.TimeSpan?` | How often the system may wake the app in the background to call `OnBackgroundRefreshAsync`, at the soonest, or null (the default) for never. The system decides when, from how the app is used. iOS only (a background app refresh task, which the app's Info.plist declares; see the docs). |
| `BackgroundRefreshInterval.set -> void` | How often the system may wake the app in the background to call `OnBackgroundRefreshAsync`, at the soonest, or null (the default) for never. The system decides when, from how the app is used. iOS only (a background app refresh task, which the app's Info.plist declares; see the docs). |
| `BadgeCount.get -> int` | The number on the app's icon, such as the tasks due: 0 (the default) for none. On iOS the first count asks the user to allow badges; on the web an installed app shows it (the Badging API). Android and Windows don't show it. Set it as the app suspends (see `OnSuspend`) and in `OnBackgroundRefreshAsync` to keep it current. |
| `BadgeCount.set -> void` | The number on the app's icon, such as the tasks due: 0 (the default) for none. On iOS the first count asks the user to allow badges; on the web an installed app shows it (the Badging API). Android and Windows don't show it. Set it as the app suspends (see `OnSuspend`) and in `OnBackgroundRefreshAsync` to keep it current. |
| `CanGoBack.get -> bool` | Whether going back has somewhere to go: a popup or modal page to close, or a page in the back stack. |
| `CanOpenWindows.get -> bool` | Whether the app can open windows besides its main one (on the desktop). |
| `CreateTheme(XForma.ThemeMode mode, XForma.Color accentColor) -> XForma.Theme` | Builds the application's theme for a mode and accent color. Override it for a brand's colors, type scale, shapes or control styles; call `InvalidateTheme` when what it depends on changes. |
| `Current.get -> XForma.Application` |  |
| `Dispatcher.get -> XForma.Dispatcher` | Runs code on the UI thread. |
| `DragSlop = 10 -> float` | How far a touch or pen moves before it's a drag, in points. Less than `TapSlop`: past this, a drag gesture takes the pointer from a tap that would still accept it. |
| `Exit() -> void` | Ends the app, on Windows and Android. iOS and the browser don't let an app close itself, so there it does nothing: an app that has to end on them shows the user a page saying so. |
| `GetDragSlop(XForma.PointerDeviceType deviceType) -> float` | How far a pointer of this kind moves before it's a drag, in points. |
| `GetVersion() -> System.Version` |  |
| `GoBackAsync() -> System.Threading.Tasks.Task<bool>` | Goes back: closes the top popup, or the top modal page, or returns to the previous page. Completes with false if there was nowhere to go. |
| `HotReloaded -> System.EventHandler<XForma.HotReloadEventArgs>` | Raised after an edit while the app runs with hot reload, once the theme, the pages and the controls showing have been built again, for anything else the app built once (a cache). |
| `InvalidateTheme() -> void` | Builds the theme again (see `CreateTheme`), and gives it to every view if it changed. |
| `IsStarted.get -> bool` | Whether a platform has started the app. |
| `IsTouchPrimary.get -> bool` | Whether touch, rather than a mouse, is the platform's primary input. |
| `MainWindow.get -> XForma.Window` | The app's main window. |
| `MouseDragSlop = 6 -> float` | How far a mouse moves before it's a drag, in points: a mouse holds steadier than a finger. |
| `NavigateAsync(string path, XForma.PageTransition transition = null) -> System.Threading.Tasks.Task` | Shows the page for a path (see `OnConfigureRoutes`), keeping the current page in the back stack. Completes when the transition has. Throws when no route matches. |
| `NavigateAsync<TResult>(string path, XForma.PageTransition transition = null) -> System.Threading.Tasks.Task<TResult>` |  |
| `NavigationStack.get -> System.Collections.Generic.IReadOnlyList<string>` | The paths of the pages in the back stack and the page showing (modal pages too), oldest first. |
| `OnBackgroundRefreshAsync() -> System.Threading.Tasks.Task` | Called when the system wakes the app in the background (see `BackgroundRefreshInterval`): reload the app's data and update what shows outside it, such as `BadgeCount`, in about 30 seconds. The app may not have started (no page shows), as when the system launched it for this. |
| `OnConfigureRoutes(XForma.Routes routes) -> void` | Lists the app's pages by path, each with a function that makes the page from a path that matches (see `Routes`). Called once, as the app starts, before `OnStart`. |
| `OnLoadResources() -> void` |  |
| `OnResume() -> void` |  |
| `OnRouteNotFound(string path) -> string` | Chooses what to show for a path from outside the app that no route matches (an address, a link, a stack saved by an older version): the path to show instead, `/` unless overridden, or null to stay where the app is. Not called for the app's own navigation, which throws instead. |
| `OnStart() -> void` |  |
| `OnSuspend() -> void` | Save application state and stop any background activity. |
| `OnThemeChanged(XForma.Theme theme) -> void` | Called with the application's theme when it starts, and whenever the theme changes. |
| `OpenUri(System.Uri uri) -> void` | Opens the URI in the system's handler for it, such as the web browser. |
| `OpenWindowAsync(string path, string title = null, XForma.Size? size = null) -> System.Threading.Tasks.Task<XForma.Window>` | Opens a window showing the page for a path (see `OnConfigureRoutes`), in a frame of its own. Throws where the platform has one window (see `CanOpenWindows`). |
| `PreviewKeyDown -> System.EventHandler<XForma.KeyEventArgs>` | Raised for every key press before the focused view gets it. Set `Handled` to keep it from the view. |
| `PreviewPointerInput -> System.EventHandler<XForma.PointerEventArgs>` | Raised for every pointer event before the view under the pointer gets it (the event's `OriginalSource`). Set `Handled` to keep it from the view. |
| `RegisterFont(string familyName, string fileName) -> void` | Registers a font file bundled with the app (TrueType or OpenType) under a family name, for `FontFamily` and `FontFamily`. The file is next to the app on Windows, in its assets on Android, and in its bundle on iOS. |
| `ReportException(System.Exception exception) -> bool` | Reports an exception that nothing can deal with, as code at the top of an `async void` method or a callback should: it's written to `FrameworkLog` and raises `UnhandledException`. Returns whether a handler handled it. |
| `RequestAnimationFrame(System.Action<System.TimeSpan> callback) -> void` | Calls the callback once, on the UI thread, before the display's next frame, with the frame's time (as a browser's `requestAnimationFrame`). Request again from the callback to run every frame, for animations the compositor can't play, such as redrawing an owner-drawn view. |
| `RequestedThemeMode.get -> XForma.ThemeMode?` | The mode the app asks for, or null (the default) to follow the system. |
| `RequestedThemeMode.set -> void` | The mode the app asks for, or null (the default) to follow the system. |
| `ShowModalAsync(string path) -> System.Threading.Tasks.Task` | Shows the page for a path over the current one, sliding up. Completes when it's closed: by Back, or by the page (`CloseModalAsync`). |
| `ShowModalAsync<TResult>(string path) -> System.Threading.Tasks.Task<TResult>` |  |
| `StartPath.get -> string` | The path the app starts on when the platform gives none (a link, a stack restored after the system ended the app, the browser's address): `/` unless set. XForma navigates to it after `OnStart`; the app doesn't navigate to its first page itself. |
| `StartPath.set -> void` | The path the app starts on when the platform gives none (a link, a stack restored after the system ended the app, the browser's address): `/` unless set. XForma navigates to it after `OnStart`; the app doesn't navigate to its first page itself. |
| `SystemThemeMode.get -> XForma.ThemeMode` | The system's theme mode. |
| `TapSlop = 15 -> float` | How far a press can move and still be a tap (or a hold), in points. |
| `TextScale.get -> float` | The system's text size, as a scale (1 is the default). Every font size is scaled by it, on every platform, so text follows the user's setting; layouts that depend on text size should leave room for it. |
| `TextScaleChanged -> System.EventHandler` |  |
| `Theme.get -> XForma.Theme` | The application's theme: the one `CreateTheme` builds for the actual mode and accent color. Views inherit it unless they set their own `Theme`. |
| `ThemeChanged -> System.EventHandler` | Raised when the application's theme changes: its mode, accent color or design tokens. |
| `ThemeExtender.get -> System.Func<XForma.Theme, XForma.Theme>` | Adds to the theme `CreateTheme` builds, before an app's override adjusts it: set by what hosts the app, such as a control library adding its theme package's tokens and templates. |
| `ThemeExtender.set -> void` | Adds to the theme `CreateTheme` builds, before an app's override adjusts it: set by what hosts the app, such as a control library adding its theme package's tokens and templates. |
| `ThemePalette.get -> System.Func<XForma.ThemeMode, XForma.Color, XForma.ColorScheme>` | Makes the colors `CreateTheme` starts from, for a mode and accent color: set by what hosts the app, such as a theme package's palette. Null (the default) for `Plain`. |
| `ThemePalette.set -> void` | Makes the colors `CreateTheme` starts from, for a mode and accent color: set by what hosts the app, such as a theme package's palette. Null (the default) for `Plain`. |
| `TitleBar.get -> XForma.TitleBar` |  |
| `UnhandledException -> System.EventHandler<XForma.ApplicationExceptionEventArgs>` | Raised when nothing in the app caught an exception, for crash reporting. It's usually on the UI thread; an exception from another thread raises it there, and the app ends whatever the handler does. Setting `Handled` keeps the app running on Windows and Android. A web app keeps running either way, and iOS ends the app either way, after this event. |
| `Windows.get -> System.Collections.Generic.IReadOnlyList<XForma.Window>` | The app's windows: the main one, then those it opened. |

## ApplicationExceptionEventArgs

An exception that nothing in the app caught.

| Member | |
|---|---|
| `Exception.get -> System.Exception` |  |
| `Handled.get -> bool` | Keeps the app running where the platform allows it (see `UnhandledException`). |
| `Handled.set -> void` | Keeps the app running where the platform allows it (see `UnhandledException`). |

## Binding

Keeps a view property up to date with a value from a source, such as a view model, and (for a two-way binding) the source with the view.

| Member | |
|---|---|
| `Dispose() -> void` | Stops the binding: the view and source stop updating each other. |
| `Refresh() -> void` | Reads the source again and updates the view. |

## Bitmap

An image to draw or show, loaded from a file or from pixels.

| Member | |
|---|---|
| `Bitmap(XForma.ImageSource source) -> void` |  |
| `LoadAsync() -> System.Threading.Tasks.Task` |  |
| `Source.get -> XForma.ImageSource` |  |

## BitmapImageSource

An `ImageSource` showing a `Bitmap`.

| Member | |
|---|---|
| `Bitmap.get -> XForma.Bitmap` |  |
| `BitmapImageSource(XForma.Bitmap bitmap) -> void` |  |

## Brush

What fills a shape. Brushes are immutable, so they can be shared and cached.

## Color

A color, with 8 bits each of alpha, red, green and blue.

| Member | |
|---|---|
| `A.get -> byte` |  |
| `A.set -> void` |  |
| `B.get -> byte` |  |
| `B.set -> void` |  |
| `Color() -> void` |  |
| `Equals(object obj) -> bool` |  |
| `FromArgb(byte a, byte r, byte g, byte b) -> XForma.Color` |  |
| `FromArgb(float a, float r, float g, float b) -> XForma.Color` |  |
| `FromArgb(float r, float g, float b) -> XForma.Color` |  |
| `FromArgb(uint argb) -> XForma.Color` |  |
| `FromRgb(byte r, byte g, byte b) -> XForma.Color` |  |
| `FromRgb(uint rgb) -> XForma.Color` |  |
| `FromString(string colorString) -> XForma.Color` |  |
| `G.get -> byte` |  |
| `G.set -> void` |  |
| `GetHashCode() -> int` |  |
| `IsTransparent.get -> bool` |  |
| `MultiplyAlpha(float multiplier) -> XForma.Color` |  |
| `R.get -> byte` |  |
| `R.set -> void` |  |
| `ToArgb() -> uint` |  |
| `ToString() -> string` |  |
| `ToString(System.Globalization.CultureInfo cultureInfo) -> string` |  |
| `operator !=(XForma.Color a, XForma.Color b) -> bool` |  |
| `operator ==(XForma.Color a, XForma.Color b) -> bool` |  |

## ColorScheme

A theme's colors, by role. Most come in pairs: a color, and the color of text and icons drawn on it.

| Member | |
|---|---|
| `<Clone>$() -> XForma.ColorScheme` |  |
| `Accent.get -> XForma.Color` | The app's accent: selection, primary actions, focus. |
| `Accent.init -> void` | The app's accent: selection, primary actions, focus. |
| `Background.get -> XForma.Color` | A page's background. |
| `Background.init -> void` | A page's background. |
| `ColorScheme() -> void` |  |
| `Disabled.get -> XForma.Color` | Text and icons of disabled controls. |
| `Disabled.init -> void` | Text and icons of disabled controls. |
| `Equals(XForma.ColorScheme other) -> bool` |  |
| `Equals(object obj) -> bool` |  |
| `Error.get -> XForma.Color` | Errors and destructive actions. |
| `Error.init -> void` | Errors and destructive actions. |
| `Foreground.get -> XForma.Color` | Text and icons on the background. |
| `Foreground.init -> void` | Text and icons on the background. |
| `GetHashCode() -> int` |  |
| `OnAccent.get -> XForma.Color` | Text and icons on the accent color. |
| `OnAccent.init -> void` | Text and icons on the accent color. |
| `OnError.get -> XForma.Color` | Text and icons on the error color. |
| `OnError.init -> void` | Text and icons on the error color. |
| `OnSurface.get -> XForma.Color` | Text and icons on a surface. |
| `OnSurface.init -> void` | Text and icons on a surface. |
| `Outline.get -> XForma.Color` | Borders and dividers. |
| `Outline.init -> void` | Borders and dividers. |
| `Plain(XForma.ThemeMode mode, XForma.Color accentColor) -> XForma.ColorScheme` | Plain colors for a mode, for an app with no theme package: black and white and a gray, around the accent color. A theme package has its own palette (`AppTheme.CreateColors`). |
| `Scrim.get -> XForma.Color` | What dims the window behind a flyout or a drawer, with its opacity. |
| `Scrim.init -> void` | What dims the window behind a flyout or a drawer, with its opacity. |
| `SubtleForeground.get -> XForma.Color` | Secondary text and icons on the background. |
| `SubtleForeground.init -> void` | Secondary text and icons on the background. |
| `Surface.get -> XForma.Color` | Raised or set-apart areas: text boxes, fields, bars, popups. |
| `Surface.init -> void` | Raised or set-apart areas: text boxes, fields, bars, popups. |
| `ToString() -> string` |  |
| `operator !=(XForma.ColorScheme left, XForma.ColorScheme right) -> bool` |  |
| `operator ==(XForma.ColorScheme left, XForma.ColorScheme right) -> bool` |  |

## Colors

Named colors. In views, prefer the theme's roles, which work in every theme mode.

| Member | |
|---|---|
| `AliceBlue -> XForma.Color` |  |
| `AntiqueWhite -> XForma.Color` |  |
| `Aqua -> XForma.Color` |  |
| `Aquamarine -> XForma.Color` |  |
| `Azure -> XForma.Color` |  |
| `Beige -> XForma.Color` |  |
| `Bisque -> XForma.Color` |  |
| `Black -> XForma.Color` |  |
| `BlanchedAlmond -> XForma.Color` |  |
| `Blue -> XForma.Color` |  |
| `BlueViolet -> XForma.Color` |  |
| `Brown -> XForma.Color` |  |
| `BurlyWood -> XForma.Color` |  |
| `CadetBlue -> XForma.Color` |  |
| `Chartreuse -> XForma.Color` |  |
| `Chocolate -> XForma.Color` |  |
| `Coral -> XForma.Color` |  |
| `CornflowerBlue -> XForma.Color` |  |
| `Cornsilk -> XForma.Color` |  |
| `Crimson -> XForma.Color` |  |
| `Cyan -> XForma.Color` |  |
| `DarkBlue -> XForma.Color` |  |
| `DarkCyan -> XForma.Color` |  |
| `DarkGoldenrod -> XForma.Color` |  |
| `DarkGray -> XForma.Color` |  |
| `DarkGreen -> XForma.Color` |  |
| `DarkKhaki -> XForma.Color` |  |
| `DarkMagenta -> XForma.Color` |  |
| `DarkOliveGreen -> XForma.Color` |  |
| `DarkOrange -> XForma.Color` |  |
| `DarkOrchid -> XForma.Color` |  |
| `DarkRed -> XForma.Color` |  |
| `DarkSalmon -> XForma.Color` |  |
| `DarkSeaGreen -> XForma.Color` |  |
| `DarkSlateBlue -> XForma.Color` |  |
| `DarkSlateGray -> XForma.Color` |  |
| `DarkTurquoise -> XForma.Color` |  |
| `DarkViolet -> XForma.Color` |  |
| `DeepPink -> XForma.Color` |  |
| `DeepSkyBlue -> XForma.Color` |  |
| `DimGray -> XForma.Color` |  |
| `DodgerBlue -> XForma.Color` |  |
| `Firebrick -> XForma.Color` |  |
| `FloralWhite -> XForma.Color` |  |
| `ForestGreen -> XForma.Color` |  |
| `Fuchsia -> XForma.Color` |  |
| `Gainsboro -> XForma.Color` |  |
| `GhostWhite -> XForma.Color` |  |
| `Gold -> XForma.Color` |  |
| `Goldenrod -> XForma.Color` |  |
| `Gray -> XForma.Color` |  |
| `Green -> XForma.Color` |  |
| `GreenYellow -> XForma.Color` |  |
| `Honeydew -> XForma.Color` |  |
| `HotPink -> XForma.Color` |  |
| `IndianRed -> XForma.Color` |  |
| `Indigo -> XForma.Color` |  |
| `Ivory -> XForma.Color` |  |
| `Khaki -> XForma.Color` |  |
| `Lavender -> XForma.Color` |  |
| `LavenderBlush -> XForma.Color` |  |
| `LawnGreen -> XForma.Color` |  |
| `LemonChiffon -> XForma.Color` |  |
| `LightBlue -> XForma.Color` |  |
| `LightCoral -> XForma.Color` |  |
| `LightCyan -> XForma.Color` |  |
| `LightGoldenrodYellow -> XForma.Color` |  |
| `LightGray -> XForma.Color` |  |
| `LightGreen -> XForma.Color` |  |
| `LightPink -> XForma.Color` |  |
| `LightSalmon -> XForma.Color` |  |
| `LightSeaGreen -> XForma.Color` |  |
| `LightSkyBlue -> XForma.Color` |  |
| `LightSlateGray -> XForma.Color` |  |
| `LightSteelBlue -> XForma.Color` |  |
| `LightYellow -> XForma.Color` |  |
| `Lime -> XForma.Color` |  |
| `LimeGreen -> XForma.Color` |  |
| `Linen -> XForma.Color` |  |
| `Magenta -> XForma.Color` |  |
| `Maroon -> XForma.Color` |  |
| `MediumAquamarine -> XForma.Color` |  |
| `MediumBlue -> XForma.Color` |  |
| `MediumOrchid -> XForma.Color` |  |
| `MediumPurple -> XForma.Color` |  |
| `MediumSeaGreen -> XForma.Color` |  |
| `MediumSlateBlue -> XForma.Color` |  |
| `MediumSpringGreen -> XForma.Color` |  |
| `MediumTurquoise -> XForma.Color` |  |
| `MediumVioletRed -> XForma.Color` |  |
| `MidnightBlue -> XForma.Color` |  |
| `MintCream -> XForma.Color` |  |
| `MistyRose -> XForma.Color` |  |
| `Moccasin -> XForma.Color` |  |
| `NavajoWhite -> XForma.Color` |  |
| `Navy -> XForma.Color` |  |
| `OldLace -> XForma.Color` |  |
| `Olive -> XForma.Color` |  |
| `OliveDrab -> XForma.Color` |  |
| `Orange -> XForma.Color` |  |
| `OrangeRed -> XForma.Color` |  |
| `Orchid -> XForma.Color` |  |
| `PaleGoldenrod -> XForma.Color` |  |
| `PaleGreen -> XForma.Color` |  |
| `PaleTurquoise -> XForma.Color` |  |
| `PaleVioletRed -> XForma.Color` |  |
| `PapayaWhip -> XForma.Color` |  |
| `PeachPuff -> XForma.Color` |  |
| `Peru -> XForma.Color` |  |
| `Pink -> XForma.Color` |  |
| `Plum -> XForma.Color` |  |
| `PowderBlue -> XForma.Color` |  |
| `Purple -> XForma.Color` |  |
| `Red -> XForma.Color` |  |
| `RosyBrown -> XForma.Color` |  |
| `RoyalBlue -> XForma.Color` |  |
| `SaddleBrown -> XForma.Color` |  |
| `Salmon -> XForma.Color` |  |
| `SandyBrown -> XForma.Color` |  |
| `SeaGreen -> XForma.Color` |  |
| `SeaShell -> XForma.Color` |  |
| `Sienna -> XForma.Color` |  |
| `Silver -> XForma.Color` |  |
| `SkyBlue -> XForma.Color` |  |
| `SlateBlue -> XForma.Color` |  |
| `SlateGray -> XForma.Color` |  |
| `Snow -> XForma.Color` |  |
| `SpringGreen -> XForma.Color` |  |
| `SteelBlue -> XForma.Color` |  |
| `Tan -> XForma.Color` |  |
| `Teal -> XForma.Color` |  |
| `Thistle -> XForma.Color` |  |
| `Tomato -> XForma.Color` |  |
| `Transparent -> XForma.Color` |  |
| `TransparentBlack -> XForma.Color` |  |
| `TransparentWhite -> XForma.Color` |  |
| `Turquoise -> XForma.Color` |  |
| `Violet -> XForma.Color` |  |
| `Wheat -> XForma.Color` |  |
| `White -> XForma.Color` |  |
| `WhiteSmoke -> XForma.Color` |  |
| `Yellow -> XForma.Color` |  |
| `YellowGreen -> XForma.Color` |  |

## ContextRequestedEventArgs

The user asked for a view's context menu: with a right-click, a long press, or the Menu key or Shift+F10 on the focused view.

| Member | |
|---|---|
| `ContextRequestedEventArgs(XForma.Point? position, XForma.PointerDeviceType? deviceType) -> void` |  |
| `DeviceType.get -> XForma.PointerDeviceType?` | The pointer that asked (a mouse's right-click, a touch's or pen's long press), or null for the keyboard. |
| `Handled.get -> bool` | Set when the request was answered, so it goes no further. |
| `Handled.set -> void` | Set when the request was answered, so it goes no further. |
| `OriginalSource.get -> XForma.View` | The view the request started at, before going to its ancestors. |
| `Position.get -> XForma.Point?` | Where the pointer asked, in `OriginalSource`'s coordinates; null when the keyboard asked. |

## CubicEase

An easing that follows a cubic curve.

| Member | |
|---|---|
| `CubicEase(XForma.EasingMode easingMode) -> void` |  |
| `EaseInCore(float t) -> float` |  |

## DataPackage

What's being dragged. Text and a URI go to other apps too; `LocalData` stays in the app, and files only come in from other apps.

| Member | |
|---|---|
| `DataPackage() -> void` |  |
| `Files.get -> System.Collections.Generic.IReadOnlyList<XForma.DroppedFile>` | The files dropped from another app. Empty while the drag is still moving on platforms that show them only at the drop. |
| `Files.set -> void` | The files dropped from another app. Empty while the drag is still moving on platforms that show them only at the drop. |
| `LocalData.get -> object` | Any object, for a drop in the same app. |
| `LocalData.set -> void` | Any object, for a drop in the same app. |
| `Text.get -> string` |  |
| `Text.set -> void` |  |
| `Uri.get -> System.Uri` |  |
| `Uri.set -> void` |  |

## Dimension

Values for a view's size, such as `Auto` to fit its content.

| Member | |
|---|---|
| `Auto -> XForma.Dimension` |  |
| `Dimension() -> void` |  |
| `Dimension(float value) -> void` |  |
| `Equals(object obj) -> bool` |  |
| `GetHashCode() -> int` |  |
| `Value -> float` |  |
| `implicit operator XForma.Dimension(float value) -> XForma.Dimension` |  |
| `implicit operator float(XForma.Dimension dimension) -> float` |  |
| `operator !=(XForma.Dimension a, XForma.Dimension b) -> bool` |  |
| `operator ==(XForma.Dimension a, XForma.Dimension b) -> bool` |  |

## Dispatcher

Runs code on the UI thread: one queue, with no priorities. From the UI thread, `await` already returns to it.

| Member | |
|---|---|
| `CheckAccess() -> bool` | Whether the calling code is on the UI thread. |
| `InvokeAsync(System.Action action) -> System.Threading.Tasks.Task` | Runs the action on the UI thread, and completes when it has (with its exception, if it throws). |
| `InvokeAsync(System.Func<System.Threading.Tasks.Task> function) -> System.Threading.Tasks.Task` | Runs the action on the UI thread, and completes when it has (with its exception, if it throws). |
| `InvokeAsync<TResult>(System.Func<TResult> function) -> System.Threading.Tasks.Task<TResult>` |  |
| `Post(System.Action action) -> void` | Queues the action to run on the UI thread. |

## DispatcherTimer

Raises `Tick` on the UI thread after each `Interval`, until stopped.

| Member | |
|---|---|
| `DispatcherTimer() -> void` |  |
| `DispatcherTimer(System.TimeSpan interval, System.Action action) -> void` |  |
| `Interval.get -> System.TimeSpan` |  |
| `Interval.set -> void` |  |
| `IsEnabled.get -> bool` |  |
| `Start() -> void` | Starts the timer, or restarts it from now if it's running. |
| `Stop() -> void` |  |
| `Tick -> System.EventHandler` |  |

## DragEventArgs

A drag over a view that allows drops. A handler of `DragEnter` or `DragOver` sets `AcceptedOperation` to take the drop.

| Member | |
|---|---|
| `AcceptedOperation.get -> XForma.DragOperation` |  |
| `AcceptedOperation.set -> void` |  |
| `Data.get -> XForma.DataPackage` |  |
| `IsFromThisApp.get -> bool` | Whether the drag started in this app. |
| `Position.get -> XForma.Point` | Where the drag is, in the view's coordinates. |

## DragOperation

What a drop does with the data.

| Member | |
|---|---|
| `Copy = 1 -> XForma.DragOperation` |  |
| `Move = 2 -> XForma.DragOperation` |  |
| `None = 0 -> XForma.DragOperation` |  |

## DragStartingEventArgs

A drag is starting from a view: fill in `Data`, or set `Cancel`.

| Member | |
|---|---|
| `Cancel.get -> bool` |  |
| `Cancel.set -> void` |  |
| `Data.get -> XForma.DataPackage` |  |
| `DragStartingEventArgs() -> void` |  |

## DrawingContext

Draws an owner-drawn view's content. Each platform draws with its native 2D library: Direct2D, Core Graphics or Android's Canvas.

| Member | |
|---|---|
| `Clear(XForma.Color color) -> void` | Fills the view's area with the color. Transparent clears it. |
| `ClearCore(XForma.Color color) -> void` |  |
| `DrawArc(XForma.Point center, float radius, float startDegrees, float sweepDegrees, XForma.Color color, float strokeWidth = 1) -> void` | Draws part of a circle with round ends, clockwise from the start angle (in degrees, with 0 at the top). |
| `DrawEllipse(XForma.Point point, float radiusX, float radiusY, XForma.Color color, float strokeWidth = 1) -> void` |  |
| `DrawGeometry(XForma.PathGeometry geometry, XForma.Color color, float strokeWidth = 1) -> void` | Strokes a path, with round joins and caps. |
| `DrawImage(XForma.Bitmap bitmap, XForma.Rect destinationRectangle, XForma.Color tint) -> void` | Draws a bitmap, optionally tinted; a null bitmap draws nothing. |
| `DrawLine(XForma.Point start, XForma.Point end, XForma.Color color, float strokeWidth = 1) -> void` | Draws a straight line with round ends. |
| `DrawRectangle(XForma.Rect rectangle, XForma.Color color, float strokeWidth = 1) -> void` |  |
| `DrawRoundedRectangle(XForma.Rect rectangle, float cornerRadius, XForma.Color color, float strokeWidth = 1) -> void` | Strokes a rectangle with corners rounded to the radius. |
| `DrawText(XForma.TextLayout layout, XForma.Point origin, XForma.Color color) -> void` | Draws laid-out text with its top left corner at the origin. |
| `DrawText(string text, XForma.Rect bounds, XForma.Color color, XForma.TextFormat textFormat) -> void` | Draws laid-out text with its top left corner at the origin. |
| `DrawingContext() -> void` |  |
| `FillEllipse(XForma.Point point, float radiusX, float radiusY, XForma.Color color) -> void` |  |
| `FillEllipse(XForma.Rect rectangle, XForma.Brush brush) -> void` |  |
| `FillEllipse(XForma.Rect rectangle, XForma.Color color) -> void` |  |
| `FillGeometry(XForma.PathGeometry geometry, XForma.Brush brush) -> void` | Fills the inside of a path (nonzero winding). |
| `FillRectangle(XForma.Rect rectangle, XForma.Brush brush) -> void` | Fills a rectangle with a brush (a solid color or a gradient). |
| `FillRectangle(XForma.Rect rectangle, XForma.Color color) -> void` | Fills a rectangle with a brush (a solid color or a gradient). |
| `FillRoundedRectangle(XForma.Rect rectangle, float cornerRadius, XForma.Brush brush) -> void` | Fills a rectangle with corners rounded to the radius. |
| `FillRoundedRectangle(XForma.Rect rectangle, float cornerRadius, XForma.Color color) -> void` | Fills a rectangle with corners rounded to the radius. |
| `FillShadow(XForma.Rect rectangle, XForma.Shadow shadow) -> void` | Draws the shadow a shape in the rectangle casts (not the shape): a view's `Shadow`, or one an owner-drawn control draws beneath what it draws. |
| `Pop() -> void` | Undoes the most recent push. |
| `PushClip(XForma.Rect clip) -> XForma.DrawingContextState` | Limits drawing to the rectangle until `Pop`. |
| `PushClipCore(XForma.Rect clip) -> void` |  |
| `PushOpacity(float opacity) -> XForma.DrawingContextState` | Draws everything until `Pop` at the opacity, as a group. |
| `PushOpacityCore(float opacity) -> void` |  |
| `PushTransform(System.Numerics.Matrix3x2 transform) -> XForma.DrawingContextState` | Transforms drawing (on top of any transform already pushed) until `Pop`. |
| `PushTransformCore(System.Numerics.Matrix3x2 transform) -> void` |  |

## DrawingContextState

Undoes a `Push` on a `DrawingContext` when disposed, for use with `using`.

| Member | |
|---|---|
| `Dispose() -> void` |  |
| `DrawingContextState() -> void` |  |

## DrawnAnimator

A scale and opacity that an owner-drawn control applies to its own drawing, animated frame by frame on the UI thread. Unlike an `Animator`, it needs no native layer, so the control stays lightweight. Use it for small effects such as press feedback; create one with `CreateDrawnAnimator`.

| Member | |
|---|---|
| `FadeTo(float opacity, System.TimeSpan duration, XForma.EasingFunction easing = null) -> XForma.AnimationHandle` |  |
| `Opacity.get -> float` | The opacity of the control's drawing. Setting it stops its animation. |
| `Opacity.set -> void` | The opacity of the control's drawing. Setting it stops its animation. |
| `Scale.get -> float` | The scale, around the center of the control. Setting it stops its animation. |
| `Scale.set -> void` | The scale, around the center of the control. Setting it stops its animation. |
| `ScaleTo(float scale, System.TimeSpan duration, XForma.EasingFunction easing = null) -> XForma.AnimationHandle` |  |
| `Stop() -> void` | Stops both animations where they are. |

## DropCompletedEventArgs

A drag from a view has ended.

| Member | |
|---|---|
| `Operation.get -> XForma.DragOperation` | What the drop did, or `None` if nothing took it: the source removes the data after a `Move`. |

## DroppedFile

A file dropped on the app from another one.

| Member | |
|---|---|
| `DroppedFile() -> void` |  |
| `Name.get -> string` |  |
| `OpenReadAsync() -> System.Threading.Tasks.Task<System.IO.Stream>` | Opens the file's content. Read it during or soon after the drop: the system can take the app's access back. |

## EasingFunction

The shape of an animation's progress over time. Platforms sample it to play the animation on their compositor.

| Member | |
|---|---|
| `Ease(float t) -> float` | The progress (0 at the start, 1 at the end, possibly overshooting) at a normalized time from 0 to 1. |
| `EaseInCore(float t) -> float` | The ease-in curve, from (0, 0) to (1, 1). |
| `EasingFunction(XForma.EasingMode easingMode) -> void` |  |
| `EasingMode.get -> XForma.EasingMode` |  |
| `Linear -> XForma.EasingFunction` | Constant speed. |

## EasingMode

Which end of an animation an easing applies to.

| Member | |
|---|---|
| `EaseIn = 0 -> XForma.EasingMode` |  |
| `EaseInOut = 2 -> XForma.EasingMode` |  |
| `EaseOut = 1 -> XForma.EasingMode` |  |

## Element

The base of everything in the view tree: a theme, attached properties and a platform renderer.

| Member | |
|---|---|
| `ActualTheme.get -> XForma.Theme` | The element's effective theme: its own (a view's `Theme`), or else its parent's, or else the application's. |
| `Application.get -> XForma.Application` |  |
| `Element() -> void` |  |
| `HasDescendant(XForma.Element element) -> bool` | Whether the element is this element or one of its descendants. |
| `OnAttaching(XForma.Theme theme) -> void` | Called once, with the element's effective theme, as it's first attached (or shown): every constructor has run, and its native object doesn't exist yet. The last chance to set up what must come before it, such as a view's own animators (`CreateOwnAnimator`) that depend on its theme. |
| `OnThemeChanged(XForma.Theme theme) -> void` | Called with the element's effective theme once it's attached (every constructor has run, and its native object exists), and whenever that theme changes. Never called from a constructor. |

## FileImageSource

An `ImageSource` showing an image file.

| Member | |
|---|---|
| `FileImageSource(string path) -> void` |  |
| `Path.get -> string` |  |

## FlowDirection

The direction a view lays out its children in.

| Member | |
|---|---|
| `LeftToRight = 0 -> XForma.FlowDirection` |  |
| `RightToLeft = 1 -> XForma.FlowDirection` |  |

## FontFamilies

Generic font families, as in CSS: each platform uses a matching system font, so text looks native everywhere without bundling a font.

| Member | |
|---|---|
| `Monospace = "monospace" -> string` | A fixed-width font (Consolas on Windows, Droid Sans Mono on Android, SF Mono on iOS). |
| `SansSerif = "sans-serif" -> string` | The platform's UI font (Segoe UI, Roboto, San Francisco). The default. |
| `Serif = "serif" -> string` | A serif font (Cambria on Windows, Noto Serif on Android, New York on iOS). |

## FontStyle

Whether text is upright or italic.

| Member | |
|---|---|
| `Italic = 1 -> XForma.FontStyle` |  |
| `Normal = 0 -> XForma.FontStyle` |  |

## FontWeight

How heavy text is.

| Member | |
|---|---|
| `Bold = 700 -> XForma.FontWeight` |  |
| `Light = 300 -> XForma.FontWeight` |  |
| `Medium = 500 -> XForma.FontWeight` |  |
| `Normal = 400 -> XForma.FontWeight` |  |
| `SemiBold = 600 -> XForma.FontWeight` |  |

## Frame

Shows one page at a time, with a back stack. Navigation is asynchronous: each method completes when its transition has.

| Member | |
|---|---|
| `Background.get -> XForma.Color?` | The frame's background, behind its pages: where a page isn't (as it slides, or is pulled past the end), what's behind the frame shows through a transparent one. Null (the default) for the platform's (the system background on iOS; none elsewhere). |
| `Background.set -> void` | The frame's background, behind its pages: where a page isn't (as it slides, or is pulled past the end), what's behind the frame shows through a transparent one. Null (the default) for the platform's (the system background on iOS; none elsewhere). |
| `BeginBounce() -> XForma.PageBounce` | Starts pulling the current page sideways as a gesture moves where there's no page to switch to (past the first or last tab): it moves as far as the bounce's `Offset`, with its edge shadow, and springs back when released. Returns null if there's no current page, or a switch or another bounce is under way. |
| `BeginReplace(XForma.Page page, XForma.NavigationMode mode = XForma.NavigationMode.Forward) -> XForma.PageSwitch` | Starts showing the page in place of the current one (as `ReplaceAsync` does), following a gesture: it slides in as far as the switch's `Progress`, then completes or goes back. Returns null if there's no current page, it's the page, the current page cancels, or a switch is under way. |
| `CanGoBack.get -> bool` |  |
| `CurrentPage.get -> XForma.Page` | The page showing, or null. |
| `Frame() -> void` |  |
| `GoBackAsync(XForma.PageTransition transition = null) -> System.Threading.Tasks.Task<bool>` | Goes back to the previous page. Completes with false if there's none, or the current page cancels. |
| `NavigateAsync(XForma.Page page, XForma.PageTransition transition = null) -> System.Threading.Tasks.Task` | Shows the page, keeping the current one in the back stack. Completes when the transition (by default, the theme's `PageTransition`) has, or at once if the current page cancels. |
| `OnHotReload() -> void` |  |
| `Push(XForma.Page page) -> void` | Adds a page to the frame without showing it, so it's ready to show at once (as tabs are). |
| `ReplaceAsync(XForma.Page page, XForma.NavigationMode mode = XForma.NavigationMode.Forward, XForma.PageTransition transition = null) -> System.Threading.Tasks.Task` | Shows the page in place of the current one, without the back stack (as tabs switch). The mode sets the direction it slides from. |
| `TransitionDuration.get -> System.TimeSpan` | How long a page takes to arrive or leave (see `PageTransition`). |
| `TransitionDuration.set -> void` | How long a page takes to arrive or leave (see `PageTransition`). |

## FrameworkLog

What the framework has to say about a running app, for the app's own logging. Without a listener, warnings and errors go to the console.

| Member | |
|---|---|
| `MessageLogged -> System.EventHandler<XForma.LogEventArgs>` | Raised for each message, on the thread that wrote it. |
| `Write(XForma.LogLevel level, string message, System.Exception exception = null) -> void` | Writes a message to the framework's log. Control libraries and platforms use it; it never throws. |

## GestureRecognizer

Interprets the pointer input of the view it's added to. A press makes it `Possible`; it then calls `Recognize` or `Fail`, and is `Ready` again once the pointer is released or canceled.

| Member | |
|---|---|
| `Fail() -> void` | The pointer isn't the gesture: the recognizer gets no more of its input, leaving it to others. |
| `GestureRecognizer() -> void` |  |
| `GestureRecognizer(XForma.IGestureRecognizerDelegate gestureRecognizerDelegate) -> void` |  |
| `IsDragGesture.get -> bool` | Whether the gesture follows drags (a swipe, a pan). Only these are offered a drag that starts on a view inside theirs (as a sideways drag on a row's button swipes the row), and they see each event before the view's other recognizers, so a quick flick that lifts before a move arrives is theirs, not a tap. |
| `OnAttached(XForma.View view) -> void` | Called when the recognizer becomes a view's `GestureRecognizers`. |
| `OnDetached(XForma.View view) -> void` | Called when the recognizer stops being a view's `GestureRecognizers`. |
| `OnPointerInput(XForma.PointerEventArgs e) -> void` | A pointer event for the gesture, while it's `Possible` or `Recognized`. |
| `Recognize() -> void` | The pointer is the gesture: the view's other recognizers stop following it, and the recognizer claims its moves. Does nothing unless the state is `Possible`. |
| `State.get -> XForma.GestureState` |  |
| `TracksMultiplePointers.get -> bool` | Whether the recognizer gets every pointer pressed on its view (as a pinch does). Otherwise it follows only the first one, until released. |
| `View.get -> XForma.View` | The view the recognizer is attached to, or null. |

## GestureState

Where a gesture recognizer is in following the pointer on its view.

| Member | |
|---|---|
| `Failed = 3 -> XForma.GestureState` | The pointer isn't the gesture: the recognizer ignores it until it's released. |
| `Possible = 1 -> XForma.GestureState` | A pointer is down, and may yet turn out to be the gesture. |
| `Ready = 0 -> XForma.GestureState` | No pointer is down: the recognizer waits for a press. |
| `Recognized = 2 -> XForma.GestureState` | The pointer is the gesture. The recognizer claims its moves, and the view's other recognizers have stopped following it. |

## GradientStop

A color at a position along a gradient.

| Member | |
|---|---|
| `Color.get -> XForma.Color` |  |
| `GradientStop() -> void` |  |
| `GradientStop(XForma.Color color, float offset) -> void` |  |
| `Offset.get -> float` | Where the color is along the gradient, from 0 to 1. |

## HorizontalAlignment

Where a view sits horizontally in the space its parent gives it.

| Member | |
|---|---|
| `Center = 1 -> XForma.HorizontalAlignment` |  |
| `Left = 0 -> XForma.HorizontalAlignment` |  |
| `Right = 2 -> XForma.HorizontalAlignment` |  |
| `Stretch = 3 -> XForma.HorizontalAlignment` |  |

## HotReloadEventArgs

Data for `HotReloaded`.

| Member | |
|---|---|
| `HotReloadEventArgs(System.Collections.Generic.IReadOnlyList<System.Type> updatedTypes) -> void` |  |
| `UpdatedTypes.get -> System.Collections.Generic.IReadOnlyList<System.Type>` | The types the edit changed (empty when .NET doesn't say). |

## IAnimationLayer

One animator's native layer: a platform's compositor layer, or a view whose render properties it owns. Platforms play animations on it.

| Member | |
|---|---|
| `Animate(XForma.AnimationChannel channel, float from, float to, XForma.AnimationTiming timing) -> void` | Animates the channel from one value to another. |
| `IsClipEnabled.set -> void` | Whether the clip channels limit what the layer shows. |
| `SetValue(XForma.AnimationChannel channel, float value) -> void` | Sets the channel's value now, ending any animation of it. |
| `TransformOrigin.set -> void` | The point that scale and rotation are around, relative to the layer's size: (0.5, 0.5) is the center. |

## IGestureRecognizerDelegate

What a control implements to hear when a touch begins and ends on its gesture recognizer.

| Member | |
|---|---|
| `OnTouchBegan() -> void` |  |
| `OnTouchCanceled() -> void` |  |
| `OnTouchEnded() -> void` |  |

## ImageProperties

The bindable properties of `Image`, such as `Image.SourceProperty`, for `ViewBindingExtensions`.

| Member | |
|---|---|
| `extension(XForma.Primitives.Image)` |  |
| `extension(XForma.Primitives.Image).SourceProperty.get -> XForma.ViewProperty<XForma.Primitives.Image, XForma.ImageSource>` |  |
| `extension(XForma.Primitives.Image).TintProperty.get -> XForma.ViewProperty<XForma.Primitives.Image, XForma.Color>` |  |
| `get_SourceProperty() -> XForma.ViewProperty<XForma.Primitives.Image, XForma.ImageSource>` |  |
| `get_TintProperty() -> XForma.ViewProperty<XForma.Primitives.Image, XForma.Color>` |  |

## ImageSource

Where an `Image` gets its picture.

| Member | |
|---|---|
| `implicit operator XForma.ImageSource(XForma.Bitmap source) -> XForma.ImageSource` |  |
| `implicit operator XForma.ImageSource(string source) -> XForma.ImageSource` |  |

## Key

Keys that views can handle. Text typed into text boxes goes to the native editor and isn't reported as keys.

| Member | |
|---|---|
| `A = 28 -> XForma.Key` |  |
| `B = 29 -> XForma.Key` |  |
| `Back = 14 -> XForma.Key` |  |
| `C = 30 -> XForma.Key` |  |
| `D = 31 -> XForma.Key` |  |
| `D0 = 54 -> XForma.Key` |  |
| `D1 = 55 -> XForma.Key` |  |
| `D2 = 56 -> XForma.Key` |  |
| `D3 = 57 -> XForma.Key` |  |
| `D4 = 58 -> XForma.Key` |  |
| `D5 = 59 -> XForma.Key` |  |
| `D6 = 60 -> XForma.Key` |  |
| `D7 = 61 -> XForma.Key` |  |
| `D8 = 62 -> XForma.Key` |  |
| `D9 = 63 -> XForma.Key` |  |
| `Delete = 15 -> XForma.Key` |  |
| `Down = 8 -> XForma.Key` |  |
| `E = 32 -> XForma.Key` |  |
| `End = 10 -> XForma.Key` |  |
| `Enter = 2 -> XForma.Key` |  |
| `Escape = 4 -> XForma.Key` |  |
| `F = 33 -> XForma.Key` |  |
| `F1 = 17 -> XForma.Key` |  |
| `F10 = 25 -> XForma.Key` |  |
| `F11 = 26 -> XForma.Key` |  |
| `F12 = 27 -> XForma.Key` |  |
| `F2 = 18 -> XForma.Key` |  |
| `F3 = 19 -> XForma.Key` |  |
| `F4 = 13 -> XForma.Key` |  |
| `F5 = 20 -> XForma.Key` |  |
| `F6 = 21 -> XForma.Key` |  |
| `F7 = 22 -> XForma.Key` |  |
| `F8 = 23 -> XForma.Key` |  |
| `F9 = 24 -> XForma.Key` |  |
| `G = 34 -> XForma.Key` |  |
| `H = 35 -> XForma.Key` |  |
| `Home = 9 -> XForma.Key` |  |
| `I = 36 -> XForma.Key` |  |
| `Insert = 16 -> XForma.Key` |  |
| `J = 37 -> XForma.Key` |  |
| `K = 38 -> XForma.Key` |  |
| `L = 39 -> XForma.Key` |  |
| `Left = 5 -> XForma.Key` |  |
| `M = 40 -> XForma.Key` |  |
| `Menu = 64 -> XForma.Key` | The Menu (Application) key, which asks for the focused view's context menu. |
| `N = 41 -> XForma.Key` |  |
| `None = 0 -> XForma.Key` |  |
| `O = 42 -> XForma.Key` |  |
| `P = 43 -> XForma.Key` |  |
| `PageDown = 12 -> XForma.Key` |  |
| `PageUp = 11 -> XForma.Key` |  |
| `Q = 44 -> XForma.Key` |  |
| `R = 45 -> XForma.Key` |  |
| `Right = 7 -> XForma.Key` |  |
| `S = 46 -> XForma.Key` |  |
| `Space = 3 -> XForma.Key` |  |
| `T = 47 -> XForma.Key` |  |
| `Tab = 1 -> XForma.Key` |  |
| `U = 48 -> XForma.Key` |  |
| `Up = 6 -> XForma.Key` |  |
| `V = 49 -> XForma.Key` |  |
| `W = 50 -> XForma.Key` |  |
| `X = 51 -> XForma.Key` |  |
| `Y = 52 -> XForma.Key` |  |
| `Z = 53 -> XForma.Key` |  |

## KeyEventArgs

A key pressed or released while a view (or one of its descendants) has keyboard focus.

| Member | |
|---|---|
| `Handled.get -> bool` | Set to true to stop the key going to the view's ancestors. |
| `Handled.set -> void` | Set to true to stop the key going to the view's ancestors. |
| `IsAltDown.get -> bool` |  |
| `IsShiftDown.get -> bool` |  |
| `Key.get -> XForma.Key` |  |
| `KeyEventArgs(XForma.Key key, XForma.KeyModifiers keyModifiers) -> void` |  |
| `KeyEventArgs(XForma.Key key, bool isShiftDown, bool isAltDown) -> void` |  |
| `KeyModifiers.get -> XForma.KeyModifiers` | The modifier keys held down. |
| `OriginalSource.get -> XForma.View` | The view with keyboard focus when the key was pressed. |

## KeyModifiers

The modifier keys held during input.

| Member | |
|---|---|
| `Alt = 4 -> XForma.KeyModifiers` |  |
| `Control = 2 -> XForma.KeyModifiers` |  |
| `Meta = 8 -> XForma.KeyModifiers` |  |
| `None = 0 -> XForma.KeyModifiers` |  |
| `Shift = 1 -> XForma.KeyModifiers` |  |

## LinearGradientBrush

A gradient along a line. The start and end points are relative to the bounds of the shape filled: (0, 0) is its top left corner, (1, 1) its bottom right.

| Member | |
|---|---|
| `EndPoint.get -> XForma.Point` |  |
| `GetAbsolutePoints(XForma.Rect bounds, out XForma.Point start, out XForma.Point end) -> void` | The start and end points within the bounds, in the bounds' coordinates. |
| `LinearGradientBrush(XForma.Point startPoint, XForma.Point endPoint, params XForma.GradientStop[] stops) -> void` |  |
| `StartPoint.get -> XForma.Point` |  |
| `Stops.get -> System.Collections.Generic.IReadOnlyList<XForma.GradientStop>` |  |
| `Vertical(XForma.Color top, XForma.Color bottom) -> XForma.LinearGradientBrush` | A gradient from the first color at the top to the second at the bottom. |

## LogEventArgs

A message from the framework.

| Member | |
|---|---|
| `Exception.get -> System.Exception` | The exception behind the message, or null. |
| `Level.get -> XForma.LogLevel` |  |
| `Message.get -> string` |  |

## LogLevel

How much a message from the framework matters.

| Member | |
|---|---|
| `Error = 2 -> XForma.LogLevel` | An exception reached the framework. |
| `Information = 0 -> XForma.LogLevel` | Worth knowing when looking into something. |
| `Warning = 1 -> XForma.LogLevel` | Something failed, and the app carries on without it, such as an image that didn't load. |

## NavigationEventArgs

A page being navigated to or from.

| Member | |
|---|---|
| `NavigationEventArgs(XForma.NavigationMode navigationMode) -> void` |  |
| `NavigationMode.get -> XForma.NavigationMode` | Whether the navigation went forward to a page or back to it. |

## NavigationMode

Whether navigation goes forward, back or replaces the page.

| Member | |
|---|---|
| `Back = 1 -> XForma.NavigationMode` |  |
| `Forward = 0 -> XForma.NavigationMode` |  |

## Orientation

The direction a panel stacks or wraps its children in, or a list lays its items out in.

| Member | |
|---|---|
| `Horizontal = 0 -> XForma.Orientation` |  |
| `Vertical = 1 -> XForma.Orientation` |  |

## Origin

A point on a view that a transform works around.

| Member | |
|---|---|
| `BottomCenter -> XForma.Origin` |  |
| `BottomLeft -> XForma.Origin` |  |
| `BottomRight -> XForma.Origin` |  |
| `Center -> XForma.Origin` |  |
| `Equals(object obj) -> bool` |  |
| `GetHashCode() -> int` |  |
| `MiddleLeft -> XForma.Origin` |  |
| `MiddleRight -> XForma.Origin` |  |
| `Origin() -> void` |  |
| `Origin(float x, float y) -> void` |  |
| `TopCenter -> XForma.Origin` |  |
| `TopLeft -> XForma.Origin` |  |
| `TopRight -> XForma.Origin` |  |
| `X.get -> float` |  |
| `Y.get -> float` |  |
| `operator !=(XForma.Origin a, XForma.Origin b) -> bool` |  |
| `operator ==(XForma.Origin a, XForma.Origin b) -> bool` |  |

## Page

A screen of the app. Override `Build` to return its views.

| Member | |
|---|---|
| `BackgroundColor.get -> XForma.Color` | The page's background: the theme's background color unless set. Pages are opaque, so they can overlap as they slide in and out. |
| `BackgroundColor.set -> void` | The page's background: the theme's background color unless set. Pages are opaque, so they can overlap as they slide in and out. |
| `BottomSafeAreaColor.get -> XForma.Color?` |  |
| `BottomSafeAreaColor.set -> void` |  |
| `Build() -> XForma.View` | Builds the page's views, and returns its content. XForma calls it once, as the page is first shown (before its native object exists), and again after an edit while the app runs with hot reload. |
| `CloseModalAsync() -> System.Threading.Tasks.Task` | Closes the page, when it's shown with `ShowModalAsync`. |
| `Content.get -> XForma.View` | The page's content, which fills it: what `Build` returns. |
| `Content.set -> void` | The page's content, which fills it: what `Build` returns. |
| `GetBackgroundColor() -> XForma.Color` |  |
| `IsBuilt.get -> bool` | Whether `Build` has run: the page's views exist. |
| `NavigationPath.get -> string` | The path the page was made from (see `Routes`), such as `/item/42`; null for a page made otherwise (one shown in a `Frame` inside a page). |
| `OnHotReload() -> void` |  |
| `OnNavigatedFrom(XForma.NavigationEventArgs navigationEventArgs) -> void` |  |
| `OnNavigatedTo(XForma.NavigationEventArgs navigationEventArgs) -> void` |  |
| `OnNavigatingFrom(XForma.NavigationEventArgs navigationEventArgs) -> bool` |  |
| `Page() -> void` |  |
| `SetResult(object result) -> void` | Sets what the page gives back when it goes (by Back, or closing), for `NavigateAsync``1` and `ShowModalAsync``1`. |
| `UsesSafeArea.get -> bool` | Whether the platform keeps the page clear of the system bars, display cutout and soft keyboard (the default). Set it to false to draw under them (a full-bleed image, say) and lay content out around `SafeAreaInsets` yourself. |
| `UsesSafeArea.set -> void` | Whether the platform keeps the page clear of the system bars, display cutout and soft keyboard (the default). Set it to false to draw under them (a full-bleed image, say) and lay content out around `SafeAreaInsets` yourself. |

## PageBounce

A frame's current page pulled sideways by a gesture with nowhere to go (see `BeginBounce`), such as past the last tab: it moves by the `Offset`, then springs back.

| Member | |
|---|---|
| `Offset.get -> float` | How far the page is pulled, in points: to the right for a positive offset. The gesture damps it, so the page resists the further it's pulled. |
| `Offset.set -> void` | How far the page is pulled, in points: to the right for a positive offset. The gesture damps it, so the page resists the further it's pulled. |
| `ReleaseAsync() -> System.Threading.Tasks.Task` | Springs the page back into place, in the frame's `TransitionDuration`. |

## PageSwitch

A page replacing a frame's current one as a gesture moves (see `BeginReplace`): it slides in as far as the `Progress`, then completes, or slides back out. The current page slides out with it, unless the frame's theme has `SlideOver`.

| Member | |
|---|---|
| `CancelAsync() -> System.Threading.Tasks.Task` | Slides back out; the current page stays. |
| `CompleteAsync() -> System.Threading.Tasks.Task` | Slides the rest of the way in, in the share of the frame's `TransitionDuration` left. The page is the frame's current one (and hears it's navigated to) as it starts. |
| `Page.get -> XForma.Page` | The page arriving. |
| `Progress.get -> float` | How far the page has slid in: 0 (none of it shows) to 1 (all of it). |
| `Progress.set -> void` | How far the page has slid in: 0 (none of it shows) to 1 (all of it). |

## PageTransition

How a page arrives or leaves a `Frame`: the built-in slides and fade, or any other a control library or app defines by overriding `PlayAsync`.

| Member | |
|---|---|
| `Fade.get -> XForma.PageTransition` | Fades in, and out. |
| `None.get -> XForma.PageTransition` | Shows the page at once. |
| `PageTransition() -> void` |  |
| `PlayAsync(XForma.PageTransitionContext context) -> System.Threading.Tasks.Task` | Moves the page on top in (as a page arrives) or out (as it leaves, uncovering the one before), and the page beneath if the transition moves it too. The frame hides the page leaving when the task completes. It's left where the transition ends: the frame hides it, then resets its animator (translation, scale, rotation and opacity), so it doesn't show again in between. The page staying must end in place. |
| `Slide.get -> XForma.PageTransition` | Slides in from the side as the page beneath slides out the other way, the two moving together, and back (the default). |
| `SlideOver.get -> XForma.PageTransition` | Slides in from the side over the page beneath, which stays where it is, and back out. |
| `SlideUp.get -> XForma.PageTransition` | Slides up from the bottom, and back down (as modal pages do). |

## PageTransitionContext

What a `PageTransition` moves, and how far and long.

| Member | |
|---|---|
| `Animator.get -> XForma.Animator` | The page's animator. |
| `AnimatorBeneath.get -> XForma.Animator` | The animator of the page beneath. |
| `Duration.get -> System.TimeSpan` | How long the transition takes: the frame's `TransitionDuration`. |
| `FromStart.get -> bool` | Whether the page arrives from the start side (as when tabs switch back), rather than the end. |
| `IsLeaving.get -> bool` | Whether the page is leaving, uncovering the page before it, rather than arriving over the current one. |
| `Page.get -> XForma.Page` | The page on top: the one arriving, or (going back) the one leaving. |
| `PageBeneath.get -> XForma.Page` | The page beneath it: the one it's covering, or (going back) the one it uncovers. |
| `Size.get -> XForma.Size` | The frame's size. |

## PathGeometry

A shape made of lines and curves, built with `MoveTo`, `LineTo`, curves and `Close`, then filled or stroked by a `DrawingContext`.

| Member | |
|---|---|
| `ArcTo(XForma.Point point, float radiusX, float radiusY, bool isLargeArc, bool isClockwise) -> XForma.PathGeometry` | An elliptical arc from the current point to the point, as in SVG: the radii, whether it's the larger of the two possible arcs, and whether it turns clockwise. Arcs become cubic curves. |
| `Bounds.get -> XForma.Rect` | A rectangle holding every point of the path (including control points). |
| `Close() -> XForma.PathGeometry` | Closes the figure with a line back to its start. |
| `CubicTo(XForma.Point control1, XForma.Point control2, XForma.Point point) -> XForma.PathGeometry` |  |
| `LineTo(XForma.Point point) -> XForma.PathGeometry` |  |
| `MoveTo(XForma.Point point) -> XForma.PathGeometry` | Starts a new figure at the point. |
| `PathGeometry() -> void` |  |
| `QuadraticTo(XForma.Point control, XForma.Point point) -> XForma.PathGeometry` |  |
| `Segments.get -> System.Collections.Generic.IReadOnlyList<XForma.PathSegment>` |  |

## PathSegment

A segment of a path: the end point, and the control points of a curve.

| Member | |
|---|---|
| `Control1.get -> XForma.Point` |  |
| `Control2.get -> XForma.Point` |  |
| `Kind.get -> XForma.PathSegmentKind` |  |
| `PathSegment() -> void` |  |
| `Point.get -> XForma.Point` |  |

## PathSegmentKind

What a segment of a path geometry is.

| Member | |
|---|---|
| `Close = 4 -> XForma.PathSegmentKind` |  |
| `CubicTo = 3 -> XForma.PathSegmentKind` |  |
| `LineTo = 1 -> XForma.PathSegmentKind` |  |
| `MoveTo = 0 -> XForma.PathSegmentKind` |  |
| `QuadraticTo = 2 -> XForma.PathSegmentKind` |  |

## Point

A position, in device-independent units.

| Member | |
|---|---|
| `DistanceSquaredTo(XForma.Point other) -> float` |  |
| `DistanceTo(XForma.Point other) -> float` |  |
| `Equals(object obj) -> bool` |  |
| `GetHashCode() -> int` |  |
| `Point() -> void` |  |
| `Point(float x, float y) -> void` |  |
| `ToString() -> string` |  |
| `Translate(XForma.Angle angle, float distance) -> XForma.Point` |  |
| `Translate(XForma.Point point, XForma.Angle angle, float distance) -> XForma.Point` |  |
| `Translate(XForma.Point point, float dx, float dy) -> XForma.Point` |  |
| `Translate(float dx, float dy) -> XForma.Point` |  |
| `X.get -> float` |  |
| `Y.get -> float` |  |
| `Zero -> XForma.Point` |  |
| `operator !=(XForma.Point a, XForma.Point b) -> bool` |  |
| `operator *(XForma.Point a, float v) -> XForma.Point` |  |
| `operator *(float v, XForma.Point a) -> XForma.Point` |  |
| `operator +(XForma.Point a, XForma.Point v) -> XForma.Point` |  |
| `operator -(XForma.Point a) -> XForma.Point` |  |
| `operator -(XForma.Point a, XForma.Point v) -> XForma.Point` |  |
| `operator /(XForma.Point a, float v) -> XForma.Point` |  |
| `operator ==(XForma.Point a, XForma.Point b) -> bool` |  |

## PointerDeviceType

What a pointer is: touch, a mouse or a pen.

| Member | |
|---|---|
| `Mouse = 1 -> XForma.PointerDeviceType` |  |
| `Pen = 2 -> XForma.PointerDeviceType` |  |
| `Touch = 0 -> XForma.PointerDeviceType` |  |

## PointerEventArgs

A pointer (touch, mouse or pen) pressing, moving, releasing or turning a wheel over a view.

| Member | |
|---|---|
| `DeviceType.get -> XForma.PointerDeviceType` |  |
| `Handled.get -> bool` | Set to true to stop the event: a handled press keeps the pointer for this view, a handled move keeps scrolling parents from taking it. |
| `Handled.set -> void` | Set to true to stop the event: a handled press keeps the pointer for this view, a handled move keeps scrolling parents from taking it. |
| `IsHovering.get -> bool` |  |
| `IsInContact.get -> bool` |  |
| `KeyModifiers.get -> XForma.KeyModifiers` | The modifier keys held down. |
| `OriginalSource.get -> XForma.View` | The view the event was first delivered to. |
| `PointerEventArgs(XForma.Point position, XForma.PointerInputState state, bool isInContact) -> void` |  |
| `PointerEventArgs(int pointerId, XForma.PointerDeviceType deviceType, XForma.Point position, XForma.PointerInputState state, bool isInContact, XForma.KeyModifiers keyModifiers, float wheelDelta = 0) -> void` |  |
| `PointerId.get -> int` | Identifies the pointer while it's down: each finger of a multi-touch gesture has its own. |
| `Position.get -> XForma.Point` | Where the pointer is, relative to the view receiving the event. |
| `State.get -> XForma.PointerInputState` |  |
| `WheelDelta.get -> float` | For a wheel event, how far the wheel turned, in device-independent pixels (positive is up, or away from the user). |

## PointerInputState

What a pointer just did.

| Member | |
|---|---|
| `Began = 0 -> XForma.PointerInputState` |  |
| `Canceled = 3 -> XForma.PointerInputState` |  |
| `Ended = 2 -> XForma.PointerInputState` |  |
| `Moved = 1 -> XForma.PointerInputState` |  |

## Rect

A rectangle, in device-independent units.

| Member | |
|---|---|
| `Bottom.get -> float` |  |
| `Center.get -> XForma.Point` |  |
| `Deflate(float dx, float dy) -> XForma.Rect` |  |
| `Equals(object obj) -> bool` |  |
| `FromLTRB(float left, float top, float right, float bottom) -> XForma.Rect` |  |
| `GetHashCode() -> int` |  |
| `Height.get -> float` |  |
| `Height.set -> void` |  |
| `Inflate(float dx, float dy) -> XForma.Rect` |  |
| `Left.get -> float` |  |
| `Left.set -> void` |  |
| `Location.get -> XForma.Point` |  |
| `Offset(float x, float y) -> XForma.Rect` |  |
| `Rect() -> void` |  |
| `Rect(float x, float y, float width, float height) -> void` |  |
| `Right.get -> float` |  |
| `Size.get -> XForma.Size` |  |
| `Top.get -> float` |  |
| `Top.set -> void` |  |
| `Union(XForma.Rect other) -> XForma.Rect` |  |
| `Width.get -> float` |  |
| `Width.set -> void` |  |
| `X.get -> float` |  |
| `X.set -> void` |  |
| `Y.get -> float` |  |
| `Y.set -> void` |  |
| `operator !=(XForma.Rect a, XForma.Rect b) -> bool` |  |
| `operator ==(XForma.Rect a, XForma.Rect b) -> bool` |  |

## Route

A path matched to one of the app's routes (see `OnConfigureRoutes`): the values of its parameters, as text, for the route's function to make its page from.

| Member | |
|---|---|
| `GetGuid(string name) -> System.Guid` | A parameter's value as a `Guid`. Throws when it's missing or isn't one. |
| `GetInt32(string name) -> int` | A parameter's value as an `Int32`. Throws when it's missing or isn't a number. |
| `GetInt64(string name) -> long` | A parameter's value as a `Int64`. Throws when it's missing or isn't a number. |
| `Path.get -> string` | The path navigated to, such as `/item/42`. |
| `Pattern.get -> string` | The route it matched, such as `/item/{id}`. |
| `this[string name].get -> string` |  |

## Routes

The app's pages by path, each with a function that makes the page (see `OnConfigureRoutes`). Nothing is found by reflection: a page is made by the app's own code, so routes are safe for trimming and AOT.

| Member | |
|---|---|
| `Add(string pattern, System.Func<XForma.Route, XForma.Page> createPage) -> void` | Adds a route: its pattern, and the function that makes its page from a path that matches. The function may return null when the path's values name nothing (an item that's gone), as if no route matched. |

## SafeAreaEdges

Edges of the safe area: the parts of the screen next to the system's bars, display cutout and home indicator.

| Member | |
|---|---|
| `All = XForma.SafeAreaEdges.Left \| XForma.SafeAreaEdges.Top \| XForma.SafeAreaEdges.Right \| XForma.SafeAreaEdges.Bottom -> XForma.SafeAreaEdges` |  |
| `Bottom = 8 -> XForma.SafeAreaEdges` |  |
| `Left = 1 -> XForma.SafeAreaEdges` |  |
| `None = 0 -> XForma.SafeAreaEdges` |  |
| `Right = 4 -> XForma.SafeAreaEdges` |  |
| `Top = 2 -> XForma.SafeAreaEdges` |  |

## Shadow

A drop shadow beneath a view, outside its bounds, as CSS's `box-shadow`: its color, offset and blur radius. Set one with `Shadow`; the platform draws it natively.

| Member | |
|---|---|
| `<Clone>$() -> XForma.Shadow` |  |
| `Blur.get -> float` | How far the shadow's edge blurs, as CSS's blur radius (twice the blur's standard deviation). |
| `Blur.init -> void` | How far the shadow's edge blurs, as CSS's blur radius (twice the blur's standard deviation). |
| `Color.get -> XForma.Color` | The shadow's color, with its opacity: black at 30% by default. |
| `Color.init -> void` | The shadow's color, with its opacity: black at 30% by default. |
| `CornerRadius.get -> float` | The rectangle's corner radius. |
| `CornerRadius.init -> void` | The rectangle's corner radius. |
| `Equals(XForma.Shadow other) -> bool` |  |
| `Equals(object obj) -> bool` |  |
| `GetHashCode() -> int` |  |
| `GlowColor -> XForma.Color` | What an elevated surface casts on a dark background in place of a shadow, which wouldn't show there: a faint white glow (14%), centered on its edges. Frames' sliding pages and themes' popups use it in dark mode. |
| `Offset.get -> XForma.Point` | How far the shadow is from the shape. |
| `Offset.init -> void` | How far the shadow is from the shape. |
| `Outset.get -> XForma.Thickness` | How far the shape reaches past the view's bounds on each side, for a view whose content does (such as a page whose views reach into the safe area beyond it): zero by default. |
| `Outset.init -> void` | How far the shape reaches past the view's bounds on each side, for a view whose content does (such as a page whose views reach into the safe area beyond it): zero by default. |
| `Shadow() -> void` |  |
| `Shape.get -> XForma.ShadowShape` | The shape that casts it. |
| `Shape.init -> void` | The shape that casts it. |
| `ToString() -> string` |  |
| `operator !=(XForma.Shadow left, XForma.Shadow right) -> bool` |  |
| `operator ==(XForma.Shadow left, XForma.Shadow right) -> bool` |  |

## ShadowShape

The shape that casts a `Shadow`: the view's bounds, or the ellipse (a circle, for a square view) inside them.

| Member | |
|---|---|
| `Ellipse = 1 -> XForma.ShadowShape` |  |
| `Rectangle = 0 -> XForma.ShadowShape` |  |

## Size

A width and a height, in device-independent units.

| Member | |
|---|---|
| `Empty -> XForma.Size` |  |
| `Equals(object obj) -> bool` |  |
| `GetHashCode() -> int` |  |
| `Height.get -> float` |  |
| `Size() -> void` |  |
| `Size(XForma.Dimension size) -> void` |  |
| `Size(XForma.Dimension width, XForma.Dimension height) -> void` |  |
| `Width.get -> float` |  |
| `operator !=(XForma.Size a, XForma.Size b) -> bool` |  |
| `operator ==(XForma.Size a, XForma.Size b) -> bool` |  |

## SolidColorBrush

A brush that fills with one color.

| Member | |
|---|---|
| `Color.get -> XForma.Color` |  |
| `SolidColorBrush(XForma.Color color) -> void` |  |

## SpringEase

A spring: it overshoots and settles, oscillating the given number of times (as WPF's ElasticEase).

| Member | |
|---|---|
| `EaseInCore(float t) -> float` |  |
| `Oscillations.get -> int` |  |
| `Oscillations.set -> void` |  |
| `SpringEase(XForma.EasingMode easingMode) -> void` |  |
| `Springiness.get -> float` | How quickly the oscillation dies away: larger is stiffer. |
| `Springiness.set -> void` | How quickly the oscillation dies away: larger is stiffer. |

## TextAlignment

Where text sits in its space.

| Member | |
|---|---|
| `Center = 1 -> XForma.TextAlignment` |  |
| `End = 2 -> XForma.TextAlignment` |  |
| `Start = 0 -> XForma.TextAlignment` |  |

## TextBlockProperties

The bindable properties of `TextBlock`, such as `TextBlock.TextProperty`, for `ViewBindingExtensions`.

| Member | |
|---|---|
| `extension(XForma.Primitives.TextBlock)` |  |
| `extension(XForma.Primitives.TextBlock).FontSizeProperty.get -> XForma.ViewProperty<XForma.Primitives.TextBlock, float>` |  |
| `extension(XForma.Primitives.TextBlock).FontWeightProperty.get -> XForma.ViewProperty<XForma.Primitives.TextBlock, XForma.FontWeight>` |  |
| `extension(XForma.Primitives.TextBlock).ForegroundProperty.get -> XForma.ViewProperty<XForma.Primitives.TextBlock, XForma.Color>` |  |
| `extension(XForma.Primitives.TextBlock).StrikethroughProperty.get -> XForma.ViewProperty<XForma.Primitives.TextBlock, bool>` |  |
| `extension(XForma.Primitives.TextBlock).TextProperty.get -> XForma.ViewProperty<XForma.Primitives.TextBlock, string>` |  |
| `get_FontSizeProperty() -> XForma.ViewProperty<XForma.Primitives.TextBlock, float>` |  |
| `get_FontWeightProperty() -> XForma.ViewProperty<XForma.Primitives.TextBlock, XForma.FontWeight>` |  |
| `get_ForegroundProperty() -> XForma.ViewProperty<XForma.Primitives.TextBlock, XForma.Color>` |  |
| `get_StrikethroughProperty() -> XForma.ViewProperty<XForma.Primitives.TextBlock, bool>` |  |
| `get_TextProperty() -> XForma.ViewProperty<XForma.Primitives.TextBlock, string>` |  |

## TextBoxProperties

The bindable properties of `TextBox`, such as `TextBox.TextProperty`, for `ViewBindingExtensions`.

| Member | |
|---|---|
| `extension(XForma.Primitives.TextBox)` |  |
| `extension(XForma.Primitives.TextBox).PlaceholderTextProperty.get -> XForma.ViewProperty<XForma.Primitives.TextBox, string>` |  |
| `extension(XForma.Primitives.TextBox).TextProperty.get -> XForma.ViewProperty<XForma.Primitives.TextBox, string>` |  |
| `get_PlaceholderTextProperty() -> XForma.ViewProperty<XForma.Primitives.TextBox, string>` |  |
| `get_TextProperty() -> XForma.ViewProperty<XForma.Primitives.TextBox, string>` |  |

## TextFormat

How text looks and flows: its font and size, alignment, wrapping and trimming. Used to create a `TextLayout`, or to draw text directly with `DrawText`.

| Member | |
|---|---|
| `FontFamily.get -> string` | The font family: a generic family from `FontFamilies` (serif, sans-serif or monospace), a family registered with `RegisterFont`, or an installed family's name. Null (the default) is the platform's UI font. |
| `FontFamily.set -> void` | The font family: a generic family from `FontFamilies` (serif, sans-serif or monospace), a family registered with `RegisterFont`, or an installed family's name. Null (the default) is the platform's UI font. |
| `FontSize.get -> float` |  |
| `FontSize.set -> void` |  |
| `FontStyle.get -> XForma.FontStyle` |  |
| `FontStyle.set -> void` |  |
| `FontWeight.get -> XForma.FontWeight` |  |
| `FontWeight.set -> void` |  |
| `HorizontalAlignment.get -> XForma.HorizontalAlignment` |  |
| `HorizontalAlignment.set -> void` |  |
| `MaxLines.get -> int` | The most lines shown (0, the default, for no limit). Text cut off by the limit ends with an ellipsis. |
| `MaxLines.set -> void` | The most lines shown (0, the default, for no limit). Text cut off by the limit ends with an ellipsis. |
| `TextFormat() -> void` |  |
| `TextTrimming.get -> XForma.TextTrimming` |  |
| `TextTrimming.set -> void` |  |
| `TextWrapping.get -> XForma.TextWrapping` |  |
| `TextWrapping.set -> void` |  |
| `VerticalAlignment.get -> XForma.VerticalAlignment` |  |
| `VerticalAlignment.set -> void` |  |

## TextLayout

Text laid out with a format, within a maximum size: measured once, and drawn with `DrawText`. The layout is immutable; dispose it to free its native resources early.

| Member | |
|---|---|
| `Dispose() -> void` |  |
| `Format.get -> XForma.TextFormat` | A copy of the format the layout was created with. |
| `LineCount.get -> int` | The number of lines shown. |
| `MaxHeight.get -> float` |  |
| `MaxWidth.get -> float` |  |
| `Measure(string text, XForma.TextFormat format, float maxWidth = Infinity) -> XForma.Size` | The size of the text on one line (or wrapped and limited as the format says, within the width), for sizing owner-drawn controls. |
| `Size.get -> XForma.Size` | The size of the laid-out text. |
| `Spans.get -> System.Collections.Generic.IReadOnlyList<XForma.TextSpan>` |  |
| `Text.get -> string` |  |
| `TextLayout(string text, XForma.TextFormat format, float maxWidth = Infinity, float maxHeight = Infinity, System.Collections.Generic.IReadOnlyList<XForma.TextSpan> spans = null) -> void` |  |

## TextSpan

A range of text with its own style, for text with mixed formatting. Unset values keep the text's own.

| Member | |
|---|---|
| `FontStyle.get -> XForma.FontStyle?` |  |
| `FontWeight.get -> XForma.FontWeight?` |  |
| `Foreground.get -> XForma.Color?` |  |
| `Length.get -> int` |  |
| `Start.get -> int` |  |
| `Strikethrough.get -> bool` |  |
| `TextSpan() -> void` |  |
| `TextSpan(int start, int length, XForma.FontWeight? fontWeight = null, XForma.FontStyle? fontStyle = null, XForma.Color? foreground = null, bool underline = false, bool strikethrough = false) -> void` |  |
| `Underline.get -> bool` |  |

## TextTrimming

What happens to text that doesn't fit.

| Member | |
|---|---|
| `CharacterEllipsis = 1 -> XForma.TextTrimming` |  |
| `None = 0 -> XForma.TextTrimming` |  |

## TextWrapping

Whether text wraps onto more lines when it doesn't fit.

| Member | |
|---|---|
| `NoWrap = 0 -> XForma.TextWrapping` |  |
| `Wrap = 1 -> XForma.TextWrapping` |  |

## Theme

A theme: the mode and colors for it, and whatever else a control library keeps in it (its type scale, shapes, motion, control styles) as extensions. Set one on the application, or on any view for it and its descendants.

| Member | |
|---|---|
| `<Clone>$() -> XForma.Theme` |  |
| `Colors.get -> XForma.ColorScheme` |  |
| `Colors.init -> void` |  |
| `ContrastingColor(XForma.Color color) -> XForma.Color` | Black or white, whichever reads better on the color. |
| `DefaultAccentColor -> XForma.Color` | The accent color when neither the app nor the system gives one. |
| `Equals(XForma.Theme other) -> bool` |  |
| `Equals(object obj) -> bool` |  |
| `Extensions.get -> System.Collections.Immutable.ImmutableDictionary<System.Type, object>` | What control libraries keep in the theme, by type: a type scale, shapes, motion, default styles for controls (see `GetExtension``1`). |
| `Extensions.init -> void` | What control libraries keep in the theme, by type: a type scale, shapes, motion, default styles for controls (see `GetExtension``1`). |
| `GetExtension<TExtension>() -> TExtension` |  |
| `GetHashCode() -> int` |  |
| `Mode.get -> XForma.ThemeMode` |  |
| `Mode.init -> void` |  |
| `PageEdgeShadow.get -> XForma.Shadow` | The shadow along a sliding page's moving edges, so they show over a page of the same color; null (the default) for none. |
| `PageEdgeShadow.init -> void` | The shadow along a sliding page's moving edges, so they show over a page of the same color; null (the default) for none. |
| `PageTransition.get -> XForma.PageTransition` | How a frame's pages come and go when navigating doesn't name a transition; null (the default) for `Slide`. A switch following a gesture always slides: over the current page for `SlideOver`, otherwise together with it. |
| `PageTransition.init -> void` | How a frame's pages come and go when navigating doesn't name a transition; null (the default) for `Slide`. A switch following a gesture always slides: over the current page for `SlideOver`, otherwise together with it. |
| `Theme() -> void` |  |
| `ToString() -> string` |  |
| `WithExtension<TExtension>(TExtension extension) -> XForma.Theme` |  |
| `operator !=(XForma.Theme left, XForma.Theme right) -> bool` |  |
| `operator ==(XForma.Theme left, XForma.Theme right) -> bool` |  |

## ThemeMode

Whether the UI is light, dark or high contrast. The system reports its mode; an app can follow it or choose one.

| Member | |
|---|---|
| `Dark = 1 -> XForma.ThemeMode` |  |
| `HighContrast = 2 -> XForma.ThemeMode` |  |
| `Light = 0 -> XForma.ThemeMode` |  |

## Thickness

A distance on each of four sides, for margins and padding.

| Member | |
|---|---|
| `Bottom -> float` |  |
| `Equals(object obj) -> bool` |  |
| `GetHashCode() -> int` |  |
| `Horizontal.get -> float` |  |
| `Left -> float` |  |
| `Right -> float` |  |
| `Thickness() -> void` |  |
| `Thickness(float all) -> void` |  |
| `Thickness(float left, float top, float right, float bottom) -> void` |  |
| `Thickness(float leftAndRight, float topAndBottom) -> void` |  |
| `ToString() -> string` |  |
| `Top -> float` |  |
| `Vertical.get -> float` |  |
| `Zero -> XForma.Thickness` |  |
| `operator !=(XForma.Thickness a, XForma.Thickness b) -> bool` |  |
| `operator ==(XForma.Thickness a, XForma.Thickness b) -> bool` |  |

## TitleBar

The main window's title bar, on the platforms that have one.

| Member | |
|---|---|
| `AccentColor.get -> XForma.Color?` |  |
| `AccentColor.set -> void` |  |
| `ForegroundColor.get -> XForma.Color?` |  |
| `ForegroundColor.set -> void` |  |
| `TitleBar(XForma.Application application) -> void` |  |

## VerticalAlignment

Where a view sits vertically in the space its parent gives it.

| Member | |
|---|---|
| `Bottom = 2 -> XForma.VerticalAlignment` |  |
| `Center = 1 -> XForma.VerticalAlignment` |  |
| `Stretch = 3 -> XForma.VerticalAlignment` |  |
| `Top = 0 -> XForma.VerticalAlignment` |  |

## View

Something on screen: it measures, arranges and draws itself, and takes input.

| Member | |
|---|---|
| `AccessibilityIsChecked.get -> bool?` | Whether the view is checked, or null when it has no checked state. |
| `AccessibilityIsExpanded.get -> bool?` | Whether the view (such as a picker) is expanded, or null when it has no expanded state. |
| `AccessibilityIsHidden.get -> bool` | Whether assistive technology skips the view and everything in it, though it shows: content an open drawer or dialog covers, say. |
| `AccessibilityIsHidden.set -> void` | Whether assistive technology skips the view and everything in it, though it shows: content an open drawer or dialog covers, say. |
| `AccessibilityIsMixed.get -> bool` | Whether the view's checked state is neither checked nor unchecked (a check box's indeterminate state). |
| `AccessibilityIsSelected.get -> bool` | Whether the view (a list item, tab or radio button) is selected. |
| `AccessibilityLabel.get -> string` | What assistive technology reads for the view. Controls with text provide a default. |
| `AccessibilityLabel.set -> void` | What assistive technology reads for the view. Controls with text provide a default. |
| `AccessibilityRange.get -> XForma.AccessibilityRange?` | The view's value within a range (a slider's, a progress bar's), or null when it has none. |
| `AccessibilityRole.get -> XForma.AccessibilityRole` | What the view is to assistive technology. Controls provide a default. |
| `AccessibilityRole.set -> void` | What the view is to assistive technology. Controls provide a default. |
| `AccessibilityValue.get -> string` | The value assistive technology reads after the label, such as a picker's current choice, or null. |
| `Activated -> System.EventHandler` | Raised when the view is activated without a pointer: Enter or Space while it has focus, or a screen reader's or UI Automation's activate command. Handling it says the view does something when activated, so it's focusable unless `Focusable` says otherwise. Gestures that respond to taps handle it, as a tap. |
| `AddVisualChild(XForma.View child) -> void` | Adds a child after the others. |
| `AllowDrop.get -> bool` | Whether drags can drop on the view (and the views inside it that don't allow drops themselves). |
| `AllowDrop.set -> void` | Whether drags can drop on the view (and the views inside it that don't allow drops themselves). |
| `AnimatesChildren.get -> bool` | Whether the framework animates the children's moves (a panel with layout transitions), so each needs an animator layer. |
| `Animator.get -> XForma.Animator` | The app's animator for the view: its translation, scale, rotation and opacity, set directly or animated. Null (the default) for a view that never animates. Set it when the view is created, for example `new ProgressRing { Animator = new() }`. Controls never set their own; they use `CreateOwnAnimator`. |
| `Animator.init -> void` | The app's animator for the view: its translation, scale, rotation and opacity, set directly or animated. Null (the default) for a view that never animates. Set it when the view is created, for example `new ProgressRing { Animator = new() }`. Controls never set their own; they use `CreateOwnAnimator`. |
| `Arrange(XForma.Rect finalRect) -> void` |  |
| `ArrangeContent(XForma.Size size) -> void` | Positions the view's own children within the given size using their last measurement. For platforms whose layout system guarantees a measure pass before every arrange (remeasuring during arrange there would make the platform lay out again). |
| `ArrangeOverride(XForma.Size finalSize) -> void` | Arranges the children (by calling their `Arrange`) within the final size, inside the view's padding: a child arranged at (0, 0) sits at the padding's top left corner. |
| `Attached -> System.EventHandler` | Raised when the view is added to a parent (or shown), once its native object exists. |
| `Bounds.get -> XForma.Rect` | The view's position and size from the last arrange, relative to its parent. |
| `CanDrag.get -> bool` | Whether the user can drag the view: by moving a mouse over it while pressed, by holding a finger on it, or from the keyboard, with Ctrl+Space while it has focus. Handle `DragStarting` to say what's dragged. |
| `CanDrag.set -> void` | Whether the user can drag the view: by moving a mouse over it while pressed, by holding a finger on it, or from the keyboard, with Ctrl+Space while it has focus. Handle `DragStarting` to say what's dragged. |
| `CapturePointer(XForma.PointerEventArgs e) -> bool` | Keeps the pointer's moves for this view, so a scrolling parent can't take them over (as it would for an unhandled drag). The capture ends when the pointer is released or canceled. |
| `ClearVisualChildren() -> void` | Removes every child. |
| `ClipToBounds.get -> bool` | Whether the view clips its children (and drawing) to its bounds. False by default: children can draw outside their parent. |
| `ClipToBounds.set -> void` | Whether the view clips its children (and drawing) to its bounds. False by default: children can draw outside their parent. |
| `ContentPadding.get -> XForma.Thickness` | Space between the view's edges and its children. Containers expose it as `Padding`. |
| `ContentPadding.set -> void` | Space between the view's edges and its children. Containers expose it as `Padding`. |
| `ContextRequested -> System.EventHandler<XForma.ContextRequestedEventArgs>` | Raised when the user asks for the view's context menu (a right-click, a long press, the Menu key or Shift+F10), and then for its ancestors until one handles it. |
| `CreateDrawnAnimator() -> XForma.DrawnAnimator` | Creates a scale and opacity the view applies to its own drawing, animated without a native layer (see `DrawnAnimator`). For small effects, such as press feedback, that shouldn't cost the view a layer of its own, as an `Animator` would. |
| `CreateOwnAnimator() -> XForma.Animator` | Creates an animator that the view keeps privately, in its own field, to animate its own content (such as a spinning ring). Call it from the view's constructor or `OnAttaching`: it throws once the view's native object exists. Each call creates another animator, so a subclass can add its own. The public `Animator` always belongs to the app. |
| `DefaultAccessibilityLabel.get -> string` |  |
| `DefaultAccessibilityRole.get -> XForma.AccessibilityRole` |  |
| `DesiredSize.get -> XForma.Size` | The size the view asked for in the last measure, including its margin. |
| `Detached -> System.EventHandler` | Raised when the view is removed from its parent. |
| `DragEnter -> System.EventHandler<XForma.DragEventArgs>` |  |
| `DragLeave -> System.EventHandler<XForma.DragEventArgs>` |  |
| `DragOver -> System.EventHandler<XForma.DragEventArgs>` |  |
| `DragStarting -> System.EventHandler<XForma.DragStartingEventArgs>` |  |
| `DrawsContent.get -> bool` | Whether the view draws its own content, in `Render`, beneath its children. Set it in the constructor. |
| `DrawsContent.set -> void` | Whether the view draws its own content, in `Render`, beneath its children. Set it in the constructor. |
| `Drop -> System.EventHandler<XForma.DragEventArgs>` |  |
| `DropCompleted -> System.EventHandler<XForma.DropCompletedEventArgs>` |  |
| `ExtendsIntoSafeArea.get -> XForma.SafeAreaEdges` | The edges on which the view reaches into the safe area: where it lies along the edge of a page that keeps clear of the system's bars, it grows through them to the screen's edge, and its content is padded by as much, staying clear. So a bar docked at the top or bottom of a page fills the status bar or home indicator area behind it, and moves as one view when it animates. None by default. |
| `ExtendsIntoSafeArea.set -> void` | The edges on which the view reaches into the safe area: where it lies along the edge of a page that keeps clear of the system's bars, it grows through them to the screen's edge, and its content is padded by as much, staying clear. So a bar docked at the top or bottom of a page fills the status bar or home indicator area behind it, and moves as one view when it animates. None by default. |
| `FlowDirection.get -> XForma.FlowDirection` | Whether the view's children are laid out left to right or right to left. Inherited from the parent unless set. |
| `FlowDirection.set -> void` | Whether the view's children are laid out left to right or right to left. Inherited from the parent unless set. |
| `Focus() -> bool` | Moves keyboard focus to the view. Returns false where the platform doesn't support keyboard focus for the view. |
| `Focusable.get -> bool` | Whether keyboard focus can move to the view (with Tab). By default, views that respond to taps are focusable. |
| `Focusable.set -> void` | Whether keyboard focus can move to the view (with Tab). By default, views that respond to taps are focusable. |
| `GestureRecognizers.get -> System.Collections.Generic.IList<XForma.GestureRecognizer>` | The gesture recognizers that interpret the view's pointer input (taps, swipes, pans and so on), each given every pointer event in turn. |
| `GetBackgroundColor() -> XForma.Color` | The color the view is filled with beneath its children, when it doesn't draw itself: transparent unless a subclass says otherwise. Call `InvalidateBackground` when it changes. |
| `GetUserState() -> object` | What has changed in the view since it was first attached, mostly through input (text typed, a selection, where it's scrolled), or null: carried over to the view that replaces it when its page or control builds again after a hot reload edit (see `SetUserState`). A control with such state overrides both. |
| `GotFocus -> System.EventHandler` |  |
| `HasPointerCapture(int pointerId) -> bool` | Whether the view has captured the pointer. |
| `Height.get -> float` | The height the view asks for, or `Auto` (the default) to size to its content. |
| `Height.set -> void` | The height the view asks for, or `Auto` (the default) to size to its content. |
| `HitTestOutset.get -> XForma.Thickness` | How far past its bounds the view takes presses (taps and gestures alike), on each side: a bigger touch target than it looks, as UIKit's `point(inside:)` and Android's touch delegates give. Zero by default. A view drawn above it still takes presses on itself; what's beneath it doesn't, in the outset. |
| `HitTestOutset.set -> void` | How far past its bounds the view takes presses (taps and gestures alike), on each side: a bigger touch target than it looks, as UIKit's `point(inside:)` and Android's touch delegates give. Zero by default. A view drawn above it still takes presses on itself; what's beneath it doesn't, in the outset. |
| `HorizontalAlignment.get -> XForma.HorizontalAlignment` |  |
| `HorizontalAlignment.set -> void` |  |
| `InsertVisualChild(int index, XForma.View child) -> void` | Inserts a child at the index, in layout order. |
| `InvalidateArrange() -> void` | Arranges the view's children again, without changing the view's size. |
| `InvalidateBackground() -> void` | Applies a change to `GetBackgroundColor` (or draws a view that draws itself again). |
| `InvalidateMeasure() -> void` | Lays the view out again because its size may have changed: its parent measures it again, and arranges it. |
| `InvalidateVisual() -> void` | Draws the view again. |
| `IsAccessibilityActionSupported(XForma.AccessibilityAction action) -> bool` | Whether assistive technology can ask the view to do the action. |
| `IsDragPossible.get -> bool` | Whether a touch pressed on the view may yet become a drag: the view or an ancestor has a drag gesture, or an ancestor scrolls. A control holds back its pressed look briefly then, so a press that turns into a swipe or a scroll doesn't flash it. |
| `IsFocused.get -> bool` | Whether the view has keyboard focus. |
| `IsHitTestVisible.get -> bool` |  |
| `IsHitTestVisible.set -> void` |  |
| `IsInTabOrder(XForma.View child) -> bool` | Whether Tab can reach this child, and the views inside it: not content an open drawer covers, say. |
| `IsPointerOver.get -> bool` | Whether a hovering pointer is over the view or one of its descendants. |
| `IsTabScope.get -> bool` | Whether Tab stays inside this view for now, wrapping around, as in an open drawer. |
| `IsTabStop.get -> bool` | Whether Tab stops at the view: it's focusable, and (for an item in a list) it's the list's current item. Arrow keys reach the rest. |
| `IsVisible.get -> bool` |  |
| `IsVisible.set -> void` |  |
| `KeyDown -> System.EventHandler<XForma.KeyEventArgs>` |  |
| `KeyUp -> System.EventHandler<XForma.KeyEventArgs>` |  |
| `LayoutContent(XForma.Size size) -> void` | Positions the view's own children within the given size. Called by native containers from their layout pass; remeasures the children first if the final size differs from the size they were measured at. |
| `LostFocus -> System.EventHandler` |  |
| `Margin.get -> XForma.Thickness` |  |
| `Margin.set -> void` |  |
| `MaxHeight.get -> float` | The most height the view takes, or `Auto` (the default) for no limit. |
| `MaxHeight.set -> void` | The most height the view takes, or `Auto` (the default) for no limit. |
| `MaxWidth.get -> float` | The most width the view takes, or `Auto` (the default) for no limit. |
| `MaxWidth.set -> void` | The most width the view takes, or `Auto` (the default) for no limit. |
| `Measure(XForma.Size availableSize) -> void` | Measures the view within the available size, which may be infinite, and updates `DesiredSize`. Called by the parent view. |
| `MeasureContent(XForma.Size availableSize) -> XForma.Size` | Measures the view's own children within the available size and returns the size they need, plus the view's padding. Called by native containers from their measure pass; views without layout children return their padding. |
| `MeasureOverride(XForma.Size availableSize) -> XForma.Size` | Measures the children (by calling their `Measure`) and returns the size the view needs for them, within its padding. |
| `MinHeight.get -> float` | The least height the view takes (0 by default). |
| `MinHeight.set -> void` | The least height the view takes (0 by default). |
| `MinWidth.get -> float` | The least width the view takes (0 by default). |
| `MinWidth.set -> void` | The least width the view takes (0 by default). |
| `Name.get -> string` | The view's name, which identifies it among the views its page or control builds (XAML's `x:Name`). When a page or control builds again after a hot reload edit, a new view takes the user state of the old view with the same name, wherever it moved; an unnamed view takes that of the old view in the same place, of the same type (see `GetUserState`). |
| `Name.set -> void` | The view's name, which identifies it among the views its page or control builds (XAML's `x:Name`). When a page or control builds again after a hot reload edit, a new view takes the user state of the old view with the same name, wherever it moved; an unnamed view takes that of the old view in the same place, of the same type (see `GetUserState`). |
| `NotifyAccessibilityStateChanged() -> void` | Tells assistive technology that the view's checked or selected state, value or label changed. |
| `OnActivate() -> void` | Called for Enter or Space on a focused view, and for a screen reader's activate command. By default this raises `Activated`; a control that does something else overrides it. |
| `OnAttached() -> void` |  |
| `OnContextRequested(XForma.ContextRequestedEventArgs e) -> void` | Called when the user asks for the view's (or, unhandled, a descendant's) context menu. Set `e.Handled` to answer it. |
| `OnDetached() -> void` |  |
| `OnGotFocus() -> void` |  |
| `OnHotReload() -> void` | Called after an edit while the app runs with hot reload, on every view being shown, parents first. A view that built something once from code (a template's parts, a control's views) runs that code again here; a page's `Build`, and a list's item views, run again by themselves. |
| `OnKeyDown(XForma.KeyEventArgs e) -> void` | Called when a key is pressed while the view (or a descendant, which didn't handle it) has keyboard focus. Set `e.Handled` to stop it going to the view's ancestors. |
| `OnKeyUp(XForma.KeyEventArgs e) -> void` | Called when a key is released while the view (or a descendant) has keyboard focus. |
| `OnLostFocus() -> void` |  |
| `OnPointerCanceled(XForma.PointerEventArgs e) -> void` |  |
| `OnPointerEntered(XForma.PointerEventArgs e) -> void` |  |
| `OnPointerExited(XForma.PointerEventArgs e) -> void` |  |
| `OnPointerMoved(XForma.PointerEventArgs e) -> void` |  |
| `OnPointerPressed(XForma.PointerEventArgs e) -> void` |  |
| `OnPointerReleased(XForma.PointerEventArgs e) -> void` |  |
| `OnPointerWheelChanged(XForma.PointerEventArgs e) -> void` |  |
| `OnPropertyChanged(string propertyName = null) -> void` |  |
| `OnThemeChanged(XForma.Theme theme) -> void` |  |
| `Parent.get -> XForma.Element` | The view this view was added to, or null. |
| `PerformAccessibilityAction(XForma.AccessibilityAction action) -> bool` | Does an action for assistive technology. By default, each action does what a tap would (toggling, selecting); selecting something already selected does nothing. Returns whether the action was done. |
| `PointerCanceled -> System.EventHandler<XForma.PointerEventArgs>` |  |
| `PointerEntered -> System.EventHandler<XForma.PointerEventArgs>` | Raised when a hovering pointer (a mouse or pen, not a touch) moves over the view or one of its descendants. |
| `PointerExited -> System.EventHandler<XForma.PointerEventArgs>` | Raised when a hovering pointer leaves the view and its descendants, or the window. |
| `PointerMoved -> System.EventHandler<XForma.PointerEventArgs>` |  |
| `PointerPressed -> System.EventHandler<XForma.PointerEventArgs>` |  |
| `PointerReleased -> System.EventHandler<XForma.PointerEventArgs>` |  |
| `PointerWheelChanged -> System.EventHandler<XForma.PointerEventArgs>` |  |
| `PropertyChanged -> System.ComponentModel.PropertyChangedEventHandler` | Raised when a property of the view changes, whether code or the user (typing, toggling, choosing) changed it. |
| `ReleasePointerCapture(XForma.PointerEventArgs e) -> void` |  |
| `RemoveVisualChild(XForma.View child) -> bool` | Removes the child. Returns whether it was a child. |
| `RemoveVisualChildAt(int index) -> void` | Removes the child at the index, in layout order. |
| `Render(XForma.DrawingContext drawingContext) -> void` | Draws the view, within its bounds (from 0, 0), when it `DrawsContent`. By default it fills the view with its background color. Call `InvalidateVisual` to draw again. |
| `ReplaceVisualChild(int index, XForma.View child) -> void` | Replaces the child at the index with another. |
| `RestoreUserState(object saved, XForma.View newViews) -> void` | Gives the new views the user state and keyboard focus that `SaveUserState` recorded of the views they replace: by `Name`, then by place and type. |
| `SaveUserState(XForma.View views) -> object` | Records the user state and keyboard focus of views a page or control is about to replace, as it builds again: call it before they're removed (removing a focused view takes its focus), and pass what it returns to `RestoreUserState` once the new views are attached. A page and a `UserControl` do both around `Build`; a library control that builds views again itself calls them. |
| `SetAccessibilityRangeValue(double value) -> bool` | Sets the view's value within its range, for assistive technology. Returns whether it was set: false for a view whose range only shows a value (a progress bar's). |
| `SetProperty<T>(ref T field, T value, string propertyName = null) -> bool` |  |
| `SetUserState(object state) -> void` | Takes over the user state of the view this one replaces (see `GetUserState`), after it's attached. Where the code and the user disagree, the user's state wins. |
| `Shadow.get -> XForma.Shadow` | A drop shadow beneath the view, outside its bounds, or null (the default) for none. It moves, scales and fades with the view's animators. A frame gives its pages one while they slide. |
| `Shadow.set -> void` | A drop shadow beneath the view, outside its bounds, or null (the default) for none. It moves, scales and fades with the view's animators. A frame gives its pages one while they slide. |
| `TabIndex.get -> int` | Where Tab reaches the view among its siblings: lower first, and siblings with the same index in view order. A view's descendants come before its next sibling. By default `MaxValue`, so views given an index come before the rest, as in WinUI. |
| `TabIndex.set -> void` | Where Tab reaches the view among its siblings: lower first, and siblings with the same index in view order. A view's descendants come before its next sibling. By default `MaxValue`, so views given an index come before the rest, as in WinUI. |
| `Theme.get -> XForma.Theme` | A theme for this view and its descendants, or null (the default) to inherit its parent's. Use it for a section that looks different, such as a dark banner in a light app. |
| `Theme.set -> void` | A theme for this view and its descendants, or null (the default) to inherit its parent's. Use it for a section that looks different, such as a dark banner in a light app. |
| `VerticalAlignment.get -> XForma.VerticalAlignment` |  |
| `VerticalAlignment.set -> void` |  |
| `View() -> void` |  |
| `VisualChildren.get -> System.Collections.Generic.IReadOnlyList<XForma.View>` | The view's children, in the order they were added. A view with children is a container on every platform: a native view, or a lightweight one drawn into its layer. |
| `Width.get -> float` | The width the view asks for, or `Auto` (the default) to size to its content. |
| `Width.set -> void` | The width the view asks for, or `Auto` (the default) to size to its content. |
| `ZIndex.get -> int` | The view's order among its siblings in a panel: views with a higher index draw above (and are hit before) views with a lower one. Views with the same index keep the order they were added in. To draw over a native sibling, such as a frame, set it before the view is shown: a view with an index other than 0 is then a layer of its own. |
| `ZIndex.set -> void` | The view's order among its siblings in a panel: views with a higher index draw above (and are hit before) views with a lower one. Views with the same index keep the order they were added in. To draw over a native sibling, such as a frame, set it before the view is shown: a view with an index other than 0 is then a layer of its own. |

## ViewBindingExtensions

| Member | |
|---|---|
| `Bind<TView, TValue, TSource>(this TView view, XForma.ViewProperty<TView, TValue> property, TSource source, System.Func<TSource, TValue> getValue) -> XForma.Binding` |  |
| `Bind<TView, TValue, TSource>(this TView view, XForma.ViewProperty<TView, TValue> property, TSource source, System.Func<TSource, TValue> getValue, System.Action<TSource, TValue> setValue) -> XForma.Binding` |  |

## ViewProperties

The bindable properties of `View`, such as `View.IsVisibleProperty`, for `ViewBindingExtensions`.

| Member | |
|---|---|
| `extension(XForma.View)` |  |
| `extension(XForma.View).AccessibilityLabelProperty.get -> XForma.ViewProperty<XForma.View, string>` |  |
| `extension(XForma.View).HeightProperty.get -> XForma.ViewProperty<XForma.View, float>` |  |
| `extension(XForma.View).IsHitTestVisibleProperty.get -> XForma.ViewProperty<XForma.View, bool>` |  |
| `extension(XForma.View).IsVisibleProperty.get -> XForma.ViewProperty<XForma.View, bool>` |  |
| `extension(XForma.View).MarginProperty.get -> XForma.ViewProperty<XForma.View, XForma.Thickness>` |  |
| `extension(XForma.View).WidthProperty.get -> XForma.ViewProperty<XForma.View, float>` |  |
| `get_AccessibilityLabelProperty() -> XForma.ViewProperty<XForma.View, string>` |  |
| `get_HeightProperty() -> XForma.ViewProperty<XForma.View, float>` |  |
| `get_IsHitTestVisibleProperty() -> XForma.ViewProperty<XForma.View, bool>` |  |
| `get_IsVisibleProperty() -> XForma.ViewProperty<XForma.View, bool>` |  |
| `get_MarginProperty() -> XForma.ViewProperty<XForma.View, XForma.Thickness>` |  |
| `get_WidthProperty() -> XForma.ViewProperty<XForma.View, float>` |  |

## ViewProperty<TView, TValue>

| Member | |
|---|---|
| `GetValue(TView view) -> TValue` |  |
| `Name.get -> string` |  |
| `SetValue(TView view, TValue value) -> void` |  |
| `ToString() -> string` |  |
| `ViewProperty(string name, System.Func<TView, TValue> getValue, System.Action<TView, TValue> setValue) -> void` |  |

## Window

A window: the app's main one, or one it opened with `OpenWindowAsync` (on the desktop). On the desktop it has a title and a size the app can set; on phones the window is the screen, and only its size is meaningful.

| Member | |
|---|---|
| `CanResize.get -> bool` | Whether the app can resize the window (on the desktop). |
| `Close() -> void` | Closes a window the app opened. The main window closes with the app. |
| `Closed -> System.EventHandler` | Raised when a window the app opened is closed (by the user, or with `Close`). |
| `IsMainWindow.get -> bool` | Whether this is the app's main window, which closes the app. |
| `MinimumSize.get -> XForma.Size` | The smallest size the user can make the window (on the desktop). |
| `MinimumSize.set -> void` | The smallest size the user can make the window (on the desktop). |
| `Resize(XForma.Size size) -> void` | Resizes the window's content area (on the desktop; elsewhere it does nothing). |
| `SafeAreaInsets.get -> XForma.Thickness` | The parts of the window covered by system bars, a display cutout or the soft keyboard, in DIPs. Pages are kept clear of them unless a page sets `UsesSafeArea` to false, to draw under the bars and lay its content out around them itself. Zero on the desktop. |
| `SafeAreaInsetsChanged -> System.EventHandler` |  |
| `Size.get -> XForma.Size` | The size of the window's content area, in device-independent pixels. |
| `SizeChanged -> System.EventHandler` |  |
| `Title.get -> string` | The title in the window's title bar (and the task bar). |
| `Title.set -> void` | The title in the window's title bar (and the task bar). |

## XFormaApp

Runs an XForma app, with its theme: on each platform, it starts the framework's host with the app, and registers XForma.Toolkit's services with what the host knows (its window, activity or view controller).

| Member | |
|---|---|
| `Run(Android.App.Activity activity, Android.OS.Bundle savedInstanceState, XForma.Application application, XForma.Controls.AppTheme theme) -> void` | Runs the app, with its theme, in the activity (the app's own, with any base class): call it from the activity's `OnCreate`, after the base class's, with the state `OnCreate` was given. When Android re-creates the activity, the running app moves to the new one, and the new app object is left unused. The back stack is saved with the activity's state, and a link the activity is started with opens its page. |
| `Run(UIKit.UIWindowScene windowScene, UIKit.UISceneConnectionOptions connectionOptions, XForma.Application application, XForma.Controls.AppTheme theme) -> void` | Runs the app, with its theme, in the activity (the app's own, with any base class): call it from the activity's `OnCreate`, after the base class's, with the state `OnCreate` was given. When Android re-creates the activity, the running app moves to the new one, and the new app object is left unused. The back stack is saved with the activity's state, and a link the activity is started with opens its page. |
| `Run(string title, XForma.Application application, XForma.Controls.AppTheme theme) -> void` | Runs the app, with its theme, in the activity (the app's own, with any base class): call it from the activity's `OnCreate`, after the base class's, with the state `OnCreate` was given. When Android re-creates the activity, the running app moves to the new one, and the new app object is left unused. The back stack is saved with the activity's state, and a link the activity is started with opens its page. |
| `Run(string[] args, XForma.Application application, XForma.Controls.AppTheme theme) -> void` | Runs the app, with its theme, in the activity (the app's own, with any base class): call it from the activity's `OnCreate`, after the base class's, with the state `OnCreate` was given. When Android re-creates the activity, the running app moves to the new one, and the new app object is left unused. The back stack is saved with the activity's state, and a link the activity is started with opens its page. |
| `RunAsync(string title, XForma.Application application, XForma.Controls.AppTheme theme) -> System.Threading.Tasks.Task` | Runs the app in the page, with its theme. Call it once, from `Main`. |
