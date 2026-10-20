using System;

namespace XForms.Controls
{
    public interface IScrollViewRenderer : IViewRenderer
    {
        View Content { get; set; }

        bool IsVerticalScrollingEnabled { get; set; }

        bool IsHorizontalScrollingEnabled { get; set; }

        float VerticalOffset { get; }

        void ScrollTo(float verticalOffset, bool animated);
    }

    public class ScrollView : View
    {
        public ScrollView()
        {
            this.HorizontalAlignment = LayoutAlignment.Fill;
            this.VerticalAlignment = LayoutAlignment.Fill;
        }

        public View Content
        {
            get
            {
                return this.Renderer.Content;
            }

            set
            {
                this.Renderer.Content = value;
            }
        }

        public bool IsVerticalScrollingEnabled
        {
            get
            {
                return this.Renderer.IsVerticalScrollingEnabled;
            }

            set
            {
                this.Renderer.IsVerticalScrollingEnabled = value;
            }
        }

        public bool IsHorizontalScrollingEnabled
        {
            get
            {
                return this.Renderer.IsHorizontalScrollingEnabled;
            }

            set
            {
                this.Renderer.IsHorizontalScrollingEnabled = value;
            }
        }

        /// <summary>
        /// How far the content is scrolled down.
        /// </summary>
        public float VerticalOffset
        {
            get
            {
                return this.Renderer.VerticalOffset;
            }
        }

        /// <summary>
        /// Scrolls the content so the given vertical offset is at the top
        /// (limited to the content's extent).
        /// </summary>
        public void ScrollTo(
            float verticalOffset,
            bool animated)
        {
            this.Renderer.ScrollTo(verticalOffset, animated);
        }

        public new IScrollViewRenderer Renderer
        {
            get
            {
                return (IScrollViewRenderer)base.Renderer;
            }
        }

        protected override IElementRenderer CreateRenderer()
        {
            return this.Application.Platform.CreateScrollViewRenderer(this);
        }
    }
}
