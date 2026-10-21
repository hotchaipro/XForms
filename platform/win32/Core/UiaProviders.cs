using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using System.Text;
using System.Threading;
using XForms.Win32.Renderers;

namespace XForms.Win32.Automation
{
    /// <summary>
    /// Exposes the XForms view tree to UI Automation (screen readers such as
    /// Narrator). Views with an accessibility role become automation elements;
    /// views without one are skipped and their accessible descendants are
    /// promoted. Native RichEdit text entries expose themselves.
    /// </summary>
    internal static class AutomationTree
    {
        private static readonly StrategyBasedComWrappers ComWrappers = new StrategyBasedComWrappers();
        private static readonly ConditionalWeakTable<ViewRenderer, ElementProvider> Elements = new ConditionalWeakTable<ViewRenderer, ElementProvider>();
        private static int _nextRuntimeId;
        private static RootProvider _root;

        public static RootProvider Root
        {
            get
            {
                if (null == _root)
                {
                    _root = new RootProvider(Win32Host.Current);

                    // Once a client has asked for the tree, views tell it when
                    // their states change
                    View.AccessibilityStateChanged += RaiseStateChanged;
                }

                return _root;
            }
        }

        public static ElementProvider GetElement(
            ViewRenderer renderer)
        {
            return Elements.GetValue(renderer, (r) => new ElementProvider(r, Interlocked.Increment(ref _nextRuntimeId)));
        }

        /// <summary>
        /// Returns an owned COM pointer to the object's interface.
        /// </summary>
        public static nint GetInterface<T>(
            object provider)
        {
            if (null == provider)
            {
                return 0;
            }

            nint unknown = ComWrappers.GetOrCreateComInterfaceForObject(provider, CreateComInterfaceFlags.None);
            Marshal.QueryInterface(unknown, typeof(T).GUID, out nint result);
            Marshal.Release(unknown);
            return result;
        }

        public static bool IsAccessible(
            ViewRenderer renderer)
        {
            if (!(renderer.Element is View view) || (view.AccessibilityRole == AccessibilityRole.None) || !renderer.IsShownOnScreen || view.IsHiddenFromAccessibility)
            {
                return false;
            }

            // A visible RichEdit window describes itself
            return !((renderer is TextBoxRenderer textEntry) && textEntry.IsEditWindowShown);
        }

        /// <summary>
        /// The accessible children of an element (or of the window, for null).
        /// </summary>
        public static List<ViewRenderer> GetChildren(
            ViewRenderer parent)
        {
            var children = new List<ViewRenderer>();

            if (null == parent)
            {
                // The window holds the page, then any popups above it
                var host = Win32Host.Current;
                if (null != host.Root)
                {
                    CollectAccessible(host.Root, children);
                }

                foreach (var overlay in host.Overlays.Overlays)
                {
                    CollectAccessible(overlay.Renderer, children);
                }

                return children;
            }

            foreach (var child in parent.Children)
            {
                CollectAccessible(child, children);
            }

            return children;
        }

        private static void CollectAccessible(
            ViewRenderer renderer,
            List<ViewRenderer> result)
        {
            if (!renderer.IsShownOnScreen || ((renderer.Element is View view) && view.IsHiddenFromAccessibility))
            {
                return;
            }

            if (IsAccessible(renderer))
            {
                result.Add(renderer);
                return;
            }

            foreach (var child in renderer.Children)
            {
                CollectAccessible(child, result);
            }
        }

        /// <summary>
        /// The nearest accessible ancestor, or null for the window.
        /// </summary>
        public static ViewRenderer GetParent(
            ViewRenderer renderer)
        {
            for (var parent = renderer.ParentRenderer; null != parent; parent = parent.ParentRenderer)
            {
                if (IsAccessible(parent))
                {
                    return parent;
                }
            }

            return null;
        }

        public static ViewRenderer AccessibleAt(
            Point point)
        {
            Win32Host.Current.HitTest(point, out var hit);
            for (var renderer = hit; null != renderer; renderer = renderer.ParentRenderer)
            {
                if (IsAccessible(renderer))
                {
                    return renderer;
                }
            }

            return null;
        }

