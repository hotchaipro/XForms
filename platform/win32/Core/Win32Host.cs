using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Direct2D;
using Windows.Win32.Graphics.Direct3D;
using Windows.Win32.Graphics.Direct3D11;
using Windows.Win32.Graphics.DirectComposition;
using Windows.Win32.Graphics.DirectWrite;
using Windows.Win32.Graphics.Dxgi;
using Windows.Win32.Graphics.Gdi;
using Windows.Win32.Graphics.Imaging;
using Windows.Win32.System.Com;
using Windows.Win32.UI.Controls;
using Windows.Win32.UI.HiDpi;
using Windows.Win32.UI.Input.Pointer;
using Windows.Win32.UI.WindowsAndMessaging;
using XForms.Win32.Renderers;

namespace XForms.Win32
{
    /// <summary>
    /// PROTOTYPE: hosts an XForms app in a plain Win32 window. Views are
    /// DirectComposition visuals, drawing goes through Direct2D and DirectWrite,
    /// and text entry uses native RichEdit controls. Layout, drawing and the
    /// composition commit are batched into one pass per message loop turn.
    /// </summary>
    internal sealed unsafe class Win32Host
    {
        private const uint WM_APP_UPDATE = PInvoke.WM_APP + 1;
        private const uint WM_APP_INVOKE = PInvoke.WM_APP + 2;
        private const uint WM_APP_INVOKE_SYNC = PInvoke.WM_APP + 3;

        private static Win32Host _current;

        private readonly ConcurrentQueue<Action> _invokeQueue = new ConcurrentQueue<Action>();
        private readonly HashSet<ViewRenderer> _dirtyRenderers = new HashSet<ViewRenderer>();
        private readonly List<TextBoxRenderer> _textEntries = new List<TextBoxRenderer>();
        private HWND _hwnd;
        private bool _isUpdatePending;
        private bool _isLayoutPending;
        private ViewRenderer _root;
        private PointerDispatcher _pointers;
        private FocusManager _focus;
        private bool _isUpdating;
        private bool _isTextBoxUpdatePending;
        private int _uiThreadId;

        private Win32Host()
        {
        }

        public static Win32Host Current
        {
            get
            {
                return _current;
            }
        }

        public ViewRenderer Root
        {
            get
            {
                return this._root;
            }
        }

        public FocusManager Focus
        {
            get
            {
                return this._focus;
            }
        }

        public HWND Hwnd
        {
            get
            {
                return this._hwnd;
            }
        }

        /// <summary>
        /// Raised when Windows' theme settings may have changed: dark mode, the
        /// accent color or high contrast.
        /// </summary>
        public event Action SystemThemeChanged;

        /// <summary>
        /// Device pixels per device-independent pixel.
        /// </summary>
        public float Scale
        {
            get;
            private set;
        } = 1.0f;

        public ID2D1Device* D2DDevice { get; private set; }

        public ID2D1DeviceContext* ResourceContext { get; private set; }

        public IDCompositionDesktopDevice* CompositionDevice { get; private set; }

        public IDWriteFactory* DWriteFactory { get; private set; }

        public IWICImagingFactory* WicFactory { get; private set; }

        private IDCompositionTarget* _target;
        private IDCompositionTarget* _overlayTarget;
        private OverlayLayer _overlays;

        /// <summary>
        /// Popups shown above the page.
        /// </summary>
        public OverlayLayer Overlays
        {
            get
            {
                return this._overlays;
            }
        }

        /// <summary>
        /// The client area's size in DIPs.
        /// </summary>
        public Size ClientSize
        {
            get
            {
                PInvoke.GetClientRect(this._hwnd, out var clientRect);
                return new Size(clientRect.right / this.Scale, clientRect.bottom / this.Scale);
            }
        }

