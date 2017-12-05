using System;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using XForms.Windows;

namespace XForms.Test.UniversalWindows
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : UniversalWindowsApplication
    {
        private TestApp _app;

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
        }

        protected override void OnStart()
        {
            base.OnStart();

            if (null == this._app)
            {
                var platform = new UniversalWindowsPlatform(this);
                this._app = new TestApp(platform);
            }

            this._app.Start();
        }
    }
}
