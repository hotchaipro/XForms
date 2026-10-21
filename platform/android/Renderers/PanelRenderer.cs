using System;
using AndroidView = global::Android.Views.View;
using AndroidViewGroup = global::Android.Views.ViewGroup;

namespace XForms.Android.Renderers
{
    public abstract class PanelRenderer<TNativeLayout> : ViewRenderer<TNativeLayout>
        where TNativeLayout : AndroidViewGroup
    {
        protected PanelRenderer(
            global::Android.Content.Context context,
            XForms.View view)
            : base(context, view)
        {
        }

        protected override void OnNativeElementSet(
            TNativeLayout oldElement,
            TNativeLayout newElement)
        {
            base.OnNativeElementSet(oldElement, newElement);

            if (null != newElement)
            {
                // Default to no clipping
                this.IsClippedToBounds = false;

#if DEBUG_LAYOUT
                newElement.SetWillNotDraw(false); // Enable drawing for the background color
                newElement.SetBackgroundColor(new global::Android.Graphics.Color(0, 0, 0xff, 0x80));
#endif
            }
        }

        public bool IsClippedToBounds
        {
            get
            {
                return ((this.NativeElement.ClipChildren) && (this.NativeElement.ClipToPadding));
            }

            set
            {
                this.NativeElement.SetClipChildren(value);
                this.NativeElement.SetClipToPadding(value);
            }
        }

        // The children in drawing order. Lightweight children have no native
        // view, and views hosted for lightweight descendants come after these.
        private readonly System.Collections.Generic.List<IViewRenderer> _children = new System.Collections.Generic.List<IViewRenderer>();

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

        public virtual void ClearChildren()
        {
            foreach (var child in this._children)
            {
                if (child.NativeElement is AndroidView nativeChild)
                {
                    this.NativeElement.RemoveView(nativeChild);
                }
            }

            this._children.Clear();
        }

        public virtual void AddChild(
            IViewRenderer childRenderer)
        {
            this.InsertChild(this._children.Count, childRenderer);
        }

        public virtual void InsertChild(
            int index,
            IViewRenderer childRenderer)
        {
            int nativeIndex = this.NativeIndex(index);
            this._children.Insert(index, childRenderer);
            if (childRenderer.NativeElement is AndroidView nativeChild)
            {
                this.NativeElement.AddView(nativeChild, nativeIndex);
            }
            else
            {
                this.NativeElement.Invalidate();
            }
        }

        public virtual void RemoveChildAt(
            int index)
        {
            var child = this._children[index];
            this._children.RemoveAt(index);
            if (child.NativeElement is AndroidView nativeChild)
            {
                this.NativeElement.RemoveView(nativeChild);
            }
            else
            {
                this.NativeElement.Invalidate();
            }
        }

        public virtual void ReplaceChild(
            int index,
            IViewRenderer childRenderer)
        {
            this.RemoveChildAt(index);
            this.InsertChild(index, childRenderer);
        }
    }

    /// <summary>
    /// Renderer for every XForms layout: the layout itself positions its
    /// children, so the platform only needs a generic container.
    /// </summary>
    public class PanelRenderer : PanelRenderer<NativeLayoutView>, IPanelRenderer
    {
        public PanelRenderer(
            global::Android.Content.Context context,
            XForms.Panel layout)
            : base(context, layout)
        {
            this.SetNativeElement(new NativeLayoutView(context, layout));
        }
    }
}
