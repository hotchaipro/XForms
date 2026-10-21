using System;

namespace XForms
{
    /// <summary>
    /// Draws an owner-drawn view's content. Each platform draws with its
    /// native 2D library: Direct2D, Core Graphics or Android's Canvas.
    /// </summary>
    public abstract class DrawingContext
    {
        /// <summary>
        /// Fills the view's area with the color. Transparent clears it.
        /// </summary>
        public void Clear(
            Color color)
        {
            var bounds = this.LightweightBounds;
            if (bounds.HasValue)
            {
                // A lightweight view shares its layer's surface: fill only its
                // own area, and leave what's beneath it
                if (!color.IsTransparent)
                {
                    this.FillRectangle(bounds.Value, color);
                }

                return;
            }

            this.ClearCore(color);
        }

        protected abstract void ClearCore(
            Color color);

        /// <summary>
        /// While a lightweight view draws, its area (in its own coordinates):
        /// <see cref="Clear"/> fills only that.
        /// </summary>
        internal Rect? LightweightBounds
        {
            get;
            set;
        }

        public abstract void DrawRectangle(
            Rect rectangle,
            Color color,
            float strokeWidth = 1.0f);

        public abstract void FillRectangle(
            Rect rectangle,
            Color color);

        /// <summary>
        /// Strokes a rectangle with corners rounded to the radius.
        /// </summary>
        public abstract void DrawRoundedRectangle(
            Rect rectangle,
            float cornerRadius,
            Color color,
            float strokeWidth = 1.0f);

        /// <summary>
        /// Fills a rectangle with corners rounded to the radius.
        /// </summary>
        public abstract void FillRoundedRectangle(
            Rect rectangle,
            float cornerRadius,
            Color color);

        public abstract void DrawEllipse(
            Point point,
            float radiusX,
            float radiusY,
            Color color,
            float strokeWidth = 1.0f);

        public abstract void FillEllipse(
            Point point,
            float radiusX,
            float radiusY,
            Color color);

        public abstract void FillEllipse(
            Rect rectangle,
            Color color);

        public abstract void FillEllipseShadow(
            Rect rectangle,
            Color color,
            float blurAmount = 8.0f);

        /// <summary>
        /// Draws a bitmap, optionally tinted; a null bitmap draws nothing.
        /// </summary>
        public abstract void DrawImage(
            Bitmap bitmap,
            Rect destinationRectangle,
            Color tint);

        /// <summary>
        /// Draws a straight line with round ends.
        /// </summary>
        public abstract void DrawLine(
            Point start,
            Point end,
            Color color,
            float strokeWidth = 1.0f);

        /// <summary>
        /// Draws part of a circle with round ends, clockwise from the start
        /// angle (in degrees, with 0 at the top).
        /// </summary>
        public abstract void DrawArc(
            Point center,
            float radius,
            float startDegrees,
            float sweepDegrees,
            Color color,
            float strokeWidth = 1.0f);

        /// <summary>
        /// Draws laid-out text with its top left corner at the origin.
        /// </summary>
        public abstract void DrawText(
            TextLayout layout,
            Point origin,
            Color color);

        /// <summary>
        /// Lays out and draws text within the bounds: aligned horizontally and
        /// vertically by the format, and wrapped, trimmed and limited to lines
        /// as it says.
        /// </summary>
        public virtual void DrawText(
            string text,
            Rect bounds,
            Color color,
            TextFormat textFormat)
        {
            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            using (var layout = new TextLayout(text, textFormat, bounds.Width, bounds.Height))
            {
                float height = layout.Size.Height;
                float y = bounds.Y;
                switch (textFormat.VerticalAlignment)
                {
                    case VerticalAlignment.Center:
                    case VerticalAlignment.Stretch:
                        y += (bounds.Height - height) / 2;
                        break;

                    case VerticalAlignment.Bottom:
                        y += bounds.Height - height;
                        break;
                }

                this.DrawText(layout, new Point(bounds.X, y), color);
            }
        }

        /// <summary>
        /// Fills a rectangle with a brush (a solid color or a gradient).
        /// </summary>
        public abstract void FillRectangle(
            Rect rectangle,
            Brush brush);

        public abstract void FillRoundedRectangle(
            Rect rectangle,
            float cornerRadius,
            Brush brush);

        public abstract void FillEllipse(
            Rect rectangle,
            Brush brush);

        /// <summary>
        /// Fills the inside of a path (nonzero winding).
        /// </summary>
        public abstract void FillGeometry(
            PathGeometry geometry,
            Brush brush);

        /// <summary>
        /// Strokes a path, with round joins and caps.
        /// </summary>
        public abstract void DrawGeometry(
            PathGeometry geometry,
            Color color,
            float strokeWidth = 1.0f);

        /// <summary>
        /// Limits drawing to the rectangle until <see cref="Pop"/>.
        /// </summary>
        public DrawingContextState PushClip(
            Rect clip)
        {
            this.PushClipCore(clip);
            return new DrawingContextState(this);
        }

        /// <summary>
        /// Transforms drawing (on top of any transform already pushed) until
        /// <see cref="Pop"/>.
        /// </summary>
        public DrawingContextState PushTransform(
            System.Numerics.Matrix3x2 transform)
        {
            this.PushTransformCore(transform);
            return new DrawingContextState(this);
        }

        /// <summary>
        /// Draws everything until <see cref="Pop"/> at the opacity, as a group.
        /// </summary>
        public DrawingContextState PushOpacity(
            float opacity)
        {
            this.PushOpacityCore(Math.Max(0, Math.Min(1, opacity)));
            return new DrawingContextState(this);
        }

        /// <summary>
        /// Undoes the most recent push.
        /// </summary>
        public abstract void Pop();

        protected abstract void PushClipCore(
            Rect clip);

        protected abstract void PushTransformCore(
            System.Numerics.Matrix3x2 transform);

        protected abstract void PushOpacityCore(
            float opacity);
    }
}
