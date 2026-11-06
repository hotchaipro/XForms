using System;
using System.Linq;
using BackgroundTasks;
using Foundation;
using XForma.Platform;

namespace XForma.iOS
{
    /// <summary>
    /// Wakes the app in the background to refresh (see
    /// <see cref="Application.BackgroundRefreshInterval"/>): a background app
    /// refresh task, <c>&lt;bundle identifier&gt;.refresh</c>, scheduled as the
    /// app goes into the background and again each time it runs. The app's
    /// Info.plist lists the identifier in <c>BGTaskSchedulerPermittedIdentifiers</c>
    /// and <c>fetch</c> in <c>UIBackgroundModes</c>; without them there's no
    /// refresh.
    /// </summary>
    internal static class BackgroundRefresh
    {
        private static bool _isRegistered;

        private static string Identifier
        {
            get
            {
                return NSBundle.MainBundle.BundleIdentifier + ".refresh";
            }
        }

        /// <summary>
        /// Registers the task, before the app finishes launching (iOS asks
        /// it to), if the app's Info.plist permits it.
        /// </summary>
        public static void Register()
        {
            if (_isRegistered)
            {
                return;
            }

            var permitted = NSBundle.MainBundle.ObjectForInfoDictionary("BGTaskSchedulerPermittedIdentifiers") as NSArray;
            string identifier = Identifier;
            if ((null == permitted) || !Enumerable.Range(0, (int)permitted.Count).Any(i => permitted.GetItem<NSString>((nuint)i)?.ToString() == identifier))
            {
                return;
            }

            _isRegistered = BGTaskScheduler.Shared.Register(identifier, CoreFoundation.DispatchQueue.MainQueue, Run);
        }

        /// <summary>
        /// Schedules the next refresh for the app's interval (or cancels it,
        /// without one).
        /// </summary>
        public static void Schedule(
            Application application)
        {
            if (!_isRegistered || (null == application))
            {
                return;
            }

            var interval = ApplicationHost.For(application).BackgroundRefreshInterval;
            if (!interval.HasValue)
            {
                BGTaskScheduler.Shared.Cancel(Identifier);
                return;
            }

            var request = new BGAppRefreshTaskRequest(Identifier)
            {
                EarliestBeginDate = NSDate.FromTimeIntervalSinceNow(Math.Max(0, interval.Value.TotalSeconds)),
            };
            BGTaskScheduler.Shared.Submit(request, out _);
        }

        // The system woke the app: the next refresh is scheduled, then the app
        // refreshes, until it's done or its time is up
        private static async void Run(
            BGTask task)
        {
            var application = Application.Current ?? IosApplication.PendingApplication;
            if (null == application)
            {
                task.SetTaskCompleted(false);
                return;
            }

            bool isDone = false;
            task.ExpirationHandler = () =>
            {
                if (!isDone)
                {
                    isDone = true;
                    task.SetTaskCompleted(false);
                }
            };

            bool succeeded = true;
            try
            {
                AppBadge.Watch(application);
                Schedule(application);
                await ApplicationHost.For(application).RefreshInBackgroundAsync();
            }
            catch (Exception ex)
            {
                succeeded = false;
                FrameworkLog.Write(LogLevel.Warning, "The background refresh failed.", ex);
            }

            if (!isDone)
            {
                isDone = true;
                task.SetTaskCompleted(succeeded);
            }
        }
    }
}
