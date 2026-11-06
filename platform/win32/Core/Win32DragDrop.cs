using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Windows.Win32.Foundation;
using XForma.Platform;

namespace XForma.Win32
{
    /// <summary>
    /// Drag and drop through OLE, so drags go to and come from other apps: a drop
    /// target on each window, and a data object (the shell's) and a drop source
    /// for a drag that starts here. Called through COM's tables, as for the rest of
    /// this backend. Text, a URI and files come in; text and a URI go out.
    /// </summary>
    internal static unsafe class Win32DragDrop
    {
        private const short CF_UNICODETEXT = 13;
        private const short CF_HDROP = 15;
        private const uint TYMED_HGLOBAL = 1;
        private const uint DVASPECT_CONTENT = 1;
        private const uint DROPEFFECT_NONE = 0;
        private const uint DROPEFFECT_COPY = 1;
        private const uint DROPEFFECT_MOVE = 2;
        private const int DRAGDROP_S_DROP = 0x00040100;
        private const int DRAGDROP_S_CANCEL = 0x00040101;
        private const int DRAGDROP_S_USEDEFAULTCURSORS = 0x00040102;
        private const uint MK_LBUTTON = 1;
        private const uint GMEM_MOVEABLE = 2;
        private const int GetDataSlot = 3;
        private const int SetDataSlot = 7;

        private static readonly Guid IID_IDataObject = new Guid("0000010e-0000-0000-C000-000000000046");

