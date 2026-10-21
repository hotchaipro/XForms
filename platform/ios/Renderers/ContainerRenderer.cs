using System;
using UIKit;
using XForms.Platform;

namespace XForms.iOS.Renderers
{
    /// <summary>
    /// The renderer of every view that holds children (panels, content and
    /// owner-drawn controls, pages): the view positions its children, so the
    /// platform only keeps the native child list, and draws a view that draws
    /// itself beneath it.
    /// </summary>
    internal class ContainerRenderer : ViewRenderer<NativeLayoutView>, IContainerRenderer
    {
        private readonly bool _drawsContent;

        // The children in drawing order. Lightweight children have no native
        // view, and views hosted for lightweight descendants come after these.
        private readonly System.Collections.Generic.List<IViewRenderer> _children = new System.Collections.Generic.List<IViewRenderer>();

        public ContainerRenderer(
            View view)
            : base(view)
        {
            this._drawsContent = LayerHost.For(view).DrawsContent;
            var native = this._drawsContent ? new OwnerDrawControl(view) : new NativeLayoutView(view);

            // A page's background fills the screen; its content stays clear of
            // the status bar, notch and home indicator
            native.RespectsSafeArea = view is Page;
            this.SetNativeElement(native);
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
                // the native view stays transparent
                if (!this._drawsContent)
                {
                    base.BackgroundColor = value;
                }
            }
        }

        public void Invalidate()
        {
            this.NativeElement.SetNeedsDisplay();
        }

        // Where a child at the index goes among the native views
        private int NativeIndex(
            int index)
        {
            int nativeIndex = 0;
            for (int i = 0; i < index; i += 1)
            {
                if (null != this._children[i].NativeElement)
                {
                    nativeIndex += 1;
                }
            }

            return nativeIndex;
        }

        public void ClearChildren()
        {
            foreach (var child in this._children)
            {
                (child.NativeElement as UIView)?.RemoveFromSuperview();
            }

            this._children.Clear();
            this.NativeElement.SetNeedsDisplay();
        }

        public void InsertChild(
            int index,
            IViewRenderer childRenderer)
        {
            int nativeIndex = this.NativeIndex(index);
            this._children.Insert(index, childRenderer);
            if (childRenderer.NativeElement is UIView nativeChild)
            {
                this.NativeElement.InsertSubview(nativeChild, nativeIndex);
            }
            else
            {
                this.NativeElement.SetNeedsDisplay();
            }
        }

        public void RemoveChildAt(
            int index)
        {
            var child = this._children[index];
            this._children.RemoveAt(index);
            if (child.NativeElement is UIView nativeChild)
            {
                nativeChild.RemoveFromSuperview();
            }
            else
            {
                this.NativeElement.SetNeedsDisplay();
            }
        }
    }
}
