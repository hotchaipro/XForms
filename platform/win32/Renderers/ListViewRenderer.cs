using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Direct2D;
using Windows.Win32.Graphics.Direct2D.Common;
using Windows.Win32.Graphics.DirectComposition;
using Windows.Win32.Graphics.Dxgi.Common;
using XForms.Controls;
using XForms.Layouts;
using XForms.Platform;

namespace XForms.Win32.Renderers
{
    /// <summary>
    /// A virtualized list: only lines in or near the viewport exist, item
    /// containers are recycled as lines scroll out of view, and the compositor
    /// scrolls the content. A line is one item in a vertical or horizontal
    /// list, and a row of items in a grid.
    /// </summary>
    internal sealed unsafe class ListViewRenderer : ViewRenderer, IListViewRenderer, IScrollableRenderer, IScrollContent, IPullToRefresh
    {
        // Scrolled-out content takes no input
        protected override bool ClipsHitTesting => true;

        // Lines are realized this far beyond the viewport, in dips
        private const float RealizationMargin = 300;
        private const float DefaultLineExtent = 48;

        // How far to pull to refresh, and the indicator's size, in dips
        private const float RefreshDistance = 72;
        private const float IndicatorSize = 40;

        private readonly IDCompositionVisual2* _contentVisual;
        private readonly ScrollController _scroller;
        private readonly List<float> _lineExtents = new List<float>();
        private readonly List<bool> _isLineMeasured = new List<bool>();
        private readonly Dictionary<int, ListViewItem> _realizedItems = new Dictionary<int, ListViewItem>();
        private readonly Stack<ListViewItem> _recycledContainers = new Stack<ListViewItem>();
        private float[] _lineStarts = Array.Empty<float>();
        private bool _areLineStartsValid;
        private ListViewSource _source;
        private ListLayout _layout = ListLayout.Vertical;

        // The list's size across the lines: its width, or its height for a
        // horizontal list
        private float _crossSize;

        private bool _isRefreshEnabled;
        private bool _isRefreshing;
        private float _pullDistance;
        private IDCompositionVisual3* _indicatorVisual;
        private IDCompositionRotateTransform* _indicatorRotation;

        public ListViewRenderer(
            ListView listView)
            : base(listView, isContainer: false)
        {
            IDCompositionVisual2* contentVisual;
            this.Host.CompositionDevice->CreateVisual(&contentVisual);
            this._contentVisual = contentVisual;
            this.Visual->AddVisual((IDCompositionVisual*)contentVisual, true, null);

            this._scroller = new ScrollController(this);
        }

        protected override IDCompositionVisual2* ChildVisual
        {
            get
            {
                return this._contentVisual;
            }
        }

        public override Point ChildOffset
        {
            get
            {
                return this.IsHorizontal ? new Point(-this._scroller.Offset, 0) : new Point(0, -this._scroller.Offset);
            }
        }

        public ScrollController Scroller
        {
            get
            {
                return this._scroller;
            }
        }

        public float FooterHeight
        {
            get;
            set;
        }

        private int Span
        {
            get
            {
                return this._layout.Columns;
            }
        }

        private int ItemCount
        {
            get
            {
                return this._source?.Count ?? 0;
            }
        }

        private int LineCount
        {
            get
            {
                return (this.ItemCount + this.Span - 1) / this.Span;
            }
        }

        public ListLayout Layout
        {
            set
            {
                if (value == this._layout)
                {
                    return;
                }

                this._layout = value;

                // Start over at the start, on the new axis
                this._contentVisual->SetOffsetX(0);
                this._contentVisual->SetOffsetY(0);
                this._scroller.SetOffset(0);
                this._crossSize = 0;
                this.ResetLines();
            }
        }

        #region IScrollContent

        public bool IsHorizontal
        {
            get
            {
                return this._layout.Orientation == Orientation.Horizontal;
            }
        }

