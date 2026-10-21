using System;

namespace XForms.Controls
{
    /// <summary>
    /// Base for controls: a view with a background.
    /// </summary>
    public abstract class Control : View
    {
        protected Control()
        {
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

        protected override Color GetBackgroundColor()
        {
            return this.BackgroundColor;
        }
    }
}
