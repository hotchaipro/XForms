using System;
using XForms.Controls;
using AndroidProgressRing = global::Android.Widget.ProgressBar;

namespace XForms.Android.Renderers
{
    public class ProgressRingRenderer : ControlRenderer<AndroidProgressRing>, IProgressRingRenderer
    {
        private AndroidProgressRing _nativeProgressRing;

        public ProgressRingRenderer(
            global::Android.Content.Context context,
            ProgressRing progressRing)
            : base(context, progressRing)
        {
            this._nativeProgressRing = new AndroidProgressRing(context)
            {
                Indeterminate = true,
            };

            this.SetNativeElement(this._nativeProgressRing);
        }

        public bool IsActive
        {
            get
            {
                return this.IsVisible;
            }

            set
            {
                this.IsVisible = value;
            }
        }
    }
}
