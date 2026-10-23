using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace XForms.Win32.Automation
{
    // UI Automation provider interfaces (from UIAutomationCore.h), declared for
    // .NET's source-generated COM so they work with trimming and Native AOT.
    // Interface pointers are passed as raw pointers to keep marshalling explicit.

    [GeneratedComInterface]
    [Guid("d6dd68d1-86fd-4332-8666-9abedea2d24c")]
    internal partial interface IRawElementProviderSimple
    {
        [PreserveSig]
        int GetProviderOptions(out int options);

        [PreserveSig]
        int GetPatternProvider(int patternId, out nint provider);

        [PreserveSig]
        unsafe int GetPropertyValue(int propertyId, Variant* value);

        [PreserveSig]
        int GetHostRawElementProvider(out nint provider);
    }

    [GeneratedComInterface]
    [Guid("f7063da8-8359-439c-9297-bbc5299a7d87")]
    internal partial interface IRawElementProviderFragment
    {
        [PreserveSig]
        int Navigate(int direction, out nint fragment);

        [PreserveSig]
        int GetRuntimeId(out nint runtimeId);

        [PreserveSig]
        int GetBoundingRectangle(out UiaRect rectangle);

        [PreserveSig]
        int GetEmbeddedFragmentRoots(out nint roots);

        [PreserveSig]
        int SetFocus();

        [PreserveSig]
        int GetFragmentRoot(out nint root);
    }

    [GeneratedComInterface]
    [Guid("620ce2a5-ab8f-40a9-86cb-de3c75599b58")]
    internal partial interface IRawElementProviderFragmentRoot
    {
        [PreserveSig]
        int ElementProviderFromPoint(double x, double y, out nint fragment);

        [PreserveSig]
        int GetFocus(out nint fragment);
    }

    [GeneratedComInterface]
    [Guid("54fcb24b-e18e-47a2-b4d3-eccbe77599a2")]
    internal partial interface IInvokeProvider
    {
        [PreserveSig]
        int Invoke();
    }

    [GeneratedComInterface]
    [Guid("56d00bd0-c4f4-433c-a836-1a52a57e0892")]
    internal partial interface IToggleProvider
    {
        [PreserveSig]
        int Toggle();

        [PreserveSig]
        int get_ToggleState(out int state);
    }

    [GeneratedComInterface]
    [Guid("2acad808-b2d4-452d-a407-91ff1ad167b2")]
    internal partial interface ISelectionItemProvider
    {
        [PreserveSig]
        int Select();

        [PreserveSig]
        int AddToSelection();

        [PreserveSig]
        int RemoveFromSelection();

        [PreserveSig]
        int get_IsSelected(out int isSelected);

        [PreserveSig]
        int get_SelectionContainer(out nint container);
    }

    [GeneratedComInterface]
    [Guid("d847d3a5-cab0-4a98-8c32-ecb45c59ad24")]
    internal partial interface IExpandCollapseProvider
    {
        [PreserveSig]
        int Expand();

        [PreserveSig]
        int Collapse();

        [PreserveSig]
        int get_ExpandCollapseState(out int state);
    }

    [GeneratedComInterface]
    [Guid("c7935180-6fb3-4201-b174-7df73adbf64a")]
    internal partial interface IValueProvider
    {
        [PreserveSig]
        int SetValue(nint value);

        [PreserveSig]
        int get_Value(out nint value);

        [PreserveSig]
        int get_IsReadOnly(out int isReadOnly);
    }

    [GeneratedComInterface]
    [Guid("36dc7aef-33e6-4691-afe1-2be7274b3d33")]
    internal partial interface IRangeValueProvider
    {
        [PreserveSig]
        int SetValue(double value);

        [PreserveSig]
        int get_Value(out double value);

        [PreserveSig]
        int get_IsReadOnly(out int isReadOnly);

        [PreserveSig]
        int get_Maximum(out double maximum);

        [PreserveSig]
        int get_Minimum(out double minimum);

        [PreserveSig]
        int get_LargeChange(out double largeChange);

        [PreserveSig]
        int get_SmallChange(out double smallChange);
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct UiaRect
    {
        public double Left;
        public double Top;
        public double Width;
        public double Height;
    }

    /// <summary>
    /// The parts of VARIANT that providers return.
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Size = 24)]
    internal struct Variant
    {
        public const ushort VT_EMPTY = 0;
        public const ushort VT_I4 = 3;
        public const ushort VT_R8 = 5;
        public const ushort VT_BSTR = 8;
        public const ushort VT_BOOL = 11;

        [FieldOffset(0)]
        public ushort VariantType;

        [FieldOffset(8)]
        public int IntValue;

        [FieldOffset(8)]
        public short BoolValue;

        [FieldOffset(8)]
        public nint PointerValue;

        [FieldOffset(8)]
        public double DoubleValue;

        public static Variant FromDouble(double value)
        {
            return new Variant() { VariantType = VT_R8, DoubleValue = value };
        }

        public static Variant FromInt(int value)
        {
            return new Variant() { VariantType = VT_I4, IntValue = value };
        }

        public static Variant FromBool(bool value)
        {
            return new Variant() { VariantType = VT_BOOL, BoolValue = (short)(value ? -1 : 0) };
        }

        public static Variant FromString(string value)
        {
            return new Variant() { VariantType = VT_BSTR, PointerValue = Marshal.StringToBSTR(value ?? string.Empty) };
        }
    }

    internal static class UiaConstants
    {
        // ProviderOptions
        public const int ServerSideProvider = 0x1;
        public const int UseComThreading = 0x20;

        // NavigateDirection
        public const int Parent = 0;
        public const int NextSibling = 1;
        public const int PreviousSibling = 2;
        public const int FirstChild = 3;
        public const int LastChild = 4;

        // Properties
        public const int ControlTypePropertyId = 30003;
        public const int NamePropertyId = 30005;
        public const int HasKeyboardFocusPropertyId = 30008;
        public const int IsKeyboardFocusablePropertyId = 30009;
        public const int IsEnabledPropertyId = 30010;
        public const int AutomationIdPropertyId = 30011;
        public const int IsControlElementPropertyId = 30016;
        public const int IsContentElementPropertyId = 30017;
        public const int IsOffscreenPropertyId = 30022;
        public const int FrameworkIdPropertyId = 30024;
        public const int ProviderDescriptionPropertyId = 30107;
        public const int ValueValuePropertyId = 30045;
        public const int SelectionItemIsSelectedPropertyId = 30079;
        public const int ToggleToggleStatePropertyId = 30086;
        public const int ExpandCollapseExpandCollapseStatePropertyId = 30070;
        public const int HeadingLevelPropertyId = 30173;
        public const int RangeValueValuePropertyId = 30047;
        public const int RangeValueIsReadOnlyPropertyId = 30048;
        public const int RangeValueMinimumPropertyId = 30049;
        public const int RangeValueMaximumPropertyId = 30050;
        public const int RangeValueLargeChangePropertyId = 30051;
        public const int RangeValueSmallChangePropertyId = 30052;
        public const int LiveSettingPropertyId = 30135;

        // LiveSetting
        public const int Polite = 1;

        // HeadingLevel
        public const int HeadingLevel1 = 80051;

        // Patterns
        public const int InvokePatternId = 10000;
        public const int ValuePatternId = 10002;
        public const int RangeValuePatternId = 10003;
        public const int ExpandCollapsePatternId = 10005;

        // ExpandCollapseState
        public const int Collapsed = 0;
        public const int Expanded = 1;
        public const int SelectionItemPatternId = 10010;
        public const int TogglePatternId = 10015;

        // ToggleState
        public const int ToggleStateOff = 0;
        public const int ToggleStateOn = 1;
        public const int ToggleStateIndeterminate = 2;

        // Events
        public const int AutomationFocusChangedEventId = 20005;
        public const int SelectionItemElementSelectedEventId = 20012;
        public const int LiveRegionChangedEventId = 20024;

        // Control types
        public const int ButtonControlTypeId = 50000;
        public const int CheckBoxControlTypeId = 50002;
        public const int ComboBoxControlTypeId = 50003;
        public const int EditControlTypeId = 50004;
        public const int ImageControlTypeId = 50006;
        public const int ListItemControlTypeId = 50007;
        public const int ListControlTypeId = 50008;
        public const int MenuControlTypeId = 50009;
        public const int MenuItemControlTypeId = 50011;
        public const int ProgressBarControlTypeId = 50012;
        public const int SliderControlTypeId = 50015;
        public const int SpinnerControlTypeId = 50016;
        public const int RadioButtonControlTypeId = 50013;
        public const int TabItemControlTypeId = 50019;
        public const int TextControlTypeId = 50020;
        public const int ToolTipControlTypeId = 50022;
        public const int TreeControlTypeId = 50023;
        public const int TreeItemControlTypeId = 50024;
        public const int GroupControlTypeId = 50026;
        public const int PaneControlTypeId = 50033;

        public const int UiaRootObjectId = -25;
        public const int UiaAppendRuntimeId = 3;
    }

    internal static unsafe class UiaNative
    {
        [DllImport("UIAutomationCore.dll")]
        public static extern nint UiaReturnRawElementProvider(nint hwnd, nint wParam, nint lParam, nint provider);

        [DllImport("UIAutomationCore.dll")]
        public static extern int UiaHostProviderFromHwnd(nint hwnd, out nint provider);

        [DllImport("UIAutomationCore.dll")]
        public static extern int UiaRaiseAutomationEvent(nint provider, int eventId);

        [DllImport("UIAutomationCore.dll")]
        public static extern int UiaRaiseAutomationPropertyChangedEvent(nint provider, int propertyId, Variant oldValue, Variant newValue);

        [DllImport("UIAutomationCore.dll")]
        public static extern bool UiaClientsAreListening();

        [DllImport("oleaut32.dll")]
        public static extern nint SafeArrayCreateVector(ushort variantType, int lowerBound, uint count);

        [DllImport("oleaut32.dll")]
        public static extern int SafeArrayPutElement(nint array, int* index, void* value);

        public static nint CreateIntArray(
            params int[] values)
        {
            nint array = SafeArrayCreateVector(Variant.VT_I4, 0, (uint)values.Length);
            for (int i = 0; i < values.Length; i += 1)
            {
                int value = values[i];
                SafeArrayPutElement(array, &i, &value);
            }

            return array;
        }
    }
}
