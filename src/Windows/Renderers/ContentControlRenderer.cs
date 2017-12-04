using System;
using XForms.Controls;
using XamlBorderControl = global::Windows.UI.Xaml.Controls.Border;

namespace XForms.Windows.Renderers
{
    public class ContentControlRenderer : ControlRenderer, IContentControlRenderer
    {
        private XamlBorderControl _xamlBorderControl;

        public ContentControlRenderer(
            ContentControl customControl)
            : base(customControl)
        {
            this._xamlBorderControl = new XamlBorderControl();

            this.SetNativeElement(this._xamlBorderControl);
        }

        public override Color BackgroundColor
        {
            get
            {
                var backgroundBrush = this._xamlBorderControl.Background as global::Windows.UI.Xaml.Media.SolidColorBrush;

                return backgroundBrush?.Color.ToColor() ?? Colors.Transparent;
            }

            set
            {
                this._xamlBorderControl.Background = new global::Windows.UI.Xaml.Media.SolidColorBrush(value.ToXamlColor());
            }
        }

        void IContentControlRenderer.SetContent(
            IViewRenderer content)
        {
            var uiElement = (global::Windows.UI.Xaml.UIElement)content.NativeElement;

            this._xamlBorderControl.Child = uiElement;
        }
    }
}
