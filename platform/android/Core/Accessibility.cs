using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Android.OS;
using Android.Views.Accessibility;
using AndroidView = Android.Views.View;
using NodeAction = Android.Views.Accessibility.Action;
using XForms.Platform;

namespace XForms.Android
{
    /// <summary>
    /// Describes XForms views to TalkBack and other accessibility services:
    /// their roles, labels, values and checked and selected states, and the
    /// actions they support.
    /// </summary>
    internal static class AndroidAccessibility
    {
        private static bool _isListening;

        public static void Attach(
            AndroidView nativeView,
            View view)
        {
            nativeView.SetAccessibilityDelegate(new ViewAccessibilityDelegate(view));
            ApplyHidden(nativeView, view);

            if (!_isListening)
            {
                _isListening = true;
                LayerHost.AccessibilityStateChanged += OnStateChanged;
            }
        }

        public static bool IsEnabled(
            global::Android.Content.Context context)
        {
            var manager = (AccessibilityManager)context.GetSystemService(global::Android.Content.Context.AccessibilityService);
            return manager?.IsEnabled ?? false;
        }

        public static string ClassFor(
            AccessibilityRole role)
        {
            switch (role)
            {
                case AccessibilityRole.Text: return "android.widget.TextView";
                case AccessibilityRole.Button: return "android.widget.Button";
                case AccessibilityRole.ToggleButton: return "android.widget.ToggleButton";
                case AccessibilityRole.RadioButton: return "android.widget.RadioButton";
                case AccessibilityRole.TabItem: return "android.app.ActionBar$Tab";
                case AccessibilityRole.ComboBox: return "android.widget.Spinner";
                case AccessibilityRole.List: return "android.widget.ListView";
                case AccessibilityRole.Image: return "android.widget.ImageView";
                case AccessibilityRole.TextBox: return "android.widget.EditText";
                case AccessibilityRole.Heading: return "android.widget.TextView";
                case AccessibilityRole.CheckBox: return "android.widget.CheckBox";
                case AccessibilityRole.Switch: return "android.widget.Switch";
                case AccessibilityRole.Slider: return "android.widget.SeekBar";
                case AccessibilityRole.ProgressBar: return "android.widget.ProgressBar";
                case AccessibilityRole.SpinButton: return "android.widget.NumberPicker";
                case AccessibilityRole.ToolTip: return "android.widget.TextView";
                case AccessibilityRole.Alert: return "android.widget.TextView";
                case AccessibilityRole.Menu: return "android.widget.ListView";
                case AccessibilityRole.MenuItem: return "android.widget.Button";
                case AccessibilityRole.Tree: return "android.widget.ListView";
                default: return "android.view.ViewGroup";
            }
        }



        /// <summary>
        /// Fills in a node for an accessible view.
        /// </summary>
        public static void Populate(
            AccessibilityNodeInfo info,
            View view)
        {
            var role = view.AccessibilityRole;
            info.ClassName = ClassFor(role);

            // NOTE: TalkBack reads a list item's children itself
            string text = (role == AccessibilityRole.ListItem) ? view.AccessibilityLabel : LayerHost.For(view).AccessibilityText;
            if (!string.IsNullOrEmpty(text))
            {
                if (role == AccessibilityRole.Image)
                {
                    info.ContentDescription = text;
                }
                else if (role != AccessibilityRole.TextBox)
                {
                    // A text box's native view reports its own text
                    info.Text = text;
                }
            }

            if (view.AccessibilityIsChecked is bool isChecked)
            {
                info.Checkable = true;
#pragma warning disable CA1422 // Replaced by a tri-state in Android 16
                info.Checked = isChecked && !view.AccessibilityIsMixed;
#pragma warning restore CA1422
                if (view.AccessibilityIsMixed && OperatingSystem.IsAndroidVersionAtLeast(30))
                {
                    info.StateDescription = "Partially checked";
                }
            }

            // A range: TalkBack's adjust gestures scroll it forward and back
            if (view.AccessibilityRange is AccessibilityRange range)
            {
#pragma warning disable CA1422 // Obtain is replaced by a constructor in Android 16
                info.SetRangeInfo(AccessibilityNodeInfo.RangeInfo.Obtain(RangeType.Float, (float)range.Minimum, (float)range.Maximum, (float)range.Value));
#pragma warning restore CA1422
                if (view.IsAccessibilityActionSupported(AccessibilityAction.Increment))
                {
                    info.AddAction(AccessibilityNodeInfo.AccessibilityAction.ActionScrollForward);
                    info.AddAction(AccessibilityNodeInfo.AccessibilityAction.ActionScrollBackward);
                    if (OperatingSystem.IsAndroidVersionAtLeast(24))
                    {
                        info.AddAction(AccessibilityNodeInfo.AccessibilityAction.ActionSetProgress);
                    }
                }
            }

            // An alert is read as it appears or changes
            if (view.AccessibilityRole == AccessibilityRole.Alert)
            {
                info.LiveRegion = global::Android.Views.AccessibilityLiveRegion.Polite;
            }

            if (view.IsAccessibilityActionSupported(AccessibilityAction.Select))
            {
                info.Selected = view.AccessibilityIsSelected;
            }

            if (view.AccessibilityIsExpanded is bool isExpanded)
            {
                info.AddAction(isExpanded ? AccessibilityNodeInfo.AccessibilityAction.ActionCollapse : AccessibilityNodeInfo.AccessibilityAction.ActionExpand);
            }

            if ((role == AccessibilityRole.Heading) && OperatingSystem.IsAndroidVersionAtLeast(28))
            {
                info.Heading = true;
            }

            if (LayerHost.For(view).IsAccessibilityActionable)
            {
                info.Clickable = true;
                info.AddAction(AccessibilityNodeInfo.AccessibilityAction.ActionClick);
            }

            info.Enabled = view.IsHitTestVisible;
        }