        /// <summary>
        /// Tells screen readers that keyboard focus moved to the view.
        /// </summary>
        public static void RaiseFocusChanged(
            ViewRenderer renderer)
        {
            if ((null == renderer) || !IsAccessible(renderer) || !UiaNative.UiaClientsAreListening())
            {
                return;
            }

            nint provider = GetInterface<IRawElementProviderSimple>(GetElement(renderer));
            UiaNative.UiaRaiseAutomationEvent(provider, UiaConstants.AutomationFocusChangedEventId);
            Marshal.Release(provider);
        }

        /// <summary>
        /// Tells screen readers that a view's checked or selected state, or its
        /// value, changed.
        /// </summary>
        private static void RaiseStateChanged(
            View view)
        {
            if (!(view.Renderer is ViewRenderer renderer) || !IsAccessible(renderer) || !UiaNative.UiaClientsAreListening())
            {
                return;
            }

            nint provider = GetInterface<IRawElementProviderSimple>(GetElement(renderer));
            var empty = new Variant();
            if (view.AccessibilityIsChecked is bool isChecked)
            {
                UiaNative.UiaRaiseAutomationPropertyChangedEvent(provider, UiaConstants.ToggleToggleStatePropertyId, empty, Variant.FromInt(isChecked ? UiaConstants.ToggleStateOn : UiaConstants.ToggleStateOff));
            }

            if (view.IsAccessibilityActionSupported(AccessibilityAction.Select))
            {
                UiaNative.UiaRaiseAutomationPropertyChangedEvent(provider, UiaConstants.SelectionItemIsSelectedPropertyId, empty, Variant.FromBool(view.AccessibilityIsSelected));
                if (view.AccessibilityIsSelected)
                {
                    UiaNative.UiaRaiseAutomationEvent(provider, UiaConstants.SelectionItemElementSelectedEventId);
                }
            }

            if (view.AccessibilityIsExpanded is bool isExpanded)
            {
                UiaNative.UiaRaiseAutomationPropertyChangedEvent(provider, UiaConstants.ExpandCollapseExpandCollapseStatePropertyId, empty, Variant.FromInt(isExpanded ? UiaConstants.Expanded : UiaConstants.Collapsed));
            }

            if (null != view.AccessibilityValue)
            {
                var value = Variant.FromString(view.AccessibilityValue);
                UiaNative.UiaRaiseAutomationPropertyChangedEvent(provider, UiaConstants.ValueValuePropertyId, empty, value);
                Marshal.FreeBSTR(value.PointerValue);
            }

            Marshal.Release(provider);
        }

        public static nint Navigate(
            ViewRenderer element,
            int direction)
        {
            switch (direction)
            {
                case UiaConstants.Parent:
                    {
                        var parent = GetParent(element);
                        return (null != parent)
                            ? GetInterface<IRawElementProviderFragment>(GetElement(parent))
                            : GetInterface<IRawElementProviderFragment>(Root);
                    }

                case UiaConstants.NextSibling:
                case UiaConstants.PreviousSibling:
                    {
                        var siblings = GetChildren(GetParent(element));
                        int index = siblings.IndexOf(element) + ((direction == UiaConstants.NextSibling) ? 1 : -1);
                        return ((index >= 0) && (index < siblings.Count))
                            ? GetInterface<IRawElementProviderFragment>(GetElement(siblings[index]))
                            : 0;
                    }

                case UiaConstants.FirstChild:
                case UiaConstants.LastChild:
                    {
                        var children = GetChildren(element);
                        if (children.Count == 0)
                        {
                            return 0;
                        }

                        var child = (direction == UiaConstants.FirstChild) ? children[0] : children[children.Count - 1];
                        return GetInterface<IRawElementProviderFragment>(GetElement(child));
                    }
            }

            return 0;
        }

