using System;
using XForms.Controls;
using XForms.Layouts;

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

        public IDockLayoutRenderer CreateDockLayoutRenderer(
            DockLayout dockLayout)
        {
            return new DockLayoutRenderer(dockLayout);
        }

        public IContentControlRenderer CreateContentControlRenderer(
            ContentControl xControl)
        {
            throw new NotImplementedException(); //return new ContentControlRenderer(this._androidApplication.ApplicationContext, xControl);
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
            throw new NotImplementedException(); //return new ListViewRenderer(this._androidApplication.ApplicationContext, listView);
        }

        public ITextEntryRenderer CreateTextEntryRenderer(
            TextEntry textEntry)
        {
            throw new NotImplementedException(); //return new TextEntryRenderer(this._androidApplication.ApplicationContext, textEntry);
        }

        public ICalendarDatePickerRenderer CreateCalendarDatePickerRenderer(
            CalendarDatePicker datePicker)
        {
            throw new NotImplementedException(); //return new CalendarDatePickerRenderer(this._androidApplication.ApplicationContext, datePicker);
        }

        public IListPickerRenderer CreateListPickerRenderer(
            ListPicker listPicker)
        {
            throw new NotImplementedException(); //return new ListPickerRenderer(this._androidApplication.ApplicationContext, listPicker);
        }

        public IScrollViewRenderer CreateScrollViewRenderer(
            ScrollView scrollView)
        {
            throw new NotImplementedException(); //return new ScrollViewRenderer(this._androidApplication.ApplicationContext, scrollView);
        }

        public IUserControlRenderer CreateUserControlRenderer(
            UserControl userControl)
        {
            throw new NotImplementedException(); //return new UserControlRenderer(this._androidApplication.ApplicationContext, userControl);
        }

        public IPopoverRenderer CreatePopoverRenderer(
            Popover popover)
        {
            throw new NotImplementedException(); //return new PopoverRenderer(this._androidApplication.ApplicationContext, popover);
        }

        public ICalendarControlRenderer CreateCalendarControlRenderer(
            CalendarControl calendarControl)
        {
            throw new NotImplementedException(); //return new CalendarControlRenderer(this._androidApplication.ApplicationContext, calendarControl);
        }

        public IDistributedStackLayoutRenderer CreateDistributedStackLayoutRenderer(
            DistributedStackLayout distributedStackLayout)
        {
            return new DistributedStackLayoutRenderer(distributedStackLayout);
        }

        public IProgressRingRenderer CreateProgressRingRenderer(
            ProgressRing progressRing)
        {
            throw new NotImplementedException(); //return new ProgressRingRenderer(this._androidApplication.ApplicationContext, progressRing);
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

            throw new NotImplementedException();
        }
    }
}
