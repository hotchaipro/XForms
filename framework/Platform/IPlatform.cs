using System;
using XForms.Primitives;

namespace XForms.Platform
{
    public interface IPlatform
    {
        /// <summary>
        /// Whether the framework draws lightweight views into their layer with
        /// its own renderers (through <see cref="LayerHost"/>), rather than the
        /// platform creating every view's renderer and drawing lightweight
        /// views itself (as Win32 does).
        /// </summary>
        bool UsesLightweightRenderers { get; }

        IApplicationRenderer CreateApplicationRenderer(Application application);

        /// <summary>
        /// The renderer of a view that holds children (any view the platform
        /// doesn't render natively).
        /// </summary>
        IContainerRenderer CreateContainerRenderer(View view);
        ITextBlockRenderer CreateTextBlockRenderer(TextBlock label);

        IImageRenderer CreateImageRenderer(Image image);
        IBitmapRenderer CreateBitmapRenderer(Bitmap bitmap);
        IFrameRenderer CreateFrameRenderer(Frame frame);
        IItemsViewRenderer CreateItemsViewRenderer(ItemsView itemsView);
        ITextBoxRenderer CreateTextBoxRenderer(TextBox textEntry);
        IScrollViewerRenderer CreateScrollViewerRenderer(ScrollViewer scrollView);

        IPopupRenderer CreatePopupRenderer(Popup popup);
        void NavigateToUri(Uri uri);

        ITextLayoutRenderer CreateTextLayout(TextLayout layout);

        void RegisterFont(string familyName, string fileName);
    }
}
