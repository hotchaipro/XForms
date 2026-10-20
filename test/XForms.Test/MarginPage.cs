using System;
using XForms.Layouts;

namespace XForms.Test
{
    public class MarginPage : Page
    {
        public MarginPage()
        {
            var pageLayout = new DockPanel()
            {
                Margin = new Thickness(50),
            };

            var dockLayout1 = new DockPanel()
            {
                Margin = new Thickness(50),
            };
            pageLayout.Children.Add(dockLayout1);

            var dockLayout2 = new DockPanel()
            {
                Margin = new Thickness(-25),
            };
            dockLayout1.Children.Add(dockLayout2);

            this.Content = pageLayout;
        }
    }
}
