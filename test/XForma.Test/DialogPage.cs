using System;
using XForma.Controls;
using XForma.Primitives;
using XForma.Layouts;

namespace XForma.Test
{
    /// <summary>
    /// A dialog-like page: a text box, a toggle, and a menu bar of commands
    /// (two subtle, one in the accent).
    /// </summary>
    public class DialogPage : Page
    {
        protected override View Build()
        {
            var layout = new DockPanel()
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
            };

            var form = new StackPanel()
            {
                Spacing = 16,
                Margin = new Thickness(20),
            };
            form.Children.Add(new TextBox() { PlaceholderText = "Name" });
            form.Children.Add(new TextBox() { PlaceholderText = "Notes" });
            form.Children.Add(new ToggleButton()
            {
                Icon = ThemeResources.Default.AboutLogo,
                Text = "repeat",
                HorizontalAlignment = HorizontalAlignment.Center,
            });
            layout.Children.Add(form, Dock.Top);

            var menuBar = new MenuBar()
            {
                ExtendsIntoSafeArea = SafeAreaEdges.Bottom,
            };
            menuBar.MenuItems.Add(new MenuItem() { Text = "cancel", Icon = ThemeResources.Default.AboutLogo, IsSubtleStyle = true });
            menuBar.MenuItems.Add(new MenuItem() { Text = "delete", Icon = ThemeResources.Default.AboutLogo, IsSubtleStyle = true });
            menuBar.MenuItems.Add(new MenuItem() { Text = "save", Icon = ThemeResources.Default.AboutLogo });
            layout.Children.Add(menuBar, Dock.Bottom);

            return layout;
        }
    }
}
