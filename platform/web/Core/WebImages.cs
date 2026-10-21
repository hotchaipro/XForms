using System;
using System.Collections.Generic;
using System.Runtime.Versioning;
using System.Threading.Tasks;

namespace XForms.Web
{
    /// <summary>
    /// The app's images, loaded once each by URL, with their natural sizes
    /// (in CSS pixels) once loaded.
    /// </summary>
    [SupportedOSPlatform("browser")]
    internal static class WebImages
    {
        private static readonly Dictionary<string, Entry> ByUrl = new Dictionary<string, Entry>();
        private static readonly Dictionary<int, Entry> ById = new Dictionary<int, Entry>();
        private static int _nextId = 1;

        /// <summary>
        /// The URL of an image file in the app's Content folder.
        /// </summary>
        public static string UrlFor(
            ImageSource source)
        {
            switch (source)
            {
                case FileImageSource file:
                    return "Content/" + file.Path.Replace('\\', '/');

                case BitmapImageSource bitmap when (bitmap.Bitmap.Source is FileImageSource file):
                    return "Content/" + file.Path.Replace('\\', '/');

                default:
                    return null;
            }
        }

        public static Entry Load(
            string url)
        {
            if (!ByUrl.TryGetValue(url, out var entry))
            {
                entry = new Entry(_nextId++, url);
                ByUrl[url] = entry;
                ById[entry.Id] = entry;
                Js.LoadImage(entry.Id, url);
            }

            return entry;
        }

        public static void OnLoaded(
            int id,
            double width,
            double height)
        {
            if (ById.TryGetValue(id, out var entry))
            {

                entry.OnLoaded(new Size((float)width, (float)height));
            }
        }

        public sealed class Entry
        {
            private readonly TaskCompletionSource<Size> _loaded = new TaskCompletionSource<Size>();
            private List<Action> _onLoaded = new List<Action>();

            public Entry(
                int id,
                string url)
            {
                this.Id = id;
                this.Url = url;
            }

            public int Id { get; }

            public string Url { get; }

            public bool IsLoaded { get; private set; }

            public Size Size { get; private set; }

            public Task<Size> Loaded
            {
                get
                {
                    return this._loaded.Task;
                }
            }

            /// <summary>
            /// Calls back once the image has loaded (right away if it has).
            /// </summary>
            public void WhenLoaded(
                Action action)
            {
                if (this.IsLoaded)
                {
                    action();
                }
                else
                {
                    this._onLoaded.Add(action);
                }
            }

            internal void OnLoaded(
                Size size)
            {
                this.Size = size;
                this.IsLoaded = true;
                this._loaded.TrySetResult(size);

                var callbacks = this._onLoaded;
                this._onLoaded = new List<Action>();
                foreach (var callback in callbacks)
                {
                    callback();
                }
            }
        }
    }
}
