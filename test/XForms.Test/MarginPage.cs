using System;
using XForms.Layouts;

namespace XForms.Test
{
    public class MarginPage : Page
    {
        public MarginPage()
        {
            var pageLayout = new DockLayout()
            {
                Margin = new Thickness(50),
            };

            var dockLayout1 = new DockLayout()
            {
                Margin = new Thickness(50),
            };
            pageLayout.Children.Add(dockLayout1);

            var dockLayout2 = new DockLayout()
            {
                Margin = new Thickness(-25),
            };
            dockLayout1.Children.Add(dockLayout2);

            this.Layout = pageLayout;
        }
    }
}
