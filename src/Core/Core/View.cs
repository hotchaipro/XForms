using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace XForms
{
    public interface IViewDelegate
    {
        bool HandlePointerEvent(PointerInputEvent pointerInputEvent);
    }

    public interface IViewRenderer : IElementRenderer
    {
        bool IsVisible { get; set; }

        bool IsInputVisible { get; set; }

        Size Size { get; set; }

        Size MaximumSize { get; set; }

        Thickness Margin { get; set; }

        LayoutAlignment HorizontalAlignment { get; set; }

        LayoutAlignment VerticalAlignment { get; set; }

        float Opacity { get; set; }

        float Scale { get; set; }

        Task ScaleTo(float scale, TimeSpan duration, EasingFunction ease);

        Angle Rotation { get; set; }

        Task RotateTo(Angle angle, TimeSpan duration, EasingFunction ease);

        Point Translation { get; set; }

        Task TranslateTo(Point point, TimeSpan duration, EasingFunction ease);

        void StopAnimation();
    }

    public abstract class View : Element, IViewDelegate
    {
        protected View()
        {
        }

        public new IViewRenderer Renderer
        {
            get
            {
                return (IViewRenderer)base.Renderer;
            }
        }

        public bool IsVisible
        {
            get
            {
                return this.Renderer.IsVisible;
            }

            set
            {
                this.Renderer.IsVisible = value;
            }
        }

        public bool IsInputVisible
        {
            get
            {
                return this.Renderer.IsInputVisible;
            }

            set
            {
                this.Renderer.IsInputVisible = value;
            }
        }

        public Size Size
        {
            get
            {
                return this.Renderer.Size;
            }

            set
            {
                this.Renderer.Size = value;
            }
        }

        public Size MaximumSize
        {
            get
            {
                return this.Renderer.MaximumSize;
            }

            set
            {
                this.Renderer.MaximumSize = value;
            }
        }

        public Thickness Margin
        {
            get
            {
                return this.Renderer.Margin;
            }

            set
            {
                this.Renderer.Margin = value;
            }
        }

        public LayoutAlignment HorizontalAlignment
        {
            get
            {
                return this.Renderer.HorizontalAlignment;
            }

            set
            {
                this.Renderer.HorizontalAlignment = value;
            }
        }

        public LayoutAlignment VerticalAlignment
        {
            get
            {
                return this.Renderer.VerticalAlignment;
            }

            set
            {
                this.Renderer.VerticalAlignment = value;
            }
        }

        public float Opacity
        {
            get
            {
                return this.Renderer.Opacity;
            }

            set
            {
                this.Renderer.Opacity = value;
            }
        }

        public float Scale
        {
            get
            {
                return this.Renderer.Scale;
            }

            set
            {
                this.Renderer.Scale = value;
            }
        }

        public Task ScaleTo(
            float scale,
            TimeSpan duration,
            EasingFunction ease)
        {
            return this.Renderer.ScaleTo(scale, duration, ease);
        }

        public Angle Rotation
        {
            get
            {
                return this.Renderer.Rotation;
            }

            set
            {
                this.Renderer.Rotation = value;
            }
        }

        public Task RotateTo(
            Angle angle,
            TimeSpan duration,
            EasingFunction ease)
        {
            return this.Renderer.RotateTo(angle, duration, ease);
        }

        public Point Translation
        {
            get
            {
                return this.Renderer.Translation;
            }

            set
            {
                this.Renderer.Translation = value;
            }
        }

        public Task TranslateTo(
            Point point,
            TimeSpan duration,
            EasingFunction ease)
        {
            return this.Renderer.TranslateTo(point, duration, ease);
        }

        public void StopAnimation()
        {
            this.Renderer.StopAnimation();
        }

        public GestureRecognizer GestureRecognizer
        {
            get;
            set;
        }

        public virtual bool HandlePointerEvent(
            PointerInputEvent pointerInputEvent)
        {
            if (this.Application.HandlePreviewInput(this, pointerInputEvent))
            {
                return true;
            }

            GestureEventResult result = new GestureEventResult();
            this.GestureRecognizer?.HandlePointerInput(pointerInputEvent, ref result);
            return result.DidHandleEvent;
        }

        internal override IEnumerable<Element> GetChildElements()
        {
            yield break;
        }
    }
}
