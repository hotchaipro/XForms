using System;

namespace XForms.Controls
{
    public interface IScrollViewerRenderer : IViewRenderer
    {
        View Content { get; set; }

        bool IsVerticalScrollingEnabled { get; set; }

        bool IsHorizontalScrollingEnabled { get; set; }

        float VerticalOffset { get; }

        void ScrollTo(float verticalOffset, bool animated);
    }

    public class ScrollViewer : View
    {
        public ScrollViewer()
        {
            this.HorizontalAlignment = HorizontalAlignment.Stretch;
            this.VerticalAlignment = VerticalAlignment.Stretch;
        }

        private View _content;
        private bool? _isVerticalScrollingEnabled;
        private bool? _isHorizontalScrollingEnabled;
        private float? _pendingVerticalOffset;

        public View Content
        {
            get
            {
                return this._content;
            }

            set
            {
                var oldContent = this._content;
                if (value == oldContent)
                {
                    return;
                }

                this._content = value;

                var native = this.Native;
                if (null != native)
                {
                    value?.AttachTo(this);
                    native.Content = value;
                    oldContent?.NotifyDetached();
                    value?.NotifyAttached();
                }

                this.InvalidateMeasure();
            }
        }

        public bool IsVerticalScrollingEnabled
        {
            get
            {
                return this._isVerticalScrollingEnabled ?? this.Native?.IsVerticalScrollingEnabled ?? true;
            }

            set
            {
                this._isVerticalScrollingEnabled = value;
                this.Native?.IsVerticalScrollingEnabled = value;
            }
        }

        public bool IsHorizontalScrollingEnabled
        {
            get
            {
                return this._isHorizontalScrollingEnabled ?? this.Native?.IsHorizontalScrollingEnabled ?? false;
            }

            set
            {
                this._isHorizontalScrollingEnabled = value;
                this.Native?.IsHorizontalScrollingEnabled = value;
            }
        }

        /// <summary>
        /// How far the content is scrolled down.
        /// </summary>
        public float VerticalOffset
        {
            get
            {
                return this.Native?.VerticalOffset ?? this._pendingVerticalOffset ?? 0;
            }
        }

        /// <summary>
        /// Scrolls the content so the given vertical offset is at the top
        /// (limited to the content's extent).
        /// </summary>
        public void ScrollToVerticalOffset(
            float verticalOffset,
            bool animated)
        {
            var native = this.Native;
            if (null != native)
            {
                native.ScrollTo(verticalOffset, animated);
            }
            else
            {
                this._pendingVerticalOffset = verticalOffset;
            }
        }

        internal new IScrollViewerRenderer Renderer
        {
            get
            {
                return (IScrollViewerRenderer)base.Renderer;
            }
        }

        private IScrollViewerRenderer Native
        {
            get
            {
                return (IScrollViewerRenderer)this.CreatedRenderer;
            }
        }

        internal override void OnRendererCreated()
        {
            base.OnRendererCreated();

            var native = this.Native;
            if (this._isVerticalScrollingEnabled.HasValue) native.IsVerticalScrollingEnabled = this._isVerticalScrollingEnabled.Value;
            if (this._isHorizontalScrollingEnabled.HasValue) native.IsHorizontalScrollingEnabled = this._isHorizontalScrollingEnabled.Value;

            var content = this._content;
            if (null != content)
            {
                content.AttachTo(this);
                native.Content = content;
                content.NotifyAttached();
            }

            if (this._pendingVerticalOffset.HasValue)
            {
                native.ScrollTo(this._pendingVerticalOffset.Value, false);
                this._pendingVerticalOffset = null;
            }
        }

        protected override IElementRenderer CreateRenderer()
        {
            return this.Application.Platform.CreateScrollViewerRenderer(this);
        }
    }
}
