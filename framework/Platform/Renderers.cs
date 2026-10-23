using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using XForms.Primitives;

namespace XForms.Platform
{
    internal interface IElementRenderer
    {
        Element Element
        {
            get;
        }

        object NativeElement
        {
            get;
        }

        /// <summary>
        /// The element's effective theme, when its native object is created and
        /// whenever the theme changes.
        /// </summary>
        void OnThemeChanged(Theme theme);
    }

    internal interface IViewRenderer : IElementRenderer
    {
        bool IsVisible { get; set; }

        bool IsHitTestVisible { get; set; }

        /// <summary>
        /// Measures the native view within the available size (which excludes the
        /// view's margin and may be infinite). A native container measures its
        /// children by calling back into <see cref="View.MeasureContent"/>.
        /// </summary>
        Size MeasureNative(Size availableSize);

        /// <summary>
        /// Positions the native view at the frame, relative to its parent view. A
        /// native container arranges its children by calling back into
        /// <see cref="View.LayoutContent"/>.
        /// </summary>
        void ArrangeNative(Rect frame);

        /// <summary>
        /// Asks the platform to lay this view (and its ancestors) out again.
        /// </summary>
        void InvalidateMeasure();

        /// <summary>
        /// Whether the native view clips its children and drawing to its bounds.
        /// </summary>
        bool ClipToBounds { set; }

        /// <summary>
        /// Creates the view's animation layers, one per animator, outermost
        /// first: each wraps the next, and the innermost wraps the view's own
        /// content and children. Called once, when the native view is created,
        /// and only for views with animators.
        /// </summary>
        IAnimationLayer[] CreateAnimationLayers(int count);

        /// <summary>
        /// Adds the native object of a view inside lightweight descendants of
        /// this view, which have none of their own to add it to, at its place
        /// in drawing order among the views it hosts. Hosted views are drawn
        /// above the lightweight content and this view's own children.
        /// </summary>
        void AddHostedView(IViewRenderer child, int index);

        void RemoveHostedView(IViewRenderer child);

        /// <summary>
        /// Draws the lightweight views inside this view again.
        /// </summary>
        void InvalidateLayerContent();

        /// <summary>
        /// Moves keyboard focus to the view. Returns false where the platform
        /// has no keyboard focus for owner-drawn views.
        /// </summary>
        bool Focus();
    }

    /// <summary>
    /// A view that holds children: every view the platform doesn't render
    /// natively (panels, content and owner-drawn controls, pages). The view
    /// lays its children out; the platform keeps their native views in drawing
    /// order, fills the view's background, and draws a view that
    /// <see cref="LayerHost.DrawsContent"/> (through
    /// <see cref="LayerHost.Render"/>) beneath its children.
    /// </summary>
    internal interface IContainerRenderer : IViewRenderer
    {
        /// <summary>
        /// Fills the view beneath its children (transparent when the view draws
        /// itself).
        /// </summary>
        Color BackgroundColor { get; set; }

        /// <summary>
        /// Draws a view that draws itself again.
        /// </summary>
        void Invalidate();

        /// <summary>
        /// Inserts a child's native view at the index in drawing order (a
        /// lightweight child has none; it's drawn into the layer).
        /// </summary>
        void InsertChild(int index, IViewRenderer childRenderer);

        void RemoveChildAt(int index);

        void ClearChildren();
    }

    internal interface IFrameRenderer : IViewRenderer
    {
        /// <summary>
        /// Adds a page's native view, above the frame's other pages, if it
        /// isn't there. The frame shows and hides pages with their
        /// <see cref="View.IsVisible"/>.
        /// </summary>
        void AddPage(Page page);

        void RemovePage(Page page);

        /// <summary>
        /// Draws the page above the frame's other pages.
        /// </summary>
        void BringToFront(Page page);
    }

    internal interface ITextBlockRenderer : IViewRenderer
    {
        string Text { get; set; }

        TextAlignment HorizontalTextAlignment { get; set; }

        TextAlignment VerticalTextAlignment { get; set; }

        bool WordWrap { get; set; }

        bool TextTrimming { get; set; }

        Color ForegroundColor { get; set; }

        Color BackgroundColor { get; set; }

        float FontSize { get; set; }

        FontStyle FontStyle { get; set; }

        FontWeight FontWeight { get; set; }

