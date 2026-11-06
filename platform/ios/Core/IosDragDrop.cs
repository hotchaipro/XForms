using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using CoreGraphics;
using Foundation;
using UIKit;
using XForma.Platform;

namespace XForma.iOS
{
    /// <summary>
    /// iOS's drag and drop, through interactions on the app's root view: iOS
    /// recognizes the long press that starts a drag, and asks here what's dragged.
    /// A drag can go to other apps.
    /// </summary>
    internal static class IosDragDrop
    {
        /// <summary>
        /// Adds the interactions to the app's root view, once.
        /// </summary>
        public static void Install(
            UIView root)
        {
            foreach (var interaction in root.Interactions)
            {
                if (interaction is UIDragInteraction)
                {
                    return;
                }
            }

            var drag = new UIDragInteraction(new DragDelegate()) { Enabled = true };
            root.AddInteraction(drag);
            root.AddInteraction(new UIDropInteraction(new DropDelegate()));
        }

        // The deepest view at the point in the root, and the point in its
        // coordinates
        private static (View View, Point Point) Find(
            UIView root,
            CGPoint location)
        {
            for (var native = root.HitTest(location, null); null != native; native = native.Superview)
            {
                if (native is NativeLayoutView layer)
                {
                    var inLayer = root.ConvertPointToView(location, layer);
                    var point = new Point((float)inLayer.X, (float)inLayer.Y);
                    var deepest = LayerHost.For(layer.Owner).HitTestPointerOver(point);
                    var offset = (deepest == layer.Owner) ? default : LayerHost.For(deepest).OffsetInLayer;
                    return (deepest, new Point(point.X - offset.X, point.Y - offset.Y));
                }
            }

            return (null, default);
        }

        private sealed class DragDelegate : UIDragInteractionDelegate
        {
            private CGRect _dragged;

            public override UIDragItem[] GetItemsForBeginningSession(
                UIDragInteraction interaction,
                IUIDragSession session)
            {
                var host = ApplicationHost.Current;
                var root = interaction.View;
                if ((null == host) || (null == root))
                {
                    return Array.Empty<UIDragItem>();
                }

                var location = session.LocationInView(root);
                var (view, point) = Find(root, location);
                var data = (null != view) ? host.BeginDrag(view) : null;
                if (null == data)
                {
                    return Array.Empty<UIDragItem>();
                }

                var size = LayerHost.For(view).Size;
                this._dragged = new CGRect(location.X - point.X, location.Y - point.Y, size.Width, size.Height);

                NSItemProvider provider;
                if (null != data.Uri)
                {
                    provider = new NSItemProvider(new NSUrl(data.Uri.AbsoluteUri));
                }
                else
                {
                    provider = new NSItemProvider(new NSString(data.Text ?? string.Empty));
                }

                return new[] { new UIDragItem(provider) };
            }

            // A plain box the size of the view
            public override UITargetedDragPreview GetPreviewForLiftingItem(
                UIDragInteraction interaction,
                UIDragItem item,
                IUIDragSession session)
            {
                var root = interaction.View;
                var box = new UIView(new CGRect(0, 0, this._dragged.Width, this._dragged.Height))
                {
                    BackgroundColor = UIColor.SystemGray.ColorWithAlpha(0.4f),
                };
                var center = new CGPoint(this._dragged.GetMidX(), this._dragged.GetMidY());
                return new UITargetedDragPreview(box, new UIDragPreviewParameters(), new UIDragPreviewTarget(root, center));
            }

            public override void SessionDidEnd(
                UIDragInteraction interaction,
                IUIDragSession session,
                UIDropOperation operation)
            {
                ApplicationHost.Current?.EndDrag(
                    (operation == UIDropOperation.Move) ? DragOperation.Move : (operation == UIDropOperation.Copy) ? DragOperation.Copy : DragOperation.None);
            }
        }

        private sealed class DropDelegate : UIDropInteractionDelegate
        {
            public override bool CanHandleSession(
                UIDropInteraction interaction,
                IUIDropSession session)
            {
                return true;
            }

            public override UIDropProposal SessionDidUpdate(
                UIDropInteraction interaction,
                IUIDropSession session)
            {
                var host = ApplicationHost.Current;
                var root = interaction.View;
                if ((null == host) || (null == root))
                {
                    return new UIDropProposal(UIDropOperation.Forbidden);
                }

                var (view, point) = Find(root, session.LocationInView(root));
                var operation = host.UpdateDrag(view, point, null);
                return new UIDropProposal((operation == DragOperation.Move) ? UIDropOperation.Move : (operation == DragOperation.Copy) ? UIDropOperation.Copy : UIDropOperation.Forbidden);
            }

            public override void SessionDidExit(
                UIDropInteraction interaction,
                IUIDropSession session)
            {
                ApplicationHost.Current?.ExitDrag();
            }

            public override void PerformDrop(
                UIDropInteraction interaction,
                IUIDropSession session)
            {
                var root = interaction.View;
                var (view, point) = Find(root, session.LocationInView(root));
                var package = new DataPackage();
                var files = new List<DroppedFile>();

                // A drag from this app has its own data already; another's is read
                // first (iOS hands it over asynchronously)
                var remaining = new List<Task>();
                foreach (var item in session.Items)
                {
                    var provider = item.ItemProvider;
                    if (provider.CanLoadObject(typeof(NSString)))
                    {
                        var completion = new TaskCompletionSource();
                        provider.LoadObject<NSString>((text, error) =>
                        {
                            package.Text ??= text?.ToString();
                            completion.TrySetResult();
                        });
                        remaining.Add(completion.Task);
                    }

                    if (provider.CanLoadObject(typeof(NSUrl)))
                    {
                        var completion = new TaskCompletionSource();
                        provider.LoadObject<NSUrl>((url, error) =>
                        {
                            if ((null != url) && Uri.TryCreate(url.AbsoluteString, UriKind.Absolute, out var parsed))
                            {
                                package.Uri ??= parsed;
                            }

                            completion.TrySetResult();
                        });
                        remaining.Add(completion.Task);
                    }

                    if (0 < provider.RegisteredTypeIdentifiers.Length)
                    {
                        var completion = new TaskCompletionSource();
                        string type = provider.RegisteredTypeIdentifiers[0];
                        provider.LoadFileRepresentation(type, (url, error) =>
                        {
                            // The file goes when this returns: it's copied
                            if ((null != url) && url.IsFileUrl)
                            {
                                string copy = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + Path.GetExtension(url.Path));
                                File.Copy(url.Path, copy);
                                lock (files)
                                {
                                    files.Add(new CopiedFile(provider.SuggestedName ?? Path.GetFileName(url.Path), copy));
                                }
                            }

                            completion.TrySetResult();
                        });
                        remaining.Add(completion.Task);
                    }
                }

                _ = FinishAsync(remaining, view, point, package, files);
            }

            private static async Task FinishAsync(
                List<Task> remaining,
                View view,
                Point point,
                DataPackage package,
                List<DroppedFile> files)
            {
                try
                {
                    await Task.WhenAll(remaining);
                    package.Files = files;
                    ApplicationHost.Current?.DropDrag(view, point, package);
                }
                catch (Exception ex)
                {
                    Application.ReportException(ex);
                }
            }
        }

        private sealed class CopiedFile : DroppedFile
        {
            private readonly string _path;

            public CopiedFile(
                string name,
                string path)
            {
                this.Name = name;
                this._path = path;
            }

            public override string Name
            {
                get;
            }

            public override Task<Stream> OpenReadAsync()
            {
                return Task.FromResult<Stream>(File.OpenRead(this._path));
            }
        }
    }
}
