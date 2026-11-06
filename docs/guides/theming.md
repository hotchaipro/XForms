# Theming

Controls have no look of their own. A **theme package** gives each control a template that draws
it, and the sizes, colors and motion it uses. An app passes one to `XFormaApp.Run`.

## Light, dark and high contrast

An app follows the system unless it says otherwise, and restyles when the system changes:

```csharp
this.Application.RequestedThemeMode = ThemeMode.Dark;
this.Application.RequestedThemeMode = null;
```

## Your colors

`AccentColor` changes the app's accent:

```csharp
this.Application.AccentColor = Colors.Teal;
```

For more, override `CreateTheme`, which runs for each mode:

```csharp
public sealed class App : Application
{
    protected override Theme CreateTheme(ThemeMode mode, Color accentColor)
    {
        return base.CreateTheme(mode, (mode == ThemeMode.Dark) ? Colors.Orange : Colors.Teal);
    }
}
```

## Reading the theme

Inside a view, `ActualTheme` is the theme it's under. Use the roles rather than fixed colors, so
your views work in every mode:

```csharp
var colors = this.ActualTheme.Colors;
var background = colors.Surface;
var text = colors.OnSurface;
```

Override `OnThemeChanged` to update what you've set from the theme.

## Changing a control's look

Every control's size is in a style. To change part of one, copy it, and apply the result to a view
and everything inside it:

```csharp
var theme = this.ActualTheme;
var panel = new StackPanel();
panel.Theme = theme.WithExtension(theme.CheckBoxStyle with { BoxSize = 24 });
```

To change how a control draws, write a template, and add it with `WithTemplate`:

```csharp
public sealed class SquareButtonTemplate : XForma.Controls.ControlTemplate<Button>
{
    protected override void Render(Button control, DrawingContext drawingContext)
    {
        var colors = control.ActualTheme.Colors;
        drawingContext.FillRectangle(control.Bounds, control.IsPressed ? colors.Surface : colors.Accent);
    }
}
```

```csharp
var view = new StackPanel();
view.Theme = this.ActualTheme.WithTemplate(new SquareButtonTemplate());
```

## Writing a theme package

Derive from `AppTheme`, whose `CreateColors` gives the palette for each mode and whose `Apply` adds
styles and templates. `themes/basic` is the example to copy: every style's properties are
`required`, so the compiler tells you what's missing, and a control whose theme has no template or
style for it throws.

Build rule **XF0003** keeps look out of XForma's own controls, so anything visual can be changed
from a theme.
