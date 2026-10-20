using System;
using System.Collections.Generic;

namespace XForms
{
    public interface IPageDelegate
    {
        bool HandlePointerEvent(PointerInputEvent pointerInputEvent);
    }

    public interface IPageRenderer : IViewRenderer
    {
        void SetLayout(ILayoutRenderer layoutRenderer);

        Color BackgroundColor { get; set; }
    }

    public class Page : View, IPageDelegate
    {
        private Layout _layout;

        public Page()
        {
        }

        protected override void OnApplyTheme(
            AppTheme theme)
        {
#if !DEBUG_LAYOUT
            // Need an opaque background to support overlapping page animations
            this.Renderer.BackgroundColor = theme.BackgroundColor;
#endif
        }

        public new IPageRenderer Renderer
        {
            get
            {
                return (IPageRenderer)base.Renderer;
            }
        }

        public Color BackgroundColor
        {
            get
            {
                return this.Renderer.BackgroundColor;
            }

            set
            {
                this.Renderer.BackgroundColor = value;
            }
        }

        public Layout Layout
        {
            get
            {
                return this._layout;
            }

            set
            {
                if (null != this._layout)
                {
                    throw new NotSupportedException();
                }

                this._layout = value;

                this.Renderer.SetLayout(value.Renderer);
                this.InvalidateLayout();
            }
        }

        protected override IElementRenderer CreateRenderer()
        {
            return this.Application.Platform.CreatePageRenderer(this);
        }

        internal protected virtual bool OnNavigatingFrom(
            NavigationEventArgs navigationEventArgs)
        {
            return true;
        }

        internal protected virtual void OnNavigatedTo(
            NavigationEventArgs navigationEventArgs)
        {
        }

        internal protected virtual void OnNavigatedFrom(
            NavigationEventArgs navigationEventArgs)
        {
        }

        public override bool HandlePointerEvent(
            PointerInputEvent pointerInputEvent)
        {
            return this.Application.HandlePreviewInput(this, pointerInputEvent);
        }

        protected override Size MeasureOverride(
            Size availableSize)
        {
            var content = this._layout;
            if (null == content)
            {
                return Size.Empty;
            }

            content.Measure(availableSize);
            return content.DesiredSize;
        }

        protected override void ArrangeOverride(
            Size finalSize)
        {
            this._layout?.Arrange(new Rectangle(0, 0, finalSize.Width, finalSize.Height));
        }

        internal override IEnumerable<Element> GetChildElements()
        {
            var layout = this._layout;
            if (null != layout)
            {
                yield return layout;
            }
        }
    }
}
