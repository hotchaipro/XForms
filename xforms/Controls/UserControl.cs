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

        private bool _isBuilt;

        /// <summary>
        /// Builds the control's views, and returns its content. XForms calls
        /// it once, as the control is first attached (before its native object
        /// exists), and again after an edit while the app runs with hot reload.
        /// A control whose content is given to it instead (as a border's
        /// child) sets <see cref="Content"/>.
        /// </summary>
        /// <remarks>
        /// The control's views are built here, not in its constructor, which
        /// sets up only its state. A field that refers to a view is assigned
        /// here, so after a rebuild it refers to the new view.
        /// </remarks>
        protected virtual View Build()
        {
            return null;
        }

        /// <summary>
        /// Whether <see cref="Build"/> has run: the control's views exist.
        /// </summary>
        protected bool IsBuilt
        {
            get
            {
                return this._isBuilt;
            }
        }

        protected override void OnAttaching(
            Theme theme)
        {
            base.OnAttaching(theme);
            if (!this._isBuilt)
            {
                this._isBuilt = true;
                var content = this.Build();
                if (null != content)
                {
                    this.Content = content;
                }
            }
        }

        // After a hot reload edit: the control builds again
        protected override void OnHotReload()
        {
            base.OnHotReload();
            if (this._isBuilt)
            {
                var content = this.Build();
                if (null != content)
                {
                    this.Content = content;
                }
            }
        }

        /// <summary>
        /// The control's content: what <see cref="Build"/> returns, or what
        /// the control is given.
        /// </summary>
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
