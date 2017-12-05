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

            this.NavigateTo(new DockLayoutPage());
            //this.NavigateTo(new DistributedStackLayoutPage());
            //this.NavigateTo(new ControlPage());
            //this.NavigateTo(new TabbedPage());
            //this.NavigateTo(new MarginPage());
        }
    }
}
