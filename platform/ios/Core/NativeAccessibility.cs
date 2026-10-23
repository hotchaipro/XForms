using System;
using System.Collections.Generic;
using CoreGraphics;
using Foundation;
using UIKit;
using XForms.Platform;

namespace XForms.iOS
{
    /// <summary>
    /// Describes XForms views to VoiceOver: their labels, values and traits,
    /// and an element for each accessible lightweight view inside a layer.
    /// </summary>
    internal static class NativeAccessibility
    {
        private static bool _isListening;

        public static string LabelFor(
            View view)
        {
            string label = view.AccessibilityLabel;
            if (string.IsNullOrEmpty(label) && (view.AccessibilityRole == AccessibilityRole.ListItem))
            {
                // Named by the text inside it
                return LayerHost.For(view).AccessibilityText;
            }

            return label;
        }

        public static string ValueFor(
            View view)
        {
            // A picker may be labeled by its value
            string value = view.AccessibilityValue;
            if ((null == value) && (view.AccessibilityRange is AccessibilityRange range))
            {
                // A range without its own text is read as a percentage
                double span = range.Maximum - range.Minimum;
                value = (span > 0) ? ((range.Value - range.Minimum) / span).ToString("P0", System.Globalization.CultureInfo.CurrentCulture) : null;
            }
            else if (view.AccessibilityIsMixed)
            {
                value = "Mixed";
            }
            else if ((null == value) && IsToggle(view) && (view.AccessibilityIsChecked is bool isOn))
            {
                // A switch's state, which VoiceOver reads as on or off
                value = isOn ? "1" : "0";
            }

            return (value == view.AccessibilityLabel) ? null : value;
        }

        // UIKit's toggle button trait (iOS 17), which isn't bound: a switch
        // reads as one, with its state as its value
        private static readonly UIAccessibilityTrait ToggleButtonTrait = LoadToggleButtonTrait();

        private static UIAccessibilityTrait LoadToggleButtonTrait()
        {
            if (!UIDevice.CurrentDevice.CheckSystemVersion(17, 0))
            {
                return UIAccessibilityTrait.None;
            }

            var uiKit = ObjCRuntime.Dlfcn.dlopen(ObjCRuntime.Constants.UIKitLibrary, 0);
            return (UIAccessibilityTrait)ObjCRuntime.Dlfcn.GetUInt64(uiKit, "UIAccessibilityTraitToggleButton");
        }

        private static bool IsToggle(
            View view)
        {
            return (view.AccessibilityRole == AccessibilityRole.Switch) && (ToggleButtonTrait != UIAccessibilityTrait.None);
        }

        /// <summary>
        /// Whether the role is a container of other elements (a list, a menu,
        /// a tree, a group): VoiceOver reaches what's inside, so it isn't an
        /// element itself.
        /// </summary>
        public static bool IsContainer(
            AccessibilityRole role)
        {
            return (role == AccessibilityRole.List) || (role == AccessibilityRole.Group) || (role == AccessibilityRole.Menu) || (role == AccessibilityRole.Tree);
        }

        /// <summary>
        /// UIAccessibilityExpandedStatus (iOS 18): unsupported (0), expanded
        /// (1) or collapsed (2).
        /// </summary>
        public static nint ExpandedStatusFor(
            View view)
        {
            return (view.AccessibilityIsExpanded is bool isExpanded) ? (isExpanded ? 1 : 2) : 0;
        }

        /// <summary>
        /// VoiceOver's swipe up and down on an adjustable view.
        /// </summary>
        public static void Adjust(
            View view,
            bool up)
        {
            view.PerformAccessibilityAction(up ? AccessibilityAction.Increment : AccessibilityAction.Decrement);
        }

        public static UIAccessibilityTrait TraitsFor(
            View view)
        {
            var traits = UIAccessibilityTrait.None;
            switch (view.AccessibilityRole)
            {
                case AccessibilityRole.Text:
                    traits |= UIAccessibilityTrait.StaticText;
                    break;

                case AccessibilityRole.Image:
                    traits |= UIAccessibilityTrait.Image;
                    break;

                case AccessibilityRole.Heading:
                    traits |= UIAccessibilityTrait.Header;
                    break;

                case AccessibilityRole.ToggleButton:
                case AccessibilityRole.Button:
                case AccessibilityRole.RadioButton:
                case AccessibilityRole.TabItem:
                case AccessibilityRole.ComboBox:
                case AccessibilityRole.CheckBox:
                case AccessibilityRole.Switch:
                case AccessibilityRole.MenuItem:
                case AccessibilityRole.TreeItem:
                    traits |= UIAccessibilityTrait.Button;
                    break;

                case AccessibilityRole.ToolTip:
                case AccessibilityRole.Alert:
                    traits |= UIAccessibilityTrait.StaticText;
                    break;

                case AccessibilityRole.ProgressBar:
                    traits |= UIAccessibilityTrait.UpdatesFrequently;
                    break;
            }

            // A range the user can step is adjusted with swipes up and down
            if (view.AccessibilityRange.HasValue && view.IsAccessibilityActionSupported(AccessibilityAction.Increment))
            {
                traits |= UIAccessibilityTrait.Adjustable;
            }

            if (IsToggle(view))
            {
                traits |= ToggleButtonTrait;
            }

            // VoiceOver reads a checked check box as selected
            if (view.AccessibilityIsSelected || ((view.AccessibilityIsChecked == true) && !IsToggle(view)))
            {
                traits |= UIAccessibilityTrait.Selected;
            }

            if (!view.IsHitTestVisible)
            {
                traits |= UIAccessibilityTrait.NotEnabled;
            }

            return traits;
        }

