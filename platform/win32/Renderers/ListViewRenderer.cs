using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using Windows.Win32.Graphics.Direct2D.Common;
using Windows.Win32.Graphics.DirectComposition;
using XForms.Controls;

namespace XForms.Win32.Renderers
{
    /// <summary>
    /// A virtualized list: only rows in or near the viewport exist, item
    /// containers are recycled as rows scroll out of view, and the compositor
    /// scrolls the content.
    /// </summary>
    internal sealed unsafe class ListViewRenderer : ViewRenderer, IListViewRenderer, IScrollableRenderer, IScrollContent
    {
        // Rows are realized this far beyond the viewport, in dips
        private const float RealizationMargin = 300;
        private const float DefaultRowHeight = 48;

        private readonly IDCompositionVisual2* _contentVisual;
        private readonly ScrollController _scroller;
        private readonly List<float> _rowHeights = new List<float>();
        private readonly List<bool> _isRowMeasured = new List<bool>();
        private readonly Dictionary<int, ListViewItemContainer> _realizedRows = new Dictionary<int, ListViewItemContainer>();
        private readonly Stack<ListViewItemContainer> _recycledContainers = new Stack<ListViewItemContainer>();
        private float[] _rowTops = Array.Empty<float>();
        private bool _areRowTopsValid;
        private ListViewSource _source;
        private float _width;

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
                return new Point(0, -this._scroller.Offset);
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

        #region IScrollContent

        IDCompositionVisual2* IScrollContent.ContentVisual
        {
            get
            {
                return this._contentVisual;
            }
        }

        public float ViewportHeight
        {
            get
            {
                return this.Frame.Height;
            }
        }

        public float ContentHeight
        {
            get
            {
                this.EnsureRowTops();
                int count = this._rowHeights.Count;
                float rowsHeight = (count > 0) ? this._rowTops[count - 1] + this._rowHeights[count - 1] : 0;
                return rowsHeight + this.FooterHeight;
            }
        }

        public void OnScrollRange(
            float fromOffset,
            float toOffset)
        {
            this.RealizeRows(fromOffset, toOffset + this.ViewportHeight);
        }

        public void OnScrollSettled()
        {
            // Drop the rows the content passed over on its way here
            float offset = this._scroller.Offset;
            this.RealizeRows(offset, offset + this.ViewportHeight);
            this.Host.UpdateTextEntryPositions();
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

            this.ResetRows();
        }

        private void Source_CollectionChanged(
            object sender,
            NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add when (e.NewStartingIndex >= 0):
                    this.ShiftRows(e.NewStartingIndex, e.NewItems?.Count ?? 1);
                    break;

                case NotifyCollectionChangedAction.Remove when (e.OldStartingIndex >= 0):
                    this.ShiftRows(e.OldStartingIndex, -(e.OldItems?.Count ?? 1));
                    break;

                default:
                    // Replace, move and reset: start over, keeping the scroll position
                    this.ResetRows();
                    break;
            }
        }

        /// <summary>
        /// Inserts (positive count) or removes (negative count) rows at the index,
        /// keeping the measured heights and realized rows of everything else.
        /// </summary>
        private void ShiftRows(
            int index,
            int count)
        {
            if (count < 0)
            {
                for (int i = index; i < index - count; i += 1)
                {
                    this.RecycleRow(i);
                }

                this._rowHeights.RemoveRange(index, -count);
                this._isRowMeasured.RemoveRange(index, -count);
            }
            else
            {
                for (int i = 0; i < count; i += 1)
                {
                    this._rowHeights.Insert(index, DefaultRowHeight);
                    this._isRowMeasured.Insert(index, false);
                }
            }

            // Renumber the realized rows after the change
            var shifted = new Dictionary<int, ListViewItemContainer>();
            foreach (var pair in this._realizedRows)
            {
                shifted[(pair.Key >= index) ? pair.Key + count : pair.Key] = pair.Value;
            }

            this._realizedRows.Clear();
            foreach (var pair in shifted)
            {
                this._realizedRows[pair.Key] = pair.Value;
            }

            this._areRowTopsValid = false;
            this.Host.RequestLayout();
        }

        private void ResetRows()
        {
            foreach (var index in new List<int>(this._realizedRows.Keys))
            {
                this.RecycleRow(index);
            }

            this._rowHeights.Clear();
            this._isRowMeasured.Clear();

            int count = this._source?.Count ?? 0;
            for (int i = 0; i < count; i += 1)
            {
                this._rowHeights.Add(DefaultRowHeight);
                this._isRowMeasured.Add(false);
            }

            this._areRowTopsValid = false;
            this.Host.RequestLayout();
        }

