using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Runtime.Versioning;
using XForms.Primitives;
using XForms.Platform;

namespace XForms.Web.Renderers
{
    /// <summary>
    /// A renderer whose element scrolls natively.
    /// </summary>
    internal interface IScrollingRenderer
    {
        void OnScrolled(float left, float top);
    }

    /// <summary>
    /// A scrolling element holding the content (a layer of its own).
    /// </summary>
    [SupportedOSPlatform("browser")]
    internal sealed class ScrollViewerRenderer : ViewRenderer, IScrollViewerRenderer, IScrollingRenderer
    {
        private View _content;
        private bool _isVerticalScrollingEnabled = true;
        private bool _isHorizontalScrollingEnabled;
        private float _verticalOffset;

        public ScrollViewerRenderer(
            ScrollViewer scrollViewer)
            : base(scrollViewer, "div", isContainer: false)
        {
            this.ApplyOverflow();
            Js.ListenScroll(this.Id);
        }

        public View Content
        {
            get
            {
                return this._content;
            }

            set
            {
                this.ClearChildren();
                this._content = value;
                if (value?.Renderer is ViewRenderer child)
                {
                    this.AddChild(child);
                }
            }
        }

        public bool IsVerticalScrollingEnabled
        {
            get => this._isVerticalScrollingEnabled;
            set { this._isVerticalScrollingEnabled = value; this.ApplyOverflow(); this.InvalidateMeasure(); }
        }

        public bool IsHorizontalScrollingEnabled
        {
            get => this._isHorizontalScrollingEnabled;
            set { this._isHorizontalScrollingEnabled = value; this.ApplyOverflow(); this.InvalidateMeasure(); }
        }

        public float VerticalOffset
        {
            get
            {
                return this._verticalOffset;
            }
        }

        private void ApplyOverflow()
        {
            Js.SetStyle(this.Id, "overflow-y", this._isVerticalScrollingEnabled ? "auto" : "hidden");
            Js.SetStyle(this.Id, "overflow-x", this._isHorizontalScrollingEnabled ? "auto" : "hidden");

            // Touch pans the content here (the page itself doesn't pan)
            Js.SetStyle(this.Id, "touch-action", this._isHorizontalScrollingEnabled ? "pan-x pan-y" : "pan-y");
            Js.SetStyle(this.Id, "overscroll-behavior", "contain");
        }

        public void ScrollTo(
            float verticalOffset,
            bool animated)
        {
            Js.ScrollTo(this.Id, 0, verticalOffset, animated);
        }

        public void OnScrolled(
            float left,
            float top)
        {
            this._verticalOffset = top;
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

            var content = this._content;
            if (null != content)
            {
                content.Measure(new Size(
                    this._isHorizontalScrollingEnabled ? float.PositiveInfinity : frame.Width,
                    this._isVerticalScrollingEnabled ? float.PositiveInfinity : frame.Height));

                // Content smaller than the viewport still fills it
                content.Arrange(new Rect(
                    0,
                    0,
                    Math.Max(content.DesiredSize.Width, frame.Width),
                    Math.Max(content.DesiredSize.Height, frame.Height)));
            }
        }
    }

    /// <summary>
    /// A list in a scrolling element, stacked in lines (one item each, or a
    /// grid's row or column). Only the lines near the viewport have
    /// containers; a sizer element gives the element its scroll extent.
    /// </summary>
    [SupportedOSPlatform("browser")]
    internal sealed class ItemsViewRenderer : ViewRenderer, IItemsViewRenderer, IScrollingRenderer
    {
        // Lines this far outside the viewport keep containers
        private const float Margin = 400;

        // A line's extent before it is measured (when none has been)
        private const float DefaultExtent = 48;

        // How far a pull goes before it refreshes
        private const float RefreshDistance = 72;
        private const float IndicatorSize = 36;

        // The containers by item index, and those ready for reuse
        private readonly SortedDictionary<int, ItemContainer> _realized = new SortedDictionary<int, ItemContainer>();
        private readonly List<ItemContainer> _recycled = new List<ItemContainer>();

        // The lines' measured extents (NaN when not measured)
        private readonly List<float> _lineExtents = new List<float>();

        private readonly int _sizerId;
        private ItemsViewSource _source;
        private ListLayout _layout = ListLayout.Vertical;
        private Rect _viewport;
        private float _scrollOffset;

