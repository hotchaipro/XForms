using System;
using global::Windows.UI.Xaml.Controls;
using XForms.Controls;

namespace XForms.Windows.Renderers
{
    public class ScrollViewRenderer : ViewRenderer, IScrollViewRenderer
    {
        private ScrollViewer _xamlScrollViewer;
        private View _content;

        public ScrollViewRenderer(
            ScrollView scrollView)
            : base(scrollView)
        {
            this._xamlScrollViewer = new ScrollViewer();

            this.SetNativeElement(this._xamlScrollViewer);
        }

        public View Content
        {
            get
            {
                return this._content;
            }

            set
            {
                this._content = value;

                if (null != value)
                {
                    this._xamlScrollViewer.Content = value.Renderer?.NativeElement;
                }
                else
                {
                    this._xamlScrollViewer.Content = null;
                }
            }
        }

        public bool IsVerticalScrollingEnabled
        {
            get
            {
                return (this._xamlScrollViewer.VerticalScrollMode != ScrollMode.Disabled);
            }

            set
            {
                this._xamlScrollViewer.VerticalScrollMode = (value ? ScrollMode.Auto : ScrollMode.Disabled);
            }
        }

        public bool IsHorizontalScrollingEnabled
        {
            get
            {
                return (this._xamlScrollViewer.HorizontalScrollMode != ScrollMode.Disabled);
            }

            set
            {
                this._xamlScrollViewer.HorizontalScrollMode = (value ? ScrollMode.Auto : ScrollMode.Disabled);
            }
        }
    }
}
