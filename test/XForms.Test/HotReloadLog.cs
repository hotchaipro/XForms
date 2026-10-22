using System;
using System.IO;
using System.Linq;

[assembly: System.Reflection.Metadata.MetadataUpdateHandler(typeof(XForms.Test.HotReloadLog))]

namespace XForms.Test
{
    /// <summary>
    /// Logs each hot reload update .NET applies to the test app (to the console,
    /// which Android's logcat and the browser's console show, and on the desktop
    /// to xforms-hotreload.log in the temp folder), for checking how updates
    /// arrive on each platform.
    /// </summary>
    internal static class HotReloadLog
    {
        internal static void UpdateApplication(
            Type[] updatedTypes)
        {
            string message = $"XForms.Test: hot reload updated {string.Join(", ", updatedTypes?.Select((type) => type.Name) ?? Array.Empty<string>())}";
            Console.WriteLine(message);
            if (!OperatingSystem.IsBrowser() && !OperatingSystem.IsAndroid() && !OperatingSystem.IsIOS())
            {
                File.AppendAllText(Path.Combine(Path.GetTempPath(), "xforms-hotreload.log"), $"{DateTime.Now:T} {message}{Environment.NewLine}");
            }
        }
    }
}
