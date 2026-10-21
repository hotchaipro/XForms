using System;
using System.Threading.Tasks;
using Foundation;
using Security;
using UIKit;

namespace XForms.iOS
{
    /// <summary>
    /// iOS's services for the application.
    /// </summary>
    internal static class IosServices
    {
        public static PlatformServices Create(
            IosApplication application)
        {
            return new PlatformServices()
            {
                Clipboard = new Pasteboard(),
                Launcher = new UrlLauncher(),
                Preferences = new UserDefaultsPreferences(),
                SecureStorage = new KeychainStorage(),
                Share = new ActivityShare(application),
                HapticFeedback = new GeneratorHapticFeedback(),
                Connectivity = new PathConnectivity(),
                FilePicker = new DocumentPicker(application),
            };
        }
    }

    internal sealed class Pasteboard : IClipboard
    {
        public bool HasText => UIPasteboard.General.HasStrings;

        public Task<string> GetTextAsync()
        {
            return Task.FromResult(UIPasteboard.General.String);
        }

        public Task SetTextAsync(
            string text)
        {
            UIPasteboard.General.String = text ?? string.Empty;
            return Task.CompletedTask;
        }
    }

    internal sealed class UrlLauncher : ILauncher
    {
        public async Task<bool> OpenAsync(
            Uri uri)
        {
            if (null == uri)
            {
                throw new ArgumentNullException(nameof(uri));
            }

            return await UIApplication.SharedApplication.OpenUrlAsync(new NSUrl(uri.AbsoluteUri), new UIApplicationOpenUrlOptions());
        }
    }

    internal sealed class UserDefaultsPreferences : IPreferences
    {
        private static NSUserDefaults Defaults => NSUserDefaults.StandardUserDefaults;

        public bool ContainsKey(string key) => null != Defaults.ValueForKey(new NSString(key));

        public string Get(
            string key,
            string defaultValue = null)
        {
            return Defaults.StringForKey(key) ?? defaultValue;
        }

        public void Set(
            string key,
            string value)
        {
            if (null == value)
            {
                Defaults.RemoveObject(key);
            }
            else
            {
                Defaults.SetString(value, key);
            }
        }

        public void Remove(string key) => Defaults.RemoveObject(key);

        public void Clear()
        {
            string domain = NSBundle.MainBundle.BundleIdentifier;
            if (null != domain)
            {
                Defaults.RemovePersistentDomain(domain);
            }
        }
    }

    /// <summary>
    /// Secrets in the Keychain, as generic passwords for the app.
    /// </summary>
    internal sealed class KeychainStorage : ISecureStorage
    {
        private static string Service => (NSBundle.MainBundle.BundleIdentifier ?? "XForms") + ".xforms";

        private static SecRecord Query(string key) => new SecRecord(SecKind.GenericPassword)
        {
            Service = Service,
            Account = key,
        };

        public Task<string> GetAsync(
            string key)
        {
            var record = SecKeyChain.QueryAsRecord(Query(key), out var status);
            string value = ((status == SecStatusCode.Success) && (null != record?.ValueData)) ? record.ValueData.ToString(NSStringEncoding.UTF8) : null;
            return Task.FromResult(value);
        }

        public Task SetAsync(
            string key,
            string value)
        {
            SecKeyChain.Remove(Query(key));
            if (null != value)
            {
                var record = Query(key);
                record.ValueData = NSData.FromString(value, NSStringEncoding.UTF8);
                record.Accessible = SecAccessible.AfterFirstUnlock;
                SecKeyChain.Add(record);
            }

            return Task.CompletedTask;
        }

        public bool Remove(
            string key)
        {
            return SecKeyChain.Remove(Query(key)) == SecStatusCode.Success;
        }

        public void RemoveAll()
        {
            SecKeyChain.Remove(new SecRecord(SecKind.GenericPassword) { Service = Service });
        }
    }

    internal sealed class ActivityShare : IShare
    {
        private readonly IosApplication _application;

        public ActivityShare(
            IosApplication application)
        {
            this._application = application;
        }

        public Task RequestAsync(
            ShareRequest request)
        {
            if (null == request)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var items = new System.Collections.Generic.List<NSObject>();
            if (!string.IsNullOrEmpty(request.Text))
            {
                items.Add(new NSString(request.Text));
            }

            if (null != request.Uri)
            {
                items.Add(new NSUrl(request.Uri.AbsoluteUri));
            }

            var controller = new UIActivityViewController(items.ToArray(), null);
            var presenter = this._application.RootViewController;
            if (null != presenter)
            {
                // On iPad the sheet is a popover, anchored to the window
                if (null != controller.PopoverPresentationController)
                {
                    controller.PopoverPresentationController.SourceView = presenter.View;
                    controller.PopoverPresentationController.SourceRect = new CoreGraphics.CGRect(presenter.View.Bounds.Width / 2, presenter.View.Bounds.Height / 2, 0, 0);
                }

                presenter.PresentViewController(controller, true, null);
            }

            return Task.CompletedTask;
        }
    }

    internal sealed class GeneratorHapticFeedback : IHapticFeedback
    {
        public bool IsSupported => true;

        public void Perform(
            HapticFeedbackType type)
        {
            var style = (type == HapticFeedbackType.LongPress) ? UIImpactFeedbackStyle.Heavy : UIImpactFeedbackStyle.Light;
            using (var generator = new UIImpactFeedbackGenerator(style))
            {
                generator.Prepare();
                generator.ImpactOccurred();
            }
        }
    }
}
