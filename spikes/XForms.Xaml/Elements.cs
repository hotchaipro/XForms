using System;
using System.Collections.Generic;
using System.Windows.Input;
using XForms.Controls;
using XForms.Input;
using XForms.Layouts;

namespace XForms.Xaml
{
    // The XAML elements the spike supports, built on public framework API.
    //
    // GAP: A custom layout can't manage its own children through public API
    // (LayoutViewCollection's constructor and Layout's child methods are
    // internal). WORKAROUND: the panels derive from DistributedStackLayout,
    // whose child collection is public, and replace its layout logic.

    /// <summary>
    /// Stacks children vertically or horizontally, with spacing.
    /// </summary>
    public class StackPanel : UniformGrid
    {
        public bool IsHorizontal { get; set; }

        public float Spacing { get; set; }

        protected override Size MeasureChildren(
            Size availableSize)
        {
            var children = this.LayoutChildren;
            float main = 0, cross = 0;
            int visible = 0;

            foreach (var child in children)
            {
                if (!child.IsVisible)
                {
                    continue;
                }

                child.Measure(this.IsHorizontal
                    ? new Size(float.PositiveInfinity, availableSize.Height)
                    : new Size(availableSize.Width, float.PositiveInfinity));

                var size = child.DesiredSize;
                main += this.IsHorizontal ? size.Width : size.Height;
                cross = Math.Max(cross, this.IsHorizontal ? size.Height : size.Width);
                visible += 1;
            }

            main += Math.Max(0, visible - 1) * this.Spacing;
            return this.IsHorizontal ? new Size(main, cross) : new Size(cross, main);
        }

        protected override void ArrangeChildren(
            Size finalSize)
        {
            float offset = 0;
            foreach (var child in this.LayoutChildren)
            {
                if (!child.IsVisible)
                {
                    continue;
                }

                var size = child.DesiredSize;
                if (this.IsHorizontal)
                {
                    child.Arrange(new Rect(offset, 0, size.Width, finalSize.Height));
                    offset += size.Width + this.Spacing;
                }
                else
                {
                    child.Arrange(new Rect(0, offset, finalSize.Width, size.Height));
                    offset += size.Height + this.Spacing;
                }
            }
        }
    }

    /// <summary>
    /// A grid with Auto, star and fixed rows and columns.
    /// </summary>
    public class Grid : UniformGrid
    {
        public static readonly Element.AttachedProperty<int> RowProperty = new Element.AttachedProperty<int>();
        public static readonly Element.AttachedProperty<int> ColumnProperty = new Element.AttachedProperty<int>();

        public List<GridLength> Rows { get; } = new List<GridLength>();

        public List<GridLength> Columns { get; } = new List<GridLength>();

        private float[] _rowSizes = Array.Empty<float>();
        private float[] _columnSizes = Array.Empty<float>();

        private static int Get(Element.AttachedProperty<int> property, View view, int count)
        {
            property.TryGetValue(view, out int value);
            return Math.Max(0, Math.Min(value, Math.Max(0, count - 1)));
        }

        private static float[] Resolve(List<GridLength> definitions, float available, Func<int, float> autoSize)
        {
            int count = Math.Max(1, definitions.Count);
            var sizes = new float[count];
            float used = 0, stars = 0;

            for (int i = 0; i < count; i += 1)
            {
                var length = (i < definitions.Count) ? definitions[i] : GridLength.Star(1);
                if (length.IsAuto)
                {
                    sizes[i] = autoSize(i);
                    used += sizes[i];
                }
                else if (length.IsStar)
                {
                    stars += length.Value;
                }
                else
                {
                    sizes[i] = length.Value;
                    used += sizes[i];
                }
            }

            float remaining = float.IsInfinity(available) ? 0 : Math.Max(0, available - used);
            for (int i = 0; i < count; i += 1)
            {
                var length = (i < definitions.Count) ? definitions[i] : GridLength.Star(1);
                if (length.IsStar)
                {
                    sizes[i] = (stars > 0) ? remaining * length.Value / stars : 0;
                }
            }

            return sizes;
        }

