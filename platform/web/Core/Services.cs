using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;
using System.Threading.Tasks;

namespace XForms.Web
{
    /// <summary>
    /// The page's services. A page has no secret store, so secure storage is
    /// local storage too: don't keep real secrets in it on the web.
    /// </summary>
    [SupportedOSPlatform("browser")]
    internal static class WebServices
    {
        public static PlatformServices Create()
        {
            return new PlatformServices()
            {
                Clipboard = new WebClipboard(),
                Launcher = new WebLauncher(),
                Preferences = new LocalStoragePreferences("xforms.preferences."),
                SecureStorage = new LocalStorageSecrets("xforms.secure."),
                Share = new WebShare(),
                HapticFeedback = new VibrationHapticFeedback(),
                Connectivity = new WebConnectivity(),
                FilePicker = new WebFilePicker(),
            };
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
                return await Js.ClipboardReadAsync();
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
            return Js.ClipboardWriteAsync(text ?? string.Empty);
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

            Js.OpenUrl(uri.AbsoluteUri);
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

        public bool ContainsKey(string key) => null != Js.StorageGet(this._prefix + key);

        public string Get(
            string key,
            string defaultValue = null)
        {
            return Js.StorageGet(this._prefix + key) ?? defaultValue;
        }

        public void Set(
            string key,
            string value)
        {
            Js.StorageSet(this._prefix + key, value);
        }

        public void Remove(
            string key)
        {
            Js.StorageSet(this._prefix + key, null);
        }

        public void Clear()
        {
            Js.StorageClear(this._prefix);
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

            if (Js.CanShare())
            {
                return Js.ShareAsync(request.Title, request.Text, request.Uri?.AbsoluteUri);
            }

            string text = string.Join("\n", new[] { request.Text, request.Uri?.AbsoluteUri }.Where((part) => !string.IsNullOrEmpty(part)));
            return Js.ClipboardWriteAsync(text);
        }
    }

    [SupportedOSPlatform("browser")]
    internal sealed class VibrationHapticFeedback : IHapticFeedback
    {
        public bool IsSupported => true;

        public void Perform(
            HapticFeedbackType type)
        {
            Js.Vibrate((type == HapticFeedbackType.LongPress) ? 30 : 10);
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
            Js.ListenOnline();
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
            return Js.IsOnline() ? NetworkAccess.Internet : NetworkAccess.None;
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
            string picked = await Js.PickFilesAsync(accept, options?.AllowMultiple ?? false);
            var names = string.IsNullOrEmpty(picked) ? Array.Empty<string>() : picked.Split('\n');

            var files = new List<PickedFile>();
            for (int i = 0; i < names.Length; i += 1)
            {
                int index = i;
                files.Add(new PickedFile(names[i], async () =>
                {
                    var bytes = new byte[await Js.LoadPickedFileAsync(index)];
                    Js.CopyPickedFile(index, bytes);
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
                Js.SaveFile(string.IsNullOrEmpty(suggestedName) ? "file" : suggestedName, stream.ToArray());
            }

            return true;
        }
    }
}
