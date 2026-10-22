using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using XForms.Platform;

namespace XForms.Android
{
    /// <summary>
    /// Runs an XForms app in an activity, of any kind: the app's own, with
    /// whatever base class it needs. Gives native code the running app's
    /// activity.
    /// </summary>
    /// <example>
    /// <code>
    /// [Activity(MainLauncher = true, ConfigurationChanges = ...)]
    /// public class MainActivity : Activity
    /// {
    ///     protected override void OnCreate(Bundle savedInstanceState)
    ///     {
    ///         base.OnCreate(savedInstanceState);
    ///         AndroidApplication.Run(this, savedInstanceState, new App());
    ///     }
    /// }
    /// </code>
    /// </example>
    public sealed class AndroidApplication
    {
        private readonly XForms.Application _application;
        private readonly HostView _hostView;
        private readonly LifecycleCallbacks _lifecycleCallbacks;
        private Activity _activity;
        private bool _extendsIntoSafeArea;
        private bool _isStatusBarLight;
        private bool _isNavigationBarLight;
        private Action _backHandler;
        private BackCallback _backCallback;
        private BackKeyWindowCallback _backKeyCallback;

        // The system bars and cutout, and the soft keyboard, in pixels
        private global::Android.Graphics.Rect _barInsets = new global::Android.Graphics.Rect();
        private int _keyboardInset;

        private AndroidApplication(
            Activity activity,
            XForms.Application application)
        {
            this._application = application;

            // NOTE: The views use the application context, so they outlive the
            // activity when Android re-creates it
            this._hostView = new HostView(activity.ApplicationContext, this);
            this._lifecycleCallbacks = new LifecycleCallbacks(this);
            activity.Application.RegisterActivityLifecycleCallbacks(this._lifecycleCallbacks);
        }

        /// <summary>
        /// The running app's host, or null before <see cref="Run"/> (and after
        /// its activity finishes).
        /// </summary>
        public static AndroidApplication Current
        {
            get;
            private set;
        }

        /// <summary>
        /// The activity the app runs in.
        /// </summary>
        public Activity Activity
        {
            get
            {
                return this._activity;
            }
        }

        /// <summary>
        /// Runs the app in the activity: call it from the activity's
        /// <c>OnCreate</c>, after the base class's, with the state
        /// <c>OnCreate</c> was given. The app fills the activity's content.
        /// </summary>
        /// <remarks>
        /// <para>
        /// When Android re-creates the activity (for a configuration change it
        /// doesn't declare), the new activity calls this again: the running app
        /// moves to it, keeping its state, and the new app object is left
        /// unused. Declaring the changes XForms handles in place (dark mode,
        /// text size, orientation and screen size) makes that rare. When the
        /// activity finishes, the app stops, and the next launch runs a new one.
        /// </para>
        /// <para>
        /// The app's back stack is saved with the activity's state, so when
        /// Android ends the app in the background and the user returns to it,
        /// it starts on the same pages. A link the activity is started with
        /// (its intent's data, such as <c>https://example.com/item/42</c>)
        /// opens the page for the link's path (<c>/item/42</c>).
        /// </para>
        /// </remarks>
        public static AndroidApplication Run(
            Activity activity,
            Bundle savedInstanceState,
            XForms.Application application)
        {
            ArgumentNullException.ThrowIfNull(activity);
            ArgumentNullException.ThrowIfNull(application);

            var current = Current;
            if (null != current)
            {
                current.AttachTo(activity);

                // A new activity for a link, while the app runs
                string link = (null == savedInstanceState) ? GetLinkPath(activity.Intent) : null;
                if (null != link)
                {
                    _ = ApplicationHost.For(current._application).OpenPathAsync(link);
                }

                return current;
            }

            var host = new AndroidApplication(activity, application);
            Current = host;
            host.AttachTo(activity);

            // The stack saved when Android ended the app, or a link's page
            var saved = savedInstanceState?.GetStringArray(NavigationStackKey);
            var link2 = GetLinkPath(activity.Intent);
            var startStack = ((null != saved) && (0 < saved.Length)) ? saved : ((null != link2) ? new[] { link2 } : null);
            ApplicationHost.For(application).Start(new AndroidPlatform(host), startStack);
            return host;
        }

        // Where the back stack is kept in the activity's saved state
        private const string NavigationStackKey = "xforms.navigation";

        // A link's path and query (/item/42?x=1), from the intent's data, or null
        private static string GetLinkPath(
            Intent intent)
        {
            var uri = intent?.Data;
            if (null == uri)
            {
                return null;
            }

            string path = uri.EncodedPath;
            if (string.IsNullOrEmpty(path))
            {
                path = "/";
            }

            string query = uri.EncodedQuery;
            return string.IsNullOrEmpty(query) ? path : path + "?" + query;
        }