        /// <summary>
        /// Creates the window and graphics devices, lets the caller create the
        /// app, then runs the message loop until the window closes.
        /// </summary>
        public static void Run(
            string title,
            Func<IPlatform, Application> createApplication)
        {
            var host = new Win32Host();
            _current = host;
            host._uiThreadId = Environment.CurrentManagedThreadId;

            host.Initialize(title);

            SynchronizationContext.SetSynchronizationContext(new Win32SynchronizationContext(host));

            var application = createApplication(new Win32Platform(host));
            application.Start();

            PInvoke.ShowWindow(host._hwnd, SHOW_WINDOW_CMD.SW_SHOWDEFAULT);

            MSG msg;
            while (PInvoke.GetMessage(&msg, HWND.Null, 0, 0))
            {
                // Focus, activation and back keys work wherever keyboard focus is
                if (((msg.message == PInvoke.WM_KEYDOWN) || (msg.message == PInvoke.WM_SYSKEYDOWN)
                        || (msg.message == PInvoke.WM_KEYUP) || (msg.message == PInvoke.WM_SYSKEYUP))
                    && host._focus.PreviewKey(msg.message, (Windows.Win32.UI.Input.KeyboardAndMouse.VIRTUAL_KEY)msg.wParam.Value))
                {
                    continue;
                }

                PInvoke.TranslateMessage(&msg);
                PInvoke.DispatchMessage(&msg);
            }
        }

        private void Initialize(
            string title)
        {
            PInvoke.SetProcessDpiAwarenessContext(DPI_AWARENESS_CONTEXT.DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2);
            PInvoke.CoInitializeEx(null, COINIT.COINIT_APARTMENTTHREADED).ThrowOnFailure();

            this.CreateDevices();

            // RichEdit with DirectWrite rendering
            PInvoke.LoadLibrary("Msftedit.dll");

            // Mouse, touch and pen all arrive as pointer messages
            PInvoke.EnableMouseInPointer(true);
            this._pointers = new PointerDispatcher(this);
            this._focus = new FocusManager(this);

            var instance = PInvoke.GetModuleHandle((PCWSTR)null);

            fixed (char* className = "XFormsWin32Window")
            {
                var windowClass = new WNDCLASSEXW()
                {
                    cbSize = (uint)sizeof(WNDCLASSEXW),
                    style = WNDCLASS_STYLES.CS_HREDRAW | WNDCLASS_STYLES.CS_VREDRAW,
                    lpfnWndProc = &WindowProc,
                    hInstance = (HINSTANCE)instance.Value,
                    hCursor = PInvoke.LoadCursor(HINSTANCE.Null, PInvoke.IDC_ARROW),
                    lpszClassName = className,
                };
                PInvoke.RegisterClassEx(&windowClass);

                fixed (char* windowTitle = title)
                {
                    this._hwnd = PInvoke.CreateWindowEx(
                        // NOTE: The window keeps its redirection surface so the RichEdit
                        // child windows (which draw with GDI interop) are composited
                        0,
                        className,
                        windowTitle,
                        WINDOW_STYLE.WS_OVERLAPPEDWINDOW | WINDOW_STYLE.WS_CLIPCHILDREN,
                        PInvoke.CW_USEDEFAULT,
                        PInvoke.CW_USEDEFAULT,
                        1100,
                        800,
                        HWND.Null,
                        HMENU.Null,
                        (HINSTANCE)instance.Value,
                        null);
                }
            }

            this.Scale = PInvoke.GetDpiForWindow(this._hwnd) / 96.0f;


            IDCompositionTarget* target;
            this.CompositionDevice->CreateTargetForHwnd(this._hwnd, false, &target);
            this._target = target;

            // Popups go on a second target that draws above the child windows
            IDCompositionTarget* overlayTarget;
            this.CompositionDevice->CreateTargetForHwnd(this._hwnd, true, &overlayTarget);
            this._overlayTarget = overlayTarget;

            IDCompositionVisual2* overlayVisual;
            this.CompositionDevice->CreateVisual(&overlayVisual);
            overlayTarget->SetRoot((IDCompositionVisual*)overlayVisual);
            this._overlays = new OverlayLayer(this, overlayVisual);
        }