        // An item to scroll to once the list is laid out (ScrollToIndex)
        private int _pendingScrollIndex = -1;

        private bool _isRefreshEnabled;
        private bool _isRefreshing;
        private float _pullDistance;
        private int _indicatorId;

        public ItemsViewRenderer(
            ItemsView listView)
            : base(listView, "div", isContainer: false)
        {
            this.ApplyOverflow();
            Js.ListenScroll(this.Id);

            this._sizerId = Js.Create("div");
            Js.SetStyle(this._sizerId, "visibility", "hidden");
            Js.SetStyle(this._sizerId, "pointer-events", "none");
            Js.Insert(this.Id, this._sizerId, -1);
        }

        private Thickness _padding;
        private Color _accentColor;

        public Thickness Padding
        {
            set
            {
                this._padding = value;
                this.Realize();
            }
        }

        public Color AccentColor
        {
            set
            {
                this._accentColor = value;
                this.ApplyIndicatorColors();
            }
        }

        public override Color BackgroundColor
        {
            get
            {
                return base.BackgroundColor;
            }

            set
            {
                base.BackgroundColor = value;
                this.ApplyIndicatorColors();
            }
        }

        // The padding before the first line, after the last, and either side
        // across the lines
        private float LeadingPadding => this.IsHorizontal ? this._padding.Left : this._padding.Top;

        private float TrailingPadding => this.IsHorizontal ? this._padding.Right : this._padding.Bottom;

        private float CrossStartPadding => this.IsHorizontal ? this._padding.Top : this._padding.Left;

        private float CrossPadding => this.IsHorizontal ? this._padding.Vertical : this._padding.Horizontal;

        public ListLayout Layout
        {
            set
            {
                this._layout = value;
                this.ApplyOverflow();
                this.Reset();
            }
        }

        private bool IsHorizontal
        {
            get
            {
                return this._layout.Orientation == Orientation.Horizontal;
            }
        }

        private void ApplyOverflow()
        {
            Js.SetStyle(this.Id, "overflow-y", this.IsHorizontal ? "hidden" : "auto");
            Js.SetStyle(this.Id, "overflow-x", this.IsHorizontal ? "auto" : "hidden");
            Js.SetStyle(this.Id, "touch-action", this.IsHorizontal ? "pan-x" : "pan-y");
            Js.SetStyle(this.Id, "overscroll-behavior", "contain");
        }

        public void OnScrolled(
            float left,
            float top)
        {
            this._scrollOffset = this.IsHorizontal ? left : top;
            this.Realize();
        }

        public void Bind(
            ItemsViewSource source)
        {
            if (null != this._source)
            {
                this._source.CollectionChanged -= this.Source_CollectionChanged;
            }

            this._source = source;
            if (null != source)
            {
                source.CollectionChanged += this.Source_CollectionChanged;
            }

            this.Reset();
        }

        private void Source_CollectionChanged(
            object sender,
            NotifyCollectionChangedEventArgs e)
        {
            this.Reset();
        }

        // Every item may have changed: the containers rebind and the lines
        // are measured again
        private void Reset()
        {
            foreach (var index in new List<int>(this._realized.Keys))
            {
                this.Recycle(index);
            }

            this._lineExtents.Clear();
            this.InvalidateMeasure();
        }

        private int LineCount
        {
            get
            {
                int span = this._layout.Columns;
                return ((this._source?.Count ?? 0) + span - 1) / span;
            }
        }

        // The extent of a line not measured yet: the measured ones' average
        private float EstimatedExtent
        {
            get
            {
                float total = 0;
                int count = 0;
                foreach (var extent in this._lineExtents)
                {
                    if (!float.IsNaN(extent))
                    {
                        total += extent;
                        count += 1;
                    }
                }

                return (0 == count) ? DefaultExtent : (total / count);
            }
        }

        private float LineExtent(
            int line,
            float estimate)
        {
            return ((line < this._lineExtents.Count) && !float.IsNaN(this._lineExtents[line])) ? this._lineExtents[line] : estimate;
        }

