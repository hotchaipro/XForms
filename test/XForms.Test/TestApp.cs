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

        protected override void OnStart()
        {
            base.OnStart();

            StartAsync();
        }

        private async void StartAsync()
        {
            await ThemeResources.Default.LoadResourcesAsync();

            // XFORMS_TEST_PAGE picks another page, such as "pickers"
            if (string.Equals(Environment.GetEnvironmentVariable("XFORMS_TEST_PAGE"), "pickers", StringComparison.OrdinalIgnoreCase))
            {
                this.NavigateTo(new PickersPage());
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