        protected override Size MeasureChildren(
            Size availableSize)
        {
            int rowCount = Math.Max(1, this.Rows.Count);
            int columnCount = Math.Max(1, this.Columns.Count);
            var rowAuto = new float[rowCount];
            var columnAuto = new float[columnCount];

            foreach (var child in this.LayoutChildren)
            {
                child.Measure(availableSize);
                int row = Get(RowProperty, child, rowCount);
                int column = Get(ColumnProperty, child, columnCount);
                rowAuto[row] = Math.Max(rowAuto[row], child.DesiredSize.Height);
                columnAuto[column] = Math.Max(columnAuto[column], child.DesiredSize.Width);
            }

            this._columnSizes = Resolve(this.Columns, availableSize.Width, (i) => columnAuto[i]);
            this._rowSizes = Resolve(this.Rows, availableSize.Height, (i) => rowAuto[i]);

            float width = 0, height = 0;
            foreach (var size in this._columnSizes) width += size;
            foreach (var size in this._rowSizes) height += size;
            return new Size(width, height);
        }

        protected override void ArrangeChildren(
            Size finalSize)
        {
            this._columnSizes = Resolve(this.Columns, finalSize.Width, (i) => this._columnSizes[Math.Min(i, this._columnSizes.Length - 1)]);
            this._rowSizes = Resolve(this.Rows, finalSize.Height, (i) => this._rowSizes[Math.Min(i, this._rowSizes.Length - 1)]);

            foreach (var child in this.LayoutChildren)
            {
                int row = Get(RowProperty, child, this._rowSizes.Length);
                int column = Get(ColumnProperty, child, this._columnSizes.Length);

                float x = 0, y = 0;
                for (int i = 0; i < column; i += 1) x += this._columnSizes[i];
                for (int i = 0; i < row; i += 1) y += this._rowSizes[i];

                child.Arrange(new Rect(x, y, this._columnSizes[column], this._rowSizes[row]));
            }
        }
    }

    public readonly struct GridLength
    {
        private GridLength(float value, bool isAuto, bool isStar)
        {
            this.Value = value;
            this.IsAuto = isAuto;
            this.IsStar = isStar;
        }

        public float Value { get; }

        public bool IsAuto { get; }

        public bool IsStar { get; }

        public static GridLength Auto => new GridLength(0, true, false);

        public static GridLength Star(float value) => new GridLength(value, false, true);

        public static GridLength Fixed(float value) => new GridLength(value, false, false);

        public static GridLength Parse(string text)
        {
            text = text.Trim();
            if (string.Equals(text, "Auto", StringComparison.OrdinalIgnoreCase))
            {
                return Auto;
            }

            if (text.EndsWith("*"))
            {
                string number = text.Substring(0, text.Length - 1);
                return Star(number.Length == 0 ? 1 : float.Parse(number, System.Globalization.CultureInfo.InvariantCulture));
            }

            return Fixed(float.Parse(text, System.Globalization.CultureInfo.InvariantCulture));
        }
    }

    /// <summary>
    /// A background and border around one child, with padding.
    /// </summary>
    /// <remarks>
    /// GAP: No rounded rectangles in the drawing API. WORKAROUND: CornerRadius
    /// is accepted but drawn square.
    /// GAP: No Padding on views. WORKAROUND: padding is the child's margin.
    /// </remarks>
    public class Border : UserControl
    {
        // GAP: Control.BackgroundColor defaults to the theme's background, and the
        // Android renderer paints it behind owner-drawn controls (Win32 doesn't).
        // WORKAROUND: owner-drawn spike elements report a transparent background.
        public override Color BackgroundColor => Colors.Transparent;

        public Color Background { get; set; } = Colors.Transparent;

        public Color BorderBrush { get; set; } = Colors.Transparent;

        public float BorderThickness { get; set; }

        public float CornerRadius { get; set; }

        public Thickness Padding { get; set; }

