# Accessibility

XForma's controls describe themselves to the platform's assistive technology: UI Automation on
Windows, TalkBack on Android, VoiceOver on iOS and ARIA on the web. Your own controls should too.

What VoiceOver, TalkBack and web screen readers actually say hasn't been checked by a person yet:
see [what's supported](../support.md).

## Describing a view

```csharp
var image = new Image() { AccessibilityLabel = "The company's logo" };
var decoration = new Border() { AccessibilityIsHidden = true };
```

In a custom control, override the defaults, so that an app can still set its own:

```csharp
protected override AccessibilityRole DefaultAccessibilityRole
{
    get
    {
        return AccessibilityRole.Button;
    }
}

protected override string? DefaultAccessibilityLabel
{
    get
    {
        return "Play";
    }
}

public override bool PerformAccessibilityAction(AccessibilityAction action)
{
    if (action == AccessibilityAction.Invoke)
    {
        return true;
    }

    return base.PerformAccessibilityAction(action);
}
```

Call `NotifyAccessibilityStateChanged` when a checked, selected or expanded state changes.

## The keyboard

Everything a pointer can do should work from the keyboard. Set `Focusable` on a control, handle
`OnActivate`, and use arrow keys inside groups of items, as `NavigationView`'s pane does: it's one
Tab stop. Override `IsTabStop` to do the same in your own control.

## Text size

`Application.TextScale` is the system's text size, and `TextScaleChanged` says when it changes.
XForma's text follows it, so don't fix a text view's height.

## High contrast

An app follows the system's high contrast mode through `ThemeMode.HighContrast`. Use the theme's
color roles, rather than fixed colors, and it just works.