        /// <summary>
        /// Whether any of the view shows within its ancestors' bounds, such as a
        /// list row that is realized just outside the scrolled viewport.
        /// </summary>
        public static bool IsOnScreen(
            ViewRenderer renderer)
        {
            var origin = renderer.AbsoluteOrigin();
            var visible = new Rect(origin.X, origin.Y, renderer.Frame.Width, renderer.Frame.Height);

            for (var ancestor = renderer.ParentRenderer; null != ancestor; ancestor = ancestor.ParentRenderer)
            {
                var ancestorOrigin = ancestor.AbsoluteOrigin();
                float left = Math.Max(visible.X, ancestorOrigin.X);
                float top = Math.Max(visible.Y, ancestorOrigin.Y);
                float right = Math.Min(visible.X + visible.Width, ancestorOrigin.X + ancestor.Frame.Width);
                float bottom = Math.Min(visible.Y + visible.Height, ancestorOrigin.Y + ancestor.Frame.Height);

                if ((right <= left) || (bottom <= top))
                {
                    return false;
                }

                visible = new Rect(left, top, right - left, bottom - top);
            }

            return true;
        }

        public static UiaRect ScreenBounds(
            Point origin,
            Size size)
        {
            var host = Win32Host.Current;
            var screen = host.ClientToScreen(origin);

            return new UiaRect()
            {
                Left = screen.X,
                Top = screen.Y,
                Width = size.Width * host.Scale,
                Height = size.Height * host.Scale,
            };
        }
    }

    /// <summary>
    /// An automation element for one accessible view.
    /// </summary>
    [GeneratedComClass]
    internal sealed unsafe partial class ElementProvider : IRawElementProviderSimple, IRawElementProviderFragment, IInvokeProvider, IToggleProvider, ISelectionItemProvider, IValueProvider, IExpandCollapseProvider
    {
        private readonly WeakReference<ViewRenderer> _renderer;
        private readonly int _runtimeId;

        public ElementProvider(
            ViewRenderer renderer,
            int runtimeId)
        {
            this._renderer = new WeakReference<ViewRenderer>(renderer);
            this._runtimeId = runtimeId;
        }

        private const int E_ELEMENTNOTAVAILABLE = unchecked((int)0x80040201);

        private int OnUIThread(
            Func<ViewRenderer, int> action)
        {
            return Win32Host.Current.InvokeSync(() =>
            {
                if (!this._renderer.TryGetTarget(out var renderer))
                {
                    return E_ELEMENTNOTAVAILABLE;
                }

                return action(renderer);
            });
        }

        private static int ControlTypeFor(
            AccessibilityRole role)
        {
            switch (role)
            {
                case AccessibilityRole.Button: return UiaConstants.ButtonControlTypeId;
                case AccessibilityRole.ToggleButton: return UiaConstants.CheckBoxControlTypeId;
                case AccessibilityRole.RadioButton: return UiaConstants.RadioButtonControlTypeId;
                case AccessibilityRole.ComboBox: return UiaConstants.ComboBoxControlTypeId;
                case AccessibilityRole.TabItem: return UiaConstants.TabItemControlTypeId;
                case AccessibilityRole.List: return UiaConstants.ListControlTypeId;
                case AccessibilityRole.ListItem: return UiaConstants.ListItemControlTypeId;
                case AccessibilityRole.Image: return UiaConstants.ImageControlTypeId;
                case AccessibilityRole.TextBox: return UiaConstants.EditControlTypeId;
                case AccessibilityRole.Group: return UiaConstants.GroupControlTypeId;
                default: return UiaConstants.TextControlTypeId;
            }
        }

        private static string NameFor(
            ViewRenderer renderer)
        {
            var view = (View)renderer.Element;
            string label = view.AccessibilityLabel;
            if (!string.IsNullOrEmpty(label))
            {
                return label;
            }

            if (view.AccessibilityRole != AccessibilityRole.ListItem)
            {
                return string.Empty;
            }

            // Name a list item by the text inside it
            var text = new StringBuilder();
            AppendText(renderer, text);
            return text.ToString();
        }

        private static void AppendText(
            ViewRenderer renderer,
            StringBuilder text)
        {
            foreach (var child in renderer.Children)
            {
                if (!child.IsShownOnScreen)
                {
                    continue;
                }

                if ((child.Element is View view) && (view.AccessibilityRole == AccessibilityRole.Text) && !string.IsNullOrEmpty(view.AccessibilityLabel))
                {
                    if (text.Length > 0)
                    {
                        text.Append(", ");
                    }

                    text.Append(view.AccessibilityLabel);
                }
                else
                {
                    AppendText(child, text);
                }
            }
        }



