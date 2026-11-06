# XForma.Layouts.Grid

## DefinitionCollection

Row heights or column widths; changing them lays the grid out again.

| Member | |
|---|---|
| `AddRange(string definitions) -> void` | Adds a definition for each part of a list such as "Auto,*,2*,100". |

## GridChildren

A grid's children, each in a cell.

| Member | |
|---|---|
| `Add(XForma.View child, int row, int column, int rowSpan = 1, int columnSpan = 1) -> void` | Adds a child in a cell, optionally spanning more rows or columns. |
