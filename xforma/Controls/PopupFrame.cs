using System;

namespace XForma.Controls
{
    /// <summary>
    /// The frame around a popup's content (a picker's list or calendar, a
    /// flyout's): its theme's template draws it, and its content stays
    /// <see cref="PopupStyle.ContentInset"/> inside its edges.
    /// </summary>
    public abstract class PopupFrame : TemplatedControl
    {
        private View _framedContent;

        protected PopupFrame()
        {
        }

        /// <summary>
        /// How far inside the frame's edges its content stays, clear of its
        /// rounded corners.
        /// </summary>
        protected float ContentInset
        {
            get
            {
                return this.ActualTheme.PopupStyle.ContentInset;
            }
        }

        /// <summary>
        /// The frame's content, inset from its edges (for a frame that doesn't
        /// lay its content out itself).
        /// </summary>
        protected View FramedContent
        {
            get
            {
                return this._framedContent;
            }

            set
            {
                this._framedContent = value;
                this.Content = value;
            }
        }

        protected override void OnThemeChanged(
            Theme theme)
        {
            base.OnThemeChanged(theme);
            if (null != this._framedContent)
            {
                this._framedContent.Margin = new Thickness(theme.PopupStyle.ContentInset);
            }
        }
    }
}
