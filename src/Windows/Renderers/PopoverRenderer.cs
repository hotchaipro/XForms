using System;
using global::Windows.UI.Xaml;
using XForms.Controls;
using XamlPopup = global::Windows.UI.Xaml.Controls.Primitives.Popup;

namespace XForms.Windows.Renderers
{
    public class PopoverRenderer : ElementRenderer, IPopoverRenderer
    {
        private View _content;
        private XamlPopup _xamlPopupControl;
        private BackgroundBlurEffect _backgroundBlurEffect;

        public PopoverRenderer(
            Popover popover)
            : base(popover)
        {
            this._xamlPopupControl = new XamlPopup()
            {
                IsLightDismissEnabled = true,
                VerticalAlignment = global::Windows.UI.Xaml.VerticalAlignment.Center,
                HorizontalAlignment = global::Windows.UI.Xaml.HorizontalAlignment.Center,
                Visibility = Visibility.Collapsed,
            };

            this._xamlPopupControl.Loaded += PopupControl_Loaded;
            this._xamlPopupControl.Unloaded += PopupControl_Unloaded;
            this._xamlPopupControl.Opened += PopupControl_Opened;
            this._xamlPopupControl.Closed += PopupControl_Closed;

            this.SetNativeElement(this._xamlPopupControl);

            this.CenterPopup();
        }

        private void PopupControl_Loaded(
            object sender,
            global::Windows.UI.Xaml.RoutedEventArgs e)
        {
            Window.Current.SizeChanged += Window_SizeChanged;
        }

        private void PopupControl_Unloaded(
            object sender,
            global::Windows.UI.Xaml.RoutedEventArgs e)
        {
            Window.Current.SizeChanged -= Window_SizeChanged;
        }

        private void PopupControl_Opened(
            object sender,
            object e)
        {
            this.CenterPopup();
        }

        private void PopupControl_Closed(
            object sender,
            object e)
        {
            this._backgroundBlurEffect?.Remove();
            this._backgroundBlurEffect = null;

            ((IPopoverDelegate)this.Element).NotifyClosed();
        }

        private void Window_SizeChanged(
            object sender,
            global::Windows.UI.Core.WindowSizeChangedEventArgs e)
        {
            this.CenterPopup();
        }

        private async void CenterPopup()
        {
            // HACK: Ridiculous hack to ensure the popup is centered

            var content = this.Content;
            if (null != content)
            {
                for (int i = 0; i < 10; i += 1)
                {
                    if (content.Size != Size.Empty)
                    {
                        break;
                    }

                    await System.Threading.Tasks.Task.Delay(25);
                }

                this._xamlPopupControl.HorizontalOffset = (Window.Current.Bounds.Width - content.Size.Width) / 2;
                this._xamlPopupControl.VerticalOffset = (Window.Current.Bounds.Height - content.Size.Height) / 2;

                this._xamlPopupControl.Visibility = Visibility.Visible;
            }
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

                this._xamlPopupControl.Child = value?.Renderer.NativeElement as global::Windows.UI.Xaml.UIElement;
            }
        }

        public void ShowAsync()
        {
            this._xamlPopupControl.IsOpen = true;

            var frame = Window.Current.Content as global::Windows.UI.Xaml.Controls.Frame;
            var panel = frame.Content as global::Windows.UI.Xaml.Controls.Panel;

            this._backgroundBlurEffect = new BackgroundBlurEffect(panel);
            this._backgroundBlurEffect.Apply(15f);
        }

        public void Close()
        {
            this._xamlPopupControl.IsOpen = false;
        }
    }
}