        IDCompositionVisual2* IScrollContent.ContentVisual
        {
            get
            {
                return this._contentVisual;
            }
        }

        // NOTE: Along the axis the list scrolls on
        public float ViewportHeight
        {
            get
            {
                return this.IsHorizontal ? this.Frame.Width : this.Frame.Height;
            }
        }

        public float ContentHeight
        {
            get
            {
                this.EnsureLineStarts();
                int count = this._lineExtents.Count;
                float linesExtent = (count > 0) ? this._lineStarts[count - 1] + this._lineExtents[count - 1] : 0;
                return linesExtent + (this.IsHorizontal ? 0 : this.FooterHeight);
            }
        }

        public void OnScrollRange(
            float fromOffset,
            float toOffset)
        {
            this.RealizeLines(fromOffset, toOffset + this.ViewportHeight);
        }

        public void OnScrollSettled()
        {
            // Drop the lines the content passed over on its way here
            float offset = this._scroller.Offset;
            this.RealizeLines(offset, offset + this.ViewportHeight);
            this.Host.UpdateTextBoxPositions();
            this.Host.Flush();
        }

        #endregion IScrollContent

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

            this.ResetLines();
        }

        private void Source_CollectionChanged(
            object sender,
            NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                // In a list, an item is a line: shift the lines after it
                case NotifyCollectionChangedAction.Add when (e.NewStartingIndex >= 0) && (this.Span == 1):
                    this.ShiftLines(e.NewStartingIndex, e.NewItems?.Count ?? 1);
                    break;

                case NotifyCollectionChangedAction.Remove when (e.OldStartingIndex >= 0) && (this.Span == 1):
                    this.ShiftLines(e.OldStartingIndex, -(e.OldItems?.Count ?? 1));
                    break;

                default:
                    // Otherwise start over, keeping the scroll position
                    this.ResetLines();
                    break;
            }
        }

        private void ShiftLines(
            int index,
            int count)
        {
            if (count < 0)
            {
                for (int i = index; i < index - count; i += 1)
                {
                    this.RecycleItem(i);
                }

                this._lineExtents.RemoveRange(index, -count);
                this._isLineMeasured.RemoveRange(index, -count);
            }
            else
            {
                for (int i = 0; i < count; i += 1)
                {
                    this._lineExtents.Insert(index, DefaultLineExtent);
                    this._isLineMeasured.Insert(index, false);
                }
            }

            // Renumber the realized items after the change
            var shifted = new Dictionary<int, ListViewItem>();
            foreach (var pair in this._realizedItems)
            {
                shifted[(pair.Key >= index) ? pair.Key + count : pair.Key] = pair.Value;
            }

            this._realizedItems.Clear();
            foreach (var pair in shifted)
            {
                this._realizedItems[pair.Key] = pair.Value;
            }

            this._areLineStartsValid = false;
            this.Host.RequestLayout();
        }

        private void ResetLines()
        {
            foreach (var index in new List<int>(this._realizedItems.Keys))
            {
                this.RecycleItem(index);
            }

            this._lineExtents.Clear();
            this._isLineMeasured.Clear();

            int count = this.LineCount;
            for (int i = 0; i < count; i += 1)
            {
                this._lineExtents.Add(DefaultLineExtent);
                this._isLineMeasured.Add(false);
            }

            this._areLineStartsValid = false;
            this.Host.RequestLayout();
        }

        private void EnsureLineStarts()
        {
            if (this._areLineStartsValid)
            {
                return;
            }

            int count = this._lineExtents.Count;
            if (this._lineStarts.Length < count)
            {
                this._lineStarts = new float[Math.Max(count, this._lineStarts.Length * 2)];
            }

            float start = 0;
            for (int i = 0; i < count; i += 1)
            {
                this._lineStarts[i] = start;
                start += this._lineExtents[i];
            }

            this._areLineStartsValid = true;
        }

        private int LineAt(
            float position)
        {
            this.EnsureLineStarts();

            // Binary search for the last line starting at or before the position
            int low = 0;
            int high = this._lineExtents.Count - 1;
            while (low < high)
            {
                int middle = (low + high + 1) / 2;
                if (this._lineStarts[middle] <= position)
                {
                    low = middle;
                }
                else
                {
                    high = middle - 1;
                }
            }

            return low;
        }

        private void RealizeLines(
            float start,
            float end)
        {
            int count = this._lineExtents.Count;
            if ((null == this._source) || (count == 0) || (this._crossSize <= 0))
            {
                return;
            }

            start = Math.Max(0, start - RealizationMargin);
            end += RealizationMargin;

            int first = this.LineAt(start);
            int last = first;

            // Realize lines until the range is covered; measuring can change their extents
            for (int line = first; line < count; line += 1)
            {
                this.RealizeLine(line);
                last = line;

                this.EnsureLineStarts();
                if (this._lineStarts[line] + this._lineExtents[line] >= end)
                {
                    break;
                }
            }

            int span = this.Span;
            foreach (var index in new List<int>(this._realizedItems.Keys))
            {
                int line = index / span;
                if ((line < first) || (line > last))
                {
                    this.RecycleItem(index);
                }
            }

            this.ArrangeRealizedItems();
        }

        private void RealizeLine(
            int line)
        {
            int span = this.Span;
            int end = Math.Min(this.ItemCount, (line + 1) * span);
            for (int index = line * span; index < end; index += 1)
            {
                if (!this._realizedItems.ContainsKey(index))
                {
                    this.RealizeItem(index);
                    this._isLineMeasured[line] = false;
                }
            }

            if (!this._isLineMeasured[line])
            {
                this.MeasureLine(line);
            }
        }

        private void RealizeItem(
            int index)
        {
            var listView = (ListView)this.Element;
            var reuse = (this._recycledContainers.Count > 0) ? this._recycledContainers.Pop() : null;

            var container = this._source.GetItemContainer(listView, reuse, this._source[index]);
            this._realizedItems[index] = container;

            var renderer = (ViewRenderer)container.Renderer;
            if (renderer.ParentRenderer != this)
            {
                this.AddChild(renderer);
            }
            else
            {
                renderer.IsVisible = true;
            }
        }

        // The room each item has: a horizontal list's items are as tall as the
        // list; a grid's share its width
        private Size ItemAvailableSize
        {
            get
            {
                return this.IsHorizontal
                    ? new Size(float.PositiveInfinity, this._crossSize)
                    : new Size(this._crossSize / this.Span, float.PositiveInfinity);
            }
        }

        private void MeasureLine(
            int line)
        {
            int span = this.Span;
            int end = Math.Min(this.ItemCount, (line + 1) * span);
            float extent = 0;
            for (int index = line * span; index < end; index += 1)
            {
                var container = this._realizedItems[index];
                container.Measure(this.ItemAvailableSize);
                extent = Math.Max(extent, this.IsHorizontal ? container.DesiredSize.Width : container.DesiredSize.Height);
            }

            if (extent != this._lineExtents[line])
            {
                this._lineExtents[line] = extent;
                this._areLineStartsValid = false;
            }

            this._isLineMeasured[line] = true;
        }

        private void RecycleItem(
            int index)
        {
            if (this._realizedItems.Remove(index, out var container))
            {
                container.Recycle();

                // NOTE: Recycled items stay in the visual tree, hidden, to avoid
                // rebuilding visuals as they come back into use
                ((ViewRenderer)container.Renderer).IsVisible = false;
                this._recycledContainers.Push(container);
            }
        }

        private void ArrangeRealizedItems()
        {
            this.EnsureLineStarts();

            int span = this.Span;
            float cellWidth = this._crossSize / span;
            foreach (var pair in this._realizedItems)
            {
                int line = pair.Key / span;
                float start = this._lineStarts[line];
                float extent = this._lineExtents[line];

                if (this.IsHorizontal)
                {
                    pair.Value.Arrange(new Rect(start, 0, extent, this._crossSize));
                }
                else
                {
                    pair.Value.Arrange(new Rect((pair.Key % span) * cellWidth, start, cellWidth, extent));
                }
            }
        }

        public void ScrollToItem(
            object item)
        {
            int index = this._source?.IndexOf(item) ?? -1;
            int line = (index < 0) ? -1 : index / this.Span;
            if ((line < 0) || (line >= this._lineExtents.Count))
            {
                return;
            }

            this.EnsureLineStarts();
            float start = this._lineStarts[line];
            float end = start + this._lineExtents[line];
            float offset = this._scroller.TargetOffset;
            float visibleExtent = this.ViewportHeight - (this.IsHorizontal ? 0 : this.FooterHeight);

            if (start < offset)
            {
                this._scroller.ScrollTo(start, animated: true);
            }
            else if (end > offset + visibleExtent)
            {
                this._scroller.ScrollTo(end - visibleExtent, animated: true);
            }
        }

        public void ScrollBy(
            float delta)
        {
            this._scroller.ScrollBy(delta);
        }

        public override Size MeasureNative(
            Size availableSize)
        {
            // A list takes whatever (finite) room it is given
            return new Size(
                float.IsInfinity(availableSize.Width) ? 0 : availableSize.Width,
                float.IsInfinity(availableSize.Height) ? 0 : availableSize.Height);
        }

        public override void ArrangeNative(
            Rect frame)
        {
            base.ArrangeNative(frame);

            float scale = this.Host.Scale;
            this.Visual->SetClip(new D2D_RECT_F() { left = 0, top = 0, right = frame.Width * scale, bottom = frame.Height * scale });

            // A new size across can rewrap every item
            float crossSize = this.IsHorizontal ? frame.Height : frame.Width;
            if (crossSize != this._crossSize)
            {
                this._crossSize = crossSize;
                for (int i = 0; i < this._isLineMeasured.Count; i += 1)
                {
                    this._isLineMeasured[i] = false;
                }
            }
            else
            {
                // Items may have changed size themselves (such as a command bar opening)
                int span = this.Span;
                foreach (var index in this._realizedItems.Keys)
                {
                    this._isLineMeasured[index / span] = false;
                }
            }

            float offset = this._scroller.Offset;
            this.RealizeLines(offset, offset + this.ViewportHeight);
            this._scroller.Coerce();

            this.PlaceIndicator();
        }

        #region Pull to refresh

        public bool IsRefreshEnabled
        {
            set
            {
                this._isRefreshEnabled = value;
            }
        }

        public bool IsRefreshing
        {
            set
            {
                this._isRefreshing = value;
                this._pullDistance = 0;
                this.PlaceIndicator();
            }
        }

        bool IPullToRefresh.IsPullEnabled
        {
            get
            {
                return this._isRefreshEnabled && !this._isRefreshing && !this.IsHorizontal;
            }
        }

        void IPullToRefresh.OnPull(
            float distance)
        {
            this._pullDistance = distance;
            this.PlaceIndicator();
        }

        void IPullToRefresh.OnPullReleased()
        {
            bool refresh = this._pullDistance >= RefreshDistance;
            this._pullDistance = 0;

            if (refresh)
            {
                this._isRefreshing = true;
                ((ListView)this.Element).NotifyRefreshRequested();
            }

            this.PlaceIndicator();
        }

        // Shows the indicator following the pull (turning as it goes), or at
        // rest and spinning while refreshing, or hides it
        private void PlaceIndicator()
        {
            bool isShown = this._isRefreshing || (0 < this._pullDistance);
            if (!isShown && (null == this._indicatorVisual))
            {
                return;
            }

            this.EnsureIndicator();

            float scale = this.Host.Scale;
            float restTop = 16;
            float top = this._isRefreshing
                ? restTop
                : Math.Min(this._pullDistance, RefreshDistance) / RefreshDistance * (restTop + IndicatorSize) - IndicatorSize;

            this._indicatorVisual->SetOffsetX((float)Math.Round(((this.Frame.Width - IndicatorSize) / 2) * scale));
            this._indicatorVisual->SetOffsetY((float)Math.Round(top * scale));
            this._indicatorVisual->SetOpacity(isShown ? 1.0f : 0.0f);

            if (this._isRefreshing)
            {
                // Spins until the refresh is done
                const double Period = 0.9;
                IDCompositionAnimation* spin;
                this.Host.CompositionDevice->CreateAnimation(&spin);
                spin->AddCubic(0, 0, (float)(360 / Period), 0, 0);
                spin->AddRepeat(Period, Period);
                this._indicatorRotation->SetAngle(spin);
                spin->Release();
            }
            else
            {
                this._indicatorRotation->SetAngle(this._pullDistance / RefreshDistance * 270);
            }

            this.Host.RequestCommit();
        }

        private void EnsureIndicator()
        {
            if (null != this._indicatorVisual)
            {
                return;
            }

            var device = this.Host.CompositionDevice;
            float scale = this.Host.Scale;
            int size = (int)Math.Ceiling(IndicatorSize * scale);

            IDCompositionVisual2* visual;
            device->CreateVisual(&visual);
            ((Windows.Win32.System.Com.IUnknown*)visual)->QueryInterface(IDCompositionVisual3.IID_Guid, out var visual3).ThrowOnFailure();
            visual->Release();
            this._indicatorVisual = (IDCompositionVisual3*)visual3;

            // Turns around its center
            IDCompositionRotateTransform* rotation;
            device->CreateRotateTransform(&rotation);
            rotation->SetCenterX(size / 2.0f);
            rotation->SetCenterY(size / 2.0f);
            this._indicatorRotation = rotation;
            this._indicatorVisual->SetTransform((IDCompositionTransform*)rotation);

            IDCompositionSurface* surface;
            device->CreateSurface((uint)size, (uint)size, DXGI_FORMAT.DXGI_FORMAT_B8G8R8A8_UNORM, DXGI_ALPHA_MODE.DXGI_ALPHA_MODE_PREMULTIPLIED, &surface);
            this._indicatorVisual->SetContent((Windows.Win32.System.Com.IUnknown*)surface);

            surface->BeginDraw(null, ID2D1DeviceContext.IID_Guid, out var contextPointer, out var offset);
            var context = (ID2D1DeviceContext*)contextPointer;
            try
            {
                context->SetDpi(96, 96);
                context->SetTransform(new D2D_MATRIX_3X2_F()
                {
                    Anonymous = new D2D_MATRIX_3X2_F._Anonymous_e__Union()
                    {
                        Anonymous1 = new D2D_MATRIX_3X2_F._Anonymous_e__Union._Anonymous1_e__Struct()
                        {
                            m11 = scale,
                            m22 = scale,
                            dx = offset.X,
                            dy = offset.Y,
                        },
                    },
                });

                // A disc with an arc in the accent color, as on Android
                var colors = this.Element.ActualTheme.Colors;
                var drawContext = new DrawContext(context, new Rect(0, 0, 0, 0));
                drawContext.Clear(Colors.Transparent);
                float radius = IndicatorSize / 2;
                var center = new Point(radius, radius);
                drawContext.FillEllipse(center, radius - 1, radius - 1, colors.Surface);
                drawContext.DrawEllipse(center, radius - 1, radius - 1, colors.Outline, 1);
                drawContext.DrawArc(center, radius - 10, 0, 270, colors.Accent, 3);
                drawContext.Release();
            }
            finally
            {
                surface->EndDraw();
                context->Release();
                surface->Release();
            }

            // Above the items
            this.Visual->AddVisual((IDCompositionVisual*)this._indicatorVisual, false, null);
        }

        #endregion Pull to refresh
    }
}
