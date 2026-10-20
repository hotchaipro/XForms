using System;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.System.Com;
using Windows.Win32.UI.Accessibility;

namespace XForms.Win32.Automation
{
    /// <summary>
    /// Gives native child windows (such as RichEdit) the accessible name of the
    /// view they belong to, in place of their generic class name.
    /// </summary>
    internal static unsafe class AccessibleNames
    {
        // OBJID_CLIENT
        private const uint ObjectIdClient = 0xFFFFFFFC;

        private static IAccPropServices* _services;

        public static void Set(
            HWND hwnd,
            string name)
        {
            if (null == _services)
            {
                if (PInvoke.CoCreateInstance(PInvoke.CLSID_AccPropServices, null, CLSCTX.CLSCTX_INPROC_SERVER, IAccPropServices.IID_Guid, out var pointer).Failed)
                {
                    return;
                }

                _services = (IAccPropServices*)pointer;
            }

            var property = PInvoke.PROPID_ACC_NAME;
            if (string.IsNullOrEmpty(name))
            {
                _services->ClearHwndProps(hwnd, ObjectIdClient, PInvoke.CHILDID_SELF, &property, 1);
                return;
            }

            fixed (char* text = name)
            {
                _services->SetHwndPropStr(hwnd, ObjectIdClient, PInvoke.CHILDID_SELF, property, text);
            }
        }
    }
}
