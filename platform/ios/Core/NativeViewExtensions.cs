using System;
using UIKit;

namespace XForms.iOS
{
    internal static class NativeViewExtensions
    {
        /// <summary>
        /// Asks for the view and every ancestor to be laid out again, dropping the
        /// cached measurements of the XForms containers along the way.
        /// </summary>
        public static void RequestLayout(
            this UIView view)
        {
            for (var current = view; null != current; current = current.Superview)
            {
                if (current is NativeLayoutView layoutView)
                {
                    layoutView.InvalidateContentLayout();
                }
                else
                {
                    current.SetNeedsLayout();
                }
            }
        }

        /// <summary>
        /// Converts an XForms available size (which may be infinite) to the size
        /// UIKit expects for <see cref="UIView.SizeThatFits"/>.
        /// </summary>
        public static CoreGraphics.CGSize ToFittingSize(
            this Size availableSize)
        {
            // NOTE: UIKit treats zero as "no room", so an unbounded dimension is
            // expressed as a very large size instead
            const float Unbounded = 1000000;

            return new CoreGraphics.CGSize(
                float.IsInfinity(availableSize.Width) ? Unbounded : availableSize.Width,
                float.IsInfinity(availableSize.Height) ? Unbounded : availableSize.Height);
        }
    }
}