        /// <summary>
        /// Steps or sets a range's value: TalkBack's scroll forward and back,
        /// and set progress. Returns whether the action is a range's.
        /// </summary>
        public static bool PerformRangeAction(
            View view,
            NodeAction action,
            Bundle arguments,
            out bool handled)
        {
            handled = false;
            if (!view.AccessibilityRange.HasValue)
            {
                return false;
            }

            if ((int)action == global::Android.Resource.Id.AccessibilityActionSetProgress)
            {
                if (arguments?.ContainsKey(AccessibilityNodeInfo.ActionArgumentProgressValue) ?? false)
                {
                    handled = view.SetAccessibilityRangeValue(arguments.GetFloat(AccessibilityNodeInfo.ActionArgumentProgressValue));
                }

                return true;
            }

            switch (action)
            {
                case NodeAction.ScrollForward:
                    handled = view.PerformAccessibilityAction(AccessibilityAction.Increment);
                    return true;

                case NodeAction.ScrollBackward:
                    handled = view.PerformAccessibilityAction(AccessibilityAction.Decrement);
                    return true;

                default:
                    return false;
            }
        }

        // A view's checked or selected state, or its value, changed
        private static void OnStateChanged(
            View view)
        {
            if (LayerHost.For(view).IsLightweight)
            {
                var layer = LayerHost.For(view).Layer?.NativeElement as NativeLayoutView;
                layer?.NodeProvider?.NotifyChanged(view);
                return;
            }

            if (view.Renderer?.NativeElement is AndroidView nativeView)
            {
                ApplyHidden(nativeView, view);
                if (IsEnabled(nativeView.Context))
                {
                    nativeView.SendAccessibilityEvent(view.AccessibilityIsSelected ? EventTypes.ViewSelected : EventTypes.WindowContentChanged);
                }
            }
        }

        // A view hidden from assistive technology is, with everything in it
        private static void ApplyHidden(
            AndroidView nativeView,
            View view)
        {
            nativeView.ImportantForAccessibility = view.AccessibilityIsHidden
                ? global::Android.Views.ImportantForAccessibility.NoHideDescendants
                : global::Android.Views.ImportantForAccessibility.Auto;
        }
    }

    /// <summary>
    /// Describes a native view's XForms view, and exposes the lightweight
    /// views inside a layer.
    /// </summary>
    internal sealed class ViewAccessibilityDelegate : AndroidView.AccessibilityDelegate
    {
        private readonly WeakReference<View> _view;

        public ViewAccessibilityDelegate(
            View view)
        {
            this._view = new WeakReference<View>(view);
        }

