using System;
using XForms.Controls;
using XForms.Primitives;

namespace XForms.Controls
{
    /// <summary>
    /// A popup with its content in a frame (a <see cref="PopupFrame"/>, which
    /// its theme's template draws). Clicking outside it, pressing Escape or
    /// going back closes it.
    /// </summary>
    public class Flyout : Popup
    {
        private readonly FlyoutFrame _frame = new FlyoutFrame();

        public Flyout()
        {
            base.Content = this._frame;
        }

        /// <summary>
        /// The view shown inside the popover's frame.
        /// </summary>
        public new View Content
        {
            get
            {
                return this._frame.Body;
            }

            set
            {
                this._frame.Body = value;
            }
        }

        /// <summary>
        /// Shows the popover. (The popover stays open until it is closed.)
        /// </summary>
        public void ShowAsync()
        {
            this.Show();
        }

        /// <summary>
        /// The frame around the popover's content.
        /// </summary>
        private sealed class FlyoutFrame : PopupFrame
        {
            public View Body
            {
                get
                {
                    return this.Content;
                }

                set
                {
                    if (null != value)
                    {
                        value.Margin = new Thickness(BorderThickness);
                    }

                    this.Content = value;
                }
            }
        }
    }
}
