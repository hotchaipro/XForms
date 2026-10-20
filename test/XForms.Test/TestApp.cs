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
        /// The page to start on: "pickers", "xaml", "benchmark-views" or
        /// "benchmark-drawn" (otherwise the text and list page). Set by each
        /// platform's entry point, for example from an environment variable or
        /// an intent extra.
        /// </summary>
        public static string StartPage
        {
            get;
            set;
        }

        protected override void OnStart()
        {
            base.OnStart();

            StartAsync();
        }

        private async void StartAsync()
        {
            await ThemeResources.Default.LoadResourcesAsync();

            switch (StartPage?.ToLowerInvariant())
            {
                case "pickers":
                    this.NavigateTo(new PickersPage());
                    return;

                case "benchmark-views":
                    this.NavigateTo(new BenchmarkPage(drawAsOneView: false));
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
