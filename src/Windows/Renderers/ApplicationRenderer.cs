using System;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Metadata;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using XamlApplication = global::Windows.UI.Xaml.Application;
using XamlColor = global::Windows.UI.Color;

namespace XForms.Windows.Renderers
{
    public class ApplicationRenderer : IApplicationRenderer
    {
        private Application _application;
        private IApplicationDelegate _applicationDelegate;
        private XamlApplication _xamlApplication;
        private Color? _titleBarAccentColor, _titleBarForegroundColor;
        private Frame _rootFrame;

        public ApplicationRenderer(
            XForms.Application application,
            XamlApplication xamlApplication)
        {
            if (null == application)
            {
                throw new ArgumentNullException(nameof(application));
            }

            if (null == xamlApplication)
            {
                throw new ArgumentNullException(nameof(xamlApplication));
            }

            this._application = application;
            this._applicationDelegate = application;
            this._xamlApplication = xamlApplication;

            // Handle lifetime events
            if (ApiInformation.IsApiContractPresent("global::Windows.Foundation.UniversalApiContract", 3, 0))
            {
                xamlApplication.EnteredBackground += Application_EnteredBackground;
                xamlApplication.LeavingBackground += Application_LeavingBackground;
            }
            else
            {
                xamlApplication.Suspending += Application_Suspending;
                xamlApplication.Resuming += Application_Resuming;
            }

            // Handle the back button
            var systemNavigationView = SystemNavigationManager.GetForCurrentView();
            systemNavigationView.BackRequested += SystemNavigationView_BackRequested;

            // Handle the input pane being shown or hidden
            InputPane inputPane = InputPane.GetForCurrentView();
            inputPane.Showing += InputPane_Showing;
            inputPane.Hiding += InputPane_Hiding;
        }

        private void InputPane_Showing(
            InputPane sender,
            InputPaneVisibilityEventArgs args)
        {
            var frame = Window.Current.Content as global::Windows.UI.Xaml.Controls.Frame;
            if (null != frame)
            {
                var appView = ApplicationView.GetForCurrentView();
                var visibleBounds = appView.VisibleBounds;
                Rect frameRect = new Rect(0, 0, frame.ActualWidth, frame.ActualHeight);
                Rect keyboardRect = args.OccludedRect;
                frame.Height = visibleBounds.Height - keyboardRect.Height;
            }
        }

        private void InputPane_Hiding(
            InputPane sender,
            InputPaneVisibilityEventArgs args)
        {
            var frame = Window.Current.Content as global::Windows.UI.Xaml.Controls.Frame;
            if (null != frame)
            {
                var appView = ApplicationView.GetForCurrentView();
                var visibleBounds = appView.VisibleBounds;
                frame.Height = visibleBounds.Height;
            }
        }

        public Color? TitleBarAccentColor
        {
            get
            {
                if (this._titleBarAccentColor.HasValue)
                {
                    return this._titleBarAccentColor.Value;
                }
                else
                {
                    var appView = ApplicationView.GetForCurrentView();
                    return appView.TitleBar.BackgroundColor?.ToColor() ?? null;
                }
            }

            set
            {
                this._titleBarAccentColor = value;

               //this.UpdateTitleBar();
            }
        }

        public Color? TitleBarForegroundColor
        {
            get
            {
                if (this._titleBarForegroundColor.HasValue)
                {
                    return this._titleBarForegroundColor.Value;
                }
                else
                {
                    var appView = ApplicationView.GetForCurrentView();
                    return appView.TitleBar.ForegroundColor?.ToColor() ?? null;
                }
            }

            set
            {
                this._titleBarForegroundColor = value;

                //this.UpdateTitleBar();

                //if ((null != value) && (value.HasValue))
                //{
                //    global::Windows.UI.Xaml.Application.Current.Resources["SystemControlForegroundBaseHighBrush"] = value.Value.ToXamlColor();
                //    global::Windows.UI.Xaml.Application.Current.Resources["SystemControlForegroundBaseMediumHighBrush"] = value.Value.ToXamlColor();
                //    global::Windows.UI.Xaml.Application.Current.Resources["SystemControlPageTextBaseMediumBrush"] = value.Value.ToXamlColor();
                //}
            }
        }

        //public Color? BackgroundColor
        //{
        //    get
        //    {
        //        //var uiSettings = new global::Windows.UI.ViewManagement.UISettings();
        //        //var backgroundColor = uiSettings.GetColorValue(global::Windows.UI.ViewManagement.UIColorType.Background);

