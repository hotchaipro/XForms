using System;

namespace XForms
{
    /// <summary>
    /// A view that closes when the user touches outside it, such as a flyout.
    /// Set it as <see cref="Application.LightDismissTarget"/> while it is open.
    /// </summary>
    public interface ILightDismissTarget
    {
        /// <summary>
        /// Whether the view is part of the target (touching it doesn't dismiss).
        /// </summary>
        bool Contains(View view);

        /// <summary>
        /// Called when a touch outside the target dismisses it.
        /// </summary>
        void OnLightDismiss();
    }
}
