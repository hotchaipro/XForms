using System;
using System.Collections.ObjectModel;
using System.Globalization;

namespace XForms.Layouts
{
    public enum GridUnitType
    {
        /// <summary>
        /// Sized to the content.
        /// </summary>
        Auto = 0,

        /// <summary>
        /// A fixed length.
        /// </summary>
        Pixel = 1,

        /// <summary>
        /// A share of the space left over by the other rows or columns.
        /// </summary>
        Star = 2,
    }

    /// <summary>
    /// The height of a grid row or the width of a column: Auto, a fixed length,
    /// or a weighted share of the remaining space (star).
    /// </summary>
    public readonly struct GridLength : IEquatable<GridLength>
    {
        public GridLength(
            float value,
            GridUnitType unitType = GridUnitType.Pixel)
        {
            if ((value < 0) || float.IsNaN(value) || float.IsInfinity(value))
            {
                throw new ArgumentOutOfRangeException(nameof(value));
            }

            this.Value = (unitType == GridUnitType.Auto) ? 0 : value;
            this.GridUnitType = unitType;
        }

        public float Value { get; }

        public GridUnitType GridUnitType { get; }

        public bool IsAuto => this.GridUnitType == GridUnitType.Auto;

        public bool IsAbsolute => this.GridUnitType == GridUnitType.Pixel;

        public bool IsStar => this.GridUnitType == GridUnitType.Star;

        public static GridLength Auto => new GridLength(0, GridUnitType.Auto);

        /// <summary>
        /// One share of the remaining space ("*").
        /// </summary>
        public static GridLength Star => new GridLength(1, GridUnitType.Star);

        /// <summary>
        /// Parses "Auto", "*", "2*" or a fixed length such as "100".
        /// </summary>
        public static GridLength Parse(
            string text)
        {
            text = text.Trim();
            if (string.Equals(text, "Auto", StringComparison.OrdinalIgnoreCase))
            {
                return Auto;
            }

            if (text.EndsWith("*", StringComparison.Ordinal))
            {
                string weight = text.Substring(0, text.Length - 1);
                return new GridLength((weight.Length == 0) ? 1 : float.Parse(weight, CultureInfo.InvariantCulture), GridUnitType.Star);
            }

            return new GridLength(float.Parse(text, CultureInfo.InvariantCulture));
        }

        public bool Equals(GridLength other) => (this.Value == other.Value) && (this.GridUnitType == other.GridUnitType);

        public override bool Equals(object obj) => (obj is GridLength other) && this.Equals(other);

        public override int GetHashCode() => HashCode.Combine(this.Value, this.GridUnitType);

        public static bool operator ==(GridLength a, GridLength b) => a.Equals(b);

        public static bool operator !=(GridLength a, GridLength b) => !a.Equals(b);

        public override string ToString()
        {
            switch (this.GridUnitType)
            {
                case GridUnitType.Auto:
                    return "Auto";

                case GridUnitType.Star:
                    return (this.Value == 1) ? "*" : this.Value.ToString(CultureInfo.InvariantCulture) + "*";

                default:
                    return this.Value.ToString(CultureInfo.InvariantCulture);
            }
        }
    }

    /// <summary>
    /// The cell a grid child occupies.
    /// </summary>
    public readonly struct GridCell
    {
        public GridCell(
            int row,
            int column,
            int rowSpan = 1,
            int columnSpan = 1)
        {
            if ((row < 0) || (column < 0))
            {
                throw new ArgumentOutOfRangeException((row < 0) ? nameof(row) : nameof(column));
            }

            if ((rowSpan < 1) || (columnSpan < 1))
            {
                throw new ArgumentOutOfRangeException((rowSpan < 1) ? nameof(rowSpan) : nameof(columnSpan));
            }

            this.Row = row;
            this.Column = column;
            this.RowSpan = rowSpan;
            this.ColumnSpan = columnSpan;
        }

        public int Row { get; }

        public int Column { get; }

        public int RowSpan { get; }

        public int ColumnSpan { get; }
    }

    /// <summary>
    /// Arranges its children in rows and columns. Each row and column is Auto
    /// (sized to its content), a fixed length, or a share of the remaining
    /// space (star). Without definitions, the grid has one star row and column.
    /// </summary>
    public class Grid : Panel
    {
        /// <summary>
        /// A grid's children, each in a cell.
        /// </summary>
        public sealed class GridChildren : ViewCollection<GridCell>
        {
            internal GridChildren(
                Grid owner)
                : base(owner, new GridCell(0, 0))
            {
            }

            /// <summary>
            /// Adds a child in a cell, optionally spanning more rows or columns.
            /// </summary>
            public void Add(
                View child,
                int row,
                int column,
                int rowSpan = 1,
                int columnSpan = 1)
            {
                this.Add(child, new GridCell(row, column, rowSpan, columnSpan));
            }
        }

        /// <summary>
        /// Row heights or column widths; changing them lays the grid out again.
        /// </summary>
        public sealed class DefinitionCollection : Collection<GridLength>
        {
            private readonly Grid _owner;

