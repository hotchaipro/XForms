using System;

namespace XForms.Layouts
{
    public interface IDistributedStackLayoutRenderer : ILayoutRenderer
    {
        void ClearChildren();

        void AddChild(IElementRenderer childRenderer);

        void InsertChild(int index, IElementRenderer childRenderer);

        void RemoveChildAt(int index);

        void ReplaceChild(int index, IElementRenderer childRenderer);
    }

    public class DistributedStackLayout : Layout
    {
        public class DistributedStackLayoutViewCollection : LayoutViewCollection
        {
            public DistributedStackLayoutViewCollection(
                DistributedStackLayout layout)
                : base(layout)
            {
            }
        }

        private DistributedStackLayoutViewCollection _children;

        public DistributedStackLayout()
        {
            this.HorizontalAlignment = LayoutAlignment.Start;
            this.VerticalAlignment = LayoutAlignment.Start;

            this._children = new DistributedStackLayoutViewCollection(this);
        }

        public DistributedStackLayoutViewCollection Children
        {
            get
            {
                return this._children;
            }
        }

        protected override void OnChildrenCleared()
        {
            this.Renderer.ClearChildren();
        }

        protected override void OnChildAdded(
            View child)
        {
            this.Renderer.AddChild(child.Renderer);
        }

        protected override void OnChildInserted(
            int index,
            View child)
        {
            this.Renderer.InsertChild(index, child.Renderer);
        }

        protected override void OnChildRemoved(
            int index)
        {
            this.Renderer.RemoveChildAt(index);
        }

        protected override void OnChildReplaced(
            int index,
            View child)
        {
            this.Renderer.ReplaceChild(index, child.Renderer);
        }

        public new IDistributedStackLayoutRenderer Renderer
        {
            get
            {
                return (IDistributedStackLayoutRenderer)base.Renderer;
            }
        }

        protected override IElementRenderer CreateRenderer()
        {
            return this.Application.Platform.CreateDistributedStackLayoutRenderer(this);
        }
    }
}
