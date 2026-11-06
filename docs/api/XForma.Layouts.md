# XForma.Layouts

## Canvas

Places each child at a position, at its desired size. The canvas asks for no space of its own; size it explicitly or let it stretch.

| Member | |
|---|---|
| `ArrangeChildren(XForma.Size finalSize) -> void` |  |
| `Canvas() -> void` |  |
| `Children.get -> XForma.Layouts.Canvas.CanvasChildren` |  |
| `MeasureChildren(XForma.Size availableSize) -> XForma.Size` |  |

## Dock

| Member | |
|---|---|
| `Bottom = 4 -> XForma.Layouts.Dock` |  |
| `BottomOverlay = 8 -> XForma.Layouts.Dock` |  |
| `CenterOverlay = 0 -> XForma.Layouts.Dock` |  |
| `Left = 1 -> XForma.Layouts.Dock` |  |
| `LeftOverlay = 5 -> XForma.Layouts.Dock` |  |
| `Right = 3 -> XForma.Layouts.Dock` |  |
| `RightOverlay = 7 -> XForma.Layouts.Dock` |  |
| `Top = 2 -> XForma.Layouts.Dock` |  |
| `TopOverlay = 6 -> XForma.Layouts.Dock` |  |

## DockPanel

Docks children to the edges in the order they were added, then fills the remaining center. Overlay regions dock without taking the space away from the children that follow.

| Member | |
|---|---|
| `ArrangeChildren(XForma.Size finalSize) -> void` |  |
| `Children.get -> XForma.Controls.ViewCollection<XForma.Layouts.Dock>` | The children, each docked to an edge or overlaid: `Children.Add(view, Dock.Top)`. |
| `DockPanel() -> void` |  |
| `MeasureChildren(XForma.Size availableSize) -> XForma.Size` |  |

## Grid

Arranges its children in rows and columns. Each row and column is Auto (sized to its content), a fixed length, or a share of the remaining space (star). Without definitions, the grid has one star row and column.

| Member | |
|---|---|
| `ArrangeChildren(XForma.Size finalSize) -> void` |  |
| `Children.get -> XForma.Layouts.Grid.GridChildren` |  |
| `ColumnDefinitions.get -> XForma.Layouts.Grid.DefinitionCollection` |  |
| `ColumnSpacing.get -> float` | The space between columns. |
| `ColumnSpacing.set -> void` | The space between columns. |
| `Grid() -> void` |  |
| `MeasureChildren(XForma.Size availableSize) -> XForma.Size` |  |
| `RowDefinitions.get -> XForma.Layouts.Grid.DefinitionCollection` |  |
| `RowSpacing.get -> float` | The space between rows. |
| `RowSpacing.set -> void` | The space between rows. |

## GridCell

The cell a grid child occupies.

| Member | |
|---|---|
| `Column.get -> int` |  |
| `ColumnSpan.get -> int` |  |
| `GridCell() -> void` |  |
| `GridCell(int row, int column, int rowSpan = 1, int columnSpan = 1) -> void` |  |
| `Row.get -> int` |  |
| `RowSpan.get -> int` |  |

## GridLength

The height of a grid row or the width of a column: Auto, a fixed length, or a weighted share of the remaining space (star).

| Member | |
|---|---|
| `Auto.get -> XForma.Layouts.GridLength` |  |
| `Equals(XForma.Layouts.GridLength other) -> bool` |  |
| `Equals(object obj) -> bool` |  |
| `GetHashCode() -> int` |  |
| `GridLength() -> void` |  |
| `GridLength(float value, XForma.Layouts.GridUnitType unitType = XForma.Layouts.GridUnitType.Pixel) -> void` |  |
| `GridUnitType.get -> XForma.Layouts.GridUnitType` |  |
| `IsAbsolute.get -> bool` |  |
| `IsAuto.get -> bool` |  |
| `IsStar.get -> bool` |  |
| `Parse(string text) -> XForma.Layouts.GridLength` | Parses "Auto", "*", "2*" or a fixed length such as "100". |
| `Star.get -> XForma.Layouts.GridLength` | One share of the remaining space ("*"). |
| `ToString() -> string` |  |
| `Value.get -> float` |  |
| `operator !=(XForma.Layouts.GridLength a, XForma.Layouts.GridLength b) -> bool` |  |
| `operator ==(XForma.Layouts.GridLength a, XForma.Layouts.GridLength b) -> bool` |  |

## GridUnitType

| Member | |
|---|---|
| `Auto = 0 -> XForma.Layouts.GridUnitType` | Sized to the content. |
| `Pixel = 1 -> XForma.Layouts.GridUnitType` | A fixed length. |
| `Star = 2 -> XForma.Layouts.GridUnitType` | A share of the space left over by the other rows or columns. |

## StackPanel

Stacks its children vertically (the default) or horizontally, with optional spacing between them. Children stretch across the panel unless their alignment says otherwise.

| Member | |
|---|---|
| `ArrangeChildren(XForma.Size finalSize) -> void` |  |
| `Children.get -> XForma.Controls.ViewCollection` |  |
| `MeasureChildren(XForma.Size availableSize) -> XForma.Size` |  |
| `Orientation.get -> XForma.Orientation` |  |
| `Orientation.set -> void` |  |
| `Spacing.get -> float` | The space between neighboring children. |
| `Spacing.set -> void` | The space between neighboring children. |
| `StackPanel() -> void` |  |

## UniformGrid

Stacks children horizontally and distributes the spare width evenly between them.

| Member | |
|---|---|
| `ArrangeChildren(XForma.Size finalSize) -> void` |  |
| `Children.get -> XForma.Controls.ViewCollection` |  |
| `MeasureChildren(XForma.Size availableSize) -> XForma.Size` |  |
| `UniformGrid() -> void` |  |

## WrapPanel

Places its children in a row (or column) and starts a new line when the next child doesn't fit.

| Member | |
|---|---|
| `ArrangeChildren(XForma.Size finalSize) -> void` |  |
| `Children.get -> XForma.Controls.ViewCollection` |  |
| `ItemSpacing.get -> float` | The space between neighboring children in a line. |
| `ItemSpacing.set -> void` | The space between neighboring children in a line. |
| `LineSpacing.get -> float` | The space between lines. |
| `LineSpacing.set -> void` | The space between lines. |
| `MeasureChildren(XForma.Size availableSize) -> XForma.Size` |  |
| `Orientation.get -> XForma.Orientation` | The direction of a line: horizontal (the default) fills rows. |
| `Orientation.set -> void` | The direction of a line: horizontal (the default) fills rows. |
| `WrapPanel() -> void` |  |
