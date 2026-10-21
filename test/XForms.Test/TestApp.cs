using System;
using XForms;

namespace XForms.Test
{
    public class TestApp : Application
    {


        /// <summary>
        /// The page to start on: "pickers", "panels", "binding", "input", "drawing", "xaml", "benchmark-views" or
        /// "benchmark-drawn" (otherwise the text and list page). Set by each
        /// platform's entry point, for example from an environment variable or
        /// an intent extra.
        /// </summary>
        public static string StartPage
        {
            get;
            set;
        }

        /// <summary>
        /// For testing theme changes without input: modes ("light", "dark",
        /// "highcontrast" or "system") to switch to in turn, two seconds
        /// apart, separated by commas.
        /// </summary>
        public static string ThemeModeSwitches
        {
            get;
            set;
        }

        /// <summary>
        /// Re-creates the native host (Android's activity), as a configuration
        /// change it doesn't declare would; null where there's none. Set by
        /// the platform's entry point.
        /// </summary>
        public static Action RecreateHost
        {
            get;
            set;
        }

        // For checking the platforms' lifecycle (Android's logcat shows these)
        protected override void OnSuspend()
        {
            base.OnSuspend();
            Console.WriteLine("XForms.Test: suspended");
        }

        protected override void OnResume()
        {
            base.OnResume();
            Console.WriteLine("XForms.Test: resumed");
        }

        protected override void OnStart()
        {
            base.OnStart();

            StartAsync();
        }

        private async void SwitchThemeModesAsync()
        {
            foreach (string name in ThemeModeSwitches.Split(','))
            {
                await System.Threading.Tasks.Task.Delay(2000);
                this.RequestedThemeMode = name.Trim().ToLowerInvariant() switch
                {
                    "light" => ThemeMode.Light,
                    "dark" => ThemeMode.Dark,
                    "highcontrast" => ThemeMode.HighContrast,
                    _ => null,
                };
            }
        }

        private async void StartAsync()
        {
            if (!string.IsNullOrEmpty(ThemeModeSwitches))
            {
                this.SwitchThemeModesAsync();
            }
            await ThemeResources.Default.LoadResourcesAsync();

            switch (StartPage?.ToLowerInvariant())
            {
                case "pickers":
                    _ = this.NavigateAsync(new PickersPage());
                    return;

                case "benchmark-views":
                    _ = this.NavigateAsync(new BenchmarkPage(drawAsOneView: false));
                    return;

                case "drawing":
                    _ = this.NavigateAsync(new DrawingPage());
                    return;

                case "input":
                    _ = this.NavigateAsync(new InputPage());
                    return;

                case "binding":
                    _ = this.NavigateAsync(new BindingPage());
                    return;

                case "panels":
                    _ = this.NavigateAsync(new PanelsPage());
                    return;

                case "text":
                    _ = this.NavigateAsync(new TextEntryPage());
                    return;

                case "lists":
                    _ = this.NavigateAsync(new ListsPage());
                    return;

                case "app":
                    _ = this.NavigateAsync(new AppLayerPage());
                    return;

                case "a11y":
                    _ = this.NavigateAsync(new AccessibilityPage());
                    return;

                case "theme":
                    _ = this.NavigateAsync(new ThemePage());
                    return;

                case "animation":
                    _ = this.NavigateAsync(new AnimationPage());
                    return;

                case "xaml":
                    _ = this.NavigateAsync(new XamlPage());
                    return;

                case "benchmark-drawn":
                    _ = this.NavigateAsync(new BenchmarkPage(drawAsOneView: true));
                    return;
            }

            _ = this.NavigateAsync(new TextAndListPage());
            //this.NavigateAsync(new DockLayoutPage());
            //this.NavigateAsync(new DistributedStackLayoutPage());
            //this.NavigateAsync(new TabbedPage());
            //this.NavigateAsync(new MarginPage());
            //this.NavigateAsync(new ControlPage());
        }
    }
}
