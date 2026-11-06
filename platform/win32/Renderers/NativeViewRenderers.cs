using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;
using XForma.Primitives;
using XForma.Platform;

namespace XForma.Win32.Renderers
{
    /// <summary>
    /// A child window that follows its view, above everything drawn in the
    /// window: an app's own window, or the one a web view fills.
    /// </summary>
    internal abstract unsafe class WindowRenderer : ViewRenderer, IHostedWindow
    {
        private Win32Host _registeredHost;
        private RECT _windowBounds;
        private bool _isWindowShown;

        protected WindowRenderer(
            View view)
            : base(view, isContainer: false)
        {
        }

        protected HWND Hwnd
        {
            get;
            private set;
        }

        protected void Attach(
            HWND hwnd)
        {
            this.Hwnd = hwnd;
            var style = (uint)PInvoke.GetWindowLong(hwnd, WINDOW_LONG_PTR_INDEX.GWL_STYLE);
            style = (style & ~(uint)WINDOW_STYLE.WS_POPUP & ~(uint)WINDOW_STYLE.WS_OVERLAPPEDWINDOW) | (uint)WINDOW_STYLE.WS_CHILD;
            PInvoke.SetWindowLong(hwnd, WINDOW_LONG_PTR_INDEX.GWL_STYLE, (int)style);
            PInvoke.SetParent(hwnd, this.Host.Hwnd);
            this._registeredHost = this.Host;
            this._registeredHost.RegisterHostedWindow(this);
        }

        public override object NativeElement
        {
            get
            {
                return (nint)this.Hwnd.Value;
            }
        }

        public override Size MeasureNative(
            Size availableSize)
        {
            return new Size(
                float.IsInfinity(availableSize.Width) ? 0 : availableSize.Width,
                float.IsInfinity(availableSize.Height) ? 0 : availableSize.Height);
        }

        public override void ArrangeNative(
            Rect frame)
        {
            base.ArrangeNative(frame);
            this.Host.RequestTextBoxPositionUpdate();
        }

        public void UpdateWindowPosition()
        {
            var host = this.Host;
            if (host != this._registeredHost)
            {
                this._registeredHost.UnregisterHostedWindow(this);
                PInvoke.SetParent(this.Hwnd, host.Hwnd);
                host.RegisterHostedWindow(this);
                this._registeredHost = host;
            }

            float scale = host.Scale;
            var origin = this.AbsoluteOrigin();
            var frame = this.Frame;
            var bounds = new RECT()
            {
                left = (int)Math.Round(origin.X * scale),
                top = (int)Math.Round(origin.Y * scale),
            };
            bounds.right = bounds.left + (int)Math.Round(frame.Width * scale);
            bounds.bottom = bounds.top + (int)Math.Round(frame.Height * scale);

            bool isShown = this.IsShownOnScreen;
            if ((bounds.left == this._windowBounds.left) && (bounds.top == this._windowBounds.top)
                && (bounds.right == this._windowBounds.right) && (bounds.bottom == this._windowBounds.bottom)
                && (isShown == this._isWindowShown))
            {
                return;
            }

            this._windowBounds = bounds;
            this._isWindowShown = isShown;

            PInvoke.SetWindowPos(
                this.Hwnd,
                HWND.Null,
                bounds.left,
                bounds.top,
                bounds.right - bounds.left,
                bounds.bottom - bounds.top,
                SET_WINDOW_POS_FLAGS.SWP_NOZORDER | SET_WINDOW_POS_FLAGS.SWP_NOACTIVATE
                    | (isShown ? SET_WINDOW_POS_FLAGS.SWP_SHOWWINDOW : SET_WINDOW_POS_FLAGS.SWP_HIDEWINDOW));
            this.OnWindowMoved(bounds.right - bounds.left, bounds.bottom - bounds.top, isShown);
        }

        protected virtual void OnWindowMoved(
            int width,
            int height,
            bool isShown)
        {
        }
    }

    internal sealed class NativeViewHostRenderer : WindowRenderer
    {
        public NativeViewHostRenderer(
            NativeViewHost host)
            : base(host)
        {
            if (!(host.CreateNativeView() is nint hwnd))
            {
                throw new InvalidOperationException("On Windows, a NativeViewHost's function returns a window handle (an IntPtr).");
            }

            unsafe
            {
                this.Attach(new HWND((void*)hwnd));
            }
        }
    }

    /// <summary>
    /// WebView2, in a child window of its own. Its API is COM, called here by its
    /// slots in WebView2.h's tables (no interop assemblies, as for the rest of this
    /// backend), and it's created asynchronously: what's asked for before then
    /// waits.
    /// </summary>
    internal sealed unsafe class WebViewRenderer : WindowRenderer, IWebViewRenderer
    {
        private const int EnvironmentCreateController = 3;
        private const int ControllerPutIsVisible = 4;
        private const int ControllerPutBounds = 6;
        private const int ControllerNotifyParentMoved = 23;
        private const int ControllerClose = 24;
        private const int ControllerGetWebView = 25;
        private const int WebGetSource = 4;
        private const int WebNavigate = 5;
        private const int WebNavigateToString = 6;
        private const int WebAddNavigationCompleted = 15;
        private const int WebReload = 31;
        private const int WebGetCanGoBack = 38;
        private const int WebGetCanGoForward = 39;
        private const int WebGoBack = 40;
        private const int WebGoForward = 41;
        private const int ArgsGetIsSuccess = 3;