        //        //// Get the system background color
        //        //return ((global::Windows.UI.Color)global::Windows.UI.Xaml.Application.Current.Resources["SystemColorWindowColor"]).ToColor();

        //        return this._backgroundColor;
        //    }

        //    set
        //    {
        //        //// Set the system background color
        //        //global::Windows.UI.Xaml.Application.Current.Resources["SystemColorWindowColor"] = value.ToXamlColor();

        //        this._backgroundColor = value;

        //        if ((null != value) && (value.HasValue) && (null != this._rootFrame))
        //        {
        //            var nativeFrame = (global::Windows.UI.Xaml.Controls.Frame)this._rootFrame.Renderer.NativeElement;
        //            if (null != nativeFrame)
        //            {
        //                nativeFrame.Background = new SolidColorBrush(value.Value.ToXamlColor());
        //            }
        //        }
        //    }
        //}

        public Color AccentColor
        {
            get
            {
                // Get the system accent color
                return ((global::Windows.UI.Color)global::Windows.UI.Xaml.Application.Current.Resources["SystemAccentColor"]).ToColor();
            }

            set
            {
                // Set the system accent color
                global::Windows.UI.Xaml.Application.Current.Resources["SystemAccentColor"] = value.ToXamlColor();
            }
        }

        public Color ForegroundColor
        {
            get
            {
                return this.Theme.ForegroundColor;

                // Get the system foreground color
                //return ((global::Windows.UI.Xaml.Media.SolidColorBrush)global::Windows.UI.Xaml.Application.Current.Resources["SystemControlForegroundBaseHighBrush"]).Color.ToColor();
                //return ((global::Windows.UI.Color)global::Windows.UI.Xaml.Application.Current.Resources["SystemColorHighlightTextColor"]).ToColor();
            }
        }

        private void SystemNavigationView_BackRequested(
            object sender,
            BackRequestedEventArgs e)
        {
            if (this._application.CanNavigateBack)
            {
                this._application.NavigateBack();
                e.Handled = true;
            }
        }

        private void Application_Suspending(
            object sender,
            global::Windows.ApplicationModel.SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            try
            {
                this._applicationDelegate.NotifySuspend();
            }
            finally
            {
                deferral.Complete();
            }
        }

        private void Application_Resuming(
            object sender,
            object e)
        {
            this._applicationDelegate.NotifyResume();

            GraphicsManager.Shared.ResumeDevice();
        }

        private void Application_EnteredBackground(
            object sender,
            global::Windows.ApplicationModel.EnteredBackgroundEventArgs e)
        {
            var deferral = e.GetDeferral();
            try
            {
                this._applicationDelegate.NotifySuspend();
            }
            finally
            {
                deferral.Complete();
            }
        }

        private void Application_LeavingBackground(
            object sender,
            global::Windows.ApplicationModel.LeavingBackgroundEventArgs e)
        {
            var deferral = e.GetDeferral();
            try
            {
                this._applicationDelegate.NotifyResume();

                GraphicsManager.Shared.ResumeDevice();
            }
            finally
            {
                deferral.Complete();
            }
        }

        public void SetRootFrame(
            Frame frame)
        {
            if (null == frame)
            {
                throw new ArgumentNullException(nameof(frame));
            }

            this._rootFrame = frame;

            var nativeFrame = (global::Windows.UI.Xaml.Controls.Frame)frame.Renderer.NativeElement;
            //if ((null != this._backgroundColor) && (this._backgroundColor.HasValue))
            //{
            //    nativeFrame.Background = new SolidColorBrush(this._backgroundColor.Value.ToXamlColor());
            //}
            global::Windows.UI.Xaml.Window.Current.Content = nativeFrame;
        }

        public void OnNavigationComplete()
        {
            // Show a system back button on devices that don't have a hardware back button
            var systemNavigationView = SystemNavigationManager.GetForCurrentView();
            systemNavigationView.AppViewBackButtonVisibility = (this._application.CanNavigateBack ? AppViewBackButtonVisibility.Visible : AppViewBackButtonVisibility.Collapsed);
        }

        public void InvokeOnUIThreadAsync(
            Action action)
        {
            CoreDispatcher dispatcher = CoreApplication.MainView.CoreWindow.Dispatcher;
            if (dispatcher.HasThreadAccess)
            {
                action();
            }
            else
            {
                // NOTE: Assign a value to the result to avoid a compiler warning about not using the await keyword
                var asyncAction = dispatcher.RunAsync(CoreDispatcherPriority.Normal, new DispatchedHandler(action));
            }
        }

