using System;
using XForms.Controls;
using XForms.Layouts;
using XForms.Toolkit;

namespace XForms.Test
{
    /// <summary>
    /// The app layer: navigating forward and back with transitions, a modal
    /// page that returns a result, the platform services, the main window and
    /// the system's text size.
    /// </summary>
    public class AppLayerPage : Page
    {
        private readonly TextBlock _status;

        public AppLayerPage()
            : this(1)
        {
        }

        private AppLayerPage(
            int depth)
        {
            var layout = new StackPanel() { Spacing = 10, Padding = new Thickness(16) };

            layout.Children.Add(new TextBlock() { Text = $"Page {depth}", FontSize = 22, FontWeight = FontWeight.SemiBold });

            this._status = new TextBlock() { FontSize = 13, TextWrapping = TextWrapping.Wrap };
            layout.Children.Add(this._status);

            // Navigation: parameters go to the page's constructor
            var navigation = new WrapPanel() { ItemSpacing = 8, LineSpacing = 8 };
            navigation.Children.Add(this.CreateButton("Next (slide)", () => _ = this.Application.NavigateAsync(new AppLayerPage(depth + 1))));
            navigation.Children.Add(this.CreateButton("Next (fade)", () => _ = this.Application.NavigateAsync(new AppLayerPage(depth + 1), PageTransition.Fade)));
            navigation.Children.Add(this.CreateButton("Back", async () => this.Show($"Went back: {await this.Application.GoBackAsync()}")));
            navigation.Children.Add(this.CreateButton("Pick a color", this.PickColorAsync));
            layout.Children.Add(navigation);

            // Services
            var services = new WrapPanel() { ItemSpacing = 8, LineSpacing = 8 };
            services.Children.Add(this.CreateButton("Copy", async () =>
            {
                await this.Application.Services.Clipboard.SetTextAsync($"XForms page {depth}");
                this.Show($"Clipboard: \"{await this.Application.Services.Clipboard.GetTextAsync()}\"");
            }));
            services.Children.Add(this.CreateButton("Count runs", () =>
            {
                var preferences = this.Application.Services.Preferences;
                int count = preferences.Get("count", 0) + 1;
                preferences.Set("count", count);
                this.Show($"Preference count: {count}");
            }));
            services.Children.Add(this.CreateButton("Secret", async () =>
            {
                var storage = this.Application.Services.SecureStorage;
                await storage.SetAsync("secret", $"s3cret at {DateTime.Now:T}");
                this.Show($"Secure storage: \"{await storage.GetAsync("secret")}\"");
            }));
            services.Children.Add(this.CreateButton("Share", async () =>
            {
                await this.Application.Services.Share.RequestAsync(new ShareRequest() { Title = "XForms", Text = "Shared from XForms", Uri = new Uri("https://example.com") });
                this.Show("Shared");
            }));
            services.Children.Add(this.CreateButton("Buzz", () =>
            {
                var haptics = this.Application.Services.HapticFeedback;
                haptics.Perform(HapticFeedbackType.Click);
                this.Show($"Haptics supported: {haptics.IsSupported}");
            }));
            layout.Children.Add(services);

            // The window
            var window = new WrapPanel() { ItemSpacing = 8, LineSpacing = 8 };
            window.Children.Add(this.CreateButton("Title", () =>
            {
                this.Application.MainWindow.Title = $"XForms Test, page {depth}";
                this.ShowWindow();
            }));
            window.Children.Add(this.CreateButton("Resize", () =>
            {
                this.Application.MainWindow.Resize(new Size(900, 700));
                this.ShowWindow();
            }));
            layout.Children.Add(window);

            // Connectivity, files and the safe area
            var system = new WrapPanel() { ItemSpacing = 8, LineSpacing = 8 };
            system.Children.Add(this.CreateButton("Network", () =>
            {
                var connectivity = this.Application.Services.Connectivity;
                connectivity.ConnectivityChanged -= this.Connectivity_Changed;
                connectivity.ConnectivityChanged += this.Connectivity_Changed;
                this.Show($"Network: {connectivity.NetworkAccess}");
            }));
            system.Children.Add(this.CreateButton("Open file", async () =>
            {
                var files = await this.Application.Services.FilePicker.PickAsync(new FilePickerOptions() { Title = "Open a text file", FileTypes = new[] { ".txt" } });
                if (0 == files.Count)
                {
                    this.Show("Open: canceled");
                    return;
                }

                using (var stream = await files[0].OpenReadAsync())
                using (var reader = new System.IO.StreamReader(stream))
                {
                    string text = await reader.ReadToEndAsync();
                    this.Show($"Opened {files[0].Name}: \"{text.Trim()}\"");
                }
            }));
            system.Children.Add(this.CreateButton("Save file", async () =>
            {
                bool saved = await this.Application.Services.FilePicker.SaveAsync("hello.txt", async (stream) =>
                {
                    using (var writer = new System.IO.StreamWriter(stream))
                    {
                        await writer.WriteAsync($"Saved from XForms page {depth}");
                    }
                }, new FilePickerOptions() { FileTypes = new[] { ".txt" } });
                this.Show($"Saved: {saved}");
            }));
            system.Children.Add(this.CreateButton("Edge to edge", () =>
            {
                this.UsesSafeArea = !this.UsesSafeArea;
                this.Show($"Uses safe area: {this.UsesSafeArea}; insets {this.Application.MainWindow.SafeAreaInsets}");
            }));
            layout.Children.Add(system);

            this.Application.MainWindow.SizeChanged += (sender, e) => this.ShowWindow();
            this.Application.TextScaleChanged += (sender, e) => this.ShowWindow();

            var page = new DockPanel();
            page.Children.Add(layout, Dock.CenterOverlay);
            this.Content = page;
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            this.ShowWindow();
        }

        private async void PickColorAsync()
        {
            string color = await this.Application.ShowModalAsync(new ColorPage());
            this.Show($"Modal result: {color ?? "(closed)"}");
        }

        private void ShowWindow()
        {
            var window = this.Application.MainWindow;
            this.Show($"Window {window.Size.Width:0}×{window.Size.Height:0} (resizable: {window.CanResize}); text scale {this.Application.TextScale:0.##}");
        }

        private void Connectivity_Changed(
            object sender,
            EventArgs e)
        {
            this.Show($"Network changed: {this.Application.Services.Connectivity.NetworkAccess}");
        }

        private void Show(
            string message)
        {
            this._status.Text = message;
        }

        private Button CreateButton(
            string text,
            Action action)
        {
            var button = new Button() { Text = text, Height = 34, Width = 24 + (text.Length * 9) };
            button.Clicked += (sender, e) => action();
            return button;
        }

        // A modal page that closes with the color chosen
        private sealed class ColorPage : ModalPage<string>
        {
            public ColorPage()
            {
                var layout = new StackPanel() { Spacing = 10, Padding = new Thickness(24) };
                layout.Children.Add(new TextBlock() { Text = "Pick a color", FontSize = 22 });
                foreach (var name in new[] { "Red", "Green", "Blue" })
                {
                    var button = new Button() { Text = name, Width = 140, Height = 40, HorizontalAlignment = HorizontalAlignment.Left };
                    button.Clicked += (sender, e) => _ = this.CloseAsync(name);
                    layout.Children.Add(button);
                }

                var page = new DockPanel();
                page.Children.Add(layout, Dock.CenterOverlay);
                this.Content = page;
            }
        }
    }
}
