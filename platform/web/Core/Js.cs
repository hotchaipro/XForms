using System;
using System.Runtime.InteropServices.JavaScript;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using XForms.Web.Renderers;
using XForms.Platform;

namespace XForms.Web
{
    /// <summary>
    /// The JavaScript side of the platform (wwwroot/xforms.js), and the events
    /// it sends back.
    /// </summary>
    [SupportedOSPlatform("browser")]
    internal static partial class Js
    {
        private const string Module = "xforms";

        public static async Task InitializeAsync()
        {
            // NOTE: Relative to the .NET runtime, in _framework
            await JSHost.ImportAsync(Module, "../_content/XForms.Framework.Web/xforms.js");
            await InitAsync();
        }

        [JSImport("init", Module)]
        private static partial Task InitAsync();

        // Elements

        [JSImport("create", Module)]
        public static partial int Create(string kind);

        [JSImport("destroy", Module)]
        public static partial void Destroy(int id);

        [JSImport("setRoot", Module)]
        public static partial void SetRoot(int id);

        [JSImport("insert", Module)]
        public static partial void Insert(int parentId, int childId, int index);

        [JSImport("insertBefore", Module)]
        public static partial void InsertBefore(int parentId, int childId, int beforeId);

        [JSImport("placeAccessible", Module)]
        public static partial void PlaceAccessible(int parentId, [JSMarshalAs<JSType.Array<JSType.Number>>] int[] ids);

        [JSImport("detach", Module)]
        public static partial void Detach(int id);

        [JSImport("setBounds", Module)]
        public static partial void SetBounds(int id, double x, double y, double width, double height);

        [JSImport("setStyle", Module)]
        public static partial void SetStyle(int id, string name, string value);

        [JSImport("setAttribute", Module)]
        public static partial void SetAttribute(int id, string name, string value);

        [JSImport("setText", Module)]
        public static partial void SetText(int id, string text);

        [JSImport("setRuns", Module)]
        public static partial void SetRuns(int id, string[] texts, string[] styles);

        [JSImport("setTitle", Module)]
        public static partial void SetTitle(string title);

        // Text

        [JSImport("measureText", Module)]
        public static partial double MeasureText(string font, string text);

        [JSImport("fontMetrics", Module)]
        [return: JSMarshalAs<JSType.Array<JSType.Number>>]
        public static partial double[] FontMetrics(string font);

        // Viewport, scheduling and theme

        [JSImport("viewport", Module)]
        [return: JSMarshalAs<JSType.Array<JSType.Number>>]
        public static partial double[] Viewport();

        [JSImport("requestFrame", Module)]
        public static partial void RequestFrame();

        [JSImport("post", Module)]
        public static partial void Post();

        // Dark (1), more contrast (2), forced colors (4)
        [JSImport("themeState", Module)]
        public static partial int ThemeState();

        // Images and drawing

        [JSImport("loadImage", Module)]
        public static partial void LoadImage(int imageId, string url);

        [JSImport("setImageSource", Module)]
        public static partial void SetImageSource(int id, string url, int imageId);

        [JSImport("draw", Module)]
        public static partial void Draw(
            int canvasId,
            double width,
            double height,
            [JSMarshalAs<JSType.Array<JSType.Number>>] double[] numbers,
            string[] strings);

        // Scrolling, text entry, overlays and fonts

        [JSImport("listenScroll", Module)]
        public static partial void ListenScroll(int id);

        [JSImport("scrollTo", Module)]
        public static partial void ScrollTo(int id, double left, double top, bool smooth);

        [JSImport("listenInput", Module)]
        public static partial void ListenInput(int id, bool isSingleLine);

        [JSImport("getValue", Module)]
        public static partial string GetValue(int id);

        [JSImport("setValue", Module)]
        public static partial void SetValue(int id, string value);

        [JSImport("getSelection", Module)]
        [return: JSMarshalAs<JSType.Array<JSType.Number>>]
        public static partial double[] GetSelection(int id);

        [JSImport("setSelection", Module)]
        public static partial void SetSelection(int id, int start, int end);

        [JSImport("focus", Module)]
        public static partial void Focus(int id);

        [JSImport("getRect", Module)]
        [return: JSMarshalAs<JSType.Array<JSType.Number>>]
        public static partial double[] GetRect(int id);

        [JSImport("addOverlay", Module)]
        public static partial void AddOverlay(int id);

        [JSImport("listenPress", Module)]
        public static partial void ListenPress(int id);

        [JSImport("registerFont", Module)]
        public static partial Task RegisterFontAsync(string family, string url);

        // The page

        [JSImport("setPageBackground", Module)]
        public static partial void SetPageBackground(string css);

        [JSImport("openUrl", Module)]
        public static partial void OpenUrl(string url);

        [JSImport("listenVisibility", Module)]
        public static partial void ListenVisibility();

        // Local storage, which keeps the app's files

        [JSImport("storageGet", Module)]
        public static partial string StorageGet(string key);

        [JSImport("storageSet", Module)]
        public static partial void StorageSet(string key, string value);

        // Keyboard, history, accessibility and pulls

        [JSImport("listenKeyboard", Module)]
        public static partial void ListenKeyboard();

        [JSImport("pushHistory", Module)]
        public static partial void PushHistory([JSMarshalAs<JSType.Array<JSType.String>>] string[] stack, bool popup);

