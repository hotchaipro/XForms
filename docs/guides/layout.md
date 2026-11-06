# Layout

A view says how big it wants to be, and its parent decides where it goes. You'll mostly combine
panels and set a few properties on the views inside them.

## Properties every view has

| Property | Means |
|---|---|
| `Width`, `Height` | A fixed size. `Dimension.Auto`, the default, fits the content |
| `MinWidth`, `MaxWidth`, `MinHeight`, `MaxHeight` | Limits |
| `Margin` | Space outside the view |
| `HorizontalAlignment`, `VerticalAlignment` | Where it goes in the room it's given, or `Stretch` to fill it |
| `IsVisible` | A hidden view takes no room |
| `FlowDirection` | `RightToLeft` mirrors the view and what's inside it |

Sizes are device-independent: the same number looks about the same size on every screen.

## Panels

**`StackPanel`** puts views in a line:

```csharp
var layout = new StackPanel() { Spacing = 12, Padding = new Thickness(24) };
layout.Children.Add(new TextBlock() { Text = "Name" });
layout.Children.Add(new TextBox() { PlaceholderText = "Your name" });
```

**`DockPanel`** docks views to its edges, in order. Use it for a bar above content:

```csharp
var layout = new DockPanel();
layout.Children.Add(new TextBlock() { Text = "Title" }, Dock.Top);
layout.Children.Add(new ListView<string>(), Dock.CenterOverlay);
```

**`Grid`** has rows and columns: a number for a fixed size, `Auto` to fit the content, `*` to share
what's left.

```csharp
var grid = new Grid() { ColumnSpacing = 8, RowSpacing = 4 };
grid.ColumnDefinitions.AddRange("Auto,*");
grid.RowDefinitions.AddRange("Auto,Auto");
grid.Children.Add(new TextBlock() { Text = "Name" }, 0, 0);
grid.Children.Add(new TextBox(), 0, 1);
grid.Children.Add(new Button() { Text = "Save" }, 1, 0, columnSpan: 2);
```

**`WrapPanel`** flows views and wraps at the edge. **`UniformGrid`** shares its width evenly.
**`Canvas`** puts views exactly where you say:

```csharp
var canvas = new Canvas();
canvas.Children.Add(new Border() { Width = 40, Height = 40, Background = Colors.Red }, 10, 20);
```

## Scrolling

`ScrollViewer` scrolls one view, with the platform's own scrolling. Lists scroll themselves; see
[Controls](controls.md).

```csharp
var content = new StackPanel();
var scroll = new ScrollViewer() { Content = content };
```

## Your own layout

Derive from `View`, and override `MeasureOverride` and `ArrangeOverride`. See
[Custom controls](custom-controls.md).
