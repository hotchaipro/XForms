using System;
using XForms.Controls;
using XForms.Primitives;

namespace XForms.Controls
{
    /// <summary>
    /// A control that closes (through <see cref="OnLightDismiss"/>) when the user
    /// touches anywhere outside it, while light dismiss is enabled.
    /// </summary>
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
                if (value == this._isLightDismissEnabled)
                {
                    return;
                }

                this._isLightDismissEnabled = value;
                if (value)
                {
                    this.Application.PreviewPointerInput += this.Application_PreviewPointerInput;
                }
                else
                {
                    this.Application.PreviewPointerInput -= this.Application_PreviewPointerInput;
                }
            }
        }

        protected virtual void OnLightDismiss()
        {
        }

        // A press outside the control (or on the page itself) dismisses it,
        // and does nothing else
        private void Application_PreviewPointerInput(
            object sender,
            PointerEventArgs e)
        {
            if (e.State != PointerInputState.Began)
            {
                return;
            }

            var view = e.OriginalSource;
            if ((view is Page) || !this.HasDescendant(view))
            {
                e.Handled = true;
                this.IsLightDismissEnabled = false;
                this.OnLightDismiss();
            }
        }
    }
}
