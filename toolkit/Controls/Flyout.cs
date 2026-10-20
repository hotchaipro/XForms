using System;
using XForms.Controls;

namespace XForms.Toolkit
{
    /// <summary>
    /// A popup with the toolkit's look: its content on the app's background,
    /// inside a thin border. Clicking outside it, pressing Escape or going back
    /// closes it.
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
        /// Draws the popover's background and border around its content.
        /// </summary>
        private sealed class FlyoutFrame : UserControl
        {
            private const float BorderThickness = 1.0f;

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

            protected override void DrawBackground(
                IDrawContext drawContext,
                Rect bounds)
            {
                var theme = this.Theme;
                drawContext.FillRectangle(bounds, theme.BackgroundColor);
                drawContext.DrawRectangle(
                    bounds.Deflate(BorderThickness / 2, BorderThickness / 2),
                    theme.SubtleForegroundColor.MultiplyAlpha(0.4f),
                    BorderThickness);
            }
        }
    }
}
