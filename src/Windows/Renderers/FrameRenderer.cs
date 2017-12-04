using System;
using XamlFrame = global::Windows.UI.Xaml.Controls.Frame;

namespace XForms.Windows.Renderers
{
    public class FrameRenderer : ViewRenderer, IFrameRenderer
    {
        private global::Windows.UI.Xaml.Controls.Panel _pageCache;
        private global::Windows.UI.Xaml.Controls.Page _activePage;

        public FrameRenderer(
            Frame frame)
            : base(frame)
        {
            var nativeFrame = new XamlFrame()
            {
            };
            this._pageCache = new global::Windows.UI.Xaml.Controls.Grid()
            {
                HorizontalAlignment = global::Windows.UI.Xaml.HorizontalAlignment.Stretch,
                VerticalAlignment = global::Windows.UI.Xaml.VerticalAlignment.Stretch,
            };
            nativeFrame.Content = this._pageCache;

            this.SetNativeElement(nativeFrame);
        }

        private XamlFrame NativeFrame
        {
            get
            {
                return (XamlFrame)this.NativeElement;
            }
        }

        void IFrameRenderer.Push(
            Page page)
        {
            var pageRenderer = page.Renderer as PageRenderer;
            var nativePage = pageRenderer.NativeElement as global::Windows.UI.Xaml.Controls.Page;
            if (!this._pageCache.Children.Contains(nativePage))
            {
                this._pageCache.Children.Add(nativePage);
            }
        }

        public void SetContent(
            Page newPage,
            bool hideCurrentPage)
        {
            var newPageRenderer = newPage.Renderer as PageRenderer;
            var nativePage = newPageRenderer.NativeElement as global::Windows.UI.Xaml.Controls.Page;
            if (!this._pageCache.Children.Contains(nativePage))
            {
                this._pageCache.Children.Add(nativePage);
            }

            if (hideCurrentPage)
            {
                var activePage = this._activePage;
                if (null != activePage)
                {
                    activePage.Visibility = global::Windows.UI.Xaml.Visibility.Collapsed;
                }
            }

            nativePage.Visibility = global::Windows.UI.Xaml.Visibility.Visible;
            this._activePage = nativePage;
        }
    }
}
