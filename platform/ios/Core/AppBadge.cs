using System;
using System.Runtime.CompilerServices;
using UIKit;
using UserNotifications;
using XForms.Platform;

namespace XForms.iOS
{
    /// <summary>
    /// Shows an app's <see cref="Application.BadgeCount"/> on its icon. The
    /// first count asks the user to allow badges (and only badges).
    /// </summary>
    internal static class AppBadge
    {
        private static readonly ConditionalWeakTable<Application, object> Watched = new ConditionalWeakTable<Application, object>();
        private static bool _isAuthorizationRequested;
        private static int _count;

        /// <summary>
        /// Shows the app's count, now and as it changes.
        /// </summary>
        public static void Watch(
            Application application)
        {
            if (Watched.TryGetValue(application, out _))
            {
                return;
            }

            Watched.Add(application, null);
            var host = ApplicationHost.For(application);
            host.BadgeCountChanged += (sender, e) => Show(host.BadgeCount);
            if (0 != host.BadgeCount)
            {
                Show(host.BadgeCount);
            }
        }

        private static void Show(
            int count)
        {
            _count = count;
            if ((count > 0) && !_isAuthorizationRequested)
            {
                _isAuthorizationRequested = true;
                UNUserNotificationCenter.Current.RequestAuthorization(UNAuthorizationOptions.Badge, (granted, error) =>
                {
                    // Allowed: show the count it asked for (and any since)
                    if (granted)
                    {
                        CoreFoundation.DispatchQueue.MainQueue.DispatchAsync(() => Apply(_count));
                    }
                });
            }

            Apply(count);
        }

        private static void Apply(
            int count)
        {
            if (OperatingSystem.IsIOSVersionAtLeast(16))
            {
                UNUserNotificationCenter.Current.SetBadgeCount(count, null);
            }
            else
            {
                UIApplication.SharedApplication.ApplicationIconBadgeNumber = count;
            }
        }
    }
}
