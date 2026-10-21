using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Runtime.Versioning;
using XForms.Controls;
using XForms.Layouts;

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
    /// A list in a scrolling element. Every item has a container (no
    /// virtualization yet), stacked in a line or a grid.
    /// </summary>
    [SupportedOSPlatform("browser")]
    internal sealed class ListViewRenderer : ViewRenderer, IListViewRenderer, IScrollingRenderer
    {
        private readonly List<ListViewItem> _containers = new List<ListViewItem>();
        private ListViewSource _source;
        private ListLayout _layout = ListLayout.Vertical;

        public ListViewRenderer(
            ListView listView)
            : base(listView, "div", isContainer: false)
        {
            this.ApplyOverflow();
            Js.ListenScroll(this.Id);
        }

        public float FooterHeight
        {
            get;
            set;
        }

        public ListLayout Layout
        {
            set
            {
                this._layout = value;
                this.ApplyOverflow();
                this.InvalidateMeasure();
            }
        }

        public bool IsRefreshEnabled
        {
            set
            {
            }
        }

        public bool IsRefreshing
        {
            set
            {
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
        }

        public void Bind(
            ListViewSource source)
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

            this.Rebuild();
        }

        private void Source_CollectionChanged(
            object sender,
            NotifyCollectionChangedEventArgs e)
        {
            this.Rebuild();
        }

        // A container for every item, reusing those it has
        private void Rebuild()
        {
            var listView = (ListView)this.Element;
            int count = this._source?.Count ?? 0;
            for (int i = 0; i < count; i += 1)
            {
                var reuse = (i < this._containers.Count) ? this._containers[i] : null;
                var container = this._source.GetItemContainer(listView, reuse, this._source[i]);
                if (i < this._containers.Count)
                {
                    this._containers[i] = container;
                }
                else
                {
                    this._containers.Add(container);
                }

                var renderer = (ViewRenderer)container.Renderer;
                if (renderer.ParentRenderer != this)
                {
                    this.InsertChild(Math.Min(i, this.Children.Count), renderer);
                }
            }

            while (this._containers.Count > count)
            {
                var container = this._containers[this._containers.Count - 1];
                this._containers.RemoveAt(this._containers.Count - 1);
                container.Recycle();
                this.RemoveChild((ViewRenderer)container.Renderer);
            }

            this.InvalidateMeasure();
        }

        public void ScrollToItem(
            object item)
        {
            int index = this._source?.IndexOf(item) ?? -1;
            if ((index >= 0) && (index < this._containers.Count))
            {
                var frame = ((ViewRenderer)this._containers[index].Renderer).Frame;
                Js.ScrollTo(this.Id, frame.X, frame.Y, true);
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

            int span = this._layout.Columns;
            float cellWidth = frame.Width / span;
            float position = 0;
            for (int line = 0; (line * span) < this._containers.Count; line += 1)
            {
                float extent = 0;
                for (int column = 0; column < span; column += 1)
                {
                    int index = (line * span) + column;
                    if (index >= this._containers.Count)
                    {
                        break;
                    }

                    var container = this._containers[index];
                    if (this.IsHorizontal)
                    {
                        container.Measure(new Size(float.PositiveInfinity, frame.Height));
                        extent = container.DesiredSize.Width;
                        container.Arrange(new Rect(position, 0, extent, frame.Height));
                    }
                    else
                    {
                        container.Measure(new Size(cellWidth, float.PositiveInfinity));
                        extent = Math.Max(extent, container.DesiredSize.Height);
                    }
                }

                if (!this.IsHorizontal)
                {
                    for (int column = 0; column < span; column += 1)
                    {
                        int index = (line * span) + column;
                        if (index < this._containers.Count)
                        {
                            this._containers[index].Arrange(new Rect(column * cellWidth, position, cellWidth, extent));
                        }
                    }
                }

                position += extent;
            }
        }
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
        private Theme _theme;

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
            this.ApplyTheme();

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

        public override void OnThemeChanged(
            Theme theme)
        {
            base.OnThemeChanged(theme);
            this._theme = theme;
            this.ApplyTheme();
        }

        private void ApplyTheme()
        {
            var theme = this._theme;
            if (null == theme)
            {
                return;
            }

            Js.SetStyle(this._inputId, "color", Css(theme.Colors.Foreground));
            Js.SetStyle(this._inputId, "background", Css(theme.Colors.Surface));
            Js.SetStyle(this._inputId, "caret-color", Css(theme.Colors.Accent));
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

        public void Show()
        {
            if (this._isOpen)
            {
                return;
            }

            this._isOpen = true;
            var host = WebHost.Current;
            host.RegisterBackdrop(this._backdropId, this.Close);
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
            ((IPopupDelegate)this.Element).NotifyClosed();
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

            if (target.IsLightweight)
            {
                if (!(target.FindLayer() is ViewRenderer layer))
                {
                    return null;
                }

                var layerRect = Js.GetRect(layer.Id);
                var offset = target.OffsetInLayer;
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
