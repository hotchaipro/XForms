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

        /// <summary>
        /// Raised when the configuration changes in place, such as dark mode.
        /// The activity must declare <c>ConfigChanges.UiMode</c>, or Android
        /// recreates it instead.
        /// </summary>
        public event EventHandler ConfigurationChanged;

        private readonly SystemBarsBackground _systemBarsBackground = new SystemBarsBackground();
        private global::Android.Views.View _rootView;
        private bool _extendsIntoSafeArea;

        // The system bars and cutout, and the soft keyboard, in pixels
        private global::Android.Graphics.Rect _barInsets = new global::Android.Graphics.Rect();
        private int _keyboardInset;

        /// <summary>
        /// Raised when the system bars, cutout or soft keyboard cover a
        /// different part of the window, with the insets in DIPs.
        /// </summary>
        internal event Action<Thickness> SafeAreaInsetsChanged;

        /// <summary>
        /// Whether the current page draws under the system bars and cutout.
        /// The soft keyboard still pushes it up.
        /// </summary>
        internal bool ExtendsIntoSafeArea
        {
            get
            {
                return this._extendsIntoSafeArea;
            }

            set
            {
                if (value != this._extendsIntoSafeArea)
                {
                    this._extendsIntoSafeArea = value;
                    this.ApplyInsets();
                }
            }
        }
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

        private readonly Dictionary<int, System.Threading.Tasks.TaskCompletionSource<(Result, Intent)>> _pendingResults = new Dictionary<int, System.Threading.Tasks.TaskCompletionSource<(Result, Intent)>>();
        private int _nextRequestCode = 0x5800;

        /// <summary>
        /// Starts an activity (such as the document picker) and completes with
        /// its result.
        /// </summary>
        internal System.Threading.Tasks.Task<(Result Result, Intent Data)> StartActivityForResultAsync(
            Intent intent)
        {
            int requestCode = this._nextRequestCode++;
            var completion = new System.Threading.Tasks.TaskCompletionSource<(Result, Intent)>();
            this._pendingResults[requestCode] = completion;
            this.StartActivityForResult(intent, requestCode);
            return completion.Task;
        }

        protected override void OnActivityResult(
            int requestCode,
            [GeneratedEnum] Result resultCode,
            Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            if (this._pendingResults.Remove(requestCode, out var completion))
            {
                completion.SetResult((resultCode, data));
            }
        }

        private BackCallback _backCallback;

        /// <summary>
        /// Handles the Back key and gesture with the action, or leaves them to
        /// the system (null).
        /// </summary>
        internal void SetBackHandler(
            Action onBack)
        {
            if (global::Android.OS.Build.VERSION.SdkInt < global::Android.OS.BuildVersionCodes.Tiramisu)
            {
                this._legacyBackHandler = onBack;
                return;
            }

            if ((null != onBack) && (null == this._backCallback))
            {
                this._backCallback = new BackCallback(onBack);
                this.OnBackInvokedDispatcher.RegisterOnBackInvokedCallback(0, this._backCallback);
            }
            else if (null != onBack)
            {
                this._backCallback.OnBack = onBack;
            }
            else if (null != this._backCallback)
            {
                this.OnBackInvokedDispatcher.UnregisterOnBackInvokedCallback(this._backCallback);
                this._backCallback = null;
            }
        }

        private Action _legacyBackHandler;

        // Before Android 13, Back arrives here
        [Obsolete]
        public override void OnBackPressed()
        {
            if (null != this._legacyBackHandler)
            {
                this._legacyBackHandler();
                return;
            }

            base.OnBackPressed();
        }

        private sealed class BackCallback : Java.Lang.Object, global::Android.Window.IOnBackInvokedCallback
        {
            public BackCallback(
                Action onBack)
            {
                this.OnBack = onBack;
            }

            public Action OnBack { get; set; }

            public void OnBackInvoked()
            {
                this.OnBack?.Invoke();
            }
        }

        public override void OnConfigurationChanged(
            global::Android.Content.Res.Configuration newConfig)
        {
            base.OnConfigurationChanged(newConfig);

            this.ConfigurationChanged?.Invoke(this, EventArgs.Empty);
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

            this._rootView = view;
            view.SetOnApplyWindowInsetsListener(new SystemInsetsListener(this));
            view.RequestApplyInsets();
        }

        private void OnInsets(
            global::Android.Graphics.Rect barInsets,
            int keyboardInset)
        {
            this._barInsets = barInsets;
            this._keyboardInset = keyboardInset;
            this.ApplyInsets();

            this._systemBarsBackground.StatusBarHeight = barInsets.Top;
            this._systemBarsBackground.InvalidateSelf();

            float scale = this.Resources.DisplayMetrics.Density;
            this.SafeAreaInsetsChanged?.Invoke(new Thickness(
                barInsets.Left / scale,
                barInsets.Top / scale,
                barInsets.Right / scale,
                Math.Max(barInsets.Bottom, keyboardInset) / scale));
        }

        // Pads the root view clear of the bars (unless the page extends under
        // them) and the keyboard
        private void ApplyInsets()
        {
            var bars = this._extendsIntoSafeArea ? new global::Android.Graphics.Rect() : this._barInsets;
            this._rootView?.SetPadding(bars.Left, bars.Top, bars.Right, Math.Max(bars.Bottom, this._keyboardInset));
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
            private readonly AndroidApplicationActivity _activity;

            public SystemInsetsListener(
                AndroidApplicationActivity activity)
            {
                this._activity = activity;
            }

            public WindowInsets OnApplyWindowInsets(
                global::Android.Views.View view,
                WindowInsets insets)
            {
                int left, top, right, bottom, keyboard = 0;

                if (OperatingSystem.IsAndroidVersionAtLeast(30))
                {
                    var systemInsets = insets.GetInsets(WindowInsets.Type.SystemBars() | WindowInsets.Type.DisplayCutout());
                    left = systemInsets.Left;
                    top = systemInsets.Top;
                    right = systemInsets.Right;
                    bottom = systemInsets.Bottom;
                    keyboard = insets.GetInsets(WindowInsets.Type.Ime()).Bottom;
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

                this._activity.OnInsets(new global::Android.Graphics.Rect(left, top, right, bottom), keyboard);
                return insets;
            }
        }
    }
}