        // Containers for the lines near the viewport, measured and placed;
        // the others' containers are recycled
        private void Realize()
        {
            int lines = this.LineCount;
            while (this._lineExtents.Count < lines)
            {
                this._lineExtents.Add(float.NaN);
            }

            if (this._lineExtents.Count > lines)
            {
                this._lineExtents.RemoveRange(lines, this._lineExtents.Count - lines);
            }

            float viewportExtent = this.IsHorizontal ? this._viewport.Width : this._viewport.Height;
            float low = this._scrollOffset - Margin;
            float high = this._scrollOffset + viewportExtent + Margin;
            int span = this._layout.Columns;
            float estimate = this.EstimatedExtent;

            // Lines leaving the range give up their containers first, for
            // the lines coming into it to reuse
            var needed = new HashSet<int>();
            float position = this.LeadingPadding;
            for (int line = 0; line < lines; line += 1)
            {
                float extent = this.LineExtent(line, estimate);
                if ((position + extent >= low) && (position <= high))
                {
                    needed.Add(line);
                }

                position += extent;
            }

            foreach (var index in new List<int>(this._realized.Keys))
            {
                if (!needed.Contains(index / span))
                {
                    this.Recycle(index);
                }
            }

            float crossSize = Math.Max(0, (this.IsHorizontal ? this._viewport.Height : this._viewport.Width) - this.CrossPadding);
            float crossStart = this.CrossStartPadding;
            float cellSize = crossSize / span;
            int count = this._source?.Count ?? 0;
            var kept = new HashSet<int>();
            position = this.LeadingPadding;
            for (int line = 0; line < lines; line += 1)
            {
                float extent = this.LineExtent(line, estimate);
                if ((position + extent >= low) && (position <= high))
                {
                    // The line's extent is its largest item's
                    extent = 0;
                    int first = line * span;
                    int last = Math.Min(first + span, count);
                    for (int index = first; index < last; index += 1)
                    {
                        var container = this.Realize(index);
                        container.Measure(this.IsHorizontal
                            ? new Size(float.PositiveInfinity, cellSize)
                            : new Size(cellSize, float.PositiveInfinity));
                        extent = Math.Max(extent, this.IsHorizontal ? container.DesiredSize.Width : container.DesiredSize.Height);
                        kept.Add(index);
                    }

                    for (int index = first; index < last; index += 1)
                    {
                        int column = index - first;
                        this._realized[index].Arrange(this.IsHorizontal
                            ? new Rect(position, crossStart + (column * cellSize), extent, cellSize)
                            : new Rect(crossStart + (column * cellSize), position, cellSize, extent));
                    }

                    this._lineExtents[line] = extent;
                }

                position += extent;
            }

            foreach (var index in new List<int>(this._realized.Keys))
            {
                if (!kept.Contains(index))
                {
                    this.Recycle(index);
                }
            }

            position += this.TrailingPadding;
            if (this.IsHorizontal)
            {
                Js.SetBounds(this._sizerId, 0, 0, position, 1);
            }
            else
            {
                Js.SetBounds(this._sizerId, 0, 0, 1, position);
            }

            // An item to scroll to, now that the content has its extent
            int pendingIndex = this._pendingScrollIndex;
            if ((pendingIndex >= 0) && (viewportExtent > 0))
            {
                this._pendingScrollIndex = -1;
                float target = 0;
                for (int line = 0; line < pendingIndex / span; line += 1)
                {
                    target += this.LineExtent(line, this.EstimatedExtent);
                }

                this._scrollOffset = target;
                Js.ScrollTo(this.Id, this.IsHorizontal ? target : 0, this.IsHorizontal ? 0 : target, false);
                this.Realize();
            }
        }

        // The item's container, bound to it (reusing a recycled one), placed
        // among the others in item order
        private ItemContainer Realize(
            int index)
        {
            if (this._realized.TryGetValue(index, out var container))
            {
                return container;
            }

            ItemContainer reuse = null;
            if (this._recycled.Count > 0)
            {
                reuse = this._recycled[this._recycled.Count - 1];
                this._recycled.RemoveAt(this._recycled.Count - 1);
            }

            container = this._source.GetItemContainer((ItemsView)this.Element, reuse, this._source[index]);
            if ((null != reuse) && (reuse != container))
            {
                this._recycled.Add(reuse);
            }

            int position = 0;
            foreach (var realized in this._realized.Keys)
            {
                if (realized < index)
                {
                    position += 1;
                }
            }

            this._realized[index] = container;
            this.InsertChild(position, (ViewRenderer)container.Renderer);
            return container;
        }