        private void CreateDevices()
        {
            // Direct3D device shared by Direct2D and DirectComposition
            ID3D11Device* d3dDevice;
            PInvoke.D3D11CreateDevice(
                null,
                D3D_DRIVER_TYPE.D3D_DRIVER_TYPE_HARDWARE,
                HMODULE.Null,
                D3D11_CREATE_DEVICE_FLAG.D3D11_CREATE_DEVICE_BGRA_SUPPORT,
                null,
                0,
                PInvoke.D3D11_SDK_VERSION,
                &d3dDevice,
                null,
                null).ThrowOnFailure();

            IDXGIDevice* dxgiDevice;
            ((IUnknown*)d3dDevice)->QueryInterface(IDXGIDevice.IID_Guid, out var dxgiPointer).ThrowOnFailure();
            dxgiDevice = (IDXGIDevice*)dxgiPointer;

            PInvoke.D2D1CreateFactory(D2D1_FACTORY_TYPE.D2D1_FACTORY_TYPE_SINGLE_THREADED, ID2D1Factory1.IID_Guid, null, out var factoryPointer).ThrowOnFailure();
            var d2dFactory = (ID2D1Factory1*)factoryPointer;

            ID2D1Device* d2dDevice;
            d2dFactory->CreateDevice(dxgiDevice, &d2dDevice);
            this.D2DDevice = d2dDevice;

            ID2D1DeviceContext* resourceContext;
            d2dDevice->CreateDeviceContext(D2D1_DEVICE_CONTEXT_OPTIONS.D2D1_DEVICE_CONTEXT_OPTIONS_NONE, &resourceContext);
            this.ResourceContext = resourceContext;

            PInvoke.DCompositionCreateDevice2((IUnknown*)d2dDevice, IDCompositionDesktopDevice.IID_Guid, out var compositionPointer).ThrowOnFailure();
            this.CompositionDevice = (IDCompositionDesktopDevice*)compositionPointer;

            PInvoke.DWriteCreateFactory(DWRITE_FACTORY_TYPE.DWRITE_FACTORY_TYPE_SHARED, IDWriteFactory.IID_Guid, out var dwritePointer).ThrowOnFailure();
            this.DWriteFactory = (IDWriteFactory*)dwritePointer;

            PInvoke.CoCreateInstance(PInvoke.CLSID_WICImagingFactory, null, CLSCTX.CLSCTX_INPROC_SERVER, IWICImagingFactory.IID_Guid, out var wicPointer).ThrowOnFailure();
            this.WicFactory = (IWICImagingFactory*)wicPointer;

            dxgiDevice->Release();
            d3dDevice->Release();
        }

        public void SetRoot(
            ViewRenderer root)
        {
            this._root = root;
            this._target->SetRoot((IDCompositionVisual*)root.RootVisual);
            this.RequestLayout();
        }

        public void RegisterTextBox(
            TextBoxRenderer textEntry)
        {
            this._textEntries.Add(textEntry);
        }

        /// <summary>
        /// Schedules a layout pass (and the redraw and commit that follow it).
        /// </summary>
        public void RequestLayout()
        {
            this._isLayoutPending = true;
            this.ScheduleUpdate();
        }

        /// <summary>
        /// Schedules a redraw of the renderer's content.
        /// </summary>
        public void RequestRender(
            ViewRenderer renderer)
        {
            this._dirtyRenderers.Add(renderer);
            this.ScheduleUpdate();
        }

        /// <summary>
        /// Schedules a composition commit for changes that need no layout or drawing.
        /// </summary>
        public void RequestCommit()
        {
            this.ScheduleUpdate();
        }

        private void ScheduleUpdate()
        {
            if (!this._isUpdatePending)
            {
                this._isUpdatePending = true;
                PInvoke.PostMessage(this._hwnd, WM_APP_UPDATE, default, default);
            }
        }

        public void Post(
            Action action)
        {
            this._invokeQueue.Enqueue(action);
            PInvoke.PostMessage(this._hwnd, WM_APP_INVOKE, default, default);
        }

        /// <summary>
        /// Runs the function on the UI thread and waits for its result, such as
        /// for UI Automation calls that arrive on another thread. Exceptions are
        /// returned as their HRESULT.
        /// </summary>
        public int InvokeSync(
            Func<int> function)
        {
            int result = 0;
            Action action = () =>
            {
                try
                {
                    result = function();
                }
                catch (Exception ex)
                {
                    result = ex.HResult;
                }
            };

            if (Environment.CurrentManagedThreadId == this._uiThreadId)
            {
                action();
                return result;
            }

            var handle = GCHandle.Alloc(action);
            try
            {
                PInvoke.SendMessage(this._hwnd, WM_APP_INVOKE_SYNC, default, (LPARAM)GCHandle.ToIntPtr(handle));
            }
            finally
            {
                handle.Free();
            }

            return result;
        }

