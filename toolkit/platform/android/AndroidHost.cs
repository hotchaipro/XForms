using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;

namespace XForms.Toolkit
{
    /// <summary>
    /// What the services need of the running app: its context, its activity
    /// (from the provider it registered with), the UI thread, and activities
    /// started for a result.
    /// </summary>
    internal static class AndroidHost
    {
        private static readonly Handler MainThread = new Handler(Looper.MainLooper);
        private static readonly Dictionary<int, TaskCompletionSource<(Result, Intent)>> PendingResults = new Dictionary<int, TaskCompletionSource<(Result, Intent)>>();
        private static int _nextRequestCode = 0x5800;

        public static Context Context
        {
            get
            {
                return global::Android.App.Application.Context;
            }
        }

        public static Func<Activity> ActivityProvider
        {
            get;
            set;
        }

        public static Activity Activity
        {
            get
            {
                return ActivityProvider?.Invoke();
            }
        }

        public static void RunOnUiThread(
            Action action)
        {
            MainThread.Post(action);
        }

        /// <summary>
        /// Starts an activity from the app's activity (or, without one, in a
        /// new task).
        /// </summary>
        public static void StartActivity(
            Intent intent)
        {
            var activity = Activity;
            if (null == activity)
            {
                intent.AddFlags(ActivityFlags.NewTask);
                Context.StartActivity(intent);
                return;
            }

            activity.StartActivity(intent);
        }

        /// <summary>
        /// Starts an activity (such as the document picker) and completes with
        /// its result, through a short-lived activity of the toolkit's own, so
        /// the app's activity needn't forward its results.
        /// </summary>
        public static Task<(Result Result, Intent Data)> StartActivityForResultAsync(
            Intent intent)
        {
            int requestCode = _nextRequestCode++;
            var completion = new TaskCompletionSource<(Result, Intent)>();
            PendingResults[requestCode] = completion;

            var proxy = new Intent(Activity ?? Context, typeof(ResultActivity));
            proxy.PutExtra(ResultActivity.IntentExtra, intent);
            proxy.PutExtra(ResultActivity.RequestCodeExtra, requestCode);
            StartActivity(proxy);
            return completion.Task;
        }

        internal static void OnActivityResult(
            int requestCode,
            Result resultCode,
            Intent data)
        {
            if (PendingResults.Remove(requestCode, out var completion))
            {
                completion.SetResult((resultCode, data));
            }
        }
    }

    /// <summary>
    /// Starts an activity for its result on the toolkit's behalf, then reports
    /// the result and finishes.
    /// </summary>
    [Activity(
        Exported = false,
        ConfigurationChanges = global::Android.Content.PM.ConfigChanges.Orientation | global::Android.Content.PM.ConfigChanges.ScreenSize | global::Android.Content.PM.ConfigChanges.UiMode,
        Theme = "@android:style/Theme.Translucent.NoTitleBar")]
    internal sealed class ResultActivity : Activity
    {
        public const string IntentExtra = "xforms.intent";
        public const string RequestCodeExtra = "xforms.requestCode";

        private bool _isStarted;

        protected override void OnCreate(
            Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            this._isStarted = savedInstanceState?.GetBoolean(nameof(this._isStarted)) ?? false;
            if (!this._isStarted)
            {
                this._isStarted = true;
#pragma warning disable CA1422 // The typed overload needs Android 13
                var intent = (Intent)this.Intent.GetParcelableExtra(IntentExtra);
#pragma warning restore CA1422
                this.StartActivityForResult(intent, 1);
            }
        }

        protected override void OnSaveInstanceState(
            Bundle outState)
        {
            base.OnSaveInstanceState(outState);
            outState.PutBoolean(nameof(this._isStarted), this._isStarted);
        }

        protected override void OnActivityResult(
            int requestCode,
            [GeneratedEnum] Result resultCode,
            Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            AndroidHost.OnActivityResult(this.Intent.GetIntExtra(RequestCodeExtra, -1), resultCode, data);
            this.Finish();
        }
    }
}
