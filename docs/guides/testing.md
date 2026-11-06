# Testing your app

`XForma.Testing` runs views in an ordinary unit test, with no device or window. It lays out as a
real platform does, but draws nothing and has no native text measurement.

```csharp
public sealed class SampleApp : Application
{
}

public static class LayoutCheck
{
    public static void Run()
    {
        XForma.Testing.TestHost.Start(new SampleApp());

        var save = new Button() { Name = "save", Width = 100, Height = 40 };
        var layout = new StackPanel();
        layout.Children.Add(save);
        layout.Theme = new XForma.Themes.Basic.BasicTheme().Create(ThemeMode.Light, Colors.Blue);

        XForma.Testing.TestHost.LayOut(layout, 400, 800);

        var found = XForma.Testing.TestHost.Find<Button>(layout, "save");
        var frame = XForma.Testing.TestHost.FrameOf(found);
    }
}
```

- `Name` a view to find it, however the page is laid out.
- An app is process-wide, so run these tests one at a time. With xUnit:
  `[assembly: CollectionBehavior(DisableTestParallelization = true)]`.
- Keep logic in view models, which need no host at all.

For what only a real screen shows, compare pictures as XForma does in
`test/visual/Check-Screens.ps1`.