        private void EnsureRowTops()
        {
            if (this._areRowTopsValid)
            {
                return;
            }

            int count = this._rowHeights.Count;
            if (this._rowTops.Length < count)
            {
                this._rowTops = new float[Math.Max(count, this._rowTops.Length * 2)];
            }

            float top = 0;
            for (int i = 0; i < count; i += 1)
            {
                this._rowTops[i] = top;
                top += this._rowHeights[i];
            }

            this._areRowTopsValid = true;
        }

        private int RowAt(
            float y)
        {
            this.EnsureRowTops();

            // Binary search for the last row starting at or above y
            int low = 0;
            int high = this._rowHeights.Count - 1;
            while (low < high)
            {
                int middle = (low + high + 1) / 2;
                if (this._rowTops[middle] <= y)
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

        /// <summary>
        /// Makes sure the rows covering the range exist and are laid out, and
        /// recycles the rest.
        /// </summary>
        private void RealizeRows(
            float top,
            float bottom)
        {
            var source = this._source;
            int count = this._rowHeights.Count;
            if ((null == source) || (count == 0) || (this._width <= 0))
            {
                return;
            }

            top = Math.Max(0, top - RealizationMargin);
            bottom += RealizationMargin;

            int first = this.RowAt(top);
            int last = first;

            // Realize rows until the range is covered; measuring can change heights
            for (int i = first; i < count; i += 1)
            {
                this.RealizeRow(i);
                last = i;

                this.EnsureRowTops();
                if (this._rowTops[i] + this._rowHeights[i] >= bottom)
                {
                    break;
                }
            }

            foreach (var index in new List<int>(this._realizedRows.Keys))
            {
                if ((index < first) || (index > last))
                {
                    this.RecycleRow(index);
                }
            }

            this.ArrangeRealizedRows();
        }

        private void RealizeRow(
            int index)
        {
            if (!this._realizedRows.TryGetValue(index, out var container))
            {
                var listView = (ListView)this.Element;
                var reuse = (this._recycledContainers.Count > 0) ? this._recycledContainers.Pop() : null;

                container = this._source.GetItemContainer(listView, reuse, this._source[index]);
                this._realizedRows[index] = container;

                var renderer = (ViewRenderer)container.Renderer;
                if (renderer.ParentRenderer != this)
                {
                    this.AddChild(renderer);
                }
                else
                {
                    renderer.IsVisible = true;
                }

                this._isRowMeasured[index] = false;
            }

            if (!this._isRowMeasured[index])
            {
                this.MeasureRow(index, container);
            }
        }

        private void MeasureRow(
            int index,
            ListViewItemContainer container)
        {
            container.Measure(new Size(this._width, float.PositiveInfinity));

            float height = container.DesiredSize.Height;
            if (height != this._rowHeights[index])
            {
                this._rowHeights[index] = height;
                this._areRowTopsValid = false;
            }

            this._isRowMeasured[index] = true;
        }

        private void RecycleRow(
            int index)
        {
            if (this._realizedRows.Remove(index, out var container))
            {
                container.Recycle();

                // NOTE: Recycled rows stay in the visual tree, hidden, to avoid
                // rebuilding visuals as they come back into use
                ((ViewRenderer)container.Renderer).IsVisible = false;
                this._recycledContainers.Push(container);
            }
        }

        private void ArrangeRealizedRows()
        {
            this.EnsureRowTops();

            foreach (var pair in this._realizedRows)
            {
                pair.Value.Arrange(new Rectangle(0, this._rowTops[pair.Key], this._width, this._rowHeights[pair.Key]));
            }
        }

        public void ScrollToItem(
            object item)
        {
            int index = this._source?.IndexOf(item) ?? -1;
            if ((index < 0) || (index >= this._rowHeights.Count))
            {
                return;
            }

            this.EnsureRowTops();
            float top = this._rowTops[index];
            float bottom = top + this._rowHeights[index];
            float offset = this._scroller.TargetOffset;
            float visibleHeight = this.ViewportHeight - this.FooterHeight;

            if (top < offset)
            {
                this._scroller.ScrollTo(top, animated: true);
            }
            else if (bottom > offset + visibleHeight)
            {
                this._scroller.ScrollTo(bottom - visibleHeight, animated: true);
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
            Rectangle frame)
        {
            base.ArrangeNative(frame);

            float scale = this.Host.Scale;
            this.Visual->SetClip(new D2D_RECT_F() { left = 0, top = 0, right = frame.Width * scale, bottom = frame.Height * scale });

            // A new width can rewrap every row
            if (frame.Width != this._width)
            {
                this._width = frame.Width;
                for (int i = 0; i < this._isRowMeasured.Count; i += 1)
                {
                    this._isRowMeasured[i] = false;
                }
            }
            else
            {
                // Rows may have changed size themselves (such as a command bar opening)
                foreach (var index in this._realizedRows.Keys)
                {
                    this._isRowMeasured[index] = false;
                }
            }

            float offset = this._scroller.Offset;
            this.RealizeRows(offset, offset + frame.Height);
            this._scroller.Coerce();
        }
    }
}
