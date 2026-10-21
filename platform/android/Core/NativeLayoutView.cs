using System;
using Android.Views;

namespace XForms.Android
{
    /// <summary>
    /// Native container for any XForms view that lays out children of its own.
    /// Android's layout pass drives it (including its measure cache and dirty
    /// tracking), and it hands the measuring and positioning to the shared layout
    /// engine in Core.
    /// </summary>
    public class NativeLayoutView : ViewGroup
    {
        private readonly XForms.View _owner;

        public NativeLayoutView(
            global::Android.Content.Context context,
            XForms.View owner)
            : base(context)
        {
            if (null == owner)
            {
                throw new ArgumentNullException(nameof(owner));
            }

            this._owner = owner;

            // NOTE: XForms views don't clip their children unless ClipToBounds is
            // set (which clips to the outline), as on the other platforms. By
            // default Android clips each child to the child's bounds, which cuts
            // off anything a descendant moves or scales outside them.
            this.SetClipChildren(false);
            this.SetClipToPadding(false);
        }

        protected override void OnMeasure(
            int widthMeasureSpec,
            int heightMeasureSpec)
        {
            var availableSize = new Size(
                this.MeasureSpecToDevicePixels(widthMeasureSpec),
                this.MeasureSpecToDevicePixels(heightMeasureSpec));

            Size contentSize = this._owner.MeasureContent(availableSize);

            this.SetMeasuredDimension(
                this.ResolveMeasuredSize(contentSize.Width, widthMeasureSpec),
                this.ResolveMeasuredSize(contentSize.Height, heightMeasureSpec));
        }

        protected override void OnLayout(
            bool changed,
            int left,
            int top,
            int right,
            int bottom)
        {
            this._owner.LayoutContent(new Size(
                NativeConversions.PixelsToDevicePixels(this.Context, right - left),
                NativeConversions.PixelsToDevicePixels(this.Context, bottom - top)));
        }

        public override bool ShouldDelayChildPressedState()
        {
            return false;
        }

        private float MeasureSpecToDevicePixels(
            int measureSpec)
        {
            if (MeasureSpec.GetMode(measureSpec) == MeasureSpecMode.Unspecified)
            {
                return float.PositiveInfinity;
            }

            return NativeConversions.PixelsToDevicePixels(this.Context, MeasureSpec.GetSize(measureSpec));
        }

        private int ResolveMeasuredSize(
            float devicePixels,
            int measureSpec)
        {
            int pixels = (int)Math.Ceiling(NativeConversions.DevicePixelsToPixels(this.Context, devicePixels));
            int specSize = MeasureSpec.GetSize(measureSpec);

            switch (MeasureSpec.GetMode(measureSpec))
            {
                case MeasureSpecMode.Exactly:
                    return specSize;

                case MeasureSpecMode.AtMost:
                    return Math.Min(pixels, specSize);

                default:
                    return pixels;
            }
        }
    }
}