        public int GetProviderOptions(out int options)
        {
            options = UiaConstants.ServerSideProvider | UiaConstants.UseComThreading;
            return 0;
        }

        public int GetPatternProvider(int patternId, out nint provider)
        {
            nint result = 0;
            int hr = this.OnUIThread((renderer) =>
            {
                var view = (View)renderer.Element;
                switch (patternId)
                {
                    case UiaConstants.InvokePatternId:
                        if (view.IsAccessibilityActionSupported(AccessibilityAction.Invoke))
                        {
                            result = AutomationTree.GetInterface<IInvokeProvider>(this);
                        }

                        break;

                    case UiaConstants.TogglePatternId:
                        if (view.IsAccessibilityActionSupported(AccessibilityAction.Toggle))
                        {
                            result = AutomationTree.GetInterface<IToggleProvider>(this);
                        }

                        break;

                    case UiaConstants.SelectionItemPatternId:
                        if (view.IsAccessibilityActionSupported(AccessibilityAction.Select))
                        {
                            result = AutomationTree.GetInterface<ISelectionItemProvider>(this);
                        }

                        break;

                    case UiaConstants.ExpandCollapsePatternId:
                        if (view.IsAccessibilityActionSupported(AccessibilityAction.Expand))
                        {
                            result = AutomationTree.GetInterface<IExpandCollapseProvider>(this);
                        }

                        break;

                    case UiaConstants.ValuePatternId:
                        if (null != view.AccessibilityValue)
                        {
                            result = AutomationTree.GetInterface<IValueProvider>(this);
                        }

                        break;
                }

                return 0;
            });

            provider = result;
            return hr;
        }

        public int GetPropertyValue(int propertyId, Variant* value)
        {
            var result = new Variant();
            int hr = this.OnUIThread((renderer) =>
            {
                var view = (View)renderer.Element;

                switch (propertyId)
                {
                    case UiaConstants.ControlTypePropertyId:
                        result = Variant.FromInt(ControlTypeFor(view.AccessibilityRole));
                        break;

                    case UiaConstants.NamePropertyId:
                        result = Variant.FromString(NameFor(renderer));
                        break;

                    case UiaConstants.IsKeyboardFocusablePropertyId:
                        result = Variant.FromBool(FocusManager.IsFocusable(renderer));
                        break;

                    case UiaConstants.HasKeyboardFocusPropertyId:
                        result = Variant.FromBool(Win32Host.Current.Focus.Focused == renderer);
                        break;

                    case UiaConstants.IsEnabledPropertyId:
                        result = Variant.FromBool(renderer.IsHitTestVisible);
                        break;

                    case UiaConstants.IsControlElementPropertyId:
                    case UiaConstants.IsContentElementPropertyId:
                        result = Variant.FromBool(true);
                        break;

                    case UiaConstants.IsOffscreenPropertyId:
                        result = Variant.FromBool(!AutomationTree.IsOnScreen(renderer));
                        break;

                    case UiaConstants.FrameworkIdPropertyId:
                        result = Variant.FromString("XForms");
                        break;

                    case UiaConstants.ToggleToggleStatePropertyId:
                        if (view.AccessibilityIsChecked is bool isChecked)
                        {
                            result = Variant.FromInt(isChecked ? UiaConstants.ToggleStateOn : UiaConstants.ToggleStateOff);
                        }

                        break;

                    case UiaConstants.ExpandCollapseExpandCollapseStatePropertyId:
                        if (view.AccessibilityIsExpanded is bool isExpanded)
                        {
                            result = Variant.FromInt(isExpanded ? UiaConstants.Expanded : UiaConstants.Collapsed);
                        }

                        break;

                    case UiaConstants.HeadingLevelPropertyId:
                        if (view.AccessibilityRole == AccessibilityRole.Heading)
                        {
                            result = Variant.FromInt(UiaConstants.HeadingLevel1);
                        }

                        break;

                    case UiaConstants.SelectionItemIsSelectedPropertyId:
                        result = Variant.FromBool(view.AccessibilityIsSelected);
                        break;

                    case UiaConstants.ValueValuePropertyId:
                        if (null != view.AccessibilityValue)
                        {
                            result = Variant.FromString(view.AccessibilityValue);
                        }

                        break;
                }

                return 0;
            });

            *value = result;
            return hr;
        }