        public override void OnInitializeAccessibilityNodeInfo(
            AndroidView host,
            AccessibilityNodeInfo info)
        {
            base.OnInitializeAccessibilityNodeInfo(host, info);

            if (this._view.TryGetTarget(out var hidden) && LayerHost.For(hidden).IsHiddenFromAccessibility)
            {
                info.VisibleToUser = false;
                return;
            }

            if (this._view.TryGetTarget(out var view) && (view.AccessibilityRole != AccessibilityRole.None))
            {
                AndroidAccessibility.Populate(info, view);
            }
        }

        public override bool PerformAccessibilityAction(
            AndroidView host,
            NodeAction action,
            Bundle args)
        {
            if ((action == NodeAction.Click) && this._view.TryGetTarget(out var view) && LayerHost.For(view).IsAccessibilityActionable)
            {
                if (LayerHost.For(view).PerformDefaultAccessibilityAction())
                {
                    host.SendAccessibilityEvent(EventTypes.ViewClicked);
                    return true;
                }

                return false;
            }

            if (((action == NodeAction.Expand) || (action == NodeAction.Collapse)) && this._view.TryGetTarget(out var expandable))
            {
                return expandable.PerformAccessibilityAction((action == NodeAction.Expand) ? AccessibilityAction.Expand : AccessibilityAction.Collapse);
            }

            if (this._view.TryGetTarget(out var ranged) && AndroidAccessibility.PerformRangeAction(ranged, action, args, out bool rangeHandled))
            {
                return rangeHandled;
            }

            return base.PerformAccessibilityAction(host, action, args);
        }

        public override AccessibilityNodeProvider GetAccessibilityNodeProvider(
            AndroidView host)
        {
            // A layer's lightweight views are virtual nodes inside it
            if ((host is NativeLayoutView layer) && LayerHost.For(layer.Owner).HasLightweightChildren)
            {
                return layer.GetOrCreateNodeProvider();
            }

            return base.GetAccessibilityNodeProvider(host);
        }
    }

    /// <summary>
    /// Exposes the accessible lightweight views drawn into a layer as virtual
    /// nodes, as Android's ExploreByTouchHelper does for custom views.
    /// </summary>
    internal sealed class LayerNodeProvider : AccessibilityNodeProvider
    {
        private const int InvalidId = int.MinValue;

        private readonly NativeLayoutView _host;
        private readonly ConditionalWeakTable<View, object> _ids = new ConditionalWeakTable<View, object>();
        private readonly Dictionary<int, WeakReference<View>> _views = new Dictionary<int, WeakReference<View>>();
        private int _nextId;
        private int _pruneAt = 64;
        private int _focusedId = InvalidId;
        private int _hoveredId = InvalidId;

        public LayerNodeProvider(
            NativeLayoutView host)
        {
            this._host = host;
        }

        private int IdOf(
            View view)
        {
            var box = this._ids.GetValue(view, (v) =>
            {
                int id = this._nextId++;

                // Drops the views that are gone (list rows come and go)
                if (this._views.Count >= this._pruneAt)
                {
                    foreach (var entry in new List<KeyValuePair<int, WeakReference<View>>>(this._views))
                    {
                        if (!entry.Value.TryGetTarget(out _))
                        {
                            this._views.Remove(entry.Key);
                        }
                    }

                    this._pruneAt = Math.Max(64, this._views.Count * 2);
                }

                this._views[id] = new WeakReference<View>(v);
                return id;
            });

            return (int)box;
        }

        private View ViewOf(
            int id)
        {
            if (this._views.TryGetValue(id, out var reference) && reference.TryGetTarget(out var view)
                && LayerHost.For(view).IsLightweight && (LayerHost.For(view).Layer?.NativeElement == this._host))
            {
                return view;
            }

            return null;
        }

        /// <summary>
        /// The accessible lightweight views directly inside the view (skipping
        /// views that aren't accessible themselves), in drawing order, and,
        /// for the layer, the native views among them.
        /// </summary>
        private static void CollectAccessible(
            View parent,
            List<object> result,
            bool includeNative)
        {
            var children = LayerHost.For(parent).Children;
            for (int i = 0; i < children.Count; i += 1)
            {
                var child = children[i];
                if (!child.IsVisible || LayerHost.For(child).IsHiddenFromAccessibility)
                {
                    continue;
                }

                if (!LayerHost.For(child).IsLightweight)
                {
                    if (includeNative && (child.CreatedRenderer?.NativeElement is AndroidView nativeView))
                    {
                        result.Add(nativeView);
                    }
                }
                else if (child.AccessibilityRole != AccessibilityRole.None)
                {
                    result.Add(child);
                }
                else
                {
                    CollectAccessible(child, result, includeNative);
                }
            }
        }