            internal DefinitionCollection(
                Grid owner)
            {
                this._owner = owner;
            }

            /// <summary>
            /// Adds a definition for each part of a list such as "Auto,*,2*,100".
            /// </summary>
            public void AddRange(
                string definitions)
            {
                foreach (var part in definitions.Split(','))
                {
                    this.Add(GridLength.Parse(part));
                }
            }

            protected override void InsertItem(int index, GridLength item)
            {
                base.InsertItem(index, item);
                this._owner.InvalidateMeasure();
            }

            protected override void SetItem(int index, GridLength item)
            {
                base.SetItem(index, item);
                this._owner.InvalidateMeasure();
            }

            protected override void RemoveItem(int index)
            {
                base.RemoveItem(index);
                this._owner.InvalidateMeasure();
            }

            protected override void ClearItems()
            {
                base.ClearItems();
                this._owner.InvalidateMeasure();
            }
        }

        private readonly GridChildren _children;
        private float _rowSpacing;
        private float _columnSpacing;

        // Content sizes of the Auto and star rows and columns, from the last measure
        private float[] _rowContent = Array.Empty<float>();
        private float[] _columnContent = Array.Empty<float>();

        public Grid()
        {
            this._children = new GridChildren(this);
            this.RowDefinitions = new DefinitionCollection(this);
            this.ColumnDefinitions = new DefinitionCollection(this);
        }

        public GridChildren Children
        {
            get
            {
                return this._children;
            }
        }

        public DefinitionCollection RowDefinitions
        {
            get;
        }

        public DefinitionCollection ColumnDefinitions
        {
            get;
        }

        /// <summary>
        /// The space between rows.
        /// </summary>
        public float RowSpacing
        {
            get
            {
                return this._rowSpacing;
            }

            set
            {
                this._rowSpacing = value;
                this.InvalidateMeasure();
            }
        }

        /// <summary>
        /// The space between columns.
        /// </summary>
        public float ColumnSpacing
        {
            get
            {
                return this._columnSpacing;
            }

            set
            {
                this._columnSpacing = value;
                this.InvalidateMeasure();
            }
        }

        private static GridLength[] Definitions(
            DefinitionCollection definitions)
        {
            if (definitions.Count == 0)
            {
                return new[] { GridLength.Star };
            }

            var result = new GridLength[definitions.Count];
            definitions.CopyTo(result, 0);
            return result;
        }

        // A child's cell, kept within the grid
        private static void Span(
            int start,
            int span,
            int count,
            out int first,
            out int last)
        {
            first = Math.Min(start, count - 1);
            last = Math.Min(first + span, count) - 1;
        }

        private static bool SpansStar(
            GridLength[] definitions,
            int first,
            int last)
        {
            for (int i = first; i <= last; i += 1)
            {
                if (definitions[i].IsStar)
                {
                    return true;
                }
            }

            return false;
        }

        // The length available to a child spanning the tracks: fixed tracks
        // count their length, the others count their size so far (or are
        // unlimited when not yet sized)
        private static float SpanLength(
            GridLength[] definitions,
            float[] sizes,
            bool[] isSized,
            int first,
            int last,
            float spacing)
        {
            float length = (last - first) * spacing;
            for (int i = first; i <= last; i += 1)
            {
                if (definitions[i].IsAbsolute)
                {
                    length += definitions[i].Value;
                }
                else if (isSized[i])
                {
                    length += sizes[i];
                }
                else
                {
                    return float.PositiveInfinity;
                }
            }

            return length;
        }

        // Grows the Auto (or star) tracks a child spans so they hold its length
        private static void Grow(
            GridLength[] definitions,
            float[] content,
            int first,
            int last,
            float length,
            float spacing,
            bool star)
        {
            float current = (last - first) * spacing;
            int growable = 0;
            for (int i = first; i <= last; i += 1)
            {
                current += definitions[i].IsAbsolute ? definitions[i].Value : content[i];
                if (definitions[i].IsStar == star && !definitions[i].IsAbsolute)
                {
                    growable += 1;
                }
            }

            if ((length <= current) || (growable == 0))
            {
                return;
            }

            float extra = (length - current) / growable;
            for (int i = first; i <= last; i += 1)
            {
                if (definitions[i].IsStar == star && !definitions[i].IsAbsolute)
                {
                    content[i] += extra;
                }
            }
        }

        // Final track sizes: fixed tracks keep their length, Auto tracks their
        // content, and star tracks share what's left (or, without a limit, are
        // as large as their content needs at the same size per share)
        private static float[] Resolve(
            GridLength[] definitions,
            float[] content,
            float available,
            float spacing)
        {
            int count = definitions.Length;
            var sizes = new float[count];
            float used = Math.Max(0, count - 1) * spacing;
            float stars = 0;
            float unit = 0;

            for (int i = 0; i < count; i += 1)
            {
                var definition = definitions[i];
                if (definition.IsStar)
                {
                    stars += definition.Value;
                    if (definition.Value > 0)
                    {
                        unit = Math.Max(unit, content[i] / definition.Value);
                    }
                }
                else
                {
                    sizes[i] = definition.IsAbsolute ? definition.Value : content[i];
                    used += sizes[i];
                }
            }

            if (!float.IsInfinity(available))
            {
                unit = (stars > 0) ? Math.Max(0, available - used) / stars : 0;
            }

            for (int i = 0; i < count; i += 1)
            {
                if (definitions[i].IsStar)
                {
                    sizes[i] = definitions[i].Value * unit;
                }
            }

            return sizes;
        }

