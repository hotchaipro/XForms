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

        public ITextViewRenderer CreateLabelRenderer(
            TextView textView)
        {
            return new TextViewRenderer(this._androidApplication.ApplicationContext, textView);
        }

        public IDockLayoutRenderer CreateDockLayoutRenderer(
            DockLayout dockLayout)
        {
            return new DockLayoutRenderer(this._androidApplication.ApplicationContext, dockLayout);
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

        public ITextEntryRenderer CreateTextEntryRenderer(
            TextEntry textEntry)
        {
            return new TextEntryRenderer(this._androidApplication.ApplicationContext, textEntry);
        }

        public ICalendarDatePickerRenderer CreateCalendarDatePickerRenderer(
            CalendarDatePicker datePicker)
        {
            return new CalendarDatePickerRenderer(this._androidApplication.ApplicationContext, datePicker);
        }

        //public IToggleSwitchRenderer CreateToggleSwitchRenderer(
        //    ToggleSwitch toggleSwitch)
        //{
        //    throw new NotImplementedException(); // return new AndroidToggleSwitchRenderer(toggleSwitch);
        //}

        public IListPickerRenderer CreateListPickerRenderer(
            ListPicker listPicker)
        {
            return new ListPickerRenderer(this._androidApplication.ApplicationContext, listPicker);
        }

        public IScrollViewRenderer CreateScrollViewRenderer(
            ScrollView scrollView)
        {
            return new ScrollViewRenderer(this._androidApplication.ApplicationContext, scrollView);
        }

        public IUserControlRenderer CreateUserControlRenderer(
            UserControl userControl)
        {
            return new UserControlRenderer(this._androidApplication.ApplicationContext, userControl);
        }

        public IPopoverRenderer CreatePopoverRenderer(
            Popover popover)
        {
            return new PopoverRenderer(this._androidApplication.ApplicationContext, popover);
        }

        public ICalendarControlRenderer CreateCalendarControlRenderer(
            CalendarControl calendarControl)
        {
            return new CalendarControlRenderer(this._androidApplication.ApplicationContext, calendarControl);
        }

        public IDistributedStackLayoutRenderer CreateDistributedStackLayoutRenderer(
            DistributedStackLayout distributedStackLayout)
        {
            return new DistributedStackLayoutRenderer(this._androidApplication.ApplicationContext, distributedStackLayout);
        }

        public IProgressRingRenderer CreateProgressRingRenderer(
            ProgressRing progressRing)
        {
            return new ProgressRingRenderer(this._androidApplication.ApplicationContext, progressRing);
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
