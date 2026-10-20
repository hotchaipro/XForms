using System;
using XForms.Controls;
using XForms.Layouts;

using XForms.iOS.Renderers;

namespace XForms.iOS
{
    public class NativePlatform : IPlatform
    {
        private NativeApplicationDelegate _nativeApplication;

        public NativePlatform(
            NativeApplicationDelegate nativeApplication)
        {
            if (null == nativeApplication)
            {
                throw new ArgumentNullException(nameof(nativeApplication));
            }

            this._nativeApplication = nativeApplication;
        }

        public IApplicationRenderer CreateApplicationRenderer(
            Application application)
        {
            return new ApplicationRenderer(application, this._nativeApplication);
        }

        public IFrameRenderer CreateFrameRenderer(
            Frame frame)
        {
            return new FrameRenderer(frame);
        }

        public IPageRenderer CreatePageRenderer(
            Page page)
        {
            return new PageRenderer(page);
        }

        public ITextViewRenderer CreateLabelRenderer(
            TextView textView)
        {
            return new TextViewRenderer(textView);
        }

        public ILayoutRenderer CreateLayoutRenderer(
            Layout layout)
        {
            return new LayoutRenderer(layout);
        }

        public IContentControlRenderer CreateContentControlRenderer(
            ContentControl xControl)
        {
            return new ContentControlRenderer(xControl);
        }

        public IImageRenderer CreateImageRenderer(
            Image image)
        {
            return new ImageRenderer(image);
        }

        public IBitmapRenderer CreateBitmapRenderer(
            Bitmap bitmap)
        {
            return new BitmapRenderer(bitmap);
        }

        public IListViewRenderer CreateListViewRenderer(
            ListView listView)
        {
            return new ListViewRenderer(listView);
        }

        public ITextEntryRenderer CreateTextEntryRenderer(
            TextEntry textEntry)
        {
            return new TextEntryRenderer(textEntry);
        }



        public IScrollViewRenderer CreateScrollViewRenderer(
            ScrollView scrollView)
        {
            return new ScrollViewRenderer(scrollView);
        }

        public IUserControlRenderer CreateUserControlRenderer(
            UserControl userControl)
        {
            return new UserControlRenderer(userControl);
        }

        public IPopupRenderer CreatePopupRenderer(
            Popup popup)
        {
            return new PopupRenderer(popup);
        }



        public Size MeasureText(
            string text,
            TextFormat textFormat)
        {
            return DrawContext.MeasureText(text, textFormat);
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

            UIKit.UIApplication.SharedApplication.OpenUrl(
                new Foundation.NSUrl(uri.AbsoluteUri),
                new UIKit.UIApplicationOpenUrlOptions(),
                null);
        }
    }
}
