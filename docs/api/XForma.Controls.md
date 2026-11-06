# XForma.Controls

## AppTheme

A theme package's entry, which an app passes to `XFormaApp.Run`: it gives the palette for each mode, and adds the package's tokens, styles and control templates to the theme the framework builds from it.

| Member | |
|---|---|
| `AppTheme() -> void` |  |
| `Apply(XForma.Theme theme) -> XForma.Theme` | The theme, with this package's tokens, styles and templates added. What the theme already has (an app's own) is kept. |
| `Create(XForma.ThemeMode mode, XForma.Color accentColor) -> XForma.Theme` | A whole theme for a mode, in this package's palette and look: for a section with a theme of its own (see `Theme`), whatever the app's mode. |
| `CreateColors(XForma.ThemeMode mode, XForma.Color accentColor) -> XForma.ColorScheme` | The package's colors for a mode, around an accent color. A package over another gives that one's, or its own. Where the system gives high-contrast colors, the application uses those instead, in that mode. |

## AutoSuggestBox

A text box offering suggestions as the user types: the app sets `ItemsSource` when the text changes (from the user's typing), and they show in a list below the box. Down and Up move through them, putting each in the box; Enter submits the text (or the suggestion), Escape goes back to what was typed and closes the list, and a click on a suggestion chooses and submits it.

| Member | |
|---|---|
| `AccessibilityIsExpanded.get -> bool?` |  |
| `AutoSuggestBox() -> void` |  |
| `Build() -> XForma.View` |  |
| `DefaultAccessibilityLabel.get -> string` |  |
| `DefaultAccessibilityRole.get -> XForma.AccessibilityRole` |  |
| `IsSuggestionListOpen.get -> bool` | Whether the list of suggestions is open. |
| `ItemsSource.get -> System.Collections.Generic.IReadOnlyList<string>` | The suggestions for the text, which the app sets as it changes. |
| `ItemsSource.set -> void` | The suggestions for the text, which the app sets as it changes. |
| `MaximumSuggestions = 8 -> int` | The most suggestions shown. |
| `OnKeyDown(XForma.KeyEventArgs keyInputEvent) -> void` |  |
| `PerformAccessibilityAction(XForma.AccessibilityAction action) -> bool` |  |
| `PlaceholderText.get -> string` | Shown while the box is empty; it also names the box for assistive technology. |
| `PlaceholderText.set -> void` | Shown while the box is empty; it also names the box for assistive technology. |
| `QuerySubmitted -> System.EventHandler<XForma.Controls.AutoSuggestBoxQuerySubmittedEventArgs>` | Raised when the user submits the text, or chooses a suggestion. |
| `SuggestionChosen -> System.EventHandler<XForma.Controls.AutoSuggestBoxSuggestionChosenEventArgs>` | Raised as the user moves to a suggestion (it's put in the box). |
| `SuggestionCount.get -> int` | The suggestions shown: the first `MaximumSuggestions`. |
| `Text.get -> string` |  |
| `Text.set -> void` |  |
| `TextChanged -> System.EventHandler<XForma.Controls.AutoSuggestBoxTextChangedEventArgs>` |  |
| `TextProperty -> XForma.ViewProperty<XForma.Controls.AutoSuggestBox, string>` |  |

## AutoSuggestBoxListView

An auto-suggest box's suggestions, in a list below it: a click chooses one. Its theme's template draws the rows, each `RowHeight` tall.

| Member | |
|---|---|
| `AutoSuggestBox.get -> XForma.Controls.AutoSuggestBox` |  |
| `HighlightedIndex.get -> int` | The suggestion the keyboard is on, or -1. |
| `PressedIndex.get -> int` | The suggestion being pressed, or -1. |
| `RowHeight.get -> float` |  |

## AutoSuggestBoxQuerySubmittedEventArgs

The user submitted an `AutoSuggestBox`'s text.

| Member | |
|---|---|
| `AutoSuggestBoxQuerySubmittedEventArgs(string queryText, string chosenSuggestion) -> void` |  |
| `ChosenSuggestion.get -> string` | The suggestion chosen, or null when the text was submitted with Enter. |
| `QueryText.get -> string` | The box's text. |

## AutoSuggestBoxSuggestionChosenEventArgs

The user chose one of an `AutoSuggestBox`'s suggestions.

| Member | |
|---|---|
| `AutoSuggestBoxSuggestionChosenEventArgs(string selectedItem) -> void` |  |
| `SelectedItem.get -> string` |  |

## AutoSuggestBoxTextChangedEventArgs

An `AutoSuggestBox`'s text changed.

| Member | |
|---|---|
| `AutoSuggestBoxTextChangedEventArgs(XForma.Controls.AutoSuggestionBoxTextChangeReason reason) -> void` |  |
| `Reason.get -> XForma.Controls.AutoSuggestionBoxTextChangeReason` |  |

## AutoSuggestionBoxTextChangeReason

Why an auto-suggest box's text changed.

| Member | |
|---|---|
| `ProgrammaticChange = 1 -> XForma.Controls.AutoSuggestionBoxTextChangeReason` | The app set `Text`. |
| `SuggestionChosen = 2 -> XForma.Controls.AutoSuggestionBoxTextChangeReason` | The user moved to or chose a suggestion. |
| `UserInput = 0 -> XForma.Controls.AutoSuggestionBoxTextChangeReason` | The user typed. |

## Border

Draws a background and a border, optionally with rounded corners, around one child, inset by the border's thickness and the padding.

| Member | |
|---|---|
| `Background.get -> XForma.Color` | The fill inside the border (transparent by default). |
| `Background.set -> void` | The fill inside the border (transparent by default). |
| `BackgroundColor.get -> XForma.Color` |  |
| `BackgroundProperty -> XForma.ViewProperty<XForma.Controls.Border, XForma.Color>` |  |
| `Border() -> void` |  |
| `BorderBrush.get -> XForma.Color` | The color of the border. |
| `BorderBrush.set -> void` | The color of the border. |
| `BorderBrushProperty -> XForma.ViewProperty<XForma.Controls.Border, XForma.Color>` |  |
| `BorderThickness.get -> XForma.Thickness` | The width of the border on each side. With rounded corners, the border is drawn with the left width on every side. |
| `BorderThickness.set -> void` | The width of the border on each side. With rounded corners, the border is drawn with the left width on every side. |
| `Child.get -> XForma.View` | The view inside the border. |
| `Child.set -> void` | The view inside the border. |
| `CornerRadius.get -> float` | The radius of the corners (0, square, by default). |
| `CornerRadius.set -> void` | The radius of the corners (0, square, by default). |
| `Padding.get -> XForma.Thickness` | Space between the border and the child. |
| `Padding.set -> void` | Space between the border and the child. |
| `Render(XForma.DrawingContext drawContext) -> void` |  |

## Button

A button with text, which raises `Clicked` and runs its `Command` when tapped. Its theme's template draws it.

| Member | |
|---|---|
| `Button() -> void` |  |
| `Clicked -> System.EventHandler` |  |
| `Command.get -> System.Windows.Input.ICommand` | The command the button runs when clicked. The button is enabled while the command can run. |
| `Command.set -> void` | The command the button runs when clicked. The button is enabled while the command can run. |
| `CommandParameter.get -> object` | The value passed to the command. |
| `CommandParameter.set -> void` | The value passed to the command. |
| `CommandProperty -> XForma.ViewProperty<XForma.Controls.Button, System.Windows.Input.ICommand>` |  |
| `DefaultAccessibilityLabel.get -> string` |  |
| `DefaultAccessibilityRole.get -> XForma.AccessibilityRole` |  |
| `IsAlertStyle.get -> bool` | Whether the button does something destructive, which its look warns of. |
| `IsAlertStyle.set -> void` | Whether the button does something destructive, which its look warns of. |
| `IsEnabledProperty -> XForma.ViewProperty<XForma.Controls.Button, bool>` |  |
| `Text.get -> string` |  |
| `Text.set -> void` |  |
| `TextProperty -> XForma.ViewProperty<XForma.Controls.Button, string>` |  |

## CalendarDatePicker

A field showing the selected date; clicking it opens a calendar below it.

| Member | |
|---|---|
| `CalendarDatePicker() -> void` |  |
| `ClosePicker() -> void` |  |
| `DefaultAccessibilityLabel.get -> string` |  |
| `DisplayText.get -> string` |  |
| `GetUserState() -> object` |  |
| `IsPickerOpen.get -> bool` |  |
| `IsPlaceholder.get -> bool` |  |
| `MaximumTextWidth.get -> float` |  |
| `MinimumDate.get -> System.DateTime` |  |
| `MinimumDate.set -> void` |  |
| `OnAttaching(XForma.Theme theme) -> void` |  |
| `OpenPicker() -> void` |  |
| `PlaceholderText.get -> string` | Shown while no date is selected. |
| `PlaceholderText.set -> void` | Shown while no date is selected. |
| `SelectedDate.get -> System.DateTime?` |  |
| `SelectedDate.set -> void` |  |
| `SelectedDateChanged -> System.EventHandler<System.DateTime?>` |  |
| `SelectedDateProperty -> XForma.ViewProperty<XForma.Controls.CalendarDatePicker, System.DateTime?>` |  |
| `SetUserState(object state) -> void` |  |

## CalendarView

A month calendar: a header with the month and arrows to change it, the days of the week, and six weeks of days (weeks start on Monday). Days before the minimum date can't be chosen. With keyboard focus, the arrow keys move between days, Page Up and Page Down change the month, and Enter or Space chooses the day. Its theme's template draws it, in the layout the calendar measures from the theme's `CalendarViewStyle`.

| Member | |
|---|---|
| `CalendarView() -> void` |  |
| `CanShowPreviousMonth.get -> bool` | Whether there's an earlier month to show. |
| `DefaultAccessibilityLabel.get -> string` |  |
| `DefaultAccessibilityRole.get -> XForma.AccessibilityRole` |  |
| `DisplayedMonth.get -> System.DateTime` | The month shown (its first day). |
| `FirstShownDay.get -> System.DateTime` | The first day shown: the Monday on or before the first of the month. |
| `FocusedDate.get -> System.DateTime` | The day the keyboard is on. |
| `GetDayBounds(int row, int column) -> XForma.Rect` | Where the day in the row and column (from 0) is: the days start with `FirstShownDay`. |
| `GetWeekdayBounds(int column) -> XForma.Rect` | Where the name of the day of the week in the column (from 0, Monday) is. |
| `HeaderBounds.get -> XForma.Rect` | Where the month's name is, in the header. |
| `IsDateEnabled(System.DateTime date) -> bool` | Whether the day can be chosen (it's on or after the minimum date). |
| `MeasureOverride(XForma.Size availableSize) -> XForma.Size` |  |
| `MinimumDate.get -> System.DateTime` |  |
| `MinimumDate.set -> void` |  |
| `NextButtonBounds.get -> XForma.Rect` | Where the button for the next month is. |
| `OnActivate() -> void` |  |
| `OnGotFocus() -> void` |  |
| `OnKeyDown(XForma.KeyEventArgs keyInputEvent) -> void` |  |
| `OnLostFocus() -> void` |  |
| `OnPointerCanceled(XForma.PointerEventArgs e) -> void` |  |
| `OnPointerPressed(XForma.PointerEventArgs e) -> void` |  |
| `OnPointerReleased(XForma.PointerEventArgs e) -> void` |  |
| `OnThemeChanged(XForma.Theme theme) -> void` |  |
| `PressedDate.get -> System.DateTime` | The day being pressed, when `PressedPart` is a day. |
| `PressedPart.get -> XForma.Controls.CalendarViewPart` | The part of the calendar being pressed. |
| `PreviousButtonBounds.get -> XForma.Rect` | Where the button for the previous month is. |
| `SelectedDate.get -> System.DateTime?` |  |
| `SelectedDate.set -> void` |  |
| `SelectedDateChanged -> System.EventHandler<System.DateTime?>` | Raised when the user chooses a day. |

## CalendarViewPart

A part of a `CalendarView` that can be pressed.

| Member | |
|---|---|
| `Day = 3 -> XForma.Controls.CalendarViewPart` |  |
| `NextMonth = 2 -> XForma.Controls.CalendarViewPart` |  |
| `None = 0 -> XForma.Controls.CalendarViewPart` |  |
| `PreviousMonth = 1 -> XForma.Controls.CalendarViewPart` |  |

## CalendarViewStyle

How a theme lays out a `CalendarView`: a header with the month and its arrows, the days of the week, and six weeks of days.

| Member | |
|---|---|
| `<Clone>$() -> XForma.Controls.CalendarViewStyle` |  |
| `CalendarViewStyle() -> void` |  |
| `CellSize.get -> float` | The size of a day's cell, and the width of an arrow button. |
| `CellSize.init -> void` | The size of a day's cell, and the width of an arrow button. |
| `Equals(XForma.Controls.CalendarViewStyle other) -> bool` |  |
| `Equals(object obj) -> bool` |  |
| `GetHashCode() -> int` |  |
| `HeaderHeight.get -> float` | The height of the header with the month and its arrows. |
| `HeaderHeight.init -> void` | The height of the header with the month and its arrows. |
| `HeaderInset.get -> float` | The space before the month's name. |
| `HeaderInset.init -> void` | The space before the month's name. |
| `Inset.get -> float` | The space around the calendar. |
| `Inset.init -> void` | The space around the calendar. |
| `ToString() -> string` |  |
| `WeekdayHeight.get -> float` | The height of the row with the days of the week. |
| `WeekdayHeight.init -> void` | The height of the row with the days of the week. |
| `operator !=(XForma.Controls.CalendarViewStyle left, XForma.Controls.CalendarViewStyle right) -> bool` |  |
| `operator ==(XForma.Controls.CalendarViewStyle left, XForma.Controls.CalendarViewStyle right) -> bool` |  |

## CheckBox

A box and its text, checked or unchecked by a tap, Space or Enter, and, when `IsThreeState`, also indeterminate (neither): it goes from unchecked to checked to indeterminate. Its theme's template draws it, in the layout the check box measures from the theme's `CheckBoxStyle`: a box at the start, then the text.

| Member | |
|---|---|
| `AccessibilityIsChecked.get -> bool?` |  |
| `AccessibilityIsMixed.get -> bool` |  |
| `BoxBounds.get -> XForma.Rect` | Where the box is: at the start, centered vertically. |
| `CheckBox() -> void` |  |
| `CreateTextFormat() -> XForma.TextFormat` | The text's format, which the check box measures with. |
| `DefaultAccessibilityLabel.get -> string` |  |
| `DefaultAccessibilityRole.get -> XForma.AccessibilityRole` |  |
| `GetUserState() -> object` |  |
| `IsAccessibilityActionSupported(XForma.AccessibilityAction action) -> bool` |  |
| `IsChecked.get -> bool?` | Checked (true), unchecked (false) or indeterminate (null). |
| `IsChecked.set -> void` | Checked (true), unchecked (false) or indeterminate (null). |
| `IsCheckedChanged -> System.EventHandler` | Raised when `IsChecked` changes. |
| `IsCheckedProperty -> XForma.ViewProperty<XForma.Controls.CheckBox, bool?>` |  |
| `IsThreeState.get -> bool` | Whether a tap can make the box indeterminate. |
| `IsThreeState.set -> void` | Whether a tap can make the box indeterminate. |
| `MeasureOverride(XForma.Size availableSize) -> XForma.Size` |  |
| `OnActivate() -> void` |  |
| `OnAttaching(XForma.Theme theme) -> void` |  |
| `SetUserState(object state) -> void` |  |
| `Text.get -> string` |  |
| `Text.set -> void` |  |
| `TextBounds.get -> XForma.Rect` | Where the text is: after the box. |

## CheckBoxStyle

How a theme lays out a `CheckBox`: a box at the start, then its text.

| Member | |
|---|---|
| `<Clone>$() -> XForma.Controls.CheckBoxStyle` |  |
| `BoxSize.get -> float` | The box's size. |
| `BoxSize.init -> void` | The box's size. |
| `CheckBoxStyle() -> void` |  |
| `Equals(XForma.Controls.CheckBoxStyle other) -> bool` |  |
| `Equals(object obj) -> bool` |  |
| `GetHashCode() -> int` |  |
| `MinimumHeight.get -> float` | The check box's least height, which takes presses. |
| `MinimumHeight.init -> void` | The check box's least height, which takes presses. |
| `TextGap.get -> float` | The space between the box and the text. |
| `TextGap.init -> void` | The space between the box and the text. |
| `ToString() -> string` |  |
| `operator !=(XForma.Controls.CheckBoxStyle left, XForma.Controls.CheckBoxStyle right) -> bool` |  |
| `operator ==(XForma.Controls.CheckBoxStyle left, XForma.Controls.CheckBoxStyle right) -> bool` |  |

## ComboBox

A field showing the selected item; clicking it opens a list of the items below it. Up and Down change the selection without opening the list.

| Member | |
|---|---|
| `ClosePicker() -> void` |  |
| `ComboBox() -> void` |  |
| `DefaultAccessibilityLabel.get -> string` |  |
| `DisplayText.get -> string` |  |
| `GetUserState() -> object` |  |
| `IsPickerOpen.get -> bool` |  |
| `Items.get -> XForma.Controls.ComboBoxItemCollection` |  |
| `MaximumTextWidth.get -> float` |  |
| `OnAttaching(XForma.Theme theme) -> void` |  |
| `OnKeyDown(XForma.KeyEventArgs keyInputEvent) -> void` |  |
| `OpenPicker() -> void` |  |
| `Select(int index) -> void` | Selects the item at the index (or nothing, for -1), and raises `SelectionChanged` when the selection changes. |
| `SelectedIndex.get -> int` |  |
| `SelectedIndex.set -> void` |  |
| `SelectedIndexProperty -> XForma.ViewProperty<XForma.Controls.ComboBox, int>` |  |
| `SelectedItem.get -> XForma.Controls.ComboBoxItem` |  |
| `SelectedItem.set -> void` |  |
| `SelectedItemProperty -> XForma.ViewProperty<XForma.Controls.ComboBox, XForma.Controls.ComboBoxItem>` |  |
| `SelectionChanged -> System.EventHandler<XForma.Controls.ComboBoxItem>` |  |
| `SetUserState(object state) -> void` |  |

## ComboBoxItem

An item in a `ComboBox`.

| Member | |
|---|---|
| `ComboBoxItem() -> void` |  |
| `ComboBoxItem(string text) -> void` |  |
| `PropertyChanged -> System.ComponentModel.PropertyChangedEventHandler` |  |
| `Text.get -> string` |  |
| `Text.set -> void` |  |
| `ToString() -> string` |  |
| `Value.get -> object` |  |
| `Value.set -> void` |  |

## ComboBoxItemCollection

The items of a `ComboBox`.

| Member | |
|---|---|
| `Add(XForma.Controls.ComboBoxItem listPickerItem) -> void` |  |
| `Clear() -> void` |  |
| `Count.get -> int` |  |
| `GetEnumerator() -> System.Collections.Generic.IEnumerator<XForma.Controls.ComboBoxItem>` |  |
| `IndexOf(XForma.Controls.ComboBoxItem item) -> int` |  |
| `this[int index].get -> XForma.Controls.ComboBoxItem` |  |

## ComboBoxItemsView

A combo box's list of items, in its popup: picks the row under a click; Up and Down move the highlight, and Enter or Space chooses it. Its theme's template draws the rows, each `RowHeight` tall.

| Member | |
|---|---|
| `ComboBox.get -> XForma.Controls.ComboBox` | The combo box whose items it shows. |
| `HighlightedIndex.get -> int` | The row the keyboard is on. |
| `PressedIndex.get -> int` | The row being pressed, or -1. |
| `RowHeight.get -> float` |  |

## CommandBar

A bar of command buttons.

| Member | |
|---|---|
| `CommandBar() -> void` |  |
| `CommandParameter.get -> object` | The value the bar's buttons pass to their commands, such as the list item the bar acts on. |
| `CommandParameter.set -> void` | The value the bar's buttons pass to their commands, such as the list item the bar acts on. |
| `MeasureOverride(XForma.Size availableSize) -> XForma.Size` |  |
| `OnThemeChanged(XForma.Theme theme) -> void` |  |

## CommandBarButton

A `CommandBar`'s button: an icon and a label, showing while its command can run. Its theme's template draws it.

| Member | |
|---|---|
| `Command.get -> System.Windows.Input.ICommand` |  |
| `Command.set -> void` |  |
| `CommandBarButton() -> void` |  |
| `CommandParameter.get -> object` | The value passed to the command. The button shows only while the command can run with it. |
| `CommandParameter.set -> void` | The value passed to the command. The button shows only while the command can run with it. |
| `Icon.get -> XForma.Bitmap` |  |
| `Icon.set -> void` |  |
| `Text.get -> string` |  |
| `Text.set -> void` |  |

## CommandBarCommandCollection

A command bar's commands: primary on one side, secondary on the other.

| Member | |
|---|---|
| `CommandBarCommandCollection() -> void` |  |
| `PrimaryCommands.get -> System.Collections.Generic.List<XForma.Controls.UICommand>` |  |
| `SecondaryCommands.get -> System.Collections.Generic.List<XForma.Controls.UICommand>` |  |

## CommandBarStyle

How a theme lays out a `CommandBar` (a command list's rows have one too).

| Member | |
|---|---|
| `<Clone>$() -> XForma.Controls.CommandBarStyle` |  |
| `ButtonHeight.get -> float` | The buttons' height. |
| `ButtonHeight.init -> void` | The buttons' height. |
| `ButtonSpacing.get -> float` | The space between neighbouring buttons' icons and labels. Each button's touch area takes its share of it, so a tap between two lands on the nearer. Without room for it across the bar, the buttons go side by side. |
| `ButtonSpacing.init -> void` | The space between neighbouring buttons' icons and labels. Each button's touch area takes its share of it, so a tap between two lands on the nearer. Without room for it across the bar, the buttons go side by side. |
| `ButtonWidth.get -> float` | The buttons' width, before their share of the spacing. |
| `ButtonWidth.init -> void` | The buttons' width, before their share of the spacing. |
| `CommandBarStyle() -> void` |  |
| `Equals(XForma.Controls.CommandBarStyle other) -> bool` |  |
| `Equals(object obj) -> bool` |  |
| `GetHashCode() -> int` |  |
| `LabelFontSize.get -> float?` | The labels' font size, or null (the default) for the template's. |
| `LabelFontSize.init -> void` | The labels' font size, or null (the default) for the template's. |
| `SwipeIconSize.get -> XForma.Size` | The size of the icon on a command list's swiped row. |
| `SwipeIconSize.init -> void` | The size of the icon on a command list's swiped row. |
| `ToString() -> string` |  |
| `operator !=(XForma.Controls.CommandBarStyle left, XForma.Controls.CommandBarStyle right) -> bool` |  |
| `operator ==(XForma.Controls.CommandBarStyle left, XForma.Controls.CommandBarStyle right) -> bool` |  |

## CommandListView

A list whose items reveal a command bar when tapped (one item at a time), and run a command when swiped sideways.

| Member | |
|---|---|
| `CommandBarCommands.get -> XForma.Controls.CommandBarCommandCollection` | The commands shown below a tapped item. |
| `CommandListView() -> void` |  |
| `CreateItemContainer() -> XForma.Controls.ListViewItem` |  |
| `SwipeCommands.get -> XForma.Controls.SwipeCommandCollection` | The commands run by swiping an item right (primary) or left (secondary). |

## CommandListViewItem

An item view in a `CommandListView`, which can show or hide its command bar.

| Member | |
|---|---|
| `CommandBarMargin.get -> XForma.Thickness` |  |
| `CommandBarMargin.set -> void` |  |
| `CommandListViewItem() -> void` |  |
| `IsCommandBarVisible.get -> bool` |  |
| `IsCommandBarVisible.set -> void` |  |
| `Item.get -> object` | The item the view shows. The list's item source sets it as the view is reused for item after item. Setting the item it already shows does nothing (`OnItemChanged` isn't called): to show an item's changes, have the item notify them. |
| `Item.set -> void` | The item the view shows. The list's item source sets it as the view is reused for item after item. Setting the item it already shows does nothing (`OnItemChanged` isn't called): to show an item's changes, have the item notify them. |
| `OnItemChanged() -> void` | Called when the view starts showing another item: bind it here. |

## CommandListViewItemContainer

The item container of a `CommandListView`: reveals the list's command bar below the item when tapped, and runs a swipe command when the item is swiped sideways.

| Member | |
|---|---|
| `AddContent(XForma.View content) -> void` |  |
| `CommandBarMargin.get -> XForma.Thickness` |  |
| `CommandBarMargin.set -> void` |  |
| `CommandListViewItemContainer() -> void` |  |
| `IsCommandBarVisible.get -> bool` |  |
| `IsCommandBarVisible.set -> void` |  |
| `OnBind() -> void` |  |
| `OnRecycle() -> void` |  |
| `OnTapped() -> void` |  |
| `OnThemeChanged(XForma.Theme theme) -> void` |  |
| `RemoveContent(XForma.View content) -> void` |  |

## ContentControl

A control with one child, its content, inside its padding.

| Member | |
|---|---|
| `Content.get -> XForma.View` |  |
| `Content.set -> void` |  |
| `ContentControl() -> void` |  |
| `Padding.get -> XForma.Thickness` | Space between the control's edges and its content. |
| `Padding.set -> void` | Space between the control's edges and its content. |

## Control

Base for controls: a view with a background.

| Member | |
|---|---|
| `BackgroundColor.get -> XForma.Color` | The control's background: transparent unless the control gives it one. Owner-drawn controls paint it in their drawing. |
| `Control() -> void` |  |
| `GetBackgroundColor() -> XForma.Color` |  |

## ControlTemplate

How a kind of control looks: its drawing, the child views (parts) it adds, its layout of them, and its response to state changes. A theme holds one for each control type (see `ThemeTemplates`); a control uses its theme's.

| Member | |
|---|---|
| `ControlType.get -> System.Type` | The type of control the template is for. |

## ControlTemplate<TControl>

| Member | |
|---|---|
| `Apply(TControl control, XForma.Controls.TemplateContext context) -> void` |  |
| `Arrange(TControl control, XForma.Size finalSize) -> bool` |  |
| `Background(TControl control) -> XForma.Color?` |  |
| `ControlTemplate() -> void` |  |
| `ControlType.get -> System.Type` |  |
| `Measure(TControl control, XForma.Size availableSize) -> XForma.Size?` |  |
| `OnStateChanged(TControl control, XForma.Controls.TemplateContext context) -> void` |  |
| `OnThemeChanged(TControl control, XForma.Controls.TemplateContext context) -> void` |  |
| `Render(TControl control, XForma.DrawingContext drawingContext) -> void` |  |
| `Unapply(TControl control, XForma.Controls.TemplateContext context) -> void` |  |

## ControlTemplates

A theme's templates, by control type: the theme extension that a theme package (such as XForma.Themes.Basic) fills, and controls read through `ThemeTemplates`.

| Member | |
|---|---|
| `<Clone>$() -> XForma.Controls.ControlTemplates` |  |
| `ControlTemplates() -> void` |  |
| `Empty.get -> XForma.Controls.ControlTemplates` |  |
| `Equals(XForma.Controls.ControlTemplates other) -> bool` |  |
| `Equals(object obj) -> bool` |  |
| `Get(System.Type controlType) -> XForma.Controls.ControlTemplate` | The template for the control type, or for its nearest base type that has one (a subclass of a control looks like it); or null. |
| `GetHashCode() -> int` |  |
| `ToString() -> string` |  |
| `With(XForma.Controls.ControlTemplate template) -> XForma.Controls.ControlTemplates` | The templates, with this one for its control type (replacing any). |
| `operator !=(XForma.Controls.ControlTemplates left, XForma.Controls.ControlTemplates right) -> bool` |  |
| `operator ==(XForma.Controls.ControlTemplates left, XForma.Controls.ControlTemplates right) -> bool` |  |

## DayOfWeekToggleButton

A `DaysOfWeekPicker`'s button for one day. Its theme's template draws it.

| Member | |
|---|---|
| `DaysOfWeekPicker.get -> XForma.Controls.DaysOfWeekPicker` |  |
| `IsChecked.get -> bool` |  |
| `IsChecked.set -> void` |  |
| `IsCheckedChanged -> System.EventHandler<XForma.Controls.ToggledEventArgs>` |  |
| `Text.get -> string` |  |
| `Text.set -> void` |  |

## DaysOfWeekPicker

Chooses days of the week.

| Member | |
|---|---|
| `DaysOfWeekPicker() -> void` |  |
| `OnThemeChanged(XForma.Theme theme) -> void` |  |
| `SelectedDaysOfWeek.get -> System.DayOfWeek[]` |  |
| `SelectedDaysOfWeek.set -> void` |  |

## DaysOfWeekPickerStyle

How a theme lays out a `DaysOfWeekPicker`.

| Member | |
|---|---|
| `<Clone>$() -> XForma.Controls.DaysOfWeekPickerStyle` |  |
| `ButtonSize.get -> float` | A day button's size, and the picker's height. |
| `ButtonSize.init -> void` | A day button's size, and the picker's height. |
| `DaysOfWeekPickerStyle() -> void` |  |
| `Equals(XForma.Controls.DaysOfWeekPickerStyle other) -> bool` |  |
| `Equals(object obj) -> bool` |  |
| `GetHashCode() -> int` |  |
| `MaximumWidth.get -> float` | The widest the buttons spread. |
| `MaximumWidth.init -> void` | The widest the buttons spread. |
| `ToString() -> string` |  |
| `operator !=(XForma.Controls.DaysOfWeekPickerStyle left, XForma.Controls.DaysOfWeekPickerStyle right) -> bool` |  |
| `operator ==(XForma.Controls.DaysOfWeekPickerStyle left, XForma.Controls.DaysOfWeekPickerStyle right) -> bool` |  |

## Expander

A header that shows and hides the content beneath it: a tap, Enter or Space on the header expands or collapses it, and the content fades and slides in as it expands. Its theme's templates draw the expander's frame and its header.

| Member | |
|---|---|
| `Collapsed -> System.EventHandler` | Raised when the content is hidden. |
| `Content.get -> XForma.View` | What shows beneath the header while expanded. |
| `Content.set -> void` | What shows beneath the header while expanded. |
| `Expander() -> void` |  |
| `Expanding -> System.EventHandler` | Raised as the content starts to show. |
| `GetUserState() -> object` |  |
| `Header.get -> string` | The header's text, which also names the expander for assistive technology. |
| `Header.set -> void` | The header's text, which also names the expander for assistive technology. |
| `IsExpanded.get -> bool` |  |
| `IsExpanded.set -> void` |  |
| `IsExpandedProperty -> XForma.ViewProperty<XForma.Controls.Expander, bool>` |  |
| `OnAttaching(XForma.Theme theme) -> void` |  |
| `OnThemeChanged(XForma.Theme theme) -> void` |  |
| `SetUserState(object state) -> void` |  |

## ExpanderHeader

An expander's header: its text and a chevron, toggling the expander. Its theme's template draws it.

| Member | |
|---|---|
| `AccessibilityIsExpanded.get -> bool?` |  |
| `CreateTextFormat() -> XForma.TextFormat` |  |
| `Expander.get -> XForma.Controls.Expander` |  |
| `PerformAccessibilityAction(XForma.AccessibilityAction action) -> bool` |  |

## ExpanderStyle

How a theme lays out an `Expander`, and how its content arrives.

| Member | |
|---|---|
| `<Clone>$() -> XForma.Controls.ExpanderStyle` |  |
| `ContentPadding.get -> XForma.Thickness` | The space around the content. |
| `ContentPadding.init -> void` | The space around the content. |
| `Equals(XForma.Controls.ExpanderStyle other) -> bool` |  |
| `Equals(object obj) -> bool` |  |
| `ExpanderStyle() -> void` |  |
| `GetHashCode() -> int` |  |
| `HeaderChromeWidth.get -> float` | The space beside the header's text when it sizes to it, which holds the chevron. |
| `HeaderChromeWidth.init -> void` | The space beside the header's text when it sizes to it, which holds the chevron. |
| `HeaderHeight.get -> float` | The header's height. |
| `HeaderHeight.init -> void` | The header's height. |
| `RevealOffset.get -> float` | How far above its place the content starts as it slides and fades in. |
| `RevealOffset.init -> void` | How far above its place the content starts as it slides and fades in. |
| `ToString() -> string` |  |
| `operator !=(XForma.Controls.ExpanderStyle left, XForma.Controls.ExpanderStyle right) -> bool` |  |
| `operator ==(XForma.Controls.ExpanderStyle left, XForma.Controls.ExpanderStyle right) -> bool` |  |

## FlipView

Views shown one at a time, side by side: a horizontal swipe moves to the next or previous (following the finger, then settling), as do its arrow buttons and, with keyboard focus, Left and Right (following the flow direction), Page Up, Page Down, Home and End. Its theme's templates draw its arrow buttons.

| Member | |
|---|---|
| `AccessibilityValue.get -> string` |  |
| `ArrangeOverride(XForma.Size finalSize) -> void` |  |
| `DefaultAccessibilityRole.get -> XForma.AccessibilityRole` |  |
| `FlipView() -> void` |  |
| `GetUserState() -> object` |  |
| `Items.get -> XForma.Controls.FlipViewItemCollection` | The views, in order. |
| `MeasureOverride(XForma.Size availableSize) -> XForma.Size` |  |
| `OnAttaching(XForma.Theme theme) -> void` |  |
| `OnKeyDown(XForma.KeyEventArgs keyInputEvent) -> void` |  |
| `Select(int index, bool animated) -> void` | Shows the view at the index (kept within the items), sliding to it when animated. |
| `SelectedIndex.get -> int` | The view shown (its place in `Items`). |
| `SelectedIndex.set -> void` | The view shown (its place in `Items`). |
| `SelectedIndexProperty -> XForma.ViewProperty<XForma.Controls.FlipView, int>` |  |
| `SelectionChanged -> System.EventHandler` | Raised when the shown view changes. |
| `SetUserState(object state) -> void` |  |

## FlipViewButton

A flip view's arrow button, to its previous or next view. Its theme's template draws it.

| Member | |
|---|---|
| `Direction.get -> int` | -1 for the previous view, 1 for the next. |
| `IsAccessibilityActionSupported(XForma.AccessibilityAction action) -> bool` |  |
| `PerformAccessibilityAction(XForma.AccessibilityAction action) -> bool` |  |

## FlipViewItemCollection

A flip view's views: added to it as they're added here.

| Member | |
|---|---|
| `Add(XForma.View item) -> void` |  |
| `Count.get -> int` |  |
| `GetEnumerator() -> System.Collections.Generic.IEnumerator<XForma.View>` |  |
| `this[int index].get -> XForma.View` |  |

## FlipViewStyle

How a theme lays out a `FlipView`.

| Member | |
|---|---|
| `<Clone>$() -> XForma.Controls.FlipViewStyle` |  |
| `ButtonMaximumHeight.get -> float` | The tallest the previous and next buttons grow. |
| `ButtonMaximumHeight.init -> void` | The tallest the previous and next buttons grow. |
| `ButtonWidth.get -> float` | The width of the previous and next buttons. |
| `ButtonWidth.init -> void` | The width of the previous and next buttons. |
| `DefaultSize.get -> XForma.Size` | The view's size when it's offered any. |
| `DefaultSize.init -> void` | The view's size when it's offered any. |
| `Equals(XForma.Controls.FlipViewStyle other) -> bool` |  |
| `Equals(object obj) -> bool` |  |
| `FlipViewStyle() -> void` |  |
| `GetHashCode() -> int` |  |
| `ToString() -> string` |  |
| `operator !=(XForma.Controls.FlipViewStyle left, XForma.Controls.FlipViewStyle right) -> bool` |  |
| `operator ==(XForma.Controls.FlipViewStyle left, XForma.Controls.FlipViewStyle right) -> bool` |  |

## FloatingButton

A round button with an icon, floating over the content, which can also be swiped. Its theme's template draws it.

| Member | |
|---|---|
| `Clicked -> System.EventHandler` |  |
| `DefaultAccessibilityRole.get -> XForma.AccessibilityRole` | The button shows only an icon: give it an `AccessibilityLabel`. |
| `FloatingButton() -> void` |  |
| `Icon.get -> XForma.Bitmap` |  |
| `Icon.set -> void` |  |
| `IsSubtleStyle.get -> bool` |  |
| `IsSubtleStyle.set -> void` |  |
| `OnTapBegan() -> void` |  |
| `OnTapEnded() -> void` |  |
| `SwipeRecognizer.get -> XForma.Input.SwipeGestureRecognizer` | The button's swipe: its `Swiping` is for content that follows the finger, such as `FollowSwipe`. |
| `Swiped -> System.EventHandler<int>` | The button was swiped sideways and released: 1 to the right, -1 to the left. |

## Flyout

A popup with its content in a frame (a `PopupFrame`, which its theme's template draws). Clicking outside it, pressing Escape or going back closes it.

| Member | |
|---|---|
| `Content.get -> XForma.View` | The view shown inside the popover's frame. |
| `Content.set -> void` | The view shown inside the popover's frame. |
| `Flyout() -> void` |  |
| `GetDefaultScrimColor(XForma.Theme theme) -> XForma.Color` | A flyout dims the rest of the window, with its theme's scrim color. |
| `ShowAsync() -> void` | Shows the popover. (The popover stays open until it is closed.) |

## FlyoutButton

A floating button that opens other buttons around it.

| Member | |
|---|---|
| `AddButton(XForma.Controls.FloatingButton button) -> void` |  |
| `BackgroundColor.get -> XForma.Color` |  |
| `ButtonSpacing.get -> float?` | Whether the button is in the subtle style while its flyout is closed (the default), or the accent color, as an action button. Open, it's always subtle. |
| `ButtonSpacing.set -> void` | Whether the button is in the subtle style while its flyout is closed (the default), or the accent color, as an action button. Open, it's always subtle. |
| `Collapsing -> System.EventHandler` |  |
| `Expanding -> System.EventHandler` |  |
| `FlyoutButton() -> void` |  |
| `Icon.get -> XForma.Bitmap` |  |
| `Icon.set -> void` |  |
| `IsSubtleStyle.get -> bool` |  |
| `IsSubtleStyle.set -> void` |  |
| `OnLightDismiss() -> void` |  |

## FlyoutButtonStyle

How a theme moves a `FlyoutButton`: the button turning and shrinking as it opens, and its buttons fanning out on an arc.

| Member | |
|---|---|
| `<Clone>$() -> XForma.Controls.FlyoutButtonStyle` |  |
| `ArcCenter.get -> XForma.Angle` | The direction of the arc's middle, from the left (a quarter turn is straight up). |
| `ArcCenter.init -> void` | The direction of the arc's middle, from the left (a quarter turn is straight up). |
| `ArcSweep.get -> XForma.Angle` | How far an odd number of buttons spreads. |
| `ArcSweep.init -> void` | How far an odd number of buttons spreads. |
| `ButtonClearance.get -> float` | The least space between the button and its buttons. |
| `ButtonClearance.init -> void` | The least space between the button and its buttons. |
| `ButtonSpacing.get -> float` | The space between neighbouring buttons on the arc, unless the button sets its own. |
| `ButtonSpacing.init -> void` | The space between neighbouring buttons on the arc, unless the button sets its own. |
| `CollapsedButtonScale.get -> float` | The scale its buttons grow from and shrink back to. |
| `CollapsedButtonScale.init -> void` | The scale its buttons grow from and shrink back to. |
| `Equals(XForma.Controls.FlyoutButtonStyle other) -> bool` |  |
| `Equals(object obj) -> bool` |  |
| `EvenArcSweep.get -> XForma.Angle` | How far an even number of buttons spreads. |
| `EvenArcSweep.init -> void` | How far an even number of buttons spreads. |
| `FlyoutButtonStyle() -> void` |  |
| `GetHashCode() -> int` |  |
| `OpenRotation.get -> XForma.Angle` | How far the button turns as it opens. |
| `OpenRotation.init -> void` | How far the button turns as it opens. |
| `OpenScale.get -> float` | The button's scale while it's open. |
| `OpenScale.init -> void` | The button's scale while it's open. |
| `ToString() -> string` |  |
| `operator !=(XForma.Controls.FlyoutButtonStyle left, XForma.Controls.FlyoutButtonStyle right) -> bool` |  |
| `operator ==(XForma.Controls.FlyoutButtonStyle left, XForma.Controls.FlyoutButtonStyle right) -> bool` |  |

## HyperlinkButton

Content that opens a URI when tapped. Its theme's template gives it its feedback.

| Member | |
|---|---|
| `Content.get -> XForma.View` |  |
| `Content.set -> void` |  |
| `HyperlinkButton() -> void` |  |
| `NavigationUri.get -> System.Uri` |  |
| `NavigationUri.set -> void` |  |

## IMenuItemContainer

What holds menu items.

| Member | |
|---|---|
| `Add(XForma.Controls.MenuItem menuItem) -> void` |  |

## ImageToggleButton

An icon that toggles between a checked and an unchecked image when pressed. Its theme's template draws it.

| Member | |
|---|---|
| `CheckedIcon.get -> XForma.Bitmap` |  |
| `CheckedIcon.set -> void` |  |
| `CommandParameter.get -> object` | The value passed to the command when it runs (and when asked whether it can). |
| `CommandParameter.set -> void` | The value passed to the command when it runs (and when asked whether it can). |
| `GetUserState() -> object` |  |
| `IconSize.get -> XForma.Size?` |  |
| `IconSize.set -> void` |  |
| `ImageToggleButton() -> void` |  |
| `IsChecked.get -> bool` |  |
| `IsChecked.set -> void` |  |
| `IsCheckedChanged -> System.EventHandler<XForma.Controls.ToggledEventArgs>` |  |
| `OnAttaching(XForma.Theme theme) -> void` |  |
| `SetUserState(object state) -> void` |  |
| `Tint.get -> XForma.Color` |  |
| `Tint.set -> void` |  |
| `ToggleCommand.get -> System.Windows.Input.ICommand` |  |
| `ToggleCommand.set -> void` |  |
| `UncheckedIcon.get -> XForma.Bitmap` |  |
| `UncheckedIcon.set -> void` |  |

## InfoBar

A message in the page about the app's state: a title and a message beside an icon for its severity, an optional action button beneath, and a close button. Assistive technology reads it as it opens. Its theme's template draws it, in the layout it measures: the icon at the start, the text after it, wrapped, and the close button at the end.

| Member | |
|---|---|
| `ActionButton.get -> XForma.View` | A button (or another view) beneath the message, for what the user can do about it. |
| `ActionButton.set -> void` | A button (or another view) beneath the message, for what the user can do about it. |
| `ArrangeOverride(XForma.Size finalSize) -> void` |  |
| `Closed -> System.EventHandler` | Raised when the user closes the bar with its close button. |
| `CreateMessageFormat() -> XForma.TextFormat` |  |
| `CreateTitleFormat() -> XForma.TextFormat` |  |
| `DefaultAccessibilityLabel.get -> string` |  |
| `DefaultAccessibilityRole.get -> XForma.AccessibilityRole` |  |
| `IconBounds.get -> XForma.Rect` | Where the severity icon is. |
| `InfoBar() -> void` |  |
| `IsClosable.get -> bool` | Whether the bar has a close button. |
| `IsClosable.set -> void` | Whether the bar has a close button. |
| `IsOpen.get -> bool` | Whether the bar shows (by default, it doesn't). |
| `IsOpen.set -> void` | Whether the bar shows (by default, it doesn't). |
| `IsOpenProperty -> XForma.ViewProperty<XForma.Controls.InfoBar, bool>` |  |
| `MeasureOverride(XForma.Size availableSize) -> XForma.Size` |  |
| `Message.get -> string` |  |
| `Message.set -> void` |  |
| `MessageBounds.get -> XForma.Rect` | Where the message is. |
| `Severity.get -> XForma.Controls.InfoBarSeverity` |  |
| `Severity.set -> void` |  |
| `Title.get -> string` |  |
| `Title.set -> void` |  |
| `TitleBounds.get -> XForma.Rect` | Where the title is (above the message). |

## InfoBarCloseButton

An info bar's close button. Its theme's template draws it.

| Member | |
|---|---|
| `InfoBar.get -> XForma.Controls.InfoBar` |  |

## InfoBarSeverity

How serious an info bar's message is, which its icon and color show.

| Member | |
|---|---|
| `Error = 3 -> XForma.Controls.InfoBarSeverity` |  |
| `Informational = 0 -> XForma.Controls.InfoBarSeverity` |  |
| `Success = 1 -> XForma.Controls.InfoBarSeverity` |  |
| `Warning = 2 -> XForma.Controls.InfoBarSeverity` |  |

## InfoBarStyle

How a theme lays out an `InfoBar`: an icon, then the title, message and action, and a close button in the corner.

| Member | |
|---|---|
| `<Clone>$() -> XForma.Controls.InfoBarStyle` |  |
| `CloseButtonOutset.get -> float` | How far the close button sits out into the spacing at the corner. |
| `CloseButtonOutset.init -> void` | How far the close button sits out into the spacing at the corner. |
| `CloseButtonSize.get -> float` | The close button's size. |
| `CloseButtonSize.init -> void` | The close button's size. |
| `DefaultWidth.get -> float` | The bar's width when it's offered any. |
| `DefaultWidth.init -> void` | The bar's width when it's offered any. |
| `Equals(XForma.Controls.InfoBarStyle other) -> bool` |  |
| `Equals(object obj) -> bool` |  |
| `GetHashCode() -> int` |  |
| `IconOffset.get -> float` | How far the icon sits below the top of the text. |
| `IconOffset.init -> void` | How far the icon sits below the top of the text. |
| `IconSize.get -> float` | The icon's size. |
| `IconSize.init -> void` | The icon's size. |
| `InfoBarStyle() -> void` |  |
| `Spacing.get -> float` | The space around and between its parts. |
| `Spacing.init -> void` | The space around and between its parts. |
| `TitleFontWeight.get -> XForma.FontWeight` | The title's weight. |
| `TitleFontWeight.init -> void` | The title's weight. |
| `ToString() -> string` |  |
| `operator !=(XForma.Controls.InfoBarStyle left, XForma.Controls.InfoBarStyle right) -> bool` |  |
| `operator ==(XForma.Controls.InfoBarStyle left, XForma.Controls.InfoBarStyle right) -> bool` |  |

## ItemClickEventArgs

An item in a list was clicked.

| Member | |
|---|---|
| `ClickedItem.get -> object` | The item that was clicked (as WinUI's `ItemClickEventArgs`). |
| `ItemClickEventArgs(object clickedItem, XForma.View itemView) -> void` |  |
| `ItemView.get -> XForma.View` | The view showing the item. |

## ItemViewFactory<T>

| Member | |
|---|---|
| `Create<TView>(System.Func<TView> create, System.Action<TView, T> bind) -> XForma.Controls.ItemViewFactory<T>` |  |

## LayoutTransition

How a panel animates its children when its layout changes: children that move slide from where they were, and children that are added fade in. Set it on `ChildTransitions` before the children are attached, so they get the animation layers it needs.

| Member | |
|---|---|
| `AnimatesEntrances.get -> bool` | Whether children added after the first layout fade in. |
| `AnimatesEntrances.init -> void` | Whether children added after the first layout fade in. |
| `AnimatesMoves.get -> bool` | Whether children that move slide from their old positions. |
| `AnimatesMoves.init -> void` | Whether children that move slide from their old positions. |
| `Duration.get -> System.TimeSpan?` | How long children take, or null (the default) for the theme's medium duration. |
| `Duration.init -> void` | How long children take, or null (the default) for the theme's medium duration. |
| `Easing.get -> XForma.EasingFunction` | How children arrive, or null (the default) for the theme's decelerating curve. |
| `Easing.init -> void` | How children arrive, or null (the default) for the theme's decelerating curve. |
| `LayoutTransition() -> void` |  |

## LightDismissControl

A control that closes (through `OnLightDismiss`) when the user touches anywhere outside it, while light dismiss is enabled.

| Member | |
|---|---|
| `IsLightDismissEnabled.get -> bool` |  |
| `IsLightDismissEnabled.set -> void` |  |
| `LightDismissControl() -> void` |  |
| `OnLightDismiss() -> void` |  |

## ListView

An items view whose items the user taps to click and select them.

| Member | |
|---|---|
| `ClearSelection() -> void` |  |
| `ContainerFromItem(object item) -> XForma.Controls.ListViewItem` | The container showing the item, if it's in view, or null. |
| `CreateItemContainer() -> XForma.Controls.ListViewItem` | Creates the container for an item's view: a `ListViewItem`. Control libraries override this to add behavior to every item, such as swipe gestures. |
| `GetUserState() -> object` |  |
| `ItemClick -> System.EventHandler<XForma.Controls.ItemClickEventArgs>` | Raised when the user taps an item (whatever the selection mode). |
| `ListView() -> void` |  |
| `OnAttaching(XForma.Theme theme) -> void` |  |
| `OnContainerBound(XForma.Primitives.ItemContainer container) -> void` |  |
| `SelectedItem.get -> object` |  |
| `SelectedItem.set -> void` |  |
| `SelectedItems.get -> System.Collections.Generic.IReadOnlyList<object>` | The selected items, in the order they were selected. |
| `SelectionBackground.get -> XForma.Color?` | The tint over a selected row, or null (the default) for the theme's. |
| `SelectionBackground.set -> void` | The tint over a selected row, or null (the default) for the theme's. |
| `SelectionChanged -> System.EventHandler<XForma.Controls.SelectionChangedEventArgs>` |  |
| `SelectionMode.get -> XForma.Controls.SelectionMode` | How tapping items selects them: one at a time (the default), several, or none. |
| `SelectionMode.set -> void` | How tapping items selects them: one at a time (the default), several, or none. |
| `SetUserState(object state) -> void` |  |

## ListView<T>

| Member | |
|---|---|
| `GroupHeaderFactory.get -> XForma.Controls.ItemViewFactory<object>` |  |
| `GroupHeaderFactory.set -> void` |  |
| `GroupKeySelector.get -> System.Func<T, object>` |  |
| `GroupKeySelector.set -> void` |  |
| `ItemViewFactory.get -> XForma.Controls.ItemViewFactory<T>` |  |
| `ItemViewFactory.set -> void` |  |
| `ItemsSource.get -> System.Collections.Generic.IReadOnlyList<T>` |  |
| `ItemsSource.set -> void` |  |
| `ListView() -> void` |  |
| `ScrollIntoView(T item) -> void` |  |
| `SelectedItem.get -> T` |  |
| `SelectedItem.set -> void` |  |
| `SelectedItems.get -> System.Collections.Generic.IReadOnlyList<T>` |  |

## ListViewItem

A list's container for one item: it handles taps, which click and select the item, and its theme's template draws the row's background and selection. Control libraries derive from it (and override `CreateItemContainer`) to add behavior such as swipe gestures.

| Member | |
|---|---|
| `AccessibilityIsSelected.get -> bool` |  |
| `ContainerFromContent(XForma.View view) -> XForma.Controls.ListViewItem` | The container of a view that shows a list item (the view, or one of its ancestors, is a container's content), or null. |
| `DefaultAccessibilityRole.get -> XForma.AccessibilityRole` |  |
| `IsSelected.get -> bool` | Whether the container's item is selected. The list sets it. |
| `ListView.get -> XForma.Controls.ListView` | The list the container is showing an item for. |
| `ListViewItem() -> void` |  |
| `OnAttaching(XForma.Theme theme) -> void` |  |
| `OnHotReload() -> void` |  |
| `OnIsSelectedChanged() -> void` | Called when the item is selected or unselected. |
| `OnTapped() -> void` | Called when the container is tapped. By default it clicks the item, and selects it as the list's `SelectionMode` says. |
| `OnThemeChanged(XForma.Theme theme) -> void` |  |
| `Render(XForma.DrawingContext drawingContext) -> void` |  |

## ListViewStyle

How a theme lays out a `ListView`1`'s default group headers.

| Member | |
|---|---|
| `<Clone>$() -> XForma.Controls.ListViewStyle` |  |
| `Equals(XForma.Controls.ListViewStyle other) -> bool` |  |
| `Equals(object obj) -> bool` |  |
| `GetHashCode() -> int` |  |
| `GroupHeaderFontWeight.get -> XForma.FontWeight` | A group header's weight. |
| `GroupHeaderFontWeight.init -> void` | A group header's weight. |
| `GroupHeaderMargin.get -> XForma.Thickness` | The space around a group header. |
| `GroupHeaderMargin.init -> void` | The space around a group header. |
| `ListViewStyle() -> void` |  |
| `ToString() -> string` |  |
| `operator !=(XForma.Controls.ListViewStyle left, XForma.Controls.ListViewStyle right) -> bool` |  |
| `operator ==(XForma.Controls.ListViewStyle left, XForma.Controls.ListViewStyle right) -> bool` |  |

## ListViewSwipeDirection

Which way a list row was swiped: toward its start (primary) or its end (secondary).

| Member | |
|---|---|
| `Primary = 0 -> XForma.Controls.ListViewSwipeDirection` |  |
| `Secondary = 1 -> XForma.Controls.ListViewSwipeDirection` |  |

## ListViewSwipeItem

What shows behind a list row as it's swiped: the command the swipe runs, in its color. Its theme's template draws it.

| Member | |
|---|---|
| `BackgroundColor.get -> XForma.Color` |  |
| `Command.get -> XForma.Controls.UICommand` |  |
| `Command.set -> void` |  |
| `Direction.get -> XForma.Controls.ListViewSwipeDirection` |  |
| `Direction.set -> void` |  |
| `Icon.get -> XForma.Bitmap` |  |
| `IconSize.get -> XForma.Size` |  |
| `IconSize.set -> void` |  |
| `ListViewSwipeItem() -> void` |  |
| `Reset() -> void` |  |
| `Text.get -> string` |  |

## MenuBar

A bar of commands: a button for each item, sized and placed as the theme's `MenuBarStyle` says. If they don't fit across the bar they go side by side, then share its width. Its theme's template draws it.

| Member | |
|---|---|
| `MeasureOverride(XForma.Size availableSize) -> XForma.Size` |  |
| `MenuBar() -> void` |  |
| `MenuItems.get -> XForma.Controls.MenuItemCollection` |  |
| `OnThemeChanged(XForma.Theme theme) -> void` |  |

## MenuBarStyle

How a theme lays out a `MenuBar`.

| Member | |
|---|---|
| `<Clone>$() -> XForma.Controls.MenuBarStyle` |  |
| `Equals(XForma.Controls.MenuBarStyle other) -> bool` |  |
| `Equals(object obj) -> bool` |  |
| `GetHashCode() -> int` |  |
| `Height.get -> float` | The bar's height. |
| `Height.init -> void` | The bar's height. |
| `ItemSpacing.get -> float` | The space between items. |
| `ItemSpacing.init -> void` | The space between items. |
| `ItemWidth.get -> float` | Each item's width. |
| `ItemWidth.init -> void` | Each item's width. |
| `ItemsAlignment.get -> XForma.HorizontalAlignment` | Where the items sit across the bar. |
| `ItemsAlignment.init -> void` | Where the items sit across the bar. |
| `MenuBarStyle() -> void` |  |
| `ToString() -> string` |  |
| `operator !=(XForma.Controls.MenuBarStyle left, XForma.Controls.MenuBarStyle right) -> bool` |  |
| `operator ==(XForma.Controls.MenuBarStyle left, XForma.Controls.MenuBarStyle right) -> bool` |  |

## MenuButton

A `MenuBar`'s button, for one of its items. Its theme's template draws it.

| Member | |
|---|---|
| `CommandParameter.get -> object` | The value passed to the command when it runs (and when asked whether it can). |
| `CommandParameter.set -> void` | The value passed to the command when it runs (and when asked whether it can). |
| `MenuBar.get -> XForma.Controls.MenuBar` |  |
| `MenuItem.get -> XForma.Controls.MenuItem` |  |

## MenuFlyout

A menu of commands in a popup, beside the view it opens from: a view's context menu (its `ContextFlyout`, see `ViewAttachments`), or opened by the app. Choosing an item raises its `Click` and runs its command; a click outside, Escape or going back closes it.

| Member | |
|---|---|
| `Items.get -> System.Collections.Generic.List<XForma.Controls.MenuItem>` | The commands, in order (those not `IsVisible` left out). |
| `MenuFlyout() -> void` |  |

## MenuFlyoutStyle

How a theme lays out a `MenuFlyout`'s rows.

| Member | |
|---|---|
| `<Clone>$() -> XForma.Controls.MenuFlyoutStyle` |  |
| `Equals(XForma.Controls.MenuFlyoutStyle other) -> bool` |  |
| `Equals(object obj) -> bool` |  |
| `GetHashCode() -> int` |  |
| `MenuFlyoutStyle() -> void` |  |
| `MinimumWidth.get -> float` | The menu's least width. |
| `MinimumWidth.init -> void` | The menu's least width. |
| `TextEndInset.get -> float` | The space after an item's text. |
| `TextEndInset.init -> void` | The space after an item's text. |
| `TextInset.get -> float` | The space before an item's text, which leaves room for its icon. |
| `TextInset.init -> void` | The space before an item's text, which leaves room for its icon. |
| `ToString() -> string` |  |
| `operator !=(XForma.Controls.MenuFlyoutStyle left, XForma.Controls.MenuFlyoutStyle right) -> bool` |  |
| `operator ==(XForma.Controls.MenuFlyoutStyle left, XForma.Controls.MenuFlyoutStyle right) -> bool` |  |

## MenuFlyoutView

A menu flyout's items: a click or a tap chooses one; with keyboard focus, Up and Down (wrapping), Home and End move the highlight, and Enter or Space chooses it. Its theme's template draws the rows, each `RowHeight` tall.

| Member | |
|---|---|
| `AccessibilityValue.get -> string` |  |
| `CreateTextFormat() -> XForma.TextFormat` |  |
| `HighlightedIndex.get -> int` | The item the keyboard (or a hovering pointer) is on. |
| `IsItemEnabled(int index) -> bool` | Whether the item can be chosen (its command, if any, can run). |
| `Items.get -> System.Collections.Generic.IReadOnlyList<XForma.Controls.MenuItem>` | The items shown. |
| `PressedIndex.get -> int` | The item being pressed, or -1. |
| `RowHeight.get -> float` |  |

## MenuItem

An item in a menu: its text, icon and command.

| Member | |
|---|---|
| `Click -> System.EventHandler` | Raised when the item is chosen (before its command runs). |
| `Command.get -> System.Windows.Input.ICommand` |  |
| `Command.set -> void` |  |
| `Icon.get -> XForma.Bitmap` |  |
| `Icon.set -> void` |  |
| `IsSubtleStyle.get -> bool` | Whether the item is in the subtle style (gray, as for cancel or delete) rather than the accent color (the default). |
| `IsSubtleStyle.set -> void` | Whether the item is in the subtle style (gray, as for cancel or delete) rather than the accent color (the default). |
| `IsVisible.get -> bool` |  |
| `IsVisible.set -> void` |  |
| `MenuItem() -> void` |  |
| `PropertyChanged -> System.ComponentModel.PropertyChangedEventHandler` |  |
| `Text.get -> string` |  |
| `Text.set -> void` |  |

## MenuItemCollection

A menu's items.

| Member | |
|---|---|
| `Add(XForma.Controls.MenuItem menuItem) -> void` |  |
| `GetEnumerator() -> System.Collections.Generic.IEnumerator<XForma.Controls.MenuItem>` |  |

## MessageBox

A flyout asking the user to confirm something, with OK and Cancel.

| Member | |
|---|---|
| `ShowAsync(string message, string title) -> System.Threading.Tasks.Task<XForma.Controls.MessageBoxResult>` |  |

## MessageBoxResult

How the user closed a `MessageBox`.

| Member | |
|---|---|
| `Cancel = 0 -> XForma.Controls.MessageBoxResult` |  |
| `OK = 1 -> XForma.Controls.MessageBoxResult` |  |

## MessageBoxView

A `MessageBox`'s content: its message, and the choices. Its theme's template lays it out, with buttons that call `Choose`.

| Member | |
|---|---|
| `Choose(XForma.Controls.MessageBoxResult result) -> void` | Closes the message box with the user's choice. |
| `Message.get -> string` |  |
| `Title.get -> string` |  |

## Motion

A theme's animation durations and easings.

| Member | |
|---|---|
| `<Clone>$() -> XForma.Controls.Motion` |  |
| `Accelerate.get -> XForma.EasingFunction` | Things that leave. |
| `Accelerate.init -> void` | Things that leave. |
| `Decelerate.get -> XForma.EasingFunction` | Things that arrive. |
| `Decelerate.init -> void` | Things that arrive. |
| `Equals(XForma.Controls.Motion other) -> bool` |  |
| `Equals(object obj) -> bool` |  |
| `GetHashCode() -> int` |  |
| `Long.get -> System.TimeSpan` | Large movements, such as pages and sheets. |
| `Long.init -> void` | Large movements, such as pages and sheets. |
| `Medium.get -> System.TimeSpan` | Most transitions. |
| `Medium.init -> void` | Most transitions. |
| `Motion() -> void` |  |
| `Short.get -> System.TimeSpan` | Press feedback. |
| `Short.init -> void` | Press feedback. |
| `Spring.get -> XForma.EasingFunction` | Playful arrivals that overshoot and settle. |
| `Spring.init -> void` | Playful arrivals that overshoot and settle. |
| `Standard.get -> XForma.EasingFunction` | Things that move from one place to another. |
| `Standard.init -> void` | Things that move from one place to another. |
| `ToString() -> string` |  |
| `operator !=(XForma.Controls.Motion left, XForma.Controls.Motion right) -> bool` |  |
| `operator ==(XForma.Controls.Motion left, XForma.Controls.Motion right) -> bool` |  |

## NavigationView

A pane of navigation items beside the app's content: on a wide window a side pane that's always there, and on a narrow one a drawer that a menu button at the top opens over the content (and a tap outside it or Escape closes). Choosing an item selects it and raises `SelectionChanged`, for the app to show its content. Its theme's templates draw its parts.

| Member | |
|---|---|
| `ArrangeOverride(XForma.Size finalSize) -> void` |  |
| `BarBounds.get -> XForma.Rect` | Where the bar is, on a narrow window (empty on a wide one). |
| `CompactThreshold = 640 -> float` | The width below which the pane becomes a drawer. |
| `Content.get -> XForma.View` | The app's content, beside (or, narrow, beneath) the pane. |
| `Content.set -> void` | The app's content, beside (or, narrow, beneath) the pane. |
| `GetUserState() -> object` |  |
| `IsInTabOrder(XForma.View child) -> bool` |  |
| `IsPaneDrawer.get -> bool` | Whether the pane is a drawer (the window is narrow). |
| `IsPaneOpen.get -> bool` | Whether the drawer is open (on a wide window, the pane is always there). |
| `IsPaneOpen.set -> void` | Whether the drawer is open (on a wide window, the pane is always there). |
| `IsTabScope.get -> bool` |  |
| `MeasureOverride(XForma.Size availableSize) -> XForma.Size` |  |
| `MenuItems.get -> XForma.Controls.NavigationViewItemCollection` | The navigation items, in order. |
| `NavigationView() -> void` |  |
| `OnAttaching(XForma.Theme theme) -> void` |  |
| `OnKeyDown(XForma.KeyEventArgs keyInputEvent) -> void` |  |
| `OnThemeChanged(XForma.Theme theme) -> void` |  |
| `PaneTitle.get -> string` | The pane's title, shown above its items (and in the bar on a narrow window). |
| `PaneTitle.set -> void` | The pane's title, shown above its items (and in the bar on a narrow window). |
| `SelectedItem.get -> XForma.Controls.NavigationViewItem` |  |
| `SelectedItem.set -> void` |  |
| `SelectionChanged -> System.EventHandler<XForma.Controls.NavigationViewSelectionChangedEventArgs>` | Raised when the user selects an item. |
| `SetUserState(object state) -> void` |  |

## NavigationViewItem

An item in a navigation view's pane: an icon and text, selected when chosen (a tap, Enter or Space); Up and Down move between the items. Its theme's template draws it.

| Member | |
|---|---|
| `AccessibilityIsSelected.get -> bool` |  |
| `CreateTextFormat() -> XForma.TextFormat` |  |
| `DefaultAccessibilityLabel.get -> string` |  |
| `DefaultAccessibilityRole.get -> XForma.AccessibilityRole` |  |
| `Icon.get -> XForma.Bitmap` |  |
| `Icon.set -> void` |  |
| `IsSelected.get -> bool` |  |
| `IsTabStop.get -> bool` |  |
| `NavigationView.get -> XForma.Controls.NavigationView` | The navigation view it's in. |
| `NavigationViewItem() -> void` |  |
| `OnActivate() -> void` |  |
| `OnKeyDown(XForma.KeyEventArgs keyInputEvent) -> void` |  |
| `OnThemeChanged(XForma.Theme theme) -> void` |  |
| `PerformAccessibilityAction(XForma.AccessibilityAction action) -> bool` |  |
| `Tag.get -> object` | Anything the app keeps with the item (such as the path it navigates to). |
| `Tag.set -> void` | Anything the app keeps with the item (such as the path it navigates to). |
| `Text.get -> string` |  |
| `Text.set -> void` |  |

## NavigationViewItemCollection

A navigation view's items: added to its pane as they're added here.

| Member | |
|---|---|
| `Add(XForma.Controls.NavigationViewItem item) -> void` |  |
| `Count.get -> int` |  |
| `GetEnumerator() -> System.Collections.Generic.IEnumerator<XForma.Controls.NavigationViewItem>` |  |
| `IndexOf(XForma.Controls.NavigationViewItem item) -> int` |  |
| `this[int index].get -> XForma.Controls.NavigationViewItem` |  |

## NavigationViewMenuButton

The button that opens and closes a navigation view's drawer. Its theme's template draws it.

| Member | |
|---|---|
| `AccessibilityIsExpanded.get -> bool?` |  |
| `NavigationView.get -> XForma.Controls.NavigationView` |  |
| `PerformAccessibilityAction(XForma.AccessibilityAction action) -> bool` |  |

## NavigationViewPane

A navigation view's pane: its title above its items. Its theme's template draws it.

| Member | |
|---|---|
| `NavigationView.get -> XForma.Controls.NavigationView` |  |
| `TitleBounds.get -> XForma.Rect` | Where the title is: at the top, or, in a drawer, beneath the menu button's bar. |

## NavigationViewScrim

The shade over a navigation view's content while its drawer is open: a tap on it closes the drawer. Its theme's template draws it.

## NavigationViewSelectionChangedEventArgs

The selected item of a navigation view.

| Member | |
|---|---|
| `NavigationViewSelectionChangedEventArgs(XForma.Controls.NavigationViewItem selectedItem) -> void` |  |
| `SelectedItem.get -> XForma.Controls.NavigationViewItem` |  |

## NavigationViewStyle

How a theme lays out a `NavigationView`.

| Member | |
|---|---|
| `<Clone>$() -> XForma.Controls.NavigationViewStyle` |  |
| `BarHeight.get -> float` | The height of the bar across the top of a narrow view, and the width of its menu button. |
| `BarHeight.init -> void` | The height of the bar across the top of a narrow view, and the width of its menu button. |
| `DefaultSize.get -> XForma.Size` | The view's size when it's offered any. |
| `DefaultSize.init -> void` | The view's size when it's offered any. |
| `Equals(XForma.Controls.NavigationViewStyle other) -> bool` |  |
| `Equals(object obj) -> bool` |  |
| `GetHashCode() -> int` |  |
| `ItemHeight.get -> float` | An item's height. |
| `ItemHeight.init -> void` | An item's height. |
| `ItemMargin.get -> XForma.Thickness` | The space around an item. |
| `ItemMargin.init -> void` | The space around an item. |
| `ItemSpacing.get -> float` | The space between items. |
| `ItemSpacing.init -> void` | The space between items. |
| `NavigationViewStyle() -> void` |  |
| `PaneWidth.get -> float` | The pane's width. |
| `PaneWidth.init -> void` | The pane's width. |
| `TitleHeight.get -> float` | The height of the pane's title. |
| `TitleHeight.init -> void` | The height of the pane's title. |
| `ToString() -> string` |  |
| `operator !=(XForma.Controls.NavigationViewStyle left, XForma.Controls.NavigationViewStyle right) -> bool` |  |
| `operator ==(XForma.Controls.NavigationViewStyle left, XForma.Controls.NavigationViewStyle right) -> bool` |  |

## NumberBox

A number typed in a text box, kept between a minimum and a maximum: what's typed becomes the value on Enter or when focus leaves (or goes back to the value, if it isn't a number). Up and Down step it by `SmallChange`, Page Up and Page Down by `LargeChange`, and so do its spin buttons. Its theme's template draws the spin buttons, each `SpinButtonWidth` wide, at the end of the box.

| Member | |
|---|---|
| `AccessibilityRange.get -> XForma.AccessibilityRange?` |  |
| `Build() -> XForma.View` |  |
| `DefaultAccessibilityLabel.get -> string` |  |
| `DefaultAccessibilityRole.get -> XForma.AccessibilityRole` |  |
| `DownButtonBounds.get -> XForma.Rect` | Where the down spin button is: the last. |
| `GetUserState() -> object` |  |
| `IsAccessibilityActionSupported(XForma.AccessibilityAction action) -> bool` |  |
| `LargeChange.get -> double` |  |
| `LargeChange.set -> void` |  |
| `Maximum.get -> double` |  |
| `Maximum.set -> void` |  |
| `Minimum.get -> double` |  |
| `Minimum.set -> void` |  |
| `NumberBox() -> void` |  |
| `OnAttaching(XForma.Theme theme) -> void` |  |
| `OnKeyDown(XForma.KeyEventArgs keyInputEvent) -> void` |  |
| `OnPointerCanceled(XForma.PointerEventArgs e) -> void` |  |
| `OnPointerPressed(XForma.PointerEventArgs e) -> void` |  |
| `OnPointerReleased(XForma.PointerEventArgs e) -> void` |  |
| `PerformAccessibilityAction(XForma.AccessibilityAction action) -> bool` |  |
| `PlaceholderText.get -> string` | Shown while the box is empty; it also names the box for assistive technology. |
| `PlaceholderText.set -> void` | Shown while the box is empty; it also names the box for assistive technology. |
| `PressedPart.get -> XForma.Controls.NumberBoxPart` | The spin button being pressed. |
| `SetAccessibilityRangeValue(double value) -> bool` |  |
| `SetUserState(object state) -> void` |  |
| `SmallChange.get -> double` |  |
| `SmallChange.set -> void` |  |
| `SpinButtonPlacementMode.get -> XForma.Controls.NumberBoxSpinButtonPlacementMode` |  |
| `SpinButtonPlacementMode.set -> void` |  |
| `UpButtonBounds.get -> XForma.Rect` | Where the up spin button is: the first of the two, at the end. |
| `Value.get -> double` | The number, or NaN while the box is empty. |
| `Value.set -> void` | The number, or NaN while the box is empty. |
| `ValueChanged -> System.EventHandler<XForma.Controls.RangeValueChangedEventArgs>` | Raised when the value changes. |
| `ValueProperty -> XForma.ViewProperty<XForma.Controls.NumberBox, double>` |  |

## NumberBoxPart

A part of a number box.

| Member | |
|---|---|
| `DownButton = 2 -> XForma.Controls.NumberBoxPart` |  |
| `None = 0 -> XForma.Controls.NumberBoxPart` |  |
| `UpButton = 1 -> XForma.Controls.NumberBoxPart` |  |

## NumberBoxSpinButtonPlacementMode

Where a number box's spin buttons are.

| Member | |
|---|---|
| `Hidden = 0 -> XForma.Controls.NumberBoxSpinButtonPlacementMode` | None: the number is typed, or stepped with the keyboard. |
| `Inline = 1 -> XForma.Controls.NumberBoxSpinButtonPlacementMode` | Up and down buttons at the end of the box. |

## NumberBoxStyle

How a theme lays out a `NumberBox`.

| Member | |
|---|---|
| `<Clone>$() -> XForma.Controls.NumberBoxStyle` |  |
| `Equals(XForma.Controls.NumberBoxStyle other) -> bool` |  |
| `Equals(object obj) -> bool` |  |
| `GetHashCode() -> int` |  |
| `NumberBoxStyle() -> void` |  |
| `SpinButtonWidth.get -> float` | The width of each spin button. |
| `SpinButtonWidth.init -> void` | The width of each spin button. |
| `ToString() -> string` |  |
| `operator !=(XForma.Controls.NumberBoxStyle left, XForma.Controls.NumberBoxStyle right) -> bool` |  |
| `operator ==(XForma.Controls.NumberBoxStyle left, XForma.Controls.NumberBoxStyle right) -> bool` |  |

## Panel

Base for layout containers. A panel creates a `ViewCollection` (or a `ViewCollection`1`, for children with layout parameters) as its `Children`, and positions them in `MeasureChildren` and `ArrangeChildren`.

| Member | |
|---|---|
| `AnimatesChildren.get -> bool` |  |
| `ArrangeChildren(XForma.Size finalSize) -> void` | Arranges each child within the layout's final size. |
| `ArrangeOverride(XForma.Size finalSize) -> void` |  |
| `ChildTransitions.get -> XForma.Controls.LayoutTransition` | How the children animate when the layout changes, or null (the default) for no animation. Set it before adding children. |
| `ChildTransitions.set -> void` | How the children animate when the layout changes, or null (the default) for no animation. Set it before adding children. |
| `LayoutChildren.get -> System.Collections.Generic.IReadOnlyList<XForma.View>` | The children in the order they were added. |
| `MeasureChildren(XForma.Size availableSize) -> XForma.Size` | Measures each child and returns the size they need together. |
| `MeasureOverride(XForma.Size availableSize) -> XForma.Size` |  |
| `Padding.get -> XForma.Thickness` | Space between the panel's edges and its children. |
| `Padding.set -> void` | Space between the panel's edges and its children. |
| `Panel() -> void` |  |

## PasswordBox

A text box for a password, its characters hidden, with a button that shows them while it's pressed. Its theme's template draws the button, `RevealButtonWidth` wide, at the end of the box.

| Member | |
|---|---|
| `Build() -> XForma.View` |  |
| `IsPasswordRevealed.get -> bool` | Whether the password is showing. |
| `IsRevealButtonVisible.get -> bool` | Whether the reveal button shows: peeking, with a password to show. |
| `OnPointerCanceled(XForma.PointerEventArgs e) -> void` |  |
| `OnPointerPressed(XForma.PointerEventArgs e) -> void` |  |
| `OnPointerReleased(XForma.PointerEventArgs e) -> void` |  |
| `Password.get -> string` |  |
| `Password.set -> void` |  |
| `PasswordBox() -> void` |  |
| `PasswordChanged -> System.EventHandler` | Raised when the password changes. |
| `PasswordProperty -> XForma.ViewProperty<XForma.Controls.PasswordBox, string>` |  |
| `PasswordRevealMode.get -> XForma.Controls.PasswordRevealMode` |  |
| `PasswordRevealMode.set -> void` |  |
| `PlaceholderText.get -> string` | Shown while the box is empty; it also names the box for assistive technology. |
| `PlaceholderText.set -> void` | Shown while the box is empty; it also names the box for assistive technology. |
| `RevealButtonBounds.get -> XForma.Rect` | Where the reveal button is: at the end. |

## PasswordBoxStyle

How a theme lays out a `PasswordBox`.

| Member | |
|---|---|
| `<Clone>$() -> XForma.Controls.PasswordBoxStyle` |  |
| `Equals(XForma.Controls.PasswordBoxStyle other) -> bool` |  |
| `Equals(object obj) -> bool` |  |
| `GetHashCode() -> int` |  |
| `PasswordBoxStyle() -> void` |  |
| `RevealButtonWidth.get -> float` | The width of the reveal button. |
| `RevealButtonWidth.init -> void` | The width of the reveal button. |
| `ToString() -> string` |  |
| `operator !=(XForma.Controls.PasswordBoxStyle left, XForma.Controls.PasswordBoxStyle right) -> bool` |  |
| `operator ==(XForma.Controls.PasswordBoxStyle left, XForma.Controls.PasswordBoxStyle right) -> bool` |  |

## PasswordRevealMode

Whether a password box can show its password.

| Member | |
|---|---|
| `Hidden = 1 -> XForma.Controls.PasswordRevealMode` | Never. |
| `Peek = 0 -> XForma.Controls.PasswordRevealMode` | A button at the end of the box shows it while it's pressed. |
| `Visible = 2 -> XForma.Controls.PasswordRevealMode` | Always. |

## PickerField

The field of a picker: shows the current value (or a placeholder) like a text entry, with a glyph at the end, and opens the picker's popup when clicked, or with Enter, Space, F4 or Alt+Down. Its theme's template draws it, in the layout the field measures: the text from the theme's `TextInset`, and the glyph in the last `GlyphWidth`.

| Member | |
|---|---|
| `AccessibilityIsExpanded.get -> bool?` |  |
| `AccessibilityValue.get -> string` |  |
| `ClosePicker() -> void` | Closes the picker's popup. |
| `CreateTextFormat() -> XForma.TextFormat` | The field's text format, which it measures with: a text box's text size, so a field and a text box beside it match. |
| `DefaultAccessibilityRole.get -> XForma.AccessibilityRole` |  |
| `DisplayText.get -> string` | The text shown in the field. |
| `IsPickerOpen.get -> bool` | Whether the picker's popup is open (a theme shows the field as focused meanwhile). |
| `IsPlaceholder.get -> bool` | Whether the text is a placeholder (drawn subtly). |
| `MaximumTextWidth.get -> float` | The widest text the field may show, which sets its width. |
| `MeasureOverride(XForma.Size availableSize) -> XForma.Size` |  |
| `OnActivate() -> void` |  |
| `OnKeyDown(XForma.KeyEventArgs keyInputEvent) -> void` |  |
| `OnPointerCanceled(XForma.PointerEventArgs e) -> void` |  |
| `OnPointerPressed(XForma.PointerEventArgs e) -> void` |  |
| `OnPointerReleased(XForma.PointerEventArgs e) -> void` |  |
| `OpenPicker() -> void` | Opens the picker's popup. |
| `PerformAccessibilityAction(XForma.AccessibilityAction action) -> bool` |  |
| `PickerField() -> void` |  |

## PickerFieldStyle

How a theme lays out a `PickerField`: the text after an inset, and a glyph at the end.

| Member | |
|---|---|
| `<Clone>$() -> XForma.Controls.PickerFieldStyle` |  |
| `Equals(XForma.Controls.PickerFieldStyle other) -> bool` |  |
| `Equals(object obj) -> bool` |  |
| `GetHashCode() -> int` |  |
| `GlyphWidth.get -> float` | The width of the glyph's area. |
| `GlyphWidth.init -> void` | The width of the glyph's area. |
| `PickerFieldStyle() -> void` |  |
| `TextInset.get -> float` | The space before the text. |
| `TextInset.init -> void` | The space before the text. |
| `ToString() -> string` |  |
| `operator !=(XForma.Controls.PickerFieldStyle left, XForma.Controls.PickerFieldStyle right) -> bool` |  |
| `operator ==(XForma.Controls.PickerFieldStyle left, XForma.Controls.PickerFieldStyle right) -> bool` |  |

## PipsPager

Dots, one for each page, showing which is current (as beneath a flip view): a tap on a dot selects its page; with keyboard focus, Left and Right (following the flow direction), Home and End move it. Its theme's template draws it, in the layout it measures: a dot each `DotSpacing` apart, centered.

| Member | |
|---|---|
| `AccessibilityRange.get -> XForma.AccessibilityRange?` |  |
| `AccessibilityValue.get -> string` |  |
| `DefaultAccessibilityLabel.get -> string` |  |
| `DefaultAccessibilityRole.get -> XForma.AccessibilityRole` |  |
| `GetDotCenter(int index) -> XForma.Point` | The center of a page's dot. |
| `IsAccessibilityActionSupported(XForma.AccessibilityAction action) -> bool` |  |
| `MeasureOverride(XForma.Size availableSize) -> XForma.Size` |  |
| `NumberOfPages.get -> int` |  |
| `NumberOfPages.set -> void` |  |
| `OnKeyDown(XForma.KeyEventArgs keyInputEvent) -> void` |  |
| `OnPointerPressed(XForma.PointerEventArgs e) -> void` |  |
| `OnPointerReleased(XForma.PointerEventArgs e) -> void` |  |
| `OnThemeChanged(XForma.Theme theme) -> void` |  |
| `PerformAccessibilityAction(XForma.AccessibilityAction action) -> bool` |  |
| `PipsPager() -> void` |  |
| `SelectedIndexChanged -> System.EventHandler` |  |
| `SelectedPageIndex.get -> int` |  |
| `SelectedPageIndex.set -> void` |  |
| `SelectedPageIndexProperty -> XForma.ViewProperty<XForma.Controls.PipsPager, int>` |  |
| `SetAccessibilityRangeValue(double value) -> bool` |  |

## PipsPagerStyle

How a theme lays out a `PipsPager`.

| Member | |
|---|---|
| `<Clone>$() -> XForma.Controls.PipsPagerStyle` |  |
| `DotSpacing.get -> float` | The distance between dots, each of which takes presses across it. |
| `DotSpacing.init -> void` | The distance between dots, each of which takes presses across it. |
| `Equals(XForma.Controls.PipsPagerStyle other) -> bool` |  |
| `Equals(object obj) -> bool` |  |
| `GetHashCode() -> int` |  |
| `Height.get -> float` | The pager's height. |
| `Height.init -> void` | The pager's height. |
| `PipsPagerStyle() -> void` |  |
| `ToString() -> string` |  |
| `operator !=(XForma.Controls.PipsPagerStyle left, XForma.Controls.PipsPagerStyle right) -> bool` |  |
| `operator ==(XForma.Controls.PipsPagerStyle left, XForma.Controls.PipsPagerStyle right) -> bool` |  |

## PopupFrame

The frame around a popup's content (a picker's list or calendar, a flyout's): its theme's template draws it, and its content stays `ContentInset` inside its edges.

| Member | |
|---|---|
| `ContentInset.get -> float` | How far inside the frame's edges its content stays, clear of its rounded corners. |
| `FramedContent.get -> XForma.View` | The frame's content, inset from its edges (for a frame that doesn't lay its content out itself). |
| `FramedContent.set -> void` | The frame's content, inset from its edges (for a frame that doesn't lay its content out itself). |
| `OnThemeChanged(XForma.Theme theme) -> void` |  |
| `PopupFrame() -> void` |  |

## PopupStyle

How a theme lays out a popup: its frame and the lists in it.

| Member | |
|---|---|
| `<Clone>$() -> XForma.Controls.PopupStyle` |  |
| `ContentInset.get -> float` | How far inside the frame's edges its content stays, clear of its corners. |
| `ContentInset.init -> void` | How far inside the frame's edges its content stays, clear of its corners. |
| `Equals(XForma.Controls.PopupStyle other) -> bool` |  |
| `Equals(object obj) -> bool` |  |
| `GetHashCode() -> int` |  |
| `ListVerticalPadding.get -> float` | The space above and below a drop-down's list, beyond the inset. |
| `ListVerticalPadding.init -> void` | The space above and below a drop-down's list, beyond the inset. |
| `PopupStyle() -> void` |  |
| `RowHeight.get -> float` | The rows of a popup list (a combo box's, a menu flyout's, a text box's suggestions) where a mouse is the primary input. |
| `RowHeight.init -> void` | The rows of a popup list (a combo box's, a menu flyout's, a text box's suggestions) where a mouse is the primary input. |
| `ToString() -> string` |  |
| `TouchRowHeight.get -> float` | The rows of a popup list where touch is the primary input. |
| `TouchRowHeight.init -> void` | The rows of a popup list where touch is the primary input. |
| `operator !=(XForma.Controls.PopupStyle left, XForma.Controls.PopupStyle right) -> bool` |  |
| `operator ==(XForma.Controls.PopupStyle left, XForma.Controls.PopupStyle right) -> bool` |  |

## ProgressBar

How far a task has got: a bar filled to its value, or, while indeterminate, a segment moving along it. Its theme's template draws it.

| Member | |
|---|---|
| `AccessibilityRange.get -> XForma.AccessibilityRange?` |  |
| `DefaultAccessibilityRole.get -> XForma.AccessibilityRole` |  |
| `IsIndeterminate.get -> bool` | Whether the bar shows that work is happening, without how far it has got. |
| `IsIndeterminate.set -> void` | Whether the bar shows that work is happening, without how far it has got. |
| `MeasureOverride(XForma.Size availableSize) -> XForma.Size` |  |
| `ProgressBar() -> void` |  |
| `ShowError.get -> bool` | Whether the bar shows that the task failed. |
| `ShowError.set -> void` | Whether the bar shows that the task failed. |
| `ShowPaused.get -> bool` | Whether the bar shows that the task is paused. |
| `ShowPaused.set -> void` | Whether the bar shows that the task is paused. |
| `ValueProperty -> XForma.ViewProperty<XForma.Controls.ProgressBar, double>` |  |

## ProgressBarStyle

How a theme lays out a `ProgressBar`.

| Member | |
|---|---|
| `<Clone>$() -> XForma.Controls.ProgressBarStyle` |  |
| `DefaultWidth.get -> float` | The bar's width when it's offered any. |
| `DefaultWidth.init -> void` | The bar's width when it's offered any. |
| `Equals(XForma.Controls.ProgressBarStyle other) -> bool` |  |
| `Equals(object obj) -> bool` |  |
| `GetHashCode() -> int` |  |
| `Height.get -> float` | The bar's height. |
| `Height.init -> void` | The bar's height. |
| `ProgressBarStyle() -> void` |  |
| `ToString() -> string` |  |
| `operator !=(XForma.Controls.ProgressBarStyle left, XForma.Controls.ProgressBarStyle right) -> bool` |  |
| `operator ==(XForma.Controls.ProgressBarStyle left, XForma.Controls.ProgressBarStyle right) -> bool` |  |

## ProgressRing

An indeterminate progress indicator, shown while it's active. Its theme's template draws it (Basic's spins an arc on the platform's compositor or animation thread, so it keeps turning while the UI thread is busy).

| Member | |
|---|---|
| `IsActive.get -> bool` | Whether the ring shows, and moves. |
| `IsActive.set -> void` | Whether the ring shows, and moves. |
| `ProgressRing() -> void` |  |

## RadioButton

One of a `RadioButtons` group's buttons, for one of its items. Its theme's template draws it.

| Member | |
|---|---|
| `AccessibilityIsSelected.get -> bool` |  |
| `Command.get -> System.Windows.Input.ICommand` |  |
| `Command.set -> void` |  |
| `IsChecked.get -> bool` |  |
| `IsChecked.set -> void` |  |
| `IsCheckedChanged -> System.EventHandler<XForma.Controls.ToggledEventArgs>` |  |
| `RadioButtonItem.get -> XForma.Controls.RadioButtonItem` |  |
| `RadioButtons.get -> XForma.Controls.RadioButtons` |  |

## RadioButtonItem

One choice in a `RadioButtons` group.

| Member | |
|---|---|
| `Icon.get -> XForma.Bitmap` |  |
| `Icon.set -> void` |  |
| `PropertyChanged -> System.ComponentModel.PropertyChangedEventHandler` |  |
| `RadioButtonItem() -> void` |  |
| `Text.get -> string` |  |
| `Text.set -> void` |  |
| `Value.get -> object` |  |
| `Value.set -> void` |  |

## RadioButtonItemCollection

A `RadioButtons` group's choices.

| Member | |
|---|---|
| `Add(XForma.Controls.RadioButtonItem radioButtonItem) -> void` |  |
| `GetEnumerator() -> System.Collections.Generic.IEnumerator<XForma.Controls.RadioButtonItem>` |  |

## RadioButtonStyle

How a theme lays out a `RadioButtons` group.

| Member | |
|---|---|
| `<Clone>$() -> XForma.Controls.RadioButtonStyle` |  |
| `Equals(XForma.Controls.RadioButtonStyle other) -> bool` |  |
| `Equals(object obj) -> bool` |  |
| `GetHashCode() -> int` |  |
| `Height.get -> float` | A button's, and the group's, height. |
| `Height.init -> void` | A button's, and the group's, height. |
| `RadioButtonStyle() -> void` |  |
| `ToString() -> string` |  |
| `Width.get -> float` | A button's width. |
| `Width.init -> void` | A button's width. |
| `operator !=(XForma.Controls.RadioButtonStyle left, XForma.Controls.RadioButtonStyle right) -> bool` |  |
| `operator ==(XForma.Controls.RadioButtonStyle left, XForma.Controls.RadioButtonStyle right) -> bool` |  |

## RadioButtons

A group of choices, of which one is selected.

| Member | |
|---|---|
| `GetUserState() -> object` |  |
| `Items.get -> XForma.Controls.RadioButtonItemCollection` |  |
| `OnAttaching(XForma.Theme theme) -> void` |  |
| `OnThemeChanged(XForma.Theme theme) -> void` |  |
| `RadioButtons() -> void` |  |
| `SelectedItem.get -> object` |  |
| `SelectedItem.set -> void` |  |
| `SelectedItemProperty -> XForma.ViewProperty<XForma.Controls.RadioButtons, object>` |  |
| `SelectionChanged -> System.EventHandler<XForma.Controls.SelectionChangedEventArgs>` |  |
| `SetUserState(object state) -> void` |  |

## RangeBase

Base for controls showing a value within a range (a slider, a progress bar): the value is kept between the minimum and the maximum.

| Member | |
|---|---|
| `AccessibilityRange.get -> XForma.AccessibilityRange?` |  |
| `CoerceValue(double value) -> double` | Adjusts a value before it's kept in the range (a slider snaps it to its steps). |
| `Fraction.get -> double` | How far the value is through the range, from 0 to 1. |
| `Maximum.get -> double` |  |
| `Maximum.set -> void` |  |
| `Minimum.get -> double` |  |
| `Minimum.set -> void` |  |
| `RangeBase() -> void` |  |
| `Value.get -> double` |  |
| `Value.set -> void` |  |
| `ValueChanged -> System.EventHandler<XForma.Controls.RangeValueChangedEventArgs>` |  |

## RangeValueChangedEventArgs

The old and new values of a range control.

| Member | |
|---|---|
| `NewValue.get -> double` |  |
| `OldValue.get -> double` |  |
| `RangeValueChangedEventArgs(double oldValue, double newValue) -> void` |  |

## RefreshContainer

Pull to refresh, around a list (an `ItemsView`, such as a `ListView`): pulling it down past its top shows the platform's refresh indicator and raises `RefreshRequested`, and the indicator spins until the app's done. `RequestRefresh` starts one from the app (a button, a key), around any content.

| Member | |
|---|---|
| `Content.get -> XForma.View` | The list inside (or any view, refreshed only by `RequestRefresh`). |
| `Content.set -> void` | The list inside (or any view, refreshed only by `RequestRefresh`). |
| `IsRefreshing.get -> bool` | Whether a refresh is going on. |
| `RefreshContainer() -> void` |  |
| `RefreshRequested -> System.EventHandler<XForma.Controls.RefreshRequestedEventArgs>` | Raised when the user pulls to refresh, or the app asks (`RequestRefresh`). |
| `RequestRefresh() -> void` | Starts a refresh, as pulling would (with the list's indicator). |

## RefreshRequestedEventArgs

A refresh the user asked for. Take a deferral to finish it later (the indicator spins until it's completed); otherwise it ends when the handlers return.

| Member | |
|---|---|
| `GetDeferral() -> System.Action` | Keeps the refresh going until the returned action is called. |

## RelayCommand

A command that runs a delegate, optionally only when another delegate says it can (as CommunityToolkit.Mvvm's RelayCommand).

| Member | |
|---|---|
| `CanExecute(object parameter) -> bool` |  |
| `CanExecuteChanged -> System.EventHandler` |  |
| `Execute(object parameter) -> void` |  |
| `NotifyCanExecuteChanged() -> void` | Tells the controls using the command to ask `CanExecute` again. |
| `RelayCommand(System.Action execute, System.Func<bool> canExecute = null) -> void` |  |
| `RelayCommand(System.Action<object> execute, System.Func<object, bool> canExecute = null) -> void` |  |

## SelectionChangedEventArgs

What's selected changed.

| Member | |
|---|---|
| `AddedItems.get -> System.Collections.Generic.IReadOnlyList<object>` |  |
| `NewItem.get -> object` | The item selected now (the first, with several), or null. |
| `OldItem.get -> object` | The item selected before (the first, with several), or null. |
| `RemovedItems.get -> System.Collections.Generic.IReadOnlyList<object>` |  |
| `SelectionChangedEventArgs(System.Collections.Generic.IReadOnlyList<object> removedItems, System.Collections.Generic.IReadOnlyList<object> addedItems, object oldItem, object newItem) -> void` |  |
| `SelectionChangedEventArgs(object oldItem, object newItem) -> void` |  |

## SelectionMode

How tapping a list's items selects them.

| Member | |
|---|---|
| `Multiple = 2 -> XForma.Controls.SelectionMode` | Tapping an item adds it to the selection or removes it. |
| `None = 0 -> XForma.Controls.SelectionMode` | Items are clicked, not selected. |
| `Single = 1 -> XForma.Controls.SelectionMode` | One item at a time; tapping it again unselects it. |

## Shapes

A theme's corner radii.

| Member | |
|---|---|
| `<Clone>$() -> XForma.Controls.Shapes` |  |
| `Equals(XForma.Controls.Shapes other) -> bool` |  |
| `Equals(object obj) -> bool` |  |
| `GetHashCode() -> int` |  |
| `LargeCornerRadius.get -> float` | Sheets and dialogs. |
| `LargeCornerRadius.init -> void` | Sheets and dialogs. |
| `MediumCornerRadius.get -> float` | Buttons, cards and popups. |
| `MediumCornerRadius.init -> void` | Buttons, cards and popups. |
| `Shapes() -> void` |  |
| `SmallCornerRadius.get -> float` | Small controls: chips, check boxes, text boxes. |
| `SmallCornerRadius.init -> void` | Small controls: chips, check boxes, text boxes. |
| `ToString() -> string` |  |
| `operator !=(XForma.Controls.Shapes left, XForma.Controls.Shapes right) -> bool` |  |
| `operator ==(XForma.Controls.Shapes left, XForma.Controls.Shapes right) -> bool` |  |

## Slider

A value picked by dragging a thumb along a track, with an optional header above it. A mouse press moves the thumb there and drags it; a touch drags it once it moves sideways (a vertical drag scrolls), and a tap moves it. With keyboard focus, the arrow keys step it by `SmallChange` (Left and Right follow the flow direction), Page Up and Page Down by `LargeChange`, and Home and End go to the ends. Its theme's template draws it, in the layout the slider measures from the theme's `SliderStyle`: the header, then the track across the middle of a band, inset by half a thumb at each end.

| Member | |
|---|---|
| `CoerceValue(double value) -> double` |  |
| `DefaultAccessibilityLabel.get -> string` |  |
| `DefaultAccessibilityRole.get -> XForma.AccessibilityRole` |  |
| `GetUserState() -> object` |  |
| `Header.get -> string` | Text above the track, which also names the slider for assistive technology. |
| `Header.set -> void` | Text above the track, which also names the slider for assistive technology. |
| `HeaderBounds.get -> XForma.Rect` | Where the header is, or an empty rectangle without one. |
| `IsAccessibilityActionSupported(XForma.AccessibilityAction action) -> bool` |  |
| `IsDragging.get -> bool` | Whether the thumb is being dragged. |
| `LargeChange.get -> double` |  |
| `LargeChange.set -> void` |  |
| `MeasureOverride(XForma.Size availableSize) -> XForma.Size` |  |
| `OnAttaching(XForma.Theme theme) -> void` |  |
| `OnKeyDown(XForma.KeyEventArgs keyInputEvent) -> void` |  |
| `OnPointerCanceled(XForma.PointerEventArgs e) -> void` |  |
| `OnPointerMoved(XForma.PointerEventArgs e) -> void` |  |
| `OnPointerPressed(XForma.PointerEventArgs e) -> void` |  |
| `OnPointerReleased(XForma.PointerEventArgs e) -> void` |  |
| `PerformAccessibilityAction(XForma.AccessibilityAction action) -> bool` |  |
| `SetAccessibilityRangeValue(double value) -> bool` |  |
| `SetUserState(object state) -> void` |  |
| `Slider() -> void` |  |
| `SmallChange.get -> double` |  |
| `SmallChange.set -> void` |  |
| `StepFrequency.get -> double` | The steps the value snaps to, from the minimum (0 for none). |
| `StepFrequency.set -> void` | The steps the value snaps to, from the minimum (0 for none). |
| `ThumbCenter.get -> XForma.Point` | The thumb's center. |
| `TrackBounds.get -> XForma.Rect` | Where the track runs: a line across the middle of the touch band, inset by half a thumb at each end. |
| `ValuePosition.get -> float` | Where along the track the value is: from its start (its right end, right to left). |
| `ValueProperty -> XForma.ViewProperty<XForma.Controls.Slider, double>` |  |

## SliderStyle

How a theme lays out a `Slider`: its header, then the track across the middle of a band, inset by half a thumb at each end.

| Member | |
|---|---|
| `<Clone>$() -> XForma.Controls.SliderStyle` |  |
| `DefaultWidth.get -> float` | The slider's width when it's offered any. |
| `DefaultWidth.init -> void` | The slider's width when it's offered any. |
| `Equals(XForma.Controls.SliderStyle other) -> bool` |  |
| `Equals(object obj) -> bool` |  |
| `GetHashCode() -> int` |  |
| `HeaderGap.get -> float` | The space between the header and the band. |
| `HeaderGap.init -> void` | The space between the header and the band. |
| `SliderStyle() -> void` |  |
| `ThumbSize.get -> float` | The thumb's diameter. |
| `ThumbSize.init -> void` | The thumb's diameter. |
| `ToString() -> string` |  |
| `TouchHeight.get -> float` | The height of the band the track runs across, which takes presses. |
| `TouchHeight.init -> void` | The height of the band the track runs across, which takes presses. |
| `operator !=(XForma.Controls.SliderStyle left, XForma.Controls.SliderStyle right) -> bool` |  |
| `operator ==(XForma.Controls.SliderStyle left, XForma.Controls.SliderStyle right) -> bool` |  |

## SwipeCommandCollection

What swiping a row does: primary commands one way, secondary the other.

| Member | |
|---|---|
| `IsFlickEnabled.get -> bool` | Whether a quick, short swipe runs its command as a full one does: false by default. A side that requires a long swipe never does. |
| `IsFlickEnabled.set -> void` | Whether a quick, short swipe runs its command as a full one does: false by default. A side that requires a long swipe never does. |
| `IsLongSwipeRequiredForPrimaryCommands.get -> bool` |  |
| `IsLongSwipeRequiredForPrimaryCommands.set -> void` |  |
| `IsLongSwipeRequiredForSecondaryCommands.get -> bool` |  |
| `IsLongSwipeRequiredForSecondaryCommands.set -> void` |  |
| `PrimaryCommands.get -> System.Collections.Generic.List<XForma.Controls.UICommand>` |  |
| `SecondaryCommands.get -> System.Collections.Generic.List<XForma.Controls.UICommand>` |  |
| `SwipeCommandCollection() -> void` |  |

## TabSelectionChangedEventArgs

The selected tab changed.

| Member | |
|---|---|
| `NavigationMode.get -> XForma.NavigationMode` |  |
| `PreviousSelection.get -> XForma.Page` |  |
| `Selection.get -> XForma.Page` |  |
| `TabSelectionChangedEventArgs(XForma.Page selection, XForma.Page previousSelection, XForma.NavigationMode navigationMode) -> void` |  |

## TabStrip

A row of tabs, one checked: the page it shows. Give it the `Frame` its pages show in, and it switches them as the selection changes, sliding from the tab's side; or handle `SelectionChanged`. A switch can also follow a gesture (see `FollowSwipe`). It draws over its siblings, so a page's shadow goes under it, and Tab still reaches it before them.

| Member | |
|---|---|
| `AddButton(XForma.Controls.TabStripItem tabButton, XForma.Page tabPage) -> void` |  |
| `BeginSwitch(int direction) -> XForma.Controls.TabSwitch` | Starts switching to the next tab (a positive direction) or the previous one (a negative direction), following a gesture: set the switch's `Progress` as it moves, then complete or cancel it. The strip's frame slides the tab's page in as far. Returns null if there's no tab that way, a switch is under way, or the current page cancels. |
| `FollowSwipe(XForma.Input.SwipingEventArgs e) -> void` | Switches tabs as a swipe moves (such as a `Swiping`): the content follows the finger, moving at twice its speed, so a swipe from the middle to the edge goes all the way; released, the switch completes past a third of the way, or with a flick, and goes back otherwise. A swipe to the right goes to the previous tab, as it pulls in the page on the left. Past the first or last tab, the page rubber-bands: it follows the finger with more and more resistance, and springs back. |
| `Frame.get -> XForma.Frame` | The frame the tabs' pages show in, or null (the default) to switch them yourself, with `SelectionChanged`. As the selection changes, the strip replaces the frame's page, sliding in from the side of the tab. |
| `Frame.set -> void` | The frame the tabs' pages show in, or null (the default) to switch them yourself, with `SelectionChanged`. As the selection changes, the strip replaces the frame's page, sliding in from the side of the tab. |
| `IsSwitching.get -> bool` | Whether a switch is following a gesture (see `BeginSwitch`): the selection indicator follows `SelectionPosition` directly, rather than sliding to it. |
| `Items.get -> System.Collections.Generic.IReadOnlyList<XForma.Controls.TabStripItem>` | The tabs, in order. |
| `OnThemeChanged(XForma.Theme theme) -> void` |  |
| `SelectRelative(int direction) -> void` |  |
| `SelectedIndex.get -> int` | The checked tab's index, or -1. |
| `SelectedPage.get -> XForma.Page` |  |
| `SelectionChanged -> System.EventHandler<XForma.Controls.TabSelectionChangedEventArgs>` |  |
| `SelectionPosition.get -> float` | Where the selection shows, in tabs: the checked tab's index, or between it and the next while a switch follows a gesture. A theme draws its selection indicator here. |
| `TabStrip() -> void` |  |
| `TransitionDuration.get -> System.TimeSpan` | How long the selection takes to move to another tab: its frame's `TransitionDuration`, so the indicator slides with the page. |

## TabStripItem

One of a `TabStrip`'s tabs: an icon and a label, checked while its page shows. Its theme's template draws it.

| Member | |
|---|---|
| `AccessibilityIsSelected.get -> bool` |  |
| `BackgroundColor.get -> XForma.Color` |  |
| `DefaultAccessibilityLabel.get -> string` |  |
| `DefaultAccessibilityRole.get -> XForma.AccessibilityRole` |  |
| `GetUserState() -> object` |  |
| `Icon.get -> XForma.Bitmap` |  |
| `Icon.set -> void` |  |
| `IsChecked.get -> bool` |  |
| `IsChecked.set -> void` |  |
| `IsCheckedChanged -> System.EventHandler<XForma.Controls.ToggledEventArgs>` |  |
| `OnAttaching(XForma.Theme theme) -> void` |  |
| `SetUserState(object state) -> void` |  |
| `TabPage.get -> XForma.Page` |  |
| `TabStripItem() -> void` |  |
| `Text.get -> string` |  |
| `Text.set -> void` |  |

## TabStripStyle

How a theme lays out a `TabStrip`.

| Member | |
|---|---|
| `<Clone>$() -> XForma.Controls.TabStripStyle` |  |
| `Equals(XForma.Controls.TabStripStyle other) -> bool` |  |
| `Equals(object obj) -> bool` |  |
| `GetHashCode() -> int` |  |
| `MaximumWidth.get -> float` | The widest the tabs spread across the strip. |
| `MaximumWidth.init -> void` | The widest the tabs spread across the strip. |
| `TabStripStyle() -> void` |  |
| `ToString() -> string` |  |
| `operator !=(XForma.Controls.TabStripStyle left, XForma.Controls.TabStripStyle right) -> bool` |  |
| `operator ==(XForma.Controls.TabStripStyle left, XForma.Controls.TabStripStyle right) -> bool` |  |

## TabSwitch

A switch to the next or previous tab that follows a gesture (see `BeginSwitch`): the selection and the tab's page move as far as the `Progress`, then it completes or goes back.

| Member | |
|---|---|
| `Cancel() -> void` | Goes back to the tab it started from. |
| `Complete() -> void` | Goes the rest of the way: the tab is checked, and its page slides the rest of the way in. |
| `Progress.get -> float` | How far it's gone: 0 (still on the tab it started from) to 1 (all the way to the next). |
| `Progress.set -> void` | How far it's gone: 0 (still on the tab it started from) to 1 (all the way to the next). |

## TemplateContext

What a template can do to its control that only the control itself could: add and remove parts, create animators, and invalidate.

| Member | |
|---|---|
| `AddPart(XForma.View part) -> void` | Adds a child view to the control, beneath its content. |
| `CreateDrawnAnimator() -> XForma.DrawnAnimator` | Creates a scale and opacity applied to the control's drawing (see `DrawnAnimator`), for feedback such as a press. |
| `CreateOwnAnimator() -> XForma.Animator` | Creates one of the control's own animators (a native layer). Only in `Apply` as the control is first attached: it throws once the control's native object exists. |
| `InvalidateMeasure() -> void` | Measures and arranges the control again. |
| `InvalidateVisual() -> void` | Redraws the control. |
| `RemovePart(XForma.View part) -> void` | Removes a part the template added. |
| `State.get -> object` | Anything the template keeps for this control (a part, an animator), since one template serves every control of its type. Cleared when the control's template changes. |
| `State.set -> void` | Anything the template keeps for this control (a part, an animator), since one template serves every control of its type. Cleared when the control's template changes. |

## TemplatedContentControl

Base for content controls whose look comes from their theme, as `TemplatedControl` for owner-drawn ones: the theme's `ControlTemplate` gives the control its background and feedback, and can add parts beneath its content.

| Member | |
|---|---|
| `ArrangeOverride(XForma.Size finalSize) -> void` |  |
| `BackgroundColor.get -> XForma.Color` | The background: the template's. |
| `IsPressed.get -> bool` | Whether the control is being pressed. |
| `IsPressed.set -> void` | Whether the control is being pressed. |
| `MeasureOverride(XForma.Size availableSize) -> XForma.Size` |  |
| `OnAttaching(XForma.Theme theme) -> void` |  |
| `OnGotFocus() -> void` |  |
| `OnHotReload() -> void` |  |
| `OnLostFocus() -> void` |  |
| `OnPointerEntered(XForma.PointerEventArgs e) -> void` |  |
| `OnPointerExited(XForma.PointerEventArgs e) -> void` |  |
| `OnStateChanged() -> void` | Tells the template, and listeners, that the control's state changed. |
| `OnThemeChanged(XForma.Theme theme) -> void` |  |
| `StateChanged -> System.EventHandler` | Raised when the control's state changes: pressed, pointer over, focused, or its own. |
| `Template.get -> XForma.Controls.ControlTemplate` | The template giving the control its look, once it's attached. |
| `TemplatedContentControl() -> void` |  |

## TemplatedControl

Base for controls whose look comes from their theme: the control says what it does and what state it's in, and its theme's `ControlTemplate` draws it, adds its parts and gives feedback.

| Member | |
|---|---|
| `ArrangeOverride(XForma.Size finalSize) -> void` |  |
| `IsEnabled.get -> bool` | Whether the control responds to the user. A disabled control takes no input or keyboard focus (it isn't hit-test visible), and reads as disabled to assistive technology. |
| `IsEnabled.set -> void` | Whether the control responds to the user. A disabled control takes no input or keyboard focus (it isn't hit-test visible), and reads as disabled to assistive technology. |
| `IsPressed.get -> bool` | Whether the control is being pressed. |
| `IsPressed.set -> void` | Whether the control is being pressed. |
| `MeasureOverride(XForma.Size availableSize) -> XForma.Size` |  |
| `OnAttaching(XForma.Theme theme) -> void` |  |
| `OnGotFocus() -> void` |  |
| `OnHotReload() -> void` |  |
| `OnLostFocus() -> void` |  |
| `OnPointerEntered(XForma.PointerEventArgs e) -> void` |  |
| `OnPointerExited(XForma.PointerEventArgs e) -> void` |  |
| `OnStateChanged() -> void` | Tells the template, and listeners, that the control's state changed, and redraws it. Subclasses call it for their own state (such as checked or selected). |
| `OnThemeChanged(XForma.Theme theme) -> void` |  |
| `Render(XForma.DrawingContext drawingContext) -> void` |  |
| `StateChanged -> System.EventHandler` | Raised when the control's state changes: pressed, pointer over, focused, enabled, or its own (such as checked). |
| `Template.get -> XForma.Controls.ControlTemplate` | The template drawing the control, once it's attached. |
| `TemplatedControl() -> void` |  |

## TextStyle

A text style in a theme's type scale.

| Member | |
|---|---|
| `<Clone>$() -> XForma.Controls.TextStyle` |  |
| `Equals(XForma.Controls.TextStyle other) -> bool` |  |
| `Equals(object obj) -> bool` |  |
| `FontFamily.get -> string` |  |
| `FontFamily.init -> void` |  |
| `FontSize.get -> float` |  |
| `FontSize.init -> void` |  |
| `FontWeight.get -> XForma.FontWeight` |  |
| `FontWeight.init -> void` |  |
| `GetHashCode() -> int` |  |
| `TextStyle() -> void` |  |
| `ToString() -> string` |  |
| `ToTextFormat() -> XForma.TextFormat` | A new text format with the style's family, size and weight. |
| `operator !=(XForma.Controls.TextStyle left, XForma.Controls.TextStyle right) -> bool` |  |
| `operator ==(XForma.Controls.TextStyle left, XForma.Controls.TextStyle right) -> bool` |  |

## ThemeTemplates

The controls' templates in a theme: `theme.GetTemplate(type)` and `theme.WithTemplate(template)`, as for a view and its descendants: `view.Theme = theme.WithTemplate(new MyButtonTemplate())`.

| Member | |
|---|---|
| `GetTemplate(this XForma.Theme theme, System.Type controlType) -> XForma.Controls.ControlTemplate` |  |
| `WithTemplate(this XForma.Theme theme, XForma.Controls.ControlTemplate template) -> XForma.Theme` |  |
| `extension(XForma.Theme)` |  |
| `extension(XForma.Theme).GetTemplate(System.Type controlType) -> XForma.Controls.ControlTemplate` |  |
| `extension(XForma.Theme).WithTemplate(XForma.Controls.ControlTemplate template) -> XForma.Theme` |  |

## ThemeTokens

XForma.Controls' tokens in a theme (its extensions): the type scale, shapes and motion (`theme.Typography` and so on) and the controls' styles (`theme.CheckBoxStyle` and so on). These declare only their shape: only a theme package has their values, and a theme without one that's needed throws, as a control with no template does.

| Member | |
|---|---|
| `extension(XForma.Theme)` |  |
| `extension(XForma.Theme).CalendarViewStyle.get -> XForma.Controls.CalendarViewStyle` |  |
| `extension(XForma.Theme).CheckBoxStyle.get -> XForma.Controls.CheckBoxStyle` |  |
| `extension(XForma.Theme).CommandBarStyle.get -> XForma.Controls.CommandBarStyle` |  |
| `extension(XForma.Theme).DaysOfWeekPickerStyle.get -> XForma.Controls.DaysOfWeekPickerStyle` |  |
| `extension(XForma.Theme).ExpanderStyle.get -> XForma.Controls.ExpanderStyle` |  |
| `extension(XForma.Theme).FlipViewStyle.get -> XForma.Controls.FlipViewStyle` |  |
| `extension(XForma.Theme).FlyoutButtonStyle.get -> XForma.Controls.FlyoutButtonStyle` |  |
| `extension(XForma.Theme).InfoBarStyle.get -> XForma.Controls.InfoBarStyle` |  |
| `extension(XForma.Theme).ListViewStyle.get -> XForma.Controls.ListViewStyle` |  |
| `extension(XForma.Theme).MenuBarStyle.get -> XForma.Controls.MenuBarStyle` |  |
| `extension(XForma.Theme).MenuFlyoutStyle.get -> XForma.Controls.MenuFlyoutStyle` |  |
| `extension(XForma.Theme).Motion.get -> XForma.Controls.Motion` |  |
| `extension(XForma.Theme).NavigationViewStyle.get -> XForma.Controls.NavigationViewStyle` |  |
| `extension(XForma.Theme).NumberBoxStyle.get -> XForma.Controls.NumberBoxStyle` |  |
| `extension(XForma.Theme).PasswordBoxStyle.get -> XForma.Controls.PasswordBoxStyle` |  |
| `extension(XForma.Theme).PickerFieldStyle.get -> XForma.Controls.PickerFieldStyle` |  |
| `extension(XForma.Theme).PipsPagerStyle.get -> XForma.Controls.PipsPagerStyle` |  |
| `extension(XForma.Theme).PopupRowHeight.get -> float` |  |
| `extension(XForma.Theme).PopupStyle.get -> XForma.Controls.PopupStyle` |  |
| `extension(XForma.Theme).ProgressBarStyle.get -> XForma.Controls.ProgressBarStyle` |  |
| `extension(XForma.Theme).RadioButtonStyle.get -> XForma.Controls.RadioButtonStyle` |  |
| `extension(XForma.Theme).Shapes.get -> XForma.Controls.Shapes` |  |
| `extension(XForma.Theme).SliderStyle.get -> XForma.Controls.SliderStyle` |  |
| `extension(XForma.Theme).TabStripStyle.get -> XForma.Controls.TabStripStyle` |  |
| `extension(XForma.Theme).TimePickerViewStyle.get -> XForma.Controls.TimePickerViewStyle` |  |
| `extension(XForma.Theme).ToggleSwitchStyle.get -> XForma.Controls.ToggleSwitchStyle` |  |
| `extension(XForma.Theme).ToolTipStyle.get -> XForma.Controls.ToolTipStyle` |  |
| `extension(XForma.Theme).TreeViewStyle.get -> XForma.Controls.TreeViewStyle` |  |
| `extension(XForma.Theme).Typography.get -> XForma.Controls.Typography` |  |
| `get_CalendarViewStyle(XForma.Theme theme) -> XForma.Controls.CalendarViewStyle` |  |
| `get_CheckBoxStyle(XForma.Theme theme) -> XForma.Controls.CheckBoxStyle` |  |
| `get_CommandBarStyle(XForma.Theme theme) -> XForma.Controls.CommandBarStyle` |  |
| `get_DaysOfWeekPickerStyle(XForma.Theme theme) -> XForma.Controls.DaysOfWeekPickerStyle` |  |
| `get_ExpanderStyle(XForma.Theme theme) -> XForma.Controls.ExpanderStyle` |  |
| `get_FlipViewStyle(XForma.Theme theme) -> XForma.Controls.FlipViewStyle` |  |
| `get_FlyoutButtonStyle(XForma.Theme theme) -> XForma.Controls.FlyoutButtonStyle` |  |
| `get_InfoBarStyle(XForma.Theme theme) -> XForma.Controls.InfoBarStyle` |  |
| `get_ListViewStyle(XForma.Theme theme) -> XForma.Controls.ListViewStyle` |  |
| `get_MenuBarStyle(XForma.Theme theme) -> XForma.Controls.MenuBarStyle` |  |
| `get_MenuFlyoutStyle(XForma.Theme theme) -> XForma.Controls.MenuFlyoutStyle` |  |
| `get_Motion(XForma.Theme theme) -> XForma.Controls.Motion` |  |
| `get_NavigationViewStyle(XForma.Theme theme) -> XForma.Controls.NavigationViewStyle` |  |
| `get_NumberBoxStyle(XForma.Theme theme) -> XForma.Controls.NumberBoxStyle` |  |
| `get_PasswordBoxStyle(XForma.Theme theme) -> XForma.Controls.PasswordBoxStyle` |  |
| `get_PickerFieldStyle(XForma.Theme theme) -> XForma.Controls.PickerFieldStyle` |  |
| `get_PipsPagerStyle(XForma.Theme theme) -> XForma.Controls.PipsPagerStyle` |  |
| `get_PopupRowHeight(XForma.Theme theme) -> float` |  |
| `get_PopupStyle(XForma.Theme theme) -> XForma.Controls.PopupStyle` |  |
| `get_ProgressBarStyle(XForma.Theme theme) -> XForma.Controls.ProgressBarStyle` |  |
| `get_RadioButtonStyle(XForma.Theme theme) -> XForma.Controls.RadioButtonStyle` |  |
| `get_Shapes(XForma.Theme theme) -> XForma.Controls.Shapes` |  |
| `get_SliderStyle(XForma.Theme theme) -> XForma.Controls.SliderStyle` |  |
| `get_TabStripStyle(XForma.Theme theme) -> XForma.Controls.TabStripStyle` |  |
| `get_TimePickerViewStyle(XForma.Theme theme) -> XForma.Controls.TimePickerViewStyle` |  |
| `get_ToggleSwitchStyle(XForma.Theme theme) -> XForma.Controls.ToggleSwitchStyle` |  |
| `get_ToolTipStyle(XForma.Theme theme) -> XForma.Controls.ToolTipStyle` |  |
| `get_TreeViewStyle(XForma.Theme theme) -> XForma.Controls.TreeViewStyle` |  |
| `get_Typography(XForma.Theme theme) -> XForma.Controls.Typography` |  |

## TimePicker

A field showing the selected time; clicking it opens a popup with a column each for the hour, the minute and (on a 12-hour clock) AM or PM, and buttons to accept or dismiss the change.

| Member | |
|---|---|
| `ClockIdentifier.get -> string` | The clock the hours follow: `TwelveHourClock` or `TwentyFourHourClock`; by default, the culture's. |
| `ClockIdentifier.set -> void` | The clock the hours follow: `TwelveHourClock` or `TwentyFourHourClock`; by default, the culture's. |
| `ClosePicker() -> void` |  |
| `DefaultAccessibilityLabel.get -> string` |  |
| `DisplayText.get -> string` |  |
| `FormatTime(System.TimeSpan time) -> string` | The time as the field shows it. |
| `GetUserState() -> object` |  |
| `Is24HourClock.get -> bool` | Whether the hours run from 0 to 23. |
| `IsPickerOpen.get -> bool` |  |
| `IsPlaceholder.get -> bool` |  |
| `MaximumTextWidth.get -> float` |  |
| `MinuteIncrement.get -> int` | The steps the minutes can be chosen in. |
| `MinuteIncrement.set -> void` | The steps the minutes can be chosen in. |
| `OnAttaching(XForma.Theme theme) -> void` |  |
| `OpenPicker() -> void` |  |
| `PlaceholderText.get -> string` | Shown while no time is selected. |
| `PlaceholderText.set -> void` | Shown while no time is selected. |
| `SelectedTime.get -> System.TimeSpan?` | The time of day chosen (hours and minutes), or null for none. |
| `SelectedTime.set -> void` | The time of day chosen (hours and minutes), or null for none. |
| `SelectedTimeChanged -> System.EventHandler<System.TimeSpan?>` | Raised when the user accepts a time. |
| `SelectedTimeProperty -> XForma.ViewProperty<XForma.Controls.TimePicker, System.TimeSpan?>` |  |
| `SetUserState(object state) -> void` |  |
| `TimePicker() -> void` |  |
| `TwelveHourClock = "12HourClock" -> string` | A `ClockIdentifier`: hours from 1 to 12, with AM and PM. |
| `TwentyFourHourClock = "24HourClock" -> string` | A `ClockIdentifier`: hours from 0 to 23. |

## TimePickerView

A time picker's popup: a column each for the hour, the minute and (on a 12-hour clock) AM or PM, each showing its value in the middle row with its neighbors above and below, and Accept and Dismiss buttons beneath. Tapping a neighbor (or the wheel) moves a column to it. With keyboard focus, Up and Down change the focused column, Left and Right move between columns, Enter accepts and Escape dismisses. Its theme's template draws it, in the layout it measures.

| Member | |
|---|---|
| `AcceptBounds.get -> XForma.Rect` |  |
| `AccessibilityValue.get -> string` |  |
| `ColumnCount.get -> int` | The number of columns: hour, minute, and AM or PM on a 12-hour clock. |
| `DismissBounds.get -> XForma.Rect` |  |
| `FocusedColumn.get -> int` | The column the keyboard is on. |
| `GetColumnBounds(int column) -> XForma.Rect` | Where a column is (the first is the hour's, at the start). |
| `GetRowBounds(int column, int row) -> XForma.Rect` | Where a row of a column is (row 2 is the middle, the value). |
| `GetText(int column, int offset) -> string` | The text of a column's value moved by the offset: the hour, the minute, or AM or PM. |
| `PressedColumn.get -> int` | The column and row being pressed, when `PressedPart` is a column. |
| `PressedPart.get -> XForma.Controls.TimePickerViewPart` |  |
| `PressedRow.get -> int` |  |
| `Time.get -> System.TimeSpan` | The time shown. |
| `TimePicker.get -> XForma.Controls.TimePicker` |  |
| `VisibleRows = 5 -> int` | The rows shown in a column: the value in the middle, and two above and below. |

## TimePickerViewPart

The part of a time picker's popup being pressed.

| Member | |
|---|---|
| `Accept = 2 -> XForma.Controls.TimePickerViewPart` |  |
| `Column = 1 -> XForma.Controls.TimePickerViewPart` |  |
| `Dismiss = 3 -> XForma.Controls.TimePickerViewPart` |  |
| `None = 0 -> XForma.Controls.TimePickerViewPart` |  |

## TimePickerViewStyle

How a theme lays out a `TimePickerView`: its columns of rows, then Accept and Dismiss.

| Member | |
|---|---|
| `<Clone>$() -> XForma.Controls.TimePickerViewStyle` |  |
| `ButtonHeight.get -> float` | The height of Accept and Dismiss. |
| `ButtonHeight.init -> void` | The height of Accept and Dismiss. |
| `ColumnWidth.get -> float` | The width of a column. |
| `ColumnWidth.init -> void` | The width of a column. |
| `Equals(XForma.Controls.TimePickerViewStyle other) -> bool` |  |
| `Equals(object obj) -> bool` |  |
| `GetHashCode() -> int` |  |
| `RowHeight.get -> float` | The height of a row. |
| `RowHeight.init -> void` | The height of a row. |
| `TimePickerViewStyle() -> void` |  |
| `ToString() -> string` |  |
| `operator !=(XForma.Controls.TimePickerViewStyle left, XForma.Controls.TimePickerViewStyle right) -> bool` |  |
| `operator ==(XForma.Controls.TimePickerViewStyle left, XForma.Controls.TimePickerViewStyle right) -> bool` |  |

## ToggleButton

A button with an icon and a label that toggles between checked and unchecked when tapped. Its theme's template draws it.

| Member | |
|---|---|
| `AccessibilityIsChecked.get -> bool?` |  |
| `Icon.get -> XForma.Bitmap` |  |
| `Icon.set -> void` |  |
| `IsChecked.get -> bool` |  |
| `IsChecked.set -> void` |  |
| `IsCheckedChanged -> System.EventHandler<XForma.Controls.ToggledEventArgs>` |  |
| `IsCheckedProperty -> XForma.ViewProperty<XForma.Controls.ToggleButton, bool>` |  |
| `Text.get -> string` |  |
| `Text.set -> void` |  |
| `ToggleButton() -> void` |  |

## ToggleSwitch

An on and off switch, with an optional header above it and its state's text beside it, turned by a tap, Space or Enter. Its theme's template draws it, in the layout the switch measures: the header, then a track at the start of a band, then the text after it, as the theme's `ToggleSwitchStyle` says.

| Member | |
|---|---|
| `AccessibilityIsChecked.get -> bool?` |  |
| `CreateHeaderFormat() -> XForma.TextFormat` |  |
| `CreateTextFormat() -> XForma.TextFormat` |  |
| `DefaultAccessibilityLabel.get -> string` |  |
| `DefaultAccessibilityRole.get -> XForma.AccessibilityRole` |  |
| `GetUserState() -> object` |  |
| `Header.get -> string` | Text above the switch, which also names it for assistive technology. |
| `Header.set -> void` | Text above the switch, which also names it for assistive technology. |
| `HeaderBounds.get -> XForma.Rect` | Where the header is (empty without one). |
| `IsAccessibilityActionSupported(XForma.AccessibilityAction action) -> bool` |  |
| `IsOn.get -> bool` |  |
| `IsOn.set -> void` |  |
| `IsOnProperty -> XForma.ViewProperty<XForma.Controls.ToggleSwitch, bool>` |  |
| `MeasureOverride(XForma.Size availableSize) -> XForma.Size` |  |
| `OffContent.get -> string` | The text beside the switch while it's off. |
| `OffContent.set -> void` | The text beside the switch while it's off. |
| `OnActivate() -> void` |  |
| `OnAttaching(XForma.Theme theme) -> void` |  |
| `OnContent.get -> string` | The text beside the switch while it's on. |
| `OnContent.set -> void` | The text beside the switch while it's on. |
| `SetUserState(object state) -> void` |  |
| `StateText.get -> string` | The text beside the switch now. |
| `TextBounds.get -> XForma.Rect` | Where the state's text is: after the track. |
| `ToggleSwitch() -> void` |  |
| `Toggled -> System.EventHandler<XForma.Controls.ToggledEventArgs>` | Raised when `IsOn` changes. |
| `TrackBounds.get -> XForma.Rect` | Where the track is: at the start of the band. |

## ToggleSwitchStyle

How a theme lays out a `ToggleSwitch`: its header, then a track at the start of a band, then the state's text.

| Member | |
|---|---|
| `<Clone>$() -> XForma.Controls.ToggleSwitchStyle` |  |
| `Equals(XForma.Controls.ToggleSwitchStyle other) -> bool` |  |
| `Equals(object obj) -> bool` |  |
| `GetHashCode() -> int` |  |
| `HeaderGap.get -> float` | The space between the header and the band. |
| `HeaderGap.init -> void` | The space between the header and the band. |
| `TextGap.get -> float` | The space between the track and the text. |
| `TextGap.init -> void` | The space between the track and the text. |
| `ToString() -> string` |  |
| `ToggleSwitchStyle() -> void` |  |
| `TouchHeight.get -> float` | The height of the band the track is in, which takes presses. |
| `TouchHeight.init -> void` | The height of the band the track is in, which takes presses. |
| `TrackHeight.get -> float` | The track's height. |
| `TrackHeight.init -> void` | The track's height. |
| `TrackWidth.get -> float` | The track's width. |
| `TrackWidth.init -> void` | The track's width. |
| `operator !=(XForma.Controls.ToggleSwitchStyle left, XForma.Controls.ToggleSwitchStyle right) -> bool` |  |
| `operator ==(XForma.Controls.ToggleSwitchStyle left, XForma.Controls.ToggleSwitchStyle right) -> bool` |  |

## ToggledEventArgs

A control was checked or unchecked.

| Member | |
|---|---|
| `ToggledEventArgs(bool value) -> void` |  |
| `Value.get -> bool` |  |

## ToolTip

A short description shown beside a view: the content of the popup a view's tooltip (see `ViewAttachments`) opens. Its theme's template draws it, in the layout it measures: the text, wrapped, inside its padding.

| Member | |
|---|---|
| `CreateTextFormat() -> XForma.TextFormat` |  |
| `Text.get -> string` |  |
| `Text.set -> void` |  |
| `ToolTip() -> void` |  |

## ToolTipStyle

How a theme lays out a `ToolTip`.

| Member | |
|---|---|
| `<Clone>$() -> XForma.Controls.ToolTipStyle` |  |
| `Equals(XForma.Controls.ToolTipStyle other) -> bool` |  |
| `Equals(object obj) -> bool` |  |
| `GetHashCode() -> int` |  |
| `MaximumWidth.get -> float` | The widest a tooltip grows before its text wraps. |
| `MaximumWidth.init -> void` | The widest a tooltip grows before its text wraps. |
| `Padding.get -> XForma.Thickness` | The space around the text. |
| `Padding.init -> void` | The space around the text. |
| `ToString() -> string` |  |
| `ToolTipStyle() -> void` |  |
| `operator !=(XForma.Controls.ToolTipStyle left, XForma.Controls.ToolTipStyle right) -> bool` |  |
| `operator ==(XForma.Controls.ToolTipStyle left, XForma.Controls.ToolTipStyle right) -> bool` |  |

## TreeView

Nested items that expand and collapse, one selected: a tap selects an item (and on its chevron, expands or collapses it); with keyboard focus, Up and Down move between the items shown, Right expands an item (or goes to its first child), Left collapses it (or goes to its parent), Home and End go to the ends, and Enter or Space selects. Tab stops once, at the item focus was last on (or the selected one). Its theme's template draws its items.

| Member | |
|---|---|
| `DefaultAccessibilityRole.get -> XForma.AccessibilityRole` |  |
| `ItemInvoked -> System.EventHandler<XForma.Controls.TreeViewItemInvokedEventArgs>` | Raised when the user chooses an item (a tap, Enter or Space). |
| `OnAttaching(XForma.Theme theme) -> void` |  |
| `Refresh() -> void` | Shows the nodes again, after the app changed them. |
| `RootNodes.get -> System.Collections.Generic.List<XForma.Controls.TreeViewNode>` | The top-level nodes, in order. |
| `SelectedNode.get -> XForma.Controls.TreeViewNode` |  |
| `SelectedNode.set -> void` |  |
| `TreeView() -> void` |  |

## TreeViewItem

An item in a tree view: a node's text, indented by its level, with a chevron when it has children. Its theme's template draws it.

| Member | |
|---|---|
| `AccessibilityIsExpanded.get -> bool?` |  |
| `AccessibilityIsSelected.get -> bool` |  |
| `ChevronBounds.get -> XForma.Rect` | Where the chevron is. |
| `CreateTextFormat() -> XForma.TextFormat` |  |
| `IsAccessibilityActionSupported(XForma.AccessibilityAction action) -> bool` |  |
| `IsSelected.get -> bool` |  |
| `Level.get -> int` | How deep the node is (0 for a root). |
| `Node.get -> XForma.Controls.TreeViewNode` |  |
| `PerformAccessibilityAction(XForma.AccessibilityAction action) -> bool` |  |
| `TextBounds.get -> XForma.Rect` | Where the text is. |
| `TreeView.get -> XForma.Controls.TreeView` |  |

## TreeViewItemInvokedEventArgs

The node chosen in a tree view.

| Member | |
|---|---|
| `Node.get -> XForma.Controls.TreeViewNode` |  |
| `TreeViewItemInvokedEventArgs(XForma.Controls.TreeViewNode node) -> void` |  |

## TreeViewNode

An item in a tree: its text, the items under it, and whether they show. The app's data, which the tree view shows.

| Member | |
|---|---|
| `Children.get -> System.Collections.Generic.List<XForma.Controls.TreeViewNode>` |  |
| `Content.get -> string` |  |
| `Content.set -> void` |  |
| `HasChildren.get -> bool` |  |
| `IsExpanded.get -> bool` |  |
| `IsExpanded.set -> void` |  |
| `Tag.get -> object` | Anything the app keeps with the node. |
| `Tag.set -> void` | Anything the app keeps with the node. |
| `TreeViewNode() -> void` |  |

## TreeViewStyle

How a theme lays out a `TreeView`'s items.

| Member | |
|---|---|
| `<Clone>$() -> XForma.Controls.TreeViewStyle` |  |
| `ChevronWidth.get -> float` | The width of the chevron's area, which takes presses. |
| `ChevronWidth.init -> void` | The width of the chevron's area, which takes presses. |
| `Equals(XForma.Controls.TreeViewStyle other) -> bool` |  |
| `Equals(object obj) -> bool` |  |
| `GetHashCode() -> int` |  |
| `Indent.get -> float` | How far each level is indented. |
| `Indent.init -> void` | How far each level is indented. |
| `ItemHeight.get -> float` | An item's height. |
| `ItemHeight.init -> void` | An item's height. |
| `ItemInset.get -> float` | The space at each end of an item. |
| `ItemInset.init -> void` | The space at each end of an item. |
| `ToString() -> string` |  |
| `TreeViewStyle() -> void` |  |
| `operator !=(XForma.Controls.TreeViewStyle left, XForma.Controls.TreeViewStyle right) -> bool` |  |
| `operator ==(XForma.Controls.TreeViewStyle left, XForma.Controls.TreeViewStyle right) -> bool` |  |

## Typography

A theme's type scale.

| Member | |
|---|---|
| `<Clone>$() -> XForma.Controls.Typography` |  |
| `Body.get -> XForma.Controls.TextStyle` | Body text. |
| `Body.init -> void` | Body text. |
| `Caption.get -> XForma.Controls.TextStyle` | Small supporting text. |
| `Caption.init -> void` | Small supporting text. |
| `Display.get -> XForma.Controls.TextStyle` | Large numbers and headlines. |
| `Display.init -> void` | Large numbers and headlines. |
| `Equals(XForma.Controls.Typography other) -> bool` |  |
| `Equals(object obj) -> bool` |  |
| `GetHashCode() -> int` |  |
| `Label.get -> XForma.Controls.TextStyle` | Text in controls: buttons, tabs, fields. |
| `Label.init -> void` | Text in controls: buttons, tabs, fields. |
| `Title.get -> XForma.Controls.TextStyle` | Page and section titles. |
| `Title.init -> void` | Page and section titles. |
| `ToString() -> string` |  |
| `Typography() -> void` |  |
| `operator !=(XForma.Controls.Typography left, XForma.Controls.Typography right) -> bool` |  |
| `operator ==(XForma.Controls.Typography left, XForma.Controls.Typography right) -> bool` |  |

## UICommand

A command with its text and icon, for buttons and menus.

| Member | |
|---|---|
| `AccentColor.get -> XForma.Color` |  |
| `AccentColor.set -> void` |  |
| `CanExecute(object parameter) -> bool` |  |
| `CanExecuteChanged -> System.EventHandler` | Raised when the wrapped command's ability to run changes. |
| `Command.get -> System.Windows.Input.ICommand` |  |
| `Command.set -> void` |  |
| `Execute(object parameter) -> void` |  |
| `ForegroundColor.get -> XForma.Color?` | The command's icon and text on its `AccentColor`, or null (the default) for what its theme chooses. |
| `ForegroundColor.set -> void` | The command's icon and text on its `AccentColor`, or null (the default) for what its theme chooses. |
| `Icon.get -> XForma.Bitmap` |  |
| `Icon.set -> void` |  |
| `Text.get -> string` |  |
| `Text.set -> void` |  |
| `UICommand() -> void` |  |

## UriNavigationCommand

A command that opens a URI.

| Member | |
|---|---|
| `CanExecute(object parameter) -> bool` |  |
| `CanExecuteChanged -> System.EventHandler` |  |
| `Execute(object parameter) -> void` |  |
| `NavigationUri.get -> System.Uri` |  |
| `NavigationUri.set -> void` |  |
| `UriNavigationCommand() -> void` |  |

## UserControl

An owner-drawn control: it draws itself (in `Render`), beneath one child, its content, inside its padding.

| Member | |
|---|---|
| `Build() -> XForma.View` | Builds the control's views, and returns its content. XForma calls it once, as the control is first attached (before its native object exists), and again after an edit while the app runs with hot reload. A control whose content is given to it instead (as a border's child) sets `Content`. |
| `Content.get -> XForma.View` | The control's content: what `Build` returns, or what the control is given. |
| `Content.set -> void` | The control's content: what `Build` returns, or what the control is given. |
| `IsBuilt.get -> bool` | Whether `Build` has run: the control's views exist. |
| `OnAttaching(XForma.Theme theme) -> void` |  |
| `OnHotReload() -> void` |  |
| `Padding.get -> XForma.Thickness` | Space between the control's edges and its content. |
| `Padding.set -> void` | Space between the control's edges and its content. |
| `UserControl() -> void` |  |

## ViewAttachments

What any view can be given beyond its own properties, writable in an initializer: a tooltip, and a context menu.

| Member | |
|---|---|
| `extension(XForma.View)` |  |
| `extension(XForma.View).ContextFlyout.get -> XForma.Primitives.Popup` |  |
| `extension(XForma.View).ContextFlyout.set -> void` |  |
| `extension(XForma.View).ToolTip.get -> string` |  |
| `extension(XForma.View).ToolTip.set -> void` |  |
| `get_ContextFlyout(XForma.View view) -> XForma.Primitives.Popup` |  |
| `get_ToolTip(XForma.View view) -> string` |  |
| `set_ContextFlyout(XForma.View view, XForma.Primitives.Popup value) -> void` |  |
| `set_ToolTip(XForma.View view, string value) -> void` |  |

## ViewCollection

A panel's children, in the order they were added. A panel creates its collection in its constructor and exposes it as `Children`.

| Member | |
|---|---|
| `Add(XForma.View child) -> void` |  |
| `Clear() -> void` |  |
| `Contains(XForma.View child) -> bool` |  |
| `Count.get -> int` |  |
| `GetEnumerator() -> System.Collections.Generic.IEnumerator<XForma.View>` |  |
| `IndexOf(XForma.View child) -> int` |  |
| `Insert(int index, XForma.View child) -> void` |  |
| `Remove(XForma.View child) -> bool` |  |
| `RemoveAt(int index) -> void` |  |
| `Replace(int index, XForma.View child) -> void` |  |
| `ViewCollection(XForma.Controls.Panel owner) -> void` |  |
| `this[int index].get -> XForma.View` |  |

## ViewCollection<TLayout>

| Member | |
|---|---|
| `Add(XForma.View child, TLayout layout) -> void` |  |
| `GetLayout(XForma.View child) -> TLayout` |  |
| `Insert(int index, XForma.View child, TLayout layout) -> void` |  |
| `Replace(int index, XForma.View child, TLayout layout) -> void` |  |
| `SetLayout(XForma.View child, TLayout layout) -> void` |  |
| `ViewCollection(XForma.Controls.Panel owner, TLayout defaultLayout = default(TLayout)) -> void` |  |
