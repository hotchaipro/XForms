using System;

namespace XForms.Controls
{
    public interface IControlRenderer : IViewRenderer
    {
        Color BackgroundColor { get; set; }
    }

    public abstract class Control : View
    {
        protected Control()
        {
        }

        internal new IControlRenderer Renderer
        {
            get
            {
                return (IControlRenderer)base.Renderer;
            }
        }

        private IControlRenderer Native
        {
            get
            {
                return (IControlRenderer)this.CreatedRenderer;
            }
        }

        /// <summary>
        /// The control's background: transparent unless the control gives it
        /// one. Owner-drawn controls paint it in their drawing.
        /// </summary>
        public virtual Color BackgroundColor
        {
            get
            {
                return Colors.Transparent;
            }
        }

        /// <summary>
        /// Creates an animator that the control keeps privately, in its own
        /// field, to animate its own content (such as a spinning ring). Call it
        /// from the control's constructor: it throws once the view has been
        /// attached. Each call creates another animator, so a subclass can add
        /// its own. The public <see cref="View.Animator"/> always belongs to the
        /// app.
        /// </summary>
        protected Animator CreateOwnAnimator()
        {
            return this.CreateOwnAnimatorCore();
        }

        internal override void OnRendererCreated()
        {
            base.OnRendererCreated();

            this.Native.BackgroundColor = this.BackgroundColor;
        }

        protected override void OnThemeChanged(
            Theme theme)
        {
            base.OnThemeChanged(theme);

            this.Native?.BackgroundColor = this.BackgroundColor;
        }
    }
}
