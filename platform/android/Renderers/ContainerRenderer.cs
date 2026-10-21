using System;
using XForms.Platform;

namespace XForms.Android.Renderers
{
    /// <summary>
    /// The renderer of every view that holds children (panels, content and
    /// owner-drawn controls, pages): the view positions its children, so the
    /// platform only keeps the native child list, and draws a view that draws
    /// itself beneath it.
    /// </summary>
    public class ContainerRenderer : PanelRenderer<NativeLayoutView>, IContainerRenderer
    {
        private readonly bool _drawsContent;

        public ContainerRenderer(
            global::Android.Content.Context context,
            View view)
            : base(context, view)
        {
            this._drawsContent = LayerHost.For(view).DrawsContent;
            this.SetNativeElement(this._drawsContent ? new OwnerDrawControl(context, view) : new NativeLayoutView(context, view));
        }

        public override Color BackgroundColor
        {
            get
            {
                return base.BackgroundColor;
            }

            set
            {
                // NOTE: A view that draws itself paints its own background, so
                // the native view stays transparent (as on Win32 and iOS)
                if (!this._drawsContent)
                {
                    base.BackgroundColor = value;
                }
            }
        }

        public void Invalidate()
        {
            this.NativeElement.Invalidate();
        }
    }
}