        private void Recycle(
            int index)
        {
            var container = this._realized[index];
            this._realized.Remove(index);
            this.RemoveChild((ViewRenderer)container.Renderer);
            container.Recycle();
            this._recycled.Add(container);
        }

        public int FirstVisibleIndex
        {
            get
            {
                float estimate = this.EstimatedExtent;
                float position = 0;
                int lines = this.LineCount;
                int line = 0;
                while ((line + 1 < lines) && (position + this.LineExtent(line, estimate) <= this._scrollOffset))
                {
                    position += this.LineExtent(line, estimate);
                    line += 1;
                }

                return line * this._layout.Columns;
            }
        }

        public void ScrollToIndex(
            int index)
        {
            if ((index < 0) || (index >= (this._source?.Count ?? 0)))
            {
                return;
            }

            // Once the list is laid out (a new list isn't yet)
            this._pendingScrollIndex = index;
            if ((this._viewport.Width > 0) && (this._viewport.Height > 0))
            {
                this.Realize();
            }
        }

        public void ScrollToItem(
            object item)
        {
            int index = this._source?.IndexOf(item) ?? -1;
            if (index < 0)
            {
                return;
            }

            // The item scrolls to just after the leading padding
            float estimate = this.EstimatedExtent;
            float position = 0;
            for (int line = 0; line < index / this._layout.Columns; line += 1)
            {
                position += this.LineExtent(line, estimate);
            }

            Js.ScrollTo(this.Id, this.IsHorizontal ? position : 0, this.IsHorizontal ? 0 : position, true);
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

            this._viewport = frame;
            this.Realize();
        }

        #region Pull to refresh

        public bool IsRefreshEnabled
        {
            set
            {
                if (value && !this._isRefreshEnabled)
                {
                    Js.ListenPull(this.Id);
                }

                this._isRefreshEnabled = value;
            }
        }

        public bool IsRefreshing
        {
            set
            {
                this._isRefreshing = value;
                this.PlaceIndicator(0);
            }
        }

        public void OnPull(
            float distance)
        {
            if (this._isRefreshEnabled && !this._isRefreshing && !this.IsHorizontal)
            {
                this._pullDistance = distance;
                this.PlaceIndicator(distance);
            }
        }

        public void OnPullReleased()
        {
            bool refresh = this._isRefreshEnabled && !this._isRefreshing && (this._pullDistance >= RefreshDistance);
            this._pullDistance = 0;
            if (refresh)
            {
                this._isRefreshing = true;
                ((ItemsView)this.Element).NotifyRefreshRequested();
            }

            this.PlaceIndicator(0);
        }

        // The indicator follows a pull down from the top, turning as it
        // goes, and spins while the list refreshes
        private void PlaceIndicator(
            float distance)
        {
            if ((0 == distance) && !this._isRefreshing)
            {
                if (0 != this._indicatorId)
                {
                    Js.SetStyle(this._indicatorId, "display", "none");
                }

                return;
            }

            if (0 == this._indicatorId)
            {
                this._indicatorId = Js.Create("div");
                Js.SetStyle(this._indicatorId, "border-radius", "50%");
                Js.SetStyle(this._indicatorId, "border-width", "3px");
                Js.SetStyle(this._indicatorId, "border-style", "solid");
                Js.SetStyle(this._indicatorId, "box-shadow", "0 1px 4px rgba(0, 0, 0, 0.3)");
                Js.SetStyle(this._indicatorId, "pointer-events", "none");
                Js.SetStyle(this._indicatorId, "z-index", "1");
                Js.SetAttribute(this._indicatorId, "aria-hidden", "true");
                Js.Insert(this.Id, this._indicatorId, -1);
                this.ApplyIndicatorColors();
            }

            float travel = this._isRefreshing ? RefreshDistance : Math.Min(distance, RefreshDistance * 1.5f);
            float x = (this._viewport.Width - IndicatorSize) / 2;
            float y = this._scrollOffset + (travel / 2) - (IndicatorSize / 2);
            Js.SetBounds(this._indicatorId, x, y, IndicatorSize, IndicatorSize);
            Js.SetStyle(this._indicatorId, "display", null);
            Js.SetStyle(this._indicatorId, "opacity", this._isRefreshing ? "1" : Math.Min(1, distance / RefreshDistance).ToString(CultureInfo.InvariantCulture));
            Js.SetStyle(this._indicatorId, "animation", this._isRefreshing ? "xforms-spin 0.8s linear infinite" : null);
            Js.SetStyle(this._indicatorId, "transform", this._isRefreshing ? null : $"rotate({(int)(distance * 3)}deg)");
        }

