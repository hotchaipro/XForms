using XForma;
using XForma.Controls;
using XForma.Layouts;
using XForma.Primitives;

namespace XFormaApp
{
    /// <summary>
    /// A page: Build returns its views, in plain C#.
    /// </summary>
    public sealed class MainPage : Page
    {
        private int _count;

        protected override View Build()
        {
            var message = new TextBlock() { Text = "Hello from XForma" };

            var button = new Button() { Text = "Click me", Width = 140, Height = 40 };
            button.Clicked += (sender, e) =>
            {
                this._count += 1;
                message.Text = $"Clicked {this._count} times";
            };

            var layout = new StackPanel() { Spacing = 12, Margin = new Thickness(24) };
            layout.Children.Add(message);
            layout.Children.Add(button);
            return layout;
        }
    }
}
