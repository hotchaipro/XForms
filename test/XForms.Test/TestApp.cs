using System;
using XForms;

namespace XForms.Test
{
    public class TestApp : Application
    {


        /// <summary>
        /// The page to start on, as a route without its leading slash
        /// ("pickers", "lists", "app/2"; see <see cref="OnConfigureRoutes"/>),
        /// or null for the text and list page. Set by each platform's entry
        /// point, for example from an environment variable or an intent extra.
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

            // The first page, unless the platform gives one (the browser's
            // address, say)
            this.StartPath = string.IsNullOrEmpty(StartPage) ? "/" : "/" + StartPage.ToLowerInvariant();

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
        }

        protected override void OnConfigureRoutes(
            Routes routes)
        {
            base.OnConfigureRoutes(routes);

            routes.Add("/", _ => new TextAndListPage());
            routes.Add("/pickers", _ => new PickersPage());
            routes.Add("/drawing", _ => new DrawingPage());
            routes.Add("/input", _ => new InputPage());
            routes.Add("/binding", _ => new BindingPage());
            routes.Add("/panels", _ => new PanelsPage());
            routes.Add("/text", _ => new TextEntryPage());
            routes.Add("/lists", _ => new ListsPage());
            routes.Add("/app", _ => new AppLayerPage());
            routes.Add("/app/{depth}", route => new AppLayerPage(route.GetInt32("depth")));
            routes.Add("/pick-color", _ => new AppLayerPage.ColorPage());
            routes.Add("/a11y", _ => new AccessibilityPage());
            routes.Add("/theme", _ => new ThemePage());
            routes.Add("/animation", _ => new AnimationPage());
            routes.Add("/xaml", _ => new XamlPage());
            routes.Add("/benchmark-views", _ => new BenchmarkPage(drawAsOneView: false));
            routes.Add("/benchmark-drawn", _ => new BenchmarkPage(drawAsOneView: true));
        }
    }
}