        private void ApplyIndicatorColors()
        {
            if (0 == this._indicatorId)
            {
                return;
            }

            Js.SetStyle(this._indicatorId, "background", Css(this.BackgroundColor));
            Js.SetStyle(this._indicatorId, "border-color", Css(this._accentColor));
            Js.SetStyle(this._indicatorId, "border-top-color", "transparent");
        }

        #endregion Pull to refresh
    }

    /// <summary>
    /// A text entry: an input element (or a text area, for several lines)
    /// inside the view's element. The inner element is replaced when it must
    /// change kind, keeping its text and settings.
    /// </summary>
    [SupportedOSPlatform("browser")]
    internal sealed class TextBoxRenderer : ViewRenderer, ITextBoxRenderer
    {
        private const float Padding = 6;
        private const float FontSize = 15;

        private int _inputId;
        private bool _isMultiline;
        private bool _isPassword;
        private bool _isReadOnly;
        private int _maxLength;
        private string _placeholder;
        private KeyboardOptions _keyboardOptions = KeyboardOptions.All;
        private Color _foregroundColor;
        private Color _placeholderColor;
        private Color _accentColor;

        public TextBoxRenderer(
            TextBox textBox)
            : base(textBox, "div", isContainer: false)
        {
            this.CreateInput(string.Empty);
        }

        private static string FontCss
        {
            get
            {
                return TextLayouts.Font(null, FontSize, FontWeight.Normal, FontStyle.Normal);
            }
        }

        private ITextBoxDelegate Delegate
        {
            get
            {
                return (ITextBoxDelegate)this.Element;
            }
        }

        private void CreateInput(
            string text)
        {
            if (0 != this._inputId)
            {
                this.Host.UnregisterAlias(this._inputId);
                Js.Destroy(this._inputId);
            }

            int id = Js.Create(this._isMultiline ? "textarea" : "input");
            this._inputId = id;
            this.Host.RegisterAlias(id, this);

            Js.SetStyle(id, "width", "100%");
            Js.SetStyle(id, "height", "100%");
            Js.SetStyle(id, "border", "none");
            Js.SetStyle(id, "outline", "none");
            Js.SetStyle(id, "background", "transparent");
            Js.SetStyle(id, "padding", Px(Padding));
            Js.SetStyle(id, "resize", "none");
            Js.SetStyle(id, "user-select", "text");
            Js.SetStyle(id, "-webkit-user-select", "text");
            Js.SetStyle(id, "touch-action", "manipulation");
            Js.SetStyle(id, "font", FontCss);
            if (!this._isMultiline)
            {
                Js.SetAttribute(id, "type", this._isPassword ? "password" : "text");
            }

            Js.SetAttribute(id, "placeholder", this._placeholder);
            Js.SetAttribute(id, "maxlength", (this._maxLength > 0) ? this._maxLength.ToString(System.Globalization.CultureInfo.InvariantCulture) : null);
            Js.SetAttribute(id, "readonly", this._isReadOnly ? string.Empty : null);
            this.ApplyKeyboardOptions();
            this.ApplyColors();

            Js.Insert(this.Id, id, -1);
            Js.SetValue(id, text ?? string.Empty);
            Js.ListenInput(id, !this._isMultiline);
        }

        public string Text
        {
            get => Js.GetValue(this._inputId);
            set => Js.SetValue(this._inputId, value ?? string.Empty);
        }

        public string PlaceholderText
        {
            get => this._placeholder;
            set { this._placeholder = value; Js.SetAttribute(this._inputId, "placeholder", value); }
        }

        public bool Multiline
        {
            get
            {
                return this._isMultiline;
            }

            set
            {
                if (value != this._isMultiline)
                {
                    string text = this.Text;
                    this._isMultiline = value;
                    this.CreateInput(text);
                    this.InvalidateMeasure();
                }
            }
        }

        public KeyboardOptions KeyboardOptions
        {
            get
            {
                return this._keyboardOptions;
            }

            set
            {
                this._keyboardOptions = value;
                this.ApplyKeyboardOptions();
            }
        }

