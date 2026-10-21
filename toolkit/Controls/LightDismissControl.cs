using System;
using XForms.Controls;

namespace XForms.Toolkit
{
    /// <summary>
    /// A control that closes (through <see cref="OnLightDismiss"/>) when the user
    /// touches anywhere outside it, while light dismiss is enabled.
    /// </summary>
    public class LightDismissControl : ContentControl, ILightDismissTarget
    {
        private bool _isLightDismissEnabled;

        public LightDismissControl()
        {
        }

        public bool IsLightDismissEnabled
        {
            get
            {
                return this._isLightDismissEnabled;
            }

            set
            {
                if (value)
                {
                    this.Application.LightDismissTarget = this;
                }
                else if (this.Application.LightDismissTarget == this)
                {
                    this.Application.LightDismissTarget = null;
                }

                this._isLightDismissEnabled = value;
            }
        }

        protected virtual void OnLightDismiss()
        {
        }

        bool ILightDismissTarget.Contains(
            View view)
        {
            return this.HasDescendant(view);
        }

        void ILightDismissTarget.OnLightDismiss()
        {
            this._isLightDismissEnabled = false;
            this.OnLightDismiss();
        }
    }
}
