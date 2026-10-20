using System;
using XForms.Controls;
using XForms.Layouts;

namespace XForms
{
    public interface IPlatform
    {
        IApplicationRenderer CreateApplicationRenderer(Application application);
        IPageRenderer CreatePageRenderer(Page page);
        IPanelRenderer CreatePanelRenderer(Panel layout);
        ITextBlockRenderer CreateTextBlockRenderer(TextBlock label);
        IContentControlRenderer CreateContentControlRenderer(ContentControl customControl);
        IImageRenderer CreateImageRenderer(Image image);
        IBitmapRenderer CreateBitmapRenderer(Bitmap bitmap);
        IFrameRenderer CreateFrameRenderer(Frame frame);
        IListViewRenderer CreateListViewRenderer(ListView listView);
        ITextBoxRenderer CreateTextBoxRenderer(TextBox textEntry);
        IScrollViewerRenderer CreateScrollViewerRenderer(ScrollViewer scrollView);
        IUserControlRenderer CreateUserControlRenderer(UserControl userControl);
        IPopupRenderer CreatePopupRenderer(Popup popup);
        void NavigateToUri(Uri uri);

        Size MeasureText(string text, TextFormat textFormat);
    }
}
