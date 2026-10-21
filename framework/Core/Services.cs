using System;
using System.Globalization;
using System.Threading.Tasks;

namespace XForms
{
    /// <summary>
    /// The system clipboard's text.
    /// </summary>
    public interface IClipboard
    {
        bool HasText { get; }

        Task<string> GetTextAsync();

        Task SetTextAsync(string text);
    }

    /// <summary>
    /// Opens URIs in the apps that handle them, such as the web browser.
    /// </summary>
    public interface ILauncher
    {
        /// <summary>
        /// Opens the URI. Completes with whether an app opened it.
        /// </summary>
        Task<bool> OpenAsync(Uri uri);
    }

    /// <summary>
    /// Small settings kept between runs, as strings. Not for secrets: see
    /// <see cref="ISecureStorage"/>.
    /// </summary>
    public interface IPreferences
    {
        bool ContainsKey(string key);

        /// <summary>
        /// The value, or the default when it isn't set.
        /// </summary>
        string Get(string key, string defaultValue = null);

        void Set(string key, string value);

        void Remove(string key);

        void Clear();
    }

    /// <summary>
    /// Secrets kept between runs, encrypted by the platform: DPAPI on Windows,
    /// the Android Keystore, and the iOS Keychain.
    /// </summary>
    public interface ISecureStorage
    {
        /// <summary>
        /// The value, or null when it isn't set.
        /// </summary>
        Task<string> GetAsync(string key);

        Task SetAsync(string key, string value);

        bool Remove(string key);

        void RemoveAll();
    }

    /// <summary>
    /// What to share: text, a link, or both.
    /// </summary>
    public sealed class ShareRequest
    {
        public string Title { get; init; }

        public string Text { get; init; }

        public Uri Uri { get; init; }
    }

    /// <summary>
    /// Shares text and links through the system's share sheet.
    /// </summary>
    public interface IShare
    {
        Task RequestAsync(ShareRequest request);
    }

    public enum HapticFeedbackType
    {
        /// <summary>A light tap, as for a key or a toggle.</summary>
        Click,

        /// <summary>A stronger one, as for a long press.</summary>
        LongPress,
    }

    /// <summary>
    /// Haptic feedback, where the device has it.
    /// </summary>
    public interface IHapticFeedback
    {
        bool IsSupported { get; }

        void Perform(HapticFeedbackType type);
    }

    /// <summary>
    /// The platform's services, from <see cref="Application.Services"/>. Each
    /// is an interface, so an app can substitute its own (in tests, say).
    /// </summary>
    /// <remarks>
    /// The API shapes follow .NET MAUI's Essentials, as instances rather than
    /// static classes.
    /// </remarks>
    public sealed class PlatformServices
    {
        public IClipboard Clipboard { get; set; }

        public ILauncher Launcher { get; set; }

        public IPreferences Preferences { get; set; }

        public ISecureStorage SecureStorage { get; set; }

        public IShare Share { get; set; }

        public IHapticFeedback HapticFeedback { get; set; }
    }

    /// <summary>
    /// Typed values in preferences, stored as invariant strings.
    /// </summary>
    public static class PreferencesExtensions
    {
        public static T Get<T>(
            this IPreferences preferences,
            string key,
            T defaultValue)
            where T : IConvertible
        {
            string value = preferences.Get(key);
            if (null == value)
            {
                return defaultValue;
            }

            try
            {
                return (T)Convert.ChangeType(value, typeof(T), CultureInfo.InvariantCulture);
            }
            catch (FormatException)
            {
                return defaultValue;
            }
        }

        public static void Set<T>(
            this IPreferences preferences,
            string key,
            T value)
            where T : IConvertible
        {
            preferences.Set(key, value.ToString(CultureInfo.InvariantCulture));
        }
    }
}
