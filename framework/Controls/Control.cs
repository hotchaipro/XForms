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

        public new IControlRenderer Renderer
        {
            get
            {
                return (IControlRenderer)base.Renderer;
            }
        }

        public virtual Color BackgroundColor
        {
            get
            {
                return this.Theme.BackgroundColor;
            }
        }

        protected override void OnApplyTheme(
            AppTheme theme)
        {
            base.OnApplyTheme(theme);

            this.Renderer.BackgroundColor = this.BackgroundColor;
        }
    }
}