        private readonly WebView _view;
        private void* _controller;
        private void* _webView;
        private Action _pending;
        private int _width;
        private int _height;
        private bool _isShown;

        // HTML given directly shows as a data address, or about:blank
        private bool _isShowingString;

        [DllImport("WebView2Loader.dll", ExactSpelling = true)]
        private static extern int CreateCoreWebView2EnvironmentWithOptions(
            [MarshalAs(UnmanagedType.LPWStr)] string browserExecutableFolder,
            [MarshalAs(UnmanagedType.LPWStr)] string userDataFolder,
            void* options,
            void* handler);

        public WebViewRenderer(
            WebView view)
            : base(view)
        {
            this._view = view;

            fixed (char* className = "STATIC")
            {
                this.Attach(PInvoke.CreateWindowEx(
                    0,
                    className,
                    null,
                    WINDOW_STYLE.WS_CHILD | WINDOW_STYLE.WS_CLIPCHILDREN,
                    0,
                    0,
                    0,
                    0,
                    this.Host.Hwnd,
                    HMENU.Null,
                    HINSTANCE.Null,
                    null));
            }

            this.CreateWebView();
        }

        private static void* Slot(
            void* instance,
            int slot)
        {
            return (*(void***)instance)[slot];
        }

        private void CreateWebView()
        {
            // The app's own folder: beside the app can be read-only
            string name = Path.GetFileNameWithoutExtension(Environment.ProcessPath) ?? "XForma";
            string folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), name, "WebView2");

            var environmentCreated = ComCallback.Create((error, environment) =>
            {
                this.OnEnvironmentCreated(error, (void*)environment);
            });

