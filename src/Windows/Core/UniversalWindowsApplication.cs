using System;
using Windows.ApplicationModel.Activation;
using global::Windows.UI.ViewManagement;
using global::Windows.UI.Xaml;
using NativeApplication = global::Windows.UI.Xaml.Application;
using NativeFrame = global::Windows.UI.Xaml.Controls.Frame;

namespace XForms.Windows
{
    public class UniversalWindowsApplication : NativeApplication
    {
        public static new UniversalWindowsApplication Current;

        protected UniversalWindowsApplication()
        {
            Current = this;
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override void OnLaunched(
            LaunchActivatedEventArgs e)
        {
//#if DEBUG
//            if (System.Diagnostics.Debugger.IsAttached)
//            {
//                this.DebugSettings.EnableFrameRateCounter = true;
//            }
//#endif

            NativeFrame rootFrame = Window.Current.Content as NativeFrame;

            if (rootFrame == null)
            {
                this.OnStart();
            }

            // Set screen bounds mode on mobile devices
            if (global::Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Mobile")
            {
                var appView = ApplicationView.GetForCurrentView();
                appView.VisibleBoundsChanged += AppView_VisibleBoundsChanged;
                appView.SetDesiredBoundsMode(ApplicationViewBoundsMode.UseCoreWindow);
                this.UpdateRootFrameBounds();
            }

            if (e.PrelaunchActivated == false)
            {
                // Ensure the current window is active
                Window.Current.Activate();
            }
        }

        private void AppView_VisibleBoundsChanged(
            ApplicationView sender,
            object args)
        {
            this.UpdateRootFrameBounds();
        }

        private void UpdateRootFrameBounds()
        {
            var appView = ApplicationView.GetForCurrentView();
            var visibleBounds = appView.VisibleBounds;

            var windowBounds = Window.Current.Bounds;

            InputPane inputPane = InputPane.GetForCurrentView();
            var keyboardBounds = inputPane.OccludedRect;

            var rootFrame = Window.Current.Content as NativeFrame;
            rootFrame.Margin = new global::Windows.UI.Xaml.Thickness(
                visibleBounds.X - windowBounds.X,
                visibleBounds.Y - windowBounds.Y,
                0,
                0);
            rootFrame.Width = visibleBounds.Width;
            rootFrame.Height = visibleBounds.Height - keyboardBounds.Height;
        }

        protected virtual void OnStart()
        {
            ResourceDictionary resources = new ResourceDictionary()
            {
                Source = new Uri("ms-appx:///XForms.Core.UniversalWindows/Resources.xbf"),
            };

            global::Windows.UI.Xaml.Application.Current.Resources.MergedDictionaries.Add(resources);
        }
    }
}