        private void OnActivitySaveInstanceState(
            Activity activity,
            Bundle outState)
        {
            if (activity != this._activity)
            {
                return;
            }

            var stack = new List<string>(ApplicationHost.For(this._application).NavigationStack);
            stack.RemoveAll((path) => null == path);
            outState.PutStringArray(NavigationStackKey, stack.ToArray());
        }

        // Shows the app in the activity (a new one, when Android re-creates it)
        private void AttachTo(
            Activity activity)
        {
            if (activity == this._activity)
            {
                return;
            }

            var previous = this._activity;
            this._activity = activity;

            // XForms apps draw their own chrome
            try
            {
                activity.RequestWindowFeature(WindowFeatures.NoTitle);
            }
            catch (global::Android.Util.AndroidRuntimeException)
            {
                // The activity's theme decides (use one without an action bar)
            }

            if (this._hostView.Parent is ViewGroup parent)
            {
                parent.RemoveView(this._hostView);
            }

            activity.SetContentView(this._hostView);
            if (null != this._title)
            {
                activity.Title = this._title;
            }
            this._hostView.RequestApplyInsets();

            if (null != previous)
            {
                this.UnregisterBack(previous);
            }

            this.SetBackHandler(this._backHandler);
            this.UpdateSystemBarAppearance();
        }

        // ── What the platform uses ─────────────────────────────────────────

        internal event EventHandler Suspending;

        internal event EventHandler Resuming;

        /// <summary>
        /// Raised when the configuration changes in place, such as dark mode.
        /// </summary>
        internal event EventHandler ConfigurationChanged;

        /// <summary>
        /// Raised when the system bars, cutout or soft keyboard cover a
        /// different part of the window, with the insets in DIPs.
        /// </summary>
        internal event Action<Thickness> SafeAreaInsetsChanged;

        internal Context Context
        {
            get
            {
                return this._hostView.Context;
            }
        }

        // The activity's resources and services follow its configuration
        internal Context ActivityOrContext
        {
            get
            {
                return (Context)this._activity ?? this.Context;
            }
        }

        private string _title;

        /// <summary>
        /// The activity's title, shown in the recent apps list.
        /// </summary>
        internal string Title
        {
            set
            {
                this._title = value;
                this._activity?.Title = value;
            }
        }

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

        /// <summary>
        /// Shows the root XForms view, keeping it clear of the system bars and
        /// the soft keyboard. Android 15 and later always lay windows out edge
        /// to edge.
        /// </summary>
        internal void SetRootView(
            global::Android.Views.View view)
        {
            this._hostView.RemoveAllViews();
            this._hostView.AddView(view, new FrameLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent));
        }

        // Starts an activity from the app's activity (or, without one, in a
        // new task)
        internal void StartActivity(
            Intent intent)
        {
            var activity = this._activity;
            if (null == activity)
            {
                intent.AddFlags(ActivityFlags.NewTask);
                this.Context.StartActivity(intent);
                return;
            }

            activity.StartActivity(intent);
        }

        internal void Finish()
        {
            this._activity?.Finish();
        }

        private static readonly Handler MainThread = new Handler(Looper.MainLooper);

        internal void RunOnUiThread(
            Action action)
        {
            MainThread.Post(action);
        }

        /// <summary>
        /// Handles the Back key and gesture with the action, or leaves them to
        /// the system (null).
        /// </summary>
        internal void SetBackHandler(
            Action onBack)
        {
            this._backHandler = onBack;

            var activity = this._activity;
            if (null == activity)
            {
                return;
            }

            if (OperatingSystem.IsAndroidVersionAtLeast(33))
            {
                if ((null != onBack) && (null == this._backCallback))
                {
                    this._backCallback = new BackCallback(onBack);
                    activity.OnBackInvokedDispatcher.RegisterOnBackInvokedCallback(0, this._backCallback);
                }
                else if (null != onBack)
                {
                    this._backCallback.OnBack = onBack;
                }
                else if (null != this._backCallback)
                {
                    activity.OnBackInvokedDispatcher.UnregisterOnBackInvokedCallback(this._backCallback);
                    this._backCallback = null;
                }
            }
            else if (null == this._backKeyCallback)
            {
                // Before Android 13, Back is a key event, which the window's
                // callback (the activity) sees first
                this._backKeyCallback = new BackKeyWindowCallback(activity.Window.Callback, this);
                activity.Window.Callback = this._backKeyCallback;
            }
        }

