using System;
using XForms;

namespace XForms.Test
{
    public class TestApp : Application
    {
        public TestApp(
            IPlatform platform)
            : base(platform)
        {
        }

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
                    this.NavigateTo(new PickersPage());
                    return;

                case "benchmark-views":
                    this.NavigateTo(new BenchmarkPage(drawAsOneView: false));
                    return;

                case "drawing":
                    this.NavigateTo(new DrawingPage());
                    return;

                case "input":
                    this.NavigateTo(new InputPage());
                    return;

                case "binding":
                    this.NavigateTo(new BindingPage());
                    return;

                case "panels":
                    this.NavigateTo(new PanelsPage());
                    return;

                case "theme":
                    this.NavigateTo(new ThemePage());
                    return;

                case "animation":
                    this.NavigateTo(new AnimationPage());
                    return;

                case "xaml":
                    this.NavigateTo(new XamlPage());
                    return;

                case "benchmark-drawn":
                    this.NavigateTo(new BenchmarkPage(drawAsOneView: true));
                    return;
            }

            this.NavigateTo(new TextAndListPage());
            //this.NavigateTo(new DockLayoutPage());
            //this.NavigateTo(new DistributedStackLayoutPage());
            //this.NavigateTo(new TabbedPage());
            //this.NavigateTo(new MarginPage());
            //this.NavigateTo(new ControlPage());
        }
    }
}