        /// <summary>
        /// Converts a point in client DIPs to screen pixels.
        /// </summary>
        public Point ClientToScreen(
            Point point)
        {
            var location = new System.Drawing.Point((int)Math.Round(point.X * this.Scale), (int)Math.Round(point.Y * this.Scale));
            PInvoke.ClientToScreen(this._hwnd, ref location);
            return new Point(location.X, location.Y);
        }

        /// <summary>
        /// Converts a point in screen pixels to client DIPs.
        /// </summary>
        public Point ScreenToClient(
            double x,
            double y)
        {
            var location = new System.Drawing.Point((int)Math.Round(x), (int)Math.Round(y));
            PInvoke.ScreenToClient(this._hwnd, ref location);
            return new Point(location.X / this.Scale, location.Y / this.Scale);
        }

        /// <summary>
        /// Lays out, draws and commits now, such as for each step of a scroll.
        /// </summary>
        public void Flush()
        {
            if (this._isUpdating)
            {
                // Already inside an update, which commits when it finishes
                return;
            }

            this.Update();
        }

        private void Update()
        {
            this._isUpdatePending = false;
            this._isUpdating = true;
            try
            {
                this.UpdateCore();
            }
            finally
            {
                this._isUpdating = false;
            }
        }

        private void UpdateCore()
        {

            if (this._isLayoutPending && (null != this._root))
            {
                this._isLayoutPending = false;

                var size = this.ClientSize;

                var root = (View)this._root.Element;
                root.Measure(size);
                root.Arrange(new Rect(0, 0, size.Width, size.Height));

                this._overlays.Layout();

                this.UpdateTextBoxPositions();
            }

            if (this._isTextBoxUpdatePending)
            {
                this.UpdateTextBoxPositions();
            }

            if (this._dirtyRenderers.Count > 0)
            {
                var dirtyRenderers = new List<ViewRenderer>(this._dirtyRenderers);
                this._dirtyRenderers.Clear();

                foreach (var renderer in dirtyRenderers)
                {
                    renderer.Render();
                }
            }

            this.CompositionDevice->Commit();
        }

        public void RequestTextBoxPositionUpdate()
        {
            this._isTextBoxUpdatePending = true;
            this.ScheduleUpdate();
        }

        public void UpdateTextBoxPositions()
        {
            this._isTextBoxUpdatePending = false;

            foreach (var textEntry in this._textEntries)
            {
                textEntry.UpdateWindowPosition();
            }

            this._focus?.UpdateRing();
        }

        #region Pointer input

        /// <summary>
        /// Finds the deepest view under the point (in client DIPs), looking in the
        /// top popup first. Returns false when a popup is open and the point is
        /// outside it.
        /// </summary>
        public bool HitTest(
            Point point,
            out ViewRenderer hit)
        {
            if (null != this._overlays.Top)
            {
                return this._overlays.HitTest(point, out hit);
            }

            hit = this._root?.HitTest(point);
            return true;
        }

        private void OnMouseWheel(
            Point point,
            float delta)
        {
            this.HitTest(point, out var hit);

            // Views under the pointer get the wheel first
            for (var renderer = hit; null != renderer; renderer = renderer.ParentRenderer)
            {
                if ((renderer.Element is View view) && renderer.IsHitTestVisible)
                {
                    var origin = renderer.AbsoluteOrigin();
                    var local = new Point(point.X - origin.X, point.Y - origin.Y);
                    var e = new PointerEventArgs(0, PointerDeviceType.Mouse, local, PointerInputState.Moved, false, FocusManager.CurrentModifiers(), delta);
                    if (view.DispatchPointerWheel(e))
                    {
                        return;
                    }
                }
            }

            for (var renderer = hit; null != renderer; renderer = renderer.ParentRenderer)
            {
                if (renderer is IScrollableRenderer scrollable)
                {
                    scrollable.ScrollBy(-delta);
                    return;
                }
            }
        }

        #endregion Pointer input

        private bool GetPointerLocation(
            uint pointerId,
            out Point point)
        {
            POINTER_INFO info;
            if (!PInvoke.GetPointerInfo(pointerId, &info))
            {
                point = Point.Zero;
                return false;
            }

            var location = info.ptPixelLocation;
            PInvoke.ScreenToClient(this._hwnd, ref location);
            point = new Point(location.X / this.Scale, location.Y / this.Scale);
            return true;
        }

