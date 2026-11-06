using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Views;
using XForma.Platform;
using AndroidUri = Android.Net.Uri;
using AndroidView = Android.Views.View;

namespace XForma.Android
{
    /// <summary>
    /// Android's drag and drop. A drag can go to other apps in split screen; a
    /// layer's native view hears of drags over it and finds the view inside it.
    /// </summary>
    internal static class AndroidDragDrop
    {
        /// <summary>
        /// Starts a drag. It shows a plain box the size of the view, under the
        /// finger.
        /// </summary>
        public static void Start(
            AndroidView anyAttachedView,
            View source,
            DataPackage data,
            Point position,
            float density)
        {
            var clip = (null != data.Uri)
                ? new ClipData("XForma", new[] { ClipDescription.MimetypeTextUrilist }, new ClipData.Item(AndroidUri.Parse(data.Uri.AbsoluteUri)))
                : ClipData.NewPlainText("XForma", data.Text ?? string.Empty);

            var size = LayerHost.For(source).Size;
            var shadow = new BoxShadow((int)(size.Width * density), (int)(size.Height * density), (int)(position.X * density), (int)(position.Y * density));
            bool isStarted = anyAttachedView.StartDragAndDrop(clip, shadow, null, (int)(DragFlags.Global | DragFlags.GlobalUriRead));
            if (!isStarted)
            {
                ApplicationHost.Current?.EndDrag(DragOperation.None);
            }
        }

        /// <summary>
        /// A drag event for a layer's native view, whose view is
        /// <paramref name="owner"/>. Returns whether the view takes it.
        /// </summary>
        public static bool OnDragEvent(
            View owner,
            DragEvent e,
            float density)
        {
            var host = ApplicationHost.Current;
            if (null == host)
            {
                return false;
            }

            switch (e.Action)
            {
                case DragAction.Started:
                    return true;

                case DragAction.Entered:
                case DragAction.Location:
                    {
                        var (deepest, point) = Find(owner, e, density);
                        return DragOperation.None != host.UpdateDrag(deepest, point, null);
                    }

                case DragAction.Exited:
                    host.ExitDrag();
                    return true;

                case DragAction.Drop:
                    {
                        var (deepest, point) = Find(owner, e, density);
                        return DragOperation.None != host.DropDrag(deepest, point, ReadData(e));
                    }

                case DragAction.Ended:
                    // Every view hears of it; only the first call does anything
                    host.EndDrag(e.Result ? DragOperation.Copy : DragOperation.None);
                    return true;

                default:
                    return false;
            }
        }

        private static (View View, Point Point) Find(
            View owner,
            DragEvent e,
            float density)
        {
            var point = new Point(e.GetX() * density, e.GetY() * density);
            var layer = LayerHost.For(owner);
            var deepest = layer.HitTestPointerOver(point);
            var offset = (deepest == owner) ? default : LayerHost.For(deepest).OffsetInLayer;
            return (deepest, new Point(point.X - offset.X, point.Y - offset.Y));
        }

        private static DataPackage ReadData(
            DragEvent e)
        {
            var package = new DataPackage();
            var clip = e.ClipData;
            if (null == clip)
            {
                return package;
            }

            var activity = AndroidApplication.Current?.Activity;
            var files = new List<DroppedFile>();
            for (int i = 0; i < clip.ItemCount; i += 1)
            {
                var item = clip.GetItemAt(i);
                if (null != item.Text)
                {
                    package.Text = item.Text;
                }

                var uri = item.Uri;
                if (null == uri)
                {
                    continue;
                }

                if ((uri.Scheme == "content") || (uri.Scheme == "file"))
                {
                    activity?.RequestDragAndDropPermissions(e);
                    files.Add(new ContentFile(activity, uri));
                }
                else if (Uri.TryCreate(uri.ToString(), UriKind.Absolute, out var parsed))
                {
                    package.Uri = parsed;
                }
            }

            package.Files = files;
            return package;
        }

        private sealed class ContentFile : DroppedFile
        {
            private readonly Activity _activity;
            private readonly AndroidUri _uri;

            public ContentFile(
                Activity activity,
                AndroidUri uri)
            {
                this._activity = activity;
                this._uri = uri;
            }

            public override string Name => this._uri.LastPathSegment;

            public override Task<Stream> OpenReadAsync()
            {
                return Task.FromResult(this._activity.ContentResolver.OpenInputStream(this._uri));
            }
        }

        private sealed class BoxShadow : AndroidView.DragShadowBuilder
        {
            private readonly int _width;
            private readonly int _height;
            private readonly int _x;
            private readonly int _y;

            public BoxShadow(
                int width,
                int height,
                int x,
                int y)
            {
                this._width = Math.Max(1, width);
                this._height = Math.Max(1, height);
                this._x = Math.Clamp(x, 0, this._width);
                this._y = Math.Clamp(y, 0, this._height);
            }

            public override void OnProvideShadowMetrics(
                global::Android.Graphics.Point outShadowSize,
                global::Android.Graphics.Point outShadowTouchPoint)
            {
                outShadowSize.Set(this._width, this._height);
                outShadowTouchPoint.Set(this._x, this._y);
            }

            public override void OnDrawShadow(
                Canvas canvas)
            {
                using var paint = new Paint();
                paint.Color = global::Android.Graphics.Color.Argb(0x60, 0x80, 0x80, 0x80);
                canvas.DrawRect(0, 0, this._width, this._height, paint);
            }
        }
    }
}
