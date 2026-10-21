using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.System.Com;
using Windows.Win32.UI.Shell;
using Windows.Win32.UI.Shell.Common;

namespace XForms.Win32
{
    /// <summary>
    /// The common file dialogs (IFileOpenDialog and IFileSaveDialog), shown
    /// modally over the window.
    /// </summary>
    internal sealed class CommonFileDialogs : IFilePicker
    {
        private const int Canceled = unchecked((int)0x800704C7); // ERROR_CANCELLED

        private readonly Win32Host _host;

        public CommonFileDialogs(
            Win32Host host)
        {
            this._host = host;
        }

        public Task<IReadOnlyList<PickedFile>> PickAsync(
            FilePickerOptions options = null)
        {
            return Task.FromResult<IReadOnlyList<PickedFile>>(this.ShowOpenDialog(options));
        }

        private unsafe List<PickedFile> ShowOpenDialog(
            FilePickerOptions options)
        {
            var files = new List<PickedFile>();

            PInvoke.CoCreateInstance(typeof(FileOpenDialog).GUID, null, CLSCTX.CLSCTX_INPROC_SERVER, IFileOpenDialog.IID_Guid, out var pointer).ThrowOnFailure();
            var dialog = (IFileOpenDialog*)pointer;
            try
            {
                FILEOPENDIALOGOPTIONS flags;
                dialog->GetOptions(&flags);
                flags |= FILEOPENDIALOGOPTIONS.FOS_FORCEFILESYSTEM | FILEOPENDIALOGOPTIONS.FOS_FILEMUSTEXIST;
                if (options?.AllowMultiple ?? false)
                {
                    flags |= FILEOPENDIALOGOPTIONS.FOS_ALLOWMULTISELECT;
                }

                dialog->SetOptions(flags);
                Configure((IFileDialog*)dialog, options);

                if (!Show((IFileDialog*)dialog, this._host.Hwnd))
                {
                    return files;
                }

                IShellItemArray* items;
                dialog->GetResults(&items);
                try
                {
                    uint count;
                    items->GetCount(&count);
                    for (uint i = 0; i < count; i += 1)
                    {
                        IShellItem* item;
                        items->GetItemAt(i, &item);
                        string path = GetPath(item);
                        item->Release();

                        files.Add(new PickedFile(Path.GetFileName(path), () => Task.FromResult<Stream>(File.OpenRead(path))));
                    }
                }
                finally
                {
                    items->Release();
                }
            }
            finally
            {
                dialog->Release();
            }

            return files;
        }

        // Shows the dialog modally; false when the user cancels
        private static unsafe bool Show(
            IFileDialog* dialog,
            HWND owner)
        {
            try
            {
                dialog->Show(owner);
                return true;
            }
            catch (System.Runtime.InteropServices.COMException ex) when (ex.HResult == Canceled)
            {
                return false;
            }
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

            string path = this.ShowSaveDialog(suggestedName, options);
            if (null == path)
            {
                return false;
            }

            using (var stream = File.Create(path))
            {
                await write(stream);
            }

            return true;
        }

        private unsafe string ShowSaveDialog(
            string suggestedName,
            FilePickerOptions options)
        {
            PInvoke.CoCreateInstance(typeof(FileSaveDialog).GUID, null, CLSCTX.CLSCTX_INPROC_SERVER, IFileSaveDialog.IID_Guid, out var pointer).ThrowOnFailure();
            var dialog = (IFileSaveDialog*)pointer;
            try
            {
                FILEOPENDIALOGOPTIONS flags;
                dialog->GetOptions(&flags);
                dialog->SetOptions(flags | FILEOPENDIALOGOPTIONS.FOS_FORCEFILESYSTEM | FILEOPENDIALOGOPTIONS.FOS_OVERWRITEPROMPT);
                Configure((IFileDialog*)dialog, options);

                if (!string.IsNullOrEmpty(suggestedName))
                {
                    dialog->SetFileName(suggestedName);
                    string extension = Path.GetExtension(suggestedName);
                    if (!string.IsNullOrEmpty(extension))
                    {
                        dialog->SetDefaultExtension(extension.TrimStart('.'));
                    }
                }

                if (!Show((IFileDialog*)dialog, this._host.Hwnd))
                {
                    return null;
                }

                IShellItem* item;
                dialog->GetResult(&item);
                string path = GetPath(item);
                item->Release();
                return path;
            }
            finally
            {
                dialog->Release();
            }
        }

        // The title and the file types, as one filter ("Files (*.txt;*.md)")
        private static unsafe void Configure(
            IFileDialog* dialog,
            FilePickerOptions options)
        {
            if (!string.IsNullOrEmpty(options?.Title))
            {
                dialog->SetTitle(options.Title);
            }

            var types = options?.FileTypes;
            if ((null != types) && (0 < types.Count))
            {
                string pattern = string.Join(";", types.Select((type) => "*." + type.TrimStart('.')));
                fixed (char* name = $"Files ({pattern})")
                fixed (char* spec = pattern)
                {
                    var filter = new COMDLG_FILTERSPEC() { pszName = name, pszSpec = spec };
                    dialog->SetFileTypes(1, &filter);
                }
            }
        }

        private static unsafe string GetPath(
            IShellItem* item)
        {
            PWSTR name;
            item->GetDisplayName(SIGDN.SIGDN_FILESYSPATH, &name);
            try
            {
                return name.ToString();
            }
            finally
            {
                PInvoke.CoTaskMemFree(name);
            }
        }
    }

    /// <summary>
    /// Whether there's a network, from the network interfaces: one that's up
    /// with a gateway is taken to reach the internet.
    /// </summary>
    internal sealed class NetworkConnectivity : IConnectivity
    {
        private readonly SynchronizationContext _uiContext = SynchronizationContext.Current;
        private NetworkAccess _networkAccess;
        private EventHandler _connectivityChanged;

        public NetworkConnectivity()
        {
            this._networkAccess = Read();
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
                    NetworkChange.NetworkAddressChanged += this.OnNetworkChanged;
                    NetworkChange.NetworkAvailabilityChanged += this.OnNetworkChanged;
                }

                this._connectivityChanged += value;
            }

            remove
            {
                this._connectivityChanged -= value;
                if (null == this._connectivityChanged)
                {
                    NetworkChange.NetworkAddressChanged -= this.OnNetworkChanged;
                    NetworkChange.NetworkAvailabilityChanged -= this.OnNetworkChanged;
                }
            }
        }

        // Raised on a thread-pool thread
        private void OnNetworkChanged(
            object sender,
            EventArgs e)
        {
            var access = Read();
            this._uiContext.Post((state) =>
            {
                if (access != this._networkAccess)
                {
                    this._networkAccess = access;
                    this._connectivityChanged?.Invoke(this, EventArgs.Empty);
                }
            }, null);
        }

        private static NetworkAccess Read()
        {
            if (!NetworkInterface.GetIsNetworkAvailable())
            {
                return NetworkAccess.None;
            }

            foreach (var network in NetworkInterface.GetAllNetworkInterfaces())
            {
                if ((network.OperationalStatus != OperationalStatus.Up)
                    || (network.NetworkInterfaceType == NetworkInterfaceType.Loopback)
                    || (network.NetworkInterfaceType == NetworkInterfaceType.Tunnel))
                {
                    continue;
                }

                foreach (var gateway in network.GetIPProperties().GatewayAddresses)
                {
                    if (!gateway.Address.Equals(IPAddress.Any) && !gateway.Address.Equals(IPAddress.IPv6Any))
                    {
                        return NetworkAccess.Internet;
                    }
                }
            }

            return NetworkAccess.Local;
        }
    }
}
