using System;
using XForms.Controls;
using XForms.Layouts;
using XForms.Primitives;

namespace XForms.Themes.Simple
{
    // The message, and Cancel and OK beneath it, side by side
    internal sealed class MessageBoxTemplate : ControlTemplate<MessageBoxView>
    {
        private sealed class Parts
        {
            public DockPanel Layout;
            public TextBlock Message;
        }

        protected override void Apply(
            MessageBoxView control,
            TemplateContext context)
        {
            var layout = new DockPanel()
            {
                MaxWidth = 300f,
            };

            var message = new TextBlock()
            {
                Text = control.Message,
                Foreground = control.ActualTheme.Colors.OnSurface,
                FontSize = 14f,
                TextWrapping = TextWrapping.Wrap,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Margin = new Thickness(20),
            };
            layout.Children.Add(message, Dock.Top);

            var buttons = new UniformGrid()
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Bottom,
            };
            layout.Children.Add(buttons, Dock.Top);

            buttons.Children.Add(CreateButton("Cancel", () => control.Choose(MessageBoxResult.Cancel)));
            buttons.Children.Add(CreateButton("Ok", () => control.Choose(MessageBoxResult.OK)));

            context.AddPart(layout);
            context.State = new Parts() { Layout = layout, Message = message };
        }

        protected override void Unapply(
            MessageBoxView control,
            TemplateContext context)
        {
            context.RemovePart(((Parts)context.State).Layout);
        }

        protected override void OnThemeChanged(
            MessageBoxView control,
            TemplateContext context)
        {
            ((Parts)context.State).Message.Foreground = control.ActualTheme.Colors.OnSurface;
        }

        private static Button CreateButton(
            string text,
            Action choose)
        {
            var button = new Button()
            {
                Text = text,
                IsAlertStyle = true,
                Width = 100,
                Height = 30,
                Margin = new Thickness(5),
                HorizontalAlignment = HorizontalAlignment.Center,
            };
            button.Clicked += (sender, e) => choose();
            return button;
        }
    }
}
