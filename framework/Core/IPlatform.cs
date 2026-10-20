using System;
using XForms.Controls;
using XForms.Layouts;

namespace XForms
{
    public interface IPlatform
    {
        IApplicationRenderer CreateApplicationRenderer(Application application);
        IPageRenderer CreatePageRenderer(Page page);
        ILayoutRenderer CreateLayoutRenderer(Layout layout);
        ITextViewRenderer CreateLabelRenderer(TextView label);
        IContentControlRenderer CreateContentControlRenderer(ContentControl customControl);
        IImageRenderer CreateImageRenderer(Image image);
        IBitmapRenderer CreateBitmapRenderer(Bitmap bitmap);
        IFrameRenderer CreateFrameRenderer(Frame frame);
        IListViewRenderer CreateListViewRenderer(ListView listView);
        ITextEntryRenderer CreateTextEntryRenderer(TextEntry textEntry);
        IScrollViewRenderer CreateScrollViewRenderer(ScrollView scrollView);
        IUserControlRenderer CreateUserControlRenderer(UserControl userControl);
        IPopupRenderer CreatePopupRenderer(Popup popup);
        void NavigateToUri(Uri uri);

        Size MeasureText(string text, TextFormat textFormat);
    }
}
