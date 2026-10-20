using System;

namespace XForms.Controls
{
    /// <summary>
    /// Draws a background and a border, optionally with rounded corners, around
    /// one child, inset by the border's thickness and the padding.
    /// </summary>
    public class Border : UserControl
    {
        public static readonly ViewProperty<Border, Color> BackgroundProperty = new(nameof(Background), v => v.Background, (v, x) => v.Background = x);
        public static readonly ViewProperty<Border, Color> BorderBrushProperty = new(nameof(BorderBrush), v => v.BorderBrush, (v, x) => v.BorderBrush = x);
        private Color _background = Colors.Transparent;
        private Color _borderBrush = Colors.Transparent;
        private Thickness _borderThickness = Thickness.Zero;
        private Thickness _padding = Thickness.Zero;
        private float _cornerRadius;

        public Border()
        {
        }

        /// <summary>
        /// The view inside the border.
        /// </summary>
        public View Child
        {
            get
            {
                return this.Content;
            }

            set
            {
                this.Content = value;

                this.OnPropertyChanged();
            }
        }

        public override Color BackgroundColor
        {
            get
            {
                return Colors.Transparent;
            }
        }

        /// <summary>
        /// The fill inside the border (transparent by default).
        /// </summary>
        public Color Background
        {
            get
            {
                return this._background;
            }

            set
            {
                this._background = value;
                this.InvalidateVisual();

                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// The color of the border.
        /// </summary>
        public Color BorderBrush
        {
            get
            {
                return this._borderBrush;
            }

            set
            {
                this._borderBrush = value;
                this.InvalidateVisual();

                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// The width of the border on each side. With rounded corners, the
        /// border is drawn with the left width on every side.
        /// </summary>
        public Thickness BorderThickness
        {
            get
            {
                return this._borderThickness;
            }

            set
            {
                this._borderThickness = value;
                this.UpdateContentPadding();
                this.InvalidateVisual();

                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// The radius of the corners (0, square, by default).
        /// </summary>
        public float CornerRadius
        {
            get
            {
                return this._cornerRadius;
            }

            set
            {
                this._cornerRadius = value;
                this.InvalidateVisual();

                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Space between the border and the child.
        /// </summary>
        public override Thickness Padding
        {
            get
            {
                return this._padding;
            }

            set
            {
                this._padding = value;
                this.UpdateContentPadding();

                this.OnPropertyChanged();
            }
        }

        private void UpdateContentPadding()
        {
            var border = this._borderThickness;
            var padding = this._padding;
            this.ContentPadding = new Thickness(
                border.Left + padding.Left,
                border.Top + padding.Top,
                border.Right + padding.Right,
                border.Bottom + padding.Bottom);
        }

        protected override void DrawBackground(
            IDrawContext drawContext,
            Rect bounds)
        {
            drawContext.Clear(Colors.Transparent);

            var border = this._borderThickness;
            float radius = this._cornerRadius;
            bool hasBorder = (border.Left > 0) || (border.Top > 0) || (border.Right > 0) || (border.Bottom > 0);
            bool isUniform = (border.Left == border.Top) && (border.Left == border.Right) && (border.Left == border.Bottom);

            if (this._background.A > 0)
            {
                if (radius > 0)
                {
                    drawContext.FillRoundedRectangle(bounds, radius, this._background);
                }
                else
                {
                    drawContext.FillRectangle(bounds, this._background);
                }
            }

            if (!hasBorder || (this._borderBrush.A == 0))
            {
                return;
            }

            if ((radius > 0) || isUniform)
            {
                // A stroke is centered on its path, so inset it by half its width
                float width = border.Left;
                var path = new Rect(
                    bounds.X + (width / 2),
                    bounds.Y + (width / 2),
                    Math.Max(0, bounds.Width - width),
                    Math.Max(0, bounds.Height - width));

                if (radius > 0)
                {
                    drawContext.DrawRoundedRectangle(path, Math.Max(0, radius - (width / 2)), this._borderBrush, width);
                }
                else
                {
                    drawContext.DrawRectangle(path, this._borderBrush, width);
                }

                return;
            }

            // Different widths per side, with square corners
            drawContext.FillRectangle(new Rect(bounds.X, bounds.Y, bounds.Width, border.Top), this._borderBrush);
            drawContext.FillRectangle(new Rect(bounds.X, bounds.Bottom - border.Bottom, bounds.Width, border.Bottom), this._borderBrush);
            drawContext.FillRectangle(new Rect(bounds.X, bounds.Y, border.Left, bounds.Height), this._borderBrush);
            drawContext.FillRectangle(new Rect(bounds.Right - border.Right, bounds.Y, border.Right, bounds.Height), this._borderBrush);
        }
    }
}
