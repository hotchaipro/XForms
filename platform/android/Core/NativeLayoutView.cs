using System;
using Android.Views;
using XForms.Platform;

namespace XForms.Android
{
    /// <summary>
    /// Native container for any XForms view that lays out children of its own.
    /// Android's layout pass drives it (including its measure cache and dirty
    /// tracking), and it hands the measuring and positioning to the shared layout
    /// engine in Core.
    /// </summary>
    internal class NativeLayoutView : ViewGroup
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

        internal XForms.View Owner
        {
            get
            {
                return this._owner;
            }
        }

        private LayerNodeProvider _nodeProvider;

        /// <summary>
        /// The virtual accessibility nodes for the lightweight views inside, or
        /// null until an accessibility service asks for them.
        /// </summary>
        internal LayerNodeProvider NodeProvider
        {
            get
            {
                return this._nodeProvider;
            }
        }

        internal LayerNodeProvider GetOrCreateNodeProvider()
        {
            return this._nodeProvider ??= new LayerNodeProvider(this);
        }

        protected override bool DispatchHoverEvent(
            global::Android.Views.MotionEvent e)
        {
            if ((null != this._nodeProvider) && this._nodeProvider.DispatchHover(e))
            {
                return true;
            }

            return base.DispatchHoverEvent(e);
        }

        /// <summary>
        /// Whether keyboard focus last arrived moving backward (Shift+Tab), so
        /// the last lightweight view inside takes it.
        /// </summary>
        internal bool IsFocusBackward
        {
            get;
            private set;
        }

        protected override void OnFocusChanged(
            bool gainFocus,
            [global::Android.Runtime.GeneratedEnum] FocusSearchDirection direction,
            global::Android.Graphics.Rect previouslyFocusedRect)
        {
            this.IsFocusBackward = gainFocus && (direction == FocusSearchDirection.Backward);

            // A layer that has focus for its lightweight views draws their
            // ring instead of Android's highlight over all of it
            if (global::Android.OS.Build.VERSION.SdkInt >= global::Android.OS.BuildVersionCodes.O)
            {
                this.DefaultFocusHighlightEnabled = this._owner.Focusable;
            }

            base.OnFocusChanged(gainFocus, direction, previouslyFocusedRect);
        }

        public override void AddFocusables(
            System.Collections.Generic.IList<global::Android.Views.View> views,
            [global::Android.Runtime.GeneratedEnum] FocusSearchDirection direction,
            [global::Android.Runtime.GeneratedEnum] FocusablesFlags focusableMode)
        {
            // Keyboard focus can move into a layer with focusable lightweight
            // views inside it
            if (!this.Focusable && LayerHost.For(this._owner).HasFocusableLightweight)
            {
                this.Focusable = true;
            }

            base.AddFocusables(views, direction, focusableMode);
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

            this._nodeProvider?.NotifyLayoutChanged();
        }

        protected override void DispatchDraw(
            global::Android.Graphics.Canvas canvas)
        {
            // Lightweight descendants draw into this view, beneath its native
            // children
            if (LayerHost.For(this._owner).HasLightweightChildren)
            {
                var drawContext = new DrawContext(this.Context, canvas);
                LayerHost.For(this._owner).PaintLightweightViews(drawContext);
            }

            base.DispatchDraw(canvas);

            if (null != LayerHost.For(this._owner).FocusedLightweight)
            {
                LayerHost.For(this._owner).PaintFocusRing(new DrawContext(this.Context, canvas));
            }
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