        private bool OnPointerMessage(
            uint msg,
            WPARAM wParam)
        {
            uint pointerId = (uint)(wParam.Value & 0xFFFF);
            uint flags = (uint)((wParam.Value >> 16) & 0xFFFF);
            bool isInContact = (flags & 0x0004) != 0; // POINTER_MESSAGE_FLAG_INCONTACT

            if (!this.GetPointerLocation(pointerId, out var point))
            {
                return false;
            }

            PInvoke.GetPointerType(pointerId, out var pointerType);
            bool isDirect = (pointerType == POINTER_INPUT_TYPE.PT_TOUCH) || (pointerType == POINTER_INPUT_TYPE.PT_PEN);
            var deviceType = (pointerType == POINTER_INPUT_TYPE.PT_TOUCH) ? PointerDeviceType.Touch
                : (pointerType == POINTER_INPUT_TYPE.PT_PEN) ? PointerDeviceType.Pen
                : PointerDeviceType.Mouse;

            switch (msg)
            {
                case PInvoke.WM_POINTERDOWN:
                    this._pointers.Down(pointerId, point, isDirect, deviceType);
                    if (!isDirect)
                    {
                        // Keep receiving mouse moves outside the window
                        PInvoke.SetCapture(this._hwnd);
                    }
                    return true;

                case PInvoke.WM_POINTERUPDATE:
                    if (isInContact)
                    {
                        this._pointers.Move(pointerId, point);
                    }
                    return true;

                case PInvoke.WM_POINTERUP:
                    this._pointers.Up(pointerId, point);
                    if (!isDirect)
                    {
                        PInvoke.ReleaseCapture();
                    }
                    return true;
            }

            return false;
        }

        private Point PointFromLParam(
            LPARAM lParam)
        {
            int x = (short)(lParam.Value & 0xFFFF);
            int y = (short)((lParam.Value >> 16) & 0xFFFF);
            return new Point(x / this.Scale, y / this.Scale);
        }

