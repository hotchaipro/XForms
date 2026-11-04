using System;
using CoreGraphics;
using UIKit;

namespace XForms.iOS
{
    /// <summary>
    /// A view that wraps another (an animation layer), and takes touches as
    /// far past its bounds as the view inside it does, so they reach it.
    /// </summary>
    internal sealed class OutsetHitView : UIView
    {
        public Thickness HitTestOutset
        {
            get;
            set;
        }

        public override bool PointInside(
            CGPoint point,
            UIEvent uievent)
        {
            return IsInside(this.Bounds, this.HitTestOutset, point);
        }

        /// <summary>
        /// Whether the point is in the bounds, grown by the outset.
        /// </summary>
        public static bool IsInside(
            CGRect bounds,
            Thickness outset,
            CGPoint point)
        {
            return (point.X >= bounds.X - outset.Left)
                && (point.Y >= bounds.Y - outset.Top)
                && (point.X < bounds.Right + outset.Right)
                && (point.Y < bounds.Bottom + outset.Bottom);
        }
    }
}
