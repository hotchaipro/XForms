using System;

namespace XForms.Controls
{
    public class LightDismissControl : ContentControl
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
                    this.Application.LightDismissControl = this;
                }
                else
                {
                    this.Application.LightDismissControl = null;
                }

                this._isLightDismissEnabled = value;
            }
        }

        internal protected virtual void OnLightDismiss()
        {
        }
    }
}
