using System;

namespace XForms.Controls
{
    /// <summary>
    /// A control with one child, its content, inside its padding.
    /// </summary>
    public abstract class ContentControl : Control
    {
        private View _content;

        public ContentControl()
        {
        }

        /// <summary>
        /// Space between the control's edges and its content.
        /// </summary>
        public Thickness Padding
        {
            get
            {
                return this.ContentPadding;
            }

            set
            {
                this.ContentPadding = value;
            }
        }

        protected View Content
        {
            get
            {
                return this._content;
            }

            set
            {
                var oldContent = this._content;
                if (value == oldContent)
                {
                    return;
                }

                if (null != oldContent)
                {
                    this.RemoveVisualChild(oldContent);
                }

                this._content = value;
                if (null != value)
                {
                    this.AddVisualChild(value);
                }
            }
        }
    }
}
