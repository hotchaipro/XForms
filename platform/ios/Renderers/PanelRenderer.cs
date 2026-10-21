using System;
using UIKit;

namespace XForms.iOS.Renderers
{
    /// <summary>
    /// Renderer for every XForms layout: the layout itself positions its
    /// children, so the platform only keeps the native child list.
    /// </summary>
    internal class PanelRenderer : ViewRenderer<NativeLayoutView>, IPanelRenderer
    {
        public PanelRenderer(
            Panel layout)
            : base(layout)
        {
            this.SetNativeElement(new NativeLayoutView(layout));
        }

        // The children in drawing order. Lightweight children have no native
        // view, and views hosted for lightweight descendants come after these.
        private readonly System.Collections.Generic.List<IViewRenderer> _children = new System.Collections.Generic.List<IViewRenderer>();

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

        public void AddChild(
            IViewRenderer childRenderer)
        {
            this.InsertChild(this._children.Count, childRenderer);
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

        public void ReplaceChild(
            int index,
            IViewRenderer childRenderer)
        {
            this.RemoveChildAt(index);
            this.InsertChild(index, childRenderer);
        }
    }
}
