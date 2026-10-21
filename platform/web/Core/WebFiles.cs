using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Versioning;

namespace XForms.Web
{
    /// <summary>
    /// Keeps the app's files in the browser's storage. In the browser .NET's
    /// file system is in memory, so the files the app writes (anywhere but
    /// the runtime's folders; the app data folders are empty paths there, so
    /// their files go under the root) are restored from local storage before
    /// the app starts, and saved there after the app suspends (when the page
    /// is hidden or closed).
    /// </summary>
    [SupportedOSPlatform("browser")]
    internal static class WebFiles
    {
        private const string Prefix = "xforms-file:";
        private const string IndexKey = "xforms-files";
        private const string Root = "/";

        // The runtime's own folders
        private static readonly HashSet<string> SystemFolders = new HashSet<string> { "dev", "proc", "tmp", "usr" };

        public static void Restore()
        {
            string index = Js.StorageGet(IndexKey);
            if (string.IsNullOrEmpty(index))
            {
                return;
            }

            foreach (var name in index.Split('\n'))
            {
                string content = Js.StorageGet(Prefix + name);
                if (null == content)
                {
                    continue;
                }

                try
                {
                    string path = Path.Combine(Root, name);
                    Directory.CreateDirectory(Path.GetDirectoryName(path));
                    File.WriteAllBytes(path, Convert.FromBase64String(content));
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"XForms: couldn't restore {name}: {ex.Message}");
                }
            }
        }

        public static void Save()
        {
            var names = new List<string>();
            foreach (var path in AppFiles())
            {
                string name = Path.GetRelativePath(Root, path).Replace('\\', '/');
                try
                {
                    Js.StorageSet(Prefix + name, Convert.ToBase64String(File.ReadAllBytes(path)));
                    names.Add(name);
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"XForms: couldn't save {name}: {ex.Message}");
                }
            }

            // Files deleted since the last save go too
            string previous = Js.StorageGet(IndexKey);
            if (!string.IsNullOrEmpty(previous))
            {
                var kept = new HashSet<string>(names);
                foreach (var name in previous.Split('\n'))
                {
                    if (!kept.Contains(name))
                    {
                        Js.StorageSet(Prefix + name, null);
                    }
                }
            }

            Js.StorageSet(IndexKey, (names.Count > 0) ? string.Join("\n", names) : null);
        }

        private static IEnumerable<string> AppFiles()
        {
            foreach (var path in Directory.EnumerateFiles(Root))
            {
                yield return path;
            }

            foreach (var folder in Directory.EnumerateDirectories(Root))
            {
                if (SystemFolders.Contains(Path.GetFileName(folder)))
                {
                    continue;
                }

                foreach (var path in Directory.EnumerateFiles(folder, "*", SearchOption.AllDirectories))
                {
                    yield return path;
                }
            }
        }
    }
}
