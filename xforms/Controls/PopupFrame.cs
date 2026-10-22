using System;

namespace XForms.Controls
{
    /// <summary>
    /// The frame around a picker's popup (its list or calendar): its theme's
    /// template draws it, with a border <see cref="BorderThickness"/> wide.
    /// </summary>
    public abstract class PopupFrame : TemplatedControl
    {
        /// <summary>
        /// The width of the frame's border, which its content stays inside.
        /// </summary>
        public const float BorderThickness = 1.0f;

        protected PopupFrame()
        {
        }
    }
}