        /// <summary>
        /// The elements inside a layer: one for each accessible lightweight
        /// view (whose own lightweight children it names), then the layer's
        /// subviews.
        /// </summary>
        public static NSObject[] ElementsFor(
            NativeLayoutView container,
            View layer)
        {
            if (!_isListening)
            {
                _isListening = true;
                LayerHost.AccessibilityStateChanged += (view) => UIAccessibility.PostNotification(UIAccessibilityPostNotification.LayoutChanged, null);
            }

            var elements = new List<NSObject>();
            Collect(container, layer, elements);
            elements.AddRange(container.Subviews);
            return elements.ToArray();
        }

        private static void Collect(
            NativeLayoutView container,
            View parent,
            List<NSObject> elements)
        {
            var children = LayerHost.For(parent).Children;
            for (int i = 0; i < children.Count; i += 1)
            {
                var child = children[i];
                if (!LayerHost.For(child).IsLightweight || !child.IsVisible || LayerHost.For(child).IsHiddenFromAccessibility)
                {
                    continue;
                }

                if ((child.AccessibilityRole != AccessibilityRole.None) && !IsContainer(child.AccessibilityRole))
                {
                    elements.Add(new LightweightElement(container, child));

                    // VoiceOver's elements don't nest: the controls inside an
                    // accessible view (an info bar's buttons) follow it
                    CollectActionable(container, child, elements);
                }
                else
                {
                    Collect(container, child, elements);
                }
            }
        }

        private static void CollectActionable(
            NativeLayoutView container,
            View parent,
            List<NSObject> elements)
        {
            var children = LayerHost.For(parent).Children;
            for (int i = 0; i < children.Count; i += 1)
            {
                var child = children[i];
                if (!LayerHost.For(child).IsLightweight || !child.IsVisible || LayerHost.For(child).IsHiddenFromAccessibility)
                {
                    continue;
                }

                if ((child.AccessibilityRole != AccessibilityRole.None) && LayerHost.For(child).IsAccessibilityActionable)
                {
                    elements.Add(new LightweightElement(container, child));
                }

                CollectActionable(container, child, elements);
            }
        }

        /// <summary>
        /// The element for a lightweight view.
        /// </summary>
        private sealed class LightweightElement : UIAccessibilityElement
        {
            private readonly WeakReference<View> _view;

            public LightweightElement(
                NativeLayoutView container,
                View view)
                : base(container)
            {
                this._view = new WeakReference<View>(view);
                this._container = new WeakReference<NativeLayoutView>(container);
                this.IsAccessibilityElement = true;
            }

            private readonly WeakReference<NativeLayoutView> _container;

            // Where the view is now, on the screen: from its place in its
            // layer's view, converted through that view (so a scroll view's
            // offset and insets count, and later layout too)
            public override CGRect AccessibilityFrame
            {
                get
                {
                    if (!this._view.TryGetTarget(out var view) || !this._container.TryGetTarget(out var container))
                    {
                        return CGRect.Empty;
                    }

                    var offset = LayerHost.For(view).OffsetInLayer;
                    var size = view.Bounds;
                    return UIAccessibility.ConvertFrameToScreenCoordinates(new CGRect(offset.X, offset.Y, size.Width, size.Height), container);
                }

                set
                {
                }
            }

            public override string AccessibilityLabel
            {
                get
                {
                    return this._view.TryGetTarget(out var view) ? LabelFor(view) : null;
                }

                set
                {
                }
            }

            public override string AccessibilityValue
            {
                get
                {
                    return this._view.TryGetTarget(out var view) ? ValueFor(view) : null;
                }

                set
                {
                }
            }

            public override ulong AccessibilityTraits
            {
                get
                {
                    return (ulong)(this._view.TryGetTarget(out var view) ? TraitsFor(view) : UIAccessibilityTrait.None);
                }

                set
                {
                }
            }

            [Export("accessibilityExpandedStatus")]
            public nint ExpandedStatus
            {
                get
                {
                    return this._view.TryGetTarget(out var view) ? ExpandedStatusFor(view) : 0;
                }
            }

            [Export("accessibilityActivate")]
            public bool Activate()
            {
                return this._view.TryGetTarget(out var view) && LayerHost.For(view).IsAccessibilityActionable && LayerHost.For(view).PerformDefaultAccessibilityAction();
            }

            [Export("accessibilityIncrement")]
            public void Increment()
            {
                if (this._view.TryGetTarget(out var view))
                {
                    Adjust(view, up: true);
                }
            }

            [Export("accessibilityDecrement")]
            public void Decrement()
            {
                if (this._view.TryGetTarget(out var view))
                {
                    Adjust(view, up: false);
                }
            }
        }
    }
}