        private void ApplyKeyboardOptions()
        {
            var options = this._keyboardOptions;
            Js.SetAttribute(this._inputId, "spellcheck", options.HasFlag(KeyboardOptions.SpellCheck) ? "true" : "false");
            Js.SetAttribute(this._inputId, "autocorrect", options.HasFlag(KeyboardOptions.Suggestions) ? "on" : "off");
            Js.SetAttribute(this._inputId, "autocomplete", options.HasFlag(KeyboardOptions.Suggestions) ? "on" : "off");
        }

        public bool IsPassword
        {
            set
            {
                this._isPassword = value;
                if (!this._isMultiline)
                {
                    Js.SetAttribute(this._inputId, "type", value ? "password" : "text");
                }
            }
        }

        public int MaxLength
        {
            set
            {
                this._maxLength = value;
                Js.SetAttribute(this._inputId, "maxlength", (value > 0) ? value.ToString(System.Globalization.CultureInfo.InvariantCulture) : null);
            }
        }

        public bool IsReadOnly
        {
            set
            {
                this._isReadOnly = value;
                Js.SetAttribute(this._inputId, "readonly", value ? string.Empty : null);
            }
        }

        public int SelectionStart
        {
            get
            {
                return (int)Js.GetSelection(this._inputId)[0];
            }
        }

        public int SelectionLength
        {
            get
            {
                var selection = Js.GetSelection(this._inputId);
                return (int)(selection[1] - selection[0]);
            }
        }

        public void Select(
            int start,
            int length)
        {
            Js.SetSelection(this._inputId, start, start + length);
        }

        void ITextBoxRenderer.Focus()
        {
            Js.Focus(this._inputId);
        }

        public override bool Focus()
        {
            Js.Focus(this._inputId);
            return true;
        }

        public void OnInput()
        {
            this.Delegate.OnNativeTextChanged();
        }

        public void OnSelectionChanged()
        {
            this.Delegate.OnNativeSelectionChanged();
        }

        public void OnSubmit()
        {
            this.Delegate.OnNativeSubmitted();
        }

        public override Size MeasureNative(
            Size availableSize)
        {
            var (ascent, descent) = TextLayouts.FontMetrics(FontCss);
            float lines = this._isMultiline ? 3 : 1;
            return new Size(
                float.IsInfinity(availableSize.Width) ? 200 : availableSize.Width,
                (float)Math.Ceiling(((ascent + descent) * 1.2f * lines) + (2 * Padding)));
        }

        public override Color BackgroundColor
        {
            get
            {
                return base.BackgroundColor;
            }

            set
            {
                base.BackgroundColor = value;
                this.ApplyColors();
            }
        }

        public Color ForegroundColor
        {
            set
            {
                this._foregroundColor = value;
                this.ApplyColors();
            }
        }

        public Color PlaceholderColor
        {
            set
            {
                this._placeholderColor = value;
                this.ApplyColors();
            }
        }

        public Color AccentColor
        {
            set
            {
                this._accentColor = value;
                this.ApplyColors();
            }
        }

        private void ApplyColors()
        {
            if (0 == this._inputId)
            {
                return;
            }

            Js.SetStyle(this._inputId, "color", Css(this._foregroundColor));
            Js.SetStyle(this._inputId, "background", Css(this.BackgroundColor));
            Js.SetStyle(this._inputId, "caret-color", Css(this._accentColor));

            // The page's style sheet colors the placeholder from this
            Js.SetStyle(this._inputId, "--xforms-placeholder", Css(this._placeholderColor));
        }
    }

    /// <summary>
    /// A popup: its content above the page, beside its placement target (or
    /// centered), over a backdrop that closes it when pressed.
    /// </summary>
    [SupportedOSPlatform("browser")]
    internal sealed class PopupRenderer : ElementRenderer, IPopupRenderer
    {
        private readonly int _backdropId;
        private View _content;
        private bool _isOpen;

        public PopupRenderer(
            Popup popup)
            : base(popup)
        {
            this._backdropId = Js.Create("div");
            Js.SetStyle(this._backdropId, "position", "fixed");
            Js.SetStyle(this._backdropId, "inset", "0");
            Js.SetStyle(this._backdropId, "width", "auto");
            Js.SetStyle(this._backdropId, "height", "auto");
            Js.SetStyle(this._backdropId, "display", "none");
            Js.ListenPress(this._backdropId);
        }

