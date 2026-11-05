using System;
using System.Runtime.CompilerServices;
using AndroidView = Android.Views.View;

namespace XForma.Android
{
    /// <summary>
    /// How far past their bounds native views take touches (see
    /// <see cref="View.HitTestOutset"/>), in pixels; their parent layer routes
    /// touches in the outset to them.
    /// </summary>
    internal static class HitTestOutsets
    {
        private sealed class Outset
        {
            public float Left, Top, Right, Bottom;
        }

        private static readonly ConditionalWeakTable<AndroidView, Outset> Outsets = new ConditionalWeakTable<AndroidView, Outset>();

        public static void Set(
            AndroidView view,
            Thickness outset,
            float density)
        {
            if (Thickness.Zero == outset)
            {
                Outsets.Remove(view);
                return;
            }

            Outsets.AddOrUpdate(view, new Outset()
            {
                Left = outset.Left * density,
                Top = outset.Top * density,
                Right = outset.Right * density,
                Bottom = outset.Bottom * density,
            });
        }

        /// <summary>
        /// Whether the point (in the parent's pixels) is in the child's outset.
        /// </summary>
        public static bool IsInOutset(
            AndroidView child,
            float x,
            float y)
        {
            if (!Outsets.TryGetValue(child, out var outset))
            {
                return false;
            }

            float left = child.GetX(), top = child.GetY();
            return (x >= left - outset.Left) && (y >= top - outset.Top)
                && (x < left + child.Width + outset.Right) && (y < top + child.Height + outset.Bottom);
        }
    }
}
