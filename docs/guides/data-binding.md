# Data binding

A binding keeps a view's property up to date with your data. Bindings are typed and use compiled
lambdas, not reflection, so the compiler checks them and they work when trimmed.

```csharp
public sealed class Person : INotifyPropertyChanged
{
    private string _name = string.Empty;

    public event PropertyChangedEventHandler? PropertyChanged;

    public string Name
    {
        get
        {
            return this._name;
        }

        set
        {
            this._name = value;
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Name)));
        }
    }
}
```

## One way

The view follows the source. The lambda can be any code, so conversions are ordinary C#:

```csharp
var person = new Person();
var greeting = new TextBlock();
greeting.Bind(TextBlock.TextProperty, person, (p) => $"Hello, {p.Name}");
```

## Two way

Add a function that writes back, and the source follows the view too:

```csharp
var person = new Person();
var name = new TextBox();
name.Bind(TextBox.TextProperty, person, (p) => p.Name, (p, value) => p.Name = value);
```

## Good to know

- The source must raise `PropertyChanged` on the UI thread. From elsewhere, use
  `Application.Dispatcher`.
- A change to an object the lambda reaches through the source (`p.Address.City`) is seen only when
  the source itself raises a change.
- Without `INotifyPropertyChanged`, call `Refresh` on the `Binding` that `Bind` returns.
- A source doesn't keep the views bound to it alive, so you needn't dispose of bindings when a page
  goes. `Dispose` stops one early.
- There's no inherited data context: hand a page or control its view model yourself.