        public override object NativeElement
        {
            get
            {
                return this._backdropId;
            }
        }

        public View Content
        {
            get
            {
                return this._content;
            }

            set
            {
                this._content = value;
                if (value?.Renderer is ViewRenderer renderer)
                {
                    Js.Insert(this._backdropId, renderer.Id, -1);
                }
            }
        }

        public View PlacementTarget
        {
            get;
            set;
        }

        public bool IsLightDismissEnabled
        {
            get;
            set;
        } = true;

        public void Show()
        {
            if (this._isOpen)
            {
                return;
            }

            this._isOpen = true;
            var host = WebHost.Current;
            this._previousFocus = host.FocusedView;

            // Without light dismiss, presses outside the content go through
            // the backdrop to the page
            Js.SetStyle(this._backdropId, "pointer-events", this.IsLightDismissEnabled ? null : "none");
            if ((this._content?.Renderer is ViewRenderer content) && !this.IsLightDismissEnabled)
            {
                Js.SetStyle(content.Id, "pointer-events", "auto");
            }

            if (this.IsLightDismissEnabled)
            {
                host.RegisterBackdrop(this._backdropId, this.Close);
            }
            host.ViewportChanged += this.Layout;
            Js.AddOverlay(this._backdropId);
            Js.SetStyle(this._backdropId, "display", null);
            this.Layout();
        }

        public void Close()
        {
            if (!this._isOpen)
            {
                return;
            }

            this._isOpen = false;
            var host = WebHost.Current;
            host.UnregisterBackdrop(this._backdropId);
            host.ViewportChanged -= this.Layout;
            Js.SetStyle(this._backdropId, "display", "none");

            // Keyboard focus that was in the popup (or is nowhere now) goes
            // back to where it was before it opened, as on the desktop
            var previous = this._previousFocus;
            this._previousFocus = null;
            var focused = host.FocusedView;
            if ((null != previous) && ((null == focused) || this.Contains(focused)))
            {
                previous.Focus();
            }

            ((IPopupDelegate)this.Element).NotifyClosed();
        }

        private View _previousFocus;

        private bool Contains(
            View view)
        {
            for (; null != view; view = view.Parent as View)
            {
                if (view == this._content)
                {
                    return true;
                }
            }

            return false;
        }

        // Below the target, or above it when there's more room there
        private void Layout()
        {
            var content = this._content;
            if (null == content)
            {
                return;
            }

            var viewport = WebHost.Current.ViewportSize;
            var target = this.TargetRect();
            if (null == target)
            {
                content.Measure(viewport);
                var size = content.DesiredSize;
                content.Arrange(new Rect((viewport.Width - size.Width) / 2, (viewport.Height - size.Height) / 2, size.Width, size.Height));
                return;
            }

            var rect = target.Value;
            float below = viewport.Height - rect.Bottom;
            float above = rect.Y;
            bool isBelow = below >= above;
            content.Measure(new Size(viewport.Width, Math.Max(0, isBelow ? below : above)));
            var desired = content.DesiredSize;
            float width = Math.Max(desired.Width, rect.Width);
            float x = Math.Max(0, Math.Min(rect.X, viewport.Width - width));
            float y = isBelow ? rect.Bottom : rect.Y - desired.Height;
            content.Arrange(new Rect(x, y, width, desired.Height));
        }

        // The target's place in the viewport (a lightweight target is found
        // through its layer)
        private Rect? TargetRect()
        {
            var target = this.PlacementTarget;
            if (null == target)
            {
                return null;
            }

            if (LayerHost.For(target).IsLightweight)
            {
                if (!(LayerHost.For(target).Layer is ViewRenderer layer))
                {
                    return null;
                }

                var layerRect = Js.GetRect(layer.Id);
                var offset = LayerHost.For(target).OffsetInLayer;
                return new Rect((float)layerRect[0] + offset.X, (float)layerRect[1] + offset.Y, target.Bounds.Width, target.Bounds.Height);
            }

            if (!(target.Renderer is ViewRenderer renderer))
            {
                return null;
            }

            var r = Js.GetRect(renderer.Id);
            return new Rect((float)r[0], (float)r[1], (float)r[2], (float)r[3]);
        }
    }
}
