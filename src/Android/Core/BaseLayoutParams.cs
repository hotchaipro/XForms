using System;
using Android.Views;
using Android.Widget;

namespace XForms.Android
{
    internal class BaseLayoutParams : FrameLayout.LayoutParams
    {
        public static int DefaultSize = ViewGroup.LayoutParams.MatchParent; // Maps to Dimension.Auto
        public static GravityFlags DefaultLayoutGravity = GravityFlags.Fill; // Maps to LayoutAlignment.Fill

        public BaseLayoutParams()
            : this(DefaultSize, DefaultSize, DefaultLayoutGravity)
        {
        }

        private BaseLayoutParams(
            int width,
            int height)
            : this(width, height, DefaultLayoutGravity)
        {
        }

        public BaseLayoutParams(
            int width,
            int height,
            GravityFlags gravityFlags)
            : base(width, height, gravityFlags)
        {
#if DEBUG_LAYOUT
            if (this.Gravity != gravityFlags)
            {
                throw new Exception();
            }
#endif
        }

        public BaseLayoutParams(
            BaseLayoutParams source)
            : base(source)
        {
        }

        public BaseLayoutParams(
            FrameLayout.LayoutParams source)
            : base(source)
        {
        }

        public BaseLayoutParams(
            ViewGroup.MarginLayoutParams source)
            : base(source)
        {
        }

        public BaseLayoutParams(
            ViewGroup.LayoutParams source)
            : base(source)
        {
        }
    }
}
