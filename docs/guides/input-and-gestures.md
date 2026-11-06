# Input and gestures

Pointer input reaches a view as events and through gesture recognizers. Mouse, touch and pen all
arrive the same way, and `PointerEventArgs.DeviceType` says which it was.

## Gestures

Add a recognizer to a view:

```csharp
var view = new Border() { Width = 100, Height = 100 };
var tap = new TapGestureRecognizer();
tap.Tapped += (sender, e) => { };
tap.DoubleTapped += (sender, e) => { };
tap.Holding += (sender, e) => { };
view.GestureRecognizers.Add(tap);
```

| Recognizer | For |
|---|---|
| `TapGestureRecognizer` | Tap, double tap, press and hold |
| `SwipeGestureRecognizer` | A horizontal drag that commits or falls back |
| `PanGestureRecognizer` | Following a drag in any direction, with its velocity |
| `PinchGestureRecognizer` | Two fingers: scale |

Recognizers on one view are exclusive: the first to recognize cancels the others. A drag on a view
inside a scrolling list goes to the list unless a recognizer claims it. A pan's `ShouldBegin` can
refuse a drag, such as one in the wrong direction, so that the list scrolls instead.

A pan's translation is in its view's coordinates, so put the recognizer on a view that stays still
rather than on the one it moves.

## Pointer events

For drawing or something a recognizer doesn't cover, override the view's methods:

```csharp
protected override void OnPointerPressed(PointerEventArgs e)
{
    base.OnPointerPressed(e);
    this.CapturePointer(e);
    e.Handled = true;
}

protected override void OnPointerMoved(PointerEventArgs e)
{
    base.OnPointerMoved(e);
    var where = e.Position;
}
```

`CapturePointer` keeps the moves for this view, so a scrolling parent can't take them.

## The keyboard

Tab moves focus in `TabIndex` order, then drawing order. A control that takes focus sets
`Focusable`. Override `OnKeyDown` to handle keys, and `OnActivate` for Enter and Space:

```csharp
protected override void OnKeyDown(KeyEventArgs keyInputEvent)
{
    if (keyInputEvent.Key == Key.Escape)
    {
        keyInputEvent.Handled = true;
    }
}

protected override void OnActivate()
{
}
```

`Application.PreviewKeyDown` sees keys first, for shortcuts across the app.

## Drag and drop

Set `CanDrag` on a view, and say what's dragged:

```csharp
var source = new Border() { Width = 100, Height = 40, CanDrag = true, Focusable = true };
source.DragStarting += (sender, e) =>
{
    e.Data.Text = "Shows in other apps";
    e.Data.LocalData = new object();
};
source.DropCompleted += (sender, e) =>
{
    if (e.Operation == DragOperation.Move)
    {
        // Remove the original
    }
};
```

Set `AllowDrop` on a target, and accept what it can use:

```csharp
var target = new Border() { Width = 200, Height = 100, AllowDrop = true };
target.DragEnter += (sender, e) => e.AcceptedOperation = DragOperation.Copy;
target.Drop += async (sender, e) =>
{
    foreach (var file in e.Data.Files)
    {
        using var stream = await file.OpenReadAsync();
    }
};
```

| | Windows | Android | iOS | Web |
|---|---|---|---|---|
| Starts with | Mouse past the slop | A held finger | A long press | The browser's drag |
| Text and URIs to other apps | Yes | In split screen | Yes | Yes |
| Files from other apps | Yes | Yes | Yes | Yes |

From the keyboard, Ctrl+Space picks up the focused view, Tab or the arrow keys go through the
targets that accept it, Enter drops it, and Escape puts it back.
