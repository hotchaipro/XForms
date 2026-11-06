using System;
using XForma.Platform;

namespace XForma.Primitives
{
    /// <summary>
    /// Shows one of the platform's own views among XForma's, for what XForma
    /// doesn't draw itself, such as a map or a media player. It takes the room
    /// layout gives it (set its <see cref="View.Width"/> and
    /// <see cref="View.Height"/>, or let a panel stretch it).
    /// </summary>
    /// <remarks>
    /// The function returns the platform's view, so call it behind the app's own
    /// platform checks: an <c>Android.Views.View</c> on Android, a
    /// <c>UIKit.UIView</c> on iOS, a window handle (<see cref="IntPtr"/>, created
    /// with <c>WS_CHILD</c> or as a top-level window to adopt) on Windows, and an
    /// element (a <c>JSObject</c>) in the browser. On Windows, a native view is
    /// above everything XForma draws in the window, popups included; on the other
    /// platforms it's in drawing order among the views.
    /// </remarks>
    public class NativeViewHost : View
    {
        private readonly Func<object> _createNativeView;

        /// <summary>
        /// Hosts the view that the function creates, once, as this view is first
        /// shown.
        /// </summary>
        public NativeViewHost(
            Func<object> createNativeView)
        {
            this._createNativeView = createNativeView ?? throw new ArgumentNullException(nameof(createNativeView));
        }

        // A control whose platforms create the native view themselves
        private protected NativeViewHost()
        {
        }

        /// <summary>
        /// The platform's view, or null before it's created.
        /// </summary>
        public object NativeView
        {
            get
            {
                return this.CreatedRenderer?.NativeElement;
            }
        }

        // The platform's view, from the app's function
        [PlatformContract]
        internal object CreateNativeView()
        {
            return this._createNativeView();
        }

        // The platforms render it natively
        internal override bool IsLightweightType
        {
            get
            {
                return false;
            }
        }

        private protected override IElementRenderer CreateRenderer()
        {
            return this.Application.Platform.CreateNativeViewHostRenderer(this);
        }
    }
}
