using System;
using XForms.Controls;
using XForms.Layouts;

namespace XForms
{
    public interface IPlatform
    {
        IApplicationRenderer CreateApplicationRenderer(Application application);
        IPageRenderer CreatePageRenderer(Page page);
        IDockLayoutRenderer CreateDockLayoutRenderer(DockLayout dockLayout);
        ITextViewRenderer CreateLabelRenderer(TextView label);
        IContentControlRenderer CreateContentControlRenderer(ContentControl customControl);
        IImageRenderer CreateImageRenderer(Image image);
        IBitmapRenderer CreateBitmapRenderer(Bitmap bitmap);
        IFrameRenderer CreateFrameRenderer(Frame frame);
        IListViewRenderer CreateListViewRenderer(ListView listView);
        ITextEntryRenderer CreateTextEntryRenderer(TextEntry textEntry);
        ICalendarDatePickerRenderer CreateCalendarDatePickerRenderer(CalendarDatePicker datePicker);
        IListPickerRenderer CreateListPickerRenderer(ListPicker listPicker);
        IScrollViewRenderer CreateScrollViewRenderer(ScrollView scrollView);
        IUserControlRenderer CreateUserControlRenderer(UserControl userControl);
        IPopoverRenderer CreatePopoverRenderer(Popover popover);
        ICalendarControlRenderer CreateCalendarControlRenderer(CalendarControl calendarControl);
        IDistributedStackLayoutRenderer CreateDistributedStackLayoutRenderer(DistributedStackLayout distributedStackLayout);
        IProgressRingRenderer CreateProgressRingRenderer(ProgressRing progressRing);
        void NavigateToUri(Uri uri);
    }
}
