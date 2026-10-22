using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;
using System.Threading.Tasks;

namespace XForms.Toolkit
{
    /// <summary>
    /// The page's services. A page has no secret store, so secure storage is
    /// local storage too: don't keep real secrets in it on the web.
    /// </summary>
    [SupportedOSPlatform("browser")]
    public static class WebToolkitServices
    {
        private static bool _isRegistered;

        /// <summary>
        /// Loads the services' script into the page and makes the page's
        /// services the app's (<see cref="Clipboard.Default"/> and the
        /// others).
        /// </summary>
        public static async Task InitializeAsync()
        {
            await ToolkitJs.InitializeAsync();
            if (_isRegistered)
            {
                return;
            }

            _isRegistered = true;
            Clipboard.Default = new WebClipboard();
            Launcher.Default = new WebLauncher();
            Preferences.Default = new LocalStoragePreferences("xforms.preferences.");
            SecureStorage.Default = new LocalStorageSecrets("xforms.secure.");
            Share.Default = new WebShare();
            HapticFeedback.Default = new VibrationHapticFeedback();
            Connectivity.Default = new WebConnectivity();
            FilePicker.Default = new WebFilePicker();
        }
    }

    [SupportedOSPlatform("browser")]
    internal sealed class WebClipboard : IClipboard
    {
        // NOTE: The browser only says whether there's text by reading it
        public bool HasText => true;

        public async Task<string> GetTextAsync()
        {
            try
            {
                return await ToolkitJs.ClipboardReadAsync();
            }
            catch (Exception)
            {
                // Not allowed (it needs the user's permission)
                return null;
            }
        }

        public Task SetTextAsync(
            string text)
        {
            return ToolkitJs.ClipboardWriteAsync(text ?? string.Empty);
        }
    }

    [SupportedOSPlatform("browser")]
    internal sealed class WebLauncher : ILauncher
    {
        public Task<bool> OpenAsync(
            Uri uri)
        {
            if (null == uri)
            {
                throw new ArgumentNullException(nameof(uri));
            }

            ToolkitJs.OpenUrl(uri.AbsoluteUri);
            return Task.FromResult(true);
        }
    }

    [SupportedOSPlatform("browser")]
    internal sealed class LocalStoragePreferences : IPreferences
    {
        private readonly string _prefix;

        public LocalStoragePreferences(
            string prefix)
        {
            this._prefix = prefix;
        }

        public bool ContainsKey(string key) => null != ToolkitJs.StorageGet(this._prefix + key);

        public string Get(
            string key,
            string defaultValue = null)
        {
            return ToolkitJs.StorageGet(this._prefix + key) ?? defaultValue;
        }

        public void Set(
            string key,
            string value)
        {
            ToolkitJs.StorageSet(this._prefix + key, value);
        }

        public void Remove(
            string key)
        {
            ToolkitJs.StorageSet(this._prefix + key, null);
        }

        public void Clear()
        {
            ToolkitJs.StorageClear(this._prefix);
        }
    }

    [SupportedOSPlatform("browser")]
    internal sealed class LocalStorageSecrets : ISecureStorage
    {
        private readonly LocalStoragePreferences _store;

        public LocalStorageSecrets(
            string prefix)
        {
            this._store = new LocalStoragePreferences(prefix);
        }

        public Task<string> GetAsync(string key) => Task.FromResult(this._store.Get(key));

        public Task SetAsync(
            string key,
            string value)
        {
            this._store.Set(key, value);
            return Task.CompletedTask;
        }

        public bool Remove(
            string key)
        {
            bool exists = this._store.ContainsKey(key);
            this._store.Remove(key);
            return exists;
        }

        public void RemoveAll()
        {
            this._store.Clear();
        }
    }

    /// <summary>
    /// The system's share sheet where the browser has one (navigator.share),
    /// or the clipboard.
    /// </summary>
    [SupportedOSPlatform("browser")]
    internal sealed class WebShare : IShare
    {
        public Task RequestAsync(
            ShareRequest request)
        {
            if (null == request)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (ToolkitJs.CanShare())
            {
                return ToolkitJs.ShareAsync(request.Title, request.Text, request.Uri?.AbsoluteUri);
            }

            string text = string.Join("\n", new[] { request.Text, request.Uri?.AbsoluteUri }.Where((part) => !string.IsNullOrEmpty(part)));
            return ToolkitJs.ClipboardWriteAsync(text);
        }
    }

    [SupportedOSPlatform("browser")]
    internal sealed class VibrationHapticFeedback : IHapticFeedback
    {
        public bool IsSupported => true;

        public void Perform(
            HapticFeedbackType type)
        {
            ToolkitJs.Vibrate((type == HapticFeedbackType.LongPress) ? 30 : 10);
        }
    }

    /// <summary>
    /// Whether the browser is online (navigator.onLine): a network, which may
    /// or may not reach the internet.
    /// </summary>
    [SupportedOSPlatform("browser")]
    internal sealed class WebConnectivity : IConnectivity
    {
        private NetworkAccess _networkAccess;

        public WebConnectivity()
        {
            Current = this;
            this._networkAccess = Read();
            ToolkitJs.ListenOnline();
        }

        public static WebConnectivity Current
        {
            get;
            private set;
        }

        public NetworkAccess NetworkAccess
        {
            get
            {
                return this._networkAccess;
            }
        }

        public event EventHandler ConnectivityChanged;

        private static NetworkAccess Read()
        {
            return ToolkitJs.IsOnline() ? NetworkAccess.Internet : NetworkAccess.None;
        }

        public void OnChanged()
        {
            var access = Read();
            if (access != this._networkAccess)
            {
                this._networkAccess = access;
                this.ConnectivityChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    /// <summary>
    /// Files through a file input, and saving as a download (the browser
    /// chooses where; it can't say whether the user canceled).
    /// </summary>
    [SupportedOSPlatform("browser")]
    internal sealed class WebFilePicker : IFilePicker
    {
        public async Task<IReadOnlyList<PickedFile>> PickAsync(
            FilePickerOptions options = null)
        {
            string accept = string.Join(",", options?.FileTypes?.Select((type) => "." + type.TrimStart('.')) ?? Array.Empty<string>());
            string picked = await ToolkitJs.PickFilesAsync(accept, options?.AllowMultiple ?? false);
            var names = string.IsNullOrEmpty(picked) ? Array.Empty<string>() : picked.Split('\n');

            var files = new List<PickedFile>();
            for (int i = 0; i < names.Length; i += 1)
            {
                int index = i;
                files.Add(new PickedFile(names[i], async () =>
                {
                    var bytes = new byte[await ToolkitJs.LoadPickedFileAsync(index)];
                    ToolkitJs.CopyPickedFile(index, bytes);
                    return (Stream)new MemoryStream(bytes);
                }));
            }

            return files;
        }

        public async Task<bool> SaveAsync(
            string suggestedName,
            Func<Stream, Task> write,
            FilePickerOptions options = null)
        {
            if (null == write)
            {
                throw new ArgumentNullException(nameof(write));
            }

            using (var stream = new MemoryStream())
            {
                await write(stream);
                ToolkitJs.SaveFile(string.IsNullOrEmpty(suggestedName) ? "file" : suggestedName, stream.ToArray());
            }

            return true;
        }
    }
}