        public int GetHostRawElementProvider(out nint provider)
        {
            // Only the window's root element is hosted by the window
            provider = 0;
            return 0;
        }

        public int Navigate(int direction, out nint fragment)
        {
            nint result = 0;
            int hr = this.OnUIThread((renderer) =>
            {
                result = AutomationTree.Navigate(renderer, direction);
                return 0;
            });

            fragment = result;
            return hr;
        }

        public int GetRuntimeId(out nint runtimeId)
        {
            runtimeId = UiaNative.CreateIntArray(UiaConstants.UiaAppendRuntimeId, this._runtimeId);
            return 0;
        }

        public int GetBoundingRectangle(out UiaRect rectangle)
        {
            var result = new UiaRect();
            int hr = this.OnUIThread((renderer) =>
            {
                result = AutomationTree.ScreenBounds(renderer.AbsoluteOrigin(), renderer.Frame.Size);
                return 0;
            });

            rectangle = result;
            return hr;
        }

        public int GetEmbeddedFragmentRoots(out nint roots)
        {
            roots = 0;
            return 0;
        }

        public int SetFocus()
        {
            return this.OnUIThread((renderer) =>
            {
                Win32Host.Current.Focus.SetFocus(renderer, showRing: true);
                return 0;
            });
        }

        public int GetFragmentRoot(out nint root)
        {
            root = AutomationTree.GetInterface<IRawElementProviderFragmentRoot>(AutomationTree.Root);
            return 0;
        }

        private const int E_FAIL = unchecked((int)0x80004005);
        private const int E_ACCESSDENIED = unchecked((int)0x80070005);

        private int Perform(
            AccessibilityAction action)
        {
            // NOTE: Actions must return right away: one may open a modal dialog,
            // which would block the client (and every later call) until closed
            return this.OnUIThread((renderer) =>
            {
                var view = (View)renderer.Element;
                if (!view.IsAccessibilityActionSupported(action))
                {
                    return E_FAIL;
                }

                Win32Host.Current.Post(() => view.PerformAccessibilityAction(action));
                return 0;
            });
        }

        public int Invoke()
        {
            return this.Perform(AccessibilityAction.Invoke);
        }

        public int Toggle()
        {
            return this.Perform(AccessibilityAction.Toggle);
        }

        public int get_ToggleState(out int state)
        {
            int result = 0;
            int hr = this.OnUIThread((renderer) =>
            {
                result = (((View)renderer.Element).AccessibilityIsChecked == true) ? UiaConstants.ToggleStateOn : UiaConstants.ToggleStateOff;
                return 0;
            });

            state = result;
            return hr;
        }

        public int Expand()
        {
            return this.Perform(AccessibilityAction.Expand);
        }

        public int Collapse()
        {
            return this.Perform(AccessibilityAction.Collapse);
        }

        public int get_ExpandCollapseState(out int state)
        {
            int result = 0;
            int hr = this.OnUIThread((renderer) =>
            {
                result = (((View)renderer.Element).AccessibilityIsExpanded == true) ? UiaConstants.Expanded : UiaConstants.Collapsed;
                return 0;
            });

            state = result;
            return hr;
        }

        public int Select()
        {
            return this.Perform(AccessibilityAction.Select);
        }

        public int AddToSelection()
        {
            return this.Perform(AccessibilityAction.Select);
        }

        public int RemoveFromSelection()
        {
            // An item is unselected by selecting another
            return E_FAIL;
        }

        public int get_IsSelected(out int isSelected)
        {
            int result = 0;
            int hr = this.OnUIThread((renderer) =>
            {
                result = ((View)renderer.Element).AccessibilityIsSelected ? 1 : 0;
                return 0;
            });

            isSelected = result;
            return hr;
        }

