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

        public ITextBlockRenderer CreateTextBlockRenderer(
            TextBlock textView)
        {
            if (textView.CanBeLightweight)
            {
                return new LightweightTextBlockRenderer(textView);
            }

            return new TextBlockRenderer(textView);
        }

        public IPanelRenderer CreatePanelRenderer(
            Panel layout)
        {
            if (layout.CanBeLightweight)
            {
                return new LightweightPanelRenderer(layout);
            }

            return new PanelRenderer(layout);
        }

        public IContentControlRenderer CreateContentControlRenderer(
            ContentControl xControl)
        {
            if (xControl.CanBeLightweight)
            {
                return new LightweightControlRenderer(xControl);
            }

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

        public ITextBoxRenderer CreateTextBoxRenderer(
            TextBox textEntry)
        {
            return new TextBoxRenderer(textEntry);
        }



        public IScrollViewerRenderer CreateScrollViewerRenderer(
            ScrollViewer scrollView)
        {
            return new ScrollViewerRenderer(scrollView);
        }

        public IUserControlRenderer CreateUserControlRenderer(
            UserControl userControl)
        {
            if (userControl.CanBeLightweight)
            {
                return new LightweightControlRenderer(userControl);
            }

            return new UserControlRenderer(userControl);
        }

        public IPopupRenderer CreatePopupRenderer(
            Popup popup)
        {
            return new PopupRenderer(popup);
        }



        public ITextLayoutRenderer CreateTextLayout(
            TextLayout layout)
        {
            return new TextLayoutRenderer(layout);
        }

        public void RegisterFont(
            string familyName,
            string fileName)
        {
            Fonts.Register(familyName, fileName);
        }

        public PlatformServices CreateServices()
        {
            return IosServices.Create(this._nativeApplication);
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
