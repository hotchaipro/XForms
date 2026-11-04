using System;

namespace XForms.Controls
{
    /// <summary>
    /// The frame around a popup's content (a picker's list or calendar, a
    /// flyout's): its theme's template draws it, and its content stays
    /// <see cref="ContentInset"/> inside its edges.
    /// </summary>
    public abstract class PopupFrame : TemplatedControl
    {
        /// <summary>
        /// How far inside the frame's edges its content stays, clear of its
        /// rounded corners.
        /// </summary>
        public const float ContentInset = 6.0f;

        protected PopupFrame()
        {
        }
    }
}
