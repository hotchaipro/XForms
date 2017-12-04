using System;
using global::Windows.UI.Xaml;
using XForms.Layouts;
using XForms.Windows.Layouts;

namespace XForms.Windows.Renderers
{
    public class DistributedStackLayoutRenderer : ViewRenderer, IDistributedStackLayoutRenderer
    {
        private XamlDistributedStackPanel _panel;

        public DistributedStackLayoutRenderer(
            XForms.Layouts.DistributedStackLayout layout)
            : base(layout)
        {
            this._panel = new XamlDistributedStackPanel()
            {
                //Background = new global::Windows.UI.Xaml.Media.SolidColorBrush(global::Windows.UI.Colors.Red),
            };

            this.SetNativeElement(this._panel);
        }

        public void ClearChildren()
        {
            this._panel.Children.Clear();
        }

        public void AddChild(
            IElementRenderer childRenderer)
        {
            var nativeChild = (FrameworkElement)childRenderer.NativeElement;
            this._panel.Children.Add(nativeChild);
        }

        public void InsertChild(
            int index,
            IElementRenderer childRenderer)
        {
            var nativeChild = (FrameworkElement)childRenderer.NativeElement;
            this._panel.Children.Insert(index, nativeChild);
        }

        public void RemoveChildAt(
            int index)
        {
            this._panel.Children.RemoveAt(index);
        }

        public void ReplaceChild(
            int index,
            IElementRenderer childRenderer)
        {
            var nativeChild = (FrameworkElement)childRenderer.NativeElement;
            this._panel.Children[index] = nativeChild;
        }
    }
}