        [StructLayout(LayoutKind.Sequential)]
        private struct POINTL
        {
            public int X;
            public int Y;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct FORMATETC
        {
            public short Format;
            public nint Device;
            public uint Aspect;
            public int Index;
            public uint Medium;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct STGMEDIUM
        {
            public uint Medium;
            public nint Handle;
            public nint ReleaseObject;
        }

        // A COM object written here: its table, its count, and what it stands for
        [StructLayout(LayoutKind.Sequential)]
        private struct ComObject
        {
            public void** Table;
            public int References;
            public nint Handle;
        }

        [DllImport("ole32.dll")]
        private static extern int OleInitialize(nint reserved);

        [DllImport("ole32.dll")]
        private static extern int RegisterDragDrop(HWND hwnd, void* target);

        [DllImport("ole32.dll")]
        private static extern int RevokeDragDrop(HWND hwnd);

        [DllImport("ole32.dll")]
        private static extern int DoDragDrop(void* data, void* source, uint allowedEffects, uint* effect);

        [DllImport("ole32.dll")]
        private static extern void ReleaseStgMedium(STGMEDIUM* medium);

        [DllImport("shell32.dll")]
        private static extern int SHCreateDataObject(nint folder, uint count, nint items, void* inner, Guid* iid, void** result);

        [DllImport("shell32.dll", CharSet = CharSet.Unicode, EntryPoint = "DragQueryFileW")]
        private static extern uint DragQueryFile(nint drop, uint index, char* file, uint length);

        [DllImport("kernel32.dll")]
        private static extern nint GlobalAlloc(uint flags, nuint bytes);

        [DllImport("kernel32.dll")]
        private static extern void* GlobalLock(nint memory);

        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GlobalUnlock(nint memory);

        [DllImport("user32.dll")]
        private static extern int ScreenToClient(HWND hwnd, POINTL* point);

        private static readonly void** TargetTable = CreateTargetTable();
        private static readonly void** SourceTable = CreateSourceTable();

        private static void** CreateTargetTable()
        {
            var table = (void**)NativeMemory.Alloc((nuint)(7 * sizeof(void*)));
            table[0] = (delegate* unmanaged[Stdcall]<ComObject*, Guid*, void**, int>)&QueryInterface;
            table[1] = (delegate* unmanaged[Stdcall]<ComObject*, uint>)&AddRef;
            table[2] = (delegate* unmanaged[Stdcall]<ComObject*, uint>)&Release;
            table[3] = (delegate* unmanaged[Stdcall]<ComObject*, void*, uint, POINTL, uint*, int>)&TargetDragEnter;
            table[4] = (delegate* unmanaged[Stdcall]<ComObject*, uint, POINTL, uint*, int>)&TargetDragOver;
            table[5] = (delegate* unmanaged[Stdcall]<ComObject*, int>)&TargetDragLeave;
            table[6] = (delegate* unmanaged[Stdcall]<ComObject*, void*, uint, POINTL, uint*, int>)&TargetDrop;
            return table;
        }

        private static void** CreateSourceTable()
        {
            var table = (void**)NativeMemory.Alloc((nuint)(5 * sizeof(void*)));
            table[0] = (delegate* unmanaged[Stdcall]<ComObject*, Guid*, void**, int>)&QueryInterface;
            table[1] = (delegate* unmanaged[Stdcall]<ComObject*, uint>)&AddRef;
            table[2] = (delegate* unmanaged[Stdcall]<ComObject*, uint>)&Release;
            table[3] = (delegate* unmanaged[Stdcall]<ComObject*, int, uint, int>)&SourceQueryContinueDrag;
            table[4] = (delegate* unmanaged[Stdcall]<ComObject*, uint, int>)&SourceGiveFeedback;
            return table;
        }

        private static ComObject* CreateObject(
            void** table,
            object target)
        {
            var native = (ComObject*)NativeMemory.Alloc((nuint)sizeof(ComObject));
            native->Table = table;
            native->References = 1;
            native->Handle = (null != target) ? GCHandle.ToIntPtr(GCHandle.Alloc(target)) : 0;
            return native;
        }

        [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
        private static int QueryInterface(
            ComObject* self,
            Guid* iid,
            void** result)
        {
            *result = self;
            self->References += 1;
            return 0;
        }

        [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
        private static uint AddRef(
            ComObject* self)
        {
            self->References += 1;
            return (uint)self->References;
        }

        [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
        private static uint Release(
            ComObject* self)
        {
            self->References -= 1;
            int references = self->References;
            if (0 == references)
            {
                if (0 != self->Handle)
                {
                    GCHandle.FromIntPtr(self->Handle).Free();
                }

                NativeMemory.Free(self);
            }

            return (uint)references;
        }

        // ── Drops on a window ──────────────────────────────────────────────

        /// <summary>
        /// Lets a window take drops. OLE needs initializing for it, on the UI
        /// thread (COM already is).
        /// </summary>
        public static void Register(
            Win32Host host)
        {
            OleInitialize(0);
            var target = CreateObject(TargetTable, host);
            RegisterDragDrop(host.Hwnd, target);
            ((delegate* unmanaged[Stdcall]<ComObject*, uint>)target->Table[2])(target);
        }

        public static void Revoke(
            Win32Host host)
        {
            RevokeDragDrop(host.Hwnd);
        }

        private static Win32Host HostOf(
            ComObject* self)
        {
            return (Win32Host)GCHandle.FromIntPtr(self->Handle).Target;
        }

        private static uint ToEffect(
            DragOperation operation)
        {
            return (operation == DragOperation.Move) ? DROPEFFECT_MOVE : (operation == DragOperation.Copy) ? DROPEFFECT_COPY : DROPEFFECT_NONE;
        }

        // Asks what's under the point (in the screen's pixels), and tells the app
        private static uint Update(
            Win32Host host,
            POINTL screen,
            DataPackage data,
            bool isDrop)
        {
            ScreenToClient(host.Hwnd, &screen);
            float scale = host.Scale;
            var (view, local) = host.ViewAt(new Point(screen.X / scale, screen.Y / scale));
            var application = ApplicationHost.Current;
            if (null == application)
            {
                return DROPEFFECT_NONE;
            }

            return ToEffect(isDrop ? application.DropDrag(view, local, data) : application.UpdateDrag(view, local, data));
        }

        // The package of the drag over the window, while it's over it
        [ThreadStatic]
        private static DataPackage _external;

        [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
        private static int TargetDragEnter(
            ComObject* self,
            void* data,
            uint keys,
            POINTL point,
            uint* effect)
        {
            try
            {
                _external = ReadData(data);
                *effect = Update(HostOf(self), point, _external, isDrop: false);
            }
            catch (Exception ex)
            {
                *effect = DROPEFFECT_NONE;
                Application.ReportException(ex);
            }

            return 0;
        }

        [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
        private static int TargetDragOver(
            ComObject* self,
            uint keys,
            POINTL point,
            uint* effect)
        {
            try
            {
                *effect = Update(HostOf(self), point, _external, isDrop: false);
            }
            catch (Exception ex)
            {
                *effect = DROPEFFECT_NONE;
                Application.ReportException(ex);
            }

            return 0;
        }

        [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
        private static int TargetDragLeave(
            ComObject* self)
        {
            try
            {
                _external = null;
                ApplicationHost.Current?.ExitDrag();
            }
            catch (Exception ex)
            {
                Application.ReportException(ex);
            }

            return 0;
        }

        [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
        private static int TargetDrop(
            ComObject* self,
            void* data,
            uint keys,
            POINTL point,
            uint* effect)
        {
            try
            {
                *effect = Update(HostOf(self), point, ReadData(data), isDrop: true);
            }
            catch (Exception ex)
            {
                *effect = DROPEFFECT_NONE;
                Application.ReportException(ex);
            }

            _external = null;
            return 0;
        }

        private static DataPackage ReadData(
            void* data)
        {
            var package = new DataPackage();
            var getData = (delegate* unmanaged[Stdcall]<void*, FORMATETC*, STGMEDIUM*, int>)(*(void***)data)[GetDataSlot];

            var format = new FORMATETC() { Format = CF_UNICODETEXT, Aspect = DVASPECT_CONTENT, Index = -1, Medium = TYMED_HGLOBAL };
            STGMEDIUM medium;
            if (getData(data, &format, &medium) >= 0)
            {
                var text = (char*)GlobalLock(medium.Handle);
                if (null != text)
                {
                    package.Text = new string(text);
                    GlobalUnlock(medium.Handle);
                }

                ReleaseStgMedium(&medium);
                if (Uri.TryCreate(package.Text, UriKind.Absolute, out var uri) && (uri.Scheme.StartsWith("http", StringComparison.OrdinalIgnoreCase)))
                {
                    package.Uri = uri;
                }
            }

            format.Format = CF_HDROP;
            if (getData(data, &format, &medium) >= 0)
            {
                var files = new List<DroppedFile>();
                uint count = DragQueryFile(medium.Handle, 0xFFFFFFFF, null, 0);
                for (uint i = 0; i < count; i += 1)
                {
                    uint length = DragQueryFile(medium.Handle, i, null, 0);
                    var buffer = new char[length + 1];
                    fixed (char* pointer = buffer)
                    {
                        DragQueryFile(medium.Handle, i, pointer, length + 1);
                    }

                    files.Add(new PathFile(new string(buffer, 0, (int)length)));
                }

                ReleaseStgMedium(&medium);
                package.Files = files;
            }

            return package;
        }

        private sealed class PathFile : DroppedFile
        {
            private readonly string _path;

            public PathFile(
                string path)
            {
                this._path = path;
            }

            public override string Name => Path.GetFileName(this._path);

            public override Task<Stream> OpenReadAsync()
            {
                return Task.FromResult<Stream>(File.OpenRead(this._path));
            }
        }

        // ── Drags from a window ────────────────────────────────────────────

        [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
        private static int SourceQueryContinueDrag(
            ComObject* self,
            int isEscapePressed,
            uint keys)
        {
            if (0 != isEscapePressed)
            {
                return DRAGDROP_S_CANCEL;
            }

            return (0 == (keys & MK_LBUTTON)) ? DRAGDROP_S_DROP : 0;
        }

        [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
        private static int SourceGiveFeedback(
            ComObject* self,
            uint effect)
        {
            return DRAGDROP_S_USEDEFAULTCURSORS;
        }

        /// <summary>
        /// Runs the drag until the user drops or cancels it: it has a message
        /// loop of its own. Returns what the drop did.
        /// </summary>
        public static DragOperation Run(
            DataPackage package)
        {
            OleInitialize(0);

            void* data;
            var iid = IID_IDataObject;
            if (SHCreateDataObject(0, 0, 0, null, &iid, &data) < 0)
            {
                return DragOperation.None;
            }

            try
            {
                string text = package.Text ?? package.Uri?.AbsoluteUri;
                if (null != text)
                {
                    SetText(data, text);
                }
                else
                {
                    // In this app only: an empty string, so that the drag's real
                    SetText(data, string.Empty);
                }

                var source = CreateObject(SourceTable, null);
                uint effect = DROPEFFECT_NONE;
                int result = DoDragDrop(data, source, DROPEFFECT_COPY | DROPEFFECT_MOVE, &effect);
                ((delegate* unmanaged[Stdcall]<ComObject*, uint>)source->Table[2])(source);
                if (result != DRAGDROP_S_DROP)
                {
                    return DragOperation.None;
                }

                return (0 != (effect & DROPEFFECT_MOVE)) ? DragOperation.Move : (0 != (effect & DROPEFFECT_COPY)) ? DragOperation.Copy : DragOperation.None;
            }
            finally
            {
                ((delegate* unmanaged[Stdcall]<void*, uint>)(*(void***)data)[2])(data);
            }
        }

        private static void SetText(
            void* data,
            string text)
        {
            nuint bytes = (nuint)((text.Length + 1) * sizeof(char));
            nint memory = GlobalAlloc(GMEM_MOVEABLE, bytes);
            var pointer = (char*)GlobalLock(memory);
            text.AsSpan().CopyTo(new Span<char>(pointer, text.Length));
            pointer[text.Length] = '\0';
            GlobalUnlock(memory);

            var format = new FORMATETC() { Format = CF_UNICODETEXT, Aspect = DVASPECT_CONTENT, Index = -1, Medium = TYMED_HGLOBAL };
            var medium = new STGMEDIUM() { Medium = TYMED_HGLOBAL, Handle = memory };
            ((delegate* unmanaged[Stdcall]<void*, FORMATETC*, STGMEDIUM*, int, int>)(*(void***)data)[SetDataSlot])(data, &format, &medium, 1);
        }
    }
}
