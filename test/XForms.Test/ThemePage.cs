using System;
using XForms.Controls;
using XForms.Themes.Simple;
using XForms.Layouts;
using XForms.Primitives;

namespace XForms.Test
{
    /// <summary>
    /// Themes: the mode (following the system, or chosen), the accent color,
    /// a section with its own theme, the type scale, and button styles from
    /// a button and from a theme.
    /// </summary>
    public class ThemePage : Page
    {
        private readonly TextBlock _status;
        private readonly Button _ownStyleButton;

        // One button's own look: the page's theme, with its own button style
        private static readonly ButtonStyle OwnStyle = new ButtonStyle() { Background = Color.FromRgb(0xff9800), Foreground = Colors.Black, CornerRadius = 18 };

        public ThemePage()
        {
            var layout = new StackPanel()
            {
                Spacing = 12,
                Padding = new Thickness(20),
            };

            this._status = new TextBlock();
            layout.Children.Add(this._status);

            // The mode: the system's, or one the app chooses
            var modes = new WrapPanel() { ItemSpacing = 8, LineSpacing = 8 };
            modes.Children.Add(this.CreateModeButton("System", null));
            modes.Children.Add(this.CreateModeButton("Light", ThemeMode.Light));
            modes.Children.Add(this.CreateModeButton("Dark", ThemeMode.Dark));
            layout.Children.Add(modes);

            // The accent color: the system's, or the app's
            var accents = new WrapPanel() { ItemSpacing = 8, LineSpacing = 8 };
            accents.Children.Add(this.CreateAccentButton("System accent", null));
            accents.Children.Add(this.CreateAccentButton("Green", Color.FromRgb(0x2e7d32)));
            accents.Children.Add(this.CreateAccentButton("Purple", Color.FromRgb(0x7b1fa2)));
            layout.Children.Add(accents);

            // The type scale
            layout.Children.Add(new TypeSample(t => t.Display, "Display"));
            layout.Children.Add(new TypeSample(t => t.Title, "Title"));
            layout.Children.Add(new TypeSample(t => t.Body, "Body text, with a text box below"));
            layout.Children.Add(new TextBox() { PlaceholderText = "A text box on the surface color", HorizontalAlignment = HorizontalAlignment.Stretch });

            // Buttons: the theme's, an alert, one with its own style, and a
            // disabled one
            var buttons = new WrapPanel() { ItemSpacing = 8, LineSpacing = 8 };
            buttons.Children.Add(new Button() { Text = "Accent", Width = 110, Height = 36 });
            buttons.Children.Add(new Button() { Text = "Alert", IsAlertStyle = true, Width = 110, Height = 36 });
            this._ownStyleButton = new Button() { Text = "Own style", Width = 110, Height = 36 };
            buttons.Children.Add(this._ownStyleButton);
            buttons.Children.Add(new Button() { Text = "Disabled", IsEnabled = false, Width = 110, Height = 36 });
            layout.Children.Add(buttons);

            // A section with its own theme: dark, with pill-shaped buttons,
            // whatever the app's mode
            var dark = new SimpleTheme().Apply(Theme.Create(ThemeMode.Dark, Color.FromRgb(0xffc107)))
                .WithExtension(new ButtonStyle() { CornerRadius = 18 });
            var sectionContent = new StackPanel() { Spacing = 8 };
            sectionContent.Children.Add(new TypeSample(t => t.Body, "A section with its own dark theme"));
            sectionContent.Children.Add(new Button() { Text = "Pill button", Width = 140, Height = 36, HorizontalAlignment = HorizontalAlignment.Left });
            layout.Children.Add(new Border()
            {
                Theme = dark,
                Background = dark.Colors.Background,
                CornerRadius = dark.Shapes.MediumCornerRadius,
                Padding = new Thickness(12),
                Child = sectionContent,
            });

            var page = new DockPanel();
            page.Children.Add(layout, Dock.CenterOverlay);
            this.Content = page;
        }

        protected override void OnThemeChanged(
            Theme theme)
        {
            base.OnThemeChanged(theme);

            this._ownStyleButton.Theme = theme.WithExtension(OwnStyle);

            var application = this.Application;
            this._status.Text = $"Mode: {application.ActualThemeMode} (system: {application.SystemThemeMode}); accent: {application.ActualAccentColor}";
        }

        private Button CreateModeButton(
            string text,
            ThemeMode? mode)
        {
            var button = new Button() { Text = text, Width = 100, Height = 32 };
            button.Clicked += (sender, e) => this.Application.RequestedThemeMode = mode;
            return button;
        }

        private Button CreateAccentButton(
            string text,
            Color? color)
        {
            var button = new Button() { Text = text, Width = 130, Height = 32 };
            button.Clicked += (sender, e) => this.Application.AccentColor = color;
            return button;
        }

        // A line of text in one of the theme's text styles
        private sealed class TypeSample : TextBlock
        {
            private readonly Func<Typography, TextStyle> _style;

            public TypeSample(
                Func<Typography, TextStyle> style,
                string text)
            {
                this._style = style;
                this.Text = text;
            }

            protected override void OnThemeChanged(
                Theme theme)
            {
                base.OnThemeChanged(theme);

                var style = this._style(theme.Typography);
                this.FontFamily = style.FontFamily;
                this.FontSize = style.FontSize;
                this.FontWeight = style.FontWeight;
            }
        }


    }
}
