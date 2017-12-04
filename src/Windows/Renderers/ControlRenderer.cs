using System;
using global::Windows.UI.Xaml.Media;
using XForms.Controls;

namespace XForms.Windows.Renderers
{
    public abstract class ControlRenderer : ViewRenderer, IControlRenderer
    {
        public ControlRenderer(
            Control control)
            : base(control)
        {
        }

        protected virtual global::Windows.UI.Xaml.Controls.Control NativeControl
        {
            get
            {
                return this.NativeElement as global::Windows.UI.Xaml.Controls.Control;
            }
        }

        public virtual Color BackgroundColor
        {
            get
            {
                var solidColorBrush = this.NativeControl?.Background as SolidColorBrush;
                if (null != solidColorBrush)
                {
                    return solidColorBrush.Color.ToColor();
                }

                return Colors.Transparent;
            }

            set
            {
                var nativeControl = this.NativeControl;
                if (null != nativeControl)
                {
                    Brush backgroundBrush = new SolidColorBrush(value.ToXamlColor());
                    nativeControl.Background = backgroundBrush;
                }
            }
        }
    }
}
