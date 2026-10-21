using System;
using Android.Content;

namespace XForms.Android
{
    /// <summary>
    /// A vertical ScrollView whose content is laid out by the shared layout engine
    /// (as on the other platforms), so the content's measure and frame are known
    /// to XForms: measured at the viewport's width with unlimited height, and
    /// arranged at least as tall as the viewport.
    /// </summary>
    internal sealed class NativeScrollView : global::Android.Widget.ScrollView
    {
        public NativeScrollView(
            Context context)
            : base(context)
        {
        }

        public View Content
        {
            get;
            set;
        }

        protected override void OnMeasure(
            int widthMeasureSpec,
            int heightMeasureSpec)
        {
            base.OnMeasure(widthMeasureSpec, heightMeasureSpec);

            var content = this.Content;
            if (null != content)
            {
                float width = NativeConversions.PixelsToDevicePixels(this.Context, this.MeasuredWidth - this.PaddingLeft - this.PaddingRight);
                content.Measure(new Size(Math.Max(0, width), float.PositiveInfinity));
            }
        }

        protected override void OnLayout(
            bool changed,
            int left,
            int top,
            int right,
            int bottom)
        {
            var content = this.Content;
            if (null != content)
            {
                float width = NativeConversions.PixelsToDevicePixels(this.Context, right - left - this.PaddingLeft - this.PaddingRight);
                float height = NativeConversions.PixelsToDevicePixels(this.Context, bottom - top - this.PaddingTop - this.PaddingBottom);

                // Content smaller than the viewport still fills it
                content.Arrange(new Rect(0, 0, Math.Max(0, width), Math.Max(content.DesiredSize.Height, height)));
            }

            // The ScrollView keeps its scroll position within the content's new extent
            base.OnLayout(changed, left, top, right, bottom);
        }
    }
}
