using System;

namespace XForma
{
    /// <summary>
    /// The main window's title bar, on the platforms that have one.
    /// </summary>
    public class TitleBar
    {
        private Application _application;

        public TitleBar(
            Application application)
        {
            if (null == application)
            {
                throw new ArgumentNullException(nameof(application));
            }

            this._application = application;
        }

        public Color? AccentColor
        {
            get
            {
                return this._application.Renderer.TitleBarAccentColor;
            }

            set
            {
                this._application.Renderer.TitleBarAccentColor = value;
                this._application.OnPageInsetsChanged();
            }
        }

        public Color? ForegroundColor
        {
            get
            {
                return this._application.Renderer.TitleBarForegroundColor;
            }

            set
            {
                this._application.Renderer.TitleBarForegroundColor = value;
            }
        }
    }
}