        public void Exit()
        {
            this._xamlApplication.Exit();
        }

        public AppTheme Theme { get; private set; }

        public void ApplyTheme(
            AppTheme theme)
        {
            this.Theme = theme;

            this.AccentColor = theme.AccentColor;

            //this.UpdateTitleBar();

            var nativeFrame = (global::Windows.UI.Xaml.Controls.Frame)this._rootFrame.Renderer.NativeElement;
            this.ApplyThemeTo(nativeFrame);

            var nativePanel = (global::Windows.UI.Xaml.Controls.Panel)nativeFrame.Content;
            this.ApplyThemeTo(nativePanel);

            foreach (global::Windows.UI.Xaml.FrameworkElement element in nativePanel.Children)
            {
                if (null != element)
                {
                    this.ApplyThemeTo(element);
                }
            }

            this.UpdateTitleBar();
        }

        public void ApplyThemeTo(
            global::Windows.UI.Xaml.FrameworkElement element)
        {
            if (null == element)
            {
                throw new ArgumentNullException(nameof(element));
            }

            var xamlFrame = element as global::Windows.UI.Xaml.Controls.Frame;
            var xamlPage = element as global::Windows.UI.Xaml.Controls.Page;

            if (this.Theme == AppTheme.Light)
            {
                element.RequestedTheme = ElementTheme.Light;
                if (null != xamlFrame)
                {
                    xamlFrame.Background = new SolidColorBrush(global::Windows.UI.Colors.White);
                }
            }
            else if (this.Theme == AppTheme.Dark)
            {
                element.RequestedTheme = ElementTheme.Dark;
                if (null != xamlFrame)
                {
                    xamlFrame.Background = new SolidColorBrush(global::Windows.UI.Colors.Black);
                }
            }
        }

        private void UpdateTitleBar()
        {

            if (global::Windows.Foundation.Metadata.ApiInformation.IsTypePresent("global::Windows.UI.ViewManagement.StatusBar"))
            {
                var statusBar = global::Windows.UI.ViewManagement.StatusBar.GetForCurrentView();
                statusBar.ForegroundColor = this.TitleBarForegroundColor?.ToXamlColor();
                statusBar.BackgroundColor = this.TitleBarAccentColor?.ToXamlColor();
                statusBar.BackgroundOpacity = 1;
                statusBar.ProgressIndicator.Text = "SOONER OR LATER"; // TODO: App title property
                statusBar.ProgressIndicator.ProgressValue = 0;
                var nowait = statusBar.ProgressIndicator.ShowAsync();
            }
            else
            {
                var appView = ApplicationView.GetForCurrentView();
                var titleBar = appView.TitleBar;

                XamlColor? backgroundColor = this._titleBarAccentColor?.ToXamlColor();
                XamlColor? foregroundColor = this._titleBarForegroundColor?.ToXamlColor();
                XamlColor? hoverBackgroundColor = global::Windows.UI.Color.FromArgb(0xff, 0x99, 0x99, 0x99);

                // Title bar colors. Alpha must be 255.
                titleBar.BackgroundColor = backgroundColor;
                titleBar.ForegroundColor = foregroundColor;
                titleBar.InactiveBackgroundColor = backgroundColor;
                titleBar.InactiveForegroundColor = foregroundColor;

                // Title bar button background colors. Alpha is respected when the view is extended
                // into the title bar, otherwise, alpha is ignored and treated as if it were 255.
                titleBar.ButtonBackgroundColor = backgroundColor;
                titleBar.ButtonHoverBackgroundColor = hoverBackgroundColor;
                titleBar.ButtonPressedBackgroundColor = backgroundColor;
                titleBar.ButtonInactiveBackgroundColor = backgroundColor;

                // Title bar button foreground colors. Alpha must be 255.
                titleBar.ButtonForegroundColor = foregroundColor;
                titleBar.ButtonHoverForegroundColor = foregroundColor;
                titleBar.ButtonPressedForegroundColor = foregroundColor;
                titleBar.ButtonInactiveForegroundColor = foregroundColor;
            }
        }

        public Version GetVersion()
        {
            var packageVersion = global::Windows.ApplicationModel.Package.Current.Id.Version;
            var version = new Version(packageVersion.Major, packageVersion.Minor, packageVersion.Build, packageVersion.Revision);
            return version;
        }
    }
}