        [UnmanagedCallersOnly(CallConvs = new[] { typeof(System.Runtime.CompilerServices.CallConvStdcall) })]
        private static LRESULT WindowProc(
            HWND hwnd,
            uint msg,
            WPARAM wParam,
            LPARAM lParam)
        {
            var host = _current;

            try
            {
                switch (msg)
                {
                    case WM_APP_UPDATE:
                        host.Update();
                        return (LRESULT)0;

                    case WM_APP_INVOKE:
                        while (host._invokeQueue.TryDequeue(out var action))
                        {
                            action();
                        }
                        return (LRESULT)0;

                    case WM_APP_INVOKE_SYNC:
                        ((Action)GCHandle.FromIntPtr(lParam.Value).Target)();
                        return (LRESULT)0;

                    case PInvoke.WM_GETOBJECT:
                        if ((int)lParam.Value == Automation.UiaConstants.UiaRootObjectId)
                        {
                            // Screen readers ask the window for its UI Automation provider
                            nint provider = Automation.AutomationTree.GetInterface<Automation.IRawElementProviderSimple>(Automation.AutomationTree.Root);
                            nint result = Automation.UiaNative.UiaReturnRawElementProvider((nint)hwnd.Value, (nint)wParam.Value, lParam.Value, provider);
                            Marshal.Release(provider);
                            return (LRESULT)result;
                        }
                        break;

                    case PInvoke.WM_SIZE:
                        host.RequestLayout();
                        break;

                    case PInvoke.WM_ERASEBKGND:
                        // The composition tree paints the whole client area
                        return (LRESULT)1;

                    case PInvoke.WM_PAINT:
                        {
                            PAINTSTRUCT paint;
                            PInvoke.BeginPaint(hwnd, &paint);
                            PInvoke.EndPaint(hwnd, &paint);
                            return (LRESULT)0;
                        }

                    case PInvoke.WM_SETTINGCHANGE:
                    case PInvoke.WM_SYSCOLORCHANGE:
                    case PInvoke.WM_THEMECHANGED:
                        // Dark mode and the accent color arrive as
                        // "ImmersiveColorSet"; high contrast as a color change.
                        // Rereading the settings is cheap, and the app ignores
                        // a theme that didn't change.
                        host.SystemThemeChanged?.Invoke();
                        break;

                    case PInvoke.WM_DPICHANGED:
                        host.Scale = (wParam.Value & 0xFFFF) / 96.0f;
                        var suggested = (RECT*)lParam.Value;
                        PInvoke.SetWindowPos(hwnd, HWND.Null, suggested->left, suggested->top, suggested->right - suggested->left, suggested->bottom - suggested->top, SET_WINDOW_POS_FLAGS.SWP_NOZORDER | SET_WINDOW_POS_FLAGS.SWP_NOACTIVATE);
                        host._root?.InvalidateRenderRecursive();
                        foreach (var overlay in host._overlays.Overlays)
                        {
                            overlay.Renderer.InvalidateRenderRecursive();
                        }

                        host.RequestLayout();
                        return (LRESULT)0;

                    case PInvoke.WM_POINTERDOWN:
                    case PInvoke.WM_POINTERUPDATE:
                    case PInvoke.WM_POINTERUP:
                        if (host.OnPointerMessage(msg, wParam))
                        {
                            return (LRESULT)0;
                        }
                        break;

                    case PInvoke.WM_POINTERCAPTURECHANGED:
                        host._pointers.Cancel();
                        break;

                    case PInvoke.WM_POINTERWHEEL:
                        {
                            var info = host.GetPointerLocation((uint)(wParam.Value & 0xFFFF), out var point);
                            if (info)
                            {
                                float delta = (short)((wParam.Value >> 16) & 0xFFFF);
                                host.OnMouseWheel(point, delta / 120.0f * 48.0f);
                            }
                            return (LRESULT)0;
                        }

                    case PInvoke.WM_MOUSEWHEEL:
                        {
                            // NOTE: Wheel coordinates are in screen space
                            var screenPoint = new System.Drawing.Point((short)(lParam.Value & 0xFFFF), (short)((lParam.Value >> 16) & 0xFFFF));
                            PInvoke.ScreenToClient(hwnd, ref screenPoint);
                            var point = new Point(screenPoint.X / host.Scale, screenPoint.Y / host.Scale);
                            float delta = (short)((wParam.Value >> 16) & 0xFFFF);
                            host.OnMouseWheel(point, delta / 120.0f * 48.0f);
                            return (LRESULT)0;
                        }

                    case PInvoke.WM_NOTIFY:
                        {
                            var header = (NMHDR*)lParam.Value;
                            foreach (var textEntry in host._textEntries)
                            {
                                if (textEntry.Hwnd == header->hwndFrom)
                                {
                                    textEntry.OnNotify(header);
                                    break;
                                }
                            }
                        }
                        break;

                    case PInvoke.WM_COMMAND:
                        {
                            var source = new HWND((void*)lParam.Value);
                            uint code = (uint)((wParam.Value >> 16) & 0xFFFF);
                            foreach (var textEntry in host._textEntries)
                            {
                                if (textEntry.Hwnd == source)
                                {
                                    textEntry.OnCommand(code);
                                    break;
                                }
                            }
                        }
                        break;

                    case PInvoke.WM_DESTROY:
                        PInvoke.PostQuitMessage(0);
                        return (LRESULT)0;
                }
            }
            catch (Exception ex)
            {
                // Surface failures instead of letting them unwind through native frames
                System.IO.File.AppendAllText(
                    System.IO.Path.Combine(AppContext.BaseDirectory, "crash.log"),
                    $"{DateTime.Now:O} {ex}{Environment.NewLine}");
                Environment.FailFast("Unhandled exception in the window procedure.", ex);
            }

            return PInvoke.DefWindowProc(hwnd, msg, wParam, lParam);
        }

        private sealed class Win32SynchronizationContext : SynchronizationContext
        {
            private readonly Win32Host _host;

            public Win32SynchronizationContext(
                Win32Host host)
            {
                this._host = host;
            }

            public override void Post(
                SendOrPostCallback d,
                object state)
            {
                this._host.Post(() => d(state));
            }

            public override SynchronizationContext CreateCopy()
            {
                return this;
            }
        }
    }
}
