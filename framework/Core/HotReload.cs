using System;
using System.Collections.Generic;
using System.Reflection.Metadata;

[assembly: MetadataUpdateHandler(typeof(XForms.HotReload))]

namespace XForms
{
    /// <summary>
    /// What .NET calls after it applies an edit to the running app (hot
    /// reload: Visual Studio, or <c>dotnet watch</c>): the running app builds
    /// what it built once again, on the UI thread (see
    /// <see cref="Application.HotReloaded"/>). Never called otherwise.
    /// </summary>
    internal static class HotReload
    {
        internal static void ClearCache(
            Type[] updatedTypes)
        {
        }

        internal static void UpdateApplication(
            Type[] updatedTypes)
        {
            var application = Application.Current;
            if ((null == application) || !application.IsStarted)
            {
                return;
            }

            IReadOnlyList<Type> types = updatedTypes ?? Array.Empty<Type>();
            application.Dispatcher.Post(() => application.OnHotReload(types));
        }
    }

    /// <summary>
    /// Data for <see cref="Application.HotReloaded"/>.
    /// </summary>
    public sealed class HotReloadEventArgs : EventArgs
    {
        public HotReloadEventArgs(
            IReadOnlyList<Type> updatedTypes)
        {
            this.UpdatedTypes = updatedTypes ?? Array.Empty<Type>();
        }

        /// <summary>
        /// The types the edit changed (empty when .NET doesn't say).
        /// </summary>
        public IReadOnlyList<Type> UpdatedTypes
        {
            get;
        }
    }
}
