using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace XForms.Win32
{
    /// <summary>
    /// Windows' services for the application.
    /// </summary>
    public static class Win32ToolkitServices
    {
        /// <summary>
        /// Makes Windows' services the app's (<see cref="PlatformServices.Current"/>):
        /// call it before <see cref="Win32Application.Run"/>.
        /// </summary>
        public static void Register()
        {
            PlatformServices.Current ??= Create();
        }

        private static PlatformServices Create()
        {
            var preferences = new FilePreferences(Path.Combine(AppDataFolder, "preferences.json"));
            return new PlatformServices()
            {
                Clipboard = new Win32Clipboard(),
                Launcher = new ShellLauncher(),
                Preferences = preferences,
                SecureStorage = new ProtectedStorage(Path.Combine(AppDataFolder, "secure.json")),
                Share = new ClipboardShare(),
                HapticFeedback = new NoHapticFeedback(),
                Connectivity = new NetworkConnectivity(),
                FilePicker = new CommonFileDialogs(),
            };
        }

        // The app's folder in the user's local application data
        private static string AppDataFolder
        {
            get
            {
                string name = Assembly.GetEntryAssembly()?.GetName().Name ?? "XForms";
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), name);
            }
        }
    }

    internal sealed class Win32Clipboard : IClipboard
    {
        private const uint CF_UNICODETEXT = 13;
        private const uint GMEM_MOVEABLE = 0x0002;

        public bool HasText
        {
            get
            {
                return IsClipboardFormatAvailable(CF_UNICODETEXT);
            }
        }

        public Task<string> GetTextAsync()
        {
            string text = null;
            if (OpenClipboard(IntPtr.Zero))
            {
                try
                {
                    var handle = GetClipboardData(CF_UNICODETEXT);
                    if (IntPtr.Zero != handle)
                    {
                        var pointer = GlobalLock(handle);
                        try
                        {
                            text = Marshal.PtrToStringUni(pointer);
                        }
                        finally
                        {
                            GlobalUnlock(handle);
                        }
                    }
                }
                finally
                {
                    CloseClipboard();
                }
            }

            return Task.FromResult(text);
        }

        public Task SetTextAsync(
            string text)
        {
            if (!OpenClipboard(IntPtr.Zero))
            {
                throw new InvalidOperationException("The clipboard is in use.");
            }

            try
            {
                EmptyClipboard();
                if (!string.IsNullOrEmpty(text))
                {
                    int bytes = (text.Length + 1) * 2;
                    var handle = GlobalAlloc(GMEM_MOVEABLE, (UIntPtr)bytes);
                    var pointer = GlobalLock(handle);
                    Marshal.Copy(text.ToCharArray(), 0, pointer, text.Length);
                    Marshal.WriteInt16(pointer, text.Length * 2, 0);
                    GlobalUnlock(handle);

                    // The clipboard owns the memory now
                    SetClipboardData(CF_UNICODETEXT, handle);
                }
            }
            finally
            {
                CloseClipboard();
            }

            return Task.CompletedTask;
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool OpenClipboard(IntPtr owner);

        [DllImport("user32.dll")]
        private static extern bool CloseClipboard();

        [DllImport("user32.dll")]
        private static extern bool EmptyClipboard();

        [DllImport("user32.dll")]
        private static extern bool IsClipboardFormatAvailable(uint format);

        [DllImport("user32.dll")]
        private static extern IntPtr GetClipboardData(uint format);

        [DllImport("user32.dll")]
        private static extern IntPtr SetClipboardData(uint format, IntPtr memory);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GlobalAlloc(uint flags, UIntPtr bytes);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GlobalLock(IntPtr memory);

        [DllImport("kernel32.dll")]
        private static extern bool GlobalUnlock(IntPtr memory);
    }

    internal sealed class ShellLauncher : ILauncher
    {
        public Task<bool> OpenAsync(
            Uri uri)
        {
            if (null == uri)
            {
                throw new ArgumentNullException(nameof(uri));
            }

            try
            {
                Process.Start(new ProcessStartInfo(uri.AbsoluteUri) { UseShellExecute = true });
                return Task.FromResult(true);
            }
            catch (System.ComponentModel.Win32Exception)
            {
                return Task.FromResult(false);
            }
        }
    }

    /// <summary>
    /// Preferences in a JSON file.
    /// </summary>
    internal sealed class FilePreferences : IPreferences
    {
        private readonly string _path;
        private Dictionary<string, string> _values;

        public FilePreferences(
            string path)
        {
            this._path = path;
        }

        private Dictionary<string, string> Values
        {
            get
            {
                if (null == this._values)
                {
                    this._values = File.Exists(this._path)
                        ? JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(this._path)) ?? new Dictionary<string, string>()
                        : new Dictionary<string, string>();
                }

                return this._values;
            }
        }

        private void Save()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(this._path));
            File.WriteAllText(this._path, JsonSerializer.Serialize(this._values));
        }

        public bool ContainsKey(string key) => this.Values.ContainsKey(key);

        public string Get(
            string key,
            string defaultValue = null)
        {
            return this.Values.TryGetValue(key, out var value) ? value : defaultValue;
        }

        public void Set(
            string key,
            string value)
        {
            if (null == value)
            {
                this.Remove(key);
                return;
            }

            this.Values[key] = value;
            this.Save();
        }

        public void Remove(
            string key)
        {
            if (this.Values.Remove(key))
            {
                this.Save();
            }
        }

        public void Clear()
        {
            this.Values.Clear();
            this.Save();
        }
    }

    /// <summary>
    /// Secrets encrypted with DPAPI for the current user, in a JSON file.
    /// </summary>
    internal sealed class ProtectedStorage : ISecureStorage
    {
        private readonly FilePreferences _file;

        public ProtectedStorage(
            string path)
        {
            this._file = new FilePreferences(path);
        }

        public Task<string> GetAsync(
            string key)
        {
            string stored = this._file.Get(key);
            if (null == stored)
            {
                return Task.FromResult<string>(null);
            }

            byte[] data = Unprotect(Convert.FromBase64String(stored));
            return Task.FromResult(Encoding.UTF8.GetString(data));
        }

        public Task SetAsync(
            string key,
            string value)
        {
            if (null == value)
            {
                this.Remove(key);
            }
            else
            {
                this._file.Set(key, Convert.ToBase64String(Protect(Encoding.UTF8.GetBytes(value))));
            }

            return Task.CompletedTask;
        }

        public bool Remove(
            string key)
        {
            bool exists = this._file.ContainsKey(key);
            this._file.Remove(key);
            return exists;
        }

        public void RemoveAll()
        {
            this._file.Clear();
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct DataBlob
        {
            public int Size;
            public IntPtr Data;
        }

        private static byte[] Protect(byte[] data) => Transform(data, protect: true);

        private static byte[] Unprotect(byte[] data) => Transform(data, protect: false);

        private static byte[] Transform(
            byte[] data,
            bool protect)
        {
            var input = new DataBlob() { Size = data.Length, Data = Marshal.AllocHGlobal(data.Length) };
            var output = new DataBlob();
            try
            {
                Marshal.Copy(data, 0, input.Data, data.Length);
                bool succeeded = protect
                    ? CryptProtectData(ref input, null, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, 0, ref output)
                    : CryptUnprotectData(ref input, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, 0, ref output);
                if (!succeeded)
                {
                    throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
                }

                var result = new byte[output.Size];
                Marshal.Copy(output.Data, result, 0, output.Size);
                return result;
            }
            finally
            {
                Marshal.FreeHGlobal(input.Data);
                if (IntPtr.Zero != output.Data)
                {
                    LocalFree(output.Data);
                }
            }
        }

        [DllImport("crypt32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool CryptProtectData(ref DataBlob input, string description, IntPtr entropy, IntPtr reserved, IntPtr prompt, int flags, ref DataBlob output);

        [DllImport("crypt32.dll", SetLastError = true)]
        private static extern bool CryptUnprotectData(ref DataBlob input, IntPtr description, IntPtr entropy, IntPtr reserved, IntPtr prompt, int flags, ref DataBlob output);

        [DllImport("kernel32.dll")]
        private static extern IntPtr LocalFree(IntPtr memory);
    }

    /// <summary>
    /// Windows' share sheet needs a packaged app's WinRT APIs, so sharing copies
    /// the text and link to the clipboard instead.
    /// </summary>
    internal sealed class ClipboardShare : IShare
    {
        public Task RequestAsync(
            ShareRequest request)
        {
            if (null == request)
            {
                throw new ArgumentNullException(nameof(request));
            }

            string text = string.Join(Environment.NewLine, new[] { request.Text, request.Uri?.AbsoluteUri }).Trim();
            return new Win32Clipboard().SetTextAsync(text);
        }
    }

    internal sealed class NoHapticFeedback : IHapticFeedback
    {
        public bool IsSupported => false;

        public void Perform(HapticFeedbackType type)
        {
        }
    }
}
