# XForma.Primitives

## Image

| Member | |
|---|---|
| `DefaultAccessibilityRole.get -> XForma.AccessibilityRole` |  |
| `Image() -> void` |  |
| `Source.get -> XForma.ImageSource` |  |
| `Source.set -> void` |  |
| `Tint.get -> XForma.Color` |  |
| `Tint.set -> void` |  |

## ItemContainer

An items view's container for one item: it holds the item's view (any view). The platform recycles containers and animates a container's height when it changes, so a container can reveal more content (such as commands) by showing it. Control libraries derive from it (and override `CreateItemContainer`) to add behavior such as selection or swipe gestures.

| Member | |
|---|---|
| `AddContent(XForma.View content) -> void` | Places the item's view in the container. By default it fills the container (children overlap, each in the whole area). |
| `ContainerFromContent(XForma.View view) -> XForma.Primitives.ItemContainer` | The container of a view that shows an item (the view, or one of its ancestors, is a container's content), or null. |
| `Content.get -> XForma.View` | The item's view. The container keeps its view as it's recycled, unless the items view needs another kind of view (a group header, say). |
| `IsTabStop.get -> bool` |  |
| `Item.get -> object` | The item the container is showing. |
| `ItemContainer() -> void` |  |
| `ItemsView.get -> XForma.Primitives.ItemsView` | The items view the container is showing an item for. |
| `OnBind() -> void` | Called when the container starts showing an item (it may be recycled). |
| `OnRecycle() -> void` | Called when the container is set aside, to reset any per-item state. |
| `Recycle() -> void` | Called by the platform when the container scrolls out of view and is set aside to show another item. |
| `RemoveContent(XForma.View content) -> void` | Takes the item's view out of the container. |

## ItemsView

A scrolling, virtualizing view of items: the platform realizes a container (an `ItemContainer`) for each item near the viewport and recycles them as the view scrolls. It lays the items out in a line or a grid, and can refresh when pulled. Control libraries add behavior such as selection (the toolkit's `ListView`).

| Member | |
|---|---|
| `AccentColor.get -> XForma.Color` | The refresh indicator's color: by default, the theme's accent color. |
| `AccentColor.set -> void` | The refresh indicator's color: by default, the theme's accent color. |
| `BackgroundColor.get -> XForma.Color` | The view's background: by default, the theme's surface color. |
| `BackgroundColor.set -> void` | The view's background: by default, the theme's surface color. |
| `Bind(XForma.Primitives.ItemsViewSource source) -> void` |  |
| `ContainerFromItem(object item) -> XForma.Primitives.ItemContainer` | The container showing the item, if it's in view, or null. |
| `CreateItemContainer() -> XForma.Primitives.ItemContainer` | Creates the container for an item's view. Control libraries override this to add behavior to every item, such as selection or swipe gestures. |
| `DefaultAccessibilityRole.get -> XForma.AccessibilityRole` |  |
| `GetUserState() -> object` |  |
| `IsRefreshEnabled.get -> bool` | Whether pulling the view down past its top refreshes it (with touch; apps can also call `RequestRefresh`). |
| `IsRefreshEnabled.set -> void` | Whether pulling the view down past its top refreshes it (with touch; apps can also call `RequestRefresh`). |
| `IsRefreshing.get -> bool` | Whether a refresh is under way, which shows the refresh indicator. |
| `IsRefreshing.set -> void` | Whether a refresh is under way, which shows the refresh indicator. |
| `ItemsView() -> void` |  |
| `Layout.get -> XForma.Primitives.ListLayout` | How the items are laid out: a vertical list (the default), a horizontal one, or a grid. |
| `Layout.set -> void` | How the items are laid out: a vertical list (the default), a horizontal one, or a grid. |
| `OnContainerBound(XForma.Primitives.ItemContainer container) -> void` | Called when a container starts showing an item (it may be recycled). |
| `OnHotReload() -> void` |  |
| `OnKeyDown(XForma.KeyEventArgs e) -> void` | The arrow keys move between the items (in a grid, all four), Home and End go to the ends, and Page Up and Page Down a page at a time, scrolling the item into view. Items that aren't focusable (a group header) are passed over. |
| `OnThemeChanged(XForma.Theme theme) -> void` |  |
| `Padding.get -> XForma.Thickness` | Space around the items, inside the scrolling area: the items start after the leading padding, and the last can scroll clear of whatever covers the trailing edge (such as a floating bar). |
| `Padding.set -> void` | Space around the items, inside the scrolling area: the items start after the leading padding, and the last can scroll clear of whatever covers the trailing edge (such as a floating bar). |
| `RealizedContainers.get -> System.Collections.Generic.IReadOnlyCollection<XForma.Primitives.ItemContainer>` | The containers the platform has made (for items in view, or set aside to be reused). |
| `RefreshRequested -> System.EventHandler` | Raised when the user pulls the view down past its top (with `IsRefreshEnabled` set). `IsRefreshing` is true until the app sets it back to false when it's done. |
| `RequestRefresh() -> void` | Refreshes as a pull would: shows the indicator and raises `RefreshRequested`. |
| `ScrollToItem(object item) -> void` |  |
| `SetUserState(object state) -> void` |  |

## ItemsViewSource

| Member | |
|---|---|
| `CollectionChanged -> System.Collections.Specialized.NotifyCollectionChangedEventHandler` |  |
| `Count.get -> int` |  |
| `GetEnumerator() -> System.Collections.IEnumerator` |  |
| `GetItemContainer(XForma.Primitives.ItemsView itemsView, XForma.Primitives.ItemContainer reuseContainer, object item) -> XForma.Primitives.ItemContainer` |  |
| `GetItemView(XForma.View reuseView, object item) -> XForma.View` | The view for an item: the view it reuses (made for an earlier item), bound to this item, or a new one. |
| `IndexOf(object item) -> int` |  |
| `ItemsViewSource() -> void` |  |
| `NotifyCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) -> void` |  |
| `this[int index].get -> object` |  |

## KeyboardDismissMode

Whether scrolling a `ScrollViewer` hides the soft keyboard.

| Member | |
|---|---|
| `Interactive = 2 -> XForma.Primitives.KeyboardDismissMode` | The keyboard follows a drag down and hides if dragged off (iOS); elsewhere, as `OnDrag`. |
| `None = 0 -> XForma.Primitives.KeyboardDismissMode` | Scrolling leaves the keyboard up. |
| `OnDrag = 1 -> XForma.Primitives.KeyboardDismissMode` | Dragging the content ends text entry, hiding the keyboard. |

## KeyboardOptions

| Member | |
|---|---|
| `All = XForma.Primitives.KeyboardOptions.SpellCheck \| XForma.Primitives.KeyboardOptions.Suggestions -> XForma.Primitives.KeyboardOptions` |  |
| `None = 0 -> XForma.Primitives.KeyboardOptions` |  |
| `SpellCheck = 1 -> XForma.Primitives.KeyboardOptions` |  |
| `Suggestions = 2 -> XForma.Primitives.KeyboardOptions` |  |

## ListLayout

How a list lays its items out: in a vertical or horizontal line, or a grid of columns that scrolls vertically.

| Member | |
|---|---|
| `Columns.get -> int` | The number of items across: 1 for a list. |
| `Grid(int columns) -> XForma.Primitives.ListLayout` | A grid with the number of items across, each as wide as the list over the columns. |
| `Horizontal.get -> XForma.Primitives.ListLayout` |  |
| `IsGrid.get -> bool` |  |
| `Orientation.get -> XForma.Orientation` | The direction the list scrolls. |
| `Vertical.get -> XForma.Primitives.ListLayout` |  |

## NativeViewHost

Shows one of the platform's own views among XForma's, for what XForma doesn't draw itself, such as a map or a media player. It takes the room layout gives it (set its `Width` and `Height`, or let a panel stretch it).

| Member | |
|---|---|
| `NativeView.get -> object` | The platform's view, or null before it's created. |
| `NativeViewHost(System.Func<object> createNativeView) -> void` |  |

## Popup

Shows a view above the page until it is closed: centered in the window, or below (or, without room, above) its `PlacementTarget`. The popup draws nothing of its own, so control libraries give it a look (a background, border or shadow) through its content. Clicking outside the popup, pressing Escape or going back closes it.

| Member | |
|---|---|
| `Close() -> void` |  |
| `Closed -> System.EventHandler` | Raised when the popup closes, however it was closed. |
| `Content.get -> XForma.View` |  |
| `Content.set -> void` |  |
| `GetDefaultScrimColor(XForma.Theme theme) -> XForma.Color` | The scrim when `ScrimColor` is null, for the theme the popup shows in: none (transparent) for a popup. |
| `IsLightDismissEnabled.get -> bool` | Whether the popup is light-dismissed (the default): a click or tap outside it, Escape or Back closes it, and keyboard focus stays inside it. Otherwise it only shows content beside its target (a suggestion list): focus stays where it was, and input outside it goes to the app, which closes it. |
| `IsLightDismissEnabled.set -> void` | Whether the popup is light-dismissed (the default): a click or tap outside it, Escape or Back closes it, and keyboard focus stays inside it. Otherwise it only shows content beside its target (a suggestion list): focus stays where it was, and input outside it goes to the app, which closes it. |
| `IsOpen.get -> bool` |  |
| `OnClosed() -> void` |  |
| `PlacementTarget.get -> XForma.View` |  |
| `PlacementTarget.set -> void` |  |
| `Popup() -> void` |  |
| `ScrimColor.get -> XForma.Color?` | A color laid over the rest of the window while the popup is open, such as a dim black, which a press on dismisses it as before; transparent (the default) for none. |
| `ScrimColor.set -> void` | A color laid over the rest of the window while the popup is open, such as a dim black, which a press on dismisses it as before; transparent (the default) for none. |
| `Show() -> void` |  |

## Run

A piece of a `TextBlock`'s text with its own style. Unset values keep the text block's own.

| Member | |
|---|---|
| `FontStyle.get -> XForma.FontStyle?` |  |
| `FontStyle.init -> void` |  |
| `FontWeight.get -> XForma.FontWeight?` |  |
| `FontWeight.init -> void` |  |
| `Foreground.get -> XForma.Color?` |  |
| `Foreground.init -> void` |  |
| `Run() -> void` |  |
| `Run(string text) -> void` |  |
| `Strikethrough.get -> bool` |  |
| `Strikethrough.init -> void` |  |
| `Text.get -> string` |  |
| `Text.init -> void` |  |
| `Underline.get -> bool` |  |
| `Underline.init -> void` |  |

## ScrollViewer

| Member | |
|---|---|
| `Content.get -> XForma.View` |  |
| `Content.set -> void` |  |
| `GetUserState() -> object` |  |
| `IsHorizontalScrollingEnabled.get -> bool` |  |
| `IsHorizontalScrollingEnabled.set -> void` |  |
| `IsVerticalScrollingEnabled.get -> bool` |  |
| `IsVerticalScrollingEnabled.set -> void` |  |
| `KeyboardDismissMode.get -> XForma.Primitives.KeyboardDismissMode` |  |
| `KeyboardDismissMode.set -> void` |  |
| `ScrollToVerticalOffset(float verticalOffset, bool animated) -> void` | Scrolls the content so the given vertical offset is at the top (limited to the content's extent). |
| `ScrollViewer() -> void` |  |
| `SetUserState(object state) -> void` |  |
| `VerticalOffset.get -> float` | How far the content is scrolled down. |

## TextBlock

| Member | |
|---|---|
| `BackgroundColor.get -> XForma.Color` |  |
| `BackgroundColor.set -> void` |  |
| `DefaultAccessibilityLabel.get -> string` |  |
| `DefaultAccessibilityRole.get -> XForma.AccessibilityRole` |  |
| `FontFamily.get -> string` | The font family: a generic family from `FontFamilies`, a family registered with `RegisterFont`, or an installed family's name. Null is the platform's UI font. |
| `FontFamily.set -> void` | The font family: a generic family from `FontFamilies`, a family registered with `RegisterFont`, or an installed family's name. Null is the platform's UI font. |
| `FontSize.get -> float` |  |
| `FontSize.set -> void` |  |
| `FontStyle.get -> XForma.FontStyle` |  |
| `FontStyle.set -> void` |  |
| `FontWeight.get -> XForma.FontWeight` |  |
| `FontWeight.set -> void` |  |
| `Foreground.get -> XForma.Color` |  |
| `Foreground.set -> void` |  |
| `Inlines.get -> System.Collections.Generic.IList<XForma.Primitives.Run>` | Text made of runs with their own styles, as an alternative to `Text`: adding runs replaces the text with theirs. |
| `MaxLines.get -> int` | The most lines shown (0, the default, for no limit); text cut off ends with an ellipsis. |
| `MaxLines.set -> void` | The most lines shown (0, the default, for no limit); text cut off ends with an ellipsis. |
| `OnThemeChanged(XForma.Theme theme) -> void` |  |
| `Strikethrough.get -> bool` |  |
| `Strikethrough.set -> void` |  |
| `Text.get -> string` |  |
| `Text.set -> void` |  |
| `TextAlignment.get -> XForma.TextAlignment` |  |
| `TextAlignment.set -> void` |  |
| `TextBlock() -> void` |  |
| `TextTrimming.get -> XForma.TextTrimming` |  |
| `TextTrimming.set -> void` |  |
| `TextWrapping.get -> XForma.TextWrapping` |  |
| `TextWrapping.set -> void` |  |
| `VerticalTextAlignment.get -> XForma.TextAlignment` |  |
| `VerticalTextAlignment.set -> void` |  |

## TextBox

| Member | |
|---|---|
| `AccentColor.get -> XForma.Color` | The caret's color: by default, the theme's accent color. |
| `AccentColor.set -> void` | The caret's color: by default, the theme's accent color. |
| `AcceptsReturn.get -> bool` |  |
| `AcceptsReturn.set -> void` |  |
| `BackgroundColor.get -> XForma.Color` | The text box's background: by default, the theme's surface color. |
| `BackgroundColor.set -> void` | The text box's background: by default, the theme's surface color. |
| `CornerRadius.get -> float` | How round the box's corners are: 6 by default. While the box has focus, a border in its `AccentColor` shows inside its edges, following them. |
| `CornerRadius.set -> void` | How round the box's corners are: 6 by default. While the box has focus, a border in its `AccentColor` shows inside its edges, following them. |
| `DefaultAccessibilityLabel.get -> string` |  |
| `DefaultAccessibilityRole.get -> XForma.AccessibilityRole` |  |
| `DefaultFontSize.get -> float` | The size of a text box's text on this platform, before the application's `TextScale`: 17 (iOS's body text), Android's edit text size, or 15 on the desktop and the web. Fields that sit beside text boxes (pickers) use it, so they match. |
| `DefaultLineHeight.get -> float` | The height of a single-line text box's line on this platform, at the current text scale, without its padding; or 0 where it's the line text of `DefaultFontSize` measures. Fields beside text boxes size themselves with it, so they're the same height. |
| `DefaultPadding -> XForma.Thickness` | The space around the text inside the box: 12 at the sides and 11 above and below by default, so a single line is a comfortable touch target (about 44 high). |
| `Focus() -> void` |  |
| `ForegroundColor.get -> XForma.Color` | The text's color: by default, the theme's color for text on its surface. |
| `ForegroundColor.set -> void` | The text's color: by default, the theme's color for text on its surface. |
| `GetUserState() -> object` |  |
| `IsPassword.get -> bool` | Shows the text as dots (as WPF's and WinUI's `PasswordBox` and MAUI's `Entry.IsPassword`). A password box is single-line and offers no suggestions. |
| `IsPassword.set -> void` | Shows the text as dots (as WPF's and WinUI's `PasswordBox` and MAUI's `Entry.IsPassword`). A password box is single-line and offers no suggestions. |
| `IsReadOnly.get -> bool` | Whether the user can select and copy the text but not change it. |
| `IsReadOnly.set -> void` | Whether the user can select and copy the text but not change it. |
| `KeyboardOptions.get -> XForma.Primitives.KeyboardOptions` |  |
| `KeyboardOptions.set -> void` |  |
| `MaxLength.get -> int` | The most characters the user can type or paste, or 0 (the default) for no limit. |
| `MaxLength.set -> void` | The most characters the user can type or paste, or 0 (the default) for no limit. |
| `OnAttached() -> void` |  |
| `OnAttaching(XForma.Theme theme) -> void` |  |
| `OnThemeChanged(XForma.Theme theme) -> void` |  |
| `Padding.get -> XForma.Thickness` | The space between the box's edges and its text (and placeholder): the box's background fills it, and a tap in it focuses the box. |
| `Padding.set -> void` | The space between the box's edges and its text (and placeholder): the box's background fills it, and a tap in it focuses the box. |
| `PlaceholderColor.get -> XForma.Color` | The placeholder text's color: by default, the theme's subtle foreground color. |
| `PlaceholderColor.set -> void` | The placeholder text's color: by default, the theme's subtle foreground color. |
| `PlaceholderText.get -> string` |  |
| `PlaceholderText.set -> void` |  |
| `Select(int start, int length) -> void` | Selects a range of the text (clamped to it); a length of 0 moves the caret. |
| `SelectAll() -> void` |  |
| `SelectedText.get -> string` | The selected text, or an empty string. |
| `SelectionChanged -> System.EventHandler` | Raised when the selection or the caret moves. |
| `SelectionLength.get -> int` |  |
| `SelectionLength.set -> void` |  |
| `SelectionStart.get -> int` | Where the selection starts (the caret, when nothing is selected), in UTF-16 code units. |
| `SelectionStart.set -> void` | Where the selection starts (the caret, when nothing is selected), in UTF-16 code units. |
| `SetUserState(object state) -> void` |  |
| `Submitted -> System.EventHandler` | Raised when the user submits a single-line text box: Enter, or the on-screen keyboard's Done or Go key. A text box that accepts Return never submits. |
| `Text.get -> string` |  |
| `Text.set -> void` |  |
| `TextBox() -> void` |  |
| `TextChanged -> System.EventHandler` | Raised when the text changes, whether the user or code changed it. |

## WebView

Shows web pages, in the platform's own web view: WebView2 on Windows, `android.webkit.WebView` on Android, `WKWebView` on iOS and an `iframe` in the browser, where a site can refuse to appear in a frame. An Android app needs the `INTERNET` permission. Windows needs the WebView2 runtime, which comes with Windows 11; without it, the view stays empty and `FrameworkLog` says why.

| Member | |
|---|---|
| `CanGoBack.get -> bool` | Whether there's a page to go back to. |
| `CanGoForward.get -> bool` | Whether there's a page to go forward to. |
| `GoBack() -> void` |  |
| `GoForward() -> void` |  |
| `NavigateToString(string html) -> void` | Shows the HTML, with no address of its own. |
| `Navigated -> System.EventHandler<XForma.Primitives.WebViewNavigatedEventArgs>` | Raised when a page has finished loading. |
| `Reload() -> void` |  |
| `Source.get -> System.Uri` | The page's address: setting it goes there, and it follows the user's navigation. |
| `Source.set -> void` | The page's address: setting it goes there, and it follows the user's navigation. |
| `WebView() -> void` |  |

## WebViewNavigatedEventArgs

What a `WebView` finished loading.

| Member | |
|---|---|
| `IsSuccess.get -> bool` | Whether it loaded. The browser doesn't tell a page in a frame what happened, so there it's always true. |
| `Uri.get -> System.Uri` | The page's address, or null for HTML given to `NavigateToString`. |
