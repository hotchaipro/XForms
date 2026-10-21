using System;
using XForms.Android.Renderers;
using XForms.Controls;
using XForms.Layouts;
using XForms.Platform;

namespace XForms.Android
{
    public class AndroidPlatform : IPlatform
    {
        private AndroidApplicationActivity _androidApplication;

        public AndroidPlatform(
            AndroidApplicationActivity androidApplication)
        {
            if (null == androidApplication)
            {
                throw new ArgumentNullException(nameof(androidApplication));
            }

            this._androidApplication = androidApplication;
        }

        public bool UsesLightweightRenderers => true;

        public IContainerRenderer CreateContainerRenderer(
            View view)
        {
            return new ContainerRenderer(this._androidApplication.ApplicationContext, view);
        }

        public IApplicationRenderer CreateApplicationRenderer(
            Application application)
        {
            return new ApplicationRenderer(application, this._androidApplication);
        }

        public IFrameRenderer CreateFrameRenderer(
            Frame frame)
        {
            return new FrameRenderer(this._androidApplication.ApplicationContext, frame);
        }

        public ITextBlockRenderer CreateTextBlockRenderer(
            TextBlock textView)
        {
            return new TextBlockRenderer(this._androidApplication.ApplicationContext, textView);
        }

        public IImageRenderer CreateImageRenderer(
            Image image)
        {
            return new ImageRenderer(this._androidApplication.ApplicationContext, image);
        }

        public IBitmapRenderer CreateBitmapRenderer(
            Bitmap bitmap)
        {
            return new BitmapRenderer(this._androidApplication.ApplicationContext, bitmap);
        }

        public IListViewRenderer CreateListViewRenderer(
            ListView listView)
        {
            return new ListViewRenderer(this._androidApplication.ApplicationContext, listView);
        }

        public ITextBoxRenderer CreateTextBoxRenderer(
            TextBox textEntry)
        {
            return new TextBoxRenderer(this._androidApplication.ApplicationContext, textEntry);
        }


        //public IToggleSwitchRenderer CreateToggleSwitchRenderer(
        //    ToggleSwitch toggleSwitch)
        //{
        //    throw new NotImplementedException(); // return new AndroidToggleSwitchRenderer(toggleSwitch);
        //}


        public IScrollViewerRenderer CreateScrollViewerRenderer(
            ScrollViewer scrollView)
        {
            return new ScrollViewerRenderer(this._androidApplication.ApplicationContext, scrollView);
        }

        public IPopupRenderer CreatePopupRenderer(
            Popup popup)
        {
            return new PopupRenderer(this._androidApplication.ApplicationContext, popup);
        }



        public ITextLayoutRenderer CreateTextLayout(
            TextLayout layout)
        {
            return new TextLayoutRenderer(this._androidApplication.ApplicationContext, layout);
        }

        public void RegisterFont(
            string familyName,
            string fileName)
        {
            Fonts.Register(this._androidApplication.ApplicationContext, familyName, fileName);
        }

        public PlatformServices CreateServices()
        {
            return AndroidServices.Create(this._androidApplication);
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
            this._androidApplication.StartActivity(intent);
        }
    }
}
