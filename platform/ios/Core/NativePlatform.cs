using System;
using XForms.Controls;
using XForms.Layouts;

using XForms.Platform;
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

        public bool UsesLightweightRenderers => true;

        public IContainerRenderer CreateContainerRenderer(
            View view)
        {
            return new ContainerRenderer(view);
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

        public ITextBlockRenderer CreateTextBlockRenderer(
            TextBlock textView)
        {
            return new TextBlockRenderer(textView);
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