        private void UnregisterBack(
            Activity activity)
        {
            if (null != this._backCallback)
            {
                if (OperatingSystem.IsAndroidVersionAtLeast(33))
                {
                    activity.OnBackInvokedDispatcher.UnregisterOnBackInvokedCallback(this._backCallback);
                }

                this._backCallback = null;
            }

            if ((null != this._backKeyCallback) && (activity.Window?.Callback == this._backKeyCallback))
            {
                activity.Window.Callback = this._backKeyCallback.Inner;
            }

            this._backKeyCallback = null;
        }

        // A Back key the app handles (before Android 13)
        private bool OnBackKey(
            KeyEvent e)
        {
            if (null == this._backHandler)
            {
                return false;
            }

            if ((e.Action == KeyEventActions.Up) && !e.IsCanceled)
            {
                this._backHandler();
            }

            return true;
        }

        private void OnInsets(
            global::Android.Graphics.Rect barInsets,
            int keyboardInset)
        {
            this._barInsets = barInsets;
            this._keyboardInset = keyboardInset;
            this.ApplyInsets();

            this._hostView.SystemBars.StatusBarHeight = barInsets.Top;
            this._hostView.SystemBars.InvalidateSelf();

            float scale = this._hostView.Resources.DisplayMetrics.Density;
            this.SafeAreaInsetsChanged?.Invoke(new Thickness(
                barInsets.Left / scale,
                barInsets.Top / scale,
                barInsets.Right / scale,
                Math.Max(barInsets.Bottom, keyboardInset) / scale));
        }

        // Pads the app clear of the bars (unless the page extends under them)
        // and the keyboard
        private void ApplyInsets()
        {
            var bars = this._extendsIntoSafeArea ? new global::Android.Graphics.Rect() : this._barInsets;
            this._hostView.SetPadding(bars.Left, bars.Top, bars.Right, Math.Max(bars.Bottom, this._keyboardInset));
        }