        private void AddChildren(
            AccessibilityNodeInfo node,
            View parent,
            bool includeNative)
        {
            var children = new List<object>();
            CollectAccessible(parent, children, includeNative);
            foreach (var child in children)
            {
                if (child is View view)
                {
                    node.AddChild(this._host, this.IdOf(view));
                }
                else if (((AndroidView)child).Parent == this._host)
                {
                    node.AddChild((AndroidView)child);
                }
            }
        }

        // The view's place in the layer, in pixels
        private global::Android.Graphics.Rect BoundsInHost(
            View view)
        {
            var context = this._host.Context;
            var offset = LayerHost.For(view).OffsetInLayer;
            var size = view.Bounds;
            return new global::Android.Graphics.Rect(
                (int)NativeConversions.DevicePixelsToPixels(context, offset.X),
                (int)NativeConversions.DevicePixelsToPixels(context, offset.Y),
                (int)NativeConversions.DevicePixelsToPixels(context, offset.X + size.Width),
                (int)NativeConversions.DevicePixelsToPixels(context, offset.Y + size.Height));
        }

#pragma warning disable CA1422 // Obtain and the parent bounds are deprecated, but are what older versions have
        public override AccessibilityNodeInfo CreateAccessibilityNodeInfo(
            int virtualViewId)
        {
            if (virtualViewId == HostViewId)
            {
                // The layer itself, with its lightweight views and its native
                // children in reading order (a view group with a node
                // provider leaves its native children out)
                var hostNode = AccessibilityNodeInfo.Obtain(this._host);
                this._host.OnInitializeAccessibilityNodeInfo(hostNode);
                this.AddChildren(hostNode, this._host.Owner, includeNative: true);
                return hostNode;
            }

            var view = this.ViewOf(virtualViewId);
            if (null == view)
            {
                return null;
            }

            var node = AccessibilityNodeInfo.Obtain(this._host, virtualViewId);
            node.PackageName = this._host.Context.PackageName;

            // Its parent is the nearest accessible lightweight ancestor, or the
            // layer
            var parent = view.Parent as View;
            while ((null != parent) && LayerHost.For(parent).IsLightweight && (parent.AccessibilityRole == AccessibilityRole.None))
            {
                parent = parent.Parent as View;
            }

            if ((null != parent) && LayerHost.For(parent).IsLightweight)
            {
                node.SetParent(this._host, this.IdOf(parent));
            }
            else
            {
                node.SetParent(this._host);
            }

            var bounds = this.BoundsInHost(view);
            node.SetBoundsInParent(bounds);
            int[] location = new int[2];
            this._host.GetLocationOnScreen(location);
            bounds.Offset(location[0], location[1]);
            node.SetBoundsInScreen(bounds);

            node.VisibleToUser = this._host.IsShown;
            AndroidAccessibility.Populate(node, view);
            node.Focusable = view.Focusable;

            if (virtualViewId == this._focusedId)
            {
                node.AccessibilityFocused = true;
                node.AddAction(AccessibilityNodeInfo.AccessibilityAction.ActionClearAccessibilityFocus);
            }
            else
            {
                node.AddAction(AccessibilityNodeInfo.AccessibilityAction.ActionAccessibilityFocus);
            }

            this.AddChildren(node, view, includeNative: false);
            return node;
        }

        private void Send(
            int virtualViewId,
            EventTypes type)
        {
            if (!AndroidAccessibility.IsEnabled(this._host.Context) || (null == this._host.Parent))
            {
                return;
            }

            var e = AccessibilityEvent.Obtain(type);
            e.PackageName = this._host.Context.PackageName;
            e.SetSource(this._host, virtualViewId);

            var view = this.ViewOf(virtualViewId);
            if (null != view)
            {
                e.ClassName = AndroidAccessibility.ClassFor(view.AccessibilityRole);
                string text = LayerHost.For(view).AccessibilityText;
                if (!string.IsNullOrEmpty(text))
                {
                    e.Text.Add(new Java.Lang.String(text));
                }

                if (view.AccessibilityIsChecked is bool isChecked)
                {
                    e.Checked = isChecked && !view.AccessibilityIsMixed;
                }

                e.Enabled = view.IsHitTestVisible;
            }

            if (type == EventTypes.WindowContentChanged)
            {
                e.ContentChangeTypes = ContentChangeTypes.Undefined;
            }

            this._host.Parent.RequestSendAccessibilityEvent(this._host, e);
        }
#pragma warning restore CA1422

