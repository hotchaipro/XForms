using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using static Android.App.Application;

namespace XForms.Android
{
    public class AndroidApplicationActivity : global::Android.App.Activity
    {
        public event EventHandler Suspending;
        public event EventHandler Resuming;

        private readonly SystemBarsBackground _systemBarsBackground = new SystemBarsBackground();
        private bool _isStatusBarLight;
        private bool _isNavigationBarLight;

        public AndroidApplicationActivity()
        {
        }

        protected override void OnPause()
        {
            base.OnPause();

            this.Suspending?.Invoke(this, EventArgs.Empty);
        }

        protected override void OnResume()
        {
            base.OnResume();

            this.Resuming?.Invoke(this, EventArgs.Empty);
        }

        protected override void OnCreate(
            Bundle savedInstanceState)
        {
            // XForms apps draw their own chrome
            this.RequestWindowFeature(WindowFeatures.NoTitle);

            base.OnCreate(savedInstanceState);
        }

        /// <summary>
        /// Shows the root XForms view, keeping it clear of the system bars and the
        /// soft keyboard. Android 15 and later always lay windows out edge to edge.
        /// </summary>
        internal void SetRootView(
            global::Android.Views.View view)
        {
            this.SetContentView(view);

            // The system bars are transparent, so the root view paints behind them
            view.Background = this._systemBarsBackground;

            view.SetOnApplyWindowInsetsListener(new SystemInsetsListener(this._systemBarsBackground));
            view.RequestApplyInsets();
        }

        /// <summary>
        /// Colors the area behind the status bar, with icons that contrast with it.
        /// </summary>
        internal void SetStatusBarColors(
            Color background,
            bool isLightBackground)
        {
            this._systemBarsBackground.StatusBarColor = background.ToAndroidColor();
            this._systemBarsBackground.InvalidateSelf();
            this._isStatusBarLight = isLightBackground;

            this.UpdateSystemBarAppearance();
        }

        /// <summary>
        /// Colors the area behind the navigation bar, with icons that contrast with it.
        /// </summary>
        internal void SetNavigationBarColors(
            Color background,
            bool isLightBackground)
        {
            this._systemBarsBackground.NavigationBarColor = background.ToAndroidColor();
            this._systemBarsBackground.InvalidateSelf();
            this._isNavigationBarLight = isLightBackground;

            this.UpdateSystemBarAppearance();
        }

        private void UpdateSystemBarAppearance()
        {
            if (OperatingSystem.IsAndroidVersionAtLeast(30))
            {
                var lightStatusBars = WindowInsetsControllerAppearance.LightStatusBars;
                var lightNavigationBars = WindowInsetsControllerAppearance.LightNavigationBars;

                var appearance = (this._isStatusBarLight ? lightStatusBars : 0)
                    | (this._isNavigationBarLight ? lightNavigationBars : 0);

                this.Window?.InsetsController?.SetSystemBarsAppearance(
                    (int)appearance,
                    (int)(lightStatusBars | lightNavigationBars));
            }
        }

        /// <summary>
        /// Paints the status bar color in the band at the top of the root view,
        /// and the navigation bar color everywhere else the content leaves uncovered.
        /// </summary>
        private sealed class SystemBarsBackground : global::Android.Graphics.Drawables.Drawable
        {
            private readonly global::Android.Graphics.Paint _paint = new global::Android.Graphics.Paint();

            public global::Android.Graphics.Color StatusBarColor { get; set; } = global::Android.Graphics.Color.Transparent;

            public global::Android.Graphics.Color NavigationBarColor { get; set; } = global::Android.Graphics.Color.Transparent;

            public int StatusBarHeight { get; set; }

            public override int Opacity
            {
                get
                {
                    return (int)global::Android.Graphics.Format.Translucent;
                }
            }

            public override void Draw(
                global::Android.Graphics.Canvas canvas)
            {
                var bounds = this.Bounds;

                this._paint.Color = this.NavigationBarColor;
                canvas.DrawRect(bounds, this._paint);

                this._paint.Color = this.StatusBarColor;
                canvas.DrawRect(bounds.Left, bounds.Top, bounds.Right, bounds.Top + this.StatusBarHeight, this._paint);
            }

            public override void SetAlpha(
                int alpha)
            {
            }

            public override void SetColorFilter(
                global::Android.Graphics.ColorFilter colorFilter)
            {
            }
        }

        private sealed class SystemInsetsListener : Java.Lang.Object, global::Android.Views.View.IOnApplyWindowInsetsListener
        {
            private readonly SystemBarsBackground _background;

            public SystemInsetsListener(
                SystemBarsBackground background)
            {
                this._background = background;
            }

            public WindowInsets OnApplyWindowInsets(
                global::Android.Views.View view,
                WindowInsets insets)
            {
                int left, top, right, bottom;

                if (OperatingSystem.IsAndroidVersionAtLeast(30))
                {
                    var systemInsets = insets.GetInsets(WindowInsets.Type.SystemBars() | WindowInsets.Type.Ime() | WindowInsets.Type.DisplayCutout());
                    left = systemInsets.Left;
                    top = systemInsets.Top;
                    right = systemInsets.Right;
                    bottom = systemInsets.Bottom;
                }
                else
                {
#pragma warning disable CA1422 // Replaced by GetInsets on Android 11
                    left = insets.SystemWindowInsetLeft;
                    top = insets.SystemWindowInsetTop;
                    right = insets.SystemWindowInsetRight;
                    bottom = insets.SystemWindowInsetBottom;
#pragma warning restore CA1422
                }

                view.SetPadding(left, top, right, bottom);

                this._background.StatusBarHeight = top;
                this._background.InvalidateSelf();

                return insets;
            }
        }
    }
}
