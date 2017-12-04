using System;
using XForms.Controls;
using XForms.Layouts;
using XForms.Windows.Renderers;
using XamlApplication = global::Windows.UI.Xaml.Application;

namespace XForms.Windows
{
    public class NativePlatform : IPlatform
    {
        private XamlApplication _xamlApplication;

        public NativePlatform(
            XamlApplication xamlApplication)
        {
            if (null == xamlApplication)
            {
                throw new ArgumentNullException(nameof(xamlApplication));
            }

            this._xamlApplication = xamlApplication;
        }

        public IApplicationRenderer CreateApplicationRenderer(
            Application application)
        {
            return new ApplicationRenderer(application, this._xamlApplication);
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

        public IDockLayoutRenderer CreateDockLayoutRenderer(
            DockLayout dockLayout)
        {
            return new DockLayoutRenderer(dockLayout);
        }

        //public IButtonRenderer CreateButtonRenderer(
        //    Button button)
        //{
        //    return new XamlButtonRenderer(button);
        //}

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

        public IFrameRenderer CreateFrameRenderer(
            Frame frame)
        {
            return new FrameRenderer(frame);
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

        public ICalendarDatePickerRenderer CreateCalendarDatePickerRenderer(
            CalendarDatePicker datePicker)
        {
            return new CalendarDatePickerRenderer(datePicker);
        }

        //public IToggleSwitchRenderer CreateToggleSwitchRenderer(
        //    ToggleSwitch toggleSwitch)
        //{
        //    return new XamlToggleSwitchRenderer(toggleSwitch);
        //}

        public IListPickerRenderer CreateListPickerRenderer(
            ListPicker listPicker)
        {
            return new ListPickerRenderer(listPicker);
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

        public IPopoverRenderer CreatePopoverRenderer(
            Popover popover)
        {
            return new PopoverRenderer(popover);
        }

        public ICalendarControlRenderer CreateCalendarControlRenderer(
            CalendarControl calendarControl)
        {
            return new CalendarControlRenderer(calendarControl);
        }

        public IDistributedStackLayoutRenderer CreateDistributedStackLayoutRenderer(
            DistributedStackLayout distributedStackLayout)
        {
            return new DistributedStackLayoutRenderer(distributedStackLayout);
        }

        public IProgressRingRenderer CreateProgressRingRenderer(
            ProgressRing progressRing)
        {
            return new ProgressRingRenderer(progressRing);
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

            var ignore = global::Windows.System.Launcher.LaunchUriAsync(uri);
        }
    }
}
