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

        // Patterns
        public const int InvokePatternId = 10000;

        // Events
        public const int AutomationFocusChangedEventId = 20005;

        // Control types
        public const int ButtonControlTypeId = 50000;
        public const int CheckBoxControlTypeId = 50002;
        public const int EditControlTypeId = 50004;
        public const int ImageControlTypeId = 50006;
        public const int ListItemControlTypeId = 50007;
        public const int ListControlTypeId = 50008;
        public const int TabItemControlTypeId = 50019;
        public const int TextControlTypeId = 50020;
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
