using System;
using XForms.Controls;
using XForms.Primitives;

namespace XForms.Controls
{
    /// <summary>
    /// An indeterminate progress indicator, shown while it's active. Its
    /// theme's template draws it (Simple's spins an arc on the platform's
    /// compositor or animation thread, so it keeps turning while the UI
    /// thread is busy).
    /// </summary>
    public class ProgressRing : TemplatedControl
    {
        private bool _isActive;

        public ProgressRing()
        {
            this.IsActive = true;
        }

        /// <summary>
        /// Whether the ring shows, and moves.
        /// </summary>
        public bool IsActive
        {
            get
            {
                return this._isActive;
            }

            set
            {
                this._isActive = value;
                this.IsVisible = value;
                this.OnStateChanged();
            }
        }
    }
}
