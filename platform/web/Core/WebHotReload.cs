using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Runtime.Versioning;
using System.Text.Json;

namespace XForms.Web
{
    /// <summary>
    /// Hot reload in the browser: <c>dotnet watch</c>'s browser script hands
    /// edits to the page through Blazor's hooks
    /// (<c>Blazor._internal.getApplyUpdateCapabilities</c> and
    /// <c>applyHotReloadDeltas</c>), which XForms provides when the app runs
    /// with hot reload. Each edit is applied to its assembly, and the
    /// assemblies' update handlers run, as .NET's agent does on the desktop.
    /// </summary>
    /// <remarks>
    /// Only when <see cref="MetadataUpdater.IsSupported"/> (a Debug build run
    /// by <c>dotnet watch</c>): it's false, and this code trimmed away, in a
    /// published app.
    /// </remarks>
    [SupportedOSPlatform("browser")]
    internal static class WebHotReload
    {
        public static void Install()
        {
            if (!MetadataUpdater.IsSupported)
            {
                return;
            }

            Js.InstallHotReload(GetCapabilities());
        }

        // What edits the runtime can apply, as .NET's agent reports them
        [UnconditionalSuppressMessage("Trimming", "IL2075", Justification = "Only with hot reload, which a trimmed app doesn't have.")]
        private static string GetCapabilities()
        {
            var method = typeof(MetadataUpdater).GetMethod("GetCapabilities", BindingFlags.NonPublic | BindingFlags.Static, Type.EmptyTypes);
            return (method?.Invoke(null, null) as string) ?? string.Empty;
        }

        /// <summary>
        /// Applies edits (a JSON array of deltas: moduleId, metadataDelta,
        /// ilDelta and pdbDelta in base 64, and updatedTypes as metadata
        /// tokens), then runs every update handler; returns the log entries,
        /// as JSON.
        /// </summary>
        [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "Only with hot reload, which a trimmed app doesn't have.")]
        [UnconditionalSuppressMessage("Trimming", "IL2072", Justification = "Only with hot reload, which a trimmed app doesn't have.")]
        [UnconditionalSuppressMessage("Trimming", "IL2075", Justification = "Only with hot reload, which a trimmed app doesn't have.")]
        public static string ApplyDeltas(
            string deltasJson)
        {
            // Never without hot reload (and trimmed away in a published app)
            if (!MetadataUpdater.IsSupported)
            {
                return "[]";
            }

            var log = new List<string>();
            try
            {
                var updatedTypes = new List<Type>();
                var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                using (var document = JsonDocument.Parse(deltasJson))
                {
                    foreach (var delta in document.RootElement.EnumerateArray())
                    {
                        var moduleId = Guid.Parse(delta.GetProperty("moduleId").GetString());
                        var assembly = assemblies.FirstOrDefault((candidate) => candidate.ManifestModule.ModuleVersionId == moduleId);
                        if (null == assembly)
                        {
                            continue;
                        }

                        MetadataUpdater.ApplyUpdate(
                            assembly,
                            Bytes(delta, "metadataDelta"),
                            Bytes(delta, "ilDelta"),
                            Bytes(delta, "pdbDelta"));

                        if (delta.TryGetProperty("updatedTypes", out var tokens) && (tokens.ValueKind == JsonValueKind.Array))
                        {
                            foreach (var token in tokens.EnumerateArray())
                            {
                                try
                                {
                                    updatedTypes.Add(assembly.ManifestModule.ResolveType(token.GetInt32()));
                                }
                                catch (ArgumentException)
                                {
                                    // A type the edit removed
                                }
                            }
                        }
                    }
                }

                var types = updatedTypes.ToArray();
                var handlers = assemblies
                    .SelectMany((assembly) => assembly.GetCustomAttributes<MetadataUpdateHandlerAttribute>())
                    .Select((attribute) => attribute.HandlerType)
                    .ToList();
                foreach (var name in new[] { "ClearCache", "UpdateApplication" })
                {
                    foreach (var handler in handlers)
                    {
                        handler.GetMethod(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static, new[] { typeof(Type[]) })
                            ?.Invoke(null, new object[] { types });
                    }
                }
            }
            catch (Exception ex)
            {
                log.Add("\"" + JsonEncodedText.Encode(ex.ToString()) + "\"");
                Console.Error.WriteLine($"XForms: hot reload failed: {ex}");
            }

            return "[" + string.Join(",", log.Select((message) => "{\"message\":" + message + ",\"severity\":2}")) + "]";
        }

        private static byte[] Bytes(
            JsonElement delta,
            string name)
        {
            return delta.TryGetProperty(name, out var value) && (value.ValueKind == JsonValueKind.String)
                ? value.GetBytesFromBase64()
                : Array.Empty<byte>();
        }
    }
}
