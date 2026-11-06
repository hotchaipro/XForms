using System;
using System.Collections.Generic;
using XForma.Platform;

namespace XForma.Primitives
{
    public class ScrollViewer : View
    {
        internal override bool ScrollsContent
        {
            get
            {
                return true;
            }
        }

        // Its content is a layer of its own
        internal override bool RequiresNativeChildren
        {
            get
            {
                return true;
            }
        }

        public ScrollViewer()
        {
            this.HorizontalAlignment = HorizontalAlignment.Stretch;
            this.VerticalAlignment = VerticalAlignment.Stretch;
        }

        private View? _content;
        private bool? _isVerticalScrollingEnabled;
        private bool? _isHorizontalScrollingEnabled;
        private KeyboardDismissMode _keyboardDismissMode;
        private float? _pendingVerticalOffset;

        public View? Content
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

        /// <summary>
        /// Whether scrolling hides the soft keyboard: none (the default), on a
        /// drag, or interactively (the keyboard follows the drag, on iOS).
        /// </summary>
        internal override void OnSafeAreaExtensionChanged()
        {
            this.Native?.ContentInset = this.SafeAreaExtension;
        }

        public KeyboardDismissMode KeyboardDismissMode
        {
            get
            {
                return this._keyboardDismissMode;
            }

            set
            {
                this._keyboardDismissMode = value;
                this.Native?.KeyboardDismissMode = value;
                this.OnPropertyChanged();
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

        [PlatformContract]
        internal new IScrollViewerRenderer? Renderer
        {
            get
            {
                return (IScrollViewerRenderer)base.Renderer;
            }
        }

        private IScrollViewerRenderer? Native
        {
            get
            {
                return (IScrollViewerRenderer)this.CreatedRenderer;
            }
        }

        // The platforms render it natively
        // The platforms place the content (filling the viewport's width)
        internal override bool MirrorsChildren
        {
            get
            {
                return false;
            }
        }

        internal override bool IsLightweightType
        {
            get
            {
                return false;
            }
        }

        protected override object? GetUserState()
        {
            float offset = this.VerticalOffset;
            return (offset > 0) ? offset : null;
        }

        protected override void SetUserState(
            object state)
        {
            if (state is float offset)
            {
                this.Application.RequestAnimationFrame((time) => this.ScrollToVerticalOffset(offset, false));
            }
        }

        internal override void OnRendererCreated()
        {
            base.OnRendererCreated();

            var native = this.Native;
            if (this._isVerticalScrollingEnabled.HasValue) native.IsVerticalScrollingEnabled = this._isVerticalScrollingEnabled.Value;
            if (this._isHorizontalScrollingEnabled.HasValue) native.IsHorizontalScrollingEnabled = this._isHorizontalScrollingEnabled.Value;
            native.KeyboardDismissMode = this._keyboardDismissMode;
            if (this.SafeAreaExtension != Thickness.Zero) native.ContentInset = this.SafeAreaExtension;

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

        private protected override IElementRenderer CreateRenderer()
        {
            return this.Application.Platform.CreateScrollViewerRenderer(this);
        }

        internal override IEnumerable<Element> GetChildElements()
        {
            var content = this._content;
            if (null != content)
            {
                yield return content;
            }
        }
    }
}
