using System;
using XForms.Primitives;

namespace XForms.Controls
{
    /// <summary>
    /// An owner-drawn control: it draws itself (in <see cref="View.Render"/>),
    /// beneath one child, its content, inside its padding.
    /// </summary>
    public abstract class UserControl : Control
    {
        private View _content;

        public UserControl()
        {
            this.DrawsContent = true;
        }

        /// <summary>
        /// Space between the control's edges and its content.
        /// </summary>
        public virtual Thickness Padding
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