        public View Child
        {
            get
            {
                return this.Content;
            }

            set
            {
                if (null != value)
                {
                    value.Margin = new Thickness(
                        this.Padding.Left + this.BorderThickness,
                        this.Padding.Top + this.BorderThickness,
                        this.Padding.Right + this.BorderThickness,
                        this.Padding.Bottom + this.BorderThickness);
                }

                this.Content = value;
            }
        }

        protected override void DrawBackground(
            IDrawContext drawContext,
            Rect bounds)
        {
            drawContext.Clear(Colors.Transparent);
            drawContext.FillRectangle(bounds, this.Background);
            if (this.BorderThickness > 0)
            {
                drawContext.DrawRectangle(bounds.Deflate(this.BorderThickness / 2, this.BorderThickness / 2), this.BorderBrush, this.BorderThickness);
            }
        }
    }

    /// <summary>
    /// Where a template puts its templated parent's content.
    /// </summary>
    public class ContentPresenter : UserControl
    {
        // GAP: Control.BackgroundColor defaults to the theme's background, and the
        // Android renderer paints it behind owner-drawn controls (Win32 doesn't).
        // WORKAROUND: owner-drawn spike elements report a transparent background.
        public override Color BackgroundColor => Colors.Transparent;

        public View Presented
        {
            get
            {
                return this.Content;
            }

            set
            {
                this.Content = value;
            }
        }

        protected override void DrawBackground(
            IDrawContext drawContext,
            Rect bounds)
        {
            drawContext.Clear(Colors.Transparent);
        }
    }

    /// <summary>
    /// A button whose look comes from a template; clicking it runs a command.
    /// </summary>
    /// <remarks>
    /// GAP: No visual states (pressed, pointer over). WORKAROUND: the button
    /// dims while pressed, using the opacity of the template's root.
    /// </remarks>
    public class Button : UserControl, ITapGestureDelegate
    {
        // GAP: Control.BackgroundColor defaults to the theme's background, and the
        // Android renderer paints it behind owner-drawn controls (Win32 doesn't).
        // WORKAROUND: owner-drawn spike elements report a transparent background.
        public override Color BackgroundColor => Colors.Transparent;

        private View _templateRoot;

        public Button()
        {
            this.GestureRecognizer = new TapGestureRecognizer(this);
        }

        public object ButtonContent { get; set; }

        public ControlTemplate Template { get; set; }

        public Color Background { get; set; } = Colors.Transparent;

        public Color Foreground { get; set; } = Colors.Black;

        public ICommand Command { get; set; }

        protected override AccessibilityRole DefaultAccessibilityRole
        {
            get
            {
                return AccessibilityRole.Button;
            }
        }

        protected override string DefaultAccessibilityLabel
        {
            get
            {
                return this.ButtonContent as string;
            }
        }

        /// <summary>
        /// Builds the button's look from its template, once its properties are set.
        /// </summary>
        public void ApplyTemplate()
        {
            this._templateRoot = this.Template?.Build(this);
            this.Content = this._templateRoot;
        }

        protected override void DrawBackground(
            IDrawContext drawContext,
            Rect bounds)
        {
            drawContext.Clear(Colors.Transparent);
        }

        void ITapGestureDelegate.OnTapBegan()
        {
            if (null != this._templateRoot)
            {
                this._templateRoot.Animator.Opacity = 0.7f;
            }
        }

        void ITapGestureDelegate.OnTapEnded()
        {
            if (null != this._templateRoot)
            {
                this._templateRoot.Animator.Opacity = 1.0f;
            }
        }

        void ITapGestureDelegate.OnTapped()
        {
            if ((null != this.Command) && this.Command.CanExecute(null))
            {
                this.Command.Execute(null);
            }
        }

        void IGestureRecognizerDelegate.OnTouchBegan()
        {
        }

        void IGestureRecognizerDelegate.OnTouchEnded()
        {
        }

        void IGestureRecognizerDelegate.OnTouchCanceled()
        {
            if (null != this._templateRoot)
            {
                this._templateRoot.Animator.Opacity = 1.0f;
            }
        }
    }
}
