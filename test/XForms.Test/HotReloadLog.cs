using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;

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
        /// <summary>
        /// Runs every assembly's hot reload handlers, as .NET's agent does
        /// after it applies an edit, for checking what follows an edit where
        /// edits can't be delivered here (Android under dotnet watch).
        /// </summary>
        [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "A test of hot reload, in Debug builds.")]
        [UnconditionalSuppressMessage("Trimming", "IL2075", Justification = "A test of hot reload, in Debug builds.")]
        public static void Simulate(
            params Type[] updatedTypes)
        {
#if DEBUG
            var handlers = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany((assembly) => assembly.GetCustomAttributes<MetadataUpdateHandlerAttribute>())
                .Select((attribute) => attribute.HandlerType)
                .ToList();
            foreach (var name in new[] { "ClearCache", "UpdateApplication" })
            {
                foreach (var handler in handlers)
                {
                    handler.GetMethod(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static, new[] { typeof(Type[]) })
                        ?.Invoke(null, new object[] { updatedTypes });
                }
            }
#endif
        }

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
