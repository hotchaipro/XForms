using System;
using System.Runtime.InteropServices.JavaScript;
using System.Runtime.Versioning;
using System.Threading.Tasks;

namespace XForms.Web
{
    /// <summary>
    /// The JavaScript side of the toolkit's services
    /// (wwwroot/xforms-toolkit.js), and the events it sends back.
    /// </summary>
    [SupportedOSPlatform("browser")]
    internal static partial class ToolkitJs
    {
        private const string Module = "xforms-toolkit";

        private static Task _initialization;

        public static Task InitializeAsync()
        {
            return _initialization ??= ImportAsync();
        }

        private static async Task ImportAsync()
        {
            // NOTE: Relative to the .NET runtime, in _framework
            await JSHost.ImportAsync(Module, "../_content/XForms.Toolkit.Web/xforms-toolkit.js");
            await InitAsync();
        }

        [JSImport("init", Module)]
        private static partial Task InitAsync();

        [JSImport("openUrl", Module)]
        public static partial void OpenUrl(string url);

        [JSImport("clipboardRead", Module)]
        public static partial Task<string> ClipboardReadAsync();

        [JSImport("clipboardWrite", Module)]
        public static partial Task ClipboardWriteAsync(string text);

        [JSImport("storageGet", Module)]
        public static partial string StorageGet(string key);

        [JSImport("storageSet", Module)]
        public static partial void StorageSet(string key, string value);

        [JSImport("storageClear", Module)]
        public static partial void StorageClear(string prefix);

        [JSImport("canShare", Module)]
        public static partial bool CanShare();

        [JSImport("share", Module)]
        public static partial Task ShareAsync(string title, string text, string url);

        [JSImport("vibrate", Module)]
        public static partial bool Vibrate(int milliseconds);

        [JSImport("isOnline", Module)]
        public static partial bool IsOnline();

        [JSImport("listenOnline", Module)]
        public static partial void ListenOnline();

        // The names of the files picked, a line each
        [JSImport("pickFiles", Module)]
        public static partial Task<string> PickFilesAsync(string accept, bool multiple);

        // Reads a picked file; its length
        [JSImport("loadPickedFile", Module)]
        public static partial Task<int> LoadPickedFileAsync(int index);

        [JSImport("copyPickedFile", Module)]
        public static partial void CopyPickedFile(int index, [JSMarshalAs<JSType.MemoryView>] Span<byte> buffer);

        [JSImport("saveFile", Module)]
        public static partial void SaveFile(string name, byte[] bytes);

        [JSExport]
        internal static void OnOnlineChanged()
        {
            WebConnectivity.Current?.OnChanged();
        }
    }
}
