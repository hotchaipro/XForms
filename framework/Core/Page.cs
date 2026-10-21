using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace XForms
{


    public interface IPageRenderer : IViewRenderer
    {
        void SetLayout(IPanelRenderer layoutRenderer);

        Color BackgroundColor { get; set; }
    }

    public class Page : View
    {
        private Panel _layout;
        private Color? _backgroundColor;

        public Page()
        {
        }

        protected override void OnThemeChanged(
            Theme theme)
        {
            base.OnThemeChanged(theme);

            if (!this._backgroundColor.HasValue)
            {
                this.Native?.BackgroundColor = theme.Colors.Background;
            }
        }

        internal new IPageRenderer Renderer
        {
            get
            {
                return (IPageRenderer)base.Renderer;
            }
        }

        private IPageRenderer Native
        {
            get
            {
                return (IPageRenderer)this.CreatedRenderer;
            }
        }

        /// <summary>
        /// The page's background: the theme's background color unless set.
        /// Pages are opaque, so they can overlap as they slide in and out.
        /// </summary>
        public Color BackgroundColor
        {
            get
            {
                return this._backgroundColor ?? this.ActualTheme.Colors.Background;
            }

            set
            {
                this._backgroundColor = value;
                this.Native?.BackgroundColor = value;
            }
        }

        public Panel Content
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

                var native = this.Native;
                if (null != native)
                {
                    native.SetLayout((IPanelRenderer)value.AttachTo(this));
                    value.NotifyAttached();
                }

                this.InvalidateMeasure();
            }
        }

        // Frames slide pages in and out
        internal override bool HasFrameworkAnimator
        {
            get
            {
                return true;
            }
        }

        internal override void OnRendererCreated()
        {
            base.OnRendererCreated();

            var native = this.Native;
            native.BackgroundColor = this.BackgroundColor;

            var layout = this._layout;
            if (null != layout)
            {
                native.SetLayout((IPanelRenderer)layout.AttachTo(this));
                layout.NotifyAttached();
            }
        }

        protected override IElementRenderer CreateRenderer()
        {
            return this.Application.Platform.CreatePageRenderer(this);
        }

        /// <summary>
        /// Closes the page, when it's shown with
        /// <see cref="Application.ShowModalAsync(Page)"/>.
        /// </summary>
        public Task CloseModalAsync()
        {
            return this.Application.CloseModalAsync(this, null);
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

        // A press on the page itself (not on any of its views) light-dismisses
        // and does nothing else
        internal override bool DispatchPointerEvent(
            PointerEventArgs e)
        {
            e.OriginalSource ??= this;
            return this.Application.FilterPointerInput(this, e) || base.DispatchPointerEvent(e);
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
            this._layout?.Arrange(new Rect(0, 0, finalSize.Width, finalSize.Height));
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
