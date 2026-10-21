using System;
using System.Collections.Generic;
using CoreGraphics;
using Foundation;
using UIKit;

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
                return view.AccessibilityText;
            }

            return label;
        }

        public static string ValueFor(
            View view)
        {
            // A picker may be labeled by its value
            string value = view.AccessibilityValue;
            return (value == view.AccessibilityLabel) ? null : value;
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
                    traits |= UIAccessibilityTrait.Button;
                    break;
            }

            // VoiceOver reads a checked toggle as selected
            if (view.AccessibilityIsSelected || (view.AccessibilityIsChecked == true))
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
                View.AccessibilityStateChanged += (view) => UIAccessibility.PostNotification(UIAccessibilityPostNotification.LayoutChanged, null);
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
            var children = parent.DrawingOrderChildren;
            for (int i = 0; i < children.Count; i += 1)
            {
                var child = children[i];
                if (!child.IsLightweight || !child.IsVisible || child.IsHiddenFromAccessibility)
                {
                    continue;
                }

                if (child.AccessibilityRole != AccessibilityRole.None)
                {
                    elements.Add(new LightweightElement(container, child));
                }
                else
                {
                    Collect(container, child, elements);
                }
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

                var offset = view.OffsetInLayer;
                var size = view.Bounds;
                this.AccessibilityFrameInContainerSpace = new CGRect(offset.X, offset.Y, size.Width, size.Height);
                this.IsAccessibilityElement = true;
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

            [Export("accessibilityActivate")]
            public bool Activate()
            {
                return this._view.TryGetTarget(out var view) && view.IsAccessibilityActionable && view.PerformDefaultAccessibilityAction();
            }
        }
    }
}