            try
            {
                int result = CreateCoreWebView2EnvironmentWithOptions(null, folder, null, environmentCreated.Pointer);
                if (result < 0)
                {
                    this.ReportFailure(result);
                }
            }
            catch (Exception ex) when ((ex is DllNotFoundException) || (ex is EntryPointNotFoundException))
            {
                FrameworkLog.Write(LogLevel.Warning, "WebView2Loader.dll wasn't found, so a WebView stays empty.", ex);
            }
        }

        private void ReportFailure(
            int result)
        {
            FrameworkLog.Write(LogLevel.Warning, $"WebView2 didn't start (0x{result:X8}), so a WebView stays empty. Is its runtime installed?");
        }

        private void OnEnvironmentCreated(
            int error,
            void* environment)
        {
            if (error < 0)
            {
                this.ReportFailure(error);
                return;
            }

            var controllerCreated = ComCallback.Create((result, controller) =>
            {
                this.OnControllerCreated(result, (void*)controller);
            });

            int created = ((delegate* unmanaged[Stdcall]<void*, HWND, void*, int>)Slot(environment, EnvironmentCreateController))(environment, this.Hwnd, controllerCreated.Pointer);
            if (created < 0)
            {
                this.ReportFailure(created);
            }
        }

        private void OnControllerCreated(
            int error,
            void* controller)
        {
            if (error < 0)
            {
                this.ReportFailure(error);
                return;
            }

            Marshal.AddRef((nint)controller);
            this._controller = controller;

            void* webView;
            ((delegate* unmanaged[Stdcall]<void*, void**, int>)Slot(controller, ControllerGetWebView))(controller, &webView);
            this._webView = webView;

            var completed = ComCallback.Create((sender, args) => this.OnNavigationCompleted((void*)args));
            long token;
            ((delegate* unmanaged[Stdcall]<void*, void*, long*, int>)Slot(webView, WebAddNavigationCompleted))(webView, completed.Pointer, &token);

            this.ApplyBounds();
            this._pending?.Invoke();
            this._pending = null;
        }

        private void OnNavigationCompleted(
            void* args)
        {
            int isSuccess = 0;
            ((delegate* unmanaged[Stdcall]<void*, int*, int>)Slot(args, ArgsGetIsSuccess))(args, &isSuccess);

            Uri uri = null;
            if (!this._isShowingString)
            {
                char* source;
                ((delegate* unmanaged[Stdcall]<void*, char**, int>)Slot(this._webView, WebGetSource))(this._webView, &source);
                string address = Marshal.PtrToStringUni((nint)source);
                Marshal.FreeCoTaskMem((nint)source);
                Uri.TryCreate(address, UriKind.Absolute, out uri);
            }

            this._view.OnNativeNavigated(uri, 0 != isSuccess);
        }

        protected override void OnWindowMoved(
            int width,
            int height,
            bool isShown)
        {
            this._width = width;
            this._height = height;
            this._isShown = isShown;
            this.ApplyBounds();
        }

        private void ApplyBounds()
        {
            if (null == this._controller)
            {
                return;
            }

            ((delegate* unmanaged[Stdcall]<void*, int, int>)Slot(this._controller, ControllerPutIsVisible))(this._controller, this._isShown ? 1 : 0);
            ((delegate* unmanaged[Stdcall]<void*, RECT, int>)Slot(this._controller, ControllerPutBounds))(this._controller, new RECT() { left = 0, top = 0, right = this._width, bottom = this._height });
            ((delegate* unmanaged[Stdcall]<void*, int>)Slot(this._controller, ControllerNotifyParentMoved))(this._controller);
        }

        // Runs now, or once WebView2 has started
        private void WhenReady(
            Action action)
        {
            if (null != this._webView)
            {
                action();
            }
            else
            {
                this._pending += action;
            }
        }

        public void Navigate(
            Uri uri)
        {
            string address = uri.AbsoluteUri;
            this.WhenReady(() =>
            {
                this._isShowingString = false;
                fixed (char* text = address)
                {
                    ((delegate* unmanaged[Stdcall]<void*, char*, int>)Slot(this._webView, WebNavigate))(this._webView, text);
                }
            });
        }

        public void NavigateToString(
            string html)
        {
            this.WhenReady(() =>
            {
                this._isShowingString = true;
                fixed (char* text = html)
                {
                    ((delegate* unmanaged[Stdcall]<void*, char*, int>)Slot(this._webView, WebNavigateToString))(this._webView, text);
                }
            });
        }

        private bool GetFlag(
            int slot)
        {
            if (null == this._webView)
            {
                return false;
            }

            int value = 0;
            ((delegate* unmanaged[Stdcall]<void*, int*, int>)Slot(this._webView, slot))(this._webView, &value);
            return 0 != value;
        }

        private void Call(
            int slot)
        {
            if (null != this._webView)
            {
                ((delegate* unmanaged[Stdcall]<void*, int>)Slot(this._webView, slot))(this._webView);
            }
        }

        public bool CanGoBack => this.GetFlag(WebGetCanGoBack);

        public bool CanGoForward => this.GetFlag(WebGetCanGoForward);

        public void GoBack() => this.Call(WebGoBack);

        public void GoForward() => this.Call(WebGoForward);

        public void Reload() => this.Call(WebReload);

        /// <summary>
        /// A COM object with one method, which all of WebView2's completion and
        /// event handlers are: IUnknown, then Invoke with two values (an HRESULT
        /// or a sender, then a result or event arguments). It stays alive while
        /// WebView2 holds it.
        /// </summary>
        private sealed class ComCallback
        {
            [StructLayout(LayoutKind.Sequential)]
            private struct Native
            {
                public void** Table;
                public int References;
                public nint Handle;
            }

            private static readonly void** Table = CreateTable();

            private readonly Action<int, nint> _invoke;

            private ComCallback(
                Action<int, nint> invoke)
            {
                this._invoke = invoke;
            }

            public void* Pointer
            {
                get;
                private set;
            }

            public static ComCallback Create(
                Action<int, nint> invoke)
            {
                var callback = new ComCallback(invoke);
                var native = (Native*)NativeMemory.Alloc((nuint)sizeof(Native));
                native->Table = Table;
                native->References = 1;
                native->Handle = GCHandle.ToIntPtr(GCHandle.Alloc(callback));
                callback.Pointer = native;
                return callback;
            }

            private static void** CreateTable()
            {
                var table = (void**)NativeMemory.Alloc((nuint)(4 * sizeof(void*)));
                table[0] = (delegate* unmanaged[Stdcall]<Native*, Guid*, void**, int>)&QueryInterface;
                table[1] = (delegate* unmanaged[Stdcall]<Native*, uint>)&AddRef;
                table[2] = (delegate* unmanaged[Stdcall]<Native*, uint>)&Release;
                table[3] = (delegate* unmanaged[Stdcall]<Native*, nint, nint, int>)&Invoke;
                return table;
            }

            [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
            private static int QueryInterface(
                Native* self,
                Guid* iid,
                void** result)
            {
                *result = self;
                self->References += 1;
                return 0;
            }

            [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
            private static uint AddRef(
                Native* self)
            {
                self->References += 1;
                return (uint)self->References;
            }

            [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
            private static uint Release(
                Native* self)
            {
                self->References -= 1;
                int references = self->References;
                if (0 == references)
                {
                    GCHandle.FromIntPtr(self->Handle).Free();
                    NativeMemory.Free(self);
                }

                return (uint)references;
            }

            [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
            private static int Invoke(
                Native* self,
                nint first,
                nint second)
            {
                try
                {
                    // The first value's an HRESULT or a pointer nobody reads; only
                    // its low half is set for an HRESULT
                    var callback = (ComCallback)GCHandle.FromIntPtr(self->Handle).Target;
                    callback._invoke((int)first, second);
                }
                catch (Exception ex)
                {
                    Application.ReportException(ex);
                }

                return 0;
            }
        }
    }
}