        public int get_SelectionContainer(out nint container)
        {
            nint result = 0;
            int hr = this.OnUIThread((renderer) =>
            {
                var parent = AutomationTree.GetParent(renderer);
                if (null != parent)
                {
                    result = AutomationTree.GetInterface<IRawElementProviderSimple>(AutomationTree.GetElement(parent));
                }

                return 0;
            });

            container = result;
            return hr;
        }

        public int SetValue(nint value)
        {
            // Values are chosen with the view's own picker
            return E_ACCESSDENIED;
        }

        public int get_Value(out nint value)
        {
            string result = null;
            int hr = this.OnUIThread((renderer) =>
            {
                result = ((View)renderer.Element).AccessibilityValue;
                return 0;
            });

            value = Marshal.StringToBSTR(result ?? string.Empty);
            return hr;
        }

        public int get_IsReadOnly(out int isReadOnly)
        {
            isReadOnly = 1;
            return 0;
        }
    }

    /// <summary>
    /// The automation element for the window's client area.
    /// </summary>
    [GeneratedComClass]
    internal sealed unsafe partial class RootProvider : IRawElementProviderSimple, IRawElementProviderFragment, IRawElementProviderFragmentRoot
    {
        private readonly Win32Host _host;

        public RootProvider(
            Win32Host host)
        {
            this._host = host;
        }

        public int GetProviderOptions(out int options)
        {
            options = UiaConstants.ServerSideProvider | UiaConstants.UseComThreading;
            return 0;
        }

        public int GetPatternProvider(int patternId, out nint provider)
        {
            provider = 0;
            return 0;
        }

        public int GetPropertyValue(int propertyId, Variant* value)
        {
            // The window's own (hosting) provider supplies its name and type
            *value = (propertyId == UiaConstants.FrameworkIdPropertyId) ? Variant.FromString("XForms") : new Variant();
            return 0;
        }

        public int GetHostRawElementProvider(out nint provider)
        {
            return UiaNative.UiaHostProviderFromHwnd((nint)this._host.Hwnd.Value, out provider);
        }

        public int Navigate(int direction, out nint fragment)
        {
            nint result = 0;
            this._host.InvokeSync(() =>
            {
                if ((direction == UiaConstants.FirstChild) || (direction == UiaConstants.LastChild))
                {
                    var children = AutomationTree.GetChildren(null);
                    if (children.Count > 0)
                    {
                        var child = (direction == UiaConstants.FirstChild) ? children[0] : children[children.Count - 1];
                        result = AutomationTree.GetInterface<IRawElementProviderFragment>(AutomationTree.GetElement(child));
                    }
                }

                return 0;
            });

            fragment = result;
            return 0;
        }

        public int GetRuntimeId(out nint runtimeId)
        {
            // The window's host provider supplies the root's runtime ID
            runtimeId = 0;
            return 0;
        }

        public int GetBoundingRectangle(out UiaRect rectangle)
        {
            // The window's host provider supplies the root's bounds
            rectangle = new UiaRect();
            return 0;
        }

        public int GetEmbeddedFragmentRoots(out nint roots)
        {
            roots = 0;
            return 0;
        }

        public int SetFocus()
        {
            return 0;
        }

        public int GetFragmentRoot(out nint root)
        {
            root = AutomationTree.GetInterface<IRawElementProviderFragmentRoot>(this);
            return 0;
        }

        public int ElementProviderFromPoint(double x, double y, out nint fragment)
        {
            nint result = 0;
            this._host.InvokeSync(() =>
            {
                var element = AutomationTree.AccessibleAt(this._host.ScreenToClient(x, y));
                result = (null != element)
                    ? AutomationTree.GetInterface<IRawElementProviderFragment>(AutomationTree.GetElement(element))
                    : AutomationTree.GetInterface<IRawElementProviderFragment>(this);
                return 0;
            });

            fragment = result;
            return 0;
        }

        public int GetFocus(out nint fragment)
        {
            nint result = 0;
            this._host.InvokeSync(() =>
            {
                var focused = this._host.Focus.Focused;
                if ((null != focused) && AutomationTree.IsAccessible(focused))
                {
                    result = AutomationTree.GetInterface<IRawElementProviderFragment>(AutomationTree.GetElement(focused));
                }

                return 0;
            });

            fragment = result;
            return 0;
        }
    }
}