        /// <summary>
        /// Colors the area behind the status bar, with icons that contrast with it.
        /// </summary>
        internal void SetStatusBarColors(
            Color background,
            bool isLightBackground)
        {
            this._hostView.SystemBars.StatusBarColor = background.ToAndroidColor();
            this._hostView.SystemBars.InvalidateSelf();
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
            this._hostView.SystemBars.NavigationBarColor = background.ToAndroidColor();
            this._hostView.SystemBars.InvalidateSelf();
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

                this._activity?.Window?.InsetsController?.SetSystemBarsAppearance(
                    (int)appearance,
                    (int)(lightStatusBars | lightNavigationBars));
            }
        }

        // The host activity paused, resumed, or was destroyed
        private void OnActivityPaused(
            Activity activity)
        {
            if (activity == this._activity)
            {
                this.Suspending?.Invoke(this, EventArgs.Empty);
            }
        }

        private void OnActivityResumed(
            Activity activity)
        {
            if (activity == this._activity)
            {
                this.Resuming?.Invoke(this, EventArgs.Empty);
            }
        }

        private void OnActivityDestroyed(
            Activity activity)
        {
            if (activity != this._activity)
            {
                return;
            }

            this.UnregisterBack(activity);
            this._activity = null;

            // Re-created, the new activity takes the app; finished, the app
            // stops, and the next launch runs a new one
            if (activity.IsFinishing && !activity.IsChangingConfigurations)
            {
                activity.Application.UnregisterActivityLifecycleCallbacks(this._lifecycleCallbacks);
                if (this._hostView.Parent is ViewGroup parent)
                {
                    parent.RemoveView(this._hostView);
                }

                if (Current == this)
                {
                    Current = null;
                }
            }
        }

        /// <summary>
        /// The activity's content: the app's root view, padded clear of the
        /// system bars (which it paints behind) and the keyboard.
        /// </summary>
        private sealed class HostView : FrameLayout
        {
            private readonly AndroidApplication _owner;

            public HostView(
                Context context,
                AndroidApplication owner)
                : base(context)
            {
                this._owner = owner;
                this.SystemBars = new SystemBarsBackground();

                // The system bars are transparent, so the app paints behind them
                this.Background = this.SystemBars;
                this.SetOnApplyWindowInsetsListener(new SystemInsetsListener(owner));
            }

            public SystemBarsBackground SystemBars { get; }

            protected override void OnConfigurationChanged(
                global::Android.Content.Res.Configuration newConfig)
            {
                base.OnConfigurationChanged(newConfig);

                this._owner.ConfigurationChanged?.Invoke(this._owner, EventArgs.Empty);
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
            private readonly AndroidApplication _owner;

            public SystemInsetsListener(
                AndroidApplication owner)
            {
                this._owner = owner;
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

                this._owner.OnInsets(new global::Android.Graphics.Rect(left, top, right, bottom), keyboard);
                return insets;
            }
        }

        private sealed class LifecycleCallbacks : Java.Lang.Object, global::Android.App.Application.IActivityLifecycleCallbacks
        {
            private readonly AndroidApplication _owner;

            public LifecycleCallbacks(
                AndroidApplication owner)
            {
                this._owner = owner;
            }

            public void OnActivityPaused(Activity activity) => this._owner.OnActivityPaused(activity);

            public void OnActivityResumed(Activity activity) => this._owner.OnActivityResumed(activity);

            public void OnActivityDestroyed(Activity activity) => this._owner.OnActivityDestroyed(activity);

            public void OnActivityCreated(Activity activity, Bundle savedInstanceState)
            {
            }

            public void OnActivitySaveInstanceState(Activity activity, Bundle outState) => this._owner.OnActivitySaveInstanceState(activity, outState);

            public void OnActivityStarted(Activity activity)
            {
            }

            public void OnActivityStopped(Activity activity)
            {
            }
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

        /// <summary>
        /// The activity's window callback, but with the Back key going to the
        /// app while it can go back (before Android 13, where Back is a key).
        /// </summary>
        private sealed class BackKeyWindowCallback : Java.Lang.Object, global::Android.Views.Window.ICallback
        {
            private readonly AndroidApplication _owner;

            public BackKeyWindowCallback(
                global::Android.Views.Window.ICallback inner,
                AndroidApplication owner)
            {
                this.Inner = inner;
                this._owner = owner;
            }

            public global::Android.Views.Window.ICallback Inner { get; }

            public bool DispatchKeyEvent(KeyEvent e)
            {
                if ((e.KeyCode == Keycode.Back) && this._owner.OnBackKey(e))
                {
                    return true;
                }

                return this.Inner.DispatchKeyEvent(e);
            }

            public bool DispatchGenericMotionEvent(MotionEvent e) => this.Inner.DispatchGenericMotionEvent(e);

            public bool DispatchKeyShortcutEvent(KeyEvent e) => this.Inner.DispatchKeyShortcutEvent(e);

            public bool DispatchPopulateAccessibilityEvent(global::Android.Views.Accessibility.AccessibilityEvent e) => this.Inner.DispatchPopulateAccessibilityEvent(e);

            public bool DispatchTouchEvent(MotionEvent e) => this.Inner.DispatchTouchEvent(e);

            public bool DispatchTrackballEvent(MotionEvent e) => this.Inner.DispatchTrackballEvent(e);

            public void OnActionModeFinished(ActionMode mode) => this.Inner.OnActionModeFinished(mode);

            public void OnActionModeStarted(ActionMode mode) => this.Inner.OnActionModeStarted(mode);

            public void OnAttachedToWindow() => this.Inner.OnAttachedToWindow();

            public void OnContentChanged() => this.Inner.OnContentChanged();

            public bool OnCreatePanelMenu(int featureId, IMenu menu) => this.Inner.OnCreatePanelMenu(featureId, menu);

            public global::Android.Views.View OnCreatePanelView(int featureId) => this.Inner.OnCreatePanelView(featureId);

            public void OnDetachedFromWindow() => this.Inner.OnDetachedFromWindow();

            public bool OnMenuItemSelected(int featureId, IMenuItem item) => this.Inner.OnMenuItemSelected(featureId, item);

            public bool OnMenuOpened(int featureId, IMenu menu) => this.Inner.OnMenuOpened(featureId, menu);

            public void OnPanelClosed(int featureId, IMenu menu) => this.Inner.OnPanelClosed(featureId, menu);

            public bool OnPreparePanel(int featureId, global::Android.Views.View view, IMenu menu) => this.Inner.OnPreparePanel(featureId, view, menu);

            public bool OnSearchRequested() => this.Inner.OnSearchRequested();

            public bool OnSearchRequested(SearchEvent searchEvent) => this.Inner.OnSearchRequested(searchEvent);

            public void OnWindowAttributesChanged(WindowManagerLayoutParams attrs) => this.Inner.OnWindowAttributesChanged(attrs);

            public void OnWindowFocusChanged(bool hasFocus) => this.Inner.OnWindowFocusChanged(hasFocus);

            public ActionMode OnWindowStartingActionMode(ActionMode.ICallback callback) => this.Inner.OnWindowStartingActionMode(callback);

            public ActionMode OnWindowStartingActionMode(ActionMode.ICallback callback, ActionModeType type) => this.Inner.OnWindowStartingActionMode(callback, type);
        }
    }
}