        [JSImport("replaceHistory", Module)]
        public static partial void ReplaceHistory([JSMarshalAs<JSType.Array<JSType.String>>] string[] stack, bool popup);

        [JSImport("goHistory", Module)]
        public static partial void GoHistory(int delta);

        [JSImport("getStartStack", Module)]
        [return: JSMarshalAs<JSType.Array<JSType.String>>]
        public static partial string[] GetStartStack();

        [JSImport("listenHistory", Module)]
        public static partial void ListenHistory();

        [JSImport("installHotReload", Module)]
        public static partial void InstallHotReload(string capabilities);

        [JSImport("listenClick", Module)]
        public static partial void ListenClick(int id);

        [JSImport("listenPull", Module)]
        public static partial void ListenPull(int id);

        [JSImport("getScroll", Module)]
        [return: JSMarshalAs<JSType.Array<JSType.Number>>]
        public static partial double[] GetScroll(int id);

        // Events from the page

        [JSExport]
        internal static void OnResize()
        {
            WebHost.Current?.OnResize();
        }

        [JSExport]
        internal static void OnFrame(double time)
        {
            WebHost.Current?.OnFrame(time);
        }

        [JSExport]
        internal static void RunPosted()
        {
            WebHost.Current?.RunPosted();
        }

        [JSExport]
        internal static void OnThemeChanged()
        {
            WebHost.Current?.OnThemeChanged();
        }

        [JSExport]
        internal static void OnImageLoaded(int imageId, double width, double height)
        {
            WebImages.OnLoaded(imageId, width, height);
        }

        [JSExport]
        internal static bool OnPointer(int id, int phase, int pointerId, int deviceType, double x, double y, bool isInContact, int modifiers)
        {
            return WebHost.Current?.OnPointer(id, phase, pointerId, deviceType, (float)x, (float)y, isInContact, modifiers) ?? false;
        }

        [JSExport]
        internal static void OnHover(int id, int pointerId, int deviceType, double x, double y, int modifiers)
        {
            WebHost.Current?.OnHover(id, pointerId, deviceType, (float)x, (float)y, modifiers);
        }

        [JSExport]
        internal static void OnPointerLeave(int pointerId)
        {
            WebHost.Current?.OnPointerLeave(pointerId);
        }

        [JSExport]
        internal static void OnScroll(int id, double left, double top)
        {
            WebHost.Current?.OnElementEvent(id, (renderer) => (renderer as IScrollingRenderer)?.OnScrolled((float)left, (float)top));
        }

        [JSExport]
        internal static void OnInput(int id)
        {
            WebHost.Current?.OnElementEvent(id, (renderer) => (renderer as TextBoxRenderer)?.OnInput());
        }

        [JSExport]
        internal static void OnSelectionChanged(int id)
        {
            WebHost.Current?.OnElementEvent(id, (renderer) => (renderer as TextBoxRenderer)?.OnSelectionChanged());
        }

        [JSExport]
        internal static void OnSubmit(int id)
        {
            WebHost.Current?.OnElementEvent(id, (renderer) => (renderer as TextBoxRenderer)?.OnSubmit());
        }

        [JSExport]
        internal static void OnFocusChanged(int id, bool hasFocus)
        {
            WebHost.Current?.OnElementEvent(id, (renderer) =>
            {
                if (renderer.Element is View view)
                {
                    LayerHost.For(view).NativeFocusChanged(hasFocus, false);
                }
            });
        }

        [JSExport]
        internal static void OnBackdropPressed(int id)
        {
            WebHost.Current?.OnBackdropPressed(id);
        }

        [JSExport]
        internal static void OnVisibilityChanged(bool isHidden)
        {
            WebHost.Current?.OnVisibilityChanged(isHidden);
        }



        [JSExport]
        internal static bool OnKey(int id, string key, int modifiers, bool isDown, bool isBackTab)
        {
            return WebHost.Current?.OnKey(id, key, modifiers, isDown) ?? false;
        }

        [JSExport]
        internal static string ApplyHotReloadDeltas(
            string deltasJson)
        {
            return WebHotReload.ApplyDeltas(deltasJson);
        }

        [JSExport]
        internal static void OnHistoryPopped(
            bool isExpected,
            [JSMarshalAs<JSType.Array<JSType.String>>] string[] stack,
            bool popup,
            string addressPath)
        {
            WebHost.Current?.OnHistoryPopped(isExpected, stack, popup, addressPath);
        }

        [JSExport]
        internal static void OnAccessibilityClick(int id)
        {
            WebHost.Current?.OnAccessibilityClick(id);
        }

        [JSExport]
        internal static void OnPull(int id, double distance)
        {
            WebHost.Current?.OnElementEvent(id, (renderer) => (renderer as ItemsViewRenderer)?.OnPull((float)distance));
        }

        [JSExport]
        internal static void OnPullReleased(int id)
        {
            WebHost.Current?.OnElementEvent(id, (renderer) => (renderer as ItemsViewRenderer)?.OnPullReleased());
        }

        [JSExport]
        internal static void OnFontLoaded()
        {
            WebHost.Current?.RequestLayout();
        }

        [JSExport]
        internal static bool OnWheel(int id, double x, double y, double delta, int modifiers)
        {
            return WebHost.Current?.OnWheel(id, (float)x, (float)y, (float)delta, modifiers) ?? false;
        }
    }
}
