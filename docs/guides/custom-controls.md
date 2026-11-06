# Custom controls

There are three ways to make a control. Start with the simplest that does the job.

## Compose views

Derive from `UserControl`, and put other views inside it:

```csharp
public sealed class LabeledField : UserControl
{
    public LabeledField(string label)
    {
        var layout = new StackPanel() { Spacing = 4 };
        layout.Children.Add(new TextBlock() { Text = label });
        layout.Children.Add(new TextBox());
        this.Content = layout;
    }
}
```

## Draw it yourself

Override `Render`. Views are lightweight: one that draws doesn't need a native view of its own, so
thousands of them stay fast.

```csharp
public sealed class Dot : View
{
    private Color _color = Colors.Red;

    public Color Color
    {
        get
        {
            return this._color;
        }

        set
        {
            this._color = value;
            this.InvalidateVisual();
        }
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        return new Size(24, 24);
    }

    protected override void Render(DrawingContext drawingContext)
    {
        var bounds = this.Bounds;
        drawingContext.FillEllipse(new Point(bounds.Width / 2, bounds.Height / 2), bounds.Width / 2, bounds.Height / 2, this._color);
    }
}
```

Call `InvalidateVisual` when what it draws changes, and `InvalidateMeasure` when its size changes.

## A themeable control

Derive from `TemplatedControl` when others should be able to restyle it: the control holds the
state and behavior, and a `ControlTemplate<T>` in the theme draws it. A control library's templates
go in a theme package, so a library can be used with any theme that gives them.

`TemplatedControl` has `IsPressed` and `IsEnabled`; call `OnStateChanged` when state of your own
changes, so that the template draws again.

## Your own layout

```csharp
public sealed class Rows : View
{
    protected override Size MeasureOverride(Size availableSize)
    {
        return new Size(availableSize.Width, 0);
    }

    protected override void ArrangeOverride(Size finalSize)
    {
    }
}
```

Call each child's `Measure` in `MeasureOverride`, and its `Arrange` in `ArrangeOverride`.
`Panel` is the place to start if you have children to manage.

## Rules for a control library

A control library uses only XForma's public API, so it runs on every platform. Name a view
(`View.Name`) when it's part of what a test or hot reload should find again.

Implement accessibility from the start: see [Accessibility](accessibility.md).
