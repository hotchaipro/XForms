using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CoreFoundation;
using Foundation;
using Network;
using UIKit;
using UniformTypeIdentifiers;

namespace XForms.iOS
{
    /// <summary>
    /// Whether the device is online, from the Network framework's path
    /// monitor.
    /// </summary>
    internal sealed class PathConnectivity : IConnectivity
    {
        private readonly NWPathMonitor _monitor = new NWPathMonitor();
        private NetworkAccess _networkAccess = NetworkAccess.Unknown;

        public PathConnectivity()
        {
            this._monitor.SnapshotHandler = this.OnPath;
            this._monitor.SetQueue(DispatchQueue.MainQueue);
            this._monitor.Start();
        }

        public NetworkAccess NetworkAccess
        {
            get
            {
                return this._networkAccess;
            }
        }

        public event EventHandler ConnectivityChanged;

        private void OnPath(
            NWPath path)
        {
            var access = (path.Status == NWPathStatus.Satisfied) ? NetworkAccess.Internet : NetworkAccess.None;
            if (access != this._networkAccess)
            {
                this._networkAccess = access;
                this.ConnectivityChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    /// <summary>
    /// The document picker: copies of the files the user picks, and exporting
    /// a file to where they choose.
    /// </summary>
    internal sealed class DocumentPicker : IFilePicker
    {
        public DocumentPicker()
        {
        }

        public async Task<IReadOnlyList<PickedFile>> PickAsync(
            FilePickerOptions options = null)
        {
            var types = ContentTypes(options?.FileTypes);
            var picker = new UIDocumentPickerViewController(types, asCopy: true)
            {
                AllowsMultipleSelection = options?.AllowMultiple ?? false,
            };

            var urls = await this.PresentAsync(picker);
            return urls
                .Select((url) => new PickedFile(url.LastPathComponent, () => Task.FromResult<Stream>(File.OpenRead(url.Path))))
                .ToList();
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

            // The file is written to a temporary folder, then exported
            string folder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(folder);
            string path = Path.Combine(folder, string.IsNullOrEmpty(suggestedName) ? "file" : suggestedName);
            using (var stream = File.Create(path))
            {
                await write(stream);
            }

            var picker = new UIDocumentPickerViewController(new[] { NSUrl.FromFilename(path) }, asCopy: true);
            var urls = await this.PresentAsync(picker);
            return 0 < urls.Length;
        }

        private Task<NSUrl[]> PresentAsync(
            UIDocumentPickerViewController picker)
        {
            var completion = new TaskCompletionSource<NSUrl[]>();
            picker.DidPickDocumentAtUrls += (sender, e) => completion.TrySetResult(e.Urls);
            picker.WasCancelled += (sender, e) => completion.TrySetResult(Array.Empty<NSUrl>());

            var presenter = IosApplication.Current?.RootViewController;
            if (null == presenter)
            {
                return Task.FromResult(Array.Empty<NSUrl>());
            }

            presenter.PresentViewController(picker, true, null);
            return completion.Task;
        }

        private static UTType[] ContentTypes(
            IEnumerable<string> extensions)
        {
            var types = extensions?
                .Select((extension) => UTType.CreateFromExtension(extension.TrimStart('.')))
                .Where((type) => null != type)
                .ToArray();

            return ((null != types) && (0 < types.Length)) ? types : new[] { UTTypes.Item };
        }
    }
}
