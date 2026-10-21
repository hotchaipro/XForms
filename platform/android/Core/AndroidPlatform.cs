using System;
using XForms.Android.Renderers;
using XForms.Primitives;
using XForms.Platform;

namespace XForms.Android
{
    internal sealed class AndroidPlatform : IPlatform
    {
        private readonly AndroidApplication _host;

        public AndroidPlatform(
            AndroidApplication host)
        {
            this._host = host ?? throw new ArgumentNullException(nameof(host));

            // The app's services are this platform's
            PlatformServices.Current = AndroidServices.Create(host);
        }

        public bool UsesLightweightRenderers => true;

        public IContainerRenderer CreateContainerRenderer(
            View view)
        {
            return new ContainerRenderer(this._host.Context, view);
        }

        public IApplicationRenderer CreateApplicationRenderer(
            Application application)
        {
            return new ApplicationRenderer(application, this._host);
        }

        public IFrameRenderer CreateFrameRenderer(
            Frame frame)
        {
            return new FrameRenderer(this._host.Context, frame);
        }

        public ITextBlockRenderer CreateTextBlockRenderer(
            TextBlock textView)
        {
            return new TextBlockRenderer(this._host.Context, textView);
        }

        public IImageRenderer CreateImageRenderer(
            Image image)
        {
            return new ImageRenderer(this._host.Context, image);
        }

        public IBitmapRenderer CreateBitmapRenderer(
            Bitmap bitmap)
        {
            return new BitmapRenderer(this._host.Context, bitmap);
        }

        public IItemsViewRenderer CreateItemsViewRenderer(
            ItemsView listView)
        {
            return new ItemsViewRenderer(this._host.Context, listView);
        }

        public ITextBoxRenderer CreateTextBoxRenderer(
            TextBox textEntry)
        {
            return new TextBoxRenderer(this._host.Context, textEntry);
        }


        //public IToggleSwitchRenderer CreateToggleSwitchRenderer(
        //    ToggleSwitch toggleSwitch)
        //{
        //    throw new NotImplementedException(); // return new AndroidToggleSwitchRenderer(toggleSwitch);
        //}


        public IScrollViewerRenderer CreateScrollViewerRenderer(
            ScrollViewer scrollView)
        {
            return new ScrollViewerRenderer(this._host.Context, scrollView);
        }

        public IPopupRenderer CreatePopupRenderer(
            Popup popup)
        {
            return new PopupRenderer(this._host.Context, popup);
        }



        public ITextLayoutRenderer CreateTextLayout(
            TextLayout layout)
        {
            return new TextLayoutRenderer(this._host.Context, layout);
        }

        public void RegisterFont(
            string familyName,
            string fileName)
        {
            Fonts.Register(this._host.Context, familyName, fileName);
        }

        public void NavigateToUri(
            Uri uri)
        {
            if (null == uri)
            {
                throw new ArgumentNullException(nameof(uri));
            }

            if (!uri.IsAbsoluteUri)
            {
                throw new ArgumentOutOfRangeException(nameof(uri));
            }

            var androidUri = global::Android.Net.Uri.Parse(uri.ToString());
            var intent = new global::Android.Content.Intent(global::Android.Content.Intent.ActionView, androidUri);
            AndroidServices.StartActivity(this._host, intent);
        }
    }
}