        protected override Size MeasureChildren(
            Size availableSize)
        {
            var rows = Definitions(this.RowDefinitions);
            var columns = Definitions(this.ColumnDefinitions);
            var rowContent = new float[rows.Length];
            var columnContent = new float[columns.Length];
            var children = this.LayoutChildren;

            // Columns first: measure each child with the width its columns allow,
            // sizing Auto columns (and the content of star columns)
            var noneSized = new bool[Math.Max(rows.Length, columns.Length)];
            foreach (var child in children)
            {
                var cell = this._children.GetLayout(child);
                Span(cell.Column, cell.ColumnSpan, columns.Length, out int firstColumn, out int lastColumn);
                Span(cell.Row, cell.RowSpan, rows.Length, out int firstRow, out int lastRow);

                float width = SpanLength(columns, columnContent, noneSized, firstColumn, lastColumn, this._columnSpacing);
                float height = SpanLength(rows, rowContent, noneSized, firstRow, lastRow, this._rowSpacing);
                child.Measure(new Size(width, height));

                bool star = SpansStar(columns, firstColumn, lastColumn);
                Grow(columns, columnContent, firstColumn, lastColumn, child.DesiredSize.Width, this._columnSpacing, star);
            }

            var columnSizes = Resolve(columns, columnContent, availableSize.Width, this._columnSpacing);

            // Then rows: measure again with each child's final width, so wrapping
            // content reports its real height
            var columnsSized = new bool[columns.Length];
            for (int i = 0; i < columnsSized.Length; i += 1)
            {
                columnsSized[i] = true;
            }

            foreach (var child in children)
            {
                var cell = this._children.GetLayout(child);
                Span(cell.Column, cell.ColumnSpan, columns.Length, out int firstColumn, out int lastColumn);
                Span(cell.Row, cell.RowSpan, rows.Length, out int firstRow, out int lastRow);

                float width = SpanLength(columns, columnSizes, columnsSized, firstColumn, lastColumn, this._columnSpacing);
                float height = SpanLength(rows, rowContent, noneSized, firstRow, lastRow, this._rowSpacing);
                child.Measure(new Size(width, height));

                bool star = SpansStar(rows, firstRow, lastRow);
                Grow(rows, rowContent, firstRow, lastRow, child.DesiredSize.Height, this._rowSpacing, star);
            }

            this._rowContent = rowContent;
            this._columnContent = columnContent;

            // The grid asks for what its content needs; star tracks share any
            // extra space when the grid is arranged
            var rowSizes = Resolve(rows, rowContent, float.PositiveInfinity, this._rowSpacing);
            var contentColumnSizes = Resolve(columns, columnContent, float.PositiveInfinity, this._columnSpacing);

            return new Size(
                Sum(contentColumnSizes, this._columnSpacing),
                Sum(rowSizes, this._rowSpacing));
        }

        protected override void ArrangeChildren(
            Size finalSize)
        {
            var rows = Definitions(this.RowDefinitions);
            var columns = Definitions(this.ColumnDefinitions);

            if ((this._rowContent.Length != rows.Length) || (this._columnContent.Length != columns.Length))
            {
                // The definitions changed since the last measure
                this.MeasureChildren(finalSize);
            }

            var rowSizes = Resolve(rows, this._rowContent, finalSize.Height, this._rowSpacing);
            var columnSizes = Resolve(columns, this._columnContent, finalSize.Width, this._columnSpacing);

            foreach (var child in this.LayoutChildren)
            {
                var cell = this._children.GetLayout(child);
                Span(cell.Column, cell.ColumnSpan, columns.Length, out int firstColumn, out int lastColumn);
                Span(cell.Row, cell.RowSpan, rows.Length, out int firstRow, out int lastRow);

                float x = Offset(columnSizes, firstColumn, this._columnSpacing);
                float y = Offset(rowSizes, firstRow, this._rowSpacing);
                float width = Offset(columnSizes, lastColumn + 1, this._columnSpacing) - this._columnSpacing - x;
                float height = Offset(rowSizes, lastRow + 1, this._rowSpacing) - this._rowSpacing - y;

                child.Arrange(new Rect(x, y, Math.Max(0, width), Math.Max(0, height)));
            }
        }

        private static float Offset(
            float[] sizes,
            int index,
            float spacing)
        {
            float offset = 0;
            for (int i = 0; i < index; i += 1)
            {
                offset += sizes[i] + spacing;
            }

            return offset;
        }

        private static float Sum(
            float[] sizes,
            float spacing)
        {
            float sum = Math.Max(0, sizes.Length - 1) * spacing;
            foreach (var size in sizes)
            {
                sum += size;
            }

            return sum;
        }
    }
}
