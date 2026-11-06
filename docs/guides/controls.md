# Controls

Controls are in `XForma.Controls`, and text, text entry, images and scrolling are in
`XForma.Primitives`. `XFormaApp.Run`'s theme gives them their look.

| To | Use |
|---|---|
| Show text | `TextBlock` |
| Enter text | `TextBox`, `PasswordBox`, `NumberBox`, `AutoSuggestBox` |
| Act | `Button`, `HyperlinkButton`, `FloatingButton`, `FlyoutButton` |
| Choose | `CheckBox`, `ToggleSwitch`, `ToggleButton`, `RadioButtons`, `ComboBox`, `Slider` |
| Pick a date or time | `CalendarDatePicker`, `CalendarView`, `TimePicker`, `DaysOfWeekPicker` |
| Show a list | `ListView<T>`, `CommandListView`, `TreeView` |
| Move around | `NavigationView`, `TabStrip`, `MenuBar`, `CommandBar`, `FlipView` |
| Show more | `Expander`, `Flyout`, `MenuFlyout`, `ToolTip`, `MessageBox` |
| Report | `ProgressBar`, `ProgressRing`, `InfoBar` |
| Show an image or web page | `Image`, `WebView` |

The test app in `test/XForma.Test` uses all of them, and the [API reference](../api/README.md)
lists what each one has.

## Buttons and commands

A button raises `Clicked`, or runs a command:

```csharp
var save = new Button() { Text = "Save", Width = 120, Height = 40 };
save.Clicked += (sender, e) => { };

var delete = new Button() { Text = "Delete", Width = 120, Height = 40 };
delete.Command = new RelayCommand(() => { });
```

A command's `CanExecute` enables and disables the button for you.

## Text entry

`TextBox` is the platform's own text entry, so selection, spell checking, input methods and
autofill behave as they do in the platform's own apps:

```csharp
var email = new TextBox() { PlaceholderText = "Email" };
email.TextChanged += (sender, e) => { };
```

## Lists

`ListView<T>` shows items, and creates only the rows on screen. Give it the items, and say how to
make a row and show an item in it. Rows are reused, so set everything in the second function:

```csharp
var list = new ListView<string>()
{
    ItemsSource = new[] { "Apple", "Banana", "Cherry" },
    ItemViewFactory = ItemViewFactory<string>.Create(
        () => new TextBlock() { Margin = new Thickness(16, 10, 16, 10) },
        (row, fruit) => row.Text = fruit),
};
list.ItemClick += (sender, e) => { };
```

An `ObservableCollection<T>` as `ItemsSource` keeps the list up to date. `GroupKeySelector` groups
the items under headers. `CommandListView` adds swipe commands and a bar of commands on each row.

## Messages and menus

```csharp
var result = await MessageBox.ShowAsync("Delete this item?", "Delete");
if (result == MessageBoxResult.OK)
{
    // Delete it
}
```

A view's `ContextFlyout` shows on a right-click, a long press or the Menu key:

```csharp
var view = new Border() { Width = 100, Height = 100 };
view.ContextFlyout = new MenuFlyout()
{
    Items =
    {
        new MenuItem() { Text = "Copy", Command = new RelayCommand(() => { }) },
    },
};
```

## Navigation chrome

`NavigationView` is a side pane on a wide window, and a drawer on a narrow one:

```csharp
var navigation = new NavigationView() { PaneTitle = "Mail" };
navigation.MenuItems.Add(new NavigationViewItem() { Text = "Inbox" });
navigation.SelectionChanged += (sender, e) => { };
```

`TabStrip` shows pages under tabs, with swiping between them.
