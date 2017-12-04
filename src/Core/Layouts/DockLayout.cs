using System;

namespace XForms.Layouts
{
    public enum DockRegion
    {
        CenterOverlay = 0,
        Left,
        Top,
        Right,
        Bottom,
        LeftOverlay,
        TopOverlay,
        RightOverlay,
        BottomOverlay,
    }

    public interface IDockLayoutRenderer : ILayoutRenderer
    {
        void ClearChildren();

        void AddChild(IElementRenderer childRenderer, DockRegion dockRegion);

        void InsertChild(int index, IElementRenderer childRenderer, DockRegion dockRegion);

        void RemoveChildAt(int index);

        void ReplaceChild(int index, IElementRenderer childRenderer, DockRegion dockRegion);
    }

    public class DockLayout : Layout
    {
        internal static readonly AttachedProperty<DockRegion> DockRegionProperty = new AttachedProperty<DockRegion>();

        public class DockLayoutViewCollection : LayoutViewCollection
        {
            public DockLayoutViewCollection(
                DockLayout layout)
                : base(layout)
            {
            }

            public void Add(
                View child,
                DockRegion position)
            {
                if (null == child)
                {
                    throw new ArgumentNullException(nameof(child));
                }

                DockRegionProperty.SetValue(child, position);

                this.Add(child);
            }

            public void Insert(
                int index,
                View child,
                DockRegion position)
            {
                if (null == child)
                {
                    throw new ArgumentNullException(nameof(child));
                }

                DockRegionProperty.SetValue(child, position);

                this.Insert(index, child);
            }

            public void Replace(
                int index,
                View child,
                DockRegion position)
            {
                if (null == child)
                {
                    throw new ArgumentNullException(nameof(child));
                }

                DockRegionProperty.SetValue(child, position);

                this.Replace(index, child);
            }
        }

        private DockLayoutViewCollection _children;

        public DockLayout()
        {
            this._children = new DockLayoutViewCollection(this);
        }

        public DockLayoutViewCollection Children
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
            var dockPosition = DockRegionProperty.GetValue(child, DockRegion.CenterOverlay);

            this.Renderer.AddChild(child.Renderer, dockPosition);
        }

        protected override void OnChildInserted(
            int index,
            View child)
        {
            var dockPosition = DockRegionProperty.GetValue(child, DockRegion.CenterOverlay);

            this.Renderer.InsertChild(index, child.Renderer, dockPosition);
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
            var dockPosition = DockRegionProperty.GetValue(child, DockRegion.CenterOverlay);

            this.Renderer.ReplaceChild(index, child.Renderer, dockPosition);
        }

        public new IDockLayoutRenderer Renderer
        {
            get
            {
                return (IDockLayoutRenderer)base.Renderer;
            }
        }

        protected override IElementRenderer CreateRenderer()
        {
            return this.Application.Platform.CreateDockLayoutRenderer(this);
        }
    }
}
