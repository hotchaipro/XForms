using System;

namespace XForms
{
    /// <summary>
    /// The shape that casts a <see cref="Shadow"/>: the view's bounds, or the
    /// ellipse (a circle, for a square view) inside them.
    /// </summary>
    public enum ShadowShape
    {
        Rectangle,
        Ellipse,
    }

    /// <summary>
    /// A drop shadow beneath a view, outside its bounds, as CSS's
    /// <c>box-shadow</c>: its color, offset and blur radius. Set one with
    /// <see cref="View.Shadow"/>; the platform draws it natively.
    /// </summary>
    /// <remarks>
    /// <c>box-shadow: 0 3px 8px rgba(0, 0, 0, 0.3)</c> is
    /// <c>new Shadow { Color = Color.FromArgb(0x4d, 0, 0, 0), Offset = new Point(0, 3), Blur = 8 }</c>.
    /// </remarks>
    public sealed record Shadow
    {
        /// <summary>
        /// The shadow's color, with its opacity: black at 30% by default.
        /// </summary>
        public Color Color { get; init; } = Color.FromArgb(0x4d, 0, 0, 0);

        /// <summary>
        /// How far the shadow is from the shape.
        /// </summary>
        public Point Offset { get; init; }

        /// <summary>
        /// How far the shadow's edge blurs, as CSS's blur radius (twice the
        /// blur's standard deviation).
        /// </summary>
        public float Blur { get; init; }

        /// <summary>
        /// The shape that casts it.
        /// </summary>
        public ShadowShape Shape { get; init; }

        /// <summary>
        /// The rectangle's corner radius.
        /// </summary>
        public float CornerRadius { get; init; }
    }
}