        bool Strikethrough { get; set; }

        string FontFamily { set; }

        int MaxLines { set; }

        IReadOnlyList<TextSpan> Spans { set; }
    }

    internal interface ITextBoxRenderer : IViewRenderer
    {
        Color BackgroundColor { get; set; }

        /// <summary>
        /// The text's color.
        /// </summary>
        Color ForegroundColor { set; }

        /// <summary>
        /// The placeholder text's color.
        /// </summary>
        Color PlaceholderColor { set; }

        /// <summary>
        /// The caret's (and, where the platform has one, the selection's)
        /// color.
        /// </summary>
        Color AccentColor { set; }
        string Text { get; set; }
        string PlaceholderText { get; set; }
        bool Multiline { get; set; }
        KeyboardOptions KeyboardOptions { get; set; }

        /// <summary>
        /// Shows the text as dots, and offers no suggestions or copying.
        /// </summary>
        bool IsPassword { set; }

        /// <summary>
        /// The most characters the user can enter, or 0 for no limit.
        /// </summary>
        int MaxLength { set; }

        bool IsReadOnly { set; }

        /// <summary>
        /// The selected range, in UTF-16 code units; an empty range is the
        /// caret.
        /// </summary>
        int SelectionStart { get; }

        int SelectionLength { get; }

        void Select(int start, int length);

        new void Focus();
    }

    internal interface ITextBoxDelegate
    {
        /// <summary>
        /// Called by the platform after the user changes the text.
        /// </summary>
        void OnNativeTextChanged();

        /// <summary>
        /// Called by the platform when the user submits a single-line text box
        /// (Enter, or the keyboard's Done or Go key).
        /// </summary>
        void OnNativeSubmitted();

        /// <summary>
        /// Called by the platform after the selection (or the caret) moves.
        /// </summary>
        void OnNativeSelectionChanged();
    }

    internal interface IImageRenderer : IViewRenderer
    {
        void SetTint(Color color);

        void SetSource(ImageSource source);
    }

    internal interface IScrollViewerRenderer : IViewRenderer
    {
        View Content { get; set; }

        bool IsVerticalScrollingEnabled { get; set; }

        bool IsHorizontalScrollingEnabled { get; set; }

        float VerticalOffset { get; }

        void ScrollTo(float verticalOffset, bool animated);
    }

    internal interface IItemsViewRenderer : IElementRenderer
    {
        Color BackgroundColor { get; set; }

        /// <summary>
        /// The refresh indicator's color.
        /// </summary>
        Color AccentColor { set; }

        /// <summary>
        /// Space around the items, inside the scrolling area: the items start
        /// after the leading padding, and the last can scroll clear of
        /// whatever covers the trailing edge.
        /// </summary>
        Thickness Padding { set; }

        void Bind(ItemsViewSource source);

        void ScrollToItem(object item);

        /// <summary>
        /// The index of the first item in view, at the leading edge (0 when
        /// there's none).
        /// </summary>
        int FirstVisibleIndex { get; }

        /// <summary>
        /// Scrolls, without animating, so the item at the index is at the
        /// leading edge (as far as the list scrolls).
        /// </summary>
        void ScrollToIndex(int index);

        /// <summary>
        /// Lays the items out in a vertical list (the default), a horizontal
        /// one, or a grid.
        /// </summary>
        ListLayout Layout { set; }

        /// <summary>
        /// Whether pulling the list down past its top asks for a refresh. The
        /// platform calls <see cref="ItemsView.NotifyRefreshRequested"/>.
        /// </summary>
        bool IsRefreshEnabled { set; }

        /// <summary>
        /// Whether the refresh indicator shows.
        /// </summary>
        bool IsRefreshing { set; }
    }

    internal interface IPopupRenderer : IElementRenderer
    {
        View Content { get; set; }

        View PlacementTarget { get; set; }

        void Show();

        void Close();
    }

    internal interface IPopupDelegate
    {
        /// <summary>
        /// Called by the platform when the popup closes, including when the user
        /// dismisses it (by clicking outside it or pressing Escape).
        /// </summary>
        void NotifyClosed();
    }

    internal interface IBitmapRenderer : IElementRenderer
    {
        Task LoadAsync();
    }

    internal interface ITextLayoutRenderer : IDisposable
    {
        Size Size { get; }

        int LineCount { get; }
    }
}
