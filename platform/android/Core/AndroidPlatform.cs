using System;
using XForms.Android.Renderers;
using XForms.Controls;
using XForms.Layouts;

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

        public IPageRenderer CreatePageRenderer(
            Page page)
        {
            return new PageRenderer(this._androidApplication.ApplicationContext, page);
        }

        public ITextBlockRenderer CreateTextBlockRenderer(
            TextBlock textView)
        {
            return new TextBlockRenderer(this._androidApplication.ApplicationContext, textView);
        }

        public IPanelRenderer CreatePanelRenderer(
            Panel layout)
        {
            return new PanelRenderer(this._androidApplication.ApplicationContext, layout);
        }

        public IContentControlRenderer CreateContentControlRenderer(
            ContentControl xControl)
        {
            return new ContentControlRenderer(this._androidApplication.ApplicationContext, xControl);
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

        public IUserControlRenderer CreateUserControlRenderer(
            UserControl userControl)
        {
            return new UserControlRenderer(this._androidApplication.ApplicationContext, userControl);
        }

        public IPopupRenderer CreatePopupRenderer(
            Popup popup)
        {
            return new PopupRenderer(this._androidApplication.ApplicationContext, popup);
        }



        public Size MeasureText(
            string text,
            TextFormat textFormat)
        {
            return DrawContext.MeasureText(this._androidApplication.ApplicationContext, text, textFormat);
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