        public override bool PerformAction(
            int virtualViewId,
            NodeAction action,
            Bundle arguments)
        {
            if (virtualViewId == HostViewId)
            {
                return this._host.PerformAccessibilityAction(action, arguments);
            }

            var view = this.ViewOf(virtualViewId);
            if (null == view)
            {
                return false;
            }

            switch (action)
            {
                case NodeAction.AccessibilityFocus:
                    if (this._focusedId != virtualViewId)
                    {
                        if (this._focusedId != InvalidId)
                        {
                            this.Send(this._focusedId, EventTypes.ViewAccessibilityFocusCleared);
                        }

                        this._focusedId = virtualViewId;
                        this.Send(virtualViewId, EventTypes.ViewAccessibilityFocused);
                    }

                    return true;

                case NodeAction.ClearAccessibilityFocus:
                    if (this._focusedId == virtualViewId)
                    {
                        this._focusedId = InvalidId;
                        this.Send(virtualViewId, EventTypes.ViewAccessibilityFocusCleared);
                    }

                    return true;

                case NodeAction.Click:
                    if (LayerHost.For(view).IsAccessibilityActionable && LayerHost.For(view).PerformDefaultAccessibilityAction())
                    {
                        this.Send(virtualViewId, EventTypes.ViewClicked);
                        return true;
                    }

                    return false;

                case NodeAction.Expand:
                    return view.PerformAccessibilityAction(AccessibilityAction.Expand);

                case NodeAction.Collapse:
                    return view.PerformAccessibilityAction(AccessibilityAction.Collapse);

                default:
                    return AndroidAccessibility.PerformRangeAction(view, action, arguments, out bool handled) && handled;
            }
        }

        /// <summary>
        /// With touch exploration on (TalkBack), a finger over the layer
        /// announces the lightweight view under it. Returns whether a
        /// lightweight view took the event.
        /// </summary>
        public bool DispatchHover(
            global::Android.Views.MotionEvent e)
        {
            var manager = (AccessibilityManager)this._host.Context.GetSystemService(global::Android.Content.Context.AccessibilityService);
            if ((null == manager) || !manager.IsEnabled || !manager.IsTouchExplorationEnabled)
            {
                return false;
            }

            int id = InvalidId;
            if (e.Action != global::Android.Views.MotionEventActions.HoverExit)
            {
                var point = new Point(
                    NativeConversions.PixelsToDevicePixels(this._host.Context, e.GetX()),
                    NativeConversions.PixelsToDevicePixels(this._host.Context, e.GetY()));
                for (var view = LayerHost.For(this._host.Owner).HitTestLightweight(point); (null != view) && LayerHost.For(view).IsLightweight; view = view.Parent as View)
                {
                    if (view.AccessibilityRole != AccessibilityRole.None)
                    {
                        id = this.IdOf(view);
                        break;
                    }
                }
            }

            if (id != this._hoveredId)
            {
                int previous = this._hoveredId;
                this._hoveredId = id;
                if (id != InvalidId)
                {
                    this.Send(id, EventTypes.ViewHoverEnter);
                }

                if (previous != InvalidId)
                {
                    this.Send(previous, EventTypes.ViewHoverExit);
                }
            }

            return id != InvalidId;
        }

        /// <summary>
        /// A lightweight view's state or value changed.
        /// </summary>
        public void NotifyChanged(
            View view)
        {
            int id = this.IdOf(view);
            this.Send(id, view.AccessibilityIsSelected ? EventTypes.ViewSelected : EventTypes.WindowContentChanged);
        }

        /// <summary>
        /// The lightweight views inside the layer moved, or were added or
        /// removed.
        /// </summary>
        public void NotifyLayoutChanged()
        {
            if (!AndroidAccessibility.IsEnabled(this._host.Context) || (null == this._host.Parent))
            {
                return;
            }

#pragma warning disable CA1422
            var e = AccessibilityEvent.Obtain(EventTypes.WindowContentChanged);
#pragma warning restore CA1422
            e.ContentChangeTypes = ContentChangeTypes.Subtree;
            e.SetSource(this._host);
            this._host.Parent.RequestSendAccessibilityEvent(this._host, e);
        }
    }
}
