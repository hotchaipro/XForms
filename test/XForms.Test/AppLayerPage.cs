using System;
using XForms.Controls;
using XForms.Layouts;
using XForms.Primitives;
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

        public AppLayerPage(
            int depth)
        {
            var layout = new StackPanel() { Spacing = 10, Padding = new Thickness(16) };

            layout.Children.Add(new TextBlock() { Text = $"Page {depth}", FontSize = 22, FontWeight = FontWeight.SemiBold });

            this._status = new TextBlock() { FontSize = 13, TextWrapping = TextWrapping.Wrap };
            layout.Children.Add(this._status);

            // Navigation: by path, which the page's route turns into its
            // constructor's parameters
            var navigation = new WrapPanel() { ItemSpacing = 8, LineSpacing = 8 };
            navigation.Children.Add(this.CreateButton("Next (slide)", () => _ = this.Application.NavigateAsync($"/app/{depth + 1}")));
            navigation.Children.Add(this.CreateButton("Next (fade)", () => _ = this.Application.NavigateAsync($"/app/{depth + 1}", PageTransition.Fade)));
            navigation.Children.Add(this.CreateButton("Back", async () => this.Show($"Went back: {await this.Application.GoBackAsync()}")));
            navigation.Children.Add(this.CreateButton("Stack", () => this.Show($"Stack: {string.Join(" ", this.Application.NavigationStack)}")));
            navigation.Children.Add(this.CreateButton("Pick a color", async () =>
            {
                string color = await this.Application.ShowModalAsync<string>("/pick-color");
                this.Show($"Modal result: {color ?? "(closed)"}");
            }));
            navigation.Children.Add(this.CreateButton("Pick (page)", async () =>
            {
                string color = await this.Application.NavigateAsync<string>("/pick-color");
                this.Show($"Page result: {color ?? "(went back)"}");
            }));
            navigation.Children.Add(this.CreateButton("Message box", async () => this.Show($"Message box: {await MessageBox.ShowAsync("Delete everything?", "Confirm")}")));
            if (null != TestApp.RecreateHost)
            {
                navigation.Children.Add(this.CreateButton("Recreate host", () => TestApp.RecreateHost()));
            }

            layout.Children.Add(navigation);

            // Services
            var services = new WrapPanel() { ItemSpacing = 8, LineSpacing = 8 };
            services.Children.Add(this.CreateButton("Copy", async () =>
            {
                await Clipboard.Default.SetTextAsync($"XForms page {depth}");
                this.Show($"Clipboard: \"{await Clipboard.Default.GetTextAsync()}\"");
            }));
            services.Children.Add(this.CreateButton("Count runs", () =>
            {
                var preferences = Preferences.Default;
                int count = preferences.Get("count", 0) + 1;
                preferences.Set("count", count);
                this.Show($"Preference count: {count}");
            }));
            services.Children.Add(this.CreateButton("Secret", async () =>
            {
                var storage = SecureStorage.Default;
                await storage.SetAsync("secret", $"s3cret at {DateTime.Now:T}");
                this.Show($"Secure storage: \"{await storage.GetAsync("secret")}\"");
            }));
            services.Children.Add(this.CreateButton("Share", async () =>
            {
                await Share.Default.RequestAsync(new ShareRequest() { Title = "XForms", Text = "Shared from XForms", Uri = new Uri("https://example.com") });
                this.Show("Shared");
            }));
            services.Children.Add(this.CreateButton("Buzz", () =>
            {
                var haptics = HapticFeedback.Default;
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
            window.Children.Add(this.CreateButton("New window", async () =>
            {
                if (!this.Application.CanOpenWindows)
                {
                    this.Show("This platform shows one window");
                    return;
                }

                var opened = await this.Application.OpenWindowAsync($"/app/{depth + 1}",$"XForms Test, window {this.Application.Windows.Count + 1}", new Size(520, 420));
                opened.Closed += (sender, e) => this.Show($"A window closed; {this.Application.Windows.Count} open");
                this.Show($"{this.Application.Windows.Count} windows open");
            }));
            layout.Children.Add(window);

            // Connectivity, files and the safe area
            var system = new WrapPanel() { ItemSpacing = 8, LineSpacing = 8 };
            system.Children.Add(this.CreateButton("Network", () =>
            {
                var connectivity = Connectivity.Default;
                connectivity.ConnectivityChanged -= this.Connectivity_Changed;
                connectivity.ConnectivityChanged += this.Connectivity_Changed;
                this.Show($"Network: {connectivity.NetworkAccess}");
            }));
            system.Children.Add(this.CreateButton("Open file", async () =>
            {
                var files = await FilePicker.Default.PickAsync(new FilePickerOptions() { Title = "Open a text file", FileTypes = new[] { ".txt" } });
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
                bool saved = await FilePicker.Default.SaveAsync("hello.txt", async (stream) =>
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

        private void ShowWindow()
        {
            var window = this.Application.MainWindow;
            this.Show($"Window {window.Size.Width:0}×{window.Size.Height:0} (resizable: {window.CanResize}); text scale {this.Application.TextScale:0.##}");
        }

        private void Connectivity_Changed(
            object sender,
            EventArgs e)
        {
            this.Show($"Network changed: {Connectivity.Default.NetworkAccess}");
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

        // A page that gives back the color chosen (or nothing, by Back), shown
        // as a modal page or navigated to
        internal sealed class ColorPage : Page
        {
            public ColorPage()
            {
                var layout = new StackPanel() { Spacing = 10, Padding = new Thickness(24) };
                layout.Children.Add(new TextBlock() { Text = "Pick a color", FontSize = 22 });
                foreach (var name in new[] { "Red", "Green", "Blue" })
                {
                    var button = new Button() { Text = name, Width = 140, Height = 40, HorizontalAlignment = HorizontalAlignment.Left };
                    button.Clicked += (sender, e) =>
                    {
                        // Back closes it as a modal page, or returns from it
                        this.SetResult(name);
                        _ = this.Application.GoBackAsync();
                    };
                    layout.Children.Add(button);
                }

                var page = new DockPanel();
                page.Children.Add(layout, Dock.CenterOverlay);
                this.Content = page;
            }
        }
    }
}
