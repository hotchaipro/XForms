# XForma.Themes.Basic

## BasicColors

The colors the Basic theme draws with that a `ColorScheme` has no role for. Set one on a theme to change them, alongside a palette of its own.

| Member | |
|---|---|
| `<Clone>$() -> XForma.Themes.Basic.BasicColors` |  |
| `BasicColors() -> void` |  |
| `Equals(XForma.Themes.Basic.BasicColors other) -> bool` |  |
| `Equals(object obj) -> bool` |  |
| `FloatingPressedBackground.get -> XForma.Color` | A floating button's fill while it's pressed or disabled. |
| `FloatingPressedBackground.init -> void` | A floating button's fill while it's pressed or disabled. |
| `FloatingPressedForeground.get -> XForma.Color` | A floating button's icon while it's pressed or disabled. |
| `FloatingPressedForeground.init -> void` | A floating button's icon while it's pressed or disabled. |
| `GetHashCode() -> int` |  |
| `Glow.get -> XForma.Color` | What shows around popups where a shadow wouldn't (dark and high contrast), with its opacity. |
| `Glow.init -> void` | What shows around popups where a shadow wouldn't (dark and high contrast), with its opacity. |
| `OnCommand.get -> XForma.Color` | The icon and text behind a swiped row, on its command's color, unless the command sets `ForegroundColor`. |
| `OnCommand.init -> void` | The icon and text behind a swiped row, on its command's color, unless the command sets `ForegroundColor`. |
| `OnPressed.get -> XForma.Color` | The icon or letter of a round toggle while it's pressed, on the subtle foreground color. |
| `OnPressed.init -> void` | The icon or letter of a round toggle while it's pressed, on the subtle foreground color. |
| `OnSuccess.get -> XForma.Color` | The mark in an info bar's icon, on the success color. |
| `OnSuccess.init -> void` | The mark in an info bar's icon, on the success color. |
| `OnWarning.get -> XForma.Color` | The mark in an info bar's icon, on the warning color. |
| `OnWarning.init -> void` | The mark in an info bar's icon, on the warning color. |
| `Shadow.get -> XForma.Color` | The shadow beneath floating buttons, and beneath popups in light mode, with its opacity. |
| `Shadow.init -> void` | The shadow beneath floating buttons, and beneath popups in light mode, with its opacity. |
| `SubtleFloatingBackground.get -> XForma.Color?` | A subtle floating button's fill, or null (the default) for the page's background in light mode, and a dark gray otherwise. |
| `SubtleFloatingBackground.init -> void` | A subtle floating button's fill, or null (the default) for the page's background in light mode, and a dark gray otherwise. |
| `Success.get -> XForma.Color` | An info bar's success. |
| `Success.init -> void` | An info bar's success. |
| `ToString() -> string` |  |
| `Warning.get -> XForma.Color` | An info bar's warning. |
| `Warning.init -> void` | An info bar's warning. |
| `operator !=(XForma.Themes.Basic.BasicColors left, XForma.Themes.Basic.BasicColors right) -> bool` |  |
| `operator ==(XForma.Themes.Basic.BasicColors left, XForma.Themes.Basic.BasicColors right) -> bool` |  |

## BasicTheme

The Basic theme: XForma's controls in a plain, flat look. An app chooses it as it starts: XFormaApp.Run("My app", new App(), new BasicTheme());

| Member | |
|---|---|
| `Apply(XForma.Theme theme) -> XForma.Theme` | The theme (with its mode and colors, the system's high-contrast colors included) with the Basic type scale, shapes, motion, control styles and templates. Tokens the theme already has (an app's own) are kept. |
| `BasicTheme() -> void` |  |
| `CreateColors(XForma.ThemeMode mode, XForma.Color accentColor) -> XForma.ColorScheme` | Basic's colors for a mode, around an accent color, which light and dark use as it is. |

## ButtonStyle

How the Basic theme draws a `Button`. Unset values come from the theme's tokens. Set one on a theme for every button under a view.

| Member | |
|---|---|
| `<Clone>$() -> XForma.Themes.Basic.ButtonStyle` |  |
| `Background.get -> XForma.Color?` | The fill: the theme's accent color by default. |
| `Background.init -> void` | The fill: the theme's accent color by default. |
| `ButtonStyle() -> void` |  |
| `CornerRadius.get -> float?` | The corners: the theme's small corner radius by default. |
| `CornerRadius.init -> void` | The corners: the theme's small corner radius by default. |
| `Equals(XForma.Themes.Basic.ButtonStyle other) -> bool` |  |
| `Equals(object obj) -> bool` |  |
| `Foreground.get -> XForma.Color?` | The text: the color on the accent by default. |
| `Foreground.init -> void` | The text: the color on the accent by default. |
| `GetHashCode() -> int` |  |
| `Over(XForma.Themes.Basic.ButtonStyle other) -> XForma.Themes.Basic.ButtonStyle` | Combines two styles: this one's values, and the other's where this one has none. |
| `TextStyle.get -> XForma.Controls.TextStyle` | The text style: the theme's label style by default. |
| `TextStyle.init -> void` | The text style: the theme's label style by default. |
| `ToString() -> string` |  |
| `operator !=(XForma.Themes.Basic.ButtonStyle left, XForma.Themes.Basic.ButtonStyle right) -> bool` |  |
| `operator ==(XForma.Themes.Basic.ButtonStyle left, XForma.Themes.Basic.ButtonStyle right) -> bool` |  |
