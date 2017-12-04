using System;
using XamlPage = global::Windows.UI.Xaml.Controls.Page;

namespace XForms.Windows.Renderers
{
    public class PageRenderer : ViewRenderer, IPageRenderer
    {
        public PageRenderer(
            Page page)
            : base(page)
        {
            var xamlPage = new XamlPage()
            {
                NavigationCacheMode = global::Windows.UI.Xaml.Navigation.NavigationCacheMode.Enabled,
            };
            this.SetNativeElement(xamlPage);
        }

        public virtual Color BackgroundColor
        {
            get
            {
                var solidColorBrush = this.NativePage?.Background as global::Windows.UI.Xaml.Media.SolidColorBrush;
                if (null != solidColorBrush)
                {
                    return solidColorBrush.Color.ToColor();
                }

                return Colors.Purple;
            }

            set
            {
                var nativePage = this.NativePage;
                if (null != nativePage)
                {
                    var backgroundBrush = new global::Windows.UI.Xaml.Media.SolidColorBrush(value.ToXamlColor());
                    nativePage.Background = backgroundBrush;

                    var layoutPanel = ((global::Windows.UI.Xaml.Controls.Panel)nativePage.Content);
                    if (null != layoutPanel)
                    {
                        layoutPanel.Background = backgroundBrush;
                    }
                }
            }
        }

        private XamlPage NativePage
        {
            get
            {
                return (XamlPage)this.NativeElement;
            }
        }

        public void SetLayout(
            ILayoutRenderer layoutRenderer)
        {
            var layoutPanel = ((global::Windows.UI.Xaml.Controls.Panel)layoutRenderer.NativeElement);
            layoutPanel.Background = this.NativePage.Background;
            this.NativePage.Content = layoutPanel;
        }
    }
}
