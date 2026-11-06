# Platform services

`XForma.Toolkit` gives every platform the same services, and `XFormaApp.Run` registers them. Each
has a `Default`, and an interface you can replace in tests.

```csharp
XForma.Toolkit.Preferences.Default.Set("name", "Ada");
var name = XForma.Toolkit.Preferences.Default.Get("name", "nobody");

await XForma.Toolkit.SecureStorage.Default.SetAsync("token", "secret");
var token = await XForma.Toolkit.SecureStorage.Default.GetAsync("token");

await XForma.Toolkit.Clipboard.Default.SetTextAsync("copied");
```

| Service | For |
|---|---|
| `Preferences` | Small settings |
| `SecureStorage` | Secrets, in the platform's keychain or equivalent |
| `Clipboard` | Copy and paste |
| `Launcher` | Opening a URI in another app |
| `Share` | The platform's share sheet |
| `FilePicker` | Choosing files |
| `HapticFeedback` | A tap the user feels |
| `Connectivity` | Whether there's a network |

The toolkit has no UI in it, so any .NET app can use it.

## Fonts and images

`Application.RegisterFont` registers a font file, in `OnLoadResources`. Images are `Bitmap`s, or
`FileImageSource`s for an `Image`. Each platform keeps its files its own way: see the test app's
projects, where `test/Content` is linked into each.

## The UI thread

Views are touched on the UI thread. `await` comes back to it by itself; from another thread, post:

```csharp
this.Application.Dispatcher.Post(() =>
{
});
```

## Animation

Views animate on the compositor, off the UI thread, through an `Animator`:

```csharp
var box = new Border() { Width = 50, Height = 50, Animator = new Animator() };
var motion = this.ActualTheme.Motion;
box.Animator.TranslateTo(new Point(100, 0), motion.Medium, motion.Decelerate);
```
