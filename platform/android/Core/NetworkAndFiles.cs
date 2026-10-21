using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Net;
using Android.Provider;
using Android.Webkit;

// Connectivity reads the network's state
[assembly: UsesPermission(global::Android.Manifest.Permission.AccessNetworkState)]

namespace XForms.Android
{
    /// <summary>
    /// Whether the device is online, from <see cref="ConnectivityManager"/>: a
    /// network the system has validated reaches the internet.
    /// </summary>
    internal sealed class AndroidConnectivity : IConnectivity
    {
        private readonly AndroidApplication _host;
        private readonly ConnectivityManager _manager;
        private NetworkAccess _networkAccess;
        private EventHandler _connectivityChanged;
        private Callback _callback;

        public AndroidConnectivity(
            AndroidApplication host)
        {
            this._host = host;
            this._manager = (ConnectivityManager)host.Context.GetSystemService(Context.ConnectivityService);
            this._networkAccess = this.Read();
        }

        public NetworkAccess NetworkAccess
        {
            get
            {
                return this._networkAccess;
            }
        }

        public event EventHandler ConnectivityChanged
        {
            add
            {
                // Listens to the system only while someone listens here
                if (null == this._connectivityChanged)
                {
                    this._callback = new Callback(this);
                    this._manager.RegisterDefaultNetworkCallback(this._callback);
                }

                this._connectivityChanged += value;
            }

            remove
            {
                this._connectivityChanged -= value;
                if ((null == this._connectivityChanged) && (null != this._callback))
                {
                    this._manager.UnregisterNetworkCallback(this._callback);
                    this._callback = null;
                }
            }
        }

        private NetworkAccess Read()
        {
            var network = this._manager.ActiveNetwork;
            var capabilities = (null == network) ? null : this._manager.GetNetworkCapabilities(network);
            if (null == capabilities)
            {
                return NetworkAccess.None;
            }

            return AccessFor(capabilities);
        }

        private static NetworkAccess AccessFor(
            NetworkCapabilities capabilities)
        {
            return (capabilities.HasCapability(NetCapability.Internet) && capabilities.HasCapability(NetCapability.Validated))
                ? NetworkAccess.Internet
                : NetworkAccess.Local;
        }

        // The callback runs on a system thread. It says what changed: while a
        // network is being lost, the system can still report it as active.
        private void OnChanged(
            NetworkAccess access)
        {
            this._host.RunOnUiThread(() =>
            {
                if (access != this._networkAccess)
                {
                    this._networkAccess = access;
                    this._connectivityChanged?.Invoke(this, EventArgs.Empty);
                }
            });
        }

        private sealed class Callback : ConnectivityManager.NetworkCallback
        {
            private readonly AndroidConnectivity _owner;

            public Callback(
                AndroidConnectivity owner)
            {
                this._owner = owner;
            }

            // The default network's capabilities follow its arrival, and change
            // as the system validates it
            public override void OnCapabilitiesChanged(Network network, NetworkCapabilities networkCapabilities) => this._owner.OnChanged(AccessFor(networkCapabilities));

            public override void OnLost(Network network) => this._owner.OnChanged(NetworkAccess.None);
        }
    }

    /// <summary>
    /// The system's document picker (the Storage Access Framework): access to
    /// the files the user picks, through content URIs.
    /// </summary>
    internal sealed class DocumentPicker : IFilePicker
    {
        private readonly AndroidApplication _host;

        public DocumentPicker(
            AndroidApplication host)
        {
            this._host = host;
        }

        public async Task<IReadOnlyList<PickedFile>> PickAsync(
            FilePickerOptions options = null)
        {
            var intent = new Intent(Intent.ActionOpenDocument);
            intent.AddCategory(Intent.CategoryOpenable);
            intent.SetType("*/*");

            var mimeTypes = MimeTypes(options?.FileTypes);
            if (0 < mimeTypes.Length)
            {
                intent.PutExtra(Intent.ExtraMimeTypes, mimeTypes);
            }

            if (options?.AllowMultiple ?? false)
            {
                intent.PutExtra(Intent.ExtraAllowMultiple, true);
            }

            var files = new List<PickedFile>();
            var (result, data) = await this._host.StartActivityForResultAsync(intent);
            if ((result != Result.Ok) || (null == data))
            {
                return files;
            }

            var uris = new List<global::Android.Net.Uri>();
            if (null != data.ClipData)
            {
                for (int i = 0; i < data.ClipData.ItemCount; i += 1)
                {
                    uris.Add(data.ClipData.GetItemAt(i).Uri);
                }
            }
            else if (null != data.Data)
            {
                uris.Add(data.Data);
            }

            var resolver = this._host.Context.ContentResolver;
            foreach (var uri in uris)
            {
                files.Add(new PickedFile(this.DisplayName(uri), () => Task.FromResult(resolver.OpenInputStream(uri))));
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

            var intent = new Intent(Intent.ActionCreateDocument);
            intent.AddCategory(Intent.CategoryOpenable);
            intent.SetType(MimeTypes(new[] { Path.GetExtension(suggestedName ?? string.Empty) }).FirstOrDefault() ?? "application/octet-stream");
            if (!string.IsNullOrEmpty(suggestedName))
            {
                intent.PutExtra(Intent.ExtraTitle, suggestedName);
            }

            var (result, data) = await this._host.StartActivityForResultAsync(intent);
            if ((result != Result.Ok) || (null == data?.Data))
            {
                return false;
            }

            using (var stream = this._host.Context.ContentResolver.OpenOutputStream(data.Data))
            {
                await write(stream);
            }

            return true;
        }

        private static string[] MimeTypes(
            IEnumerable<string> extensions)
        {
            if (null == extensions)
            {
                return Array.Empty<string>();
            }

            return extensions
                .Select((extension) => MimeTypeMap.Singleton.GetMimeTypeFromExtension(extension.TrimStart('.').ToLowerInvariant()))
                .Where((type) => null != type)
                .Distinct()
                .ToArray();
        }

        private string DisplayName(
            global::Android.Net.Uri uri)
        {
            using (var cursor = this._host.Context.ContentResolver.Query(uri, new[] { IOpenableColumns.DisplayName }, null, null, null))
            {
                if ((null != cursor) && cursor.MoveToFirst())
                {
                    return cursor.GetString(0);
                }
            }

            return uri.LastPathSegment;
        }
    }
}
