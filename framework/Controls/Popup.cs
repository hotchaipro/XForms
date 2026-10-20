using System;
using System.Collections.Generic;

namespace XForms.Controls
{
    public interface IPopupDelegate
    {
        /// <summary>
        /// Called by the platform when the popup closes, including when the user
        /// dismisses it (by clicking outside it or pressing Escape).
        /// </summary>
        void NotifyClosed();
    }

    public interface IPopupRenderer : IElementRenderer
    {
        View Content { get; set; }

        View PlacementTarget { get; set; }

        void Show();

        void Close();
    }

    /// <summary>
    /// Shows a view above the page until it is closed: centered in the window,
    /// or below (or, without room, above) its <see cref="PlacementTarget"/>.
    /// The popup draws nothing of its own, so control libraries give it a look
    /// (a background, border or shadow) through its content. Clicking outside
    /// the popup, pressing Escape or going back closes it.
    /// </summary>
    public class Popup : Element, IPopupDelegate
    {
        private bool _isOpen;
        private View _content;
        private View _placementTarget;

        public Popup()
        {
        }

        /// <summary>
        /// Raised when the popup closes, however it was closed.
        /// </summary>
        public event EventHandler Closed;

        public View Content
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

                this._content = value;

                var native = this.Native;
                if (null != native)
                {
                    value?.AttachTo(this);
                    native.Content = value;
                    oldContent?.NotifyDetached();
                    value?.NotifyAttached();
                }
            }
        }

        /// <summary>
        /// The view the popup opens next to, such as the field of a picker. The
        /// popup's content is measured with the space available there.
        /// </summary>
        public View PlacementTarget
        {
            get
            {
                return this._placementTarget;
            }

            set
            {
                this._placementTarget = value;
                this.Native?.PlacementTarget = value;
            }
        }

        public bool IsOpen
        {
            get
            {
                return this._isOpen;
            }
        }

        public void Show()
        {
            if (this._isOpen)
            {
                return;
            }

            this._isOpen = true;
            this.Renderer.Show();
            this.Application.OnPopupShown(this);
        }

        public void Close()
        {
            if (!this._isOpen)
            {
                return;
            }

            // NOTE: The platform reports the close through NotifyClosed
            this.Renderer.Close();
        }

        protected virtual void OnClosed()
        {
        }

        void IPopupDelegate.NotifyClosed()
        {
            if (!this._isOpen)
            {
                return;
            }

            this._isOpen = false;
            this.Application.OnPopupClosed(this);
            this.OnClosed();
            this.Closed?.Invoke(this, EventArgs.Empty);
        }

        internal new IPopupRenderer Renderer
        {
            get
            {
                return (IPopupRenderer)base.Renderer;
            }
        }

        private IPopupRenderer Native
        {
            get
            {
                return (IPopupRenderer)this.CreatedRenderer;
            }
        }

        internal override void OnRendererCreated()
        {
            base.OnRendererCreated();

            var native = this.Native;
            native.PlacementTarget = this._placementTarget;

            var content = this._content;
            if (null != content)
            {
                content.AttachTo(this);
                native.Content = content;
                content.NotifyAttached();
            }
        }

        protected override IElementRenderer CreateRenderer()
        {
            return this.Application.Platform.CreatePopupRenderer(this);
        }

        internal override IEnumerable<Element> GetChildElements()
        {
            var content = this.Content;
            if (null != content)
            {
                yield return content;
            }
        }
    }
}
