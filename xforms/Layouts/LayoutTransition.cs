using System;

namespace XForms.Controls
{
    /// <summary>
    /// How a panel animates its children when its layout changes: children
    /// that move slide from where they were, and children that are added fade
    /// in. Set it on <see cref="Panel.ChildTransitions"/> before the children
    /// are attached, so they get the animation layers it needs.
    /// </summary>
    /// <remarks>
    /// The same idea as WinUI's <c>ChildrenTransitions</c> and Android's
    /// <c>LayoutTransition</c>. Layout still happens at once; only the children's
    /// render translation and opacity animate, on the compositor.
    /// </remarks>
    public sealed class LayoutTransition
    {
        /// <summary>
        /// How long children take, or null (the default) for the theme's
        /// medium duration.
        /// </summary>
        public TimeSpan? Duration { get; init; }

        /// <summary>
        /// How children arrive, or null (the default) for the theme's
        /// decelerating curve.
        /// </summary>
        public EasingFunction Easing { get; init; }

        /// <summary>
        /// Whether children that move slide from their old positions.
        /// </summary>
        public bool AnimatesMoves { get; init; } = true;

        /// <summary>
        /// Whether children added after the first layout fade in.
        /// </summary>
        public bool AnimatesEntrances { get; init; } = true;
    }
}
